using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 인앱 결제 매니저 - 구매 및 서버 검증
    /// </summary>
    public class IAPManager : MonoBehaviour
#if UNITY_PURCHASING
        , IDetailedStoreListener
#endif
    {
        public static IAPManager Instance { get; private set; }

        #region Events
        public event Action OnStoreInitialized;
        public event Action<string> OnStoreInitializeFailed;
        public event Action<IAPProduct> OnPurchaseStarted;
        public event Action<IAPProduct, PurchaseResult> OnPurchaseCompleted;
        public event Action<IAPProduct, string> OnPurchaseFailed;
        public event Action<IAPProduct> OnPurchaseRestored;
        #endregion

        #region Settings
        [Header("=== IAP Settings ===")]
        [SerializeField] private List<IAPProductConfig> productConfigs = new List<IAPProductConfig>();
        [SerializeField] private bool enableServerValidation = true;
        [SerializeField] private bool enableTestMode = false;

        [Header("=== Retry Settings ===")]
        [SerializeField] private int maxValidationRetries = 3;
        [SerializeField] private float validationRetryDelay = 2f;
        #endregion

        #region State
#if UNITY_PURCHASING
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;
#endif
        private Dictionary<string, IAPProduct> products = new Dictionary<string, IAPProduct>();
        private bool isInitialized = false;
        private bool isPurchasing = false;
        private string currentPurchaseId = null;
        private Queue<PendingPurchase> pendingValidations = new Queue<PendingPurchase>();
        #endregion

        #region Properties
        public bool IsInitialized => isInitialized;
        public bool IsPurchasing => isPurchasing;
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
            InitializeIAP();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// IAP 초기화
        /// </summary>
        public void InitializeIAP()
        {
#if UNITY_PURCHASING
            if (isInitialized)
            {
                Debug.Log("[IAPManager] Already initialized");
                return;
            }

            Debug.Log("[IAPManager] Initializing IAP...");

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // 상품 등록
            foreach (var config in productConfigs)
            {
                var productType = config.productType switch
                {
                    IAPProductType.Consumable => ProductType.Consumable,
                    IAPProductType.NonConsumable => ProductType.NonConsumable,
                    IAPProductType.Subscription => ProductType.Subscription,
                    _ => ProductType.Consumable
                };

                builder.AddProduct(config.productId, productType, new IDs
                {
                    { config.googlePlayId, GooglePlay.Name },
                    { config.appStoreId, AppleAppStore.Name }
                });

                products[config.productId] = new IAPProduct
                {
                    productId = config.productId,
                    productType = config.productType,
                    rewardType = config.rewardType,
                    rewardAmount = config.rewardAmount
                };
            }

            UnityPurchasing.Initialize(this, builder);
#else
            Debug.LogWarning("[IAPManager] Unity Purchasing not enabled. Add UNITY_PURCHASING to scripting define symbols.");
            InitializeTestProducts();
#endif
        }

        /// <summary>
        /// 테스트용 상품 초기화
        /// </summary>
        private void InitializeTestProducts()
        {
            foreach (var config in productConfigs)
            {
                products[config.productId] = new IAPProduct
                {
                    productId = config.productId,
                    productType = config.productType,
                    localizedTitle = config.displayName,
                    localizedDescription = config.description,
                    localizedPrice = config.testPrice,
                    rewardType = config.rewardType,
                    rewardAmount = config.rewardAmount
                };
            }
            isInitialized = true;
            OnStoreInitialized?.Invoke();
        }

#if UNITY_PURCHASING
        /// <summary>
        /// 스토어 초기화 성공
        /// </summary>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("[IAPManager] IAP Initialized successfully");

            storeController = controller;
            storeExtensionProvider = extensions;
            isInitialized = true;

            // 상품 정보 업데이트
            foreach (var product in controller.products.all)
            {
                if (products.ContainsKey(product.definition.id))
                {
                    products[product.definition.id].localizedTitle = product.metadata.localizedTitle;
                    products[product.definition.id].localizedDescription = product.metadata.localizedDescription;
                    products[product.definition.id].localizedPrice = product.metadata.localizedPriceString;
                    products[product.definition.id].price = (float)product.metadata.localizedPrice;
                    products[product.definition.id].currencyCode = product.metadata.isoCurrencyCode;
                }
            }

            OnStoreInitialized?.Invoke();

            // 미처리 구매 검증
            ProcessPendingValidations();
        }

        /// <summary>
        /// 스토어 초기화 실패
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"[IAPManager] IAP Initialization failed: {error}");
            OnStoreInitializeFailed?.Invoke(error.ToString());
        }

        /// <summary>
        /// 스토어 초기화 실패 (상세)
        /// </summary>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAPManager] IAP Initialization failed: {error} - {message}");
            OnStoreInitializeFailed?.Invoke($"{error}: {message}");
        }
