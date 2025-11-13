# Scene 09: 스토리 & 대화 - 개발 명세서

## 📋 씬 개요

**씬 이름**: StoryScene
**씬 목적**: 비주얼 노벨 스타일 대화, 스토리 진행, 가족 인터랙션
**예상 개발 시간**: 7일
**우선순위**: 높음

---

## 🎨 UI 구성

```
┌─────────────────────────────────┐
│                                 │  ← 상단 20% (캐릭터 배경)
│   [캐릭터 일러스트]              │
│                                 │
├─────────────────────────────────┤
│  👤 어머니                       │  ← 이름표
├─────────────────────────────────┤
│  "민수야, 요즘 밤에 뭐하니?"     │  ← 대화창 30%
│  "자꾸 늦게 들어오더라..."       │
│                                 │
│  [자동] [스킵] [로그]           │  ← 하단 컨트롤
└─────────────────────────────────┘
        ↓ (선택지 등장)
┌─────────────────────────────────┐
│  [1] 그냥 산책하고 왔어요        │
│  [2] 편의점 알바 하고 왔어요     │
│  [3] (거짓말) 친구 만나고 왔어요 │
└─────────────────────────────────┘
```

---

## 🔧 핵심 시스템

### DialogueManager

```csharp
[System.Serializable]
public class DialogueData
{
    public string dialogueId;
    public string speakerName;
    public Sprite speakerSprite;
    public string[] dialogueLines;
    public DialogueChoice[] choices;
    public string nextDialogueId;
    public DialogueCondition[] conditions; // 조건부 등장
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public string nextDialogueId;
    public float suspicionChange; // 의심도 변화
    public int relationshipChange; // 호감도 변화
    public DialogueCondition[] requirements;
}

[System.Serializable]
public class DialogueCondition
{
    public ConditionType type;
    public string targetId;
    public int requiredValue;
}

public enum ConditionType
{
    PlayerLevel,
    StoryProgress,
    Relationship,
    Suspicion,
    ItemOwned,
    QuestCompleted
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public Text speakerNameText;
    public Text dialogueText;
    public Image characterImage;
    public GameObject choicePanel;
    public GameObject dialoguePanel;

    [Header("Dialogue Data")]
    public DialogueDatabase dialogueDB;
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;

    [Header("Settings")]
    public float textSpeed = 0.05f;
    public bool isAutoMode = false;
    public float autoModeDelay = 2.0f;

    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    public void StartDialogue(string dialogueId)
    {
        currentDialogue = dialogueDB.GetDialogue(dialogueId);

        if (currentDialogue == null)
        {
            Debug.LogError($"Dialogue not found: {dialogueId}");
            return;
        }

        // 조건 체크
        if (!CheckConditions(currentDialogue.conditions))
        {
            Debug.Log($"Conditions not met for: {dialogueId}");
            return;
        }

        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex >= currentDialogue.dialogueLines.Length)
        {
            // 대화 종료
            OnDialogueEnd();
            return;
        }

        string line = currentDialogue.dialogueLines[currentLineIndex];

        // 화자 정보
        speakerNameText.text = currentDialogue.speakerName;
        characterImage.sprite = currentDialogue.speakerSprite;

        // 타이핑 효과
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(line));
    }

    private IEnumerator TypewriterEffect(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        // 자동 모드
        if (isAutoMode)
        {
            yield return new WaitForSeconds(autoModeDelay);
            OnNextClicked();
        }
    }

    public void OnDialogueClicked()
    {
        if (isTyping)
        {
            // 타이핑 중이면 즉시 완료
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
            isTyping = false;
        }
        else
        {
            // 다음 대사로
            OnNextClicked();
        }
    }

    public void OnNextClicked()
    {
        currentLineIndex++;
        ShowCurrentLine();
    }

    private void OnDialogueEnd()
    {
        // 선택지가 있는가?
        if (currentDialogue.choices != null && currentDialogue.choices.Length > 0)
        {
            ShowChoices();
        }
        else
        {
            // 다음 대화로 자동 이동
            if (!string.IsNullOrEmpty(currentDialogue.nextDialogueId))
            {
                StartDialogue(currentDialogue.nextDialogueId);
            }
            else
            {
                // 대화 완전 종료
                EndDialogue();
            }
        }
    }

    private void ShowChoices()
    {
        choicePanel.SetActive(true);

        // 선택지 버튼 생성
        foreach (Transform child in choicePanel.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var choice in currentDialogue.choices)
        {
            // 선택지 조건 체크
            if (!CheckConditions(choice.requirements))
                continue;

            GameObject choiceButton = Instantiate(choiceButtonPrefab, choicePanel.transform);
            choiceButton.GetComponentInChildren<Text>().text = choice.choiceText;

            choiceButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnChoiceSelected(choice);
            });
        }
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        // 의심도 변화
        if (choice.suspicionChange != 0)
        {
            SuspicionManager.Instance.AddSuspicion(
                choice.suspicionChange,
                $"Dialogue choice: {choice.choiceText}"
            );
        }

        // 호감도 변화
        if (choice.relationshipChange != 0)
        {
            RelationshipManager.Instance.ChangeRelationship(
                currentDialogue.speakerName,
                choice.relationshipChange
            );
        }

        choicePanel.SetActive(false);

        // 다음 대화로
        if (!string.IsNullOrEmpty(choice.nextDialogueId))
        {
            StartDialogue(choice.nextDialogueId);
        }
        else
        {
            EndDialogue();
        }
    }

    private bool CheckConditions(DialogueCondition[] conditions)
    {
        if (conditions == null || conditions.Length == 0)
            return true;

        foreach (var condition in conditions)
        {
            switch (condition.type)
            {
                case ConditionType.PlayerLevel:
                    if (GameManager.Instance.playerLevel < condition.requiredValue)
                        return false;
                    break;

                case ConditionType.StoryProgress:
                    if (StoryManager.Instance.currentChapter < condition.requiredValue)
                        return false;
                    break;

                case ConditionType.Suspicion:
                    if (SuspicionManager.Instance.currentSuspicion < condition.requiredValue)
                        return false;
                    break;

                case ConditionType.Relationship:
                    if (RelationshipManager.Instance.GetRelationship(condition.targetId) < condition.requiredValue)
                        return false;
                    break;

                // ... 기타 조건
            }
        }

        return true;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // 스토리 진행도 업데이트
        StoryManager.Instance.OnDialogueComplete(currentDialogue.dialogueId);

        // 업적 체크
        AchievementManager.Instance.CheckStoryAchievements();
    }

    public void ToggleAutoMode()
    {
        isAutoMode = !isAutoMode;
        // UI 업데이트
    }

    public void OnSkipClicked()
    {
        // 스킵 가능 여부 체크 (이미 본 대화만)
        if (StoryManager.Instance.IsDialogueSeen(currentDialogue.dialogueId))
        {
            EndDialogue();
        }
        else
        {
            ShowMessage("이미 본 대화만 스킵할 수 있습니다!");
        }
    }
}
```

