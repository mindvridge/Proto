# 일일 콘텐츠 설계서 (Daily Content Design)

## 1. 개요

### 1.1 목적
- 유저의 일일 접속 유도 및 리텐션 증가
- 규칙적인 플레이 패턴 형성
- 꾸준한 보상 제공으로 성취감 부여
- 과금 유도 및 수익화 포인트 제공

### 1.2 핵심 철학
- **"백수 은둔형 외톨이"** 콘셉트에 맞춰 낮/밤 시간대 구분
- 부담 없는 플레이 타임 (하루 30분 내외)
- 놓쳐도 큰 손해 없는 구조 (FOMO 최소화)
- AFK/Idle 보상으로 접속하지 않아도 진행

---

## 2. 일일 출석 시스템

### 2.1 기본 출석 체크

#### 출석 보상표
| 일차 | 보상 |
|------|------|
| 1일 | 골드 10,000, 젬 10 |
| 2일 | 골드 20,000, 체력 물약 5개 |
| 3일 | 골드 30,000, 젬 20 |
| 4일 | 골드 50,000, 경험치 물약 5개 |
| 5일 | 골드 80,000, 젬 30, 스킬 포인트 5 |
| 6일 | 골드 100,000, 강화석 10개 |
| 7일 | 골드 200,000, 젬 100, 레어 장비 상자 1개 |
| 14일 | 골드 500,000, 젬 200, 에픽 장비 상자 1개 |
| 21일 | 골드 1,000,000, 젬 300, 스킬 포인트 20 |
| 28일 | 골드 2,000,000, 젬 500, 레전더리 장비 상자 1개 |
| 30일 | 골드 5,000,000, 젬 1000, 환생의 증표 1개 |

#### 연속 출석 보너스
```csharp
// 연속 출석일에 따른 추가 보너스
- 7일 연속: 골드 획득량 +10% (1일간)
- 14일 연속: 경험치 획득량 +10% (1일간)
- 21일 연속: 모든 능력치 +5% (1일간)
- 30일 연속: VIP 티켓 1개, 골드/경험치 +20% (3일간)
```

### 2.2 출석 보상 복구
```csharp
public class AttendanceManager : MonoBehaviour
{
    public static AttendanceManager Instance;

    private int consecutiveDays = 0;
    private int totalAttendanceDays = 0;
    private DateTime lastAttendanceDate;

    // 출석 체크
    public void CheckAttendance()
    {
        DateTime today = DateTime.Now.Date;

        if (lastAttendanceDate.Date == today)
        {
            Debug.Log("오늘은 이미 출석했습니다.");
            return;
        }

        // 연속 출석 체크
        if ((today - lastAttendanceDate).TotalDays == 1)
        {
            consecutiveDays++;
        }
        else if ((today - lastAttendanceDate).TotalDays > 1)
        {
            consecutiveDays = 1; // 연속 끊김
        }

        totalAttendanceDays++;
        lastAttendanceDate = today;

        GiveAttendanceReward(totalAttendanceDays, consecutiveDays);
        SaveAttendanceData();
    }

    private void GiveAttendanceReward(int totalDays, int consecutiveDays)
    {
        AttendanceReward reward = GetRewardForDay(totalDays);

        // 기본 보상
        PlayerManager.Instance.AddGold(reward.gold);
        PlayerManager.Instance.AddGems(reward.gems);

        if (reward.items != null)
        {
            foreach (var item in reward.items)
            {
                InventoryManager.Instance.AddItem(item.itemId, item.count);
            }
        }

        // 연속 출석 보너스
        ApplyConsecutiveBonus(consecutiveDays);

        UIManager.Instance.ShowAttendanceReward(reward);
    }

    // 출석 보상 복구 (젬 사용)
    public void RecoverMissedAttendance(int gemCost = 100)
    {
        if (PlayerManager.Instance.GetGems() < gemCost)
        {
            UIManager.Instance.ShowNotification("젬이 부족합니다!");
            return;
        }

        PlayerManager.Instance.SpendGems(gemCost);
        consecutiveDays++; // 연속 출석 복구

        UIManager.Instance.ShowNotification("출석이 복구되었습니다!");
    }
}
```

---

## 3. 일일 퀘스트 시스템

### 3.1 일일 퀘스트 목록