#endif
        #endregion

        #region Purchase
        /// <summary>
        /// 구매 시작
        /// </summary>
        public void Purchase(string productId, Action<bool, PurchaseResult> callback = null)
        {
            if (!isInitialized)
            {
                Debug.LogError("[IAPManager] Store not initialized");
                callback?.Invoke(false, null);
                return;
            }

            if (isPurchasing)
            {
                Debug.LogWarning("[IAPManager] Purchase already in progress");
                callback?.Invoke(false, null);
                return;
            }

            if (!products.ContainsKey(productId))
            {
                Debug.LogError($"[IAPManager] Product not found: {productId}");
                callback?.Invoke(false, null);
                return;
            }

            isPurchasing = true;
            currentPurchaseId = productId;

            OnPurchaseStarted?.Invoke(products[productId]);

#if UNITY_PURCHASING
            var product = storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                Debug.Log($"[IAPManager] Purchasing: {productId}");
                storeController.InitiatePurchase(product);
            }
            else
            {
                Debug.LogError($"[IAPManager] Product not available: {productId}");
                isPurchasing = false;
                callback?.Invoke(false, null);
                OnPurchaseFailed?.Invoke(products[productId], "상품을 구매할 수 없습니다.");
            }
#else
            // 테스트 모드
            if (enableTestMode)
            {
                StartCoroutine(SimulatePurchase(productId, callback));
            }
            else
            {
                isPurchasing = false;
                callback?.Invoke(false, null);
            }
#endif
        }

        /// <summary>
        /// 테스트 구매 시뮬레이션
        /// </summary>
        private IEnumerator SimulatePurchase(string productId, Action<bool, PurchaseResult> callback)
        {
            Debug.Log($"[IAPManager] Simulating purchase: {productId}");
            yield return new WaitForSeconds(1f);

            var result = new PurchaseResult
            {
                productId = productId,
                transactionId = $"test_{DateTime.Now.Ticks}",
                receipt = "test_receipt",
                validated = true
            };

            isPurchasing = false;
            OnPurchaseCompleted?.Invoke(products[productId], result);
            callback?.Invoke(true, result);

            // 보상 지급
            GrantReward(products[productId]);
        }

#if UNITY_PURCHASING
        /// <summary>
        /// 구매 처리
        /// </summary>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;
            Debug.Log($"[IAPManager] Processing purchase: {productId}");

            if (enableServerValidation)
            {
                // 서버 검증 시작
                StartCoroutine(ValidatePurchaseWithServer(args));
                return PurchaseProcessingResult.Pending;
            }
            else
            {
                // 서버 검증 없이 바로 완료
                CompletePurchase(productId, args.purchasedProduct.transactionID, true);
                return PurchaseProcessingResult.Complete;
            }
        }

        /// <summary>
        /// 구매 실패
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"[IAPManager] Purchase failed: {product.definition.id} - {failureReason}");
            isPurchasing = false;

            string errorMessage = failureReason switch
            {
                PurchaseFailureReason.UserCancelled => "구매가 취소되었습니다.",
                PurchaseFailureReason.PaymentDeclined => "결제가 거부되었습니다.",
                PurchaseFailureReason.DuplicateTransaction => "중복 거래입니다.",
                PurchaseFailureReason.ProductUnavailable => "상품을 이용할 수 없습니다.",
                _ => "구매에 실패했습니다."
            };

            if (products.ContainsKey(product.definition.id))
            {
                OnPurchaseFailed?.Invoke(products[product.definition.id], errorMessage);
            }
        }

        /// <summary>
        /// 구매 실패 (상세)
        /// </summary>
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"[IAPManager] Purchase failed: {product.definition.id} - {failureDescription.message}");
            isPurchasing = false;

            if (products.ContainsKey(product.definition.id))
            {
                OnPurchaseFailed?.Invoke(products[product.definition.id], failureDescription.message);
            }
        }
#endif
        #endregion

        #region Server Validation
        /// <summary>
        /// 서버에서 구매 검증
        /// </summary>
