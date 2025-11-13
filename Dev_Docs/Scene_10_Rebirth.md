# Scene 10: 환생 (Rebirth/Prestige) - 개발 명세서

## 📋 씬 개요

**씬 이름**: RebirthScene
**씬 목적**: 환생 시스템, 영구 강화, 무한 성장 메커니즘
**예상 개발 시간**: 8일
**우선순위**: 최상 (클리커 게임 핵심 메커니즘)

---

## 🎨 UI 구성

```
┌─────────────────────────────────┐
│     ✨ 환 생 ✨                  │
│                                 │
│  현재 레벨: 999                  │
│  달성한 골드: 999조              │
│                                 │
│  ┌─────────────────────────┐   │
│  │   💎 획득 가능            │   │
│  │   환생 포인트: 1,250     │   │
│  │                          │   │
│  │  영구 능력치 증가량       │   │
│  │  • 클릭 파워 +125%       │   │
│  │  • 골드 획득 +125%       │   │
│  │  • 경험치 획득 +125%     │   │
│  └─────────────────────────┘   │
│                                 │
│  ⚠️ 경고: 모든 진행도가 초기화   │
│  되지만 환생 보너스는 영구 유지  │
│                                 │
│  [환생하기]  [취소]             │
└─────────────────────────────────┘
```

---

## 🔧 핵심 시스템

### RebirthManager

