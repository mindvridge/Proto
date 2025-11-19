# 캐릭터 보이스 라인 시스템 - 사용 가이드

## 📋 개요

**은둔형 백수 영웅** 게임의 캐릭터 보이스 대사 시스템 완전 가이드입니다.

---

## 📁 파일 구조

```
GameData/
├── CharacterVoiceLines.json          # 메인 캐릭터 보이스 대사
├── ExtendedMonsterVoices.json        # 몬스터 보이스 대사 (F~S급)
└── StoryEventDialogues.json          # 스토리 이벤트 대사
```

---

## 🎯 CharacterVoiceLines.json

### 구조

```json
{
  "characters": [
    {
      "characterId": "hero_minsu",
      "characterName": "김민수",
      "voiceLines": {
        "battle": { /* 전투 관련 대사 */ },
        "daily": { /* 일상 대사 */ },
        "levelup": { /* 레벨업 대사 */ },
        "story": { /* 스토리 대사 */ },
        "interaction": { /* 상호작용 대사 */ },
        "special": { /* 특수 대사 */ }
      }
    }
  ]
}
```

### 사용 예시 (Unity C#)

```csharp
public class VoiceLineManager : MonoBehaviour
{
    [System.Serializable]
    public class VoiceLine
    {
        public string lineId;
        public string text;
        public string textEN;
        public string emotion;
        public string situation;
    }

    public static VoiceLineManager Instance;
    private Dictionary<string, VoiceLine> voiceLineDatabase;

    void Start()
    {
        LoadVoiceLines();
    }

    void LoadVoiceLines()
    {
        // JSON 파일 로드
        TextAsset jsonFile = Resources.Load<TextAsset>("GameData/CharacterVoiceLines");
        // 파싱 및 딕셔너리 저장
    }

    // 전투 공격 대사 재생
    public void PlayAttackLine()
    {
        VoiceLine line = GetRandomLine("hero_attack");
        PlayVoice(line);
    }

    // 레벨업 대사 재생
    public void PlayLevelUpLine(int level)
    {
        if (level == 50 || level == 100)
        {
            // 마일스톤 대사
            VoiceLine line = GetLine($"hero_levelup_milestone_{level}");
            PlayVoice(line);
        }
        else
        {
            // 일반 레벨업 대사
            VoiceLine line = GetRandomLine("hero_levelup_normal");
            PlayVoice(line);
        }
    }

    // 대사 재생
    void PlayVoice(VoiceLine line)
    {
        // UI에 텍스트 표시
        UIManager.Instance.ShowDialogue(line.text);

        // 보이스 오디오 재생 (있는 경우)
        if (HasVoiceAudio(line.lineId))
        {
            AudioManager.Instance.PlayVoice(line.lineId);
        }

        // 감정 애니메이션
        CharacterAnimator.PlayEmotion(line.emotion);
    }
}
```

---

## 🎭 주요 캐릭터 대사 카테고리

### 1. 주인공 (김민수)

#### 전투 대사
- **attack**: 일반 공격 시
- **skill**: 스킬 사용 시
- **victory**: 전투 승리 시
- **hurt**: 피격 시
- **defeat**: 사망 시

```csharp
// 공격 시 랜덤 대사 재생
void OnPlayerAttack()
{
    string[] attackLines = {
        "hero_attack_001",  // "조용히... 끝내자"
        "hero_attack_002",  // "빨리 처리하고 집에 가야지"
        "hero_attack_003",  // "들키면 안 돼..."
    };

    string randomLine = attackLines[Random.Range(0, attackLines.Length)];
    VoiceLineManager.Instance.PlayLine(randomLine);
}
```

#### 일상 대사
- **morning**: 아침 인사
- **night**: 야간 활동
- **idle**: 대기 상태

```csharp
// 시간대별 인사
void UpdateTimeOfDay()
{
    int hour = System.DateTime.Now.Hour;

    if (hour >= 6 && hour < 12)
    {
        // 아침
        PlayLine("hero_morning_001");  // "오늘도 백수답게 늦잠..."
    }
    else if (hour >= 0 && hour < 6)
    {
        // 새벽 (사냥 시간)
        PlayLine("hero_night_003");    // "새벽 3시... 완벽한 시간이네"
    }
}
```

#### 레벨업 대사
- **normal**: 일반 레벨업
- **milestone**: 특정 레벨 (50, 100 등)