---

## 📚 스토리 진행 관리

### StoryManager

```csharp
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    [Header("Story Progress")]
    public int currentChapter = 1;
    public int currentArc = 1;
    public List<string> completedDialogues = new List<string>();
    public List<string> unlockedStories = new List<string>();

    [Header("Story Unlock Conditions")]
    public StoryUnlockCondition[] storyUnlocks;

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

    public void CheckStoryUnlocks()
    {
        foreach (var unlock in storyUnlocks)
        {
            if (unlockedStories.Contains(unlock.storyId))
                continue;

            if (IsUnlockConditionMet(unlock))
            {
                UnlockStory(unlock.storyId);
            }
        }
    }

    private bool IsUnlockConditionMet(StoryUnlockCondition unlock)
    {
        // 레벨 조건
        if (GameManager.Instance.playerLevel < unlock.requiredLevel)
            return false;

        // 선행 스토리 조건
        foreach (string prereq in unlock.prerequisiteStories)
        {
            if (!completedDialogues.Contains(prereq))
                return false;
        }

        // 던전 클리어 조건
        if (!string.IsNullOrEmpty(unlock.requiredDungeon))
        {
            if (!DungeonManager.Instance.IsDungeonCleared(unlock.requiredDungeon))
                return false;
        }

        return true;
    }

    private void UnlockStory(string storyId)
    {
        unlockedStories.Add(storyId);

        // 알림 표시
        UIManager.Instance.ShowNotification($"새로운 스토리가 해금되었습니다!");

        // 스토리 버튼에 느낌표 표시
        MainMenuUI.Instance.ShowStoryNotification(true);
    }

    public void OnDialogueComplete(string dialogueId)
    {
        if (!completedDialogues.Contains(dialogueId))
        {
            completedDialogues.Add(dialogueId);
            CheckStoryUnlocks();
        }
    }

    public bool IsDialogueSeen(string dialogueId)
    {
        return completedDialogues.Contains(dialogueId);
    }

    public void AdvanceChapter()
    {
        currentChapter++;
        ShowChapterTitle();

        // 챕터 클리어 보상
        GiveChapterReward();
    }

    private void ShowChapterTitle()
    {
        // 챕터 타이틀 연출
        UIManager.Instance.ShowChapterTitle($"Chapter {currentChapter}");
    }

    private void GiveChapterReward()
    {
        // 보상 지급
        long goldReward = 10000 * currentChapter;
        int gemReward = 100 * currentChapter;

        GameManager.Instance.AddGold(goldReward);
        GameManager.Instance.AddGems(gemReward);

        UIManager.Instance.ShowRewardPopup(goldReward, gemReward);
    }
}

[System.Serializable]
public class StoryUnlockCondition
{
    public string storyId;
    public string storyName;
    public int requiredLevel;
    public string[] prerequisiteStories;
    public string requiredDungeon;
}
```

