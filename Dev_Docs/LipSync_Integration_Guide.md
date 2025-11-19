# 립싱크 시스템 통합 가이드

## 📋 개요

**은둔형 백수 영웅** 게임의 립싱크 시스템 완전 통합 가이드입니다.

### 시스템 특징
- ✅ **음소(Phoneme) 기반** 정밀 립싱크
- ✅ **오디오 분석 기반** 자동 립싱크
- ✅ **하이브리드 모드** (음소 + 볼륨)
- ✅ **한국어/영어** 완벽 지원
- ✅ **2D Sprite** 및 **3D Blend Shape** 지원
- ✅ **감정 표현** 연동

---

## 📁 파일 구조

```
Proto/
├── Scripts/
│   └── LipSync/
│       ├── LipSyncController.cs          # 메인 립싱크 컨트롤러
│       ├── LipSyncDatabase.cs            # 음소 데이터베이스
│       └── AudioLipSyncAnalyzer.cs       # 오디오 분석 시스템
│
├── GameData/
│   └── LipSync/
│       ├── PhonemeMappings.json          # 음소-입모양 매핑
│       └── CharacterLipSyncData.json     # 대사별 립싱크 데이터
│
└── Dev_Docs/
    └── LipSync_Integration_Guide.md     # 이 문서
```

---

## 🚀 빠른 시작 (Quick Start)

### 1단계: 프리팹 설정

```
GameObject (캐릭터)
├── Body (SpriteRenderer)
├── Head (SpriteRenderer)
├── Eyes (SpriteRenderer)
└── Mouth (SpriteRenderer) ← 립싱크 적용 대상
```

### 2단계: 컴포넌트 추가

```csharp
// 캐릭터 GameObject에 추가
- LipSyncController
- AudioSource
- (선택) AudioLipSyncAnalyzer
```

### 3단계: Inspector 설정

```
LipSyncController:
- Mouth Renderer: Mouth (SpriteRenderer) 드래그
- Audio Source: AudioSource 컴포넌트 연결
- Sync Mode: Phoneme
- Lip Sync Database: LipSyncDatabase Asset 연결
- Default Mouth: 기본 입 스프라이트 설정
```

### 4단계: 립싱크 재생

```csharp
public class DialogueTest : MonoBehaviour
{
    public LipSyncController lipSync;
    public AudioClip voiceClip;

    void Start()
    {
        // 립싱크 시작
        lipSync.StartLipSync("hero_attack_001", voiceClip);
    }
}
```

---

## 🎨 스프라이트 준비

### 필요한 입 모양 스프라이트

```
Sprites/Mouth/
├── mouth_closed.png        # 입 닫힘 (M, B, P)
├── mouth_narrow.png        # 좁은 입 (I, E narrow)
├── mouth_medium.png        # 중간 입 (E, D, T)
├── mouth_wide.png          # 넓은 입 (A)
├── mouth_o_shape.png       # O 모양 (O, U)
├── mouth_smile.png         # 미소
├── mouth_sad.png           # 슬픔
└── mouth_very_wide.png     # 매우 넓음 (놀람)
```

### 스프라이트 규격 권장사항

- **크기**: 128x128px (캐릭터 크기에 맞춰 조절)
- **포맷**: PNG (투명 배경)
- **피봇**: Center
- **Pixels Per Unit**: 100

---

## 🔧 Unity 설정 가이드

### LipSyncDatabase 생성

1. **Project 창**에서 우클릭
2. `Create > Hermit Hero > Lip Sync Database`
3. 이름: `LipSyncDatabase`

### Database 설정

```csharp
// Inspector에서 설정
Mouth Sprites:
- Size: 8
  - Element 0:
    - Phoneme: "A"
    - Shape: Wide
    - Sprite: mouth_wide
  - Element 1:
    - Phoneme: "E"
    - Shape: Medium
    - Sprite: mouth_medium
  - Element 2:
    - Phoneme: "I"
    - Shape: Narrow
    - Sprite: mouth_narrow
  - Element 3:
    - Phoneme: "O"
    - Shape: O_Shape
    - Sprite: mouth_o_shape
  - Element 4:
    - Phoneme: "U"
    - Shape: O_Shape
    - Sprite: mouth_o_shape
  - Element 5:
    - Phoneme: "M"
    - Shape: Closed
    - Sprite: mouth_closed
  - Element 6:
    - Phoneme: "Smile"
    - Shape: Smile
    - Sprite: mouth_smile
  - Element 7:
    - Phoneme: "Closed"
    - Shape: Closed
    - Sprite: mouth_closed
```