#### 기본 일일 퀘스트 (매일 5개 랜덤 제공)
| 퀘스트명 | 조건 | 보상 |
|---------|------|------|
| 아침 운동 | 몬스터 50마리 처치 | 골드 30,000, 경험치 5,000 |
| 던전 탐험 | 던전 3회 클리어 | 골드 50,000, 경험치 10,000 |
| 장비 강화 | 장비 5회 강화 | 골드 20,000, 강화석 5개 |
| 보스 사냥 | 보스 몬스터 1마리 처치 | 골드 80,000, 경험치 15,000 |
| 스킬 연습 | 스킬 50회 사용 | 골드 40,000, 스킬 포인트 3 |
| 골드 파밍 | 골드 100,000 획득 | 젬 20 |
| 아이템 수집 | 아이템 10개 획득 | 골드 30,000 |
| 연속 공격 | 크리티컬 30회 성공 | 골드 40,000, 경험치 8,000 |
| 은둔 생활 | 의심도 30 감소 | 골드 50,000, 젬 10 |
| 밤샘 작업 | 밤 시간대 30분 플레이 | 골드 60,000, 야행성 버프 |

#### 주간 퀘스트 (매주 3개 제공)
| 퀘스트명 | 조건 | 보상 |
|---------|------|------|
| 주간 전사 | 몬스터 500마리 처치 | 골드 500,000, 젬 100 |
| 주간 탐험가 | 던전 20회 클리어 | 골드 800,000, 젬 150 |
| 주간 성장 | 레벨 5 상승 | 골드 1,000,000, 스킬 포인트 20 |

### 3.2 일일 퀘스트 시스템 구현

```csharp
[System.Serializable]
public class DailyQuest
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public int targetValue;
    public int currentProgress;
    public List<QuestReward> rewards;
    public bool isCompleted;
    public bool isClaimed;

    public float GetProgress()
    {
        return (float)currentProgress / targetValue;
    }
}

public enum QuestType
{
    KillMonsters,
    ClearDungeons,
    EnhanceItems,
    DefeatBosses,
    UseSkills,
    EarnGold,
    CollectItems,
    CriticalHits,
    ReduceSuspicion,
    PlayAtNight
}

public class DailyQuestManager : MonoBehaviour
{
    public static DailyQuestManager Instance;

    public List<DailyQuest> activeDailyQuests;
    public List<DailyQuest> activeWeeklyQuests;

    private DateTime lastResetDate;
    private DateTime lastWeeklyResetDate;

    void Start()
    {
        CheckDailyReset();
        EventManager.OnMonsterKilled += OnMonsterKilled;
        EventManager.OnDungeonCleared += OnDungeonCleared;
        // ... 기타 이벤트 구독
    }

    // 일일 리셋 체크 (매일 오전 5시)
    public void CheckDailyReset()
    {
        DateTime now = DateTime.Now;
        DateTime resetTime = now.Date.AddHours(5); // 오전 5시

        if (now < resetTime)
            resetTime = resetTime.AddDays(-1);

        if (lastResetDate < resetTime)
        {
            ResetDailyQuests();
            lastResetDate = now;
        }
    }

    private void ResetDailyQuests()
    {
        activeDailyQuests = GenerateRandomDailyQuests(5);
        UIManager.Instance.ShowNotification("새로운 일일 퀘스트가 도착했습니다!");
    }

    private List<DailyQuest> GenerateRandomDailyQuests(int count)
    {
        List<DailyQuest> allQuests = GetAllDailyQuestTemplates();
        List<DailyQuest> selectedQuests = new List<DailyQuest>();

        // 랜덤으로 count개 선택
        while (selectedQuests.Count < count && allQuests.Count > 0)
        {
            int randomIndex = Random.Range(0, allQuests.Count);
            selectedQuests.Add(allQuests[randomIndex]);
            allQuests.RemoveAt(randomIndex);
        }

        return selectedQuests;
    }

    // 퀘스트 진행도 업데이트
    public void UpdateQuestProgress(QuestType type, int amount = 1)
    {
        foreach (var quest in activeDailyQuests)
        {
            if (quest.questType == type && !quest.isCompleted)
            {
                quest.currentProgress += amount;

                if (quest.currentProgress >= quest.targetValue)
                {
                    quest.isCompleted = true;
                    UIManager.Instance.ShowQuestComplete(quest);
                }
            }
        }

        // 주간 퀘스트도 동일하게 처리
        foreach (var quest in activeWeeklyQuests)
        {
            if (quest.questType == type && !quest.isCompleted)
            {
                quest.currentProgress += amount;

                if (quest.currentProgress >= quest.targetValue)
                {
                    quest.isCompleted = true;
                    UIManager.Instance.ShowQuestComplete(quest);
                }
            }
        }
    }

    // 보상 수령
    public void ClaimQuestReward(DailyQuest quest)
    {
        if (!quest.isCompleted || quest.isClaimed)
            return;

        foreach (var reward in quest.rewards)
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
            }
        }

        quest.isClaimed = true;
        UIManager.Instance.ShowRewardClaimed(quest.rewards);
    }

    // 이벤트 핸들러
    private void OnMonsterKilled(string monsterId)
    {
        UpdateQuestProgress(QuestType.KillMonsters, 1);
    }

    private void OnDungeonCleared(string dungeonId)
    {
        UpdateQuestProgress(QuestType.ClearDungeons, 1);
    }
}
```

