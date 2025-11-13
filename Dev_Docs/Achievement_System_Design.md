# 업적 시스템 기획서 (Achievement System Design)

## 1. 개요

### 1.1 목적
- 장기 목표 제시를 통한 유저 리텐션 증가
- 다양한 플레이 스타일 유도 및 콘텐츠 소비 촉진
- 성취감 제공 및 수집 욕구 자극
- 숨겨진 콘텐츠 발견의 재미 제공

### 1.2 핵심 설계 철학
- **단계적 성취감**: 쉬운 업적부터 어려운 업적까지 난이도 분산
- **다양한 플레이 보상**: 모든 플레이 스타일이 업적으로 이어짐
- **숨겨진 발견**: 히든 업적을 통한 탐험 욕구 자극
- **궁극의 목표**: 최종 업적 달성을 위한 장기 목표 제시

### 1.3 업적 시스템 특징
```
총 업적 수: 72개
- 일반 업적: 52개
- 히든 업적: 20개

카테고리: 13개
Progress, Combat, Collection, Skill, Story, Challenge,
Hidden, Time, Wealth, Special, Mythic, Ultimate

총 보상:
- 골드: 약 200억
- 젬: 약 20만개
- 기타: 스킬 포인트, 아이템, 칭호 등
```

---

## 2. 업적 카테고리

### 2.1 Progress (진행) - 5개
레벨 달성 및 환생 관련 업적

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| progress_001 | 첫 걸음 | 레벨 10 달성 | 골드 5,000, 젬 10 |
| progress_002 | 성장하는 영웅 | 레벨 25 달성 | 골드 20,000, 젬 25 |
| progress_003 | 중급 모험가 | 레벨 50 달성 | 골드 100,000, 젬 50, 스킬 포인트 10 |
| progress_004 | 고급 전사 | 레벨 75 달성 | 골드 500,000, 젬 100 |
| progress_005 | 전설의 영웅 | 레벨 100 달성 | 골드 2,000,000, 젬 200, 환생의 증표 |

**환생 업적 (3개)**
| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| rebirth_001 | 새로운 시작 | 1회 환생 | 골드 1,000,000, 젬 100, 스킬 포인트 20 |
| rebirth_002 | 환생의 달인 | 5회 환생 | 골드 10,000,000, 젬 500 |
| rebirth_003 | 윤회의 주인 | 10회 환생 | 골드 50,000,000, 젬 1,000 |

### 2.2 Combat (전투) - 8개
몬스터 처치 및 전투 관련 업적

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| combat_001 | 몬스터 사냥꾼 | 몬스터 100마리 처치 | 골드 10,000, 젬 5 |
| combat_002 | 몬스터 학살자 | 몬스터 1,000마리 처치 | 골드 100,000, 젬 50 |
| combat_003 | 몬스터 전멸자 | 몬스터 10,000마리 처치 | 골드 1,000,000, 젬 200 |
| combat_004 | 보스 헌터 | 보스 10마리 처치 | 골드 50,000, 젬 30 |
| combat_005 | 드래곤 슬레이어 | 드래곤 킹 처치 | 골드 200,000, 젬 100, 드래곤 비늘 50개 |
| combat_006 | 마왕 처단자 | 마제 처치 | 골드 300,000, 젬 150 |
| combat_007 | 크리티컬 마스터 | 크리티컬 100회 | 골드 50,000, 젬 25 |
| combat_008 | 원샷 원킬 (히든) | 보스를 한 방에 처치 | 골드 500,000, 젬 200 |

### 2.3 Collection (수집) - 5개
아이템 수집 및 강화 관련

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| collection_001 | 수집가의 시작 | 아이템 10종 획득 | 골드 10,000, 젬 5 |
| collection_002 | 열정적인 수집가 | 아이템 30종 획득 | 골드 50,000, 젬 30 |
| collection_003 | 완벽한 컬렉션 | 모든 아이템 획득 (48종) | 골드 1,000,000, 젬 500 |
| collection_004 | 강화의 달인 | 아이템 +10 강화 | 골드 100,000, 젬 50 |
| collection_005 | 신의 장비 | 아이템 +20 강화 | 골드 5,000,000, 젬 500 |

### 2.4 Skill (스킬) - 4개
스킬 학습 및 마스터

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| skill_001 | 스킬 배우기 | 스킬 5개 습득 | 골드 20,000, 스킬 포인트 5 |
| skill_002 | 스킬 마스터 | 스킬 20개 습득 | 골드 200,000, 스킬 포인트 20 |
| skill_003 | 전지전능 | 모든 스킬 습득 (48개) | 골드 5,000,000, 젬 500 |
| skill_004 | 최대 강화 | 스킬 1개 최대 레벨 | 골드 500,000, 젬 100 |