```csharp
void OnLevelUp(int newLevel)
{
    if (newLevel % 50 == 0)
    {
        // 마일스톤 대사
        PlayLine($"hero_levelup_milestone_00{newLevel / 50}");
    }
    else
    {
        // 랜덤 일반 대사
        PlayRandomLine("hero_levelup_normal");
    }
}
```

---

### 2. 여동생 (김민지)

#### 대사 카테고리
- **normal**: 일상 대화
- **suspicious**: 의심하는 대화
- **revealed**: 정체 공개 후
- **support**: 응원 대사
- **event**: 이벤트 대사

```csharp
// 의심도에 따른 대사 변경
void UpdateSisterDialogue()
{
    float suspicion = SuspicionManager.Instance.currentSuspicion;

    if (suspicion < 30)
    {
        PlayLine("sister_normal_001");  // "오빠! 오늘도 집에만 있을 거야?"
    }
    else if (suspicion < 70)
    {
        PlayLine("sister_suspicious_001");  // "오빠... 요즘 좀 이상해"
    }
    else
    {
        PlayLine("sister_suspicious_003");  // "혹시... 오빠도 헌터야?"
    }
}
```

---

## 🐉 ExtendedMonsterVoices.json

### 구조

```json
{
  "monstersByRank": {
    "F_Rank": { /* F급 몬스터들 */ },
    "E_Rank": { /* E급 몬스터들 */ },
    "D_Rank": { /* D급 몬스터들 */ },
    "C_Rank": { /* C급 몬스터들 */ },
    "B_Rank": { /* B급 몬스터들 */ },
    "A_Rank": { /* A급 몬스터들 */ },
    "S_Rank": { /* S급 몬스터들 */ },
    "Special": { /* 특수 몬스터들 */ }
  }
}
```

### 사용 예시

```csharp
public class MonsterVoiceManager : MonoBehaviour
{
    public void PlayMonsterLine(string monsterId, string situation)
    {
        // 몬스터 등급과 ID로 대사 검색
        string rank = GetMonsterRank(monsterId);
        VoiceLine line = GetMonsterVoice(rank, monsterId, situation);

        PlayVoice(line);
    }
}

// 사용 예
void OnMonsterEncounter(Monster monster)
{
    MonsterVoiceManager.Instance.PlayMonsterLine(
        monster.id,
        "encounter"
    );
    // 예: "크하하하! 인간 주제에 내 영역에!"
}

void OnMonsterAttack(Monster monster)
{
    MonsterVoiceManager.Instance.PlayMonsterLine(
        monster.id,
        "attack"
    );
}

void OnMonsterDeath(Monster monster)
{
    MonsterVoiceManager.Instance.PlayMonsterLine(
        monster.id,
        "death"
    );
}
```

---

## 📖 StoryEventDialogues.json

### 구조

```json
{
  "storyArcs": {
    "arc1_awakening": { /* Arc 1 대사들 */ },
    "arc2_dimension": { /* Arc 2 대사들 */ },
    "rebirth_system": { /* 환생 관련 */ },
    "seasonal_events": { /* 계절 이벤트 */ },
    "special_endings": { /* 특별 엔딩 */ }
  }
}
```

### 사용 예시

```csharp
public class StoryEventManager : MonoBehaviour
{
    // 스토리 이벤트 재생
    public void PlayStoryEvent(string arcId, string eventId)
    {
        StoryEvent storyEvent = LoadStoryEvent(arcId, eventId);

        StartCoroutine(PlayDialogueSequence(storyEvent.lines));
    }

    IEnumerator PlayDialogueSequence(DialogueLine[] lines)
    {
        foreach (var line in lines)
        {
            // 화자 표시
            UIManager.Instance.ShowSpeaker(line.speaker);

            // 대사 표시
            UIManager.Instance.ShowText(line.text);

            // 감정 표현
            CharacterAnimator.PlayEmotion(line.emotion);

            // 내적 독백 처리
            if (!string.IsNullOrEmpty(line.thought))
            {
                UIManager.Instance.ShowThought(line.thought);
            }

            // 다음 대사까지 대기
            yield return new WaitForSeconds(3f);
        }
    }
}

// Arc 1 Phase 1 이벤트 재생
void OnReachLevel10()
{
    StoryEventManager.Instance.PlayStoryEvent(
        "arc1_awakening",
        "arc1_phase1_003"
    );
    // "레벨 10... 생각보다 강해지는데?"
}
```

---

## 🎮 실전 활용 예시

### 1. 전투 시스템 통합

