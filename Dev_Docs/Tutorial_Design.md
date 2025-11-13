# 튜토리얼 설계서

**프로젝트**: 숨겨진 성장의 백수 영웅
**작성일**: 2025-11-13
**버전**: 1.0
**목표**: 신규 유저의 게임 이해도 향상 및 초기 이탈률 감소

---

## 📋 목차

1. [튜토리얼 개요](#튜토리얼-개요)
2. [튜토리얼 철학](#튜토리얼-철학)
3. [전체 흐름도](#전체-흐름도)
4. [단계별 튜토리얼](#단계별-튜토리얼)
5. [UI 가이드 시스템](#ui-가이드-시스템)
6. [튜토리얼 스킵](#튜토리얼-스킵)
7. [보상 시스템](#보상-시스템)
8. [구현 사양](#구현-사양)

---

## 🎯 튜토리얼 개요

### 목표

1. **핵심 메커니즘 학습**: 클릭, 골드, 레벨업, 던전, 스킬
2. **게임 세계관 이해**: 은둔형 헌터, 의심도 시스템
3. **단기 목표 제시**: 첫 환생까지의 로드맵
4. **이탈률 감소**: 처음 5분 내 재미 제공

### 대상 유저

- 모바일 클리커 게임 경험자: 빠른 진행 선호
- 신규 유저: 상세한 가이드 필요
- 복귀 유저: 튜토리얼 스킵 옵션 제공

### 소요 시간

- **강제 튜토리얼**: 2-3분
- **선택 튜토리얼**: 5-10분 (건너뛰기 가능)
- **전체 완료**: 15-20분

---

## 💡 튜토리얼 철학

### 1. Show, Don't Tell

❌ 나쁜 예:
```
"화면을 탭하면 골드를 획득할 수 있습니다."
```

✅ 좋은 예:
```
[손가락 아이콘이 화면을 탭하는 애니메이션]
→ 유저가 직접 탭
→ "+10 골드" 이펙트와 함께 숫자 증가
```

### 2. 점진적 공개 (Progressive Disclosure)

한 번에 하나의 시스템만 소개합니다.

```
1단계: 클릭만 (다른 UI 숨김)
2단계: 레벨업
3단계: 장비
4단계: 던전
5단계: 스킬
...
```

### 3. 즉각적인 보상

각 튜토리얼 단계마다 보상을 제공합니다.

```
클릭 학습 → 100 골드
첫 레벨업 → 무기 획득
첫 던전 → 가챠 티켓
첫 스킬 → 스킬 포인트 2개
```

### 4. 컨텍스트 기반 도움말

필요할 때만 도움말을 표시합니다.

```
조건: 골드가 10,000 이상인데 장비를 안 샀을 때
→ "장비를 구매하면 더 강해집니다!" 알림
```

---

## 🗺️ 전체 흐름도

```
[게임 시작]
    ↓
[인트로 스토리] (스킵 가능)
    ↓
┌─────────────────────────┐
│ STEP 1: 첫 클릭         │ (강제)
│ - 화면 탭하기            │
│ - 골드 획득 이해         │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 2: 레벨업          │ (강제)
│ - 경험치 바 이해         │
│ - 레벨업 효과            │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 3: 의심도 시스템    │ (강제)
│ - 게임 고유 메커니즘     │
│ - 의심도 게이지 확인     │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 4: 첫 장비         │ (강제)
│ - 인벤토리 열기          │
│ - 장비 착용              │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 5: 던전 입장       │ (강제)
│ - 던전 선택              │
│ - 전투 체험              │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 6: 스킬 배우기      │ (선택)
│ - 스킬 트리 열기         │
│ - 첫 스킬 획득           │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 7: 상점 이용       │ (선택)
│ - 상점 둘러보기          │
│ - 아이템 구매            │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 8: 가챠 체험       │ (선택)
│ - 무료 가챠 1회          │
│ - 가챠 시스템 이해       │
└────────┬────────────────┘
         ↓
┌─────────────────────────┐
│ STEP 9: 환생 안내       │ (선택)
│ - 환생 시스템 설명       │
│ - 장기 목표 제시         │
└────────┬────────────────┘
         ↓
[자유 플레이]
    ↓
[컨텍스트 튜토리얼]
- 의심도 70 도달 시
- 레벨 20 도달 시
- 첫 환생 가능 시
```

---

## 📚 단계별 튜토리얼

### STEP 1: 첫 클릭 (강제, 30초)

**목표**: 기본 클릭 메커니즘 학습

**시나리오**:
```
[화면: 주인공이 소파에 앉아있음]

여동생: "오빠, 또 집에만 있어?"
주인공: "(속으로) 아무도 몰라... 내가 S급 헌터라는 걸..."

[튜토리얼 UI 등장]
┌─────────────────────────────┐
│  화면을 탭해보세요!          │
│       ↓ ↓ ↓                 │
│  [캐릭터를 탭하는 손가락]    │
└─────────────────────────────┘

[유저가 탭]
→ "+10 골드" 데미지 넘버
→ 골드 UI 강조 (빛나는 효과)
→ "좋아요! 계속 탭해보세요!"

[5번 탭 후]
→ "훌륭합니다! 골드 100을 획득했어요!"
→ 보상: 골드 +1,000
```

**가이드 요소**:
- 손가락 아이콘 애니메이션
- 클릭 영역 하이라이트 (Spotlight)
- 다른 UI 비활성화 (어둡게 처리)

**완료 조건**:
- 최소 5번 클릭
- 골드 100 이상 획득

---

### STEP 2: 레벨업 (강제, 30초)

**목표**: 경험치와 레벨업 시스템 이해

**시나리오**:
```
[경험치 바 강조]
┌─────────────────────────────┐
│  경험치를 모아 레벨업하세요!  │
│                              │
│  [████░░░░░░] 40/100 EXP    │
└─────────────────────────────┘

[자동으로 경험치 제공]
→ 경험치 바가 채워짐
→ "레벨 업!" 애니메이션

[레벨업 효과]
→ 캐릭터 주변 빛나는 이펙트
→ "+1 스킬 포인트"
→ "능력치 상승!"

┌─────────────────────────────┐
│  축하합니다! 레벨 2 달성!     │
│                              │
│  보상: 골드 +500            │
│       스킬 포인트 +1         │
└─────────────────────────────┘
```

**가이드 요소**:
- 경험치 바 펄스 효과
- 레벨업 애니메이션 재생
- 스탯 증가 표시 (클릭 파워 +5)

**완료 조건**:
- 레벨 2 도달

---

### STEP 3: 의심도 시스템 (강제, 1분)

**목표**: 게임 고유 메커니즘 이해

**시나리오**:
```
[의심도 게이지 강조]
어머니: "민수야, 요즘 돈이 어디서 생기는 거니?"

┌─────────────────────────────┐
│  ⚠️ 의심도 시스템            │
│                              │
│  가족들이 당신의 활동을       │
│  의심하고 있습니다!           │
│                              │
│  [████████░░] 80%           │
│  위험 수준: 높음              │
└─────────────────────────────┘

[의심도 게이지 설명]
- 의심도가 높아지면?
  ✗ 골드 획득 감소
  ✗ 던전 입장 제한
  ✗ 게임 오버 위험!

- 의심도를 줄이는 방법:
  ✓ 시간이 지나면 자연 감소
  ✓ 무인 던전 이용
  ✓ 야간 시간대 활동
  ✓ 특수 아이템 사용

[튜토리얼 보상으로 의심도 감소 물약 제공]
→ "의심도 감소 물약을 사용해보세요!"
→ [물약 사용] 버튼 강조
→ 의심도 -30

┌─────────────────────────────┐
│  의심도가 감소했습니다!       │
│  이제 안전하게 활동할 수      │
│  있습니다.                   │
└─────────────────────────────┘
```

**가이드 요소**:
- 의심도 게이지 강조
- 경고 애니메이션 (70% 이상 시)
- 감소 방법 툴팁

**완료 조건**:
- 의심도 감소 물약 사용
- 의심도 시스템 이해 확인

---

### STEP 4: 첫 장비 (강제, 1분)

**목표**: 인벤토리와 장비 시스템 학습

**시나리오**:
```
[보상으로 "녹슨 검" 획득]
→ 아이템 획득 팝업

┌─────────────────────────────┐
│  🎁 아이템 획득!             │
│                              │
│  [녹슨 검]                   │
│  공격력 +10                  │
│                              │
│  [인벤토리에서 확인]          │
└─────────────────────────────┘

[인벤토리 버튼 강조]
→ "인벤토리를 열어보세요!"
→ [손가락 가이드]

[인벤토리 열림]
→ 아이템 슬롯 강조
→ "녹슨 검을 더블탭하여 장착하세요!"

[장비 장착]
→ 캐릭터 외형 변화 (검 장착)
→ 스탯 증가 표시
  "클릭 파워: 10 → 20 (+10)"

┌─────────────────────────────┐
│  장비를 착용했습니다!         │
│                              │
│  이제 더 강해졌어요!          │
│  클릭해서 확인해보세요.       │
└─────────────────────────────┘

[클릭 시 데미지 증가 확인]
→ "+20 골드" (이전 +10에서 증가)
```

**가이드 요소**:
- 인벤토리 버튼 펄스
- 아이템 슬롯 하이라이트
- 장착 방법 안내 (더블탭 또는 드래그)
- 스탯 변화 강조

**완료 조건**:
- 인벤토리 열기
- 장비 1개 착용
- 강해진 것 체감 (클릭 파워 증가)

---

### STEP 5: 던전 입장 (강제, 2분)

**목표**: 던전 시스템과 전투 체험

**시나리오**:
```
[메인 화면 하단 "던전" 버튼 강조]
→ "이제 던전에 도전해볼 시간이에요!"
→ [손가락 가이드]

[던전 선택 화면]
┌─────────────────────────────┐
│  🏰 슬라임 동굴              │
│                              │
│  난이도: ★☆☆☆☆             │
│  권장 레벨: Lv 1-5          │
│  에너지: 5                   │
│                              │
│  보상: 골드, 경험치, 장비     │
│                              │
│  [입장하기]                  │
└─────────────────────────────┘

[첫 던전은 에너지 무료]
→ "첫 던전 입장은 무료예요!"
→ [입장하기] 버튼 강조

[전투 화면]
┌─────────────────────────────┐
│  Wave 1 / 5                 │
│                              │
│  🟢 슬라임                   │
│  HP: 50/50                  │
│                              │
│  [탭하여 공격!]              │
└─────────────────────────────┘

[전투 가이드]
→ "몬스터를 탭해서 공격하세요!"
→ [유저가 슬라임 탭]
→ "-20" 데미지 표시
→ 슬라임 HP 감소

[슬라임 처치]
→ "적을 물리쳤습니다!"
→ "+50 골드, +20 EXP"
→ 드롭 아이템 (슬라임 젤리)

[웨이브 진행]
→ Wave 2, 3, 4, 5 자동 진행 (빠르게)

[던전 클리어]
┌─────────────────────────────┐
│  🎉 던전 클리어!             │
│                              │
│  보상:                       │
│  - 골드 +500                │
│  - 경험치 +200              │
│  - 슬라임 젤리 x3           │
│                              │
│  [확인]                     │
└─────────────────────────────┘
```

**가이드 요소**:
- 던전 버튼 펄스
- 던전 카드 설명
- 전투 중 공격 가이드
- 웨이브 진행 표시
- 보상 화면

**완료 조건**:
- 첫 던전 입장
- 슬라임 1마리 이상 처치
- 던전 클리어

---

### STEP 6: 스킬 배우기 (선택, 1분)

**목표**: 스킬 트리 시스템 소개

**시나리오**:
```
[스킬 포인트 알림]
┌─────────────────────────────┐
│  💡 사용 가능한 스킬 포인트   │
│                              │
│  스킬 포인트: 2개            │
│                              │
│  스킬을 배워 더 강해지세요!   │
│                              │
│  [스킬 트리 열기]            │
│  [나중에 하기]               │
└─────────────────────────────┘

[스킬 트리 화면]
→ 3개 카테고리 표시 (은둔/전투/성장)
→ "기본 무술" 스킬 강조

┌─────────────────────────────┐
│  ⚔️ 기본 무술               │
│                              │
│  클릭 파워 +10%             │
│                              │
│  필요 스킬 포인트: 1         │
│                              │
│  [배우기]                   │
└─────────────────────────────┘

[스킬 배우기 버튼 탭]
→ 스킬 획득 애니메이션
→ "클릭 파워가 증가했습니다!"
→ 스탯 창 표시: "클릭 파워 +10%"

[효과 체험]
→ "클릭해서 변화를 느껴보세요!"
→ 데미지 증가 확인
```

**가이드 요소**:
- 스킬 포인트 알림 뱃지
- 스킬 카테고리 설명
- 추천 스킬 표시 (★ 마크)
- 효과 미리보기

**스킵 가능**:
- "나중에 하기" 버튼 제공
- 스킵 시 레벨 10에 다시 알림

---

### STEP 7: 상점 이용 (선택, 1분)

**목표**: 상점 시스템 소개

**시나리오**:
```
[상점 버튼 알림 뱃지]
→ "상점에서 유용한 아이템을 구매할 수 있어요!"

[상점 화면]
┌─────────────────────────────┐
│  🏪 상점                    │
│                              │
│  [일반] [프리미엄] [패키지]  │
│                              │
│  💰 골드 부스트 (30분)      │
│  가격: 3,000 골드           │
│  효과: 골드 획득 +50%       │
│  [구매]                     │
│                              │
│  ⚔️ 철 검                  │
│  가격: 5,000 골드           │
│  공격력: +25                │
│  [구매]                     │
└─────────────────────────────┘

[튜토리얼 보상]
→ "튜토리얼 보너스로 5,000 골드를 드립니다!"
→ 골드 +5,000

[추천 아이템 강조]
→ "철 검을 구매해보세요!"
→ [철 검] 항목 하이라이트

[구매 확인]
→ 아이템 획득 팝업
→ 자동으로 장비 장착
```

**가이드 요소**:
- 상점 카테고리 설명
- 추천 아이템 표시
- 가격 비교 툴팁

**스킵 가능**:
- "나중에 방문하기" 옵션
- 골드가 부족해도 진행 가능

---

### STEP 8: 가챠 체험 (선택, 1분)

**목표**: 가챠 시스템 소개 및 첫 무료 가챠

**시나리오**:
```
[가챠 버튼 강조]
→ "무료 가챠를 돌려보세요!"

[가챠 화면]
┌─────────────────────────────┐
│  🎲 가챠                    │
│                              │
│  첫 1회 무료!                │
│                              │
│  [1회 가챠] [10연차]         │
│                              │
│  확률:                       │
│  일반: 60%                  │
│  희귀: 30%                  │
│  영웅: 9%                   │
│  전설: 0.9%                 │
│  신화: 0.1%                 │
│                              │
│  [뽑기 (무료)]               │
└─────────────────────────────┘

[가챠 연출]
→ 화려한 애니메이션
→ 빛나는 상자 등장
→ 상자 오픈

[결과]
┌─────────────────────────────┐
│  ✨ 축하합니다!              │
│                              │
│  [강철 검] (희귀)            │
│  공격력 +50                 │
│                              │
│  [확인]                     │
└─────────────────────────────┘

→ "더 좋은 장비를 얻었어요!"
→ 자동 장착 안내
```

**가이드 요소**:
- 무료 가챠 강조
- 확률 표기 (법적 요구사항)
- 등급별 색상 구분
- 가챠 연출

**스킵 가능**:
- 첫 무료는 강력 권장하지만 스킵 가능
- 나중에 메인 화면에서 알림

---

### STEP 9: 환생 안내 (선택, 1분)

**목표**: 장기 목표 제시 및 환생 시스템 소개

**시나리오**:
```
[환생 시스템 설명 팝업]
┌─────────────────────────────┐
│  ♻️ 환생 시스템              │
│                              │
│  이 게임의 핵심!              │
│                              │
│  환생하면:                   │
│  ✓ 진행도는 초기화되지만...  │
│  ✓ 영구 보너스 획득!         │
│  ✓ 훨씬 빠른 성장!           │
│  ✓ 무한한 강화!              │
│                              │
│  권장 환생 레벨: 100         │
│                              │
│  [자세히 보기] [알겠어요]     │
└─────────────────────────────┘

[자세히 보기 선택 시]
→ 환생 화면 미리보기
→ 예상 보너스 표시
→ "레벨 100에 환생하면 클릭 파워 +100% 획득!"

[튜토리얼 목표 설정]
┌─────────────────────────────┐
│  🎯 첫 번째 목표             │
│                              │
│  레벨 100 달성 후 환생하기!   │
│                              │
│  현재 진행도:                │
│  [██░░░░░░░░] Lv 2 / 100   │
│                              │
│  예상 환생 보너스:           │
│  - 클릭 파워 +100%          │
│  - 골드 획득 +100%          │
│                              │
│  [확인]                     │
└─────────────────────────────┘
```

**가이드 요소**:
- 환생 개념 간단 설명
- 시각적 다이어그램
- 목표 레벨 제시
- 진행도 표시

**스킵 가능**:
- 선택적 튜토리얼
- 레벨 20, 50에 다시 알림

---

### STEP 10: 자유 플레이 + 컨텍스트 튜토리얼

**목표**: 상황별 추가 도움말 제공

#### 컨텍스트 튜토리얼 트리거

| 조건 | 튜토리얼 내용 | 타이밍 |
|------|--------------|--------|
| 의심도 70% 도달 | 의심도 관리 방법 재안내 | 즉시 |
| 골드 50,000 이상 보유 | 장비 업그레이드 추천 | 30초 후 |
| 레벨 10 도달 | 새 던전 해금 알림 | 즉시 |
| 레벨 20 도달 | 환생 시스템 재안내 | 즉시 |
| 스킬 포인트 5개 이상 | 스킬 투자 권장 | 1분 후 |
| 에너지 MAX | 던전 입장 권장 | 5분 후 |
| 첫 보스 등장 | 보스 전투 팁 | 전투 시작 시 |
| 첫 전설 아이템 획득 | 축하 메시지 + 가챠 안내 | 즉시 |
| 환생 가능 시점 | 환생 권장 알림 | 조건 달성 시 |

---

## 🎨 UI 가이드 시스템

### 1. 손가락 가이드 (FingerGuide)

```csharp
public class FingerGuide : MonoBehaviour
{
    public enum GuideType
    {
        Tap,        // 탭 (클릭)
        Swipe,      // 스와이프
        Hold,       // 길게 누르기
        Drag        // 드래그
    }

    [Header("Settings")]
    public GuideType guideType = GuideType.Tap;
    public Transform targetUI;
    public float animationSpeed = 1.0f;

    [Header("Visual")]
    public GameObject fingerSprite;
    public ParticleSystem tapEffect;

    private void Update()
    {
        if (targetUI != null)
        {
            // 손가락을 타겟 위치로 이동
            transform.position = targetUI.position;

            // 애니메이션
            switch (guideType)
            {
                case GuideType.Tap:
                    AnimateTap();
                    break;
                case GuideType.Swipe:
                    AnimateSwipe();
                    break;
                // ...
            }
        }
    }

    private void AnimateTap()
    {
        // 위아래로 움직이며 탭하는 애니메이션
        float scale = 1f + Mathf.Sin(Time.time * animationSpeed) * 0.1f;
        fingerSprite.transform.localScale = Vector3.one * scale;

        // 주기적으로 탭 이펙트
        if (Mathf.Sin(Time.time * animationSpeed) > 0.9f)
        {
            tapEffect.Play();
        }
    }
}
```

### 2. 스포트라이트 시스템 (Spotlight)

```csharp
public class TutorialSpotlight : MonoBehaviour
{
    [Header("Target")]
    public RectTransform targetUI;

    [Header("Visual")]
    public Image maskImage;
    public float fadeDuration = 0.5f;

    public void ShowSpotlight(RectTransform target)
    {
        targetUI = target;
        StartCoroutine(FadeIn());
    }

    public void HideSpotlight()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        // 화면을 어둡게 하되, 타겟만 밝게
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration * 0.7f;
            maskImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        float timer = 0;
        float startAlpha = maskImage.color.a;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0, timer / fadeDuration);
            maskImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
```

### 3. 메시지 박스 시스템

```csharp
public class TutorialMessageBox : MonoBehaviour
{
    [Header("UI Elements")]
    public Text messageText;
    public Image characterImage;
    public Button nextButton;
    public Button skipButton;

    [Header("Animation")]
    public float typewriterSpeed = 0.05f;
    public Animator boxAnimator;

    private Queue<string> messageQueue = new Queue<string>();

    public void ShowMessage(string message, Sprite character = null)
    {
        messageQueue.Enqueue(message);
        if (character != null)
            characterImage.sprite = character;

        if (messageQueue.Count == 1)
        {
            StartCoroutine(DisplayNextMessage());
        }
    }

    private IEnumerator DisplayNextMessage()
    {
        if (messageQueue.Count == 0)
        {
            Hide();
            yield break;
        }

        string message = messageQueue.Dequeue();

        // 타이핑 효과
        messageText.text = "";
        foreach (char c in message)
        {
            messageText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // 다음 버튼 활성화
        nextButton.interactable = true;
    }

    public void OnNextClicked()
    {
        nextButton.interactable = false;
        StartCoroutine(DisplayNextMessage());
    }

    public void OnSkipClicked()
    {
        messageQueue.Clear();
        TutorialManager.Instance.SkipTutorial();
    }
}
```

---

## ⏭️ 튜토리얼 스킵

### 스킵 정책

| 튜토리얼 유형 | 스킵 가능 여부 | 조건 |
|--------------|--------------|------|
| 강제 (STEP 1-5) | ❌ 불가 | 필수 학습 |
| 선택 (STEP 6-9) | ✅ 가능 | 각 단계마다 스킵 버튼 |
| 컨텍스트 | ✅ 가능 | "다시 보지 않기" 체크박스 |

### 스킵 UI

```
┌─────────────────────────────┐
│  튜토리얼 진행 중...         │
│                              │
│  [진행 상황: 60%]            │
│  [████████░░] 6/10          │
│                              │
│  ☑️ 튜토리얼을 건너뛸까요?  │
│                              │
│  [건너뛰기] [계속하기]        │
└─────────────────────────────┘
```

### 스킵 보상

스킵해도 튜토리얼 보상은 지급합니다.

```csharp
public void SkipTutorial()
{
    // 진행 상황 저장
    PlayerPrefs.SetInt("TutorialCompleted", 1);

    // 모든 보상 일괄 지급
    GiveAllTutorialRewards();

    // 자유 플레이 모드 시작
    StartFreePlay();
}

private void GiveAllTutorialRewards()
{
    GameManager.Instance.AddGold(10000);
    GameManager.Instance.AddGems(500);
    InventoryManager.Instance.AddItem("weapon_sword_002", 1);
    GachaManager.Instance.AddFreeTicket(1);
}
```

---

## 🎁 보상 시스템

### 단계별 보상

| 단계 | 보상 | 가치 |
|------|------|------|
| STEP 1 | 골드 1,000 | 클릭 100회 분량 |
| STEP 2 | 골드 500 + 스킬 포인트 1 | - |
| STEP 3 | 의심도 감소 물약 | 3,000 골드 상당 |
| STEP 4 | 녹슨 검 (일반 등급) | 100 골드 상당 |
| STEP 5 | 골드 500 + 경험치 200 | - |
| STEP 6 | 스킬 포인트 2 | - |
| STEP 7 | 골드 5,000 | 상점 체험용 |
| STEP 8 | 무료 가챠 1회 | 10,000 골드 상당 |
| STEP 9 | 젬 500 | 5,000 원 상당 |
| **전체 완료** | **스타터 팩** | **50,000 골드 + 1,000 젬 + 희귀 장비** |

### 완료 보너스

```csharp
public void OnTutorialComplete()
{
    // 기본 보상
    GameManager.Instance.AddGold(50000);
    GameManager.Instance.AddGems(1000);

    // 특별 아이템
    InventoryManager.Instance.AddItem("weapon_sword_003", 1); // 강철 검 (희귀)
    InventoryManager.Instance.AddItem("boost_gold_1hour", 3); // 골드 부스트 3개

    // 업적 달성
    AchievementManager.Instance.UnlockAchievement("tutorial_complete");

    // 축하 메시지
    UIManager.Instance.ShowBigRewardPopup("튜토리얼 완료!", tutorialRewards);
}
```

---

## 💻 구현 사양

### TutorialManager.cs

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [System.Serializable]
    public class TutorialStep
    {
        public string stepId;
        public string stepName;
        public bool isMandatory;        // 강제 여부
        public bool isCompleted;
        public System.Action onStart;   // 시작 시 실행
        public System.Action onComplete; // 완료 시 실행
    }

    [Header("Tutorial Steps")]
    public List<TutorialStep> tutorialSteps;
    private int currentStepIndex = 0;

    [Header("UI")]
    public TutorialMessageBox messageBox;
    public FingerGuide fingerGuide;
    public TutorialSpotlight spotlight;

    [Header("State")]
    public bool isTutorialActive = false;
    public bool canSkipTutorial = false;

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
        CheckTutorialStatus();
    }

    private void CheckTutorialStatus()
    {
        // 이미 튜토리얼 완료했는지 확인
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            Debug.Log("Tutorial already completed");
            return;
        }

        // 튜토리얼 시작
        StartTutorial();
    }

    public void StartTutorial()
    {
        isTutorialActive = true;
        currentStepIndex = 0;
        StartNextStep();
    }

    public void StartNextStep()
    {
        if (currentStepIndex >= tutorialSteps.Count)
        {
            // 튜토리얼 완료
            CompleteTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStepIndex];

        Debug.Log($"Starting tutorial step: {step.stepName}");

        // 스킵 가능 여부 설정
        canSkipTutorial = !step.isMandatory;

        // 스텝 시작 콜백
        step.onStart?.Invoke();
    }

    public void CompleteCurrentStep()
    {
        if (currentStepIndex >= tutorialSteps.Count)
            return;

        TutorialStep step = tutorialSteps[currentStepIndex];
        step.isCompleted = true;

        Debug.Log($"Completed tutorial step: {step.stepName}");

        // 스텝 완료 콜백
        step.onComplete?.Invoke();

        // 다음 스텝으로
        currentStepIndex++;
        StartNextStep();
    }

    public void SkipTutorial()
    {
        if (!canSkipTutorial)
        {
            Debug.Log("Cannot skip mandatory tutorial");
            return;
        }

        // 모든 보상 지급
        GiveAllRewards();

        // 튜토리얼 완료 처리
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;

        // 완료 보상
        GiveCompletionBonus();

        // 완료 플래그 저장
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        // UI 정리
        messageBox.Hide();
        fingerGuide.gameObject.SetActive(false);
        spotlight.HideSpotlight();

        // 축하 메시지
        UIManager.Instance.ShowNotification("튜토리얼을 완료했습니다!");

        Debug.Log("Tutorial completed!");
    }

    private void GiveAllRewards()
    {
        // 모든 스텝 보상 일괄 지급
        GameManager.Instance.AddGold(10000);
        GameManager.Instance.AddGems(500);
        // ... 기타 보상
    }

    private void GiveCompletionBonus()
    {
        GameManager.Instance.AddGold(50000);
        GameManager.Instance.AddGems(1000);
        InventoryManager.Instance.AddItem("weapon_sword_003", 1);
        AchievementManager.Instance.UnlockAchievement("tutorial_complete");
    }

    // 튜토리얼 헬퍼 메서드
    public void ShowMessage(string message, Sprite character = null)
    {
        messageBox.ShowMessage(message, character);
    }

    public void ShowFingerGuide(Transform target, FingerGuide.GuideType type = FingerGuide.GuideType.Tap)
    {
        fingerGuide.gameObject.SetActive(true);
        fingerGuide.targetUI = target;
        fingerGuide.guideType = type;
    }

    public void HideFingerGuide()
    {
        fingerGuide.gameObject.SetActive(false);
    }

    public void ShowSpotlight(RectTransform target)
    {
        spotlight.ShowSpotlight(target);
    }

    public void HideSpotlight()
    {
        spotlight.HideSpotlight();
    }
}
```

### 튜토리얼 스텝 초기화 예시

```csharp
private void InitializeTutorialSteps()
{
    tutorialSteps = new List<TutorialStep>
    {
        new TutorialStep
        {
            stepId = "step_first_click",
            stepName = "첫 클릭",
            isMandatory = true,
            onStart = () =>
            {
                ShowMessage("화면을 탭해보세요!");
                ShowFingerGuide(characterTransform);
            },
            onComplete = () =>
            {
                HideFingerGuide();
                GameManager.Instance.AddGold(1000);
                ShowMessage("훌륭합니다! 골드를 획득했어요!");
            }
        },

        new TutorialStep
        {
            stepId = "step_level_up",
            stepName = "레벨업",
            isMandatory = true,
            onStart = () =>
            {
                ShowMessage("경험치를 모아 레벨업하세요!");
                ShowSpotlight(expBarRect);
            },
            onComplete = () =>
            {
                HideSpotlight();
                ShowMessage("축하합니다! 레벨 2 달성!");
            }
        },

        // ... 기타 스텝들
    };
}
```

---

## 📊 튜토리얼 분석

### 추적 지표

```csharp
public class TutorialAnalytics
{
    public static void TrackTutorialStart()
    {
        // Firebase Analytics
        Firebase.Analytics.FirebaseAnalytics.LogEvent("tutorial_begin");
    }

    public static void TrackTutorialStep(string stepId, int stepNumber)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "tutorial_step",
            new Firebase.Analytics.Parameter("step_id", stepId),
            new Firebase.Analytics.Parameter("step_number", stepNumber)
        );
    }

    public static void TrackTutorialComplete(float duration)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "tutorial_complete",
            new Firebase.Analytics.Parameter("duration", duration)
        );
    }

    public static void TrackTutorialSkip(int lastStepNumber)
    {
        Firebase.Analytics.FirebaseAnalytics.LogEvent(
            "tutorial_skip",
            new Firebase.Analytics.Parameter("last_step", lastStepNumber)
        );
    }
}
```

### 주요 지표

- **완료율**: 전체 유저 중 튜토리얼을 완료한 비율
- **단계별 이탈률**: 각 단계에서 이탈하는 유저 비율
- **평균 완료 시간**: 튜토리얼 완료까지 걸리는 시간
- **스킵률**: 튜토리얼을 스킵하는 유저 비율

**목표**:
- 완료율 80% 이상
- STEP 1-3 이탈률 5% 이하
- 평균 완료 시간 3-5분
- 스킵률 20% 이하

---

## 📋 체크리스트

### 기획
- [ ] 전체 튜토리얼 흐름 확정
- [ ] 단계별 시나리오 작성
- [ ] 보상 밸런싱
- [ ] 스킵 정책 확정

### UI 구현
- [ ] TutorialMessageBox
- [ ] FingerGuide
- [ ] TutorialSpotlight
- [ ] 스킵 팝업

### 시스템 구현
- [ ] TutorialManager
- [ ] 단계별 트리거
- [ ] 완료 체크
- [ ] 보상 시스템

### 콘텐츠
- [ ] 대사 작성 (한글/영문)
- [ ] 캐릭터 일러스트
- [ ] 손가락 아이콘
- [ ] 이펙트

### 테스트
- [ ] 전체 튜토리얼 플레이 테스트
- [ ] 스킵 기능 테스트
- [ ] 보상 지급 확인
- [ ] 다국어 테스트

### 분석
- [ ] Firebase Analytics 연동
- [ ] 지표 추적 코드
- [ ] A/B 테스트 준비

---

**작성 완료일**: 2025-11-13
**예상 개발 시간**: 5일
**담당**: 기획자, UI 디자이너, 프로그래머