#if UNITY_PURCHASING
        private IEnumerator ValidatePurchaseWithServer(PurchaseEventArgs args, int retryCount = 0)
        {
            string productId = args.purchasedProduct.definition.id;
            string receipt = args.purchasedProduct.receipt;
            string transactionId = args.purchasedProduct.transactionID;

            var validationRequest = new PurchaseValidationRequest
            {
                product_id = productId,
                transaction_id = transactionId,
                receipt = receipt,
                platform = Application.platform == RuntimePlatform.Android ? "google" : "apple",
                user_id = AuthManager.Instance?.CurrentUser?.user_id ?? ""
            };

            bool completed = false;
            bool validated = false;
            string errorMessage = null;

            NetworkManager.Instance?.Post("/iap/validate", validationRequest, (response) =>
            {
                if (response.success)
                {
                    var validationResponse = response.GetData<PurchaseValidationResponse>();
                    validated = validationResponse.is_valid;

                    if (!validated)
                    {
                        errorMessage = validationResponse.error_message ?? "결제 검증에 실패했습니다.";
                    }
                }
                else
                {
                    errorMessage = response.errorMessage;
                }
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (validated)
            {
                // 검증 성공
                storeController.ConfirmPendingPurchase(args.purchasedProduct);
                CompletePurchase(productId, transactionId, true);
            }
            else
            {
                // 검증 실패 - 재시도
                if (retryCount < maxValidationRetries && string.IsNullOrEmpty(errorMessage) == false &&
                    errorMessage.Contains("network"))
                {
                    yield return new WaitForSeconds(validationRetryDelay * Mathf.Pow(2, retryCount));
                    StartCoroutine(ValidatePurchaseWithServer(args, retryCount + 1));
                }
                else
                {
                    // 최종 실패 - pending으로 저장
                    SavePendingPurchase(args);
                    isPurchasing = false;

                    if (products.ContainsKey(productId))
                    {
                        OnPurchaseFailed?.Invoke(products[productId], errorMessage ?? "결제 검증에 실패했습니다.");
                    }
                }
            }
        }
#endif

        /// <summary>
        /// 미처리 구매 저장
        /// </summary>
#if UNITY_PURCHASING
        private void SavePendingPurchase(PurchaseEventArgs args)
        {
            var pending = new PendingPurchase
            {
                productId = args.purchasedProduct.definition.id,
                transactionId = args.purchasedProduct.transactionID,
                receipt = args.purchasedProduct.receipt,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // PlayerPrefs에 저장 (실제로는 암호화 필요)
            string key = $"pending_purchase_{pending.transactionId}";
            PlayerPrefs.SetString(key, JsonUtility.ToJson(pending));
            PlayerPrefs.Save();

            pendingValidations.Enqueue(pending);
        }
#endif

        /// <summary>
        /// 미처리 구매 검증 처리
        /// </summary>
        private void ProcessPendingValidations()
        {
            // 저장된 미처리 구매 로드
            // TODO: 모든 pending_purchase_ 키 로드

            while (pendingValidations.Count > 0)
            {
                var pending = pendingValidations.Dequeue();
                StartCoroutine(ValidatePendingPurchase(pending));
            }
        }

        /// <summary>
        /// 미처리 구매 검증
        /// </summary>
        private IEnumerator ValidatePendingPurchase(PendingPurchase pending)
        {
            var validationRequest = new PurchaseValidationRequest
            {
                product_id = pending.productId,
                transaction_id = pending.transactionId,
                receipt = pending.receipt,
                platform = Application.platform == RuntimePlatform.Android ? "google" : "apple",
                user_id = AuthManager.Instance?.CurrentUser?.user_id ?? ""
            };

            bool completed = false;
            bool validated = false;

            NetworkManager.Instance?.Post("/iap/validate", validationRequest, (response) =>
            {
                if (response.success)
                {
                    var validationResponse = response.GetData<PurchaseValidationResponse>();
                    validated = validationResponse.is_valid;
                }
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (validated)
            {
#if UNITY_PURCHASING
                // 구매 확정
                var product = storeController.products.WithID(pending.productId);
                if (product != null)
                {
                    storeController.ConfirmPendingPurchase(product);
                }
#endif
                CompletePurchase(pending.productId, pending.transactionId, true);

                // 미처리 구매 삭제
                string key = $"pending_purchase_{pending.transactionId}";
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// 구매 완료 처리
        /// </summary>
        private void CompletePurchase(string productId, string transactionId, bool validated)
        {
            isPurchasing = false;

            if (!products.ContainsKey(productId)) return;

            var product = products[productId];
            var result = new PurchaseResult
            {
                productId = productId,
                transactionId = transactionId,
                validated = validated
            };

            OnPurchaseCompleted?.Invoke(product, result);

            // 보상 지급
            if (validated)
            {
                GrantReward(product);
            }
        }
        #endregion

        #region Reward
        /// <summary>
        /// 보상 지급
        /// </summary>
        private void GrantReward(IAPProduct product)
        {
            Debug.Log($"[IAPManager] Granting reward: {product.rewardType} x{product.rewardAmount}");

            switch (product.rewardType)
            {
                case IAPRewardType.Diamond:
                    Managers.CurrencyManager.Instance?.AddCurrency(Managers.CurrencyType.Diamond, product.rewardAmount);
                    break;

                case IAPRewardType.Gold:
                    Managers.CurrencyManager.Instance?.AddCurrency(Managers.CurrencyType.Gold, product.rewardAmount);
                    break;

                case IAPRewardType.RemoveAds:
                    PlayerPrefs.SetInt("ads_removed", 1);
                    PlayerPrefs.Save();
                    break;

                case IAPRewardType.StarterPack:
                    // 스타터 팩 보상 지급
                    GrantStarterPackReward();
                    break;

                case IAPRewardType.VIP:
                    // VIP 활성화
                    PlayerPrefs.SetInt("is_vip", 1);
                    PlayerPrefs.Save();
                    break;

                case IAPRewardType.Custom:
                    // 커스텀 보상은 서버에서 처리
                    break;
            }
        }

        /// <summary>
        /// 스타터 팩 보상
        /// </summary>
        private void GrantStarterPackReward()
        {
            Managers.CurrencyManager.Instance?.AddCurrency(Managers.CurrencyType.Diamond, 500);
            Managers.CurrencyManager.Instance?.AddCurrency(Managers.CurrencyType.Gold, 100000);
            // 추가 보상 아이템 등
        }
        #endregion

        #region Restore Purchases
        /// <summary>
        /// 구매 복원 (iOS)
        /// </summary>
        public void RestorePurchases(Action<bool> callback = null)
        {
#if UNITY_PURCHASING && UNITY_IOS
            if (!isInitialized)
            {
                callback?.Invoke(false);
                return;
            }

            Debug.Log("[IAPManager] Restoring purchases...");

            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((success) =>
            {
                Debug.Log($"[IAPManager] Restore completed: {success}");
                callback?.Invoke(success);
            });
#else
            Debug.Log("[IAPManager] Restore not supported on this platform");
            callback?.Invoke(false);
#endif
        }
        #endregion

        #region Product Info
        /// <summary>
        /// 상품 정보 가져오기
        /// </summary>
        public IAPProduct GetProduct(string productId)
        {
            if (products.ContainsKey(productId))
            {
                return products[productId];
            }
            return null;
        }

        /// <summary>
        /// 모든 상품 목록
        /// </summary>
        public List<IAPProduct> GetAllProducts()
        {
            return new List<IAPProduct>(products.Values);
        }

        /// <summary>
        /// 상품 타입별 목록
        /// </summary>
        public List<IAPProduct> GetProductsByType(IAPProductType type)
        {
            List<IAPProduct> result = new List<IAPProduct>();
            foreach (var product in products.Values)
            {
                if (product.productType == type)
                {
                    result.Add(product);
                }
            }
            return result;
        }
        #endregion

        #region Purchase History
        /// <summary>
        /// 구매 내역 가져오기
        /// </summary>
        public void GetPurchaseHistory(Action<List<PurchaseHistoryItem>> callback)
        {
            NetworkManager.Instance?.Get("/iap/history", (response) =>
            {
                if (response.success)
                {
                    var historyResponse = response.GetData<PurchaseHistoryResponse>();
                    callback?.Invoke(historyResponse?.items);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// 상품 타입
    /// </summary>
    public enum IAPProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }

    /// <summary>
    /// 보상 타입
    /// </summary>
    public enum IAPRewardType
    {
        Diamond,
        Gold,
        RemoveAds,
        StarterPack,
        VIP,
        Custom
    }
    #endregion

    #region Data Classes
    [Serializable]
    public class IAPProductConfig
    {
        public string productId;
        public string googlePlayId;
        public string appStoreId;
        public IAPProductType productType;
        public string displayName;
        public string description;
        public string testPrice;
        public IAPRewardType rewardType;
        public int rewardAmount;
    }

    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public IAPProductType productType;
        public string localizedTitle;
        public string localizedDescription;
        public string localizedPrice;
        public float price;
        public string currencyCode;
        public IAPRewardType rewardType;
        public int rewardAmount;
    }

    [Serializable]
    public class PurchaseResult
    {
        public string productId;
        public string transactionId;
        public string receipt;
        public bool validated;
    }

    [Serializable]
    public class PendingPurchase
    {
        public string productId;
        public string transactionId;
        public string receipt;
        public string timestamp;
    }

    [Serializable]
    public class PurchaseValidationRequest
    {
        public string product_id;
        public string transaction_id;
        public string receipt;
        public string platform;
        public string user_id;
    }

    [Serializable]
    public class PurchaseValidationResponse
    {
        public bool is_valid;
        public string error_code;
        public string error_message;
        public string order_id;
    }

    [Serializable]
    public class PurchaseHistoryItem
    {
        public string product_id;
        public string transaction_id;
        public string purchase_time;
        public float amount;
        public string currency;
        public string status;
    }

    [Serializable]
    public class PurchaseHistoryResponse
    {
        public List<PurchaseHistoryItem> items;
    }
    #endregion
}