---

## 👨‍👩‍👧 가족 관계 시스템

### RelationshipManager

```csharp
public class RelationshipManager : MonoBehaviour
{
    public static RelationshipManager Instance;

    [System.Serializable]
    public class Relationship
    {
        public string characterName;
        public int relationshipLevel; // 0-100
        public RelationshipTier tier;
    }

    public enum RelationshipTier
    {
        Hostile = 0,      // 0-20
        Cold = 1,         // 21-40
        Neutral = 2,      // 41-60
        Friendly = 3,     // 61-80
        Trusting = 4      // 81-100
    }

    public List<Relationship> relationships = new List<Relationship>();

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

        InitializeRelationships();
    }

    private void InitializeRelationships()
    {
        // 가족 관계 초기화
        relationships.Add(new Relationship { characterName = "어머니", relationshipLevel = 60 });
        relationships.Add(new Relationship { characterName = "여동생", relationshipLevel = 55 });
        relationships.Add(new Relationship { characterName = "아버지", relationshipLevel = 50 });
    }

    public void ChangeRelationship(string characterName, int change)
    {
        Relationship rel = relationships.Find(r => r.characterName == characterName);

        if (rel != null)
        {
            int oldLevel = rel.relationshipLevel;
            rel.relationshipLevel = Mathf.Clamp(rel.relationshipLevel + change, 0, 100);

            // 티어 업데이트
            rel.tier = GetTierFromLevel(rel.relationshipLevel);

            // 티어 변경 체크
            if (GetTierFromLevel(oldLevel) != rel.tier)
            {
                OnRelationshipTierChanged(characterName, rel.tier);
            }

            UpdateUI();
        }
    }

    public int GetRelationship(string characterName)
    {
        Relationship rel = relationships.Find(r => r.characterName == characterName);
        return rel != null ? rel.relationshipLevel : 0;
    }

    private RelationshipTier GetTierFromLevel(int level)
    {
        if (level <= 20) return RelationshipTier.Hostile;
        if (level <= 40) return RelationshipTier.Cold;
        if (level <= 60) return RelationshipTier.Neutral;
        if (level <= 80) return RelationshipTier.Friendly;
        return RelationshipTier.Trusting;
    }

    private void OnRelationshipTierChanged(string characterName, RelationshipTier newTier)
    {
        // 관계 단계 변화 알림
        UIManager.Instance.ShowNotification($"{characterName}과(와)의 관계가 {GetTierName(newTier)}(으)로 변했습니다!");

        // 특별 이벤트 해금
        UnlockRelationshipEvents(characterName, newTier);
    }

    private string GetTierName(RelationshipTier tier)
    {
        switch (tier)
        {
            case RelationshipTier.Hostile: return "적대적";
            case RelationshipTier.Cold: return "냉담함";
            case RelationshipTier.Neutral: return "보통";
            case RelationshipTier.Friendly: return "친밀함";
            case RelationshipTier.Trusting: return "신뢰";
            default: return "알 수 없음";
        }
    }

    private void UnlockRelationshipEvents(string character, RelationshipTier tier)
    {
        string eventId = $"{character}_{tier}";
        StoryManager.Instance.UnlockStory(eventId);
    }
}
```

---

## 🎭 대화 데이터베이스

### DialogueDatabase

```csharp
public class DialogueDatabase : ScriptableObject
{
    public List<DialogueData> allDialogues = new List<DialogueData>();

    public DialogueData GetDialogue(string dialogueId)
    {
        return allDialogues.Find(d => d.dialogueId == dialogueId);
    }

    public List<DialogueData> GetDialoguesByCharacter(string characterName)
    {
        return allDialogues.FindAll(d => d.speakerName == characterName);
    }
}
```