### 2.5 Story & Challenge (스토리 & 도전) - 10개
던전 클리어 및 탑 도전

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| story_001 | 모험의 시작 | 첫 던전 클리어 | 골드 5,000, 젬 5 |
| story_002 | 마왕성 돌파 | 마왕성 본관 클리어 | 골드 200,000, 젬 100 |
| challenge_001 | 탑의 정복자 I | 무한의 탑 10층 | 골드 100,000, 젬 50 |
| challenge_002 | 탑의 정복자 II | 무한의 탑 30층 | 골드 500,000, 젬 200 |
| challenge_003 | 탑의 정복자 III | 무한의 탑 50층 | 골드 2,000,000, 젬 500 |
| challenge_004 | 파괴신 처단 | 무한의 탑 100층 | 골드 10,000,000, 젬 1,000, 무한의 검 |

### 2.6 Hidden (히든) - 4개
숨겨진 콘텐츠 발견

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| hidden_001 | 진정한 은둔자 (히든) | 은둔의 성역 클리어 | 골드 5,000,000, 젬 500, 무한의 검 |
| hidden_002 | 차원 정복자 (히든) | 차원의 틈 클리어 | 골드 8,000,000, 젬 800 |
| hidden_003 | 슬라임 학살자 (히든) | 무한 슬라임 처치 | 골드 10,000,000, 젬 1,000 |
| hidden_004 | 정체 발각?! (히든) | 여동생 의심 이벤트 클리어 | 골드 3,000,000, 젬 300 |

### 2.7 Time (시간) - 5개
출석 및 플레이 시간

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| time_001 | 첫 날 | 첫 로그인 | 골드 1,000, 젬 10 |
| time_002 | 일주일 플레이 | 7일 연속 로그인 | 골드 50,000, 젬 50 |
| time_003 | 한 달의 모험 | 30일 연속 로그인 | 골드 500,000, 젬 300 |
| time_004 | 1년의 헌신 | 365일 누적 로그인 | 골드 10,000,000, 젬 2,000 |
| time_005 | 야행성 인간 | 밤 시간대 100회 플레이 | 골드 100,000, 젬 50 |

### 2.8 Wealth (재화) - 3개
골드 축적

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| wealth_001 | 백만장자 | 골드 1,000,000 보유 | 젬 100 |
| wealth_002 | 억만장자 | 골드 100,000,000 보유 | 젬 500 |
| wealth_003 | 골드 파밍 마스터 | 누적 골드 1억 획득 | 골드 5,000,000, 젬 200 |

### 2.9 Special (특수) - 3개
특별한 조건 달성

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| special_001 | 의심도 0% (히든) | 의심도 0% 유지하며 레벨 30 | 골드 1,000,000, 젬 500 |
| special_002 | 완벽한 은둔 (히든) | 의심도 50% 이하 유지하며 레벨 50 | 골드 5,000,000, 젬 1,000 |
| special_003 | 속도광 (히든) | 1시간 내 환생 도달 | 골드 2,000,000, 젬 500 |

### 2.10 Mythic (신화급) - 4개
엔드게임 보스 처치

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| mythic_001 | 기원과의 대면 | 기원 처치 | 골드 50,000,000, 젬 5,000 |
| mythic_002 | 신을 넘어서 | 절대신 처치 | 골드 100,000,000, 젬 10,000 |
| mythic_003 | 카오스 정복 | 카오스의 화신 처치 | 골드 200,000,000, 젬 20,000 |
| mythic_004 | 영원의 순간 (히든) | 영원 처치 | 골드 500,000,000, 젬 50,000 |

### 2.11 Ultimate (궁극) - 3개
최종 업적

| ID | 업적명 | 조건 | 보상 |
|----|--------|------|------|
| ultimate_001 | 무한의 끝 (히든) | 무한 처치 | 골드 999,999,999, 젬 99,999, 무한의 검 |
| ultimate_002 | 진정한 은둔형 영웅 | 모든 메인 스토리 클리어 | 골드 999,999,999, 젬 99,999 |
| ultimate_003 | 완전 정복 | 모든 업적 달성 | 칭호 "완벽한 은둔자", 젬 100,000 |

---

## 3. 업적 시스템 구현

### 3.1 데이터 구조