---

## 4. 일일 던전 시스템

### 4.1 입장 제한 던전

#### 골드 던전 (황금 광산)
```
- 입장 제한: 하루 5회
- 소요 시간: 5분
- 보상: 대량의 골드 (레벨에 따라 증가)
- 난이도: 쉬움
```

#### 경험치 던전 (수련의 탑)
```
- 입장 제한: 하루 5회
- 소요 시간: 5분
- 보상: 대량의 경험치 (레벨에 따라 증가)
- 난이도: 쉬움
```

#### 강화석 던전 (광물 채굴장)
```
- 입장 제한: 하루 3회
- 소요 시간: 10분
- 보상: 강화석, 강화 보호권
- 난이도: 보통
```

#### 재료 던전 (마법 재료실)
```
- 입장 제한: 하루 3회
- 소요 시간: 10분
- 보상: 제작 재료 (드래곤 비늘, 마법 수정 등)
- 난이도: 보통
```

#### VIP 전용 던전 (비밀 금고)
```
- 입장 제한: 하루 1회 (VIP 전용)
- 소요 시간: 3분
- 보상: 젬, 레어 아이템, 특별 재료
- 난이도: 쉬움
```

### 4.2 입장권 시스템

```csharp
public class DungeonEntryManager : MonoBehaviour
{
    public static DungeonEntryManager Instance;

    [System.Serializable]
    public class DungeonEntry
    {
        public string dungeonId;
        public int maxDailyEntries;
        public int remainingEntries;
        public DateTime lastResetDate;
        public int ticketCost; // 추가 입장권 가격 (젬)
    }

    private Dictionary<string, DungeonEntry> dungeonEntries;

    // 던전 입장 체크
    public bool CanEnterDungeon(string dungeonId)
    {
        CheckDailyReset(dungeonId);

        if (!dungeonEntries.ContainsKey(dungeonId))
            return true; // 제한 없는 던전

        DungeonEntry entry = dungeonEntries[dungeonId];
        return entry.remainingEntries > 0;
    }

    // 던전 입장
    public bool EnterDungeon(string dungeonId)
    {
        if (!CanEnterDungeon(dungeonId))
        {
            UIManager.Instance.ShowNotification("오늘의 입장 횟수를 모두 사용했습니다!");

            // 추가 입장권 구매 제안
            ShowTicketPurchasePopup(dungeonId);
            return false;
        }

        if (dungeonEntries.ContainsKey(dungeonId))
        {
            dungeonEntries[dungeonId].remainingEntries--;
        }

        return true;
    }

    // 추가 입장권 구매
    public void PurchaseExtraEntry(string dungeonId)
    {
        DungeonEntry entry = dungeonEntries[dungeonId];

        if (PlayerManager.Instance.GetGems() < entry.ticketCost)
        {
            UIManager.Instance.ShowNotification("젬이 부족합니다!");
            return;
        }

        PlayerManager.Instance.SpendGems(entry.ticketCost);
        entry.remainingEntries++;

        UIManager.Instance.ShowNotification("입장권을 구매했습니다!");
    }

    // 일일 리셋
    private void CheckDailyReset(string dungeonId)
    {
        if (!dungeonEntries.ContainsKey(dungeonId))
            return;

        DungeonEntry entry = dungeonEntries[dungeonId];
        DateTime now = DateTime.Now;
        DateTime resetTime = now.Date.AddHours(5); // 오전 5시

        if (now < resetTime)
            resetTime = resetTime.AddDays(-1);

        if (entry.lastResetDate < resetTime)
        {
            entry.remainingEntries = entry.maxDailyEntries;
            entry.lastResetDate = now;
        }
    }
}
```

---

## 5. 일일 상점 시스템

### 5.1 일일 특가 상품

#### 특가 상품 목록 (매일 3~5개 랜덤)
```csharp
[System.Serializable]
public class DailyDeal
{
    public string itemId;
    public int originalPrice;
    public int discountPrice;
    public int discountPercent;
    public int stock; // 구매 제한
    public DateTime dealEndTime;
}
```

| 등급 | 상품 | 원가 | 할인가 | 할인율 | 재고 |
|------|------|------|--------|--------|------|
| 일반 | 체력 물약 10개 | 10,000G | 7,000G | 30% | 5 |
| 레어 | 강화석 20개 | 50,000G | 35,000G | 30% | 3 |
| 에픽 | 경험치 물약 5개 | 100,000G | 60,000G | 40% | 2 |
| 레전더리 | 에픽 장비 상자 | 500,000G | 300,000G | 40% | 1 |
| 신화 | 환생의 증표 | 2,000,000G | 1,000,000G | 50% | 1 |

