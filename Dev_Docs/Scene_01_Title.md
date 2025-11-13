# Scene 01: 타이틀 & 메인 메뉴 - 개발 명세서

## 📋 씬 개요

**씬 이름**: TitleScene
**씬 목적**: 게임 시작, 로딩, 메인 메뉴 제공
**예상 개발 시간**: 3일
**우선순위**: 높음 (최초 진입점)

---

## 🎨 UI 구성

### 레이아웃 구조
```
┌─────────────────────────┐
│     [게임 로고]          │  ← 상단 30%
│  숨겨진 성장의 백수 영웅  │
│                         │
│   [캐릭터 일러스트]      │  ← 중앙 40%
│   (민수 - 백수 모드)     │
│                         │
│   [TOUCH TO START]      │  ← 하단 20%
│   (깜빡이는 텍스트)      │
│                         │
│  [옵션] [크레딧] [종료]  │  ← 최하단 10%
└─────────────────────────┘
```

---

## 🔧 기술 명세

### 필요한 컴포넌트

#### 1. TitleSceneManager (C# Script)
```csharp
public class TitleSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject titlePanel;
    public GameObject mainMenuPanel;
    public Text versionText;

    [Header("Animation")]
    public Animator characterAnimator;
    public Image fadePanel;

    [Header("Audio")]
    public AudioSource bgmSource;
    public AudioClip titleBGM;

    private void Start()
    {
        // 초기화
        InitializeTitle();
        CheckSaveData();
        PlayBGM();
    }

    private void InitializeTitle()
    {
        // 버전 정보 표시
        versionText.text = $"v{Application.version}";

        // 페이드 인 애니메이션
        FadeIn();
    }

    private void CheckSaveData()
    {
        // 세이브 데이터 확인
        if (SaveManager.Instance.HasSaveData())
        {
            // 이어하기 버튼 활성화
        }
        else
        {
            // 새 게임만 가능
        }
    }

    public void OnTouchToStart()
    {
        // 메인 메뉴로 전환
        ShowMainMenu();
    }
}
```

#### 2. MainMenuUI (C# Script)
```csharp
public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    public Button continueButton;
    public Button newGameButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button exitButton;

    [Header("Panels")]
    public GameObject confirmNewGamePanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    private void Start()
    {
        SetupButtons();
        CheckContinueAvailable();
    }

    private void SetupButtons()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        creditsButton.onClick.AddListener(OnCreditsClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnContinueClicked()
    {
        // 세이브 데이터 로드
        SaveManager.Instance.LoadGame();
        // 메인 게임 씬으로 전환
        SceneTransition.Instance.LoadScene("MainGameScene");
    }

    private void OnNewGameClicked()
    {
        // 세이브 데이터 있으면 확인 팝업
        if (SaveManager.Instance.HasSaveData())
        {
            confirmNewGamePanel.SetActive(true);
        }
        else
        {
            StartNewGame();
        }
    }

    private void StartNewGame()
    {
        // 새 게임 시작
        GameManager.Instance.StartNewGame();
        // 인트로 스토리 또는 튜토리얼로
        SceneTransition.Instance.LoadScene("IntroStoryScene");
    }
}
```

---

## 🎬 애니메이션 & 효과

### 페이드 인/아웃
```csharp
public class FadeEffect : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    public IEnumerator FadeIn()
    {
        float timer = 0;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = 1 - (timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0;
        Color color = fadeImage.color;
        color.a = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = timer / fadeDuration;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1;
        fadeImage.color = color;
    }
}
```

### TOUCH TO START 깜빡임
```csharp
public class BlinkingText : MonoBehaviour
{
    public Text textComponent;
    public float blinkInterval = 0.5f;

    private void Start()
    {
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            textComponent.enabled = !textComponent.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
```

### 로고 등장 애니메이션
```
- Scale Up: 0.8 → 1.0 (0.5초, EaseOut)
- Fade In: Alpha 0 → 1 (0.3초)
- 약간의 Bounce 효과
```

---

## 🎵 오디오

### BGM
- **파일명**: `Title_BGM.mp3`
- **분위기**: 편안하고 약간 코믹한 느낌
- **루프**: Yes
- **볼륨**: 0.7

### SFX
- **버튼 클릭**: `UI_Click.wav`
- **메뉴 열기**: `UI_Open.wav`
- **씬 전환**: `Scene_Transition.wav`

---

## 💾 데이터 처리

### 세이브 데이터 체크
```csharp
public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "GameSaveData";

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public SaveData LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class SaveData
{
    public int playerLevel;
    public long gold;
    public int prestigePoints;
    public string lastPlayTime;
    // ... 기타 데이터
}
```

---

## 📱 터치/입력 처리

### 터치 감지
```csharp
public class TitleTouchHandler : MonoBehaviour
{
    public TitleSceneManager sceneManager;

    private void Update()
    {
        // 모바일 터치
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            sceneManager.OnTouchToStart();
        }

        // PC 마우스 클릭 (테스트용)
        if (Input.GetMouseButtonDown(0))
        {
            sceneManager.OnTouchToStart();
        }

        // 스페이스바 (테스트용)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            sceneManager.OnTouchToStart();
        }
    }
}
```

---

## 🔗 씬 전환