```csharp
[System.Serializable]
public class Achievement
{
    public string achievementId;
    public string achievementName;
    public string achievementNameEN;
    public string description;
    public string descriptionEN;
    public AchievementCategory category;
    public string iconName;
    public bool hidden; // 히든 업적 여부

    public RequirementType requirementType;
    public object requirementValue; // int, string, float 등

    public List<AchievementReward> rewards;
    public string unlockDialogueId; // 달성 시 대화 트리거

    // 진행 상태
    public int currentProgress;
    public bool isUnlocked;
    public bool isClaimed;
    public DateTime unlockedDate;
}

public enum AchievementCategory
{
    Progress,      // 레벨/환생 진행
    Combat,        // 전투
    Collection,    // 수집
    Skill,         // 스킬
    Story,         // 스토리
    Challenge,     // 도전 과제
    Hidden,        // 히든
    Time,          // 시간/출석
    Wealth,        // 재화
    Special,       // 특수
    Mythic,        // 신화급
    Ultimate       // 궁극
}

public enum RequirementType
{
    PlayerLevel,              // 레벨 달성
    RebirthCount,             // 환생 횟수
    TotalKills,               // 총 처치 수
    BossKills,                // 보스 처치 수
    BossDefeat,               // 특정 보스 처치
    CriticalHits,             // 크리티컬 횟수
    UniqueItemsCollected,     // 고유 아이템 수집
    ItemEnhanceLevel,         // 아이템 강화 레벨
    SkillsLearned,            // 스킬 학습 수
    SkillMaxLevel,            // 스킬 최대 레벨
    DungeonClear,             // 던전 클리어
    EventComplete,            // 이벤트 완료
    LoginDays,                // 로그인 일수
    ConsecutiveLoginDays,     // 연속 로그인
    TotalLoginDays,           // 누적 로그인
    NightPlayCount,           // 밤 플레이 횟수
    GoldOwned,                // 골드 보유량
    TotalGoldEarned,          // 누적 골드 획득
    LevelWithZeroSuspicion,   // 의심도 0% 유지
    LevelWithLowSuspicion,    // 의심도 50% 이하 유지
    TimeToRebirth,            // 환생까지 소요 시간
    OneShotBoss,              // 보스 원샷
    StoryComplete,            // 스토리 완료도
    AllAchievements           // 모든 업적 달성
}

[System.Serializable]
public class AchievementReward
{
    public string type; // Gold, Exp, Gem, Item, SkillPoint, Title
    public int amount;
    public string itemId; // type이 Item일 때
    public int count;     // type이 Item일 때
    public string titleId; // type이 Title일 때
}
```

### 3.2 AchievementManager 구현