#### 젬 특가 패키지
| 패키지 | 내용 | 젬 가격 |
|--------|------|---------|
| 스타터 패키지 | 골드 100,000 + 경험치 물약 3개 | 50 |
| 성장 패키지 | 강화석 30개 + 강화 보호권 5개 | 100 |
| 프리미엄 패키지 | 레어 장비 상자 2개 + 스킬 포인트 10 | 200 |
| VIP 패키지 | VIP 티켓 1개 + 에픽 장비 상자 1개 | 500 |

### 5.2 일일 상점 구현

```csharp
public class DailyShopManager : MonoBehaviour
{
    public static DailyShopManager Instance;

    private List<DailyDeal> currentDeals;
    private DateTime lastRefreshTime;

    void Start()
    {
        RefreshDailyDeals();
    }

    // 일일 특가 갱신
    public void RefreshDailyDeals()
    {
        DateTime now = DateTime.Now;
        DateTime nextReset = now.Date.AddDays(1).AddHours(5); // 다음날 오전 5시

        if (now >= nextReset || currentDeals == null)
        {
            currentDeals = GenerateRandomDeals(5);
            lastRefreshTime = now;

            UIManager.Instance.ShowNotification("일일 특가가 갱신되었습니다!");
        }
    }

    private List<DailyDeal> GenerateRandomDeals(int count)
    {
        List<DailyDeal> deals = new List<DailyDeal>();

        // 등급별 가중치
        int commonWeight = 50;
        int rareWeight = 30;
        int epicWeight = 15;
        int legendaryWeight = 4;
        int mythicWeight = 1;

        for (int i = 0; i < count; i++)
        {
            DailyDeal deal = CreateRandomDeal();
            deals.Add(deal);
        }

        return deals;
    }

    // 특가 상품 구매
    public void PurchaseDailyDeal(DailyDeal deal, bool useGems = false)
    {
        if (deal.stock <= 0)
        {
            UIManager.Instance.ShowNotification("품절된 상품입니다!");
            return;
        }

        if (useGems)
        {
            if (PlayerManager.Instance.GetGems() < deal.discountPrice)
            {
                UIManager.Instance.ShowNotification("젬이 부족합니다!");
                return;
            }
            PlayerManager.Instance.SpendGems(deal.discountPrice);
        }
        else
        {
            if (PlayerManager.Instance.GetGold() < deal.discountPrice)
            {
                UIManager.Instance.ShowNotification("골드가 부족합니다!");
                return;
            }
            PlayerManager.Instance.SpendGold(deal.discountPrice);
        }

        // 아이템 지급
        InventoryManager.Instance.AddItem(deal.itemId, 1);
        deal.stock--;

        UIManager.Instance.ShowNotification("구매 완료!");
    }
}
```

---

## 6. 시간대별 보너스 시스템

### 6.1 낮/밤 시스템 ("백수 은둔형 외톨이" 콘셉트)

#### 시간대 구분
```
낮 시간: 06:00 ~ 22:00
- 의심도 증가 속도 +50%
- 가족 NPC 등장 확률 증가
- 골드 획득량 기본

밤 시간: 22:00 ~ 06:00
- 의심도 증가 속도 -50%
- 골드 획득량 +30%
- 경험치 획득량 +30%
- 레어 아이템 드롭률 +20%
- "야행성" 버프 활성화
```

### 6.2 특별 시간대 이벤트

#### 골든 타임 (매일 3회, 각 1시간)
```
12:00 ~ 13:00: 점심 시간 (골드 획득량 +100%)
18:00 ~ 19:00: 저녁 시간 (경험치 획득량 +100%)
00:00 ~ 01:00: 심야 시간 (모든 보상 +50%, 보스 등장률 +30%)
```

### 6.3 시간대 시스템 구현

