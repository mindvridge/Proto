# Scene 02: 메인 게임 (클릭커) - 개발 명세서

## 📋 씬 개요

**씬 이름**: MainGameScene
**씬 목적**: 핵심 클릭커 게임플레이, 방치형 성장
**예상 개발 시간**: 10일
**우선순위**: 최고 (핵심 씬)

---

## 🎨 UI 구성

### 전체 레이아웃 (9:16 모바일)
```
┌─────────────────────────────────┐
│ [Lv.45] [Gold: 1.5M] [Time]    │ ← HUD Top
│ [의심도: 45%] ⚠️                │
├─────────────────────────────────┤
│                                 │
│     [캐릭터 Idle 애니메이션]     │ ← Center (40%)
│        [TAP 영역]               │
│     (클릭 이펙트 파티클)         │
│                                 │
├─────────────────────────────────┤
│  HP: ████████░░ (80%)          │ ← Status Bar
│  MP: ██████░░░░ (60%)          │
├─────────────────────────────────┤
│ [스킬1] [스킬2] [스킬3] [스킬4] │ ← Skill Bar
│  30s     Ready    10s    45s   │
├─────────────────────────────────┤
│ [던전] [인벤] [상점] [스킬]     │ ← Bottom Menu
│ [가족] [설정] [가챠] [환생]     │
└─────────────────────────────────┘
```

---

## 🔧 핵심 시스템 구현

### 1. ClickerManager (클릭커 핵심)
```csharp
public class ClickerManager : MonoBehaviour
{
    [Header("Player Stats")]
    public long currentGold;
    public float clickPower = 1.0f;
    public float criticalChance = 0.1f;
    public float criticalMultiplier = 2.0f;

    [Header("Idle Income")]
    public float idleGoldPerSecond = 10.0f;
    public float offlineMultiplier = 0.5f;

    [Header("UI References")]
    public Text goldText;
    public Text clickPowerText;
    public ParticleSystem clickEffect;
    public Transform characterTransform;

    [Header("Audio")]
    public AudioSource clickSFX;
    public AudioClip[] clickSounds;

    private float idleTimer = 0f;

    private void Update()
    {
        // 방치형 골드 획득 (1초마다)
        idleTimer += Time.deltaTime;
        if (idleTimer >= 1.0f)
        {
            AddGold((long)idleGoldPerSecond);
            idleTimer = 0f;
        }
    }

    public void OnCharacterClicked()
    {
        // 크리티컬 계산
        bool isCritical = Random.value < criticalChance;
        float finalPower = clickPower;

        if (isCritical)
        {
            finalPower *= criticalMultiplier;
        }

        // 골드 획득
        long goldEarned = (long)finalPower;
        AddGold(goldEarned);

        // 시각 효과
        ShowClickEffect(isCritical);
        PlayClickSound();
        ShowDamageNumber(goldEarned, isCritical);

        // 캐릭터 애니메이션
        PlayCharacterClickAnimation();
    }

    private void AddGold(long amount)
    {
        currentGold += amount;
        UpdateGoldUI();

        // 업적 체크
        AchievementManager.Instance.CheckGoldMilestone(currentGold);
    }

    private void ShowClickEffect(bool isCritical)
    {
        // 파티클 크기 조절
        if (isCritical)
        {
            clickEffect.startSize = 2.0f;
            clickEffect.startColor = Color.yellow;
        }
        else
        {
            clickEffect.startSize = 1.0f;
            clickEffect.startColor = Color.white;
        }

        clickEffect.Play();
    }

    private void ShowDamageNumber(long amount, bool isCritical)
    {
        GameObject damageNumber = ObjectPool.Instance.GetPooledObject("DamageNumber");
        damageNumber.transform.position = characterTransform.position + Vector3.up;

        DamageNumberUI dnUI = damageNumber.GetComponent<DamageNumberUI>();
        dnUI.SetNumber(amount, isCritical);
    }
}
```

