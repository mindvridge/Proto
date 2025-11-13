# Scene 03: 던전 선택 - 개발 명세서

## 📋 씬 개요

**씬 이름**: DungeonSelectionScene
**씬 목적**: 던전 선택, 난이도 확인, 입장
**예상 개발 시간**: 5일
**우선순위**: 높음

---

## 🎨 UI 구성

```
┌─────────────────────────────────┐
│  << 뒤로   던전 선택   [필터] >> │
├─────────────────────────────────┤
│ [야간전용] [무인] [안전] [전체] │ ← 필터 탭
├─────────────────────────────────┤
│ ┌───────────────────────────┐   │
│ │ 🌲 새벽 숲 던전           │   │
│ │ Lv.20-30 추천             │   │
│ │ 🕐 최적시간: 03:00-06:00  │   │
│ │ 👁 목격위험: 0%            │   │
│ │ 💰 보상: 골드 x1.5        │   │
│ │        [입장하기]         │   │
│ └───────────────────────────┘   │
│                                 │
│ ┌───────────────────────────┐   │
│ │ 🏔 야간 산 던전            │   │
│ │ ... (스크롤 가능)         │   │
│ └───────────────────────────┘   │
└─────────────────────────────────┘
```

---

## 🔧 핵심 시스템

### DungeonManager
```csharp
[System.Serializable]
public class DungeonData
{
    public string dungeonId;
    public string dungeonName;
    public int minLevel;
    public int maxLevel;
    public DungeonType type;
    public TimeManager.TimeOfDay[] availableTimes;
    public float witnessRisk; // 0-1
    public float rewardMultiplier;
    public int energyCost;
    public Sprite dungeonIcon;
    public bool isUnlocked;
}

public enum DungeonType
{
    Normal,      // 일반
    NightOnly,   // 야간 전용
    Unmanned,    // 무인
    Safe,        // 안전 (의심도 증가 없음)
    Boss,        // 보스
    Event        // 이벤트
}

public class DungeonManager : MonoBehaviour
{
    public List<DungeonData> allDungeons;
    public List<DungeonData> availableDungeons;

    public void UpdateDungeonAvailability(TimeManager.TimeOfDay currentTime)
    {
        availableDungeons.Clear();

        foreach (var dungeon in allDungeons)
        {
            if (!dungeon.isUnlocked) continue;

            // 시간 체크
            if (dungeon.type == DungeonType.NightOnly)
            {
                if (currentTime == TimeManager.TimeOfDay.Night ||
                    currentTime == TimeManager.TimeOfDay.Dawn)
                {
                    availableDungeons.Add(dungeon);
                }
            }
            else
            {
                availableDungeons.Add(dungeon);
            }
        }

        // UI 업데이트
        RefreshDungeonList();
    }

    public bool CanEnterDungeon(DungeonData dungeon)
    {
        // 레벨 체크
        if (GameManager.Instance.playerLevel < dungeon.minLevel)
        {
            ShowMessage($"레벨 {dungeon.minLevel} 이상 필요합니다.");
            return false;
        }

        // 에너지 체크
        if (GameManager.Instance.currentEnergy < dungeon.energyCost)
        {
            ShowMessage("에너지가 부족합니다!");
            ShowEnergyRefillOptions();
            return false;
        }

        return true;
    }

    public void EnterDungeon(DungeonData dungeon)
    {
        if (!CanEnterDungeon(dungeon)) return;

        // 에너지 소모
        GameManager.Instance.currentEnergy -= dungeon.energyCost;

        // 의심도 체크
        if (dungeon.witnessRisk > 0)
        {
            SuspicionManager.Instance.AddSuspicion(
                dungeon.witnessRisk * 10,
                $"Entered {dungeon.dungeonName}"
            );
        }

        // 던전 데이터 저장
        GameManager.Instance.currentDungeon = dungeon;

        // 전투 씬으로 전환
        SceneTransition.Instance.LoadScene("BattleScene");
    }
}
```

### DungeonCardUI
```csharp
public class DungeonCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text dungeonNameText;
    public Text levelRangeText;
    public Text timeText;
    public Text witnessRiskText;
    public Text rewardText;
    public Image dungeonIcon;
    public Button enterButton;
    public GameObject lockedOverlay;

    private DungeonData dungeonData;

    public void SetData(DungeonData data)
    {
        dungeonData = data;

        dungeonNameText.text = data.dungeonName;
        levelRangeText.text = $"Lv.{data.minLevel}-{data.maxLevel} 추천";

        // 최적 시간
        string timeStr = "";
        foreach (var time in data.availableTimes)
        {
            timeStr += GetTimeString(time) + ", ";
        }
        timeText.text = $"🕐 {timeStr.TrimEnd(',', ' ')}";

        // 목격 위험
        witnessRiskText.text = $"👁 목격위험: {data.witnessRisk * 100}%";
        witnessRiskText.color = data.witnessRisk < 0.3f ? Color.green : Color.red;

        // 보상
        rewardText.text = $"💰 보상: x{data.rewardMultiplier}";

        // 아이콘
        dungeonIcon.sprite = data.dungeonIcon;

        // 잠금 상태
        lockedOverlay.SetActive(!data.isUnlocked);
        enterButton.interactable = data.isUnlocked;

        // 버튼 이벤트
        enterButton.onClick.RemoveAllListeners();
        enterButton.onClick.AddListener(OnEnterClicked);
    }

    private void OnEnterClicked()
    {
        DungeonManager.Instance.EnterDungeon(dungeonData);
    }

    private string GetTimeString(TimeManager.TimeOfDay time)
    {
        switch (time)
        {
            case TimeManager.TimeOfDay.Dawn: return "03:00-06:00";
            case TimeManager.TimeOfDay.Morning: return "06:00-12:00";
            case TimeManager.TimeOfDay.Afternoon: return "12:00-18:00";
            case TimeManager.TimeOfDay.Evening: return "18:00-22:00";
            case TimeManager.TimeOfDay.Night: return "22:00-03:00";
            default: return "언제나";
        }
    }
}
```

---

## 🎯 필터 시스템

```csharp
public class DungeonFilterUI : MonoBehaviour
{
    public Toggle[] filterToggles;
    public DungeonType currentFilter = DungeonType.Normal;

    private void Start()
    {
        for (int i = 0; i < filterToggles.Length; i++)
        {
            int index = i;
            filterToggles[i].onValueChanged.AddListener((bool isOn) =>
            {
                if (isOn) OnFilterChanged(index);
            });
        }
    }

    private void OnFilterChanged(int filterIndex)
    {
        switch (filterIndex)
        {
            case 0: currentFilter = DungeonType.NightOnly; break;
            case 1: currentFilter = DungeonType.Unmanned; break;
            case 2: currentFilter = DungeonType.Safe; break;
            case 3: currentFilter = DungeonType.Normal; break;
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        // 필터에 맞는 던전만 표시
        var filtered = DungeonManager.Instance.availableDungeons
            .Where(d => currentFilter == DungeonType.Normal || d.type == currentFilter)
            .ToList();

        UpdateDungeonListUI(filtered);
    }
}
```

---

## 📋 체크리스트

- [ ] 던전 데이터 구조 생성
- [ ] 던전 리스트 UI
- [ ] 던전 카드 UI
- [ ] 필터 시스템
- [ ] 입장 조건 체크
- [ ] 에너지 시스템 연동
- [ ] 씬 전환

**개발 시간**: 5일