```csharp
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public List<Achievement> allAchievements;
    private Dictionary<string, Achievement> achievementDict;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        LoadAchievements();
        SubscribeToEvents();
    }

    // JSON 파일에서 업적 데이터 로드
    private void LoadAchievements()
    {
        TextAsset achievementsJson = Resources.Load<TextAsset>("GameData/Achievements");
        AchievementData data = JsonUtility.FromJson<AchievementData>(achievementsJson.text);

        allAchievements = data.achievements;
        achievementDict = new Dictionary<string, Achievement>();

        foreach (var achievement in allAchievements)
        {
            achievementDict[achievement.achievementId] = achievement;
        }

        LoadAchievementProgress();
    }

    // 이벤트 구독
    private void SubscribeToEvents()
    {
        EventManager.OnPlayerLevelUp += CheckLevelAchievements;
        EventManager.OnMonsterKilled += CheckCombatAchievements;
        EventManager.OnBossDefeated += CheckBossAchievements;
        EventManager.OnItemCollected += CheckCollectionAchievements;
        EventManager.OnItemEnhanced += CheckEnhancementAchievements;
        EventManager.OnSkillLearned += CheckSkillAchievements;
        EventManager.OnDungeonCleared += CheckDungeonAchievements;
        EventManager.OnRebirth += CheckRebirthAchievements;
        EventManager.OnLogin += CheckLoginAchievements;
        // ... 기타 이벤트
    }

    // 업적 달성 체크 (범용)
    public void CheckAchievement(RequirementType type, object currentValue)
    {
        foreach (var achievement in allAchievements)
        {
            if (achievement.isUnlocked || achievement.requirementType != type)
                continue;

            bool achieved = false;

            switch (type)
            {
                case RequirementType.PlayerLevel:
                    int level = (int)currentValue;
                    achieved = level >= (int)achievement.requirementValue;
                    achievement.currentProgress = level;
                    break;

                case RequirementType.TotalKills:
                    int kills = (int)currentValue;
                    achieved = kills >= (int)achievement.requirementValue;
                    achievement.currentProgress = kills;
                    break;

                case RequirementType.BossDefeat:
                    string bossId = (string)currentValue;
                    achieved = bossId == (string)achievement.requirementValue;
                    achievement.currentProgress = achieved ? 1 : 0;
                    break;

                case RequirementType.UniqueItemsCollected:
                    int itemCount = (int)currentValue;
                    achieved = itemCount >= (int)achievement.requirementValue;
                    achievement.currentProgress = itemCount;
                    break;

                case RequirementType.ConsecutiveLoginDays:
                    int days = (int)currentValue;
                    achieved = days >= (int)achievement.requirementValue;
                    achievement.currentProgress = days;
                    break;

                // ... 기타 RequirementType 처리
            }

            if (achieved)
            {
                UnlockAchievement(achievement);
            }
        }
    }

    // 업적 달성
    private void UnlockAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;
        achievement.unlockedDate = DateTime.Now;

        // 업적 달성 알림
        UIManager.Instance.ShowAchievementUnlocked(achievement);

        // 대화 트리거
        if (!string.IsNullOrEmpty(achievement.unlockDialogueId))
        {
            DialogueManager.Instance.StartDialogue(achievement.unlockDialogueId);
        }

        // 자동 보상 수령 여부 (설정에 따라)
        if (GameSettings.Instance.autoClaimAchievements)
        {
            ClaimAchievementReward(achievement);
        }

        SaveAchievementProgress();

        // 이벤트 발생
        EventManager.TriggerAchievementUnlocked(achievement.achievementId);
    }

    // 업적 보상 수령
    public void ClaimAchievementReward(Achievement achievement)
    {
        if (!achievement.isUnlocked || achievement.isClaimed)
            return;

        foreach (var reward in achievement.rewards)
        {
            switch (reward.type)
            {
                case "Gold":
                    PlayerManager.Instance.AddGold(reward.amount);
                    break;

                case "Exp":
                    PlayerManager.Instance.AddExp(reward.amount);
                    break;

                case "Gem":
                    PlayerManager.Instance.AddGems(reward.amount);
                    break;

                case "Item":
                    InventoryManager.Instance.AddItem(reward.itemId, reward.count);
                    break;

                case "SkillPoint":
                    SkillTreeManager.Instance.AddSkillPoints(reward.amount);
                    break;

                case "Title":
                    PlayerManager.Instance.UnlockTitle(reward.titleId);
                    break;
            }
        }

        achievement.isClaimed = true;
        UIManager.Instance.ShowRewardClaimed(achievement.rewards);

        SaveAchievementProgress();
    }

    // 특정 카테고리 업적 진행도 조회
    public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
    {
        return allAchievements.FindAll(a => a.category == category);
    }

    // 전체 업적 완료율
    public float GetTotalProgress()
    {
        int totalAchievements = allAchievements.Count;
        int unlockedAchievements = allAchievements.Count(a => a.isUnlocked);

        return (float)unlockedAchievements / totalAchievements * 100f;
    }

    // 카테고리별 완료율
    public float GetCategoryProgress(AchievementCategory category)
    {
        var categoryAchievements = GetAchievementsByCategory(category);
        int total = categoryAchievements.Count;
        int unlocked = categoryAchievements.Count(a => a.isUnlocked);

        return total > 0 ? (float)unlocked / total * 100f : 0f;
    }

    // 특정 업적 진행도 (0.0 ~ 1.0)
    public float GetAchievementProgress(string achievementId)
    {
        if (!achievementDict.ContainsKey(achievementId))
            return 0f;

        Achievement achievement = achievementDict[achievementId];

        if (achievement.isUnlocked)
            return 1f;

        // 진행도 계산
        int current = achievement.currentProgress;
        int target = 0;

        if (achievement.requirementValue is int)
            target = (int)achievement.requirementValue;
        else if (achievement.requirementValue is float)
            target = (int)(float)achievement.requirementValue;

        if (target == 0)
            return 0f;

        return Mathf.Clamp01((float)current / target);
    }

    // 개별 이벤트 핸들러 예시
    private void CheckLevelAchievements(int newLevel)
    {
        CheckAchievement(RequirementType.PlayerLevel, newLevel);
    }

    private void CheckCombatAchievements(string monsterId)
    {
        int totalKills = PlayerStats.Instance.totalMonstersKilled;
        CheckAchievement(RequirementType.TotalKills, totalKills);
    }

    private void CheckBossAchievements(string bossId)
    {
        CheckAchievement(RequirementType.BossDefeat, bossId);

        int bossKills = PlayerStats.Instance.totalBossesKilled;
        CheckAchievement(RequirementType.BossKills, bossKills);
    }

    // 저장/로드
    private void SaveAchievementProgress()
    {
        AchievementSaveData saveData = new AchievementSaveData
        {
            achievements = allAchievements
        };

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("AchievementProgress", json);
        PlayerPrefs.Save();
    }

    private void LoadAchievementProgress()
    {
        string json = PlayerPrefs.GetString("AchievementProgress", "");
        if (string.IsNullOrEmpty(json))
            return;

        AchievementSaveData saveData = JsonUtility.FromJson<AchievementSaveData>(json);

        // 진행도 복원
        foreach (var savedAchievement in saveData.achievements)
        {
            if (achievementDict.ContainsKey(savedAchievement.achievementId))
            {
                Achievement achievement = achievementDict[savedAchievement.achievementId];
                achievement.currentProgress = savedAchievement.currentProgress;
                achievement.isUnlocked = savedAchievement.isUnlocked;
                achievement.isClaimed = savedAchievement.isClaimed;
                achievement.unlockedDate = savedAchievement.unlockedDate;
            }
        }
    }
}

[System.Serializable]
public class AchievementData
{
    public List<Achievement> achievements;
}

[System.Serializable]
public class AchievementSaveData
{
    public List<Achievement> achievements;
}
```

---

## 4. UI/UX 설계

### 4.1 업적 메인 화면