### 2. DamageNumberUI (떠오르는 숫자)
```csharp
public class DamageNumberUI : MonoBehaviour
{
    public Text numberText;
    public float moveSpeed = 50f;
    public float lifetime = 1.0f;
    public AnimationCurve fadeCurve;

    private float timer = 0f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetNumber(long amount, bool isCritical)
    {
        numberText.text = FormatNumber(amount);

        if (isCritical)
        {
            numberText.color = Color.yellow;
            numberText.fontSize = 40;
            numberText.text = "CRITICAL!\n" + numberText.text;
        }
        else
        {
            numberText.color = Color.white;
            numberText.fontSize = 30;
        }

        timer = 0f;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 위로 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 페이드 아웃
        canvasGroup.alpha = fadeCurve.Evaluate(timer / lifetime);

        // 수명 종료
        if (timer >= lifetime)
        {
            gameObject.SetActive(false);
        }
    }

    private string FormatNumber(long number)
    {
        if (number >= 1000000000000) // 1T 이상
            return $"{number / 1000000000000.0:F2}T";
        if (number >= 1000000000) // 1B 이상
            return $"{number / 1000000000.0:F2}B";
        if (number >= 1000000) // 1M 이상
            return $"{number / 1000000.0:F2}M";
        if (number >= 1000) // 1K 이상
            return $"{number / 1000.0:F2}K";
        return number.ToString();
    }
}
```

---

## 🎮 시간 시스템

### TimeManager (시간대별 게임 변화)
```csharp
public class TimeManager : MonoBehaviour
{
    public enum TimeOfDay
    {
        Dawn,      // 새벽 (3-6시)
        Morning,   // 아침 (6-12시)
        Afternoon, // 오후 (12-18시)
        Evening,   // 저녁 (18-22시)
        Night      // 밤 (22-3시)
    }

    [Header("Time Settings")]
    public float gameTimeSpeed = 1.0f; // 게임 시간 배속
    public bool useRealTime = false;   // 실시간 사용 여부

    private TimeOfDay currentTimeOfDay;
    private System.DateTime currentGameTime;

    [Header("UI")]
    public Text timeDisplayText;
    public Image timeIconImage;
    public Sprite[] timeIcons; // 시간대별 아이콘

    [Header("Effects")]
    public Material backgroundMaterial;
    public Color[] skyColors; // 시간대별 배경색

    private void Start()
    {
        if (useRealTime)
        {
            currentGameTime = System.DateTime.Now;
        }
        else
        {
            // 세이브 데이터에서 로드
            currentGameTime = LoadGameTime();
        }

        UpdateTimeOfDay();
    }

    private void Update()
    {
        // 시간 흐름
        currentGameTime = currentGameTime.AddSeconds(Time.deltaTime * gameTimeSpeed);

        UpdateTimeOfDay();
        UpdateUI();
        ApplyTimeEffects();
    }

    private void UpdateTimeOfDay()
    {
        int hour = currentGameTime.Hour;
        TimeOfDay newTime;

        if (hour >= 3 && hour < 6)
            newTime = TimeOfDay.Dawn;
        else if (hour >= 6 && hour < 12)
            newTime = TimeOfDay.Morning;
        else if (hour >= 12 && hour < 18)
            newTime = TimeOfDay.Afternoon;
        else if (hour >= 18 && hour < 22)
            newTime = TimeOfDay.Evening;
        else
            newTime = TimeOfDay.Night;

        if (newTime != currentTimeOfDay)
        {
            currentTimeOfDay = newTime;
            OnTimeOfDayChanged();
        }
    }

    private void OnTimeOfDayChanged()
    {
        // 시간대 변경 이벤트
        Debug.Log($"Time changed to: {currentTimeOfDay}");

        // 배경 색상 변경
        backgroundMaterial.color = skyColors[(int)currentTimeOfDay];

        // 던전 활성화 상태 변경
        DungeonManager.Instance.UpdateDungeonAvailability(currentTimeOfDay);

        // 의심도 변화
        SuspicionManager.Instance.OnTimeChanged(currentTimeOfDay);
    }

    private void UpdateUI()
    {
        timeDisplayText.text = currentGameTime.ToString("HH:mm");
        timeIconImage.sprite = timeIcons[(int)currentTimeOfDay];
    }

    private void ApplyTimeEffects()
    {
        // 시간대별 보너스
        switch (currentTimeOfDay)
        {
            case TimeOfDay.Dawn:
                // 새벽: 경험치 3배
                GameManager.Instance.expMultiplier = 3.0f;
                break;
            case TimeOfDay.Morning:
            case TimeOfDay.Afternoon:
                // 낮: 의심도 증가 위험
                GameManager.Instance.expMultiplier = 1.0f;
                break;
            case TimeOfDay.Night:
                // 밤: 경험치 2배
                GameManager.Instance.expMultiplier = 2.0f;
                break;
        }
    }

    public bool IsNightTime()
    {
        return currentTimeOfDay == TimeOfDay.Night || currentTimeOfDay == TimeOfDay.Dawn;
    }
}
```