```csharp
public class BattleSystem : MonoBehaviour
{
    void OnAttack()
    {
        // 공격 애니메이션
        player.PlayAttackAnimation();

        // 공격 대사
        VoiceLineManager.Instance.PlayAttackLine();

        // 데미지 계산
        DealDamage();
    }

    void OnSkillUse(Skill skill)
    {
        // 스킬 애니메이션
        player.PlaySkillAnimation(skill);

        // 스킬 대사
        VoiceLineManager.Instance.PlayLine("hero_skill_002");
        // "진짜 힘을 보여줄게"

        // 스킬 효과
        skill.Execute();
    }

    void OnVictory()
    {
        // 승리 애니메이션
        player.PlayVictoryAnimation();

        // 승리 대사
        VoiceLineManager.Instance.PlayRandomLine("hero_victory");

        // 보상 획득
        GiveRewards();
    }
}
```

### 2. 의심도 시스템 연동

```csharp
public class SuspicionManager : MonoBehaviour
{
    public float currentSuspicion = 0f;

    void Update()
    {
        // 의심도에 따른 대사 트리거
        if (currentSuspicion >= 50 && !suspicion50Triggered)
        {
            VoiceLineManager.Instance.PlayLine("hero_suspicious_001");
            // "앗... 의심받고 있어!"
            suspicion50Triggered = true;
        }

        if (currentSuspicion >= 100)
        {
            TriggerGameOver();
        }
    }

    void TriggerGameOver()
    {
        VoiceLineManager.Instance.PlayLine("hero_suspicious_002");
        // "들켰다... 게임 오버야..."

        GameManager.Instance.GameOver();
    }
}
```

### 3. 시간대별 대사 시스템

```csharp
public class TimeManager : MonoBehaviour
{
    enum TimeOfDay { Morning, Afternoon, Evening, Night, LateNight }

    TimeOfDay currentTime;
    TimeOfDay previousTime;

    void Update()
    {
        UpdateTime();

        if (currentTime != previousTime)
        {
            OnTimeChanged();
            previousTime = currentTime;
        }
    }

    void OnTimeChanged()
    {
        switch (currentTime)
        {
            case TimeOfDay.Morning:
                VoiceLineManager.Instance.PlayLine("hero_morning_001");
                // "오늘도 백수답게 늦잠..."
                break;

            case TimeOfDay.Night:
                VoiceLineManager.Instance.PlayLine("hero_night_001");
                // "드디어 내 시간이야!"
                break;

            case TimeOfDay.LateNight:
                VoiceLineManager.Instance.PlayLine("hero_night_003");
                // "새벽 3시... 완벽한 시간이네"

                // 야간 보너스 적용
                ApplyNightBonus();
                break;
        }
    }
}
```

### 4. 환생 시스템

```csharp
public class RebirthManager : MonoBehaviour
{
    public int rebirthCount = 0;

    public void PerformRebirth()
    {
        rebirthCount++;

        // 환생 횟수에 따른 대사
        if (rebirthCount == 1)
        {
            PlayStoryEvent("rebirth_001");
            // "처음부터 완벽하게 백수가 되자"
        }
        else if (rebirthCount == 10)
        {
            PlayStoryEvent("rebirth_002");
            // "10번째 환생... 이제 좀 익숙해지네"
        }
        else if (rebirthCount == 100)
        {
            PlayStoryEvent("rebirth_003");
            // "100번째 환생... 백수가 뭐였더라?"
        }
        else
        {
            VoiceLineManager.Instance.PlayLine("hero_rebirth_001");
            // "다시 시작이야... 이번엔 완벽하게!"
        }

        // 레벨 초기화 및 보너스 적용
        ResetLevel();
        ApplyRebirthBonus();
    }
}
```

---

## 🔧 고급 기능

### 1. 대사 큐 시스템

```csharp
public class DialogueQueue : MonoBehaviour
{
    Queue<VoiceLine> dialogueQueue = new Queue<VoiceLine>();
    bool isPlaying = false;

    public void EnqueueDialogue(VoiceLine line)
    {
        dialogueQueue.Enqueue(line);

        if (!isPlaying)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (dialogueQueue.Count > 0)
        {
            VoiceLine line = dialogueQueue.Dequeue();

            // 대사 재생
            PlayVoice(line);

            // 대사 길이에 따라 대기
            float duration = CalculateDuration(line.text);
            yield return new WaitForSeconds(duration);
        }

        isPlaying = false;
    }
}
```

### 2. 조건부 대사 시스템