```csharp
public enum TimeOfDay
{
    Morning,    // 06:00 ~ 12:00
    Afternoon,  // 12:00 ~ 18:00
    Evening,    // 18:00 ~ 22:00
    Night,      // 22:00 ~ 06:00
    GoldenHour  // 특별 시간대
}

public class TimeSystemManager : MonoBehaviour
{
    public static TimeSystemManager Instance;

    public TimeOfDay currentTimeOfDay;
    private DateTime lastCheckTime;

    // 현재 시간대 체크
    void Update()
    {
        if ((DateTime.Now - lastCheckTime).TotalMinutes >= 1)
        {
            UpdateTimeOfDay();
            lastCheckTime = DateTime.Now;
        }
    }

    private void UpdateTimeOfDay()
    {
        int hour = DateTime.Now.Hour;
        TimeOfDay newTimeOfDay;

        // 골든 타임 체크
        if (IsGoldenHour(hour))
        {
            newTimeOfDay = TimeOfDay.GoldenHour;
        }
        else if (hour >= 6 && hour < 12)
        {
            newTimeOfDay = TimeOfDay.Morning;
        }
        else if (hour >= 12 && hour < 18)
        {
            newTimeOfDay = TimeOfDay.Afternoon;
        }
        else if (hour >= 18 && hour < 22)
        {
            newTimeOfDay = TimeOfDay.Evening;
        }
        else
        {
            newTimeOfDay = TimeOfDay.Night;
        }

        if (newTimeOfDay != currentTimeOfDay)
        {
            OnTimeOfDayChanged(newTimeOfDay);
            currentTimeOfDay = newTimeOfDay;
        }
    }

    private bool IsGoldenHour(int hour)
    {
        return hour == 12 || hour == 18 || hour == 0;
    }

    private void OnTimeOfDayChanged(TimeOfDay newTime)
    {
        // 시간대 변경 알림
        string message = GetTimeChangeMessage(newTime);
        UIManager.Instance.ShowNotification(message);

        // 보너스 적용
        ApplyTimeBonus(newTime);

        // 이벤트 발생
        EventManager.TriggerTimeOfDayChanged(newTime);
    }

    // 시간대별 보너스 적용
    private void ApplyTimeBonus(TimeOfDay timeOfDay)
    {
        PlayerStats stats = PlayerManager.Instance.GetStats();

        // 기존 시간 보너스 제거
        stats.RemoveTimeBonus();

        switch (timeOfDay)
        {
            case TimeOfDay.Night:
                stats.goldMultiplier *= 1.3f;
                stats.expMultiplier *= 1.3f;
                stats.dropRateBonus += 0.2f;
                stats.suspicionIncreaseRate *= 0.5f;
                break;

            case TimeOfDay.GoldenHour:
                int hour = DateTime.Now.Hour;
                if (hour == 12)
                {
                    stats.goldMultiplier *= 2.0f;
                }
                else if (hour == 18)
                {
                    stats.expMultiplier *= 2.0f;
                }
                else if (hour == 0)
                {
                    stats.goldMultiplier *= 1.5f;
                    stats.expMultiplier *= 1.5f;
                    stats.bossSpawnRate *= 1.3f;
                }
                break;

            case TimeOfDay.Morning:
            case TimeOfDay.Afternoon:
            case TimeOfDay.Evening:
                stats.suspicionIncreaseRate *= 1.5f;
                break;
        }
    }

    private string GetTimeChangeMessage(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Morning:
                return "아침이 밝았습니다. 가족들이 깨어났어요!";
            case TimeOfDay.Afternoon:
                return "오후입니다. 조심해서 활동하세요.";
            case TimeOfDay.Evening:
                return "저녁입니다. 곧 밤이 옵니다!";
            case TimeOfDay.Night:
                return "밤이 되었습니다! 은둔형 외톨이의 시간! 보너스 +30%";
            case TimeOfDay.GoldenHour:
                return "골든 타임 시작! 1시간 동안 특별 보너스!";
            default:
                return "";
        }
    }
}
```

---

## 7. AFK/Idle 보상 시스템

### 7.1 오프라인 보상

