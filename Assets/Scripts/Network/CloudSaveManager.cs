using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 클라우드 세이브 매니저 - 서버 저장/로드 관리
    /// </summary>
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        #region Events
        public event Action OnSaveStarted;
        public event Action OnSaveCompleted;
        public event Action<string> OnSaveFailed;
        public event Action OnLoadStarted;
        public event Action<CloudSaveData> OnLoadCompleted;
        public event Action<string> OnLoadFailed;
        public event Action<SaveConflictData> OnSaveConflict;
        #endregion

        #region Settings
        [Header("=== Save Settings ===")]
        [SerializeField] private float autoSaveInterval = 300f;     // 자동 저장 간격 (초)
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private int maxBackupCount = 3;

        [Header("=== Sync Settings ===")]
        [SerializeField] private bool syncOnLogin = true;
        [SerializeField] private bool syncOnResume = true;
        [SerializeField] private ConflictResolution defaultConflictResolution = ConflictResolution.UseServer;
        #endregion

        #region State
        private CloudSaveData lastSavedData;
        private CloudSaveData serverData;
        private float lastSaveTime;
        private bool isSaving = false;
        private bool isLoading = false;
        private int saveVersion = 0;
        #endregion

        #region Properties
        public bool IsSaving => isSaving;
        public bool IsLoading => isLoading;
        public DateTime? LastSaveTime => lastSavedData?.save_time != null ?
            DateTime.Parse(lastSavedData.save_time) : (DateTime?)null;
        public int SaveVersion => saveVersion;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // AuthManager 이벤트 연결
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess += HandleLoginSuccess;
                AuthManager.Instance.OnLogoutCompleted += HandleLogout;
            }
        }

        private void Update()
        {
            // 자동 저장
            if (enableAutoSave && AuthManager.Instance?.IsLoggedIn == true)
            {
                if (Time.time - lastSaveTime >= autoSaveInterval)
                {
                    SaveToCloud();
                    lastSaveTime = Time.time;
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && AuthManager.Instance?.IsLoggedIn == true)
            {
                SaveToCloud();
            }
            else if (!pause && syncOnResume)
            {
                SyncWithServer();
            }
        }

        private void OnDestroy()
        {
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
                AuthManager.Instance.OnLogoutCompleted -= HandleLogout;
            }
        }
        #endregion

        #region Event Handlers
        private void HandleLoginSuccess(UserInfo user)
        {
            if (syncOnLogin)
            {
                LoadFromCloud();
            }
        }

        private void HandleLogout()
        {
            lastSavedData = null;
            serverData = null;
            saveVersion = 0;
        }
        #endregion

        #region Save to Cloud
        /// <summary>
        /// 클라우드에 저장
        /// </summary>
        public void SaveToCloud(Action<bool> callback = null)
        {
            if (!AuthManager.Instance?.IsLoggedIn == true)
            {
                Debug.LogWarning("[CloudSaveManager] Cannot save - not logged in");
                callback?.Invoke(false);
                return;
            }

            if (isSaving)
            {
                Debug.LogWarning("[CloudSaveManager] Save already in progress");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(SaveToCloudCoroutine(callback));
        }

        /// <summary>
        /// 클라우드 저장 코루틴
        /// </summary>
        private IEnumerator SaveToCloudCoroutine(Action<bool> callback)
        {
            isSaving = true;
            OnSaveStarted?.Invoke();

            // 현재 게임 데이터 수집
            CloudSaveData saveData = CollectGameData();
            saveData.version = ++saveVersion;
            saveData.save_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData.checksum = CalculateChecksum(saveData);

            // 서버로 전송
            bool completed = false;
            bool success = false;
            string errorMessage = null;

            NetworkManager.Instance?.Post("/save/upload", saveData, (response) =>
            {
                if (response.success)
                {
                    lastSavedData = saveData;
                    success = true;
                    Debug.Log("[CloudSaveManager] Save completed successfully");
                }
                else
                {
                    // 충돌 체크
                    if (response.statusCode == 409)
                    {
                        var conflictData = response.GetData<SaveConflictResponse>();
                        HandleSaveConflict(conflictData);
                    }
                    else
                    {
                        errorMessage = response.errorMessage;
                        OnSaveFailed?.Invoke(errorMessage);
                    }
                }
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            isSaving = false;

            if (success)
            {
                OnSaveCompleted?.Invoke();
            }

            callback?.Invoke(success);
        }

        /// <summary>
        /// 게임 데이터 수집
        /// </summary>
        private CloudSaveData CollectGameData()
        {
            var gameManager = Managers.GameManager.Instance;

            CloudSaveData data = new CloudSaveData
            {
                user_id = AuthManager.Instance?.CurrentUser?.user_id ?? "",
                device_id = SystemInfo.deviceUniqueIdentifier,
                platform = Application.platform.ToString()
            };

            // PlayerStats 데이터
            if (gameManager?.PlayerStats != null)
            {
                data.player_data = new CloudPlayerData
                {
                    level = gameManager.PlayerStats.Level,
                    current_exp = gameManager.PlayerStats.CurrentExp.Serialize(),
                    gold = gameManager.PlayerStats.Gold.Serialize(),
                    current_hp = gameManager.PlayerStats.CurrentHP.Serialize(),
                    max_hp = gameManager.PlayerStats.MaxHP.Serialize(),
                    attack = gameManager.PlayerStats.Attack.Serialize(),
                    stat_points = gameManager.PlayerStats.StatPoints,
                    skill_points = gameManager.PlayerStats.SkillPoints
                };
            }

            // 스테이지 데이터
            if (gameManager?.Battle != null)
            {
                data.stage_data = new CloudStageData
                {
                    current_stage = gameManager.Battle.CurrentStage,
                    highest_stage = gameManager.Battle.CurrentStage,
                    total_monsters_killed = 0 // TODO: 추적 필요
                };
            }

            // 재화 데이터
            if (Managers.CurrencyManager.Instance != null)
            {
                var currencies = Managers.CurrencyManager.Instance.GetAllCurrencies();
                data.currency_data = new CloudCurrencyData[currencies.Count];

                int i = 0;
                foreach (var kvp in currencies)
                {
                    data.currency_data[i] = new CloudCurrencyData
                    {
                        currency_type = kvp.Key.ToString(),
                        amount = kvp.Value.Serialize()
                    };
                    i++;
                }
            }

            // 플레이 시간
            data.play_time = (int)Time.realtimeSinceStartup;

            return data;
        }
        #endregion

        #region Load from Cloud
        /// <summary>
        /// 클라우드에서 로드
        /// </summary>
        public void LoadFromCloud(Action<bool, CloudSaveData> callback = null)
        {
            if (!AuthManager.Instance?.IsLoggedIn == true)
            {
                Debug.LogWarning("[CloudSaveManager] Cannot load - not logged in");
                callback?.Invoke(false, null);
                return;
            }

            if (isLoading)
            {
                Debug.LogWarning("[CloudSaveManager] Load already in progress");
                callback?.Invoke(false, null);
                return;
            }

            StartCoroutine(LoadFromCloudCoroutine(callback));
        }

        /// <summary>
        /// 클라우드 로드 코루틴
        /// </summary>
        private IEnumerator LoadFromCloudCoroutine(Action<bool, CloudSaveData> callback)
        {
            isLoading = true;
            OnLoadStarted?.Invoke();

            bool completed = false;
            bool success = false;
            CloudSaveData loadedData = null;

            NetworkManager.Instance?.Get("/save/download", (response) =>
            {
                if (response.success)
                {
                    loadedData = response.GetData<CloudSaveData>();

                    if (loadedData != null)
                    {
                        // 체크섬 검증
                        string calculatedChecksum = CalculateChecksum(loadedData);
                        if (calculatedChecksum == loadedData.checksum)
                        {
                            serverData = loadedData;
                            saveVersion = loadedData.version;
                            success = true;
                            Debug.Log($"[CloudSaveManager] Load completed. Version: {loadedData.version}");
                        }
                        else
                        {
                            Debug.LogError("[CloudSaveManager] Checksum mismatch - data may be corrupted");
                            OnLoadFailed?.Invoke("데이터 검증 실패");
                        }
                    }
                }
                else
                {
                    if (response.statusCode == 404)
                    {
                        // 저장 데이터 없음 - 새 유저
                        Debug.Log("[CloudSaveManager] No save data found - new user");
                        success = true;
                    }
                    else
                    {
                        OnLoadFailed?.Invoke(response.errorMessage);
                    }
                }
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            isLoading = false;

            if (success && loadedData != null)
            {
                OnLoadCompleted?.Invoke(loadedData);
            }

            callback?.Invoke(success, loadedData);
        }

        /// <summary>
        /// 로드된 데이터를 게임에 적용
        /// </summary>
        public void ApplyCloudData(CloudSaveData data)
        {
            if (data == null) return;

            var gameManager = Managers.GameManager.Instance;
            if (gameManager == null) return;

            // PlayerStats 적용
            if (data.player_data != null && gameManager.PlayerStats != null)
            {
                var playerData = data.player_data;
                gameManager.PlayerStats.SetLevel(playerData.level);
                // BigNumber 데이터 적용은 PlayerStats의 LoadFromSaveData 메서드 사용
            }

            // 스테이지 적용
            if (data.stage_data != null && gameManager.Battle != null)
            {
                gameManager.Battle.GoToStage(data.stage_data.current_stage);
            }

            Debug.Log("[CloudSaveManager] Cloud data applied to game");
        }
        #endregion

        #region Sync
        /// <summary>
        /// 서버와 동기화
        /// </summary>
        public void SyncWithServer(Action<SyncResult> callback = null)
        {
            LoadFromCloud((success, cloudData) =>
            {
                if (!success)
                {
                    callback?.Invoke(SyncResult.Failed);
                    return;
                }

                if (cloudData == null)
                {
                    // 클라우드에 데이터 없음 - 로컬 데이터 업로드
                    SaveToCloud((saveSuccess) =>
                    {
                        callback?.Invoke(saveSuccess ? SyncResult.Uploaded : SyncResult.Failed);
                    });
                    return;
                }

                // 버전 비교
                if (cloudData.version > saveVersion)
                {
                    // 서버 데이터가 최신
                    if (lastSavedData == null)
                    {
                        ApplyCloudData(cloudData);
                        callback?.Invoke(SyncResult.Downloaded);
                    }
                    else
                    {
                        // 충돌 발생
                        var conflictData = new SaveConflictData
                        {
                            localData = lastSavedData,
                            serverData = cloudData
                        };
                        OnSaveConflict?.Invoke(conflictData);
                        callback?.Invoke(SyncResult.Conflict);
                    }
                }
                else
                {
                    // 로컬이 최신 또는 동일
                    if (saveVersion > cloudData.version)
                    {
                        SaveToCloud((saveSuccess) =>
                        {
                            callback?.Invoke(saveSuccess ? SyncResult.Uploaded : SyncResult.Failed);
                        });
                    }
                    else
                    {
                        callback?.Invoke(SyncResult.InSync);
                    }
                }
            });
        }
        #endregion

        #region Conflict Resolution
        /// <summary>
        /// 저장 충돌 처리
        /// </summary>
        private void HandleSaveConflict(SaveConflictResponse conflictResponse)
        {
            var conflictData = new SaveConflictData
            {
                localData = lastSavedData,
                serverData = conflictResponse.server_data
            };

            if (defaultConflictResolution == ConflictResolution.AskUser)
            {
                OnSaveConflict?.Invoke(conflictData);
            }
            else
            {
                ResolveConflict(conflictData, defaultConflictResolution);
            }
        }

        /// <summary>
        /// 충돌 해결
        /// </summary>
        public void ResolveConflict(SaveConflictData conflict, ConflictResolution resolution)
        {
            switch (resolution)
            {
                case ConflictResolution.UseLocal:
                    ForceSaveToCloud();
                    break;

                case ConflictResolution.UseServer:
                    ApplyCloudData(conflict.serverData);
                    saveVersion = conflict.serverData.version;
                    break;

                case ConflictResolution.Merge:
                    MergeData(conflict.localData, conflict.serverData);
                    break;
            }
        }

        /// <summary>
        /// 데이터 병합 (최고값 사용)
        /// </summary>
        private void MergeData(CloudSaveData local, CloudSaveData server)
        {
            // 더 높은 레벨 사용
            if (local.player_data != null && server.player_data != null)
            {
                if (local.player_data.level > server.player_data.level)
                {
                    ApplyCloudData(local);
                }
                else
                {
                    ApplyCloudData(server);
                }
            }

            // 스테이지는 더 높은 값 사용
            if (local.stage_data != null && server.stage_data != null)
            {
                int maxStage = Mathf.Max(local.stage_data.highest_stage, server.stage_data.highest_stage);
                // 적용 로직
            }

            ForceSaveToCloud();
        }

        /// <summary>
        /// 강제 저장 (버전 무시)
        /// </summary>
        private void ForceSaveToCloud()
        {
            CloudSaveData saveData = CollectGameData();
            saveData.version = saveVersion + 1;
            saveData.save_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData.checksum = CalculateChecksum(saveData);
            saveData.force_override = true;

            NetworkManager.Instance?.Post("/save/upload", saveData, (response) =>
            {
                if (response.success)
                {
                    lastSavedData = saveData;
                    saveVersion = saveData.version;
                    OnSaveCompleted?.Invoke();
                }
                else
                {
                    OnSaveFailed?.Invoke(response.errorMessage);
                }
            });
        }
        #endregion

        #region Backup
        /// <summary>
        /// 백업 목록 가져오기
        /// </summary>
        public void GetBackupList(Action<bool, CloudBackupInfo[]> callback)
        {
            NetworkManager.Instance?.Get("/save/backups", (response) =>
            {
                if (response.success)
                {
                    var backups = response.GetData<CloudBackupListResponse>();
                    callback?.Invoke(true, backups?.backups);
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            });
        }

        /// <summary>
        /// 백업에서 복원
        /// </summary>
        public void RestoreFromBackup(string backupId, Action<bool> callback)
        {
            NetworkManager.Instance?.Post($"/save/restore/{backupId}", null, (response) =>
            {
                if (response.success)
                {
                    LoadFromCloud((success, data) =>
                    {
                        if (success && data != null)
                        {
                            ApplyCloudData(data);
                        }
                        callback?.Invoke(success);
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }
        #endregion

        #region Checksum
        /// <summary>
        /// 체크섬 계산
        /// </summary>
        private string CalculateChecksum(CloudSaveData data)
        {
            // 핵심 데이터로 체크섬 생성
            string rawData = $"{data.user_id}_{data.version}_{data.player_data?.level ?? 0}_{data.stage_data?.current_stage ?? 0}";

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion

        #region Delete
        /// <summary>
        /// 클라우드 데이터 삭제
        /// </summary>
        public void DeleteCloudData(Action<bool> callback)
        {
            NetworkManager.Instance?.Delete("/save/delete", (response) =>
            {
                if (response.success)
                {
                    lastSavedData = null;
                    serverData = null;
                    saveVersion = 0;
                }
                callback?.Invoke(response.success);
            });
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// 동기화 결과
    /// </summary>
    public enum SyncResult
    {
        InSync,
        Uploaded,
        Downloaded,
        Conflict,
        Failed
    }

    /// <summary>
    /// 충돌 해결 방식
    /// </summary>
    public enum ConflictResolution
    {
        UseLocal,
        UseServer,
        Merge,
        AskUser
    }
    #endregion

    #region Data Classes
    [Serializable]
    public class CloudSaveData
    {
        public string user_id;
        public string device_id;
        public string platform;
        public int version;
        public string save_time;
        public string checksum;
        public bool force_override;

        public CloudPlayerData player_data;
        public CloudStageData stage_data;
        public CloudCurrencyData[] currency_data;
        public int play_time;
    }

    [Serializable]
    public class CloudPlayerData
    {
        public int level;
        public string current_exp;
        public string gold;
        public string current_hp;
        public string max_hp;
        public string attack;
        public int stat_points;
        public int skill_points;
    }

    [Serializable]
    public class CloudStageData
    {
        public int current_stage;
        public int highest_stage;
        public int total_monsters_killed;
    }

    [Serializable]
    public class CloudCurrencyData
    {
        public string currency_type;
        public string amount;
    }

    [Serializable]
    public class SaveConflictData
    {
        public CloudSaveData localData;
        public CloudSaveData serverData;
    }

    [Serializable]
    public class SaveConflictResponse
    {
        public string message;
        public CloudSaveData server_data;
    }

    [Serializable]
    public class CloudBackupInfo
    {
        public string backup_id;
        public string save_time;
        public int version;
        public int level;
        public int stage;
    }

    [Serializable]
    public class CloudBackupListResponse
    {
        public CloudBackupInfo[] backups;
    }
    #endregion
}
