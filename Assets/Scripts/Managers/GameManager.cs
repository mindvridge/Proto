using System;
using System.IO;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 게임 매니저 - 모든 시스템 초기화 및 연결, 저장/로드 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        #region Events
        public event Action OnGameInitialized;
        public event Action OnGameSaved;
        public event Action OnGameLoaded;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        #endregion

        #region References
        [Header("=== Manager References ===")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private ClickerManager clickerManager;
        [SerializeField] private LevelUpSystem levelUpSystem;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private DialogueEffectManager dialogueEffectManager;
        #endregion

        #region Settings
        [Header("=== Save Settings ===")]
        [SerializeField] private string saveFileName = "GameSave";
        [SerializeField] private float autoSaveInterval = 60f;  // 자동 저장 간격 (초)
        [SerializeField] private bool enableAutoSave = true;
        #endregion

        #region State
        private PlayerStats playerStats;
        private bool isInitialized = false;
        private bool isPaused = false;
        private float lastSaveTime;
        #endregion

        #region Properties
        public PlayerStats PlayerStats => playerStats;
        public bool IsInitialized => isInitialized;
        public bool IsPaused => isPaused;
        public BattleManager Battle => battleManager;
        public ClickerManager Clicker => clickerManager;
        public LevelUpSystem LevelUp => levelUpSystem;
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
            Initialize();
        }

        private void Update()
        {
            // 자동 저장 체크
            if (enableAutoSave && isInitialized)
            {
                if (Time.time - lastSaveTime >= autoSaveInterval)
                {
                    SaveGame();
                    lastSaveTime = Time.time;
                }
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && isInitialized)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            if (isInitialized)
            {
                SaveGame();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 게임 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            Debug.Log("[GameManager] Initializing game...");

            // PlayerStats 생성
            playerStats = new PlayerStats();

            // 저장 데이터 로드 시도
            LoadGame();

            // 매니저 참조 자동 찾기
            FindManagers();

            // 매니저들에 PlayerStats 연결
            ConnectManagers();

            isInitialized = true;
            lastSaveTime = Time.time;

            OnGameInitialized?.Invoke();
            Debug.Log("[GameManager] Game initialized successfully");

            // 전투 시작
            if (battleManager != null)
            {
                battleManager.StartBattle();
            }
        }

        /// <summary>
        /// 매니저 참조 자동 찾기
        /// </summary>
        private void FindManagers()
        {
            if (battleManager == null)
                battleManager = FindFirstObjectByType<BattleManager>();

            if (clickerManager == null)
                clickerManager = FindFirstObjectByType<ClickerManager>();

            if (levelUpSystem == null)
                levelUpSystem = FindFirstObjectByType<LevelUpSystem>();

            if (dialogueManager == null)
                dialogueManager = FindFirstObjectByType<DialogueManager>();

            if (dialogueEffectManager == null)
                dialogueEffectManager = FindFirstObjectByType<DialogueEffectManager>();
        }

        /// <summary>
        /// 매니저들 연결
        /// </summary>
        private void ConnectManagers()
        {
            if (battleManager != null)
            {
                battleManager.SetPlayerStats(playerStats);

                // 전투 이벤트 연결
                battleManager.OnMonsterDefeated += HandleMonsterDefeated;
                battleManager.OnStageChanged += HandleStageChanged;
            }

            if (clickerManager != null)
            {
                clickerManager.SetPlayerStats(playerStats);
            }

            if (levelUpSystem != null)
            {
                levelUpSystem.SetPlayerStats(playerStats);

                // 레벨업 이벤트 연결
                levelUpSystem.OnContentUnlocked += HandleContentUnlocked;
                levelUpSystem.OnMilestoneReached += HandleMilestoneReached;
            }

            // PlayerStats 이벤트 연결
            playerStats.OnLevelUp += HandleLevelUp;
            playerStats.OnGoldChanged += HandleGoldChanged;
            playerStats.OnHealthChanged += HandleHealthChanged;
        }
        #endregion

        #region Event Handlers
        private void HandleMonsterDefeated(MonsterData monster, BattleRewards rewards)
        {
            Debug.Log($"[GameManager] Monster defeated: {monster.monsterName}, Gold: {rewards.gold}, Exp: {rewards.exp}");
        }

        private void HandleStageChanged(int newStage)
        {
            Debug.Log($"[GameManager] Stage changed to: {newStage}");
            // 자동 저장
            SaveGame();
        }

        private void HandleLevelUp(int oldLevel, int newLevel)
        {
            Debug.Log($"[GameManager] Level up: {oldLevel} -> {newLevel}");
            // 자동 저장
            SaveGame();
        }

        private void HandleGoldChanged(long newGold)
        {
            // UI 업데이트는 별도 UI 매니저에서 처리
        }

        private void HandleHealthChanged(long currentHP, long maxHP)
        {
            // 플레이어 사망 체크
            if (currentHP <= 0)
            {
                HandlePlayerDeath();
            }
        }

        private void HandleContentUnlocked(string contentName)
        {
            Debug.Log($"[GameManager] Content unlocked: {contentName}");

            // 대화 또는 알림 표시
            // dialogueManager?.StartDialogue($"unlock_{contentName.ToLower()}");
        }

        private void HandleMilestoneReached(int level)
        {
            Debug.Log($"[GameManager] Milestone reached: Level {level}");
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("[GameManager] Player died!");

            // HP 회복 후 재시작 (또는 부활 UI 표시)
            playerStats.FullHeal();

            // 스테이지 감소 (선택적)
            // battleManager?.GoToStage(Mathf.Max(1, battleManager.CurrentStage - 1));
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame()
        {
            try
            {
                GameSaveData saveData = new GameSaveData
                {
                    playerData = playerStats.ToSaveData(),
                    currentStage = battleManager?.CurrentStage ?? 1,
                    levelUpData = levelUpSystem?.ToSaveData(),
                    saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                string json = JsonUtility.ToJson(saveData, true);
                string path = GetSavePath();

                File.WriteAllText(path, json);

                OnGameSaved?.Invoke();
                Debug.Log($"[GameManager] Game saved to: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// 게임 로드
        /// </summary>
        public void LoadGame()
        {
            try
            {
                string path = GetSavePath();

                if (!File.Exists(path))
                {
                    Debug.Log("[GameManager] No save file found, starting new game");
                    return;
                }

                string json = File.ReadAllText(path);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

                if (saveData != null)
                {
                    // PlayerStats 로드
                    if (saveData.playerData != null)
                    {
                        playerStats.LoadFromSaveData(saveData.playerData);
                    }

                    // 스테이지 로드는 Initialize 후에 처리
                    // battleManager?.GoToStage(saveData.currentStage);

                    // LevelUpSystem 로드
                    if (levelUpSystem != null && saveData.levelUpData != null)
                    {
                        levelUpSystem.LoadFromSaveData(saveData.levelUpData);
                    }

                    OnGameLoaded?.Invoke();
                    Debug.Log($"[GameManager] Game loaded. Level: {playerStats.Level}, Stage: {saveData.currentStage}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameManager] Load failed: {e.Message}");
            }
        }

        /// <summary>
        /// 저장 파일 경로
        /// </summary>
        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, $"{saveFileName}.json");
        }

        /// <summary>
        /// 저장 파일 삭제 (새 게임 시작용)
        /// </summary>
        public void DeleteSave()
        {
            string path = GetSavePath();
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[GameManager] Save file deleted");
            }
        }

        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            DeleteSave();
            playerStats = new PlayerStats();
            playerStats.Initialize();

            if (battleManager != null)
            {
                battleManager.GoToStage(1);
            }

            SaveGame();
            Debug.Log("[GameManager] New game started");
        }
        #endregion

        #region Game Control
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0;

            OnGamePaused?.Invoke();
            Debug.Log("[GameManager] Game paused");
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1;

            OnGameResumed?.Invoke();
            Debug.Log("[GameManager] Game resumed");
        }

        /// <summary>
        /// 일시정지 토글
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
        #endregion

        #region Utility
        /// <summary>
        /// 플레이어 스탯 요약 가져오기
        /// </summary>
        public string GetPlayerStatsSummary()
        {
            if (playerStats == null) return "No data";

            return $"Level: {playerStats.Level}\n" +
                   $"HP: {playerStats.CurrentHP}/{playerStats.MaxHP}\n" +
                   $"Attack: {playerStats.Attack}\n" +
                   $"Gold: {playerStats.Gold}\n" +
                   $"Exp: {playerStats.CurrentExp}/{playerStats.RequiredExp}";
        }

        /// <summary>
        /// 현재 진행 상황 요약
        /// </summary>
        public string GetProgressSummary()
        {
            string stage = battleManager != null ? $"Stage {battleManager.CurrentStage}" : "N/A";
            string monster = battleManager?.CurrentMonster?.monsterName ?? "None";

            return $"{stage} - Fighting: {monster}";
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Debug: Print Stats")]
        private void DebugPrintStats()
        {
            Debug.Log(GetPlayerStatsSummary());
        }

        [ContextMenu("Debug: Add 1000 Gold")]
        private void DebugAddGold()
        {
            playerStats?.AddGold(1000);
        }

        [ContextMenu("Debug: Add 1000 Exp")]
        private void DebugAddExp()
        {
            playerStats?.AddExp(1000);
        }

        [ContextMenu("Debug: Level Up")]
        private void DebugLevelUp()
        {
            playerStats?.SetLevel(playerStats.Level + 1);
        }

        [ContextMenu("Debug: Full Heal")]
        private void DebugFullHeal()
        {
            playerStats?.FullHeal();
        }

        [ContextMenu("Debug: Delete Save")]
        private void DebugDeleteSave()
        {
            DeleteSave();
        }
#endif
        #endregion
    }

    /// <summary>
    /// 게임 저장 데이터
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public PlayerSaveData playerData;
        public int currentStage;
        public LevelUpSaveData levelUpData;
        public string saveTime;
    }
}