```csharp
public class RebirthManager : MonoBehaviour
{
    public static RebirthManager Instance;

    [Header("Rebirth Status")]
    public int rebirthCount = 0;
    public long totalRebirthPoints = 0;
    public long currentRebirthPoints = 0;

    [Header("Rebirth Calculation")]
    public long baseRebirthRequirement = 1000000000; // 10억 골드
    public float rebirthPointMultiplier = 0.001f; // 골드 -> 포인트 변환율

    [Header("Bonus Multipliers")]
    public float clickPowerBonus = 1.0f;
    public float goldGainBonus = 1.0f;
    public float expGainBonus = 1.0f;
    public float offlineGainBonus = 1.0f;

    [Header("Rebirth Perks")]
    public List<RebirthPerk> unlockedPerks = new List<RebirthPerk>();

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
        }
    }

    private void Start()
    {
        CalculateRebirthBonuses();
    }

    public bool CanRebirth()
    {
        // 최소 조건 체크
        long currentGold = GameManager.Instance.currentGold;
        int currentLevel = GameManager.Instance.playerLevel;

        // 골드 또는 레벨 조건 중 하나만 만족하면 가능
        bool goldRequirement = currentGold >= baseRebirthRequirement;
        bool levelRequirement = currentLevel >= 100;

        return goldRequirement || levelRequirement;
    }

    public long CalculateRebirthPointsGain()
    {
        long currentGold = GameManager.Instance.currentGold;
        int currentLevel = GameManager.Instance.playerLevel;

        // 골드 기반 포인트
        long goldPoints = (long)(currentGold * rebirthPointMultiplier);

        // 레벨 기반 보너스
        long levelBonus = currentLevel * 100;

        // 던전 클리어 보너스
        int clearedDungeons = DungeonManager.Instance.GetClearedDungeonCount();
        long dungeonBonus = clearedDungeons * 500;

        // 업적 보너스
        int achievementCount = AchievementManager.Instance.GetCompletedCount();
        long achievementBonus = achievementCount * 200;

        long totalPoints = goldPoints + levelBonus + dungeonBonus + achievementBonus;

        // 환생 횟수에 따른 페널티 (10회마다 10% 감소)
        float penalty = 1.0f - (rebirthCount / 10 * 0.1f);
        penalty = Mathf.Max(penalty, 0.5f); // 최소 50%

        return (long)(totalPoints * penalty);
    }

    public void ShowRebirthConfirmation()
    {
        if (!CanRebirth())
        {
            ShowMessage("환생 조건을 만족하지 못했습니다!");
            ShowMessage($"필요 조건: 골드 {baseRebirthRequirement:N0} 또는 레벨 100");
            return;
        }

        long pointsGain = CalculateRebirthPointsGain();

        // 확인 팝업
        UIManager.Instance.ShowConfirmationPopup(
            "환생 확인",
            $"환생하시겠습니까?\n\n" +
            $"획득 포인트: {pointsGain:N0}\n" +
            $"모든 진행도가 초기화됩니다!\n\n" +
            $"영구 보너스가 증가합니다.",
            OnRebirthConfirmed,
            OnRebirthCancelled
        );
    }

    private void OnRebirthConfirmed()
    {
        // 환생 포인트 획득
        long pointsGain = CalculateRebirthPointsGain();
        currentRebirthPoints += pointsGain;
        totalRebirthPoints += pointsGain;
        rebirthCount++;

        // 환생 연출
        StartCoroutine(RebirthCutscene());

        // 게임 리셋
        ResetGameProgress();

        // 보너스 재계산
        CalculateRebirthBonuses();

        // 환생 기록 저장
        RecordRebirthHistory();

        // 업적 체크
        AchievementManager.Instance.CheckRebirthAchievements(rebirthCount);
    }

    private void OnRebirthCancelled()
    {
        // 취소
    }

    private IEnumerator RebirthCutscene()
    {
        // 화면 페이드 아웃
        yield return StartCoroutine(UIManager.Instance.FadeOut(1.0f));

        // 환생 이펙트
        ParticleSystem rebirthEffect = Instantiate(rebirthEffectPrefab);
        rebirthEffect.Play();

        // 환생 메시지
        UIManager.Instance.ShowBigMessage("환생 완료!", 2.0f);

        yield return new WaitForSeconds(2.0f);

        // 화면 페이드 인
        yield return StartCoroutine(UIManager.Instance.FadeIn(1.0f));

        // 새 게임 시작
        SceneTransition.Instance.LoadScene("MainGameScene");
    }

    private void ResetGameProgress()
    {
        GameManager gm = GameManager.Instance;

        // 레벨 초기화
        gm.playerLevel = 1;
        gm.currentExp = 0;

        // 골드 초기화 (환생 포인트로 변환됨)
        gm.currentGold = 0;

        // 인벤토리 초기화 (일부 특수 아이템은 유지 가능)
        InventoryManager.Instance.ClearInventory(keepSpecialItems: true);

        // 장비 해제
        EquipmentManager.Instance.UnequipAll();

        // 스킬 초기화 (일부 코어 스킬은 유지)
        SkillTreeManager.Instance.ResetSkillTree(keepCoreSkills: true);

        // 던전 진행도 초기화
        DungeonManager.Instance.ResetProgress();

        // 의심도 초기화
        SuspicionManager.Instance.currentSuspicion = 0;

        // 시간 초기화
        TimeManager.Instance.ResetTime();

        // 단, 환생 보너스와 업적은 유지!
    }

    private void CalculateRebirthBonuses()
    {
        // 기본 보너스: 환생 포인트 1당 0.1% 증가
        clickPowerBonus = 1.0f + (totalRebirthPoints * 0.001f);
        goldGainBonus = 1.0f + (totalRebirthPoints * 0.001f);
        expGainBonus = 1.0f + (totalRebirthPoints * 0.001f);
        offlineGainBonus = 1.0f + (totalRebirthPoints * 0.0005f);

        // 환생 횟수 보너스
        clickPowerBonus += rebirthCount * 0.05f;
        goldGainBonus += rebirthCount * 0.05f;

        // 특전(Perk) 보너스
        foreach (var perk in unlockedPerks)
        {
            ApplyPerkBonus(perk);
        }

        // 게임 매니저에 반영
        ApplyBonusesToGame();
    }

    private void ApplyBonusesToGame()
    {
        GameManager gm = GameManager.Instance;

        gm.clickPowerMultiplier = clickPowerBonus;
        gm.goldGainMultiplier = goldGainBonus;
        gm.expGainMultiplier = expGainBonus;
        gm.offlineGainMultiplier = offlineGainBonus;

        Debug.Log($"[Rebirth] Bonuses Applied - Click: {clickPowerBonus:F2}x, Gold: {goldGainBonus:F2}x");
    }

    public void SpendRebirthPoints(long amount)
    {
        if (currentRebirthPoints >= amount)
        {
            currentRebirthPoints -= amount;
            SaveManager.Instance.SaveGame();
        }
    }

    private void RecordRebirthHistory()
    {
        RebirthHistory history = new RebirthHistory
        {
            rebirthNumber = rebirthCount,
            timestamp = System.DateTime.Now.ToString(),
            pointsGained = CalculateRebirthPointsGain(),
            levelReached = GameManager.Instance.playerLevel,
            goldReached = GameManager.Instance.currentGold
        };

        // 히스토리 저장
        SaveManager.Instance.AddRebirthHistory(history);
    }
}

[System.Serializable]
public class RebirthHistory
{
    public int rebirthNumber;
    public string timestamp;
    public long pointsGained;
    public int levelReached;
    public long goldReached;
}
```