---

## 💻 코드 예제

### 기본 사용법

```csharp
using HermitHero.LipSync;

public class CharacterController : MonoBehaviour
{
    public LipSyncController lipSync;
    public AudioClip voiceClip;

    void Start()
    {
        // 립싱크 시작
        lipSync.StartLipSync("hero_attack_001", voiceClip);
    }

    void OnDialogueEnd()
    {
        // 립싱크 중지
        lipSync.StopLipSync();
    }
}
```

### 텍스트 기반 립싱크 (오디오 없이)

```csharp
public void SpeakText(string text, float duration)
{
    // 텍스트에서 자동으로 음소 생성
    lipSync.StartLipSyncFromText(text, duration);
}

// 사용 예
SpeakText("조용히... 끝내자", 2.0f);
```

### 볼륨 기반 립싱크

```csharp
void Start()
{
    // 립싱크 모드를 Volume으로 설정
    lipSync.syncMode = LipSyncController.LipSyncMode.Volume;
    lipSync.useVolumeBasedSync = true;

    // 립싱크 시작 (데이터 없이도 동작)
    lipSync.StartLipSync("unknown_dialogue", voiceClip);
}
```

### 하이브리드 모드

```csharp
void Start()
{
    // 음소 + 볼륨 조합
    lipSync.syncMode = LipSyncController.LipSyncMode.Hybrid;

    // 립싱크 시작
    lipSync.StartLipSync("hero_attack_001", voiceClip);
    // → 음소 데이터로 입 모양 결정
    // → 볼륨으로 입 크기 조절
}
```

---

## 🎭 립싱크 모드 비교

### 1. Phoneme Mode (음소 기반)

**장점:**
- 가장 정확한 립싱크
- 자연스러운 입 모양
- 감정 표현 가능

**단점:**
- 사전 데이터 필요
- 수동 작업 필요

**사용 시기:**
- 중요한 컷신
- 주요 대사
- 고품질 립싱크가 필요한 경우

```csharp
lipSync.syncMode = LipSyncController.LipSyncMode.Phoneme;
lipSync.StartLipSync("hero_reveal_001", voiceClip);
```

### 2. Volume Mode (볼륨 기반)

**장점:**
- 데이터 불필요
- 자동 작동
- 빠른 구현

**단점:**
- 정확도 낮음
- 단순한 입 모양

**사용 시기:**
- 몬스터 울음소리
- 배경 NPC 대사
- 임시 테스트

```csharp
lipSync.syncMode = LipSyncController.LipSyncMode.Volume;
lipSync.StartLipSync("goblin_attack", voiceClip);
```

### 3. Hybrid Mode (하이브리드)

**장점:**
- 정확도 + 자연스러움
- 볼륨에 따른 강약 표현

**단점:**
- 설정 복잡
- 조정 필요

**사용 시기:**
- 메인 캐릭터 대사
- 감정 표현 중요한 장면

```csharp
lipSync.syncMode = LipSyncController.LipSyncMode.Hybrid;
lipSync.StartLipSync("hero_emotion_001", voiceClip);
```

---

## 📊 립싱크 데이터 생성

### 방법 1: JSON에서 로드

```json
{
  "dialogueId": "hero_attack_001",
  "text": "조용히... 끝내자",
  "duration": 2.0,
  "phonemes": [
    { "phoneme": "J", "shape": "Narrow", "startTime": 0.0, "duration": 0.15 },
    { "phoneme": "O", "shape": "O_Shape", "startTime": 0.15, "duration": 0.2 },
    // ...
  ]
}
```

### 방법 2: 스크립트에서 직접 생성

```csharp
LipSyncData CreateCustomLipSync()
{
    LipSyncData data = new LipSyncData
    {
        dialogueId = "custom_001",
        duration = 3.0f,
        phonemes = new PhonemeData[]
        {
            new PhonemeData
            {
                phoneme = "A",
                shape = MouthShape.Wide,
                startTime = 0.0f,
                duration = 0.3f
            },
            new PhonemeData
            {
                phoneme = "E",
                shape = MouthShape.Medium,
                startTime = 0.3f,
                duration = 0.2f
            },
            // ... 계속
        }
    };

    return data;
}
```