### SceneTransition Manager
```csharp
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Transition Settings")]
    public Image fadeImage;
    public float transitionDuration = 1.0f;

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

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 페이드 아웃
        yield return StartCoroutine(FadeOut());

        // 씬 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            // 로딩 프로그레스 표시 가능
            yield return null;
        }

        // 페이드 인
        yield return StartCoroutine(FadeIn());
    }
}
```

---

## 🎯 최적화 고려사항

### 초기 로딩
```csharp
public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        // 프레임레이트 설정
        Application.targetFrameRate = 60;

        // 화면 꺼짐 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 매니저 초기화
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        // GameManager
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // SaveManager
        // AudioManager
        // ... 기타 매니저들
    }
}
```

### 리소스 관리
```csharp
public class ResourceLoader : MonoBehaviour
{
    // Resources 폴더에서 필요한 것만 로드
    public static Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }

    public static AudioClip LoadAudio(string path)
    {
        return Resources.Load<AudioClip>(path);
    }
}
```

---

## 🐛 디버그 & 테스트

### 치트 메뉴 (개발용)
```csharp
public class DebugMenu : MonoBehaviour
{
    private bool showDebugMenu = false;

    private void Update()
    {
        // PC에서만 작동 (빌드에서 제거)
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.BackQuote)) // ` 키
        {
            showDebugMenu = !showDebugMenu;
        }
        #endif
    }

    private void OnGUI()
    {
        if (showDebugMenu)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));
            GUILayout.Label("=== DEBUG MENU ===");

            if (GUILayout.Button("Delete Save Data"))
            {
                SaveManager.Instance.DeleteSaveData();
                Debug.Log("Save data deleted");
            }

            if (GUILayout.Button("Skip to Main Game"))
            {
                SceneTransition.Instance.LoadScene("MainGameScene");
            }

            GUILayout.EndArea();
        }
    }
}
```

---

## 📋 체크리스트

### 개발 단계
- [ ] UI 레이아웃 구성
- [ ] 로고 및 타이틀 텍스트 배치
- [ ] 캐릭터 일러스트 배치
- [ ] 메뉴 버튼 구현
- [ ] 페이드 인/아웃 구현
- [ ] Touch to Start 깜빡임 구현

### 기능 구현
- [ ] 세이브 데이터 체크
- [ ] 이어하기 기능
- [ ] 새 게임 시작
- [ ] 설정 메뉴 연결
- [ ] 크레딧 화면
- [ ] 게임 종료 기능

### 오디오
- [ ] BGM 재생
- [ ] 버튼 클릭 SFX
- [ ] 씬 전환 사운드

### 최적화
- [ ] 초기 로딩 시간 측정
- [ ] 메모리 사용량 체크
- [ ] 프레임레이트 안정화

### 테스트
- [ ] 신규 유저 플로우 테스트
- [ ] 기존 유저 이어하기 테스트
- [ ] 버튼 반응성 테스트
- [ ] 다양한 해상도 테스트
- [ ] 터치 입력 테스트

---

## 🎨 필요한 에셋

### 이미지
```
/Assets/UI/Title/
├── logo.png (2048x1024)
├── title_bg.png (1920x1080)
├── character_idle.png (1024x2048)
└── touch_to_start_icon.png (128x128)

/Assets/UI/MainMenu/
├── button_continue.png (400x100)
├── button_new_game.png (400x100)
├── button_settings.png (400x100)
├── button_credits.png (400x100)
└── button_exit.png (400x100)
```

### 폰트
```
/Assets/Fonts/
├── TitleFont.ttf (로고용, 굵은 체)
└── UIFont.ttf (버튼용, 가독성 좋은 체)
```

### 오디오
```
/Assets/Audio/BGM/
└── Title_BGM.mp3 (128kbps, Stereo)

/Assets/Audio/SFX/UI/
├── UI_Click.wav
├── UI_Open.wav
└── Scene_Transition.wav
```

---

## 📐 해상도 대응

### Safe Area 처리
```csharp
public class SafeAreaHandler : MonoBehaviour
{
    private void Start()
    {
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Rect safeArea = Screen.safeArea;

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

### 다양한 비율 지원
```
16:9 (일반)
18:9 (갤럭시)
19.5:9 (아이폰 X 이상)
```

---

## 🚀 빌드 설정

### Android
```
Minimum API Level: 21 (Android 5.0)
Target API Level: 33 (Android 13)
Graphics API: OpenGLES3, Vulkan
```

### iOS
```
Minimum iOS Version: 12.0
Target Device: iPhone, iPad
Architecture: ARM64
```

---

## 📝 개발 노트

### 주의사항
1. **초기 로딩 최적화**: 타이틀 씬은 가장 먼저 로드되므로 가볍게 유지
2. **BGM 프리로드**: BGM은 씬 로드 전에 미리 로드하여 끊김 없도록
3. **터치 딜레이**: 터치 후 즉시 반응하되, 중복 터치 방지
4. **메모리 관리**: 사용하지 않는 리소스는 즉시 해제

### 개선 아이디어
- [ ] 캐릭터 Idle 애니메이션 추가
- [ ] 배경 파티클 효과 (별똥별, 눈 등)
- [ ] 타이틀 로고 빛나는 효과
- [ ] 버튼 호버 효과
- [ ] 로딩 팁 표시

---

**개발 예상 시간**: 3일
**테스트 시간**: 1일
**총 소요 시간**: 4일

**담당**: UI 프로그래머, 사운드 디자이너
**의존성**: GameManager, SaveManager, SceneTransition