---

## 📜 로그 시스템

### DialogueLogManager

```csharp
public class DialogueLogManager : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLogEntry
    {
        public string speakerName;
        public string dialogueText;
        public string timestamp;
    }

    public List<DialogueLogEntry> dialogueLog = new List<DialogueLogEntry>();
    public int maxLogEntries = 100;

    [Header("UI")]
    public GameObject logPanel;
    public GameObject logEntryPrefab;
    public Transform logContent;

    public void AddLogEntry(string speaker, string text)
    {
        DialogueLogEntry entry = new DialogueLogEntry
        {
            speakerName = speaker,
            dialogueText = text,
            timestamp = System.DateTime.Now.ToString("HH:mm:ss")
        };

        dialogueLog.Add(entry);

        // 최대 개수 초과 시 오래된 것 삭제
        if (dialogueLog.Count > maxLogEntries)
        {
            dialogueLog.RemoveAt(0);
        }
    }

    public void ShowLog()
    {
        logPanel.SetActive(true);
        RefreshLogUI();
    }

    private void RefreshLogUI()
    {
        // 기존 로그 UI 삭제
        foreach (Transform child in logContent)
        {
            Destroy(child.gameObject);
        }

        // 로그 엔트리 생성
        foreach (var entry in dialogueLog)
        {
            GameObject logEntry = Instantiate(logEntryPrefab, logContent);

            Text[] texts = logEntry.GetComponentsInChildren<Text>();
            texts[0].text = $"[{entry.timestamp}] {entry.speakerName}";
            texts[1].text = entry.dialogueText;
        }
    }

    public void CloseLog()
    {
        logPanel.SetActive(false);
    }
}
```

---

## 🎬 특수 연출

### CutsceneManager

```csharp
public class CutsceneManager : MonoBehaviour
{
    public enum CutsceneType
    {
        FadeInOut,
        SlideIn,
        Shake,
        Flash,
        Zoom
    }

    public IEnumerator PlayCutscene(CutsceneType type, float duration)
    {
        switch (type)
        {
            case CutsceneType.FadeInOut:
                yield return StartCoroutine(FadeEffect(duration));
                break;
            case CutsceneType.Shake:
                yield return StartCoroutine(ShakeEffect(duration));
                break;
            case CutsceneType.Flash:
                yield return StartCoroutine(FlashEffect(duration));
                break;
            // ... 기타 연출
        }
    }

    private IEnumerator FadeEffect(float duration)
    {
        // 페이드 아웃 -> 페이드 인
        Image fadeImage = UIManager.Instance.fadeImage;

        // Fade out
        float timer = 0;
        Color color = fadeImage.color;
        color.a = 0;

        while (timer < duration / 2)
        {
            timer += Time.deltaTime;
            color.a = timer / (duration / 2);
            fadeImage.color = color;
            yield return null;
        }

        // Fade in
        timer = 0;
        while (timer < duration / 2)
        {
            timer += Time.deltaTime;
            color.a = 1 - (timer / (duration / 2));
            fadeImage.color = color;
            yield return null;
        }
    }

    private IEnumerator ShakeEffect(float duration)
    {
        Transform target = Camera.main.transform;
        Vector3 originalPos = target.localPosition;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float y = Random.Range(-0.1f, 0.1f);

            target.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    private IEnumerator FlashEffect(float duration)
    {
        Image flashImage = UIManager.Instance.flashImage;
        flashImage.gameObject.SetActive(true);

        Color color = flashImage.color;
        color.a = 1;
        flashImage.color = color;

        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = 1 - (timer / duration);
            flashImage.color = color;
            yield return null;
        }

        flashImage.gameObject.SetActive(false);
    }
}
```

---

## 📋 예시 대화 데이터

### 가족 의심 이벤트

```json
{
  "dialogueId": "mother_suspicion_01",
  "speakerName": "어머니",
  "speakerSprite": "mother_worried",
  "dialogueLines": [
    "민수야, 요즘 밤에 뭐하니?",
    "자꾸 늦게 들어오더라...",
    "혹시 이상한 일에 휘말린 건 아니지?"
  ],
  "choices": [
    {
      "choiceText": "그냥 산책하고 왔어요",
      "suspicionChange": -5,
      "relationshipChange": 5,
      "nextDialogueId": "mother_suspicion_01_A"
    },
    {
      "choiceText": "편의점 알바 하고 왔어요",
      "suspicionChange": -10,
      "relationshipChange": 10,
      "nextDialogueId": "mother_suspicion_01_B"
    },
    {
      "choiceText": "(거짓말) 친구 만나고 왔어요",
      "suspicionChange": 0,
      "relationshipChange": 0,
      "nextDialogueId": "mother_suspicion_01_C"
    }
  ]
}
```