---

## 😰 의심도 시스템

### SuspicionManager
```csharp
public class SuspicionManager : MonoBehaviour
{
    public static SuspicionManager Instance;

    [Header("Suspicion Settings")]
    [Range(0, 100)]
    public float currentSuspicion = 0f;
    public float maxSuspicion = 100f;
    public float dangerThreshold = 70f;

    [Header("Suspicion Gain")]
    public float daytimeHuntingGain = 5.0f;  // 낮 사냥 시 증가
    public float expensiveItemGain = 10.0f;  // 비싼 아이템 구매 시
    public float skillUseGain = 2.0f;        // 스킬 사용 시

    [Header("Suspicion Decrease")]
    public float familyTimeDecrease = 10.0f; // 가족과 시간 보내기
    public float restingDecrease = 5.0f;     // 집에서 쉬기
    public float dailyDecrease = 3.0f;       // 하루 자동 감소

    [Header("UI")]
    public Slider suspicionSlider;
    public Text suspicionText;
    public Image suspicionFillImage;
    public GameObject warningPanel;

    [Header("Colors")]
    public Color safeColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateUI();
        CheckDangerLevel();
    }

    public void AddSuspicion(float amount, string reason = "")
    {
        currentSuspicion += amount;
        currentSuspicion = Mathf.Clamp(currentSuspicion, 0, maxSuspicion);

        // 로그 (디버그용)
        Debug.Log($"Suspicion +{amount} ({reason}). Current: {currentSuspicion}");

        // 특정 수치 도달 시 이벤트
        if (currentSuspicion >= dangerThreshold && currentSuspicion - amount < dangerThreshold)
        {
            OnDangerLevelReached();
        }

        // 최대치 도달
        if (currentSuspicion >= maxSuspicion)
        {
            OnMaxSuspicionReached();
        }
    }

    public void ReduceSuspicion(float amount, string reason = "")
    {
        currentSuspicion -= amount;
        currentSuspicion = Mathf.Clamp(currentSuspicion, 0, maxSuspicion);

        Debug.Log($"Suspicion -{amount} ({reason}). Current: {currentSuspicion}");

        ShowFloatingText($"-{amount}% 의심도 감소", Color.green);
    }

    private void OnDangerLevelReached()
    {
        // 경고 팝업
        warningPanel.SetActive(true);

        // 가족 의심 대화 이벤트 발생 가능성
        if (Random.value < 0.3f)
        {
            TriggerFamilySuspicionEvent();
        }
    }

    private void OnMaxSuspicionReached()
    {
        // 정체 발각! (게임 오버는 아니지만 페널티)
        Debug.Log("Family discovered your secret!");

        // 강제 이벤트 발생
        StoryManager.Instance.TriggerStory("FamilyDiscovery");

        // 의심도 50%로 리셋
        currentSuspicion = 50f;

        // 페널티: 골드 30% 차감
        long penalty = (long)(GameManager.Instance.currentGold * 0.3f);
        GameManager.Instance.currentGold -= penalty;

        // 팝업 표시
        ShowPenaltyPopup(penalty);
    }

    private void TriggerFamilySuspicionEvent()
    {
        // 랜덤 가족 멤버 선택
        string[] familyMembers = { "Mother", "Father", "Sister" };
        string member = familyMembers[Random.Range(0, familyMembers.Length)];

        // 대화 이벤트 트리거
        DialogueManager.Instance.StartDialogue($"Suspicion_{member}");
    }

    public void OnTimeChanged(TimeManager.TimeOfDay timeOfDay)
    {
        // 낮 시간에 사냥하면 의심도 증가
        if (timeOfDay == TimeManager.TimeOfDay.Morning ||
            timeOfDay == TimeManager.TimeOfDay.Afternoon)
        {
            if (GameManager.Instance.isHunting)
            {
                AddSuspicion(daytimeHuntingGain, "Daytime hunting");
            }
        }
    }

    private void UpdateUI()
    {
        suspicionSlider.value = currentSuspicion / maxSuspicion;
        suspicionText.text = $"{currentSuspicion:F0}%";

        // 색상 변경
        if (currentSuspicion < 30f)
            suspicionFillImage.color = safeColor;
        else if (currentSuspicion < dangerThreshold)
            suspicionFillImage.color = warningColor;
        else
            suspicionFillImage.color = dangerColor;
    }

    private void CheckDangerLevel()
    {
        // 위험 수준일 때 경고 깜빡임
        if (currentSuspicion >= dangerThreshold)
        {
            float blink = Mathf.PingPong(Time.time * 2, 1);
            suspicionFillImage.color = Color.Lerp(warningColor, dangerColor, blink);
        }
    }

    // 광고 시청으로 의심도 감소
    public void WatchAdToReduceSuspicion()
    {
        AdManager.Instance.ShowRewardedAd(() =>
        {
            ReduceSuspicion(20f, "Watched Ad");
        });
    }
}
```