```csharp
public class ConditionalDialogue : MonoBehaviour
{
    public bool CheckCondition(VoiceLine line)
    {
        // 레벨 체크
        if (line.requiredLevel > 0)
        {
            if (PlayerLevel < line.requiredLevel)
                return false;
        }

        // 스토리 진행도 체크
        if (!string.IsNullOrEmpty(line.requiredStory))
        {
            if (!StoryManager.Instance.IsStoryCompleted(line.requiredStory))
                return false;
        }

        // 의심도 체크
        if (line.maxSuspicion > 0)
        {
            if (SuspicionManager.Instance.currentSuspicion > line.maxSuspicion)
                return false;
        }

        return true;
    }
}
```

### 3. 다국어 지원

```csharp
public class LocalizationManager : MonoBehaviour
{
    public enum Language { Korean, English, Japanese, Chinese }
    public Language currentLanguage = Language.Korean;

    public string GetLocalizedText(VoiceLine line)
    {
        switch (currentLanguage)
        {
            case Language.Korean:
                return line.text;
            case Language.English:
                return line.textEN;
            default:
                return line.text;
        }
    }
}
```

---

## 📊 대사 통계

### 전체 대사 수

```
CharacterVoiceLines.json:
- 주인공 (김민수): 80+ 대사
- 여동생 (김민지): 30+ 대사
- 어머니: 10+ 대사
- NPC들: 20+ 대사
- 보스들: 50+ 대사
- 시스템: 20+ 대사

ExtendedMonsterVoices.json:
- F급 몬스터: 40+ 대사
- E급 몬스터: 50+ 대사
- D급 몬스터: 60+ 대사
- C급 몬스터: 70+ 대사
- B급 몬스터: 80+ 대사
- A급 몬스터: 90+ 대사
- S급 몬스터: 100+ 대사
- 특수 몬스터: 50+ 대사

StoryEventDialogues.json:
- Arc 1: 100+ 대사
- Arc 2: 80+ 대사
- 환생 시스템: 30+ 대사
- 계절 이벤트: 40+ 대사
- 특별 엔딩: 30+ 대사

총계: 1000+ 대사
```

---

## 🎨 보이스 연출 가이드

### 감정 표현 목록

```
기본 감정:
- 집중, 무덤덤, 긴장, 자신감, 귀찮음
- 안도, 불안, 만족, 고통, 인내
- 좌절, 아쉬움, 간절, 기쁨, 각오

특수 감정:
- 우스꽝스러운 진지함
- 복잡함
- 허탈
- 식은땀
- 체념
```

### 상황별 연출

```csharp
void ApplyEmotionEffect(string emotion)
{
    switch (emotion)
    {
        case "집중":
            // 눈빛 날카롭게
            characterEyes.Sharpen();
            break;

        case "긴장":
            // 땀 표현
            characterSweat.Show();
            break;

        case "기쁨":
            // 별 이펙트
            PlayEffect("star_sparkle");
            break;

        case "식은땀":
            // 땀방울 애니메이션
            PlayAnimation("cold_sweat");
            break;
    }
}
```

---

## 🔊 오디오 통합

### 보이스 파일 명명 규칙

```
Audio/Voices/
├── Hero/
│   ├── hero_attack_001.wav
│   ├── hero_attack_002.wav
│   ├── hero_skill_001.wav
│   └── ...
├── Sister/
│   ├── sister_normal_001.wav
│   └── ...
├── Monsters/
│   ├── slime_attack_001.wav
│   └── ...
└── System/
    ├── system_levelup_001.wav
    └── ...
```

### 오디오 재생 시스템

```csharp
public class VoiceAudioManager : MonoBehaviour
{
    Dictionary<string, AudioClip> voiceClips;

    public void PlayVoiceAudio(string lineId)
    {
        // 오디오 클립 로드
        AudioClip clip = LoadVoiceClip(lineId);

        if (clip != null)
        {
            // 오디오 소스 재생
            AudioSource voiceSource = GetVoiceAudioSource();
            voiceSource.clip = clip;
            voiceSource.Play();

            // 립싱크 (있는 경우)
            if (HasLipSync)
            {
                StartLipSync(clip.length);
            }
        }
    }

    AudioClip LoadVoiceClip(string lineId)
    {
        // 캐시 확인
        if (voiceClips.ContainsKey(lineId))
        {
            return voiceClips[lineId];
        }

        // 파일 로드
        string path = $"Audio/Voices/{GetCharacterFolder(lineId)}/{lineId}";
        AudioClip clip = Resources.Load<AudioClip>(path);

        // 캐시 저장
        if (clip != null)
        {
            voiceClips[lineId] = clip;
        }

        return clip;
    }
}
```

---