### 여동생 대화

```json
{
  "dialogueId": "sister_conversation_01",
  "speakerName": "여동생",
  "speakerSprite": "sister_curious",
  "dialogueLines": [
    "오빠, 요즘 돈이 좀 생긴 것 같던데?",
    "혹시 복권 당첨된 거야?",
    "아니면 정말로 취직한 거야?"
  ],
  "choices": [
    {
      "choiceText": "...비밀이야",
      "suspicionChange": 10,
      "relationshipChange": -5,
      "nextDialogueId": "sister_conversation_01_A"
    },
    {
      "choiceText": "응, 취직했어",
      "suspicionChange": -15,
      "relationshipChange": 15,
      "nextDialogueId": "sister_conversation_01_B"
    },
    {
      "choiceText": "투자 좀 했어",
      "suspicionChange": 5,
      "relationshipChange": 5,
      "nextDialogueId": "sister_conversation_01_C"
    }
  ]
}
```

---

## 🎯 스토리 트리거

### StoryTriggerManager

```csharp
public class StoryTriggerManager : MonoBehaviour
{
    public static StoryTriggerManager Instance;

    public enum TriggerType
    {
        LevelUp,
        GoldThreshold,
        DungeonClear,
        SuspicionThreshold,
        TimeOfDay,
        DayPassed
    }

    [System.Serializable]
    public class StoryTrigger
    {
        public string triggerId;
        public TriggerType type;
        public int thresholdValue;
        public string dialogueToTrigger;
        public bool isOneTime = true;
        public bool isTriggered = false;
    }

    public List<StoryTrigger> triggers = new List<StoryTrigger>();

    public void CheckTrigger(TriggerType type, int currentValue)
    {
        foreach (var trigger in triggers)
        {
            if (trigger.type != type)
                continue;

            if (trigger.isOneTime && trigger.isTriggered)
                continue;

            if (currentValue >= trigger.thresholdValue)
            {
                TriggerStory(trigger);
            }
        }
    }

    private void TriggerStory(StoryTrigger trigger)
    {
        trigger.isTriggered = true;

        // 대화 시작
        DialogueManager.Instance.StartDialogue(trigger.dialogueToTrigger);
    }
}
```

---

## 📋 체크리스트

### UI 구현
- [ ] 대화창 레이아웃
- [ ] 캐릭터 일러스트 표시
- [ ] 이름표 UI
- [ ] 선택지 버튼
- [ ] 자동/스킵 버튼
- [ ] 로그 창

### 시스템 구현
- [ ] DialogueManager
- [ ] 타이핑 효과
- [ ] 선택지 시스템
- [ ] 조건부 대화
- [ ] StoryManager
- [ ] 스토리 진행도 저장

### 가족 시스템
- [ ] RelationshipManager
- [ ] 호감도 시스템
- [ ] 관계 단계 변화
- [ ] 특수 이벤트 해금

### 연출
- [ ] 페이드 인/아웃
- [ ] 화면 흔들림
- [ ] 플래시 효과
- [ ] 배경음악 변화

### 데이터
- [ ] 대화 데이터베이스
- [ ] 최소 30개 대화 작성
- [ ] 가족 이벤트 10개
- [ ] 챕터별 스토리

### 최적화
- [ ] 대화 로그 메모리 관리
- [ ] 이미지 로딩 최적화
- [ ] 선택지 UI 풀링

---

## 📝 개발 노트

### 주의사항
1. **대화 스킵**: 이미 본 대화만 스킵 가능
2. **선택지 조건**: 레벨, 호감도 등 다양한 조건 체크
3. **의심도 연동**: 대화 선택이 의심도에 영향
4. **자동 저장**: 대화 완료 시 진행도 자동 저장

### 개선 아이디어
- [ ] 보이스 오버 지원
- [ ] 캐릭터 립싱크
- [ ] 다양한 감정 표현 (이모티콘)
- [ ] 대화 중 CG 삽입
- [ ] 배경 변경 효과

---

**개발 예상 시간**: 7일
**테스트 시간**: 2일
**총 소요 시간**: 9일

**담당**: UI 프로그래머, 시나리오 작가, 일러스트레이터
**의존성**: UIManager, StoryManager, RelationshipManager, SuspicionManager
