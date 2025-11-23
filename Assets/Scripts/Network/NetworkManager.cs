using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 네트워크 매니저 - REST API 통신 기본 클래스
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        #region Events
        public event Action OnNetworkConnected;
        public event Action OnNetworkDisconnected;
        public event Action<NetworkError> OnNetworkError;
        #endregion

        #region Settings
        [Header("=== Server Settings ===")]
        [SerializeField] private string baseUrl = "https://api.yourgame.com";
        [SerializeField] private string apiVersion = "v1";
        [SerializeField] private float requestTimeout = 30f;
        [SerializeField] private int maxRetryCount = 3;
        [SerializeField] private float retryDelay = 1f;

        [Header("=== Debug Settings ===")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool useMockServer = false;
        #endregion

        #region State
        private string authToken;
        private string refreshToken;
        private bool isConnected = false;
        private Queue<NetworkRequest> requestQueue = new Queue<NetworkRequest>();
        private bool isProcessingQueue = false;
        #endregion

        #region Properties
        public bool IsConnected => isConnected;
        public string BaseUrl => baseUrl;
        public string ApiUrl => $"{baseUrl}/api/{apiVersion}";
        public bool HasAuthToken => !string.IsNullOrEmpty(authToken);
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
            StartCoroutine(CheckNetworkConnection());
        }
        #endregion

        #region Connection Check
        /// <summary>
        /// 네트워크 연결 상태 체크
        /// </summary>
        private IEnumerator CheckNetworkConnection()
        {
            while (true)
            {
                bool previousState = isConnected;
                isConnected = Application.internetReachability != NetworkReachability.NotReachable;

                if (isConnected && !previousState)
                {
                    OnNetworkConnected?.Invoke();
                    Log("Network connected");
                }
                else if (!isConnected && previousState)
                {
                    OnNetworkDisconnected?.Invoke();
                    Log("Network disconnected");
                }

                yield return new WaitForSeconds(5f);
            }
        }

        /// <summary>
        /// 서버 연결 확인 (ping)
        /// </summary>
        public void PingServer(Action<bool> callback)
        {
            Get("/ping", (response) =>
            {
                callback?.Invoke(response.success);
            });
        }
        #endregion

        #region Auth Token Management
        /// <summary>
        /// 인증 토큰 설정
        /// </summary>
        public void SetAuthToken(string token, string refresh = null)
        {
            authToken = token;
            if (!string.IsNullOrEmpty(refresh))
            {
                refreshToken = refresh;
            }
            Log($"Auth token set: {(string.IsNullOrEmpty(token) ? "cleared" : "***")}");
        }

        /// <summary>
        /// 인증 토큰 클리어
        /// </summary>
        public void ClearAuthToken()
        {
            authToken = null;
            refreshToken = null;
            Log("Auth token cleared");
        }

        /// <summary>
        /// 토큰 갱신
        /// </summary>
        public void RefreshAuthToken(Action<bool> callback)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                callback?.Invoke(false);
                return;
            }

            var data = new Dictionary<string, string>
            {
                { "refresh_token", refreshToken }
            };

            Post("/auth/refresh", data, (response) =>
            {
                if (response.success)
                {
                    var tokenData = JsonUtility.FromJson<TokenResponse>(response.data);
                    SetAuthToken(tokenData.access_token, tokenData.refresh_token);
                }
                callback?.Invoke(response.success);
            });
        }
        #endregion

        #region HTTP Methods
        /// <summary>
        /// GET 요청
        /// </summary>
        public void Get(string endpoint, Action<NetworkResponse> callback, bool requireAuth = true)
        {
            StartCoroutine(SendRequest(endpoint, "GET", null, callback, requireAuth));
        }

        /// <summary>
        /// POST 요청
        /// </summary>
        public void Post(string endpoint, object data, Action<NetworkResponse> callback, bool requireAuth = true)
        {
            string jsonData = data != null ? JsonUtility.ToJson(data) : null;
            StartCoroutine(SendRequest(endpoint, "POST", jsonData, callback, requireAuth));
        }

        /// <summary>
        /// POST 요청 (Dictionary)
        /// </summary>
        public void Post(string endpoint, Dictionary<string, string> data, Action<NetworkResponse> callback, bool requireAuth = true)
        {
            string jsonData = data != null ? DictionaryToJson(data) : null;
            StartCoroutine(SendRequest(endpoint, "POST", jsonData, callback, requireAuth));
        }

        /// <summary>
        /// PUT 요청
        /// </summary>
        public void Put(string endpoint, object data, Action<NetworkResponse> callback, bool requireAuth = true)
        {
            string jsonData = data != null ? JsonUtility.ToJson(data) : null;
            StartCoroutine(SendRequest(endpoint, "PUT", jsonData, callback, requireAuth));
        }

        /// <summary>
        /// DELETE 요청
        /// </summary>
        public void Delete(string endpoint, Action<NetworkResponse> callback, bool requireAuth = true)
        {
            StartCoroutine(SendRequest(endpoint, "DELETE", null, callback, requireAuth));
        }
        #endregion

        #region Request Processing
        /// <summary>
        /// HTTP 요청 전송
        /// </summary>
        private IEnumerator SendRequest(string endpoint, string method, string jsonData,
            Action<NetworkResponse> callback, bool requireAuth, int retryCount = 0)
        {
            if (!isConnected)
            {
                var errorResponse = new NetworkResponse
                {
                    success = false,
                    errorCode = "NETWORK_UNAVAILABLE",
                    errorMessage = "네트워크 연결을 확인해주세요."
                };
                callback?.Invoke(errorResponse);
                OnNetworkError?.Invoke(new NetworkError(errorResponse.errorCode, errorResponse.errorMessage));
                yield break;
            }

            string url = $"{ApiUrl}{endpoint}";
            Log($"[{method}] {url}");

            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                // 바디 설정
                if (!string.IsNullOrEmpty(jsonData))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    Log($"Request Body: {jsonData}");
                }

                request.downloadHandler = new DownloadHandlerBuffer();

                // 헤더 설정
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");

                if (requireAuth && !string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                // 타임아웃 설정
                request.timeout = (int)requestTimeout;

                // 요청 전송
                yield return request.SendWebRequest();

                // 응답 처리
                NetworkResponse response = new NetworkResponse();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    response.success = true;
                    response.statusCode = (int)request.responseCode;
                    response.data = request.downloadHandler.text;
                    Log($"Response [{response.statusCode}]: {response.data}");
                }
                else
                {
                    response.success = false;
                    response.statusCode = (int)request.responseCode;
                    response.errorCode = request.result.ToString();
                    response.errorMessage = request.error;

                    // 에러 응답 파싱 시도
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        try
                        {
                            var errorData = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                            response.errorCode = errorData.code;
                            response.errorMessage = errorData.message;
                        }
                        catch { }
                    }

                    Log($"Error [{response.statusCode}]: {response.errorMessage}");

                    // 401 Unauthorized - 토큰 갱신 시도
                    if (response.statusCode == 401 && !string.IsNullOrEmpty(refreshToken))
                    {
                        bool refreshed = false;
                        RefreshAuthToken((success) => refreshed = success);

                        yield return new WaitUntil(() => refreshed || !string.IsNullOrEmpty(authToken));

                        if (refreshed)
                        {
                            // 토큰 갱신 후 재시도
                            StartCoroutine(SendRequest(endpoint, method, jsonData, callback, requireAuth, retryCount));
                            yield break;
                        }
                    }

                    // 재시도 로직
                    if (ShouldRetry(response.statusCode) && retryCount < maxRetryCount)
                    {
                        Log($"Retrying... ({retryCount + 1}/{maxRetryCount})");
                        yield return new WaitForSeconds(retryDelay * Mathf.Pow(2, retryCount));
                        StartCoroutine(SendRequest(endpoint, method, jsonData, callback, requireAuth, retryCount + 1));
                        yield break;
                    }

                    OnNetworkError?.Invoke(new NetworkError(response.errorCode, response.errorMessage));
                }

                callback?.Invoke(response);
            }
        }

        /// <summary>
        /// 재시도 여부 판단
        /// </summary>
        private bool ShouldRetry(int statusCode)
        {
            // 5xx 서버 에러, 408 타임아웃, 429 Rate Limit
            return statusCode >= 500 || statusCode == 408 || statusCode == 429;
        }
        #endregion

        #region Queued Requests
        /// <summary>
        /// 요청 큐에 추가 (순차 처리용)
        /// </summary>
        public void QueueRequest(string endpoint, string method, object data, Action<NetworkResponse> callback)
        {
            requestQueue.Enqueue(new NetworkRequest
            {
                endpoint = endpoint,
                method = method,
                data = data != null ? JsonUtility.ToJson(data) : null,
                callback = callback
            });

            if (!isProcessingQueue)
            {
                StartCoroutine(ProcessRequestQueue());
            }
        }

        /// <summary>
        /// 요청 큐 처리
        /// </summary>
        private IEnumerator ProcessRequestQueue()
        {
            isProcessingQueue = true;

            while (requestQueue.Count > 0)
            {
                var request = requestQueue.Dequeue();
                bool completed = false;

                StartCoroutine(SendRequest(request.endpoint, request.method, request.data,
                    (response) =>
                    {
                        request.callback?.Invoke(response);
                        completed = true;
                    }, true));

                yield return new WaitUntil(() => completed);
            }

            isProcessingQueue = false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Dictionary를 JSON으로 변환
        /// </summary>
        private string DictionaryToJson(Dictionary<string, string> dict)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            int count = 0;
            foreach (var kvp in dict)
            {
                if (count > 0) sb.Append(",");
                sb.Append($"\"{kvp.Key}\":\"{kvp.Value}\"");
                count++;
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 로그 출력
        /// </summary>
        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[NetworkManager] {message}");
            }
        }
        #endregion

        #region Server Configuration
        /// <summary>
        /// 서버 URL 변경 (개발/운영 환경 전환용)
        /// </summary>
        public void SetServerUrl(string url)
        {
            baseUrl = url;
            Log($"Server URL changed to: {url}");
        }

        /// <summary>
        /// Mock 서버 모드 설정
        /// </summary>
        public void SetMockMode(bool enabled)
        {
            useMockServer = enabled;
            Log($"Mock mode: {(enabled ? "enabled" : "disabled")}");
        }
        #endregion
    }

    #region Data Classes
    /// <summary>
    /// 네트워크 응답
    /// </summary>
    [Serializable]
    public class NetworkResponse
    {
        public bool success;
        public int statusCode;
        public string data;
        public string errorCode;
        public string errorMessage;

        /// <summary>
        /// JSON 데이터를 객체로 변환
        /// </summary>
        public T GetData<T>()
        {
            if (string.IsNullOrEmpty(data)) return default;
            return JsonUtility.FromJson<T>(data);
        }
    }

    /// <summary>
    /// 네트워크 에러
    /// </summary>
    public class NetworkError
    {
        public string code;
        public string message;

        public NetworkError(string code, string message)
        {
            this.code = code;
            this.message = message;
        }
    }

    /// <summary>
    /// 네트워크 요청 (큐용)
    /// </summary>
    internal class NetworkRequest
    {
        public string endpoint;
        public string method;
        public string data;
        public Action<NetworkResponse> callback;
    }

    /// <summary>
    /// 토큰 응답
    /// </summary>
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string refresh_token;
        public int expires_in;
    }

    /// <summary>
    /// 에러 응답
    /// </summary>
    [Serializable]
    public class ErrorResponse
    {
        public string code;
        public string message;
    }
    #endregion
}