```csharp
public class OfflineRewardManager : MonoBehaviour
{
    public static OfflineRewardManager Instance;

    private DateTime lastLoginTime;
    private int maxOfflineHours = 12; // 최대 12시간까지 적용

    // 로그인 시 오프라인 보상 계산
    public void CalculateOfflineRewards()
    {
        DateTime now = DateTime.Now;
        TimeSpan offlineTime = now - lastLoginTime;

        // 최대 시간 제한
        int offlineHours = Mathf.Min((int)offlineTime.TotalHours, maxOfflineHours);

        if (offlineHours > 0)
        {
            GiveOfflineRewards(offlineHours);
        }

        lastLoginTime = now;
    }

    private void GiveOfflineRewards(int hours)
    {
        PlayerStats stats = PlayerManager.Instance.GetStats();

        // 시간당 보상 계산 (유저 레벨과 장비에 따라 변동)
        int goldPerHour = CalculateIdleGoldPerHour(stats);
        int expPerHour = CalculateIdleExpPerHour(stats);

        int totalGold = goldPerHour * hours;
        int totalExp = expPerHour * hours;

        // 보상 지급
        PlayerManager.Instance.AddGold(totalGold);
        PlayerManager.Instance.AddExp(totalExp);

        // VIP라면 보상 2배
        if (PlayerManager.Instance.IsVIP())
        {
            totalGold *= 2;
            totalExp *= 2;
        }

        // UI 표시
        UIManager.Instance.ShowOfflineRewardPopup(hours, totalGold, totalExp);
    }

    private int CalculateIdleGoldPerHour(PlayerStats stats)
    {
        // 기본 시간당 골드: 레벨 * 1000
        int baseGold = stats.level * 1000;

        // 장비 보너스 적용
        baseGold = (int)(baseGold * stats.goldMultiplier);

        // 스킬 "유휴 소득" 보너스 적용
        float idleIncomeBonus = SkillTreeManager.Instance.GetSkillEffect("growth_005");
        baseGold = (int)(baseGold * (1 + idleIncomeBonus));

        return baseGold;
    }

    private int CalculateIdleExpPerHour(PlayerStats stats)
    {
        int baseExp = stats.level * 500;
        baseExp = (int)(baseExp * stats.expMultiplier);

        float idleExpBonus = SkillTreeManager.Instance.GetSkillEffect("growth_006");
        baseExp = (int)(baseExp * (1 + idleExpBonus));

        return baseExp;
    }

    // 오프라인 보상 2배 받기 (광고 시청)
    public void DoubleOfflineReward()
    {
        // 광고 시청 후 보상 2배
        AdManager.Instance.ShowRewardedAd(() =>
        {
            // 이미 지급된 보상을 추가로 한 번 더 지급
            UIManager.Instance.ShowNotification("오프라인 보상이 2배가 되었습니다!");
        });
    }
}
```

### 7.2 자동 전투 보상

```csharp
public class AutoBattleManager : MonoBehaviour
{
    public static AutoBattleManager Instance;

    private bool isAutoBattleActive = false;
    private DateTime autoBattleStartTime;
    private int maxAutoBattleMinutes = 30; // 최대 30분

    // 자동 전투 시작
    public void StartAutoBattle()
    {
        // "자동 전투" 스킬 필요
        if (!SkillTreeManager.Instance.IsSkillLearned("combat_008"))
        {
            UIManager.Instance.ShowNotification("자동 전투 스킬이 필요합니다!");
            return;
        }

        isAutoBattleActive = true;
        autoBattleStartTime = DateTime.Now;

        UIManager.Instance.ShowNotification("자동 전투를 시작합니다!");
    }

    // 자동 전투 종료 및 보상 수령
    public void EndAutoBattle()
    {
        if (!isAutoBattleActive)
            return;

        TimeSpan battleTime = DateTime.Now - autoBattleStartTime;
        int minutes = Mathf.Min((int)battleTime.TotalMinutes, maxAutoBattleMinutes);

        GiveAutoBattleRewards(minutes);

        isAutoBattleActive = false;
    }

    private void GiveAutoBattleRewards(int minutes)
    {
        PlayerStats stats = PlayerManager.Instance.GetStats();

        // 자동 전투 효율 (스킬 레벨에 따라 증가)
        float efficiency = SkillTreeManager.Instance.GetSkillValue("combat_008");

        int goldEarned = (int)(stats.level * 100 * minutes * efficiency);
        int expEarned = (int)(stats.level * 50 * minutes * efficiency);

        PlayerManager.Instance.AddGold(goldEarned);
        PlayerManager.Instance.AddExp(expEarned);

        UIManager.Instance.ShowNotification($"자동 전투 완료! 골드 {goldEarned}, 경험치 {expEarned} 획득!");
    }
}
```

---

## 8. 일일 이벤트

### 8.1 요일별 이벤트

| 요일 | 이벤트 | 내용 |
|------|--------|------|
| 월요일 | 골드의 날 | 골드 획득량 +50% |
| 화요일 | 경험치의 날 | 경험치 획득량 +50% |
| 수요일 | 강화의 날 | 강화 성공률 +10% |
| 목요일 | 재료의 날 | 재료 드롭률 +50% |
| 금요일 | 보스의 날 | 보스 등장률 +50%, 보스 보상 +30% |
| 토요일 | 월드 보스 | 전 서버 월드 보스 이벤트 (20시) |
| 일요일 | 휴식의 날 | 의심도 감소 속도 +100% |

### 8.2 시즌 이벤트