### 방법 3: 오디오 자동 분석

```csharp
public class LipSyncGenerator : MonoBehaviour
{
    public AudioLipSyncAnalyzer analyzer;
    public AudioClip audioClip;

    void Start()
    {
        // 오디오 분석하여 립싱크 데이터 생성
        StartCoroutine(GenerateLipSyncData());
    }

    IEnumerator GenerateLipSyncData()
    {
        LipSyncData data = analyzer.GenerateLipSyncData(
            audioClip,
            "auto_generated_001",
            samplingInterval: 0.05f  // 50ms마다 샘플링
        );

        // 생성된 데이터 저장
        SaveLipSyncData(data);

        yield return null;
    }
}
```

---

## 🎯 실전 예제

### 예제 1: 대화 시스템 통합

```csharp
public class DialogueSystem : MonoBehaviour
{
    public LipSyncController playerLipSync;
    public LipSyncController npcLipSync;
    public LipSyncDatabase database;

    public void PlayDialogue(string dialogueId, bool isPlayer)
    {
        // 대사 데이터 로드
        DialogueData dialogue = LoadDialogue(dialogueId);

        // 오디오 클립 로드
        AudioClip voiceClip = LoadVoiceClip(dialogueId);

        // 립싱크 컨트롤러 선택
        LipSyncController lipSync = isPlayer ? playerLipSync : npcLipSync;

        // 립싱크 재생
        lipSync.StartLipSync(dialogueId, voiceClip);

        // 대사 텍스트 표시
        ShowDialogueText(dialogue.text);

        // 대사 종료 이벤트 등록
        StartCoroutine(WaitForDialogueEnd(voiceClip.length, lipSync));
    }

    IEnumerator WaitForDialogueEnd(float duration, LipSyncController lipSync)
    {
        yield return new WaitForSeconds(duration);

        lipSync.StopLipSync();
        OnDialogueComplete();
    }
}
```

### 예제 2: 전투 중 립싱크

```csharp
public class BattleVoiceManager : MonoBehaviour
{
    public LipSyncController heroLipSync;
    public LipSyncController monsterLipSync;

    public void PlayAttackVoice(bool isHero)
    {
        string dialogueId = isHero ? "hero_attack_001" : "monster_attack_001";
        AudioClip voiceClip = LoadVoiceClip(dialogueId);

        LipSyncController lipSync = isHero ? heroLipSync : monsterLipSync;

        // 짧은 공격 대사 (빠른 립싱크)
        lipSync.lipSyncSpeed = 0.03f;  // 빠르게
        lipSync.StartLipSync(dialogueId, voiceClip);
    }

    public void PlaySkillVoice()
    {
        string dialogueId = "hero_skill_002";
        AudioClip voiceClip = LoadVoiceClip(dialogueId);

        // 스킬 대사 (정확한 립싱크)
        heroLipSync.syncMode = LipSyncController.LipSyncMode.Hybrid;
        heroLipSync.lipSyncSpeed = 0.05f;  // 정확하게
        heroLipSync.StartLipSync(dialogueId, voiceClip);
    }
}
```

### 예제 3: 감정 표현

```csharp
public class EmotionLipSync : MonoBehaviour
{
    public LipSyncController lipSync;

    public void SpeakWithEmotion(string dialogueId, string emotion, AudioClip voiceClip)
    {
        // 립싱크 시작
        lipSync.StartLipSync(dialogueId, voiceClip);

        // 감정에 따른 입 모양 수정
        StartCoroutine(ApplyEmotionModifier(emotion));
    }

    IEnumerator ApplyEmotionModifier(string emotion)
    {
        float modifier = 1.0f;

        switch (emotion)
        {
            case "happy":
                modifier = 1.2f;  // 입을 크게
                lipSync.volumeMultiplier = 3.5f;
                break;

            case "sad":
                modifier = 0.8f;  // 입을 작게
                lipSync.volumeMultiplier = 2.0f;
                break;

            case "angry":
                modifier = 1.3f;  // 입을 크게, 빠르게
                lipSync.lipSyncSpeed = 0.03f;
                break;
        }

        // 립싱크 종료까지 유지
        while (lipSync.isPlaying)
        {
            yield return null;
        }

        // 원래대로 복구
        lipSync.volumeMultiplier = 3.0f;
        lipSync.lipSyncSpeed = 0.05f;
    }
}
```