---

## 🏃 캐릭터 애니메이션

### CharacterAnimationController
```csharp
public class CharacterAnimationController : MonoBehaviour
{
    [Header("Animator")]
    public Animator characterAnimator;

    [Header("Animation Clips")]
    public string idleAnimation = "Idle";
    public string clickAnimation = "Click";
    public string skillAnimation = "Skill";
    public string levelUpAnimation = "LevelUp";

    [Header("Settings")]
    public float clickAnimationDuration = 0.3f;

    private Coroutine currentAnimation;

    public void PlayClickAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(PlayAnimationRoutine(clickAnimation, clickAnimationDuration));
    }

    public void PlaySkillAnimation(int skillIndex)
    {
        characterAnimator.SetTrigger($"Skill{skillIndex}");
    }

    public void PlayLevelUpAnimation()
    {
        characterAnimator.SetTrigger("LevelUp");
    }

    private IEnumerator PlayAnimationRoutine(string animName, float duration)
    {
        characterAnimator.Play(animName);
        yield return new WaitForSeconds(duration);
        characterAnimator.Play(idleAnimation);
    }

    // 시간대별 Idle 변경
    public void SetIdleByTimeOfDay(TimeManager.TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeManager.TimeOfDay.Night:
            case TimeManager.TimeOfDay.Dawn:
                characterAnimator.Play("Idle_Alert"); // 경계 모드
                break;
            default:
                characterAnimator.Play("Idle_Lazy"); // 백수 모드
                break;
        }
    }
}
```

---

## 💪 스킬 시스템