---

## 🎁 환생 특전 (Perks)

### RebirthPerkManager

```csharp
[System.Serializable]
public class RebirthPerk
{
    public string perkId;
    public string perkName;
    public string description;
    public long cost;
    public int maxLevel;
    public int currentLevel;
    public PerkType type;
    public float effectValue;
}

public enum PerkType
{
    ClickPower,
    GoldGain,
    ExpGain,
    CriticalChance,
    CriticalDamage,
    IdleGain,
    OfflineGain,
    StartingGold,
    StartingLevel,
    SkillPointGain,
    ReduceSuspicion,
    FasterTime,
    BetterLoot,
    MoreSkillPoints
}

public class RebirthPerkManager : MonoBehaviour
{
    public static RebirthPerkManager Instance;

    public List<RebirthPerk> allPerks = new List<RebirthPerk>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePerks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePerks()
    {
        allPerks.Add(new RebirthPerk
        {
            perkId = "click_power_1",
            perkName = "강화된 클릭",
            description = "클릭 파워가 영구적으로 10% 증가합니다.",
            cost = 100,
            maxLevel = 10,
            type = PerkType.ClickPower,
            effectValue = 0.1f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "gold_gain_1",
            perkName = "황금 손가락",
            description = "골드 획득량이 영구적으로 10% 증가합니다.",
            cost = 100,
            maxLevel = 10,
            type = PerkType.GoldGain,
            effectValue = 0.1f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "starting_gold",
            perkName = "시작 자본",
            description = "환생 시 골드를 가지고 시작합니다.",
            cost = 500,
            maxLevel = 5,
            type = PerkType.StartingGold,
            effectValue = 10000f // 레벨당 10,000 골드
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "starting_level",
            perkName = "빠른 시작",
            description = "환생 시 레벨 5로 시작합니다.",
            cost = 1000,
            maxLevel = 5,
            type = PerkType.StartingLevel,
            effectValue = 5f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "critical_chance",
            perkName = "예리한 감각",
            description = "치명타 확률이 2% 증가합니다.",
            cost = 200,
            maxLevel = 10,
            type = PerkType.CriticalChance,
            effectValue = 0.02f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "offline_gain",
            perkName = "효율적인 방치",
            description = "오프라인 수익이 20% 증가합니다.",
            cost = 300,
            maxLevel = 5,
            type = PerkType.OfflineGain,
            effectValue = 0.2f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "reduce_suspicion",
            perkName = "완벽한 위장",
            description = "의심도 증가율이 5% 감소합니다.",
            cost = 400,
            maxLevel = 10,
            type = PerkType.ReduceSuspicion,
            effectValue = 0.05f
        });

        allPerks.Add(new RebirthPerk
        {
            perkId = "skill_points",
            perkName = "잠재된 재능",
            description = "환생 시 스킬 포인트를 추가로 받습니다.",
            cost = 800,
            maxLevel = 5,
            type = PerkType.MoreSkillPoints,
            effectValue = 5f
        });

        // ... 더 많은 특전
    }

    public bool CanPurchasePerk(RebirthPerk perk)
    {
        // 포인트 체크
        if (RebirthManager.Instance.currentRebirthPoints < perk.cost)
        {
            ShowMessage("환생 포인트가 부족합니다!");
            return false;
        }

        // 최대 레벨 체크
        if (perk.currentLevel >= perk.maxLevel)
        {
            ShowMessage("최대 레벨입니다!");
            return false;
        }

        return true;
    }

    public void PurchasePerk(RebirthPerk perk)
    {
        if (!CanPurchasePerk(perk))
            return;

        // 포인트 소모
        RebirthManager.Instance.SpendRebirthPoints(perk.cost);

        // 레벨업
        perk.currentLevel++;

        // 보너스 재계산
        RebirthManager.Instance.CalculateRebirthBonuses();

        // UI 업데이트
        UpdatePerkUI();

        // 저장
        SaveManager.Instance.SaveGame();

        ShowMessage($"{perk.perkName} 레벨 {perk.currentLevel} 획득!");
    }

    public void ResetPerks()
    {
        // 환생 포인트 전액 환불
        long refund = 0;

        foreach (var perk in allPerks)
        {
            refund += perk.cost * perk.currentLevel;
            perk.currentLevel = 0;
        }

        RebirthManager.Instance.currentRebirthPoints += refund;

        // 보너스 재계산
        RebirthManager.Instance.CalculateRebirthBonuses();

        UpdatePerkUI();
        SaveManager.Instance.SaveGame();
    }
}
```