```
[업적 화면]

┌─────────────────────────────────────┐
│  🏆 업적                              │
│  전체 완료: 35/72 (48.6%)            │
├─────────────────────────────────────┤
│                                      │
│  [전체] [진행] [전투] [수집] [...]    │
│                                      │
│  ┌────────────────────────────────┐ │
│  │ ⭐ 첫 걸음                  ✓   │ │
│  │ 레벨 10 달성                    │ │
│  │ 보상: 골드 5,000, 젬 10          │ │
│  └────────────────────────────────┘ │
│                                      │
│  ┌────────────────────────────────┐ │
│  │ 🎯 성장하는 영웅            ✓   │ │
│  │ 레벨 25 달성                    │ │
│  │ 보상: 골드 20,000, 젬 25         │ │
│  └────────────────────────────────┘ │
│                                      │
│  ┌────────────────────────────────┐ │
│  │ 💪 중급 모험가            75%   │ │
│  │ 레벨 50 달성 (현재: 38)         │ │
│  │ ████████░░ 38/50                │ │
│  │ 보상: 골드 100,000, 젬 50,      │ │
│  │       스킬 포인트 10             │ │
│  └────────────────────────────────┘ │
│                                      │
│  ┌────────────────────────────────┐ │
│  │ ❓ ???                      0%  │ │
│  │ ???                             │ │
│  │ 히든 업적 - 조건 비공개          │ │
│  └────────────────────────────────┘ │
│                                      │
└─────────────────────────────────────┘
```

### 4.2 업적 달성 팝업

```
┌─────────────────────────────────┐
│   🎉 업적 달성! 🎉               │
├─────────────────────────────────┤
│                                  │
│        ⭐ 첫 걸음 ⭐              │
│                                  │
│     레벨 10에 도달했습니다!       │
│                                  │
│  ┌───────────────────────────┐  │
│  │ 보상                       │  │
│  │ • 골드: 5,000              │  │
│  │ • 젬: 10                   │  │
│  └───────────────────────────┘  │
│                                  │
│       [ 보상 수령 ]               │
│                                  │
└─────────────────────────────────┘
```

### 4.3 업적 알림 (배너)

```
┌────────────────────────────┐
│ 🏆 업적 달성!               │
│ "첫 걸음" 달성!             │
│ 보상: 골드 5,000, 젬 10     │
└────────────────────────────┘
```

### 4.4 업적 UI 구현

```csharp
public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform achievementListContent;
    public GameObject achievementItemPrefab;
    public Text totalProgressText;
    public Image totalProgressBar;

    [Header("Category Tabs")]
    public Toggle[] categoryTabs;

    private AchievementCategory currentCategory = AchievementCategory.Progress;
    private List<AchievementItemUI> achievementItems = new List<AchievementItemUI>();

    void Start()
    {
        SetupCategoryTabs();
        RefreshAchievementList();
        UpdateTotalProgress();
    }

    private void SetupCategoryTabs()
    {
        for (int i = 0; i < categoryTabs.Length; i++)
        {
            int index = i;
            categoryTabs[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    currentCategory = (AchievementCategory)index;
                    RefreshAchievementList();
                }
            });
        }
    }

    public void RefreshAchievementList()
    {
        // 기존 리스트 제거
        foreach (var item in achievementItems)
        {
            Destroy(item.gameObject);
        }
        achievementItems.Clear();

        // 업적 필터링
        List<Achievement> achievements;
        if (currentCategory == (AchievementCategory)(-1)) // 전체
        {
            achievements = AchievementManager.Instance.allAchievements;
        }
        else
        {
            achievements = AchievementManager.Instance.GetAchievementsByCategory(currentCategory);
        }

        // 업적 아이템 생성
        foreach (var achievement in achievements)
        {
            // 히든 업적은 달성 전까지 ???로 표시
            if (achievement.hidden && !achievement.isUnlocked)
            {
                CreateHiddenAchievementItem(achievement);
            }
            else
            {
                CreateAchievementItem(achievement);
            }
        }

        UpdateCategoryProgress();
    }

    private void CreateAchievementItem(Achievement achievement)
    {
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContent);
        AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();

        itemUI.SetAchievement(achievement);
        achievementItems.Add(itemUI);
    }

    private void CreateHiddenAchievementItem(Achievement achievement)
    {
        GameObject itemObj = Instantiate(achievementItemPrefab, achievementListContent);
        AchievementItemUI itemUI = itemObj.GetComponent<AchievementItemUI>();

        itemUI.SetAsHidden();
        achievementItems.Add(itemUI);
    }

    private void UpdateTotalProgress()
    {
        float progress = AchievementManager.Instance.GetTotalProgress();
        int unlocked = AchievementManager.Instance.allAchievements.Count(a => a.isUnlocked);
        int total = AchievementManager.Instance.allAchievements.Count;

        totalProgressText.text = $"전체 완료: {unlocked}/{total} ({progress:F1}%)";
        totalProgressBar.fillAmount = progress / 100f;
    }

    private void UpdateCategoryProgress()
    {
        float progress = AchievementManager.Instance.GetCategoryProgress(currentCategory);
        // 카테고리 진행도 UI 업데이트
    }

    // 업적 달성 팝업 표시
    public void ShowAchievementUnlockedPopup(Achievement achievement)
    {
        // 팝업 생성 및 표시
        GameObject popupObj = Instantiate(achievementUnlockedPopupPrefab);
        AchievementUnlockedPopup popup = popupObj.GetComponent<AchievementUnlockedPopup>();

        popup.SetAchievement(achievement);
        popup.Show();

        // 사운드 재생
        AudioManager.Instance.PlaySound("achievement_unlocked");

        // 파티클 효과
        ParticleSystem particles = Instantiate(achievementParticlesPrefab);
        particles.Play();
    }

    // 업적 알림 배너
    public void ShowAchievementBanner(Achievement achievement)
    {
        // 화면 상단에 간단한 배너 표시
        AchievementBanner banner = Instantiate(achievementBannerPrefab).GetComponent<AchievementBanner>();
        banner.SetAchievement(achievement);
        banner.Show();

        // 3초 후 자동으로 사라짐
        Destroy(banner.gameObject, 3f);
    }
}

public class AchievementItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Text nameText;
    public Text descriptionText;
    public Text progressText;
    public Image progressBar;
    public GameObject completedCheckmark;
    public GameObject hiddenOverlay;
    public Button claimButton;

    private Achievement achievement;

    public void SetAchievement(Achievement achievement)
    {
        this.achievement = achievement;

        iconImage.sprite = Resources.Load<Sprite>($"Icons/{achievement.iconName}");
        nameText.text = achievement.achievementName;
        descriptionText.text = achievement.description;

        float progress = AchievementManager.Instance.GetAchievementProgress(achievement.achievementId);
        progressBar.fillAmount = progress;
        progressText.text = $"{(int)(progress * 100)}%";

        completedCheckmark.SetActive(achievement.isUnlocked);
        hiddenOverlay.SetActive(false);

        // 보상 수령 버튼
        claimButton.gameObject.SetActive(achievement.isUnlocked && !achievement.isClaimed);
        claimButton.onClick.AddListener(() => ClaimReward());
    }

    public void SetAsHidden()
    {
        nameText.text = "???";
        descriptionText.text = "히든 업적 - 조건 비공개";
        progressText.text = "0%";
        progressBar.fillAmount = 0f;
        hiddenOverlay.SetActive(true);
        claimButton.gameObject.SetActive(false);
    }

    private void ClaimReward()
    {
        AchievementManager.Instance.ClaimAchievementReward(achievement);
        claimButton.gameObject.SetActive(false);
    }
}
```