```csharp
public class SeasonalEventManager : MonoBehaviour
{
    public static SeasonalEventManager Instance;

    // 시즌 이벤트 체크
    public void CheckSeasonalEvents()
    {
        DateTime now = DateTime.Now;

        // 크리스마스 (12/24 ~ 12/26)
        if (now.Month == 12 && now.Day >= 24 && now.Day <= 26)
        {
            ActivateChristmasEvent();
        }

        // 새해 (1/1 ~ 1/3)
        if (now.Month == 1 && now.Day >= 1 && now.Day <= 3)
        {
            ActivateNewYearEvent();
        }

        // 여름 이벤트 (7/1 ~ 8/31)
        if (now.Month >= 7 && now.Month <= 8)
        {
            ActivateSummerEvent();
        }
    }

    private void ActivateChristmasEvent()
    {
        // 특별 몬스터 등장 (눈사람, 산타 슬라임 등)
        // 특별 보상 (크리스마스 장비)
        UIManager.Instance.ShowNotification("크리스마스 이벤트 진행 중!");
    }
}
```

---

## 9. UI/UX 설계

### 9.1 일일 콘텐츠 대시보드

```
[일일 콘텐츠 화면]

┌─────────────────────────────────┐
│  📅 일일 콘텐츠                    │
├─────────────────────────────────┤
│                                  │
│  ✅ 출석 체크     [완료] ✓        │
│     연속 7일 - 보너스 활성화!      │
│                                  │
│  📋 일일 퀘스트   [3/5 완료]      │
│     • 몬스터 50마리 처치 ✓        │
│     • 던전 3회 클리어 ✓           │
│     • 장비 5회 강화 ✓             │
│     • 보스 처치 (0/1)            │
│     • 골드 100,000 획득 (0/1)    │
│                                  │
│  🏰 일일 던전     [8/15 입장]    │
│     골드 던전    3/5 🎫          │
│     경험치 던전   2/5 🎫          │
│     강화석 던전   2/3 🎫          │
│     재료 던전    1/3 🎫          │
│                                  │
│  🛒 일일 특가     [새로고침까지]  │
│     5시간 32분 남음               │
│                                  │
│  🌙 현재 시간대: 밤               │
│     보너스: 골드 +30%, 경험치 +30%│
│                                  │
│  ⏰ 다음 골든타임: 00:00 (4시간)  │
│                                  │
└─────────────────────────────────┘
```

### 9.2 알림 시스템

```csharp
public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    // 일일 리셋 알림
    public void ScheduleDailyResetNotification()
    {
        DateTime resetTime = DateTime.Now.Date.AddDays(1).AddHours(5);
        ScheduleNotification(resetTime, "일일 콘텐츠가 초기화되었습니다!", "지금 접속하여 보상을 받아가세요!");
    }

    // 골든타임 알림
    public void ScheduleGoldenHourNotification()
    {
        int[] goldenHours = { 12, 18, 0 };

        foreach (int hour in goldenHours)
        {
            DateTime goldenTime = DateTime.Now.Date.AddHours(hour);
            if (goldenTime < DateTime.Now)
                goldenTime = goldenTime.AddDays(1);

            ScheduleNotification(goldenTime, "골든타임 시작!", "1시간 동안 특별 보너스를 받으세요!");
        }
    }

    // 던전 입장권 회복 알림
    public void ScheduleDungeonResetNotification()
    {
        DateTime resetTime = DateTime.Now.Date.AddDays(1).AddHours(5);
        ScheduleNotification(resetTime, "던전 입장권이 회복되었습니다!", "오늘의 던전에 도전하세요!");
    }
}
```

---

## 10. 수익화 포인트

### 10.1 유료 아이템

| 아이템 | 가격 | 효과 |
|--------|------|------|
| 던전 입장권 +1 | 50 젬 | 던전 추가 입장 1회 |
| 던전 입장권 +5 | 200 젬 | 던전 추가 입장 5회 |
| 출석 복구권 | 100 젬 | 연속 출석 복구 |
| 일일 퀘스트 즉시 완료 | 150 젬 | 퀘스트 1개 즉시 완료 |
| 오프라인 보상 2배 | 광고 시청 | 오프라인 보상 2배 |
| 골든타임 연장권 | 300 젬 | 골든타임 1시간 연장 |
| VIP 패스 (30일) | 9,900원 | 일일 보너스, VIP 던전 입장 |

### 10.2 VIP 혜택

```
VIP 등급별 혜택:

VIP 1 (4,900원/월)
- 일일 젬 50개 지급
- VIP 던전 입장 가능
- 상점 할인 5%

VIP 2 (9,900원/월)
- 일일 젬 100개 지급
- VIP 던전 입장 2회
- 상점 할인 10%
- 오프라인 보상 +50%

VIP 3 (19,900원/월)
- 일일 젬 200개 지급
- VIP 던전 입장 3회
- 상점 할인 20%
- 오프라인 보상 +100%
- 전용 VIP 장비 획득 가능
```

---

## 11. 데이터 저장