---

## 📊 환생 통계

### RebirthStatsUI

```csharp
public class RebirthStatsUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text rebirthCountText;
    public Text totalPointsText;
    public Text currentPointsText;
    public Text bonusSummaryText;

    [Header("History")]
    public GameObject historyPanel;
    public GameObject historyEntryPrefab;
    public Transform historyContent;

    public void UpdateUI()
    {
        RebirthManager rm = RebirthManager.Instance;

        rebirthCountText.text = $"환생 횟수: {rm.rebirthCount}";
        totalPointsText.text = $"총 획득 포인트: {rm.totalRebirthPoints:N0}";
        currentPointsText.text = $"사용 가능: {rm.currentRebirthPoints:N0}";

        // 보너스 요약
        bonusSummaryText.text =
            $"클릭 파워: +{(rm.clickPowerBonus - 1) * 100:F1}%\n" +
            $"골드 획득: +{(rm.goldGainBonus - 1) * 100:F1}%\n" +
            $"경험치 획득: +{(rm.expGainBonus - 1) * 100:F1}%\n" +
            $"오프라인 수익: +{(rm.offlineGainBonus - 1) * 100:F1}%";
    }

    public void ShowHistory()
    {
        historyPanel.SetActive(true);
        RefreshHistory();
    }

    private void RefreshHistory()
    {
        // 기존 히스토리 삭제
        foreach (Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }

        // 히스토리 생성
        List<RebirthHistory> history = SaveManager.Instance.GetRebirthHistory();

        foreach (var entry in history)
        {
            GameObject historyEntry = Instantiate(historyEntryPrefab, historyContent);

            Text[] texts = historyEntry.GetComponentsInChildren<Text>();
            texts[0].text = $"환생 #{entry.rebirthNumber}";
            texts[1].text = entry.timestamp;
            texts[2].text = $"획득: {entry.pointsGained:N0}pt";
            texts[3].text = $"Lv.{entry.levelReached} / {entry.goldReached:N0}G";
        }
    }
}
```

---

## 🎯 환생 마일스톤

### RebirthMilestones

```csharp
public class RebirthMilestoneManager : MonoBehaviour
{
    [System.Serializable]
    public class Milestone
    {
        public int rebirthCount;
        public string milestoneName;
        public string description;
        public MilestoneReward reward;
        public bool isCompleted;
    }

    [System.Serializable]
    public class MilestoneReward
    {
        public long bonusPoints;
        public string specialPerkId;
        public int gems;
    }

    public List<Milestone> milestones = new List<Milestone>();

    private void InitializeMilestones()
    {
        milestones.Add(new Milestone
        {
            rebirthCount = 1,
            milestoneName = "첫 환생",
            description = "처음으로 환생을 완료했습니다!",
            reward = new MilestoneReward { bonusPoints = 1000, gems = 100 }
        });

        milestones.Add(new Milestone
        {
            rebirthCount = 5,
            milestoneName = "숙련된 환생자",
            description = "5번 환생을 달성했습니다!",
            reward = new MilestoneReward { bonusPoints = 5000, gems = 500 }
        });

        milestones.Add(new Milestone
        {
            rebirthCount = 10,
            milestoneName = "환생의 달인",
            description = "10번 환생을 달성했습니다!",
            reward = new MilestoneReward { bonusPoints = 10000, gems = 1000, specialPerkId = "master_rebirth" }
        });

        milestones.Add(new Milestone
        {
            rebirthCount = 25,
            milestoneName = "무한 순환",
            description = "25번 환생을 달성했습니다!",
            reward = new MilestoneReward { bonusPoints = 25000, gems = 2500 }
        });

        milestones.Add(new Milestone
        {
            rebirthCount = 50,
            milestoneName = "영원한 성장",
            description = "50번 환생을 달성했습니다!",
            reward = new MilestoneReward { bonusPoints = 50000, gems = 5000, specialPerkId = "eternal_growth" }
        });

        milestones.Add(new Milestone
        {
            rebirthCount = 100,
            milestoneName = "초월자",
            description = "100번 환생을 달성했습니다!",
            reward = new MilestoneReward { bonusPoints = 100000, gems = 10000, specialPerkId = "transcendent" }
        });
    }

    public void CheckMilestones(int currentRebirthCount)
    {
        foreach (var milestone in milestones)
        {
            if (milestone.isCompleted)
                continue;

            if (currentRebirthCount >= milestone.rebirthCount)
            {
                CompleteMilestone(milestone);
            }
        }
    }

    private void CompleteMilestone(Milestone milestone)
    {
        milestone.isCompleted = true;

        // 보상 지급
        RebirthManager.Instance.currentRebirthPoints += milestone.reward.bonusPoints;
        GameManager.Instance.AddGems(milestone.reward.gems);

        if (!string.IsNullOrEmpty(milestone.reward.specialPerkId))
        {
            UnlockSpecialPerk(milestone.reward.specialPerkId);
        }

        // 알림
        UIManager.Instance.ShowMilestonePopup(
            milestone.milestoneName,
            milestone.description,
            milestone.reward
        );
    }

    private void UnlockSpecialPerk(string perkId)
    {
        // 특별 특전 해금
        RebirthPerk specialPerk = RebirthPerkManager.Instance.GetPerk(perkId);
        if (specialPerk != null)
        {
            RebirthManager.Instance.unlockedPerks.Add(specialPerk);
        }
    }
}
```