## 💡 베스트 프랙티스

### 1. 대사 재생 우선순위

```csharp
public enum VoicePriority
{
    Low = 0,        // 일반 대사
    Normal = 1,     // 전투 대사
    High = 2,       // 스토리 대사
    Critical = 3    // 중요 이벤트
}

public void PlayVoiceWithPriority(VoiceLine line, VoicePriority priority)
{
    // 현재 재생 중인 대사보다 우선순위가 높으면 중단하고 재생
    if (priority > currentPriority)
    {
        StopCurrentVoice();
        PlayVoice(line);
    }
    else
    {
        // 큐에 추가
        EnqueueVoice(line);
    }
}
```

### 2. 대사 중복 방지

```csharp
public class VoiceHistory : MonoBehaviour
{
    List<string> recentLines = new List<string>();
    int maxHistory = 5;

    public string GetRandomNonRepeatingLine(string category)
    {
        List<string> availableLines = GetLinesInCategory(category);

        // 최근 재생한 대사 제외
        availableLines = availableLines
            .Where(line => !recentLines.Contains(line))
            .ToList();

        if (availableLines.Count == 0)
        {
            // 모두 재생했으면 히스토리 초기화
            recentLines.Clear();
            availableLines = GetLinesInCategory(category);
        }

        // 랜덤 선택
        string selected = availableLines[Random.Range(0, availableLines.Count)];

        // 히스토리에 추가
        recentLines.Add(selected);
        if (recentLines.Count > maxHistory)
        {
            recentLines.RemoveAt(0);
        }

        return selected;
    }
}
```

### 3. 퍼포먼스 최적화

```csharp
public class VoiceOptimization : MonoBehaviour
{
    // 오디오 클립 프리로드
    void PreloadCriticalVoices()
    {
        // 자주 사용하는 대사 미리 로드
        string[] criticalLines = {
            "hero_attack_001",
            "hero_skill_001",
            "hero_victory_001"
        };

        foreach (string lineId in criticalLines)
        {
            VoiceAudioManager.Instance.PreloadVoice(lineId);
        }
    }

    // 메모리 관리
    void UnloadUnusedVoices()
    {
        // 장시간 사용하지 않은 대사 언로드
        VoiceAudioManager.Instance.UnloadInactiveVoices(300f); // 5분
    }
}
```

---

## 🐛 디버깅 도구

### 대사 테스트 도구

```csharp
#if UNITY_EDITOR
public class VoiceLineDebugger : MonoBehaviour
{
    [Header("테스트 설정")]
    public string testLineId;
    public string testCategory;

    [ContextMenu("Play Test Line")]
    void PlayTestLine()
    {
        VoiceLineManager.Instance.PlayLine(testLineId);
    }

    [ContextMenu("Play Random From Category")]
    void PlayRandomFromCategory()
    {
        VoiceLineManager.Instance.PlayRandomLine(testCategory);
    }

    [ContextMenu("List All Lines")]
    void ListAllLines()
    {
        var allLines = VoiceLineManager.Instance.GetAllLines();
        foreach (var line in allLines)
        {
            Debug.Log($"{line.lineId}: {line.text}");
        }
    }
}
#endif
```

---

## 📚 추가 리소스

### JSON 스키마

대사 JSON 파일의 유효성을 검증하기 위한 스키마를 제공합니다.

### 에디터 툴

Unity 에디터에서 대사를 쉽게 관리할 수 있는 커스텀 에디터 툴을 제공할 예정입니다.

### 대사 작성 가이드라인

1. **간결성**: 모바일 게임 특성상 짧고 임팩트 있게
2. **캐릭터성**: 캐릭터의 성격이 드러나도록
3. **상황 적합성**: 게임 상황에 자연스럽게 어울리도록
4. **다양성**: 같은 상황에서도 여러 버전 제공

---

## 🎯 체크리스트

### 구현 필수 항목
- [ ] VoiceLineManager 구현
- [ ] JSON 파일 로드 시스템
- [ ] 대사 재생 시스템
- [ ] 감정 표현 시스템
- [ ] 조건부 대사 시스템
- [ ] 대사 큐 시스템

### 선택 항목
- [ ] 보이스 오디오 통합
- [ ] 립싱크 시스템
- [ ] 다국어 지원
- [ ] 대사 에디터 툴
- [ ] 통계 및 분석

---

**작성일**: 2025-11-19
**버전**: 1.0.0
**담당**: 시나리오팀, 프로그래밍팀

**문의**: 기술 문서 관련 문의는 개발팀으로 연락 주세요.