---

## 🔨 고급 기능

### 음소 자동 생성

```csharp
public class PhonemeGenerator : MonoBehaviour
{
    public LipSyncDatabase database;

    public void GenerateFromText(string text, float duration)
    {
        // 텍스트에서 음소 자동 생성
        List<PhonemeData> phonemes = database.GeneratePhonemesFromText(text, duration);

        // 생성된 음소 확인
        foreach (var phoneme in phonemes)
        {
            Debug.Log($"{phoneme.phoneme} at {phoneme.startTime}s for {phoneme.duration}s");
        }
    }
}
```

### 3D 모델용 Blend Shape 적용

```csharp
using HermitHero.LipSync;

public class BlendShapeLipSync : MonoBehaviour
{
    public SkinnedMeshRenderer faceMesh;
    public LipSyncController lipSync;

    // Blend Shape 인덱스
    private int jawOpenIndex;
    private int mouthWideIndex;
    private int lipsTogetherIndex;

    void Start()
    {
        // Blend Shape 인덱스 찾기
        jawOpenIndex = faceMesh.sharedMesh.GetBlendShapeIndex("JawOpen");
        mouthWideIndex = faceMesh.sharedMesh.GetBlendShapeIndex("MouthWide");
        lipsTogetherIndex = faceMesh.sharedMesh.GetBlendShapeIndex("LipsTogether");
    }

    void Update()
    {
        if (!lipSync.isPlaying) return;

        // 현재 입 모양 가져오기
        MouthShape currentShape = GetCurrentMouthShape();

        // Blend Shape 가중치 설정
        ApplyBlendShapes(currentShape);
    }

    void ApplyBlendShapes(MouthShape shape)
    {
        float jawOpen = 0f;
        float mouthWide = 0f;
        float lipsTogether = 0f;

        switch (shape)
        {
            case MouthShape.Closed:
                jawOpen = 0f;
                lipsTogether = 100f;
                break;

            case MouthShape.Narrow:
                jawOpen = 30f;
                mouthWide = 20f;
                break;

            case MouthShape.Medium:
                jawOpen = 60f;
                mouthWide = 50f;
                break;

            case MouthShape.Wide:
                jawOpen = 90f;
                mouthWide = 80f;
                break;

            case MouthShape.O_Shape:
                jawOpen = 70f;
                mouthWide = 0f;
                break;
        }

        // Blend Shape 적용
        faceMesh.SetBlendShapeWeight(jawOpenIndex, jawOpen);
        faceMesh.SetBlendShapeWeight(mouthWideIndex, mouthWide);
        faceMesh.SetBlendShapeWeight(lipsTogetherIndex, lipsTogether);
    }
}
```

---

## 🎨 아트 가이드

### 입 모양 스프라이트 제작 가이드

#### 1. Closed (닫힌 입)
```
용도: ㅁ, ㅂ, ㅍ, M, B, P
특징: 입술이 완전히 닫힘
그리기: ━
```

#### 2. Narrow (좁은 입)
```
용도: ㅣ, ㅡ, I
특징: 입을 옆으로 좁게
그리기: ━━
```

#### 3. Medium (중간 입)
```
용도: ㅓ, ㅔ, E, D, T
특징: 입이 중간 정도 벌어짐
그리기:  ━━
       ━━
```

#### 4. Wide (넓은 입)
```
용도: ㅏ, ㅐ, A
특징: 입이 크게 벌어짐
그리기:  ━━━

       ━━━
```

#### 5. O_Shape (O 모양)
```
용도: ㅗ, ㅜ, O, U
특징: 입술을 동그랗게
그리기: ┌─┐
       │ │
       └─┘
```

#### 6. Smile (미소)
```
용도: 기쁨 표현
특징: 입꼬리가 올라감
그리기:  ∪
```

#### 7. Sad (슬픔)
```
용도: 슬픔 표현
특징: 입꼬리가 내려감
그리기:  ∩
```