---

## 5. 특수 업적 구현

### 5.1 히든 업적

히든 업적은 조건이 공개되지 않으며, 달성 전까지 ???로 표시됩니다.

```csharp
// 원샷 원킬 업적
public class OneShotBossChecker : MonoBehaviour
{
    private Dictionary<string, int> bossInitialHealth = new Dictionary<string, int>();

    void Start()
    {
        EventManager.OnBossSpawned += OnBossSpawned;
        EventManager.OnBossDefeated += OnBossDefeated;
    }

    private void OnBossSpawned(string bossId, int health)
    {
        bossInitialHealth[bossId] = health;
    }

    private void OnBossDefeated(string bossId, int damageDealt)
    {
        if (bossInitialHealth.ContainsKey(bossId))
        {
            int initialHealth = bossInitialHealth[bossId];

            // 한 번의 공격으로 처치했는지 확인
            if (damageDealt >= initialHealth)
            {
                AchievementManager.Instance.CheckAchievement(
                    RequirementType.OneShotBoss, 1);
            }

            bossInitialHealth.Remove(bossId);
        }
    }
}

// 의심도 0% 업적
public class SuspicionAchievementChecker : MonoBehaviour
{
    private bool hasExceededZero = false;

    void Update()
    {
        float suspicion = SuspicionManager.Instance.GetSuspicion();

        if (suspicion > 0)
        {
            hasExceededZero = true;
        }

        int playerLevel = PlayerManager.Instance.GetLevel();

        if (playerLevel >= 30 && !hasExceededZero)
        {
            AchievementManager.Instance.CheckAchievement(
                RequirementType.LevelWithZeroSuspicion, 30);
        }
    }
}
```

### 5.2 시간 제한 업적 (속도광)

```csharp
public class SpeedrunAchievement : MonoBehaviour
{
    private DateTime gameStartTime;
    private bool achievementChecked = false;

    void Start()
    {
        gameStartTime = DateTime.Now;
        EventManager.OnRebirth += CheckSpeedrunAchievement;
    }

    private void CheckSpeedrunAchievement()
    {
        if (achievementChecked)
            return;

        TimeSpan timePlayed = DateTime.Now - gameStartTime;
        int secondsPlayed = (int)timePlayed.TotalSeconds;

        // 1시간(3600초) 이내 환생
        if (secondsPlayed <= 3600)
        {
            AchievementManager.Instance.CheckAchievement(
                RequirementType.TimeToRebirth, secondsPlayed);
        }

        achievementChecked = true;
    }
}
```