---

## 🔄 빠른 환생 (Quick Rebirth)

### QuickRebirthFeature

```csharp
public class QuickRebirthFeature : MonoBehaviour
{
    // 5회 이상 환생 시 해금
    public bool isUnlocked = false;

    public void UnlockQuickRebirth()
    {
        if (RebirthManager.Instance.rebirthCount >= 5)
        {
            isUnlocked = true;
            UIManager.Instance.ShowNotification("빠른 환생이 해금되었습니다!");
        }
    }

    public void QuickRebirth()
    {
        if (!isUnlocked)
        {
            ShowMessage("빠른 환생은 5회 환생 후 해금됩니다!");
            return;
        }

        // 확인 없이 즉시 환생
        RebirthManager.Instance.OnRebirthConfirmed();
    }
}
```

---

## 📈 환생 시뮬레이터

### RebirthSimulator

```csharp
public class RebirthSimulator : MonoBehaviour
{
    [Header("Simulator UI")]
    public InputField levelInput;
    public InputField goldInput;
    public Text estimatedPointsText;
    public Text estimatedBonusText;

    public void SimulateRebirth()
    {
        // 입력값 가져오기
        int simulatedLevel = int.Parse(levelInput.text);
        long simulatedGold = long.Parse(goldInput.text);

        // 예상 포인트 계산
        long estimatedPoints = CalculateEstimatedPoints(simulatedLevel, simulatedGold);

        // 예상 보너스 계산
        float newBonus = CalculateEstimatedBonus(estimatedPoints);

        // UI 업데이트
        estimatedPointsText.text = $"예상 포인트: {estimatedPoints:N0}";
        estimatedBonusText.text = $"예상 보너스: +{newBonus:F1}%";
    }

    private long CalculateEstimatedPoints(int level, long gold)
    {
        // RebirthManager와 동일한 계산 로직
        long goldPoints = (long)(gold * 0.001f);
        long levelBonus = level * 100;
        return goldPoints + levelBonus;
    }

    private float CalculateEstimatedBonus(long additionalPoints)
    {
        long totalPoints = RebirthManager.Instance.totalRebirthPoints + additionalPoints;
        float bonus = totalPoints * 0.1f; // 포인트 1당 0.1%
        return bonus;
    }
}
```

---

## 🎨 환생 연출

### RebirthCutsceneManager