#### 8. VeryWide (매우 넓음)
```
용도: 놀람, 외침
특징: 입이 최대한 벌어짐
그리기:  ━━━━


       ━━━━
```

---

## 🐛 트러블슈팅

### 문제 1: 립싱크가 작동하지 않음

**원인:**
- LipSyncDatabase가 연결되지 않음
- 대사 ID가 존재하지 않음
- AudioSource가 없음

**해결:**
```csharp
// Inspector에서 확인
- LipSyncController의 Lip Sync Database 연결 확인
- Audio Source 컴포넌트 존재 확인
- 대사 ID가 CharacterLipSyncData.json에 있는지 확인
```

### 문제 2: 입이 너무 빠르게 움직임

**원인:**
- lipSyncSpeed 값이 너무 낮음

**해결:**
```csharp
lipSync.lipSyncSpeed = 0.05f;  // 기본값
lipSync.lipSyncSpeed = 0.1f;   // 더 느리게
```

### 문제 3: 볼륨 기반 립싱크가 반응하지 않음

**원인:**
- volumeThreshold 값이 너무 높음
- 오디오 볼륨이 너무 낮음

**해결:**
```csharp
lipSync.volumeThreshold = 0.05f;  // 낮추기
lipSync.volumeMultiplier = 5f;    // 증폭 높이기
```

### 문제 4: 한국어 립싱크가 부정확함

**원인:**
- 한글 음소 분해 오류
- 음소 매핑 누락

**해결:**
```csharp
// LipSyncDatabase의 koreanPhonemeMappings 확인
// PhonemeMappings.json에 누락된 음소 추가
```

---

## 📈 성능 최적화

### 1. 오디오 분석 최적화

```csharp
// FFT 크기 줄이기 (정확도 ↓, 성능 ↑)
analyzer.fftSize = 512;  // 기본 1024에서 512로

// 주파수 대역 수 줄이기
analyzer.frequencyBands = 4;  // 기본 8에서 4로
```

### 2. 립싱크 업데이트 빈도 조절

```csharp
// 매 프레임이 아닌 고정 간격으로 업데이트
private float updateInterval = 0.05f;
private float nextUpdateTime = 0f;

void Update()
{
    if (Time.time >= nextUpdateTime)
    {
        UpdateLipSync();
        nextUpdateTime = Time.time + updateInterval;
    }
}
```

### 3. 스프라이트 아틀라스 사용

```
// 입 스프라이트들을 하나의 아틀라스로 패킹
Sprites/Mouth/ → MouthAtlas.png
```

### 4. 오브젝트 풀링

```csharp
public class LipSyncPool : MonoBehaviour
{
    public GameObject lipSyncPrefab;
    private Queue<LipSyncController> pool = new Queue<LipSyncController>();

    public LipSyncController Get()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        GameObject obj = Instantiate(lipSyncPrefab);
        return obj.GetComponent<LipSyncController>();
    }

    public void Return(LipSyncController lipSync)
    {
        lipSync.StopLipSync();
        pool.Enqueue(lipSync);
    }
}
```

---

## 🧪 테스트 도구

### 립싱크 테스트 씬

```csharp
using UnityEngine;
using HermitHero.LipSync;

public class LipSyncTester : MonoBehaviour
{
    public LipSyncController lipSync;
    public LipSyncDatabase database;

    [Header("Test Settings")]
    public string testDialogueId = "hero_attack_001";
    public AudioClip testVoiceClip;

    [Header("Manual Control")]
    public MouthShape manualShape = MouthShape.Closed;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 500));

        GUILayout.Label("=== Lip Sync Tester ===");

        if (GUILayout.Button("Play Phoneme Lip Sync"))
        {
            lipSync.syncMode = LipSyncController.LipSyncMode.Phoneme;
            lipSync.StartLipSync(testDialogueId, testVoiceClip);
        }

        if (GUILayout.Button("Play Volume Lip Sync"))
        {
            lipSync.syncMode = LipSyncController.LipSyncMode.Volume;
            lipSync.StartLipSync(testDialogueId, testVoiceClip);
        }

        if (GUILayout.Button("Play Hybrid Lip Sync"))
        {
            lipSync.syncMode = LipSyncController.LipSyncMode.Hybrid;
            lipSync.StartLipSync(testDialogueId, testVoiceClip);
        }

        if (GUILayout.Button("Stop"))
        {
            lipSync.StopLipSync();
        }

        GUILayout.Space(20);
        GUILayout.Label("Manual Shape Test:");

        if (GUILayout.Button("Set Manual Shape"))
        {
            Sprite sprite = database.GetMouthSpriteByShape(manualShape);
            SetMouthSprite(sprite);
        }

        GUILayout.EndArea();
    }

    void SetMouthSprite(Sprite sprite)
    {
        if (lipSync.mouthRenderer != null)
        {
            lipSync.mouthRenderer.sprite = sprite;
        }
    }
}
```