---

## 6. 보상 시스템

### 6.1 보상 종류

| 보상 타입 | 설명 | 예시 |
|----------|------|------|
| Gold | 골드 | 100,000골드 |
| Exp | 경험치 | 50,000 경험치 |
| Gem | 젬 (프리미엄 화폐) | 100젬 |
| Item | 아이템 | 무한의 검, 드래곤 비늘 50개 |
| SkillPoint | 스킬 포인트 | 20 스킬 포인트 |
| Title | 칭호 | "완벽한 은둔자" |

### 6.2 칭호 시스템

업적 달성으로 얻는 칭호는 프로필에 표시되며, 능력치 보너스를 제공합니다.

```csharp
[System.Serializable]
public class Title
{
    public string titleId;
    public string titleName;
    public string description;
    public StatBonus[] bonuses;
}

[System.Serializable]
public class StatBonus
{
    public StatType statType;
    public float bonusValue;
}

public enum StatType
{
    AttackPower,
    ClickPower,
    CriticalChance,
    CriticalDamage,
    GoldBonus,
    ExpBonus,
    DropRateBonus,
    SuspicionReduction
}

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance;

    private List<Title> unlockedTitles = new List<Title>();
    private Title equippedTitle;

    public void UnlockTitle(string titleId)
    {
        Title title = GetTitleData(titleId);
        if (title != null && !unlockedTitles.Contains(title))
        {
            unlockedTitles.Add(title);
            UIManager.Instance.ShowNotification($"새로운 칭호 획득: {title.titleName}");
        }
    }

    public void EquipTitle(Title title)
    {
        if (!unlockedTitles.Contains(title))
            return;

        // 기존 칭호 효과 제거
        if (equippedTitle != null)
        {
            RemoveTitleBonuses(equippedTitle);
        }

        equippedTitle = title;

        // 새 칭호 효과 적용
        ApplyTitleBonuses(equippedTitle);
    }

    private void ApplyTitleBonuses(Title title)
    {
        PlayerStats stats = PlayerManager.Instance.GetStats();

        foreach (var bonus in title.bonuses)
        {
            switch (bonus.statType)
            {
                case StatType.AttackPower:
                    stats.attackPower *= (1 + bonus.bonusValue);
                    break;
                case StatType.GoldBonus:
                    stats.goldMultiplier *= (1 + bonus.bonusValue);
                    break;
                // ... 기타 스탯 보너스
            }
        }
    }
}
```

**칭호 예시:**

```json
{
  "titleId": "title_perfect_hermit",
  "titleName": "완벽한 은둔자",
  "description": "모든 업적을 달성한 자",
  "bonuses": [
    {"statType": "GoldBonus", "bonusValue": 0.5},
    {"statType": "ExpBonus", "bonusValue": 0.5},
    {"statType": "SuspicionReduction", "bonusValue": 0.3}
  ]
}
```

---

## 7. 알림 시스템

### 7.1 업적 관련 알림

```csharp
public class AchievementNotificationManager : MonoBehaviour
{
    // 업적 달성 가능 알림 (90% 진행도)
    public void CheckAlmostCompleteAchievements()
    {
        foreach (var achievement in AchievementManager.Instance.allAchievements)
        {
            if (achievement.isUnlocked || achievement.hidden)
                continue;

            float progress = AchievementManager.Instance.GetAchievementProgress(achievement.achievementId);

            if (progress >= 0.9f && progress < 1.0f)
            {
                UIManager.Instance.ShowNotification(
                    $"업적 '{achievement.achievementName}'이(가) 곧 달성됩니다! ({(int)(progress * 100)}%)");
            }
        }
    }

    // 미수령 보상 알림
    public void CheckUnclaimedRewards()
    {
        int unclaimedCount = AchievementManager.Instance.allAchievements
            .Count(a => a.isUnlocked && !a.isClaimed);

        if (unclaimedCount > 0)
        {
            UIManager.Instance.ShowNotificationBadge("Achievements", unclaimedCount);
        }
    }
}
```

---

## 8. 통계 및 분석

### 8.1 업적 통계 화면

```
[업적 통계]

┌─────────────────────────────────┐
│  📊 업적 통계                     │
├─────────────────────────────────┤
│                                  │
│  전체 진행도: 48.6%              │
│  ████████████░░░░░░░░░░░        │
│                                  │
│  카테고리별 진행도:               │
│  • 진행: 100% ✓                 │
│  • 전투: 75%                     │
│  • 수집: 60%                     │
│  • 스킬: 50%                     │
│  • 스토리: 40%                   │
│  • 도전: 30%                     │
│  • 히든: 20%                     │
│  • 시간: 80%                     │
│  • 재화: 66%                     │
│  • 특수: 0%                      │
│  • 신화급: 25%                   │
│  • 궁극: 0%                      │
│                                  │
│  달성한 업적: 35개                │
│  획득한 보상:                     │
│  • 골드: 52,350,000              │
│  • 젬: 15,740                    │
│  • 스킬 포인트: 120              │
│  • 아이템: 18종                  │
│  • 칭호: 3개                     │
│                                  │
│  최근 달성 업적:                  │
│  • 중급 모험가 (3일 전)          │
│  • 보스 헌터 (5일 전)            │
│  • 일주일 플레이 (7일 전)        │
│                                  │
└─────────────────────────────────┘
```

