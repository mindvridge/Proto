# UI/UX 가이드라인 (UI/UX Guidelines)
## 숨겨진 성장의 백수 영웅 (Hidden Growth NEET Hero)

---

## 📋 목차

1. [디자인 철학](#1-디자인-철학)
2. [컬러 시스템](#2-컬러-시스템)
3. [타이포그래피](#3-타이포그래피)
4. [아이콘 시스템](#4-아이콘-시스템)
5. [버튼 디자인](#5-버튼-디자인)
6. [UI 컴포넌트](#6-ui-컴포넌트)
7. [레이아웃 원칙](#7-레이아웃-원칙)
8. [화면별 가이드](#8-화면별-가이드)
9. [애니메이션](#9-애니메이션)
10. [피드백 시스템](#10-피드백-시스템)
11. [모바일 최적화](#11-모바일-최적화)
12. [접근성](#12-접근성)
13. [일관성 체크리스트](#13-일관성-체크리스트)

---

## 1. 디자인 철학

### 1.1 핵심 원칙

**"은둔형 외톨이의 숨겨진 영웅담"**

```
🌙 은둔 (Hermit)
- 어두운 색조
- 밤 분위기
- 조용하고 차분한 느낌

⚡ 성장 (Growth)
- 명확한 진행도 표시
- 숫자 상승 강조
- 성취감 시각화

🎮 클리커 (Clicker)
- 즉각적인 피드백
- 만족스러운 타격감
- 중독성 있는 반복

🎭 이중성 (Duality)
- 겉으로는 백수
- 속으로는 영웅
- 두 정체성의 균형
```

### 1.2 디자인 목표

```
✅ 직관성: 설명 없이 이해 가능
✅ 일관성: 모든 화면에서 통일된 경험
✅ 반응성: 모든 인터랙션에 피드백
✅ 효율성: 최소 클릭으로 목표 달성
✅ 몰입감: 방해 요소 최소화
```

### 1.3 타겟 유저

```
주 타겟: 20~40대 남성
- 게임 경험: 중급 이상
- 플레이 스타일: 캐주얼 하드코어
- 선호 장르: RPG, 방치형 게임
- 플레이 환경: 출퇴근, 휴식 시간

부 타겟: 여성 유저
- 귀여운 캐릭터 선호
- 컬렉션 요소 중시
```

---

## 2. 컬러 시스템

### 2.1 Primary 컬러 팔레트

**Main Colors (주요 색상):**

```css
/* 다크 베이스 - 은둔 분위기 */
--color-dark-bg: #1A1A2E;          /* 메인 배경 */
--color-dark-surface: #16213E;     /* 서피스 */
--color-dark-overlay: #0F1419;     /* 오버레이 */

/* 포인트 컬러 - 성장 강조 */
--color-primary: #00D9FF;          /* 메인 블루 (클릭 포인트) */
--color-secondary: #7B2CBF;        /* 보라 (스킬) */
--color-accent: #FFD60A;           /* 골드 (골드/보상) */

/* 기능 색상 */
--color-success: #06D6A0;          /* 성공 (레벨업, 클리어) */
--color-warning: #FFB703;          /* 경고 (의심도 중간) */
--color-danger: #EF233C;           /* 위험 (의심도 높음, HP 낮음) */
--color-info: #4CC9F0;             /* 정보 */
```

**Gradient Colors (그라데이션):**

```css
/* 버튼 그라데이션 */
--gradient-primary: linear-gradient(135deg, #667EEA 0%, #764BA2 100%);
--gradient-success: linear-gradient(135deg, #06D6A0 0%, #048A81 100%);
--gradient-gold: linear-gradient(135deg, #FFD60A 0%, #FCA311 100%);

/* 배경 그라데이션 */
--gradient-bg-night: linear-gradient(180deg, #0F1419 0%, #1A1A2E 100%);
--gradient-bg-dawn: linear-gradient(180deg, #16213E 0%, #283747 100%);
```

### 2.2 Text 컬러

```css
/* 텍스트 */
--text-primary: #FFFFFF;           /* 주요 텍스트 (100% opacity) */
--text-secondary: rgba(255, 255, 255, 0.7);  /* 부가 텍스트 */
--text-disabled: rgba(255, 255, 255, 0.4);   /* 비활성 */
--text-inverse: #1A1A2E;           /* 밝은 배경용 */

/* 강조 텍스트 */
--text-gold: #FFD60A;              /* 골드 숫자 */
--text-exp: #4CC9F0;               /* 경험치 */
--text-damage: #EF233C;            /* 데미지 */
--text-heal: #06D6A0;              /* 회복 */
```

### 2.3 등급별 컬러

**아이템/몬스터 등급:**

```css
--grade-f: #9E9E9E;         /* F급 - 회색 */
--grade-e: #8BC34A;         /* E급 - 연두 */
--grade-d: #4CAF50;         /* D급 - 초록 */
--grade-c: #03A9F4;         /* C급 - 파랑 */
--grade-b: #9C27B0;         /* B급 - 보라 */
--grade-a: #FF9800;         /* A급 - 주황 */
--grade-s: #F44336;         /* S급 - 빨강 */
--grade-ss: #E91E63;        /* SS급 - 핑크 */
--grade-sss: #FFD700;       /* SSS급 - 금색 */
--grade-ex: #00E5FF;        /* EX급 - 시안 */
--grade-mythic: #9C27B0;    /* Mythic - 진보라 */
--grade-infinite: linear-gradient(90deg, #FF00FF, #00FFFF); /* Infinite - 홀로그램 */
```

### 2.4 컬러 사용 가이드

**DO ✅:**
```
✅ 골드는 항상 노란색 (#FFD60A)
✅ 경험치는 파란색 (#4CC9F0)
✅ 위험 상황은 빨간색 (#EF233C)
✅ 성공/완료는 초록색 (#06D6A0)
✅ 등급 컬러는 일관되게 사용
✅ 배경은 어두운 톤 유지
```

**DON'T ❌:**
```
❌ 밝은 배경에 밝은 텍스트
❌ 너무 많은 컬러 동시 사용 (한 화면에 5가지 이내)
❌ 골드를 다른 색으로 표현
❌ 저대비 색상 조합
❌ 너무 화려한 네온 컬러
```

---

## 3. 타이포그래피

### 3.1 폰트 패밀리

**한글 폰트:**

```css
/* 주 폰트 - 본문 */
--font-korean-primary: 'Noto Sans KR', sans-serif;
/* Google Fonts: https://fonts.google.com/noto/specimen/Noto+Sans+KR */

/* 강조 폰트 - 제목, 숫자 */
--font-korean-display: 'Black Han Sans', sans-serif;
/* Google Fonts: https://fonts.google.com/specimen/Black+Han+Sans */

/* 대안 폰트 */
--font-korean-alt: 'Nanum Gothic', sans-serif;
```

**영문/숫자 폰트:**

```css
/* 숫자 강조 - 골드, 경험치 등 */
--font-number: 'Orbitron', monospace;
/* Google Fonts: https://fonts.google.com/specimen/Orbitron */

/* 영문 제목 */
--font-english-display: 'Exo 2', sans-serif;
/* Google Fonts: https://fonts.google.com/specimen/Exo+2 */

/* 영문 본문 */
--font-english-body: 'Roboto', sans-serif;
```

### 3.2 폰트 크기

**모바일 기준 (1080x1920 기준):**

```css
/* 제목 */
--font-size-h1: 48px;      /* 화면 제목 */
--font-size-h2: 36px;      /* 섹션 제목 */
--font-size-h3: 28px;      /* 서브 제목 */

/* 본문 */
--font-size-body-large: 24px;   /* 강조 본문 */
--font-size-body: 20px;         /* 일반 본문 */
--font-size-body-small: 16px;   /* 작은 본문 */

/* 캡션 */
--font-size-caption: 14px;      /* 설명문 */

/* 숫자 (큰 숫자 강조) */
--font-size-number-huge: 64px;  /* 골드, 레벨 */
--font-size-number-large: 48px; /* 스탯 */
--font-size-number-medium: 32px;/* 일반 숫자 */
```

### 3.3 폰트 굵기

```css
--font-weight-light: 300;
--font-weight-regular: 400;
--font-weight-medium: 500;
--font-weight-bold: 700;
--font-weight-black: 900;
```

### 3.4 사용 예시

```css
/* 제목 */
.screen-title {
    font-family: var(--font-korean-display);
    font-size: var(--font-size-h1);
    font-weight: var(--font-weight-black);
    color: var(--text-primary);
}

/* 골드 숫자 */
.gold-amount {
    font-family: var(--font-number);
    font-size: var(--font-size-number-huge);
    font-weight: var(--font-weight-bold);
    color: var(--text-gold);
    text-shadow: 0 0 10px rgba(255, 214, 10, 0.5);
}

/* 일반 본문 */
.body-text {
    font-family: var(--font-korean-primary);
    font-size: var(--font-size-body);
    font-weight: var(--font-weight-regular);
    color: var(--text-secondary);
    line-height: 1.6;
}
```

---

## 4. 아이콘 시스템

### 4.1 아이콘 스타일

**디자인 원칙:**

```
스타일: 라인 + 솔리드 조합
크기: 64x64, 128x128 (2배수)
선 굵기: 4px (64x64), 8px (128x128)
라운드: 4px 코너
배경: 반투명 또는 그라데이션
```

### 4.2 아이콘 카테고리

**스탯 아이콘:**

```
⚔️ 공격력 (Attack): 칼 모양, 빨간색
🛡️ 방어력 (Defense): 방패 모양, 파란색
❤️ HP: 하트, 빨간색
⚡ 크리티컬: 번개, 노란색
💰 골드: 동전, 금색
✨ 경험치: 별, 파란색
```

**시스템 아이콘:**

```
⚙️ 설정: 톱니바퀴
❓ 도움말: 물음표
🏠 홈: 집 모양
🎒 인벤토리: 가방
📖 스킬: 책
🏆 업적: 트로피
🏪 상점: 가게
🎯 퀘스트: 느낌표
```

**버튼 아이콘:**

```
✓ 확인: 체크마크
✗ 취소: X 표시
+ 추가: 플러스
- 제거: 마이너스
▶ 다음: 화살표 오른쪽
◀ 이전: 화살표 왼쪽
🔄 새로고침: 순환 화살표
🔒 잠금: 자물쇠
🔓 해제: 열린 자물쇠
```

### 4.3 아이콘 상태

**3가지 상태:**

```
Normal (일반):
- Opacity: 100%
- Color: 기본 색상

Hover/Active (활성):
- Opacity: 100%
- Color: 밝게 (120%)
- Scale: 110%
- Glow: 발광 효과

Disabled (비활성):
- Opacity: 40%
- Color: 회색 (#9E9E9E)
- No interaction
```

### 4.4 Unity 구현

```csharp
// IconManager.cs
public class IconManager : MonoBehaviour
{
    [System.Serializable]
    public class IconSet
    {
        public string iconName;
        public Sprite normalIcon;
        public Sprite activeIcon;  // 선택사항
        public Color normalColor = Color.white;
        public Color activeColor = Color.white;
    }

    public List<IconSet> icons;

    public Sprite GetIcon(string name, bool isActive = false)
    {
        IconSet icon = icons.Find(i => i.iconName == name);
        if (icon != null)
        {
            return isActive && icon.activeIcon != null
                ? icon.activeIcon
                : icon.normalIcon;
        }
        return null;
    }

    public void SetIconState(Image iconImage, bool isActive)
    {
        if (isActive)
        {
            iconImage.color = Color.white;
            iconImage.transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            iconImage.color = new Color(1, 1, 1, 0.4f);
            iconImage.transform.localScale = Vector3.one;
        }
    }
}
```

---

## 5. 버튼 디자인

### 5.1 버튼 타입

**Primary 버튼 (주요 액션):**

```css
배경: var(--gradient-primary)
텍스트: #FFFFFF
높이: 80px
코너: 12px rounded
그림자: 0 4px 8px rgba(0, 0, 0, 0.3)

사용: 확인, 구매, 시작 등
```

**Secondary 버튼 (부가 액션):**

```css
배경: transparent
테두리: 2px solid var(--color-primary)
텍스트: var(--color-primary)
높이: 80px
코너: 12px rounded

사용: 취소, 닫기 등
```

**Danger 버튼 (위험 액션):**

```css
배경: var(--color-danger)
텍스트: #FFFFFF
높이: 80px
코너: 12px rounded

사용: 삭제, 초기화 등
```

**Icon 버튼 (아이콘만):**

```css
크기: 64x64
배경: rgba(255, 255, 255, 0.1)
아이콘: 32x32
코너: 50% (원형)

사용: 설정, 닫기, 정보 등
```

### 5.2 버튼 상태

**Normal (일반):**
```
Opacity: 100%
Scale: 1.0
Shadow: 0 4px 8px rgba(0, 0, 0, 0.3)
```

**Pressed (눌림):**
```
Opacity: 90%
Scale: 0.95
Shadow: 0 2px 4px rgba(0, 0, 0, 0.3)
Y Position: +2px
```

**Disabled (비활성):**
```
Opacity: 40%
Grayscale: 100%
No interaction
```

**Hover (PC, 선택사항):**
```
Opacity: 100%
Scale: 1.05
Glow: 발광 효과
```

### 5.3 버튼 크기

```css
/* 높이 */
--button-height-large: 100px;   /* 큰 버튼 */
--button-height-medium: 80px;   /* 일반 버튼 */
--button-height-small: 60px;    /* 작은 버튼 */

/* 최소 너비 */
--button-min-width: 200px;

/* 패딩 */
--button-padding-h: 32px;       /* 좌우 */
--button-padding-v: 16px;       /* 상하 */
```

### 5.4 Unity 구현

```csharp
// CustomButton.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // DOTween 사용

[RequireComponent(typeof(Button))]
public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector2 originalPosition;

    [Header("Animation Settings")]
    public float pressScale = 0.95f;
    public float pressDuration = 0.1f;
    public float pressYOffset = 2f;

    void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // 스케일 축소
        rectTransform.DOScale(originalScale * pressScale, pressDuration);

        // Y축 이동
        rectTransform.DOAnchorPosY(originalPosition.y - pressYOffset, pressDuration);

        // 사운드
        AudioManager.Instance?.PlaySound("button_press");

        // 햅틱 (진동)
        #if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
        #endif
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;

        // 원래 크기로
        rectTransform.DOScale(originalScale, pressDuration);

        // 원래 위치로
        rectTransform.DOAnchorPosY(originalPosition.y, pressDuration);
    }

    // 비활성화 시 회색조
    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;

        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.color = interactable
                ? Color.white
                : new Color(1, 1, 1, 0.4f);
        }
    }
}
```

---

## 6. UI 컴포넌트

### 6.1 프로그레스 바 (Progress Bar)

**디자인:**

```css
높이: 24px
배경: rgba(0, 0, 0, 0.3)
전경: 그라데이션 (타입별)
코너: 12px rounded
테두리: 2px solid rgba(255, 255, 255, 0.2)

타입별 색상:
- HP: 빨간색 그라데이션
- 경험치: 파란색 그라데이션
- 의심도: 노란색→빨간색 그라데이션
- 로딩: 보라색 그라데이션
```

**Unity 구현:**

```csharp
// CustomProgressBar.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CustomProgressBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public Text valueText;
    public Text labelText;

    [Header("Settings")]
    public Gradient fillGradient; // Inspector에서 설정
    public float animationDuration = 0.5f;
    public bool showPercentage = true;

    private float currentValue = 0f;
    private float maxValue = 100f;

    public void SetValue(float value, float max, bool animate = true)
    {
        currentValue = value;
        maxValue = max;

        float fillAmount = Mathf.Clamp01(value / max);

        if (animate)
        {
            fillImage.DOFillAmount(fillAmount, animationDuration);
        }
        else
        {
            fillImage.fillAmount = fillAmount;
        }

        // 그라데이션 색상 적용
        fillImage.color = fillGradient.Evaluate(fillAmount);

        // 텍스트 업데이트
        if (valueText != null)
        {
            if (showPercentage)
            {
                valueText.text = $"{(fillAmount * 100):F0}%";
            }
            else
            {
                valueText.text = $"{value:F0} / {max:F0}";
            }
        }
    }
}
```

### 6.2 카드 (Card)

**디자인:**

```css
배경: rgba(22, 33, 62, 0.8)
코너: 16px rounded
그림자: 0 8px 16px rgba(0, 0, 0, 0.4)
테두리: 2px solid rgba(255, 255, 255, 0.1)
패딩: 24px

등급별 테두리:
- F: 회색
- S: 빨간색 발광
- SSS: 금색 발광
```

### 6.3 모달 (Modal/Popup)

**디자인:**

```css
배경 오버레이: rgba(0, 0, 0, 0.7)
팝업 배경: var(--color-dark-surface)
크기: 최대 80% 화면
코너: 24px rounded
그림자: 0 16px 32px rgba(0, 0, 0, 0.6)

구조:
- 헤더 (80px)
- 컨텐츠 (가변)
- 푸터/버튼 (100px)
```

**Unity 구현:**

```csharp
// PopupManager.cs
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Prefabs")]
    public GameObject popupPrefab;
    public GameObject overlayPrefab;

    private Stack<GameObject> activePopups = new Stack<GameObject>();

    public void ShowPopup(string title, string message,
        System.Action onConfirm = null, System.Action onCancel = null)
    {
        // 오버레이 생성
        GameObject overlay = Instantiate(overlayPrefab, transform);
        CanvasGroup overlayGroup = overlay.GetComponent<CanvasGroup>();
        overlayGroup.alpha = 0;
        overlayGroup.DOFade(1, 0.3f);

        // 팝업 생성
        GameObject popup = Instantiate(popupPrefab, transform);
        RectTransform popupRect = popup.GetComponent<RectTransform>();

        // 애니메이션 (아래에서 위로)
        popupRect.anchoredPosition = new Vector2(0, -1000);
        popupRect.DOAnchorPosY(0, 0.3f).SetEase(Ease.OutBack);

        // 팝업 설정
        var popupUI = popup.GetComponent<PopupUI>();
        popupUI.SetContent(title, message);
        popupUI.onConfirm = onConfirm;
        popupUI.onCancel = onCancel;

        activePopups.Push(popup);

        // 사운드
        AudioManager.Instance?.PlaySound("popup_open");
    }

    public void CloseTopPopup()
    {
        if (activePopups.Count == 0) return;

        GameObject popup = activePopups.Pop();
        RectTransform popupRect = popup.GetComponent<RectTransform>();

        // 애니메이션 (위에서 아래로)
        popupRect.DOAnchorPosY(-1000, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(popup));

        // 사운드
        AudioManager.Instance?.PlaySound("popup_close");
    }
}
```

### 6.4 토스트 (Toast Notification)

```css
위치: 화면 상단 중앙
크기: 가변 너비, 높이 60px
배경: rgba(0, 0, 0, 0.9)
코너: 30px rounded
지속 시간: 2~3초

타입별:
- 성공: 초록 아이콘
- 경고: 노란 아이콘
- 오류: 빨간 아이콘
- 정보: 파란 아이콘
```

---

## 7. 레이아웃 원칙

### 7.1 Grid System

**모바일 기준 (1080px 너비):**

```
마진 (Margin): 32px (좌우)
거터 (Gutter): 16px (컴포넌트 간격)
컬럼: 12 column grid

Safe Area:
- Top: 88px (노치 대응)
- Bottom: 88px (홈 인디케이터)
- Left/Right: 32px
```

### 7.2 여백 (Spacing)

```css
--spacing-xs: 4px;
--spacing-sm: 8px;
--spacing-md: 16px;
--spacing-lg: 24px;
--spacing-xl: 32px;
--spacing-2xl: 48px;
--spacing-3xl: 64px;
```

### 7.3 화면 구조

**기본 레이아웃:**

```
┌─────────────────────────────────┐
│  Header (120px)                  │ ← 타이틀, 골드, 젬, 설정
├─────────────────────────────────┤
│                                  │
│  Content Area (가변)              │ ← 메인 컨텐츠
│                                  │
│                                  │
├─────────────────────────────────┤
│  Bottom Navigation (100px)       │ ← 메뉴 탭
└─────────────────────────────────┘

비율: 약 10:70:10
```

### 7.4 터치 영역

```
최소 터치 영역: 88x88 px (44x44 dp)
권장 터치 영역: 120x120 px
버튼 간 최소 간격: 16px
```

---

## 8. 화면별 가이드

### 8.1 타이틀 화면 (Scene_01_Title)

**레이아웃:**

```
┌─────────────────────────────────┐
│                                  │
│  [로고]                           │ ← 상단 1/3
│  숨겨진 성장의 백수 영웅            │
│                                  │
├─────────────────────────────────┤
│                                  │
│  [캐릭터 일러스트]                 │ ← 중앙 1/3
│                                  │
├─────────────────────────────────┤
│                                  │
│  [시작하기 버튼]                   │ ← 하단 1/3
│  [계속하기 버튼]                   │
│  [설정] [크레딧]                  │
│                                  │
└─────────────────────────────────┘
```

**애니메이션:**
- 로고: Fade In + Scale (1초)
- 캐릭터: Fade In (1.5초)
- 버튼: Slide Up (2초)

### 8.2 메인 게임 화면 (Scene_02_MainGame)

**레이아웃:**

```
┌─────────────────────────────────┐
│ Lv.50  ❤️123/150  💰12,345     │ ← Top Bar
├─────────────────────────────────┤
│                                  │
│  [몬스터 영역]                     │ ← 40%
│  HP 바, 이름, 이펙트               │
│                                  │
├─────────────────────────────────┤
│  [스킬 버튼 4개]                   │ ← 15%
│  ⚔️ 🛡️ ⚡ 🔥                    │
├─────────────────────────────────┤
│  [스탯 정보]                       │ ← 20%
│  공격력, 방어력, 크리티컬            │
├─────────────────────────────────┤
│ [메뉴] [인벤] [스킬] [상점] [더보기]│ ← Bottom Nav
└─────────────────────────────────┘
```

**클릭 피드백:**
- 데미지 숫자 표시 (Floating Text)
- 파티클 효과
- 화면 쉐이크 (크리티컬 시)
- 사운드

### 8.3 인벤토리 화면 (Scene_05_Inventory)

**레이아웃:**

```
┌─────────────────────────────────┐
│  [← 뒤로]  인벤토리  [정렬 ↓]     │
├─────────────────────────────────┤
│  [장착 슬롯]                       │
│  ┌───┬───┬───┐                  │
│  │무기│방어│악세│                  │
│  └───┴───┴───┘                  │
├─────────────────────────────────┤
│  [탭] 전체 | 장비 | 소비 | 재료   │
├─────────────────────────────────┤
│  [아이템 그리드] 10x10            │
│  ┌──┬──┬──┬──┬──┐            │
│  │  │  │  │  │  │            │
│  ├──┼──┼──┼──┼──┤            │
│  │  │  │  │  │  │            │
│  └──┴──┴──┴──┴──┘            │
└─────────────────────────────────┘
```

### 8.4 스킬 트리 화면 (Scene_08_SkillTree)

```
┌─────────────────────────────────┐
│  스킬 포인트: 15  [초기화]         │
├─────────────────────────────────┤
│  [탭] 은둔 | 전투 | 성장           │
├─────────────────────────────────┤
│  [스킬 트리 - 스크롤]              │
│        🔒                        │
│       ╱ ╲                       │
│      ●   ●                      │
│     ╱ ╲ ╱ ╲                    │
│    ●   ●   🔒                   │
│   ╱                             │
│  ●  (하단으로 확장)               │
└─────────────────────────────────┘

● 학습 완료
🔒 잠금
○ 학습 가능
```

---

## 9. 애니메이션

### 9.1 화면 전환 (Scene Transition)

**Fade Transition:**
```csharp
public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;

    public void FadeOut(System.Action onComplete)
    {
        fadePanel.DOFade(1, 0.5f)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void FadeIn()
    {
        fadePanel.DOFade(0, 0.5f);
    }
}
```

**Slide Transition:**
- 좌→우: 뒤로 가기
- 우→좌: 앞으로 가기
- 하→상: 팝업 열기
- 상→하: 팝업 닫기

### 9.2 UI 애니메이션

**Floating Text (데미지, 보상):**

```csharp
public class FloatingText : MonoBehaviour
{
    public void Show(string text, Color color, Vector3 startPos)
    {
        Text textComponent = GetComponent<Text>();
        textComponent.text = text;
        textComponent.color = color;

        transform.position = startPos;

        // 위로 떠오르며 Fade Out
        transform.DOMoveY(startPos.y + 100, 1f);
        textComponent.DOFade(0, 1f)
            .OnComplete(() => Destroy(gameObject));

        // 스케일 애니메이션
        transform.localScale = Vector3.zero;
        transform.DOScale(1.5f, 0.2f)
            .OnComplete(() => transform.DOScale(1, 0.1f));
    }
}

// 사용 예시
public void ShowDamage(int damage, bool isCritical)
{
    GameObject floatingText = Instantiate(floatingTextPrefab);
    FloatingText ft = floatingText.GetComponent<FloatingText>();

    string text = isCritical ? $"Critical!\n{damage}" : damage.ToString();
    Color color = isCritical ? Color.red : Color.white;

    ft.Show(text, color, damageSpawnPoint.position);
}
```

**Number Count Up:**

```csharp
public class NumberCountUp : MonoBehaviour
{
    public Text numberText;

    public void CountUp(int from, int to, float duration = 1f)
    {
        DOTween.To(() => from, x => from = x, to, duration)
            .OnUpdate(() => {
                numberText.text = from.ToString("N0"); // 천 단위 콤마
            });

        // 사운드 (틱틱틱)
        StartCoroutine(PlayCountSound(duration));
    }

    IEnumerator PlayCountSound(float duration)
    {
        int ticks = Mathf.CeilToInt(duration / 0.05f);
        for (int i = 0; i < ticks; i++)
        {
            AudioManager.Instance?.PlaySound("number_tick", 0.3f);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
```

### 9.3 로딩 애니메이션

**스피너:**

```csharp
public class LoadingSpinner : MonoBehaviour
{
    public RectTransform spinner;

    void Update()
    {
        spinner.Rotate(0, 0, -360 * Time.deltaTime); // 초당 1회전
    }
}
```

**프로그레스 바:**

```csharp
public class LoadingBar : MonoBehaviour
{
    public Image fillImage;
    public Text percentText;

    public void SetProgress(float progress)
    {
        fillImage.fillAmount = progress;
        percentText.text = $"{(progress * 100):F0}%";
    }
}
```

---

## 10. 피드백 시스템

### 10.1 시각적 피드백

**성공 액션:**
```
- 초록색 플래시
- 체크마크 아이콘
- 파티클 (별, 반짝임)
- 스케일 펄스 (1.0 → 1.2 → 1.0)
```

**실패 액션:**
```
- 빨간색 플래시
- X 아이콘
- 흔들림 (Shake)
- 어둡게 깜빡임
```

**레벨업:**
```
- 황금빛 파티클
- 화면 플래시 (흰색)
- "LEVEL UP!" 텍스트
- 레벨 숫자 확대
```

### 10.2 사운드 피드백

```csharp
// SoundType.cs
public enum SoundType
{
    // UI
    ButtonClick,
    ButtonPress,
    PopupOpen,
    PopupClose,
    TabSwitch,

    // 게임플레이
    Click,
    CriticalHit,
    MonsterDeath,
    LevelUp,
    SkillUse,

    // 보상
    GoldGet,
    ItemGet,
    AchievementUnlock,

    // 기타
    Error,
    Success,
    NumberTick
}

// AudioManager.cs
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public List<Sound> sounds;
    private AudioSource[] audioSources;

    public void PlaySound(SoundType type, float volumeMultiplier = 1f)
    {
        Sound sound = sounds.Find(s => s.type == type);
        if (sound != null)
        {
            AudioSource source = GetAvailableAudioSource();
            source.clip = sound.clip;
            source.volume = sound.volume * volumeMultiplier;
            source.Play();
        }
    }
}
```

### 10.3 햅틱 피드백 (진동)

```csharp
public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    public void LightImpact()
    {
        #if UNITY_IOS
        // iOS Taptic Engine
        // 외부 플러그인 필요
        #elif UNITY_ANDROID
        Handheld.Vibrate(); // 짧은 진동
        #endif
    }

    public void MediumImpact()
    {
        #if UNITY_ANDROID
        AndroidVibrator.Vibrate(50); // 50ms
        #endif
    }

    public void HeavyImpact()
    {
        #if UNITY_ANDROID
        AndroidVibrator.Vibrate(100); // 100ms
        #endif
    }
}

// 사용 예시
public void OnCriticalHit()
{
    HapticManager.Instance?.HeavyImpact();
    AudioManager.Instance?.PlaySound(SoundType.CriticalHit);
    // 시각 효과...
}
```

---

## 11. 모바일 최적화

### 11.1 해상도 대응

**기준 해상도:**
- 1080 x 1920 (Full HD)
- 1080 x 2340 (19.5:9, 노치)
- 1440 x 3040 (QHD+)

**Canvas Scaler 설정:**

```csharp
Canvas Scaler:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1080 x 1920
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (가로/세로 균형)
```

### 11.2 Safe Area 대응

```csharp
public class SafeAreaFitter : MonoBehaviour
{
    void Awake()
    {
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
```

### 11.3 터치 최적화

```csharp
// 터치 영역 확대
public class TouchAreaExpander : MonoBehaviour
{
    public Vector2 expandSize = new Vector2(20, 20);

    void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta += expandSize;
    }
}

// 더블 터치 방지
public class DoubleTouchPrevention : MonoBehaviour
{
    private static float lastClickTime = 0f;
    private const float DOUBLE_CLICK_THRESHOLD = 0.3f;

    public void OnButtonClick()
    {
        if (Time.time - lastClickTime < DOUBLE_CLICK_THRESHOLD)
        {
            return; // 너무 빠른 클릭 무시
        }

        lastClickTime = Time.time;

        // 실제 버튼 액션
        DoAction();
    }
}
```

---

## 12. 접근성

### 12.1 색맹 대응

**문제 있는 조합:**
```
❌ 빨강-초록 (적록색맹)
❌ 파랑-노랑 (청황색맹)
```

**해결책:**
```
✅ 색상 + 아이콘 조합
✅ 색상 + 패턴 조합
✅ 명도 차이 충분히
✅ 색맹 모드 제공 (선택)
```

### 12.2 텍스트 가독성

```
최소 대비율: 4.5:1 (일반 텍스트)
최소 대비율: 3:1 (큰 텍스트, 24px+)

✅ 흰색 텍스트 on 어두운 배경
✅ 그림자/아웃라인으로 가독성 향상
```

### 12.3 터치 접근성

```
✅ 최소 터치 영역: 88x88px
✅ 터치 간격: 16px 이상
✅ 중요 버튼: 화면 중앙 하단
✅ 한 손 조작 고려
```

---

## 13. 일관성 체크리스트

### 13.1 컬러 체크

```
[ ] 골드는 항상 #FFD60A
[ ] 경험치는 항상 #4CC9F0
[ ] 등급별 색상 일관성
[ ] 배경 톤 통일 (어두운 톤)
[ ] 버튼 색상 규칙 준수
```

### 13.2 타이포그래피 체크

```
[ ] 제목은 Black Han Sans
[ ] 본문은 Noto Sans KR
[ ] 숫자는 Orbitron
[ ] 폰트 크기 규칙 준수
[ ] 최소 폰트: 14px
```

### 13.3 레이아웃 체크

```
[ ] 여백 규칙 준수 (8px 단위)
[ ] Safe Area 대응
[ ] 터치 영역 충분
[ ] 일관된 정렬
[ ] 컴포넌트 간격 일정
```

### 13.4 인터랙션 체크

```
[ ] 모든 버튼에 피드백
[ ] 로딩 중 표시
[ ] 에러 메시지 표시
[ ] 성공/실패 알림
[ ] 애니메이션 일관성
```

### 13.5 사운드 체크

```
[ ] 모든 버튼 클릭 사운드
[ ] 중요 액션 사운드
[ ] 배경 음악 적절
[ ] 볼륨 밸런스
[ ] 사운드 on/off 옵션
```

---

## 부록 A: Unity UI 템플릿

### Button Prefab 구조

```
Button (GameObject)
├─ Image (Background)
│  └─ Shadow (Image)
├─ Text (Label)
└─ Icon (Image, optional)

Components:
- Button
- CustomButton (스크립트)
- CanvasGroup (Fade용)
```

### Card Prefab 구조

```
Card (GameObject)
├─ Background (Image)
├─ Header (GameObject)
│  ├─ Icon (Image)
│  └─ Title (Text)
├─ Content (GameObject)
│  └─ Description (Text)
└─ Footer (GameObject)
   └─ Button
```

---

## 부록 B: 추천 에셋

**무료 에셋:**
```
- TextMesh Pro (폰트)
- DOTween (애니메이션)
- Lean Touch (터치 입력)
```

**유료 에셋 (선택):**
```
- Modern UI Pack ($20)
- Sci-Fi UI Pack ($30)
- Particle Pack ($15)
```

---

## 부록 C: 디자인 도구

**디자인 툴:**
```
Figma: 무료 (UI 목업)
Adobe XD: 무료 (프로토타입)
Photoshop: $10/월 (그래픽)
Aseprite: $20 (픽셀 아트)
```

**컬러 툴:**
```
Coolors.co: 팔레트 생성
Adobe Color: 색상 조합
Contrast Checker: 대비 확인
```

**폰트:**
```
Google Fonts: 무료
DaFont: 무료
Font Awesome: 아이콘 폰트
```

---

**작성일:** 2025-11-13
**버전:** 1.0
**작성자:** AI Assistant
**최종 수정:** 2025-11-13

**다음 업데이트:**
- 실제 디자인 적용 후 수정
- 유저 테스트 피드백 반영
- 새로운 컴포넌트 추가