### SkillManager
```csharp
public class SkillManager : MonoBehaviour
{
    [System.Serializable]
    public class Skill
    {
        public string skillName;
        public int skillLevel;
        public float cooldown;
        public float manaCost;
        public Sprite skillIcon;
        public SkillEffect effect;
    }

    [Header("Skills")]
    public List<Skill> equippedSkills = new List<Skill>(4);

    [Header("UI")]
    public SkillButtonUI[] skillButtons;

    private float[] skillCooldownTimers = new float[4];

    private void Update()
    {
        // 쿨다운 업데이트
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
                UpdateSkillButtonUI(i);
            }
        }
    }

    public void UseSkill(int index)
    {
        if (index >= equippedSkills.Count) return;

        Skill skill = equippedSkills[index];

        // 쿨다운 체크
        if (skillCooldownTimers[index] > 0)
        {
            ShowMessage("스킬이 쿨다운 중입니다!");
            return;
        }

        // 마나 체크
        if (GameManager.Instance.currentMP < skill.manaCost)
        {
            ShowMessage("마나가 부족합니다!");
            return;
        }

        // 마나 소모
        GameManager.Instance.currentMP -= skill.manaCost;

        // 스킬 효과 발동
        ActivateSkillEffect(skill);

        // 쿨다운 시작
        skillCooldownTimers[index] = skill.cooldown;

        // 애니메이션
        CharacterAnimationController.Instance.PlaySkillAnimation(index);

        // 의심도 증가 (스킬 사용)
        SuspicionManager.Instance.AddSuspicion(2.0f, $"Used {skill.skillName}");
    }

    private void ActivateSkillEffect(Skill skill)
    {
        switch (skill.effect)
        {
            case SkillEffect.ClickPowerBoost:
                StartCoroutine(BoostClickPower(skill.skillLevel));
                break;
            case SkillEffect.AutoClick:
                StartCoroutine(AutoClick(skill.skillLevel));
                break;
            case SkillEffect.GoldRush:
                StartCoroutine(GoldRush(skill.skillLevel));
                break;
            case SkillEffect.TimeFreeze:
                TimeManager.Instance.FreezeTime(5.0f);
                break;
        }
    }

    private IEnumerator BoostClickPower(int level)
    {
        float multiplier = 2.0f + (level * 0.5f);
        float originalPower = ClickerManager.Instance.clickPower;

        ClickerManager.Instance.clickPower *= multiplier;
        ShowSkillEffect("클릭 파워 증가!", Color.yellow);

        yield return new WaitForSeconds(10.0f);

        ClickerManager.Instance.clickPower = originalPower;
    }

    private IEnumerator AutoClick(int level)
    {
        int clicksPerSecond = 5 + level;
        float duration = 10.0f;
        float timer = 0f;
        float interval = 1.0f / clicksPerSecond;
        float nextClick = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            nextClick += Time.deltaTime;

            if (nextClick >= interval)
            {
                ClickerManager.Instance.OnCharacterClicked();
                nextClick = 0f;
            }

            yield return null;
        }
    }

    private void UpdateSkillButtonUI(int index)
    {
        if (skillCooldownTimers[index] > 0)
        {
            skillButtons[index].SetCooldown(skillCooldownTimers[index]);
        }
        else
        {
            skillButtons[index].SetReady();
        }
    }
}

public enum SkillEffect
{
    ClickPowerBoost,
    AutoClick,
    GoldRush,
    TimeFreeze,
    SuspicionReduce
}
```

---

## 📊 오프라인 수익

### OfflineProgressManager
```csharp
public class OfflineProgressManager : MonoBehaviour
{
    [Header("Settings")]
    public float maxOfflineHours = 8.0f;
    public float offlineEfficiency = 0.5f; // 50% 효율

    public void CalculateOfflineProgress()
    {
        // 마지막 플레이 시간 로드
        string lastPlayTimeStr = PlayerPrefs.GetString("LastPlayTime", "");
        if (string.IsNullOrEmpty(lastPlayTimeStr))
        {
            // 첫 플레이
            SaveLastPlayTime();
            return;
        }

        System.DateTime lastPlayTime = System.DateTime.Parse(lastPlayTimeStr);
        System.DateTime currentTime = System.DateTime.Now;

        TimeSpan offlineTime = currentTime - lastPlayTime;
        float offlineHours = (float)offlineTime.TotalHours;

        // 최대 시간 제한
        offlineHours = Mathf.Min(offlineHours, maxOfflineHours);

        // 오프라인 수익 계산
        float idleGoldPerSecond = GameManager.Instance.idleGoldPerSecond;
        long offlineGold = (long)(idleGoldPerSecond * offlineHours * 3600 * offlineEfficiency);

        // 결과 표시
        ShowOfflineRewardPopup(offlineHours, offlineGold);

        // 현재 시간 저장
        SaveLastPlayTime();
    }

    private void ShowOfflineRewardPopup(float hours, long gold)
    {
        GameObject popup = Instantiate(Resources.Load<GameObject>("UI/OfflineRewardPopup"));
        OfflineRewardPopupUI popupUI = popup.GetComponent<OfflineRewardPopupUI>();

        popupUI.SetData(hours, gold);

        // 2배로 받기 옵션 (광고)
        popupUI.onDoubleReward = () =>
        {
            AdManager.Instance.ShowRewardedAd(() =>
            {
                GameManager.Instance.AddGold(gold * 2);
                popupUI.Close();
            });
        };

        popupUI.onNormalReward = () =>
        {
            GameManager.Instance.AddGold(gold);
            popupUI.Close();
        };
    }

    private void SaveLastPlayTime()
    {
        PlayerPrefs.SetString("LastPlayTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}
```