### 8.2 통계 구현

```csharp
public class AchievementStatistics : MonoBehaviour
{
    public static AchievementStatistics Instance;

    // 획득한 총 보상 계산
    public Dictionary<string, int> GetTotalRewards()
    {
        Dictionary<string, int> totalRewards = new Dictionary<string, int>
        {
            { "Gold", 0 },
            { "Gem", 0 },
            { "SkillPoint", 0 },
            { "Item", 0 },
            { "Title", 0 }
        };

        foreach (var achievement in AchievementManager.Instance.allAchievements)
        {
            if (!achievement.isClaimed)
                continue;

            foreach (var reward in achievement.rewards)
            {
                if (totalRewards.ContainsKey(reward.type))
                {
                    totalRewards[reward.type] += reward.amount;
                }
            }
        }

        return totalRewards;
    }

    // 카테고리별 완료율
    public Dictionary<AchievementCategory, float> GetCategoryProgress()
    {
        Dictionary<AchievementCategory, float> categoryProgress = new Dictionary<AchievementCategory, float>();

        foreach (AchievementCategory category in Enum.GetValues(typeof(AchievementCategory)))
        {
            float progress = AchievementManager.Instance.GetCategoryProgress(category);
            categoryProgress[category] = progress;
        }

        return categoryProgress;
    }

    // 최근 달성 업적 목록
    public List<Achievement> GetRecentlyUnlockedAchievements(int count = 5)
    {
        return AchievementManager.Instance.allAchievements
            .Where(a => a.isUnlocked)
            .OrderByDescending(a => a.unlockedDate)
            .Take(count)
            .ToList();
    }
}
```

---

## 9. 소셜 기능 (추후 구현)

### 9.1 업적 공유

```csharp
// 업적 달성을 SNS에 공유
public void ShareAchievement(Achievement achievement)
{
    string shareText = $"업적 '{achievement.achievementName}' 달성! 나도 은둔형 영웅!";
    string shareImage = CaptureAchievementImage(achievement);

    SocialManager.Instance.Share(shareText, shareImage);
}
```

### 9.2 친구 비교

```
다른 플레이어와 업적 진행도 비교 기능 (추후 구현)
```

---

## 10. 개발 우선순위

### Phase 1 (핵심 기능)
1. Achievement 데이터 구조 및 JSON 로드
2. AchievementManager 기본 구현
3. 기본 업적 체크 (레벨, 전투, 수집)
4. 업적 UI (리스트, 진행도)
5. 보상 수령 시스템

### Phase 2 (확장 기능)
1. 히든 업적 구현
2. 특수 조건 업적 (의심도, 시간 제한 등)
3. 업적 달성 팝업/알림
4. 칭호 시스템
5. 업적 통계 화면

### Phase 3 (고급 기능)
1. 업적 공유 기능
2. 친구 비교 기능
3. 시즌 업적
4. 업적 랭킹 시스템

---

## 11. 밸런싱 가이드

### 11.1 난이도 분배

```
쉬움 (30%): 1~2시간 플레이로 달성 가능
보통 (40%): 1~2주 플레이로 달성 가능
어려움 (20%): 1~3개월 플레이로 달성 가능
매우 어려움 (10%): 3개월 이상 필요
```

### 11.2 보상 밸런싱

```
일반 업적: 골드 1만~10만, 젬 5~50
레어 업적: 골드 10만~100만, 젬 50~200
에픽 업적: 골드 100만~1000만, 젬 200~1000
레전더리 업적: 골드 1000만~1억, 젬 1000~10000
신화 업적: 골드 1억~10억, 젬 5000~50000
궁극 업적: 골드 10억 이상, 젬 10만, 칭호
```

---

## 12. 테스트 체크리스트

- [ ] 모든 업적이 정상적으로 달성되는지 확인
- [ ] 히든 업적이 조건 충족 전까지 ???로 표시되는지 확인
- [ ] 보상이 정확하게 지급되는지 확인
- [ ] 진행도가 정확하게 계산되는지 확인
- [ ] 업적 데이터가 정상적으로 저장/로드되는지 확인
- [ ] 업적 달성 알림이 정상 작동하는지 확인
- [ ] 칭호 시스템이 정상 작동하는지 확인
- [ ] 업적 UI가 모든 해상도에서 정상 표시되는지 확인

---

**작성일:** 2025-11-13
**버전:** 1.0
**작성자:** AI Assistant
**관련 파일:** /GameData/Achievements.json
