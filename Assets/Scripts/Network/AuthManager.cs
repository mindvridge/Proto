using System;
using System.Collections;
using UnityEngine;

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 인증 매니저 - 카카오, 구글, 이메일 로그인 관리
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; }

        #region Events
        public event Action<UserInfo> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogoutCompleted;
        public event Action<UserInfo> OnUserInfoUpdated;
        #endregion

        #region Settings
        [Header("=== OAuth Settings ===")]
        [SerializeField] private string kakaoAppKey = "YOUR_KAKAO_APP_KEY";
        [SerializeField] private string googleClientId = "YOUR_GOOGLE_CLIENT_ID";

        [Header("=== Auto Login ===")]
        [SerializeField] private bool enableAutoLogin = true;
        [SerializeField] private string tokenPrefsKey = "auth_token";
        [SerializeField] private string refreshPrefsKey = "refresh_token";
        [SerializeField] private string userIdPrefsKey = "user_id";
        [SerializeField] private string loginTypePrefsKey = "login_type";
        #endregion

        #region State
        private UserInfo currentUser;
        private LoginType lastLoginType = LoginType.None;
        private bool isLoggedIn = false;
        private bool isLoggingIn = false;
        #endregion

        #region Properties
        public UserInfo CurrentUser => currentUser;
        public bool IsLoggedIn => isLoggedIn;
        public bool IsLoggingIn => isLoggingIn;
        public LoginType LastLoginType => lastLoginType;
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
            if (enableAutoLogin)
            {
                TryAutoLogin();
            }
        }
        #endregion

        #region Auto Login
        /// <summary>
        /// 자동 로그인 시도
        /// </summary>
        public void TryAutoLogin()
        {
            string savedToken = PlayerPrefs.GetString(tokenPrefsKey, "");
            string savedRefresh = PlayerPrefs.GetString(refreshPrefsKey, "");
            string savedLoginType = PlayerPrefs.GetString(loginTypePrefsKey, "");

            if (string.IsNullOrEmpty(savedToken))
            {
                Debug.Log("[AuthManager] No saved token for auto login");
                return;
            }

            if (Enum.TryParse(savedLoginType, out LoginType loginType))
            {
                lastLoginType = loginType;
            }

            Debug.Log("[AuthManager] Attempting auto login...");

            // 토큰 설정
            NetworkManager.Instance?.SetAuthToken(savedToken, savedRefresh);

            // 토큰 유효성 검증
            ValidateToken((success, user) =>
            {
                if (success)
                {
                    currentUser = user;
                    isLoggedIn = true;
                    OnLoginSuccess?.Invoke(user);
                    Debug.Log($"[AuthManager] Auto login successful: {user.nickname}");
                }
                else
                {
                    // 토큰 갱신 시도
                    NetworkManager.Instance?.RefreshAuthToken((refreshSuccess) =>
                    {
                        if (refreshSuccess)
                        {
                            ValidateToken((validateSuccess, validateUser) =>
                            {
                                if (validateSuccess)
                                {
                                    currentUser = validateUser;
                                    isLoggedIn = true;
                                    SaveLoginInfo();
                                    OnLoginSuccess?.Invoke(validateUser);
                                }
                                else
                                {
                                    ClearLoginInfo();
                                }
                            });
                        }
                        else
                        {
                            ClearLoginInfo();
                        }
                    });
                }
            });
        }

        /// <summary>
        /// 토큰 유효성 검증
        /// </summary>
        private void ValidateToken(Action<bool, UserInfo> callback)
        {
            NetworkManager.Instance?.Get("/auth/me", (response) =>
            {
                if (response.success)
                {
                    var user = response.GetData<UserInfo>();
                    callback?.Invoke(true, user);
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            });
        }
        #endregion

        #region Email Login
        /// <summary>
        /// 이메일 로그인
        /// </summary>
        public void LoginWithEmail(string email, string password, Action<bool, string> callback = null)
        {
            if (isLoggingIn)
            {
                callback?.Invoke(false, "이미 로그인 중입니다.");
                return;
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                callback?.Invoke(false, "이메일과 비밀번호를 입력해주세요.");
                OnLoginFailed?.Invoke("이메일과 비밀번호를 입력해주세요.");
                return;
            }

            isLoggingIn = true;

            var loginData = new EmailLoginRequest
            {
                email = email,
                password = password
            };

            NetworkManager.Instance?.Post("/auth/login/email", loginData, (response) =>
            {
                isLoggingIn = false;

                if (response.success)
                {
                    HandleLoginResponse(response, LoginType.Email);
                    callback?.Invoke(true, null);
                }
                else
                {
                    string errorMsg = response.errorMessage ?? "로그인에 실패했습니다.";
                    OnLoginFailed?.Invoke(errorMsg);
                    callback?.Invoke(false, errorMsg);
                }
            }, false);
        }

        /// <summary>
        /// 이메일 회원가입
        /// </summary>
        public void RegisterWithEmail(string email, string password, string nickname, Action<bool, string> callback = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
            {
                callback?.Invoke(false, "모든 필드를 입력해주세요.");
                return;
            }

            var registerData = new EmailRegisterRequest
            {
                email = email,
                password = password,
                nickname = nickname
            };

            NetworkManager.Instance?.Post("/auth/register/email", registerData, (response) =>
            {
                if (response.success)
                {
                    // 자동 로그인
                    LoginWithEmail(email, password, callback);
                }
                else
                {
                    string errorMsg = response.errorMessage ?? "회원가입에 실패했습니다.";
                    callback?.Invoke(false, errorMsg);
                }
            }, false);
        }
        #endregion

        #region Kakao Login
        /// <summary>
        /// 카카오 로그인
        /// </summary>
        public void LoginWithKakao(Action<bool, string> callback = null)
        {
            if (isLoggingIn)
            {
                callback?.Invoke(false, "이미 로그인 중입니다.");
                return;
            }

            isLoggingIn = true;
            Debug.Log("[AuthManager] Starting Kakao login...");

#if UNITY_ANDROID && !UNITY_EDITOR
            StartKakaoLoginAndroid(callback);
#elif UNITY_IOS && !UNITY_EDITOR
            StartKakaoLoginIOS(callback);
#else
            // 에디터에서는 Mock 로그인
            StartCoroutine(MockSocialLogin(LoginType.Kakao, callback));
#endif
        }

        /// <summary>
        /// 카카오 로그인 (Android)
        /// </summary>
        private void StartKakaoLoginAndroid(Action<bool, string> callback)
        {
            try
            {
                // Kakao SDK Android 네이티브 호출
                // 실제 구현 시 Kakao SDK for Unity 또는 Android Plugin 사용
                using (AndroidJavaClass kakaoClass = new AndroidJavaClass("com.kakao.sdk.user.UserApiClient"))
                {
                    AndroidJavaObject instance = kakaoClass.CallStatic<AndroidJavaObject>("getInstance");

                    // 카카오톡 설치 여부 확인
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                        bool isKakaoTalkInstalled = instance.Call<bool>("isKakaoTalkLoginAvailable", context);

                        if (isKakaoTalkInstalled)
                        {
                            // 카카오톡으로 로그인
                            instance.Call("loginWithKakaoTalk", context, new KakaoLoginCallback(this, callback));
                        }
                        else
                        {
                            // 카카오 계정으로 로그인
                            instance.Call("loginWithKakaoAccount", context, new KakaoLoginCallback(this, callback));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AuthManager] Kakao login error: {e.Message}");
                isLoggingIn = false;
                callback?.Invoke(false, e.Message);
                OnLoginFailed?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// 카카오 로그인 (iOS)
        /// </summary>
        private void StartKakaoLoginIOS(Action<bool, string> callback)
        {
            // iOS 네이티브 플러그인 호출
            // 실제 구현 시 iOS Plugin 사용
            Debug.LogWarning("[AuthManager] Kakao iOS login not implemented yet");
            isLoggingIn = false;
            callback?.Invoke(false, "iOS 카카오 로그인 미구현");
        }

        /// <summary>
        /// 카카오 토큰 처리
        /// </summary>
        public void OnKakaoTokenReceived(string accessToken, string refreshToken)
        {
            var tokenData = new SocialLoginRequest
            {
                provider = "kakao",
                access_token = accessToken
            };

            NetworkManager.Instance?.Post("/auth/login/social", tokenData, (response) =>
            {
                isLoggingIn = false;

                if (response.success)
                {
                    HandleLoginResponse(response, LoginType.Kakao);
                }
                else
                {
                    OnLoginFailed?.Invoke(response.errorMessage ?? "카카오 로그인 실패");
                }
            }, false);
        }
        #endregion

        #region Google Login
        /// <summary>
        /// 구글 로그인
        /// </summary>
        public void LoginWithGoogle(Action<bool, string> callback = null)
        {
            if (isLoggingIn)
            {
                callback?.Invoke(false, "이미 로그인 중입니다.");
                return;
            }

            isLoggingIn = true;
            Debug.Log("[AuthManager] Starting Google login...");

#if UNITY_ANDROID && !UNITY_EDITOR
            StartGoogleLoginAndroid(callback);
#elif UNITY_IOS && !UNITY_EDITOR
            StartGoogleLoginIOS(callback);
#else
            // 에디터에서는 Mock 로그인
            StartCoroutine(MockSocialLogin(LoginType.Google, callback));
#endif
        }

        /// <summary>
        /// 구글 로그인 (Android)
        /// </summary>
        private void StartGoogleLoginAndroid(Action<bool, string> callback)
        {
            try
            {
                // Google Sign-In Android 네이티브 호출
                // 실제 구현 시 Google Play Games Services 또는 Firebase Auth 사용
                using (AndroidJavaClass signInClass = new AndroidJavaClass("com.google.android.gms.auth.api.signin.GoogleSignIn"))
                {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                        // GoogleSignInOptions 설정
                        using (AndroidJavaClass optionsBuilder = new AndroidJavaClass("com.google.android.gms.auth.api.signin.GoogleSignInOptions$Builder"))
                        {
                            // 실제 구현 필요
                            Debug.LogWarning("[AuthManager] Google Android login implementation needed");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AuthManager] Google login error: {e.Message}");
                isLoggingIn = false;
                callback?.Invoke(false, e.Message);
                OnLoginFailed?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// 구글 로그인 (iOS)
        /// </summary>
        private void StartGoogleLoginIOS(Action<bool, string> callback)
        {
            // iOS 네이티브 플러그인 호출
            Debug.LogWarning("[AuthManager] Google iOS login not implemented yet");
            isLoggingIn = false;
            callback?.Invoke(false, "iOS 구글 로그인 미구현");
        }

        /// <summary>
        /// 구글 토큰 처리
        /// </summary>
        public void OnGoogleTokenReceived(string idToken)
        {
            var tokenData = new SocialLoginRequest
            {
                provider = "google",
                access_token = idToken
            };

            NetworkManager.Instance?.Post("/auth/login/social", tokenData, (response) =>
            {
                isLoggingIn = false;

                if (response.success)
                {
                    HandleLoginResponse(response, LoginType.Google);
                }
                else
                {
                    OnLoginFailed?.Invoke(response.errorMessage ?? "구글 로그인 실패");
                }
            }, false);
        }
        #endregion

        #region Guest Login
        /// <summary>
        /// 게스트 로그인 (기기 ID 기반)
        /// </summary>
        public void LoginAsGuest(Action<bool, string> callback = null)
        {
            if (isLoggingIn)
            {
                callback?.Invoke(false, "이미 로그인 중입니다.");
                return;
            }

            isLoggingIn = true;
            string deviceId = SystemInfo.deviceUniqueIdentifier;

            var guestData = new GuestLoginRequest
            {
                device_id = deviceId,
                platform = Application.platform.ToString()
            };

            NetworkManager.Instance?.Post("/auth/login/guest", guestData, (response) =>
            {
                isLoggingIn = false;

                if (response.success)
                {
                    HandleLoginResponse(response, LoginType.Guest);
                    callback?.Invoke(true, null);
                }
                else
                {
                    string errorMsg = response.errorMessage ?? "게스트 로그인 실패";
                    OnLoginFailed?.Invoke(errorMsg);
                    callback?.Invoke(false, errorMsg);
                }
            }, false);
        }
        #endregion

        #region Common Login Handling
        /// <summary>
        /// 로그인 응답 처리
        /// </summary>
        private void HandleLoginResponse(NetworkResponse response, LoginType loginType)
        {
            var loginResponse = response.GetData<LoginResponse>();

            if (loginResponse != null)
            {
                // 토큰 저장
                NetworkManager.Instance?.SetAuthToken(loginResponse.access_token, loginResponse.refresh_token);

                // 유저 정보 저장
                currentUser = loginResponse.user;
                lastLoginType = loginType;
                isLoggedIn = true;

                // 로컬 저장
                SaveLoginInfo();

                OnLoginSuccess?.Invoke(currentUser);
                Debug.Log($"[AuthManager] Login successful: {currentUser.nickname} ({loginType})");
            }
        }

        /// <summary>
        /// 로그인 정보 저장
        /// </summary>
        private void SaveLoginInfo()
        {
            if (NetworkManager.Instance != null && NetworkManager.Instance.HasAuthToken)
            {
                // 실제로는 보안을 위해 암호화 저장 필요
                PlayerPrefs.SetString(loginTypePrefsKey, lastLoginType.ToString());
                PlayerPrefs.SetString(userIdPrefsKey, currentUser?.user_id ?? "");
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 로그인 정보 삭제
        /// </summary>
        private void ClearLoginInfo()
        {
            PlayerPrefs.DeleteKey(tokenPrefsKey);
            PlayerPrefs.DeleteKey(refreshPrefsKey);
            PlayerPrefs.DeleteKey(userIdPrefsKey);
            PlayerPrefs.DeleteKey(loginTypePrefsKey);
            PlayerPrefs.Save();

            currentUser = null;
            isLoggedIn = false;
            lastLoginType = LoginType.None;
        }
        #endregion

        #region Logout
        /// <summary>
        /// 로그아웃
        /// </summary>
        public void Logout(Action callback = null)
        {
            NetworkManager.Instance?.Post("/auth/logout", null, (response) =>
            {
                // 서버 응답과 관계없이 로컬 정리
                PerformLocalLogout();
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 로컬 로그아웃 처리
        /// </summary>
        private void PerformLocalLogout()
        {
            NetworkManager.Instance?.ClearAuthToken();
            ClearLoginInfo();
            OnLogoutCompleted?.Invoke();
            Debug.Log("[AuthManager] Logged out");
        }
        #endregion

        #region Account Linking
        /// <summary>
        /// 게스트 계정을 소셜 계정에 연동
        /// </summary>
        public void LinkAccount(LoginType targetType, Action<bool, string> callback)
        {
            if (lastLoginType != LoginType.Guest)
            {
                callback?.Invoke(false, "게스트 계정만 연동할 수 있습니다.");
                return;
            }

            // 소셜 로그인 후 연동 처리
            Action<bool, string> linkCallback = (success, error) =>
            {
                if (success)
                {
                    lastLoginType = targetType;
                    SaveLoginInfo();
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, error);
                }
            };

            switch (targetType)
            {
                case LoginType.Kakao:
                    LoginWithKakao(linkCallback);
                    break;
                case LoginType.Google:
                    LoginWithGoogle(linkCallback);
                    break;
                default:
                    callback?.Invoke(false, "지원하지 않는 연동 방식입니다.");
                    break;
            }
        }
        #endregion

        #region User Info
        /// <summary>
        /// 유저 정보 갱신
        /// </summary>
        public void RefreshUserInfo(Action<bool> callback = null)
        {
            NetworkManager.Instance?.Get("/auth/me", (response) =>
            {
                if (response.success)
                {
                    currentUser = response.GetData<UserInfo>();
                    OnUserInfoUpdated?.Invoke(currentUser);
                    callback?.Invoke(true);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// 닉네임 변경
        /// </summary>
        public void ChangeNickname(string newNickname, Action<bool, string> callback)
        {
            var data = new NicknameChangeRequest { nickname = newNickname };

            NetworkManager.Instance?.Put("/auth/nickname", data, (response) =>
            {
                if (response.success)
                {
                    currentUser.nickname = newNickname;
                    OnUserInfoUpdated?.Invoke(currentUser);
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, response.errorMessage);
                }
            });
        }
        #endregion

        #region Account Deletion
        /// <summary>
        /// 계정 삭제
        /// </summary>
        public void DeleteAccount(Action<bool, string> callback)
        {
            NetworkManager.Instance?.Delete("/auth/account", (response) =>
            {
                if (response.success)
                {
                    PerformLocalLogout();
                    callback?.Invoke(true, null);
                }
                else
                {
                    callback?.Invoke(false, response.errorMessage);
                }
            });
        }
        #endregion

        #region Mock Login (Editor Only)
        /// <summary>
        /// Mock 소셜 로그인 (에디터 테스트용)
        /// </summary>
        private IEnumerator MockSocialLogin(LoginType loginType, Action<bool, string> callback)
        {
            Debug.Log($"[AuthManager] Mock {loginType} login (Editor mode)");
            yield return new WaitForSeconds(1f);

            var mockData = new SocialLoginRequest
            {
                provider = loginType.ToString().ToLower(),
                access_token = $"mock_token_{SystemInfo.deviceUniqueIdentifier}"
            };

            NetworkManager.Instance?.Post("/auth/login/social", mockData, (response) =>
            {
                isLoggingIn = false;

                if (response.success)
                {
                    HandleLoginResponse(response, loginType);
                    callback?.Invoke(true, null);
                }
                else
                {
                    // Mock 서버가 없을 때 로컬 처리
                    currentUser = new UserInfo
                    {
                        user_id = SystemInfo.deviceUniqueIdentifier,
                        nickname = $"TestUser_{UnityEngine.Random.Range(1000, 9999)}",
                        level = 1,
                        created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    lastLoginType = loginType;
                    isLoggedIn = true;
                    SaveLoginInfo();
                    OnLoginSuccess?.Invoke(currentUser);
                    callback?.Invoke(true, null);
                }
            }, false);
        }
        #endregion
    }

    #region Kakao Login Callback (Android)
    /// <summary>
    /// 카카오 로그인 콜백 (Android용)
    /// </summary>
    internal class KakaoLoginCallback : AndroidJavaProxy
    {
        private AuthManager authManager;
        private Action<bool, string> callback;

        public KakaoLoginCallback(AuthManager manager, Action<bool, string> callback)
            : base("kotlin.jvm.functions.Function2")
        {
            this.authManager = manager;
            this.callback = callback;
        }

        public void invoke(AndroidJavaObject token, AndroidJavaObject error)
        {
            if (error != null)
            {
                string errorMessage = error.Call<string>("getMessage");
                callback?.Invoke(false, errorMessage);
            }
            else if (token != null)
            {
                string accessToken = token.Call<string>("getAccessToken");
                string refreshToken = token.Call<string>("getRefreshToken");
                authManager.OnKakaoTokenReceived(accessToken, refreshToken);
                callback?.Invoke(true, null);
            }
        }
    }
    #endregion

    #region Enums
    /// <summary>
    /// 로그인 타입
    /// </summary>
    public enum LoginType
    {
        None,
        Guest,
        Email,
        Kakao,
        Google,
        Apple,
        Facebook
    }
    #endregion

    #region Request/Response Classes
    [Serializable]
    public class EmailLoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class EmailRegisterRequest
    {
        public string email;
        public string password;
        public string nickname;
    }

    [Serializable]
    public class SocialLoginRequest
    {
        public string provider;
        public string access_token;
    }

    [Serializable]
    public class GuestLoginRequest
    {
        public string device_id;
        public string platform;
    }

    [Serializable]
    public class NicknameChangeRequest
    {
        public string nickname;
    }

    [Serializable]
    public class LoginResponse
    {
        public string access_token;
        public string refresh_token;
        public int expires_in;
        public UserInfo user;
    }

    [Serializable]
    public class UserInfo
    {
        public string user_id;
        public string nickname;
        public string email;
        public int level;
        public string profile_image;
        public string created_at;
        public string last_login;
    }
    #endregion
}
