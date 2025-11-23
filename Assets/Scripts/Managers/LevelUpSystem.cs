using System;
using System.Collections.Generic;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 레벨업 시스템 - 레벨별 보상, 스킬 해금, 콘텐츠 해금 관리
    /// </summary>
    public class LevelUpSystem : MonoBehaviour
    {
        public static LevelUpSystem Instance { get; private set; }

        #region Events
        public event Action<int, LevelUpReward> OnLevelRewardGranted;    // 레벨업 보상 지급
        public event Action<string> OnContentUnlocked;                    // 콘텐츠 해금
        public event Action<int> OnMilestoneReached;                      // 마일스톤 달성
        public event Action<string, int> OnSkillUnlocked;                 // 스킬 해금
        #endregion

        #region Settings
        [Header("=== Level Milestone Settings ===")]
        [SerializeField] private List<LevelMilestone> levelMilestones = new List<LevelMilestone>();

        [Header("=== Content Unlock Levels ===")]
        [SerializeField] private int unlockSkillTreeLevel = 5;
        [SerializeField] private int unlockEquipmentLevel = 10;
        [SerializeField] private int unlockDungeonLevel = 15;
        [SerializeField] private int unlockPvPLevel = 20;
        [SerializeField] private int unlockGuildLevel = 25;

        [Header("=== Stat Growth Per Level ===")]
        [SerializeField] private float attackGrowthPercent = 5f;      // 레벨당 공격력 증가 %
        [SerializeField] private float hpGrowthPercent = 3f;          // 레벨당 HP 증가 %
        [SerializeField] private int statPointsPerLevel = 3;
        [SerializeField] private int skillPointsPerLevel = 1;
        #endregion

        #region State
        private PlayerStats playerStats;
        private HashSet<string> unlockedContent = new HashSet<string>();
        private HashSet<int> achievedMilestones = new HashSet<int>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeDefaultMilestones();
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
        /// PlayerStats 설정 및 이벤트 연결
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            // 이전 연결 해제
            if (playerStats != null)
            {
                playerStats.OnLevelUp -= HandleLevelUp;
            }

            playerStats = stats;

            // 이벤트 연결
            if (playerStats != null)
            {
                playerStats.OnLevelUp += HandleLevelUp;
            }
        }

        /// <summary>
        /// 기본 마일스톤 초기화
        /// </summary>
        private void InitializeDefaultMilestones()
        {
            if (levelMilestones.Count > 0) return;

            // 기본 마일스톤 설정
            levelMilestones = new List<LevelMilestone>
            {
                new LevelMilestone { level = 5, rewardType = MilestoneRewardType.SkillPoints, rewardAmount = 5, description = "스킬 트리 해금" },
                new LevelMilestone { level = 10, rewardType = MilestoneRewardType.Gold, rewardAmount = 1000, description = "장비 시스템 해금" },
                new LevelMilestone { level = 15, rewardType = MilestoneRewardType.SkillPoints, rewardAmount = 10, description = "던전 해금" },
                new LevelMilestone { level = 20, rewardType = MilestoneRewardType.StatPoints, rewardAmount = 10, description = "PvP 해금" },
                new LevelMilestone { level = 25, rewardType = MilestoneRewardType.Gold, rewardAmount = 5000, description = "길드 해금" },
                new LevelMilestone { level = 50, rewardType = MilestoneRewardType.SkillPoints, rewardAmount = 20, description = "고급 스킬 해금" },
                new LevelMilestone { level = 100, rewardType = MilestoneRewardType.All, rewardAmount = 50, description = "최고 레벨 달성" },
            };
        }
        #endregion

        #region Level Up Handling
        /// <summary>
        /// 레벨업 처리
        /// </summary>
        private void HandleLevelUp(int oldLevel, int newLevel)
        {
            Debug.Log($"[LevelUpSystem] Level Up: {oldLevel} -> {newLevel}");

            // 레벨업 보상 계산 및 지급
            LevelUpReward reward = CalculateLevelUpReward(newLevel);
            GrantLevelUpReward(reward);

            OnLevelRewardGranted?.Invoke(newLevel, reward);

            // 콘텐츠 해금 체크
            CheckContentUnlocks(newLevel);

            // 마일스톤 체크
            CheckMilestones(oldLevel, newLevel);
        }

        /// <summary>
        /// 레벨업 보상 계산
        /// </summary>
        private LevelUpReward CalculateLevelUpReward(int level)
        {
            // 기본 보상
            LevelUpReward reward = new LevelUpReward
            {
                statPoints = statPointsPerLevel,
                skillPoints = skillPointsPerLevel,
                goldBonus = level * 100,  // 레벨 * 100 골드
            };

            // 10레벨마다 추가 보상
            if (level % 10 == 0)
            {
                reward.statPoints += 5;
                reward.skillPoints += 3;
                reward.goldBonus *= 5;
            }

            return reward;
        }

        /// <summary>
        /// 레벨업 보상 지급
        /// </summary>
        private void GrantLevelUpReward(LevelUpReward reward)
        {
            if (playerStats == null) return;

            // 스탯 포인트와 스킬 포인트는 PlayerStats에서 자동 지급
            // 여기서는 추가 골드만 지급
            if (reward.goldBonus > 0)
            {
                playerStats.AddGold(reward.goldBonus);
            }
        }
        #endregion

        #region Content Unlocks
        /// <summary>
        /// 콘텐츠 해금 체크
        /// </summary>
        private void CheckContentUnlocks(int level)
        {
            CheckAndUnlock(level, unlockSkillTreeLevel, "SkillTree");
            CheckAndUnlock(level, unlockEquipmentLevel, "Equipment");
            CheckAndUnlock(level, unlockDungeonLevel, "Dungeon");
            CheckAndUnlock(level, unlockPvPLevel, "PvP");
            CheckAndUnlock(level, unlockGuildLevel, "Guild");
        }

        /// <summary>
        /// 개별 콘텐츠 해금 체크 및 처리
        /// </summary>
        private void CheckAndUnlock(int currentLevel, int requiredLevel, string contentName)
        {
            if (currentLevel >= requiredLevel && !unlockedContent.Contains(contentName))
            {
                unlockedContent.Add(contentName);
                OnContentUnlocked?.Invoke(contentName);
                Debug.Log($"[LevelUpSystem] Content Unlocked: {contentName}");
            }
        }

        /// <summary>
        /// 콘텐츠 해금 여부 확인
        /// </summary>
        public bool IsContentUnlocked(string contentName)
        {
            return unlockedContent.Contains(contentName);
        }

        /// <summary>
        /// 현재 레벨로 해금 가능한 콘텐츠 확인
        /// </summary>
        public bool CanUnlock(string contentName)
        {
            if (playerStats == null) return false;

            int level = playerStats.Level;
            return contentName switch
            {
                "SkillTree" => level >= unlockSkillTreeLevel,
                "Equipment" => level >= unlockEquipmentLevel,
                "Dungeon" => level >= unlockDungeonLevel,
                "PvP" => level >= unlockPvPLevel,
                "Guild" => level >= unlockGuildLevel,
                _ => false
            };
        }
        #endregion

        #region Milestones
        /// <summary>
        /// 마일스톤 체크
        /// </summary>
        private void CheckMilestones(int oldLevel, int newLevel)
        {
            foreach (var milestone in levelMilestones)
            {
                if (milestone.level > oldLevel && milestone.level <= newLevel)
                {
                    if (!achievedMilestones.Contains(milestone.level))
                    {
                        AchieveMilestone(milestone);
                    }
                }
            }
        }

        /// <summary>
        /// 마일스톤 달성 처리
        /// </summary>
        private void AchieveMilestone(LevelMilestone milestone)
        {
            achievedMilestones.Add(milestone.level);

            // 보상 지급
            GrantMilestoneReward(milestone);

            OnMilestoneReached?.Invoke(milestone.level);
            Debug.Log($"[LevelUpSystem] Milestone Reached: Level {milestone.level} - {milestone.description}");
        }

        /// <summary>
        /// 마일스톤 보상 지급
        /// </summary>
        private void GrantMilestoneReward(LevelMilestone milestone)
        {
            if (playerStats == null) return;

            switch (milestone.rewardType)
            {
                case MilestoneRewardType.Gold:
                    playerStats.AddGold(milestone.rewardAmount);
                    break;
                case MilestoneRewardType.StatPoints:
                    for (int i = 0; i < milestone.rewardAmount; i++)
                    {
                        playerStats.AllocateStatPoint(StatType.Strength, 0);
                    }
                    // StatPoints는 직접 추가 불가하므로 다른 방식 필요
                    break;
                case MilestoneRewardType.SkillPoints:
                    playerStats.AddSkillPoints(milestone.rewardAmount);
                    break;
                case MilestoneRewardType.All:
                    playerStats.AddGold(milestone.rewardAmount * 100);
                    playerStats.AddSkillPoints(milestone.rewardAmount);
                    break;
            }
        }

        /// <summary>
        /// 마일스톤 달성 여부 확인
        /// </summary>
        public bool IsMilestoneAchieved(int level)
        {
            return achievedMilestones.Contains(level);
        }

        /// <summary>
        /// 다음 마일스톤 정보 가져오기
        /// </summary>
        public LevelMilestone? GetNextMilestone()
        {
            if (playerStats == null) return null;

            int currentLevel = playerStats.Level;
            foreach (var milestone in levelMilestones)
            {
                if (milestone.level > currentLevel)
                {
                    return milestone;
                }
            }
            return null;
        }
        #endregion

        #region Stat Growth Info
        /// <summary>
        /// 다음 레벨의 예상 스탯 정보
        /// </summary>
        public NextLevelInfo GetNextLevelInfo()
        {
            if (playerStats == null) return default;

            int nextLevel = playerStats.Level + 1;

            return new NextLevelInfo
            {
                level = nextLevel,
                requiredExp = playerStats.RequiredExp,
                currentExp = playerStats.CurrentExp,
                attackGain = (long)(playerStats.Attack * (attackGrowthPercent / 100f)),
                hpGain = (long)(playerStats.MaxHP * (hpGrowthPercent / 100f)),
                statPoints = statPointsPerLevel,
                skillPoints = skillPointsPerLevel
            };
        }

        /// <summary>
        /// 특정 레벨에서의 예상 스탯
        /// </summary>
        public long GetExpectedAttackAtLevel(int level)
        {
            if (playerStats == null) return 0;

            int levelDiff = level - playerStats.Level;
            if (levelDiff <= 0) return playerStats.Attack;

            float multiplier = Mathf.Pow(1f + (attackGrowthPercent / 100f), levelDiff);
            return (long)(playerStats.Attack * multiplier);
        }
        #endregion

        #region Save/Load
        /// <summary>
        /// 저장 데이터 생성
        /// </summary>
        public LevelUpSaveData ToSaveData()
        {
            return new LevelUpSaveData
            {
                unlockedContent = new List<string>(unlockedContent),
                achievedMilestones = new List<int>(achievedMilestones)
            };
        }

        /// <summary>
        /// 저장 데이터 로드
        /// </summary>
        public void LoadFromSaveData(LevelUpSaveData saveData)
        {
            if (saveData == null) return;

            unlockedContent = new HashSet<string>(saveData.unlockedContent ?? new List<string>());
            achievedMilestones = new HashSet<int>(saveData.achievedMilestones ?? new List<int>());
        }
        #endregion
    }

    #region Data Classes
    /// <summary>
    /// 레벨업 보상
    /// </summary>
    [Serializable]
    public struct LevelUpReward
    {
        public int statPoints;
        public int skillPoints;
        public long goldBonus;
    }

    /// <summary>
    /// 레벨 마일스톤
    /// </summary>
    [Serializable]
    public struct LevelMilestone
    {
        public int level;
        public MilestoneRewardType rewardType;
        public int rewardAmount;
        public string description;
    }

    /// <summary>
    /// 마일스톤 보상 타입
    /// </summary>
    public enum MilestoneRewardType
    {
        Gold,
        StatPoints,
        SkillPoints,
        Item,
        All
    }

    /// <summary>
    /// 다음 레벨 정보
    /// </summary>
    public struct NextLevelInfo
    {
        public int level;
        public long requiredExp;
        public long currentExp;
        public long attackGain;
        public long hpGain;
        public int statPoints;
        public int skillPoints;
    }

    /// <summary>
    /// 레벨업 시스템 저장 데이터
    /// </summary>
    [Serializable]
    public class LevelUpSaveData
    {
        public List<string> unlockedContent;
        public List<int> achievedMilestones;
    }
    #endregion
}
