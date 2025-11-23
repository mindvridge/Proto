using System;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// 플레이어 스탯 관리 클래스
    /// BigNumber를 사용하여 무제한 큰 숫자 지원
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        #region Events
        public event Action<int, int> OnLevelUp;                         // oldLevel, newLevel
        public event Action<BigNumber, BigNumber> OnExpChanged;          // currentExp, requiredExp
        public event Action<BigNumber> OnGoldChanged;                    // newGold
        public event Action<BigNumber, BigNumber> OnHealthChanged;       // currentHP, maxHP
        public event Action<BigNumber, BigNumber> OnAttackChanged;       // oldAttack, newAttack
        public event Action OnStatsChanged;
        public event Action<int> OnRebirthCountChanged;                  // rebirthCount
        #endregion

        #region Base Stats (BigNumber 사용)
        [SerializeField] private int level = 1;
        [SerializeField] private string currentExpSerialized = "0:0";
        [SerializeField] private string goldSerialized = "0:0";
        [SerializeField] private string currentHPSerialized = "0:0";
        [SerializeField] private int rebirthCount = 0;
        [SerializeField] private int skillPoints = 0;

        // Runtime BigNumber values
        private BigNumber _currentExp = BigNumber.Zero;
        private BigNumber _gold = BigNumber.Zero;
        private BigNumber _currentHP = BigNumber.Zero;
        #endregion

        #region Stat Points (레벨업 시 분배)
        [SerializeField] private int statPointsAvailable = 0;
        [SerializeField] private int strengthPoints = 0;        // 공격력
        [SerializeField] private int vitalityPoints = 0;        // 체력
        [SerializeField] private int agilityPoints = 0;         // 치명타
        [SerializeField] private int luckPoints = 0;            // 드롭률
        #endregion

        #region Constants
        private static readonly BigNumber BASE_ATTACK = new BigNumber(10);
        private static readonly BigNumber BASE_MAX_HP = new BigNumber(100);
        private const float ATTACK_PER_STRENGTH = 5f;
        private const float HP_PER_VITALITY = 20f;
        private const float CRIT_RATE_PER_AGILITY = 0.5f;       // 0.5% per point
        private const float DROP_RATE_PER_LUCK = 0.3f;          // 0.3% per point
        private const float LEVEL_ATTACK_MULTIPLIER = 1.5f;
        private const float LEVEL_HP_MULTIPLIER = 10f;
        private const float REBIRTH_BONUS_MULTIPLIER = 0.1f;    // 10% per rebirth
        private const int STAT_POINTS_PER_LEVEL = 3;
        private const int SKILL_POINTS_PER_LEVEL = 1;
        #endregion

        #region Properties
        public int Level => level;

        public BigNumber CurrentExp
        {
            get => _currentExp;
            private set
            {
                _currentExp = value;
                currentExpSerialized = value.Serialize();
            }
        }

        public BigNumber Gold
        {
            get => _gold;
            private set
            {
                _gold = value;
                goldSerialized = value.Serialize();
            }
        }

        public BigNumber CurrentHP
        {
            get => _currentHP;
            private set
            {
                _currentHP = value;
                currentHPSerialized = value.Serialize();
            }
        }

        public int RebirthCount => rebirthCount;
        public int SkillPoints => skillPoints;
        public int StatPointsAvailable => statPointsAvailable;
        public int StrengthPoints => strengthPoints;
        public int VitalityPoints => vitalityPoints;
        public int AgilityPoints => agilityPoints;
        public int LuckPoints => luckPoints;

        /// <summary>
        /// 다음 레벨에 필요한 경험치
        /// </summary>
        public BigNumber RequiredExp => CalculateRequiredExp(level);

        /// <summary>
        /// 최대 체력
        /// </summary>
        public BigNumber MaxHP => CalculateMaxHP();

        /// <summary>
        /// 공격력
        /// </summary>
        public BigNumber Attack => CalculateAttack();

        /// <summary>
        /// 치명타 확률 (0~1)
        /// </summary>
        public float CriticalRate => CalculateCriticalRate();

        /// <summary>
        /// 치명타 데미지 배율
        /// </summary>
        public float CriticalDamageMultiplier => 2.0f + (rebirthCount * 0.1f);

        /// <summary>
        /// 드롭률 보너스 (0~1)
        /// </summary>
        public float DropRateBonus => CalculateDropRateBonus();

        /// <summary>
        /// 골드 획득 보너스 (1.0 = 100%)
        /// </summary>
        public float GoldBonus => 1.0f + (rebirthCount * REBIRTH_BONUS_MULTIPLIER);

        /// <summary>
        /// 경험치 획득 보너스 (1.0 = 100%)
        /// </summary>
        public float ExpBonus => 1.0f + (rebirthCount * REBIRTH_BONUS_MULTIPLIER * 0.5f);

        /// <summary>
        /// HP 퍼센트
        /// </summary>
        public float HPPercent
        {
            get
            {
                if (MaxHP.IsZero) return 0;
                return (CurrentHP / MaxHP).ToFloat();
            }
        }

        /// <summary>
        /// 경험치 퍼센트
        /// </summary>
        public float ExpPercent
        {
            get
            {
                if (RequiredExp.IsZero) return 0;
                return (CurrentExp / RequiredExp).ToFloat();
            }
        }

        /// <summary>
        /// 포맷된 골드 문자열
        /// </summary>
        public string GoldFormatted => CurrencyFormatter.FormatKoreanSimple(Gold);

        /// <summary>
        /// 포맷된 공격력 문자열
        /// </summary>
        public string AttackFormatted => CurrencyFormatter.FormatKoreanSimple(Attack);

        /// <summary>
        /// 포맷된 HP 문자열
        /// </summary>
        public string HPFormatted => $"{CurrencyFormatter.FormatKoreanSimple(CurrentHP)}/{CurrencyFormatter.FormatKoreanSimple(MaxHP)}";
        #endregion

        #region Initialization
        public PlayerStats()
        {
            Initialize();
        }

        public void Initialize()
        {
            level = 1;
            CurrentExp = BigNumber.Zero;
            Gold = BigNumber.Zero;
            rebirthCount = 0;
            skillPoints = 0;
            statPointsAvailable = 0;
            strengthPoints = 0;
            vitalityPoints = 0;
            agilityPoints = 0;
            luckPoints = 0;
            CurrentHP = MaxHP;
        }

        /// <summary>
        /// 저장된 데이터로 초기화
        /// </summary>
        public void LoadFromSaveData(PlayerSaveData saveData)
        {
            if (saveData == null) return;

            level = saveData.level;
            CurrentExp = BigNumber.Deserialize(saveData.currentExpSerialized);
            Gold = BigNumber.Deserialize(saveData.goldSerialized);
            rebirthCount = saveData.rebirthCount;
            skillPoints = saveData.skillPoints;
            statPointsAvailable = saveData.statPointsAvailable;
            strengthPoints = saveData.strengthPoints;
            vitalityPoints = saveData.vitalityPoints;
            agilityPoints = saveData.agilityPoints;
            luckPoints = saveData.luckPoints;

            BigNumber loadedHP = BigNumber.Deserialize(saveData.currentHPSerialized);
            CurrentHP = loadedHP > BigNumber.Zero ? loadedHP : MaxHP;

            OnStatsChanged?.Invoke();
        }

        /// <summary>
        /// 저장용 데이터 생성
        /// </summary>
        public PlayerSaveData ToSaveData()
        {
            return new PlayerSaveData
            {
                level = this.level,
                currentExpSerialized = this.CurrentExp.Serialize(),
                goldSerialized = this.Gold.Serialize(),
                currentHPSerialized = this.CurrentHP.Serialize(),
                rebirthCount = this.rebirthCount,
                skillPoints = this.skillPoints,
                statPointsAvailable = this.statPointsAvailable,
                strengthPoints = this.strengthPoints,
                vitalityPoints = this.vitalityPoints,
                agilityPoints = this.agilityPoints,
                luckPoints = this.luckPoints
            };
        }

        /// <summary>
        /// 직렬화된 데이터에서 BigNumber 복원
        /// </summary>
        public void DeserializeBigNumbers()
        {
            _currentExp = BigNumber.Deserialize(currentExpSerialized);
            _gold = BigNumber.Deserialize(goldSerialized);
            _currentHP = BigNumber.Deserialize(currentHPSerialized);
        }
        #endregion

        #region Stat Calculations
        /// <summary>
        /// 필요 경험치 계산 (레벨 기반 곡선)
        /// </summary>
        private BigNumber CalculateRequiredExp(int targetLevel)
        {
            // 기본 공식: 100 * level^2 + 50 * level
            // 고레벨에서 급격히 증가
            double baseExp = 100 * Math.Pow(targetLevel, 2) + 50 * targetLevel;

            // 레벨 100 이후 추가 스케일링
            if (targetLevel > 100)
            {
                baseExp *= Math.Pow(1.1, targetLevel - 100);
            }

            return new BigNumber(baseExp);
        }

        /// <summary>
        /// 최대 체력 계산
        /// </summary>
        private BigNumber CalculateMaxHP()
        {
            BigNumber baseHP = BASE_MAX_HP;
            BigNumber levelBonus = new BigNumber(level * LEVEL_HP_MULTIPLIER);
            BigNumber vitalityBonus = new BigNumber(vitalityPoints * HP_PER_VITALITY);
            float rebirthBonus = 1 + (rebirthCount * REBIRTH_BONUS_MULTIPLIER);

            // 환생 보너스로 지수적 증가
            if (rebirthCount > 0)
            {
                rebirthBonus *= (float)Math.Pow(1.5, rebirthCount);
            }

            return (baseHP + levelBonus + vitalityBonus) * rebirthBonus;
        }

        /// <summary>
        /// 공격력 계산
        /// </summary>
        private BigNumber CalculateAttack()
        {
            BigNumber baseAtk = BASE_ATTACK;
            BigNumber levelBonus = new BigNumber(level * LEVEL_ATTACK_MULTIPLIER);
            BigNumber strengthBonus = new BigNumber(strengthPoints * ATTACK_PER_STRENGTH);
            float rebirthBonus = 1 + (rebirthCount * REBIRTH_BONUS_MULTIPLIER);

            // 환생 보너스로 지수적 증가
            if (rebirthCount > 0)
            {
                rebirthBonus *= (float)Math.Pow(2.0, rebirthCount);
            }

            return (baseAtk + levelBonus + strengthBonus) * rebirthBonus;
        }

        /// <summary>
        /// 치명타 확률 계산
        /// </summary>
        private float CalculateCriticalRate()
        {
            float baseRate = 0.05f;  // 5% 기본
            float agilityBonus = agilityPoints * CRIT_RATE_PER_AGILITY / 100f;
            return Mathf.Min(baseRate + agilityBonus, 0.8f);  // 최대 80%
        }

        /// <summary>
        /// 드롭률 보너스 계산
        /// </summary>
        private float CalculateDropRateBonus()
        {
            float luckBonus = luckPoints * DROP_RATE_PER_LUCK / 100f;
            float rebirthBonus = rebirthCount * 0.05f;
            return luckBonus + rebirthBonus;
        }
        #endregion

        #region Experience & Level
        /// <summary>
        /// 경험치 획득
        /// </summary>
        public void AddExp(BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return;

            BigNumber bonusExp = amount * ExpBonus;
            CurrentExp = CurrentExp + bonusExp;

            OnExpChanged?.Invoke(CurrentExp, RequiredExp);

            // 레벨업 체크
            while (CurrentExp >= RequiredExp)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 경험치 획득 (long 오버로드)
        /// </summary>
        public void AddExp(long amount)
        {
            AddExp(new BigNumber(amount));
        }

        /// <summary>
        /// 레벨업 처리
        /// </summary>
        private void LevelUp()
        {
            int oldLevel = level;
            CurrentExp = CurrentExp - RequiredExp;
            level++;

            // 스탯 포인트 및 스킬 포인트 지급
            statPointsAvailable += STAT_POINTS_PER_LEVEL;
            skillPoints += SKILL_POINTS_PER_LEVEL;

            // HP 회복
            CurrentHP = MaxHP;

            OnLevelUp?.Invoke(oldLevel, level);
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);

            Debug.Log($"[PlayerStats] Level Up! {oldLevel} -> {level}");
        }

        /// <summary>
        /// 강제 레벨 설정 (디버그/치트용)
        /// </summary>
        public void SetLevel(int newLevel)
        {
            if (newLevel < 1) return;

            int oldLevel = level;
            level = newLevel;
            CurrentExp = BigNumber.Zero;
            CurrentHP = MaxHP;

            OnLevelUp?.Invoke(oldLevel, level);
            OnStatsChanged?.Invoke();
        }
        #endregion

        #region Gold
        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return;

            BigNumber bonusGold = amount * GoldBonus;
            Gold = Gold + bonusGold;

            OnGoldChanged?.Invoke(Gold);
        }

        /// <summary>
        /// 골드 획득 (long 오버로드)
        /// </summary>
        public void AddGold(long amount)
        {
            AddGold(new BigNumber(amount));
        }

        /// <summary>
        /// 골드 소비
        /// </summary>
        public bool SpendGold(BigNumber amount)
        {
            if (amount <= BigNumber.Zero || Gold < amount) return false;

            Gold = Gold - amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        /// <summary>
        /// 골드 소비 (long 오버로드)
        /// </summary>
        public bool SpendGold(long amount)
        {
            return SpendGold(new BigNumber(amount));
        }

        /// <summary>
        /// 골드 충분한지 확인
        /// </summary>
        public bool HasEnoughGold(BigNumber amount)
        {
            return Gold >= amount;
        }

        /// <summary>
        /// 골드 충분한지 확인 (long 오버로드)
        /// </summary>
        public bool HasEnoughGold(long amount)
        {
            return HasEnoughGold(new BigNumber(amount));
        }
        #endregion

        #region Health
        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(BigNumber damage)
        {
            if (damage <= BigNumber.Zero) return;

            CurrentHP = BigNumber.Max(BigNumber.Zero, CurrentHP - damage);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);

            if (CurrentHP.IsZero)
            {
                Debug.Log("[PlayerStats] Player died!");
            }
        }

        /// <summary>
        /// 데미지 받기 (long 오버로드)
        /// </summary>
        public void TakeDamage(long damage)
        {
            TakeDamage(new BigNumber(damage));
        }

        /// <summary>
        /// HP 회복
        /// </summary>
        public void Heal(BigNumber amount)
        {
            if (amount <= BigNumber.Zero) return;

            CurrentHP = BigNumber.Min(MaxHP, CurrentHP + amount);
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        /// <summary>
        /// HP 회복 (long 오버로드)
        /// </summary>
        public void Heal(long amount)
        {
            Heal(new BigNumber(amount));
        }

        /// <summary>
        /// HP 퍼센트로 회복
        /// </summary>
        public void HealPercent(float percent)
        {
            BigNumber healAmount = MaxHP * percent;
            Heal(healAmount);
        }

        /// <summary>
        /// HP 전체 회복
        /// </summary>
        public void FullHeal()
        {
            CurrentHP = MaxHP;
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        /// <summary>
        /// 살아있는지 확인
        /// </summary>
        public bool IsAlive => CurrentHP > BigNumber.Zero;
        #endregion

        #region Stat Points
        /// <summary>
        /// 스탯 포인트 분배
        /// </summary>
        public bool AllocateStatPoint(StatType statType, int amount = 1)
        {
            if (amount <= 0 || statPointsAvailable < amount) return false;

            BigNumber oldAttack = Attack;

            switch (statType)
            {
                case StatType.Strength:
                    strengthPoints += amount;
                    break;
                case StatType.Vitality:
                    vitalityPoints += amount;
                    break;
                case StatType.Agility:
                    agilityPoints += amount;
                    break;
                case StatType.Luck:
                    luckPoints += amount;
                    break;
                default:
                    return false;
            }

            statPointsAvailable -= amount;
            OnStatsChanged?.Invoke();

            if (statType == StatType.Strength)
            {
                OnAttackChanged?.Invoke(oldAttack, Attack);
            }

            return true;
        }

        /// <summary>
        /// 스탯 포인트 초기화
        /// </summary>
        public void ResetStatPoints()
        {
            int totalPoints = strengthPoints + vitalityPoints + agilityPoints + luckPoints;
            statPointsAvailable += totalPoints;

            strengthPoints = 0;
            vitalityPoints = 0;
            agilityPoints = 0;
            luckPoints = 0;

            OnStatsChanged?.Invoke();
        }
        #endregion

        #region Skill Points
        /// <summary>
        /// 스킬 포인트 소비
        /// </summary>
        public bool SpendSkillPoints(int amount)
        {
            if (amount <= 0 || skillPoints < amount) return false;

            skillPoints -= amount;
            OnStatsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 스킬 포인트 추가
        /// </summary>
        public void AddSkillPoints(int amount)
        {
            if (amount <= 0) return;

            skillPoints += amount;
            OnStatsChanged?.Invoke();
        }
        #endregion

        #region Rebirth
        /// <summary>
        /// 환생 처리
        /// </summary>
        public bool Rebirth(int requiredLevel = 100)
        {
            if (level < requiredLevel) return false;

            rebirthCount++;
            level = 1;
            CurrentExp = BigNumber.Zero;

            // 스탯 포인트는 유지하되 추가 보너스 지급
            statPointsAvailable += 10;
            skillPoints = 0;

            CurrentHP = MaxHP;

            OnRebirthCountChanged?.Invoke(rebirthCount);
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(CurrentHP, MaxHP);

            Debug.Log($"[PlayerStats] Rebirth! Count: {rebirthCount}");
            return true;
        }
        #endregion

        #region Damage Calculation
        /// <summary>
        /// 최종 데미지 계산 (치명타 포함)
        /// </summary>
        public BigDamageResult CalculateDamage()
        {
            bool isCritical = UnityEngine.Random.value <= CriticalRate;
            BigNumber damage = Attack;

            if (isCritical)
            {
                damage = damage * CriticalDamageMultiplier;
            }

            return new BigDamageResult
            {
                damage = damage,
                isCritical = isCritical
            };
        }

        /// <summary>
        /// 특정 스킬의 데미지 계산
        /// </summary>
        public BigDamageResult CalculateSkillDamage(float skillMultiplier)
        {
            BigDamageResult result = CalculateDamage();
            result.damage = result.damage * skillMultiplier;
            return result;
        }
        #endregion
    }

    /// <summary>
    /// 스탯 타입
    /// </summary>
    public enum StatType
    {
        Strength,   // 힘 (공격력)
        Vitality,   // 체력 (HP)
        Agility,    // 민첩 (치명타)
        Luck        // 행운 (드롭률)
    }

    /// <summary>
    /// 데미지 결과 (BigNumber 버전)
    /// </summary>
    public struct BigDamageResult
    {
        public BigNumber damage;
        public bool isCritical;

        /// <summary>
        /// long으로 변환 (하위 호환용)
        /// </summary>
        public long ToLong() => damage.ToLong();

        /// <summary>
        /// 포맷된 데미지 문자열
        /// </summary>
        public string Formatted => CurrencyFormatter.FormatKoreanSimple(damage);

        /// <summary>
        /// 컬러 포맷된 데미지 문자열
        /// </summary>
        public string FormattedWithColor => CurrencyFormatter.FormatDamage(damage, isCritical);
    }

    /// <summary>
    /// 데미지 결과 (long 버전 - 하위 호환용)
    /// </summary>
    public struct DamageResult
    {
        public long damage;
        public bool isCritical;

        public static implicit operator DamageResult(BigDamageResult big)
        {
            return new DamageResult
            {
                damage = big.damage.ToLong(),
                isCritical = big.isCritical
            };
        }
    }

    /// <summary>
    /// 플레이어 저장 데이터
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int level;
        public string currentExpSerialized;
        public string goldSerialized;
        public string currentHPSerialized;
        public int rebirthCount;
        public int skillPoints;
        public int statPointsAvailable;
        public int strengthPoints;
        public int vitalityPoints;
        public int agilityPoints;
        public int luckPoints;

        // 하위 호환용 (기존 long 데이터)
        [Obsolete("Use currentExpSerialized instead")]
        public long currentExp;
        [Obsolete("Use goldSerialized instead")]
        public long gold;
        [Obsolete("Use currentHPSerialized instead")]
        public long currentHP;
    }
}
