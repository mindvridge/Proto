using System;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// 플레이어 스탯 관리 클래스
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        #region Events
        public event Action<int, int> OnLevelUp;                    // oldLevel, newLevel
        public event Action<long, long> OnExpChanged;               // currentExp, requiredExp
        public event Action<long> OnGoldChanged;                    // newGold
        public event Action<long, long> OnHealthChanged;            // currentHP, maxHP
        public event Action OnStatsChanged;
        public event Action<int> OnRebirthCountChanged;             // rebirthCount
        #endregion

        #region Base Stats
        [SerializeField] private int level = 1;
        [SerializeField] private long currentExp = 0;
        [SerializeField] private long gold = 0;
        [SerializeField] private long currentHP;
        [SerializeField] private int rebirthCount = 0;
        [SerializeField] private int skillPoints = 0;
        #endregion

        #region Stat Points (레벨업 시 분배)
        [SerializeField] private int statPointsAvailable = 0;
        [SerializeField] private int strengthPoints = 0;        // 공격력
        [SerializeField] private int vitalityPoints = 0;        // 체력
        [SerializeField] private int agilityPoints = 0;         // 치명타
        [SerializeField] private int luckPoints = 0;            // 드롭률
        #endregion

        #region Constants
        private const int BASE_ATTACK = 10;
        private const int BASE_MAX_HP = 100;
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
        public long CurrentExp => currentExp;
        public long Gold => gold;
        public long CurrentHP => currentHP;
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
        public long RequiredExp => CalculateRequiredExp(level);

        /// <summary>
        /// 최대 체력
        /// </summary>
        public long MaxHP => CalculateMaxHP();

        /// <summary>
        /// 공격력
        /// </summary>
        public long Attack => CalculateAttack();

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
        public float HPPercent => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0;

        /// <summary>
        /// 경험치 퍼센트
        /// </summary>
        public float ExpPercent => RequiredExp > 0 ? (float)CurrentExp / RequiredExp : 0;
        #endregion

        #region Initialization
        public PlayerStats()
        {
            Initialize();
        }

        public void Initialize()
        {
            level = 1;
            currentExp = 0;
            gold = 0;
            rebirthCount = 0;
            skillPoints = 0;
            statPointsAvailable = 0;
            strengthPoints = 0;
            vitalityPoints = 0;
            agilityPoints = 0;
            luckPoints = 0;
            currentHP = MaxHP;
        }

        /// <summary>
        /// 저장된 데이터로 초기화
        /// </summary>
        public void LoadFromSaveData(PlayerSaveData saveData)
        {
            if (saveData == null) return;

            level = saveData.level;
            currentExp = saveData.currentExp;
            gold = saveData.gold;
            rebirthCount = saveData.rebirthCount;
            skillPoints = saveData.skillPoints;
            statPointsAvailable = saveData.statPointsAvailable;
            strengthPoints = saveData.strengthPoints;
            vitalityPoints = saveData.vitalityPoints;
            agilityPoints = saveData.agilityPoints;
            luckPoints = saveData.luckPoints;
            currentHP = saveData.currentHP > 0 ? saveData.currentHP : MaxHP;

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
                currentExp = this.currentExp,
                gold = this.gold,
                currentHP = this.currentHP,
                rebirthCount = this.rebirthCount,
                skillPoints = this.skillPoints,
                statPointsAvailable = this.statPointsAvailable,
                strengthPoints = this.strengthPoints,
                vitalityPoints = this.vitalityPoints,
                agilityPoints = this.agilityPoints,
                luckPoints = this.luckPoints
            };
        }
        #endregion

        #region Stat Calculations
        /// <summary>
        /// 필요 경험치 계산 (레벨 기반 곡선)
        /// </summary>
        private long CalculateRequiredExp(int targetLevel)
        {
            // 기본 공식: 100 * level^2 + 50 * level
            return (long)(100 * Mathf.Pow(targetLevel, 2) + 50 * targetLevel);
        }

        /// <summary>
        /// 최대 체력 계산
        /// </summary>
        private long CalculateMaxHP()
        {
            float baseHP = BASE_MAX_HP;
            float levelBonus = level * LEVEL_HP_MULTIPLIER;
            float vitalityBonus = vitalityPoints * HP_PER_VITALITY;
            float rebirthBonus = 1 + (rebirthCount * REBIRTH_BONUS_MULTIPLIER);

            return (long)((baseHP + levelBonus + vitalityBonus) * rebirthBonus);
        }

        /// <summary>
        /// 공격력 계산
        /// </summary>
        private long CalculateAttack()
        {
            float baseAtk = BASE_ATTACK;
            float levelBonus = level * LEVEL_ATTACK_MULTIPLIER;
            float strengthBonus = strengthPoints * ATTACK_PER_STRENGTH;
            float rebirthBonus = 1 + (rebirthCount * REBIRTH_BONUS_MULTIPLIER);

            return (long)((baseAtk + levelBonus + strengthBonus) * rebirthBonus);
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
        public void AddExp(long amount)
        {
            if (amount <= 0) return;

            long bonusExp = (long)(amount * ExpBonus);
            currentExp += bonusExp;

            OnExpChanged?.Invoke(currentExp, RequiredExp);

            // 레벨업 체크
            while (currentExp >= RequiredExp)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// 레벨업 처리
        /// </summary>
        private void LevelUp()
        {
            int oldLevel = level;
            currentExp -= RequiredExp;
            level++;

            // 스탯 포인트 및 스킬 포인트 지급
            statPointsAvailable += STAT_POINTS_PER_LEVEL;
            skillPoints += SKILL_POINTS_PER_LEVEL;

            // HP 회복
            currentHP = MaxHP;

            OnLevelUp?.Invoke(oldLevel, level);
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(currentHP, MaxHP);

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
            currentExp = 0;
            currentHP = MaxHP;

            OnLevelUp?.Invoke(oldLevel, level);
            OnStatsChanged?.Invoke();
        }
        #endregion

        #region Gold
        /// <summary>
        /// 골드 획득
        /// </summary>
        public void AddGold(long amount)
        {
            if (amount <= 0) return;

            long bonusGold = (long)(amount * GoldBonus);
            gold += bonusGold;

            OnGoldChanged?.Invoke(gold);
        }

        /// <summary>
        /// 골드 소비
        /// </summary>
        public bool SpendGold(long amount)
        {
            if (amount <= 0 || gold < amount) return false;

            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        /// <summary>
        /// 골드 충분한지 확인
        /// </summary>
        public bool HasEnoughGold(long amount)
        {
            return gold >= amount;
        }
        #endregion

        #region Health
        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(long damage)
        {
            if (damage <= 0) return;

            currentHP = Math.Max(0, currentHP - damage);
            OnHealthChanged?.Invoke(currentHP, MaxHP);

            if (currentHP <= 0)
            {
                Debug.Log("[PlayerStats] Player died!");
            }
        }

        /// <summary>
        /// HP 회복
        /// </summary>
        public void Heal(long amount)
        {
            if (amount <= 0) return;

            currentHP = Math.Min(MaxHP, currentHP + amount);
            OnHealthChanged?.Invoke(currentHP, MaxHP);
        }

        /// <summary>
        /// HP 퍼센트로 회복
        /// </summary>
        public void HealPercent(float percent)
        {
            long healAmount = (long)(MaxHP * percent);
            Heal(healAmount);
        }

        /// <summary>
        /// HP 전체 회복
        /// </summary>
        public void FullHeal()
        {
            currentHP = MaxHP;
            OnHealthChanged?.Invoke(currentHP, MaxHP);
        }

        /// <summary>
        /// 살아있는지 확인
        /// </summary>
        public bool IsAlive => currentHP > 0;
        #endregion

        #region Stat Points
        /// <summary>
        /// 스탯 포인트 분배
        /// </summary>
        public bool AllocateStatPoint(StatType statType, int amount = 1)
        {
            if (amount <= 0 || statPointsAvailable < amount) return false;

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
            currentExp = 0;

            // 스탯 포인트는 유지하되 추가 보너스 지급
            statPointsAvailable += 10;
            skillPoints = 0;

            currentHP = MaxHP;

            OnRebirthCountChanged?.Invoke(rebirthCount);
            OnStatsChanged?.Invoke();
            OnHealthChanged?.Invoke(currentHP, MaxHP);

            Debug.Log($"[PlayerStats] Rebirth! Count: {rebirthCount}");
            return true;
        }
        #endregion

        #region Damage Calculation
        /// <summary>
        /// 최종 데미지 계산 (치명타 포함)
        /// </summary>
        public DamageResult CalculateDamage()
        {
            bool isCritical = UnityEngine.Random.value <= CriticalRate;
            long damage = Attack;

            if (isCritical)
            {
                damage = (long)(damage * CriticalDamageMultiplier);
            }

            return new DamageResult
            {
                damage = damage,
                isCritical = isCritical
            };
        }

        /// <summary>
        /// 특정 스킬의 데미지 계산
        /// </summary>
        public DamageResult CalculateSkillDamage(float skillMultiplier)
        {
            DamageResult result = CalculateDamage();
            result.damage = (long)(result.damage * skillMultiplier);
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
    /// 데미지 결과
    /// </summary>
    public struct DamageResult
    {
        public long damage;
        public bool isCritical;
    }

    /// <summary>
    /// 플레이어 저장 데이터
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int level;
        public long currentExp;
        public long gold;
        public long currentHP;
        public int rebirthCount;
        public int skillPoints;
        public int statPointsAvailable;
        public int strengthPoints;
        public int vitalityPoints;
        public int agilityPoints;
        public int luckPoints;
    }
}
