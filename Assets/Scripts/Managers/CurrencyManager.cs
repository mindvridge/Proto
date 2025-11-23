using System;
using System.Collections.Generic;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 재화 매니저 - 모든 게임 재화 관리
    /// BigNumber를 사용하여 무제한 큰 숫자 지원
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        #region Events
        public event Action<CurrencyType, BigNumber, BigNumber> OnCurrencyChanged;  // type, oldValue, newValue
        public event Action<CurrencyType, BigNumber> OnCurrencyEarned;               // type, amount
        public event Action<CurrencyType, BigNumber> OnCurrencySpent;                // type, amount
        public event Action<CurrencyType> OnCurrencyInsufficient;                    // type
        #endregion

        #region Settings
        [Header("=== Format Settings ===")]
        [SerializeField] private CurrencyFormatType defaultFormatType = CurrencyFormatType.KoreanSimple;
        [SerializeField] private int defaultDecimalPlaces = 2;

        [Header("=== Multiplier Settings ===")]
        [SerializeField] private float globalGoldMultiplier = 1f;
        [SerializeField] private float globalExpMultiplier = 1f;
        #endregion

        #region State
        private Dictionary<CurrencyType, BigNumber> currencies;
        private Dictionary<CurrencyType, BigNumber> totalEarned;
        private Dictionary<CurrencyType, BigNumber> totalSpent;
        private Dictionary<CurrencyType, float> multipliers;
        private bool isInitialized = false;
        #endregion

        #region Properties
        /// <summary>
        /// 골드
        /// </summary>
        public BigNumber Gold
        {
            get => GetCurrency(CurrencyType.Gold);
            set => SetCurrency(CurrencyType.Gold, value);
        }

        /// <summary>
        /// 다이아몬드/젬
        /// </summary>
        public BigNumber Diamond
        {
            get => GetCurrency(CurrencyType.Diamond);
            set => SetCurrency(CurrencyType.Diamond, value);
        }

        /// <summary>
        /// 경험치
        /// </summary>
        public BigNumber Exp
        {
            get => GetCurrency(CurrencyType.Exp);
            set => SetCurrency(CurrencyType.Exp, value);
        }

        /// <summary>
        /// 스킬 포인트
        /// </summary>
        public BigNumber SkillPoints
        {
            get => GetCurrency(CurrencyType.SkillPoint);
            set => SetCurrency(CurrencyType.SkillPoint, value);
        }

        /// <summary>
        /// 환생 포인트
        /// </summary>
        public BigNumber RebirthPoints
        {
            get => GetCurrency(CurrencyType.RebirthPoint);
            set => SetCurrency(CurrencyType.RebirthPoint, value);
        }

        /// <summary>
        /// 이벤트 포인트
        /// </summary>
        public BigNumber EventPoints
        {
            get => GetCurrency(CurrencyType.EventPoint);
            set => SetCurrency(CurrencyType.EventPoint, value);
        }

        public bool IsInitialized => isInitialized;
        public CurrencyFormatType DefaultFormatType => defaultFormatType;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            currencies = new Dictionary<CurrencyType, BigNumber>();
            totalEarned = new Dictionary<CurrencyType, BigNumber>();
            totalSpent = new Dictionary<CurrencyType, BigNumber>();
            multipliers = new Dictionary<CurrencyType, float>();

            // 모든 재화 타입 초기화
            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                currencies[type] = BigNumber.Zero;
                totalEarned[type] = BigNumber.Zero;
                totalSpent[type] = BigNumber.Zero;
                multipliers[type] = 1f;
            }

            isInitialized = true;
            Debug.Log("[CurrencyManager] Initialized");
        }
        #endregion

        #region Get/Set Currency
        /// <summary>
        /// 재화 가져오기
        /// </summary>
        public BigNumber GetCurrency(CurrencyType type)
        {
            if (!isInitialized) Initialize();

            if (currencies.TryGetValue(type, out BigNumber value))
                return value;
            return BigNumber.Zero;
        }

        /// <summary>
        /// 재화 설정
        /// </summary>
        public void SetCurrency(CurrencyType type, BigNumber value)
        {
            if (!isInitialized) Initialize();

            BigNumber oldValue = GetCurrency(type);
            currencies[type] = BigNumber.Max(BigNumber.Zero, value);

            if (currencies[type] != oldValue)
            {
                OnCurrencyChanged?.Invoke(type, oldValue, currencies[type]);
            }
        }
        #endregion

        #region Add/Spend Currency
        /// <summary>
        /// 재화 추가
        /// </summary>
        public void AddCurrency(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return;

            // 배율 적용
            BigNumber finalAmount = amount * GetMultiplier(type);

            BigNumber oldValue = GetCurrency(type);
            currencies[type] = oldValue + finalAmount;
            totalEarned[type] = totalEarned[type] + finalAmount;

            OnCurrencyChanged?.Invoke(type, oldValue, currencies[type]);
            OnCurrencyEarned?.Invoke(type, finalAmount);

            Debug.Log($"[CurrencyManager] Added {FormatCurrency(finalAmount, type)} {type}");
        }

        /// <summary>
        /// 재화 소비
        /// </summary>
        public bool SpendCurrency(CurrencyType type, BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return true;

            BigNumber current = GetCurrency(type);

            if (current < amount)
            {
                OnCurrencyInsufficient?.Invoke(type);
                Debug.LogWarning($"[CurrencyManager] Insufficient {type}: have {FormatCurrency(current, type)}, need {FormatCurrency(amount, type)}");
                return false;
            }

            BigNumber oldValue = current;
            currencies[type] = current - amount;
            totalSpent[type] = totalSpent[type] + amount;

            OnCurrencyChanged?.Invoke(type, oldValue, currencies[type]);
            OnCurrencySpent?.Invoke(type, amount);

            Debug.Log($"[CurrencyManager] Spent {FormatCurrency(amount, type)} {type}");
            return true;
        }

        /// <summary>
        /// 재화 충분한지 확인
        /// </summary>
        public bool HasEnough(CurrencyType type, BigNumber amount)
        {
            return GetCurrency(type) >= amount;
        }

        /// <summary>
        /// 여러 재화 동시 확인
        /// </summary>
        public bool HasEnough(params CurrencyCost[] costs)
        {
            foreach (var cost in costs)
            {
                if (!HasEnough(cost.type, cost.amount))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 여러 재화 동시 소비
        /// </summary>
        public bool SpendMultiple(params CurrencyCost[] costs)
        {
            // 먼저 모두 확인
            if (!HasEnough(costs))
                return false;

            // 모두 소비
            foreach (var cost in costs)
            {
                SpendCurrency(cost.type, cost.amount);
            }
            return true;
        }
        #endregion

        #region Multipliers
        /// <summary>
        /// 배율 가져오기
        /// </summary>
        public float GetMultiplier(CurrencyType type)
        {
            if (!multipliers.TryGetValue(type, out float mult))
                return 1f;

            // 글로벌 배율 적용
            return type switch
            {
                CurrencyType.Gold => mult * globalGoldMultiplier,
                CurrencyType.Exp => mult * globalExpMultiplier,
                _ => mult
            };
        }

        /// <summary>
        /// 배율 설정
        /// </summary>
        public void SetMultiplier(CurrencyType type, float multiplier)
        {
            multipliers[type] = Mathf.Max(0, multiplier);
        }

        /// <summary>
        /// 배율 추가
        /// </summary>
        public void AddMultiplier(CurrencyType type, float additionalMultiplier)
        {
            if (!multipliers.ContainsKey(type))
                multipliers[type] = 1f;

            multipliers[type] += additionalMultiplier;
        }

        /// <summary>
        /// 글로벌 골드 배율 설정
        /// </summary>
        public void SetGlobalGoldMultiplier(float multiplier)
        {
            globalGoldMultiplier = Mathf.Max(0, multiplier);
        }

        /// <summary>
        /// 글로벌 경험치 배율 설정
        /// </summary>
        public void SetGlobalExpMultiplier(float multiplier)
        {
            globalExpMultiplier = Mathf.Max(0, multiplier);
        }
        #endregion

        #region Statistics
        /// <summary>
        /// 총 획득량 가져오기
        /// </summary>
        public BigNumber GetTotalEarned(CurrencyType type)
        {
            if (totalEarned.TryGetValue(type, out BigNumber value))
                return value;
            return BigNumber.Zero;
        }

        /// <summary>
        /// 총 소비량 가져오기
        /// </summary>
        public BigNumber GetTotalSpent(CurrencyType type)
        {
            if (totalSpent.TryGetValue(type, out BigNumber value))
                return value;
            return BigNumber.Zero;
        }

        /// <summary>
        /// 순수익 가져오기
        /// </summary>
        public BigNumber GetNetEarnings(CurrencyType type)
        {
            return GetTotalEarned(type) - GetTotalSpent(type);
        }
        #endregion

        #region Formatting
        /// <summary>
        /// 재화 포맷팅
        /// </summary>
        public string FormatCurrency(BigNumber amount, CurrencyType type = CurrencyType.Gold)
        {
            return CurrencyFormatter.Format(amount, defaultFormatType, defaultDecimalPlaces);
        }

        /// <summary>
        /// 재화 포맷팅 (지정 포맷)
        /// </summary>
        public string FormatCurrency(BigNumber amount, CurrencyFormatType formatType)
        {
            return CurrencyFormatter.Format(amount, formatType, defaultDecimalPlaces);
        }

        /// <summary>
        /// 현재 재화 포맷팅된 문자열 가져오기
        /// </summary>
        public string GetFormattedCurrency(CurrencyType type)
        {
            return FormatCurrency(GetCurrency(type), type);
        }

        /// <summary>
        /// 재화 아이콘 문자열 가져오기
        /// </summary>
        public string GetCurrencyIcon(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Gold => "<sprite name=\"icon_gold\">",
                CurrencyType.Diamond => "<sprite name=\"icon_diamond\">",
                CurrencyType.Exp => "<sprite name=\"icon_exp\">",
                CurrencyType.SkillPoint => "<sprite name=\"icon_skill\">",
                CurrencyType.RebirthPoint => "<sprite name=\"icon_rebirth\">",
                CurrencyType.EventPoint => "<sprite name=\"icon_event\">",
                CurrencyType.DungeonKey => "<sprite name=\"icon_key\">",
                CurrencyType.ArenaTicket => "<sprite name=\"icon_ticket\">",
                _ => ""
            };
        }

        /// <summary>
        /// 재화 이름 가져오기
        /// </summary>
        public string GetCurrencyName(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Gold => "골드",
                CurrencyType.Diamond => "다이아",
                CurrencyType.Exp => "경험치",
                CurrencyType.SkillPoint => "스킬 포인트",
                CurrencyType.RebirthPoint => "환생 포인트",
                CurrencyType.EventPoint => "이벤트 포인트",
                CurrencyType.DungeonKey => "던전 열쇠",
                CurrencyType.ArenaTicket => "아레나 티켓",
                CurrencyType.SoulStone => "영혼석",
                CurrencyType.EnhanceStone => "강화석",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// 전체 재화 상태 문자열
        /// </summary>
        public string GetAllCurrenciesString()
        {
            var sb = new System.Text.StringBuilder();

            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                BigNumber value = GetCurrency(type);
                if (value > BigNumber.Zero)
                {
                    sb.AppendLine($"{GetCurrencyName(type)}: {FormatCurrency(value, type)}");
                }
            }

            return sb.ToString();
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// 저장 데이터 생성
        /// </summary>
        public CurrencySaveData ToSaveData()
        {
            var saveData = new CurrencySaveData
            {
                currencies = new List<CurrencySaveEntry>(),
                totalEarned = new List<CurrencySaveEntry>(),
                totalSpent = new List<CurrencySaveEntry>()
            };

            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                saveData.currencies.Add(new CurrencySaveEntry
                {
                    type = type.ToString(),
                    value = GetCurrency(type).Serialize()
                });

                saveData.totalEarned.Add(new CurrencySaveEntry
                {
                    type = type.ToString(),
                    value = GetTotalEarned(type).Serialize()
                });

                saveData.totalSpent.Add(new CurrencySaveEntry
                {
                    type = type.ToString(),
                    value = GetTotalSpent(type).Serialize()
                });
            }

            return saveData;
        }

        /// <summary>
        /// 저장 데이터 로드
        /// </summary>
        public void LoadFromSaveData(CurrencySaveData saveData)
        {
            if (saveData == null) return;

            if (saveData.currencies != null)
            {
                foreach (var entry in saveData.currencies)
                {
                    if (Enum.TryParse<CurrencyType>(entry.type, out CurrencyType type))
                    {
                        currencies[type] = BigNumber.Deserialize(entry.value);
                    }
                }
            }

            if (saveData.totalEarned != null)
            {
                foreach (var entry in saveData.totalEarned)
                {
                    if (Enum.TryParse<CurrencyType>(entry.type, out CurrencyType type))
                    {
                        totalEarned[type] = BigNumber.Deserialize(entry.value);
                    }
                }
            }

            if (saveData.totalSpent != null)
            {
                foreach (var entry in saveData.totalSpent)
                {
                    if (Enum.TryParse<CurrencyType>(entry.type, out CurrencyType type))
                    {
                        totalSpent[type] = BigNumber.Deserialize(entry.value);
                    }
                }
            }

            Debug.Log("[CurrencyManager] Loaded save data");
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Debug: Add 1억 Gold")]
        private void DebugAddGold()
        {
            AddCurrency(CurrencyType.Gold, new BigNumber(1, 8)); // 1억
        }

        [ContextMenu("Debug: Add 1조 Gold")]
        private void DebugAddTrillionGold()
        {
            AddCurrency(CurrencyType.Gold, new BigNumber(1, 12)); // 1조
        }

        [ContextMenu("Debug: Add 1무량대수 Gold")]
        private void DebugAddHugeGold()
        {
            AddCurrency(CurrencyType.Gold, new BigNumber(1, 68)); // 1무량대수
        }

        [ContextMenu("Debug: Print All Currencies")]
        private void DebugPrintAll()
        {
            Debug.Log(GetAllCurrenciesString());
        }

        [ContextMenu("Debug: Test Format")]
        private void DebugTestFormat()
        {
            BigNumber[] testValues = new BigNumber[]
            {
                new BigNumber(1234),
                new BigNumber(12345),
                new BigNumber(123456789),
                new BigNumber(1, 12),  // 1조
                new BigNumber(1.5, 16), // 1.5경
                new BigNumber(1, 68),  // 1무량대수
                new BigNumber(1, 120), // 1무량무진
            };

            foreach (var value in testValues)
            {
                Debug.Log($"Value: {value}");
                Debug.Log($"  Korean: {CurrencyFormatter.FormatKorean(value)}");
                Debug.Log($"  KoreanSimple: {CurrencyFormatter.FormatKoreanSimple(value)}");
                Debug.Log($"  Short: {CurrencyFormatter.FormatShort(value)}");
                Debug.Log($"  Scientific: {CurrencyFormatter.FormatScientific(value)}");
            }
        }
#endif
        #endregion
    }

    #region Data Classes
    /// <summary>
    /// 재화 타입
    /// </summary>
    public enum CurrencyType
    {
        Gold,           // 골드 (기본 재화)
        Diamond,        // 다이아몬드/젬 (프리미엄 재화)
        Exp,            // 경험치
        SkillPoint,     // 스킬 포인트
        RebirthPoint,   // 환생 포인트
        EventPoint,     // 이벤트 포인트
        DungeonKey,     // 던전 입장권
        ArenaTicket,    // 아레나 티켓
        SoulStone,      // 영혼석
        EnhanceStone,   // 강화석
        CustomCurrency1,
        CustomCurrency2,
        CustomCurrency3
    }

    /// <summary>
    /// 재화 비용
    /// </summary>
    [Serializable]
    public struct CurrencyCost
    {
        public CurrencyType type;
        public BigNumber amount;

        public CurrencyCost(CurrencyType type, BigNumber amount)
        {
            this.type = type;
            this.amount = amount;
        }

        public CurrencyCost(CurrencyType type, long amount)
        {
            this.type = type;
            this.amount = new BigNumber(amount);
        }
    }

    /// <summary>
    /// 재화 저장 데이터
    /// </summary>
    [Serializable]
    public class CurrencySaveData
    {
        public List<CurrencySaveEntry> currencies;
        public List<CurrencySaveEntry> totalEarned;
        public List<CurrencySaveEntry> totalSpent;
    }

    /// <summary>
    /// 재화 저장 항목
    /// </summary>
    [Serializable]
    public class CurrencySaveEntry
    {
        public string type;
        public string value;
    }
    #endregion
}