```csharp
[System.Serializable]
public class DailyContentSaveData
{
    public DateTime lastLoginTime;
    public int consecutiveAttendanceDays;
    public int totalAttendanceDays;
    public DateTime lastAttendanceDate;

    public List<DailyQuest> activeDailyQuests;
    public List<DailyQuest> activeWeeklyQuests;
    public DateTime lastQuestResetDate;

    public Dictionary<string, DungeonEntry> dungeonEntries;

    public List<DailyDeal> currentShopDeals;
    public DateTime lastShopResetDate;

    public TimeOfDay currentTimeOfDay;
}

public class DailyContentDataManager : MonoBehaviour
{
    public static void SaveDailyContentData()
    {
        DailyContentSaveData saveData = new DailyContentSaveData
        {
            lastLoginTime = DateTime.Now,
            consecutiveAttendanceDays = AttendanceManager.Instance.consecutiveDays,
            totalAttendanceDays = AttendanceManager.Instance.totalAttendanceDays,
            lastAttendanceDate = AttendanceManager.Instance.lastAttendanceDate,

            activeDailyQuests = DailyQuestManager.Instance.activeDailyQuests,
            activeWeeklyQuests = DailyQuestManager.Instance.activeWeeklyQuests,
            lastQuestResetDate = DailyQuestManager.Instance.lastResetDate,

            dungeonEntries = DungeonEntryManager.Instance.dungeonEntries,

            currentShopDeals = DailyShopManager.Instance.currentDeals,
            lastShopResetDate = DailyShopManager.Instance.lastRefreshTime,

            currentTimeOfDay = TimeSystemManager.Instance.currentTimeOfDay
        };

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("DailyContentData", json);
        PlayerPrefs.Save();
    }

    public static void LoadDailyContentData()
    {
        string json = PlayerPrefs.GetString("DailyContentData", "");
        if (string.IsNullOrEmpty(json))
            return;

        DailyContentSaveData saveData = JsonUtility.FromJson<DailyContentSaveData>(json);

        // 데이터 복원
        AttendanceManager.Instance.consecutiveDays = saveData.consecutiveAttendanceDays;
        AttendanceManager.Instance.totalAttendanceDays = saveData.totalAttendanceDays;
        AttendanceManager.Instance.lastAttendanceDate = saveData.lastAttendanceDate;

        DailyQuestManager.Instance.activeDailyQuests = saveData.activeDailyQuests;
        DailyQuestManager.Instance.activeWeeklyQuests = saveData.activeWeeklyQuests;
        DailyQuestManager.Instance.lastResetDate = saveData.lastQuestResetDate;

        DungeonEntryManager.Instance.dungeonEntries = saveData.dungeonEntries;

        DailyShopManager.Instance.currentDeals = saveData.currentShopDeals;
        DailyShopManager.Instance.lastRefreshTime = saveData.lastShopResetDate;

        TimeSystemManager.Instance.currentTimeOfDay = saveData.currentTimeOfDay;

        // 오프라인 보상 계산
        OfflineRewardManager.Instance.lastLoginTime = saveData.lastLoginTime;
        OfflineRewardManager.Instance.CalculateOfflineRewards();
    }
}
```

---

## 12. 밸런싱 가이드

### 12.1 일일 보상 총량

```
평균 플레이 시간: 30분
목표 일일 보상:
- 골드: 500,000 ~ 1,000,000 (레벨에 따라 변동)
- 경험치: 레벨업에 필요한 경험치의 30~50%
- 젬: 50~100개 (출석, 퀘스트, 던전 포함)
- 강화석: 10~30개
```

### 12.2 과금 유도 밸런싱

```
무과금 유저: 하루 30분 플레이로 안정적인 성장
소과금 유저: VIP 혜택으로 성장 속도 1.5배
중과금 유저: 던전 입장권 추가 구매로 성장 속도 2배
고과금 유저: 모든 혜택 구매로 성장 속도 3배

목표: 무과금으로도 즐길 수 있지만, 과금하면 더 빠르게 성장
```

---

## 13. 개발 우선순위

### Phase 1 (핵심 기능)
1. 출석 체크 시스템
2. 일일 퀘스트 시스템
3. 시간대별 보너스
4. 오프라인 보상

### Phase 2 (확장 기능)
1. 일일 던전 입장 제한
2. 일일 상점 특가
3. 요일별 이벤트

### Phase 3 (고급 기능)
1. 자동 전투 시스템
2. 시즌 이벤트
3. VIP 시스템
4. 푸시 알림

---

## 14. 참고 자료

- 유사 게임 일일 콘텐츠 분석 (방치형 RPG)
- KPI 목표: DAU, 리텐션, ARPPU
- A/B 테스트 계획: 보상량, 난이도 조절

---

**작성일:** 2025-11-13
**버전:** 1.0
**작성자:** AI Assistant