---

## 📚 API 레퍼런스

### LipSyncController

```csharp
// 주요 메서드
public void StartLipSync(string dialogueId, AudioClip audioClip)
public void StartLipSyncFromText(string text, float duration)
public void StopLipSync()

// 주요 프로퍼티
public LipSyncMode syncMode { get; set; }
public bool useVolumeBasedSync { get; set; }
public float lipSyncSpeed { get; set; }
public float volumeThreshold { get; set; }
public float volumeMultiplier { get; set; }
```

### LipSyncDatabase

```csharp
// 주요 메서드
public LipSyncData GetLipSyncData(string dialogueId)
public Sprite GetMouthSprite(string phoneme)
public Sprite GetMouthSpriteByShape(MouthShape shape)
public List<PhonemeData> GeneratePhonemesFromText(string text, float duration)
```

### AudioLipSyncAnalyzer

```csharp
// 주요 메서드
public string GetCurrentPhoneme()
public MouthShape GetCurrentMouthShape()
public float GetMouthOpenness()
public LipSyncData GenerateLipSyncData(AudioClip audioClip, string dialogueId, float samplingInterval = 0.1f)
```

---

## ✅ 체크리스트

### 구현 체크리스트

#### 기본 설정
- [ ] LipSyncController 스크립트 추가
- [ ] LipSyncDatabase ScriptableObject 생성
- [ ] 입 모양 스프라이트 8종 준비
- [ ] AudioSource 컴포넌트 추가

#### 데이터 준비
- [ ] PhonemeMappings.json 로드
- [ ] CharacterLipSyncData.json 로드
- [ ] LipSyncDatabase에 스프라이트 매핑
- [ ] 음소 매핑 설정

#### 테스트
- [ ] Phoneme 모드 테스트
- [ ] Volume 모드 테스트
- [ ] Hybrid 모드 테스트
- [ ] 한국어 대사 테스트
- [ ] 영어 대사 테스트

#### 통합
- [ ] DialogueSystem과 연동
- [ ] VoiceLineManager와 연동
- [ ] 감정 시스템과 연동
- [ ] UI 시스템과 연동

#### 최적화
- [ ] 스프라이트 아틀라스 적용
- [ ] 오브젝트 풀링 구현
- [ ] 업데이트 빈도 조절
- [ ] 프로파일링 확인

---

## 🎓 추가 학습 자료

### 음성학 기초
- [한글 자모 발음](https://ko.wikipedia.org/wiki/%ED%95%9C%EA%B8%80_%EC%9E%90%EB%AA%A8)
- [International Phonetic Alphabet](https://en.wikipedia.org/wiki/International_Phonetic_Alphabet)

### Unity 관련
- [Unity Audio Documentation](https://docs.unity3d.com/Manual/Audio.html)
- [Unity Animation System](https://docs.unity3d.com/Manual/AnimationOverview.html)

### 오디오 분석
- [FFT (Fast Fourier Transform)](https://en.wikipedia.org/wiki/Fast_Fourier_transform)
- [Audio Spectrum Analysis](https://en.wikipedia.org/wiki/Spectrum_analyzer)

---

## 📞 지원

### 문의
- 기술 지원: 개발팀
- 버그 리포트: GitHub Issues

### 업데이트 예정
- [ ] 립싱크 에디터 툴
- [ ] 자동 립싱크 생성 개선
- [ ] 추가 언어 지원 (일본어, 중국어)
- [ ] 3D 모델 완벽 지원

---

**작성일**: 2025-11-19
**버전**: 1.0.0
**담당**: 프로그래밍팀, 아트팀

**감사합니다! 행복한 립싱크 개발 되세요! 😊**
