using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 보안 매니저 - 해킹 방지 및 데이터 무결성 검증
    /// </summary>
    public class SecurityManager : MonoBehaviour
    {
        public static SecurityManager Instance { get; private set; }

        #region Events
        public event Action<SecurityViolation> OnSecurityViolationDetected;
        public event Action OnIntegrityCheckPassed;
        public event Action<string> OnIntegrityCheckFailed;
        #endregion

        #region Settings
        [Header("=== Security Settings ===")]
        [SerializeField] private bool enableMemoryProtection = true;
        [SerializeField] private bool enableSpeedHackDetection = true;
        [SerializeField] private bool enableDataIntegrityCheck = true;
        [SerializeField] private bool enableDeviceBindCheck = true;

        [Header("=== Check Intervals ===")]
        [SerializeField] private float integrityCheckInterval = 30f;
        [SerializeField] private float speedCheckInterval = 1f;

        [Header("=== Thresholds ===")]
        [SerializeField] private float speedHackThreshold = 1.5f;      // Time.timeScale 허용 범위
        [SerializeField] private int maxViolationsBeforeBan = 3;
        #endregion

        #region State
        private Dictionary<string, SecureValue> secureValues = new Dictionary<string, SecureValue>();
        private int violationCount = 0;
        private float lastRealTime;
        private float lastGameTime;
        private bool isCheckingIntegrity = false;
        private string deviceFingerprint;
        private string sessionToken;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (enableSpeedHackDetection)
            {
                StartCoroutine(SpeedHackDetectionCoroutine());
            }

            if (enableDataIntegrityCheck)
            {
                StartCoroutine(IntegrityCheckCoroutine());
            }
        }

        private void Update()
        {
            // 메모리 보호 값 검증
            if (enableMemoryProtection)
            {
                ValidateSecureValues();
            }
        }
        #endregion

        #region Initialization
        private void Initialize()
        {
            // 디바이스 핑거프린트 생성
            deviceFingerprint = GenerateDeviceFingerprint();

            // 세션 토큰 생성
            sessionToken = GenerateSessionToken();

            // 타임 초기화
            lastRealTime = Time.realtimeSinceStartup;
            lastGameTime = Time.time;

            Debug.Log("[SecurityManager] Initialized");
        }

        /// <summary>
        /// 디바이스 핑거프린트 생성
        /// </summary>
        private string GenerateDeviceFingerprint()
        {
            string rawData = $"{SystemInfo.deviceUniqueIdentifier}_{SystemInfo.deviceModel}_{SystemInfo.operatingSystem}";
            return ComputeHash(rawData);
        }

        /// <summary>
        /// 세션 토큰 생성
        /// </summary>
        private string GenerateSessionToken()
        {
            string rawData = $"{deviceFingerprint}_{DateTime.Now.Ticks}_{UnityEngine.Random.Range(0, int.MaxValue)}";
            return ComputeHash(rawData);
        }
        #endregion

        #region Secure Value Protection
        /// <summary>
        /// 보안 값 등록 (메모리 해킹 방지)
        /// </summary>
        public void RegisterSecureValue(string key, long value)
        {
            if (secureValues.ContainsKey(key))
            {
                secureValues[key].UpdateValue(value);
            }
            else
            {
                secureValues[key] = new SecureValue(value);
            }
        }

        /// <summary>
        /// 보안 값 가져오기
        /// </summary>
        public long GetSecureValue(string key)
        {
            if (secureValues.ContainsKey(key))
            {
                return secureValues[key].GetValue();
            }
            return 0;
        }

        /// <summary>
        /// 보안 값 검증
        /// </summary>
        private void ValidateSecureValues()
        {
            foreach (var kvp in secureValues)
            {
                if (!kvp.Value.ValidateIntegrity())
                {
                    Debug.LogWarning($"[SecurityManager] Memory tampering detected: {kvp.Key}");
                    ReportViolation(SecurityViolationType.MemoryTampering, $"Key: {kvp.Key}");
                }
            }
        }
        #endregion

        #region Speed Hack Detection
        /// <summary>
        /// 스피드핵 탐지 코루틴
        /// </summary>
        private IEnumerator SpeedHackDetectionCoroutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(speedCheckInterval);

                float currentRealTime = Time.realtimeSinceStartup;
                float currentGameTime = Time.time;

                float realTimeDelta = currentRealTime - lastRealTime;
                float gameTimeDelta = currentGameTime - lastGameTime;

                // 게임 시간이 실제 시간보다 비정상적으로 빠른지 체크
                if (realTimeDelta > 0 && gameTimeDelta / realTimeDelta > speedHackThreshold)
                {
                    Debug.LogWarning($"[SecurityManager] Speed hack detected! Ratio: {gameTimeDelta / realTimeDelta}");
                    ReportViolation(SecurityViolationType.SpeedHack, $"Ratio: {gameTimeDelta / realTimeDelta:F2}");
                }

                lastRealTime = currentRealTime;
                lastGameTime = currentGameTime;
            }
        }
        #endregion

        #region Data Integrity Check
        /// <summary>
        /// 데이터 무결성 체크 코루틴
        /// </summary>
        private IEnumerator IntegrityCheckCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(integrityCheckInterval);

                if (AuthManager.Instance?.IsLoggedIn == true)
                {
                    PerformIntegrityCheck();
                }
            }
        }

        /// <summary>
        /// 무결성 체크 수행
        /// </summary>
        public void PerformIntegrityCheck(Action<bool> callback = null)
        {
            if (isCheckingIntegrity)
            {
                callback?.Invoke(false);
                return;
            }

            isCheckingIntegrity = true;

            // 로컬 데이터 해시 생성
            string localDataHash = GenerateLocalDataHash();

            var checkRequest = new IntegrityCheckRequest
            {
                device_fingerprint = deviceFingerprint,
                session_token = sessionToken,
                data_hash = localDataHash,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                app_version = Application.version,
                platform = Application.platform.ToString()
            };

            NetworkManager.Instance?.Post("/security/integrity-check", checkRequest, (response) =>
            {
                isCheckingIntegrity = false;

                if (response.success)
                {
                    var checkResponse = response.GetData<IntegrityCheckResponse>();

                    if (checkResponse.is_valid)
                    {
                        OnIntegrityCheckPassed?.Invoke();
                        callback?.Invoke(true);
                    }
                    else
                    {
                        OnIntegrityCheckFailed?.Invoke(checkResponse.error_message);
                        ReportViolation(SecurityViolationType.DataTampering, checkResponse.error_message);
                        callback?.Invoke(false);
                    }
                }
                else
                {
                    // 네트워크 오류는 무시
                    callback?.Invoke(true);
                }
            });
        }

        /// <summary>
        /// 로컬 데이터 해시 생성
        /// </summary>
        private string GenerateLocalDataHash()
        {
            var gameManager = Managers.GameManager.Instance;
            if (gameManager?.PlayerStats == null) return "";

            // 핵심 게임 데이터로 해시 생성
            string rawData = $"{gameManager.PlayerStats.Level}_" +
                           $"{gameManager.PlayerStats.Gold.Serialize()}_" +
                           $"{gameManager.Battle?.CurrentStage ?? 0}";

            return ComputeHash(rawData);
        }
        #endregion

        #region Device Binding
        /// <summary>
        /// 디바이스 바인딩 검증
        /// </summary>
        public void ValidateDeviceBinding(Action<bool, string> callback)
        {
            if (!enableDeviceBindCheck)
            {
                callback?.Invoke(true, null);
                return;
            }

            var bindRequest = new DeviceBindRequest
            {
                device_fingerprint = deviceFingerprint,
                device_id = SystemInfo.deviceUniqueIdentifier,
                device_model = SystemInfo.deviceModel,
                os_version = SystemInfo.operatingSystem
            };

            NetworkManager.Instance?.Post("/security/validate-device", bindRequest, (response) =>
            {
                if (response.success)
                {
                    var bindResponse = response.GetData<DeviceBindResponse>();

                    if (bindResponse.is_valid)
                    {
                        callback?.Invoke(true, null);
                    }
                    else
                    {
                        ReportViolation(SecurityViolationType.DeviceMismatch, "Device binding mismatch");
                        callback?.Invoke(false, bindResponse.error_message);
                    }
                }
                else
                {
                    callback?.Invoke(true, null);  // 네트워크 오류는 패스
                }
            });
        }

        /// <summary>
        /// 새 디바이스 등록
        /// </summary>
        public void RegisterNewDevice(Action<bool> callback)
        {
            var registerRequest = new DeviceBindRequest
            {
                device_fingerprint = deviceFingerprint,
                device_id = SystemInfo.deviceUniqueIdentifier,
                device_model = SystemInfo.deviceModel,
                os_version = SystemInfo.operatingSystem
            };

            NetworkManager.Instance?.Post("/security/register-device", registerRequest, (response) =>
            {
                callback?.Invoke(response.success);
            });
        }
        #endregion

        #region Violation Handling
        /// <summary>
        /// 위반 보고
        /// </summary>
        private void ReportViolation(SecurityViolationType type, string details)
        {
            violationCount++;

            var violation = new SecurityViolation
            {
                type = type,
                details = details,
                timestamp = DateTime.Now,
                violationCount = violationCount
            };

            OnSecurityViolationDetected?.Invoke(violation);

            // 서버에 보고
            ReportViolationToServer(violation);

            // 최대 위반 횟수 초과 시 조치
            if (violationCount >= maxViolationsBeforeBan)
            {
                HandleMaxViolations();
            }
        }

        /// <summary>
        /// 서버에 위반 보고
        /// </summary>
        private void ReportViolationToServer(SecurityViolation violation)
        {
            var report = new ViolationReport
            {
                violation_type = violation.type.ToString(),
                details = violation.details,
                device_fingerprint = deviceFingerprint,
                session_token = sessionToken,
                timestamp = violation.timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                violation_count = violationCount
            };

            NetworkManager.Instance?.Post("/security/report-violation", report, (response) =>
            {
                // 응답 처리
                if (response.success)
                {
                    var serverResponse = response.GetData<ViolationReportResponse>();
                    if (serverResponse.action == "ban")
                    {
                        HandleBan();
                    }
                }
            });
        }

        /// <summary>
        /// 최대 위반 횟수 도달 처리
        /// </summary>
        private void HandleMaxViolations()
        {
            Debug.LogError("[SecurityManager] Maximum violations reached!");

            // 게임 일시정지
            Time.timeScale = 0;

            // 경고 UI 표시 (UI 매니저를 통해)
            // UIManager.Instance?.ShowSecurityWarning();
        }

        /// <summary>
        /// 계정 밴 처리
        /// </summary>
        private void HandleBan()
        {
            Debug.LogError("[SecurityManager] Account has been banned!");

            // 로그아웃
            AuthManager.Instance?.Logout();

            // 저장 데이터 삭제
            PlayerPrefs.DeleteAll();
        }
        #endregion

        #region Encryption
        /// <summary>
        /// 데이터 암호화
        /// </summary>
        public string EncryptData(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(GetEncryptionKey());
                byte[] iv = new byte[16];  // 실제로는 랜덤 IV 사용 권장

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecurityManager] Encryption failed: {e.Message}");
                return plainText;
            }
        }

        /// <summary>
        /// 데이터 복호화
        /// </summary>
        public string DecryptData(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] key = Encoding.UTF8.GetBytes(GetEncryptionKey());
                byte[] iv = new byte[16];

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecurityManager] Decryption failed: {e.Message}");
                return cipherText;
            }
        }

        /// <summary>
        /// 암호화 키 가져오기
        /// </summary>
        private string GetEncryptionKey()
        {
            // 실제로는 서버에서 받거나 난독화된 키 사용
            string baseKey = "YourSecretKey123";
            return baseKey.PadRight(32, '0').Substring(0, 32);
        }
        #endregion

        #region Hash
        /// <summary>
        /// SHA256 해시 계산
        /// </summary>
        public string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// HMAC 생성
        /// </summary>
        public string ComputeHMAC(string input, string key)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
        #endregion

        #region Request Signing
        /// <summary>
        /// API 요청 서명
        /// </summary>
        public string SignRequest(string endpoint, string body, string timestamp)
        {
            string dataToSign = $"{endpoint}|{body}|{timestamp}|{sessionToken}";
            return ComputeHMAC(dataToSign, deviceFingerprint);
        }

        /// <summary>
        /// 서명된 요청 헤더 생성
        /// </summary>
        public Dictionary<string, string> GetSignedHeaders(string endpoint, string body)
        {
            string timestamp = DateTime.UtcNow.ToString("o");
            string signature = SignRequest(endpoint, body, timestamp);

            return new Dictionary<string, string>
            {
                { "X-Timestamp", timestamp },
                { "X-Signature", signature },
                { "X-Device-Id", deviceFingerprint },
                { "X-Session-Token", sessionToken }
            };
        }
        #endregion

        #region Anti-Cheat Utilities
        /// <summary>
        /// 비정상적인 값 체크
        /// </summary>
        public bool ValidateGameValue(string valueName, long value, long minValue, long maxValue)
        {
            if (value < minValue || value > maxValue)
            {
                ReportViolation(SecurityViolationType.InvalidValue, $"{valueName}: {value} (valid: {minValue}-{maxValue})");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 비정상적인 진행 체크
        /// </summary>
        public bool ValidateProgressionRate(float actualRate, float expectedMaxRate)
        {
            if (actualRate > expectedMaxRate)
            {
                ReportViolation(SecurityViolationType.AbnormalProgression, $"Rate: {actualRate}, Max: {expectedMaxRate}");
                return false;
            }
            return true;
        }
        #endregion
    }

    #region Secure Value Class
    /// <summary>
    /// 메모리 보호 값 (해킹 방지)
    /// </summary>
    internal class SecureValue
    {
        private long encryptedValue;
        private long checksum;
        private int key;

        public SecureValue(long value)
        {
            key = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            UpdateValue(value);
        }

        public void UpdateValue(long value)
        {
            encryptedValue = value ^ key;
            checksum = CalculateChecksum(value);
        }

        public long GetValue()
        {
            return encryptedValue ^ key;
        }

        public bool ValidateIntegrity()
        {
            long decryptedValue = encryptedValue ^ key;
            return checksum == CalculateChecksum(decryptedValue);
        }

        private long CalculateChecksum(long value)
        {
            return value * 31 + key;
        }
    }
    #endregion

    #region Enums
    /// <summary>
    /// 보안 위반 타입
    /// </summary>
    public enum SecurityViolationType
    {
        MemoryTampering,
        SpeedHack,
        DataTampering,
        DeviceMismatch,
        InvalidValue,
        AbnormalProgression,
        RootedDevice,
        ModifiedAPK
    }
    #endregion

    #region Data Classes
    [Serializable]
    public class SecurityViolation
    {
        public SecurityViolationType type;
        public string details;
        public DateTime timestamp;
        public int violationCount;
    }

    [Serializable]
    public class IntegrityCheckRequest
    {
        public string device_fingerprint;
        public string session_token;
        public string data_hash;
        public string timestamp;
        public string app_version;
        public string platform;
    }

    [Serializable]
    public class IntegrityCheckResponse
    {
        public bool is_valid;
        public string error_code;
        public string error_message;
    }

    [Serializable]
    public class DeviceBindRequest
    {
        public string device_fingerprint;
        public string device_id;
        public string device_model;
        public string os_version;
    }

    [Serializable]
    public class DeviceBindResponse
    {
        public bool is_valid;
        public string error_message;
        public bool is_new_device;
    }

    [Serializable]
    public class ViolationReport
    {
        public string violation_type;
        public string details;
        public string device_fingerprint;
        public string session_token;
        public string timestamp;
        public int violation_count;
    }

    [Serializable]
    public class ViolationReportResponse
    {
        public string action;  // "warn", "suspend", "ban"
        public string message;
    }
    #endregion
}