---

## 🎯 자동 저장

### AutoSaveManager
```csharp
public class AutoSaveManager : MonoBehaviour
{
    [Header("Settings")]
    public float autoSaveInterval = 60.0f; // 60초마다 자동 저장

    private float saveTimer = 0f;

    private void Update()
    {
        saveTimer += Time.deltaTime;

        if (saveTimer >= autoSaveInterval)
        {
            SaveGame();
            saveTimer = 0f;
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // 플레이어 데이터
        data.playerLevel = GameManager.Instance.playerLevel;
        data.currentGold = GameManager.Instance.currentGold;
        data.currentEXP = GameManager.Instance.currentEXP;
        data.prestigePoints = GameManager.Instance.prestigePoints;

        // 스탯
        data.clickPower = ClickerManager.Instance.clickPower;
        data.idleGoldPerSecond = ClickerManager.Instance.idleGoldPerSecond;

        // 의심도
        data.suspicionLevel = SuspicionManager.Instance.currentSuspicion;

        // 시간
        data.currentGameTime = TimeManager.Instance.currentGameTime.ToString();

        // JSON 변환 및 저장
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameSaveData", json);
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");

        // 저장 알림 표시 (옵션)
        ShowSaveNotification();
    }

    public SaveData LoadGame()
    {
        string json = PlayerPrefs.GetString("GameSaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data;
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 앱이 백그라운드로 가면 저장
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        // 앱 종료 시 저장
        SaveGame();
    }
}

[System.Serializable]
public class SaveData
{
    public int playerLevel;
    public long currentGold;
    public long currentEXP;
    public int prestigePoints;
    public float clickPower;
    public float idleGoldPerSecond;
    public float suspicionLevel;
    public string currentGameTime;
    // ... 추가 데이터
}
```

---

## 📋 체크리스트

### UI 구현
- [ ] HUD (레벨, 골드, 시간, 의심도)
- [ ] 캐릭터 TAP 영역
- [ ] HP/MP 바
- [ ] 스킬 버튼 4개
- [ ] 하단 메뉴 8개 버튼

### 핵심 시스템
- [ ] 클릭 감지 및 골드 획득
- [ ] 크리티컬 시스템
- [ ] 데미지 넘버 표시
- [ ] 방치형 골드 획득

### 시간 시스템
- [ ] 게임 시간 흐름
- [ ] 시간대 변경 감지
- [ ] 시간대별 보너스 적용
- [ ] UI 시간 표시

### 의심도 시스템
- [ ] 의심도 증가/감소
- [ ] 위험 수준 경고
- [ ] 최대 의심도 이벤트
- [ ] 의심도 UI 표시

### 스킬 시스템
- [ ] 스킬 사용
- [ ] 쿨다운 타이머
- [ ] 마나 소모
- [ ] 스킬 효과 구현

### 오프라인 & 저장
- [ ] 오프라인 수익 계산
- [ ] 오프라인 보상 팝업
- [ ] 자동 저장 (60초)
- [ ] 게임 데이터 로드

### 최적화
- [ ] 오브젝트 풀링 (데미지 넘버)
- [ ] 파티클 효과 최적화
- [ ] 메모리 관리

---

**개발 예상 시간**: 10일
**테스트 시간**: 3일
**총 소요 시간**: 13일

**담당**: 게임플레이 프로그래머, UI 프로그래머
**의존성**: GameManager, TimeManager, SuspicionManager