```csharp
public class RebirthCutsceneManager : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem rebirthParticles;
    public Image whiteFlashImage;
    public Animator characterAnimator;

    [Header("Audio")]
    public AudioClip rebirthSound;
    public AudioClip rebirthMusic;

    public IEnumerator PlayRebirthCutscene()
    {
        // 1단계: 화면 밝아지기
        yield return StartCoroutine(WhiteFlash(1.0f));

        // 2단계: 파티클 효과
        rebirthParticles.Play();
        AudioManager.Instance.PlaySFX(rebirthSound);

        yield return new WaitForSeconds(1.5f);

        // 3단계: 캐릭터 변화 애니메이션
        characterAnimator.SetTrigger("Rebirth");

        yield return new WaitForSeconds(1.0f);

        // 4단계: 환생 완료 메시지
        UIManager.Instance.ShowBigMessage("환생 완료!", 2.0f);

        // 5단계: 획득 포인트 표시
        long points = RebirthManager.Instance.CalculateRebirthPointsGain();
        ShowPointsGainedAnimation(points);

        yield return new WaitForSeconds(2.0f);

        // 6단계: 페이드 아웃
        yield return StartCoroutine(UIManager.Instance.FadeOut(1.0f));
    }

    private IEnumerator WhiteFlash(float duration)
    {
        whiteFlashImage.gameObject.SetActive(true);
        Color color = whiteFlashImage.color;

        // Fade in
        float timer = 0;
        while (timer < duration / 2)
        {
            timer += Time.deltaTime;
            color.a = timer / (duration / 2);
            whiteFlashImage.color = color;
            yield return null;
        }

        // Fade out
        timer = 0;
        while (timer < duration / 2)
        {
            timer += Time.deltaTime;
            color.a = 1 - (timer / (duration / 2));
            whiteFlashImage.color = color;
            yield return null;
        }

        whiteFlashImage.gameObject.SetActive(false);
    }

    private void ShowPointsGainedAnimation(long points)
    {
        // 포인트 획득 애니메이션 (숫자가 올라가는 효과)
        StartCoroutine(CountUpAnimation(points, 2.0f));
    }

    private IEnumerator CountUpAnimation(long targetValue, float duration)
    {
        Text pointsText = UIManager.Instance.rebirthPointsText;
        long currentValue = 0;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            currentValue = (long)(targetValue * (timer / duration));
            pointsText.text = $"+{currentValue:N0}";
            yield return null;
        }

        pointsText.text = $"+{targetValue:N0}";
    }
}
```

---

## 📋 체크리스트

### 핵심 시스템
- [ ] RebirthManager 구현
- [ ] 환생 포인트 계산
- [ ] 환생 보너스 계산
- [ ] 게임 진행도 초기화
- [ ] 환생 히스토리 기록

### 특전 시스템
- [ ] RebirthPerkManager
- [ ] 15개 이상 특전 생성
- [ ] 특전 구매 시스템
- [ ] 특전 리셋 기능
- [ ] 특별 특전 (마일스톤 보상)

### UI 구현
- [ ] 환생 확인 팝업
- [ ] 환생 통계 화면
- [ ] 특전 트리 UI
- [ ] 환생 히스토리 로그
- [ ] 환생 시뮬레이터

### 마일스톤
- [ ] 마일스톤 시스템
- [ ] 마일스톤 보상
- [ ] 특별 특전 해금

### 연출
- [ ] 환생 컷씬
- [ ] 화면 플래시 효과
- [ ] 파티클 효과
- [ ] 포인트 획득 애니메이션
- [ ] 환생 전용 BGM

### 고급 기능
- [ ] 빠른 환생 (5회 이상)
- [ ] 환생 시뮬레이터
- [ ] 환생 추천 시스템
- [ ] 자동 환생 (VIP 전용)

---

## 🎯 밸런싱

### 환생 포인트 공식
```
기본 포인트 = (현재 골드 × 0.001) + (레벨 × 100)
보너스 = 던전 클리어 × 500 + 업적 × 200
페널티 = 1.0 - (환생 횟수 / 10 × 0.1) (최소 0.5)

최종 포인트 = (기본 포인트 + 보너스) × 페널티
```

### 권장 환생 타이밍
```
1회차: 레벨 100, 골드 10억
2회차: 레벨 150, 골드 100억
3회차: 레벨 200, 골드 1000억
5회차: 레벨 300, 골드 1조
10회차: 레벨 500, 골드 100조
```

### 특전 비용 밸런싱
```
Tier 1 (기본): 100pt
Tier 2 (중급): 500pt
Tier 3 (고급): 2,000pt
Tier 4 (특수): 10,000pt
```

---

## 📝 개발 노트

### 주의사항
1. **환생 확인**: 반드시 확인 팝업 표시
2. **데이터 백업**: 환생 전 자동 저장
3. **보너스 적용**: 환생 직후 즉시 적용
4. **마일스톤**: 환생 시마다 체크
5. **밸런싱**: 환생 페널티로 인플레이션 방지

### 개선 아이디어
- [ ] 환생 레벨 (환생 포인트로 해금)
- [ ] 환생 전용 던전
- [ ] 환생 챌린지 (제한된 조건으로 환생)
- [ ] 환생 리더보드
- [ ] 단체 환생 이벤트

---

**개발 예상 시간**: 8일
**테스트 시간**: 3일
**총 소요 시간**: 11일

**담당**: 시스템 프로그래머, UI 프로그래머, 밸런싱 디자이너
**의존성**: GameManager, SaveManager, UIManager, AchievementManager
**우선순위**: 최상 (클리커 게임의 핵심 Loop)
