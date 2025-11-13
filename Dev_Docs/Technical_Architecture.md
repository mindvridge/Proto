# 기술 아키텍처 문서

**프로젝트**: 숨겨진 성장의 백수 영웅
**작성일**: 2025-11-13
**버전**: 1.0
**Unity 버전**: 2022.3 LTS 이상
**플랫폼**: Android, iOS

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [기술 스택](#기술-스택)
3. [프로젝트 폴더 구조](#프로젝트-폴더-구조)
4. [아키텍처 패턴](#아키텍처-패턴)
5. [매니저 시스템](#매니저-시스템)
6. [씬 구조](#씬-구조)
7. [데이터 흐름](#데이터-흐름)
8. [코딩 컨벤션](#코딩-컨벤션)
9. [성능 최적화](#성능-최적화)
10. [빌드 설정](#빌드-설정)

---

## 📱 프로젝트 개요

### 게임 정보
- **장르**: 모바일 클리커 RPG
- **타겟 플랫폼**: Android 7.0+, iOS 13.0+
- **화면 방향**: 세로 (Portrait)
- **해상도**: 1080×1920 (9:16 권장)
- **타겟 FPS**: 60 FPS
- **파일 크기 목표**: 200MB 이하

### 기술 요구사항
- Unity 2022.3 LTS 이상
- C# 9.0+
- .NET Standard 2.1
- TextMeshPro
- Unity IAP
- Google AdMob / Unity Ads

---

## 🛠️ 기술 스택

### Unity 패키지

| 패키지 | 버전 | 용도 |
|--------|------|------|
| TextMeshPro | 3.0+ | UI 텍스트 렌더링 |
| Unity IAP | 4.9+ | 인앱 결제 |
| Unity Ads | 4.4+ | 광고 SDK |
| DOTween | 1.2+ | 트윈 애니메이션 |
| Newtonsoft.Json | 3.2+ | JSON 파싱 |
| Firebase Analytics | 11.0+ | 데이터 분석 (선택) |
| Firebase Crashlytics | 11.0+ | 크래시 리포트 (선택) |

### 외부 SDK

| SDK | 용도 |
|-----|------|
| Google Play Services | Android 빌드 |
| AdMob | 광고 수익화 |
| Adjust / AppsFlyer | 어트리뷰션 (선택) |

---

## 📁 프로젝트 폴더 구조

```
Assets/
├── _Project/                          # 프로젝트 루트
│   ├── Scenes/                        # 씬 파일
│   │   ├── 00_Initialization.unity    # 초기화 씬
│   │   ├── 01_Title.unity            # 타이틀
│   │   ├── 02_MainGame.unity         # 메인 게임
│   │   ├── 03_DungeonSelection.unity # 던전 선택
│   │   ├── 04_Battle.unity           # 전투
│   │   ├── 05_Inventory.unity        # 인벤토리
│   │   ├── 06_Shop.unity             # 상점
│   │   ├── 07_Gacha.unity            # 가챠
│   │   ├── 08_SkillTree.unity        # 스킬 트리
│   │   ├── 09_Story.unity            # 스토리
│   │   └── 10_Rebirth.unity          # 환생
│   │
│   ├── Scripts/                       # C# 스크립트
│   │   ├── Core/                      # 핵심 시스템
│   │   │   ├── GameManager.cs
│   │   │   ├── SaveManager.cs
│   │   │   ├── AudioManager.cs
│   │   │   ├── UIManager.cs
│   │   │   └── SceneTransition.cs
│   │   │
│   │   ├── Managers/                  # 기능별 매니저
│   │   │   ├── ClickerManager.cs
│   │   │   ├── TimeManager.cs
│   │   │   ├── SuspicionManager.cs
│   │   │   ├── InventoryManager.cs
│   │   │   ├── EquipmentManager.cs
│   │   │   ├── SkillTreeManager.cs
│   │   │   ├── DungeonManager.cs
│   │   │   ├── BattleManager.cs
│   │   │   ├── ShopManager.cs
│   │   │   ├── GachaManager.cs
│   │   │   ├── RebirthManager.cs
│   │   │   ├── DialogueManager.cs
│   │   │   ├── RelationshipManager.cs
│   │   │   └── AchievementManager.cs
│   │   │
│   │   ├── Data/                      # 데이터 클래스
│   │   │   ├── ItemData.cs
│   │   │   ├── SkillData.cs
│   │   │   ├── MonsterData.cs
│   │   │   ├── DungeonData.cs
│   │   │   ├── DialogueData.cs
│   │   │   └── GameDatabase.cs
│   │   │
│   │   ├── UI/                        # UI 컴포넌트
│   │   │   ├── Panels/
│   │   │   │   ├── MainGameUI.cs
│   │   │   │   ├── InventoryUI.cs
│   │   │   │   ├── ShopUI.cs
│   │   │   │   └── ...
│   │   │   ├── Elements/
│   │   │   │   ├── ItemSlot.cs
│   │   │   │   ├── SkillButton.cs
│   │   │   │   └── DamageNumber.cs
│   │   │   └── Popups/
│   │   │       ├── ConfirmPopup.cs
│   │   │       ├── RewardPopup.cs
│   │   │       └── NotificationPopup.cs
│   │   │
│   │   ├── Gameplay/                  # 게임플레이 로직
│   │   │   ├── Monster.cs
│   │   │   ├── Item.cs
│   │   │   ├── Skill.cs
│   │   │   └── Dungeon.cs
│   │   │
│   │   ├── Utils/                     # 유틸리티
│   │   │   ├── ObjectPool.cs
│   │   │   ├── Extensions.cs
│   │   │   ├── NumberFormatter.cs
│   │   │   └── DebugConsole.cs
│   │   │
│   │   └── Editor/                    # 에디터 스크립트
│   │       ├── DataCreator.cs
│   │       └── DebugTools.cs
│   │
│   ├── Prefabs/                       # 프리팹
│   │   ├── UI/
│   │   │   ├── Panels/
│   │   │   ├── Popups/
│   │   │   └── Elements/
│   │   ├── Characters/
│   │   ├── Monsters/
│   │   ├── Effects/
│   │   └── Items/
│   │
│   ├── Art/                           # 아트 에셋
│   │   ├── Sprites/
│   │   │   ├── UI/
│   │   │   ├── Characters/
│   │   │   ├── Monsters/
│   │   │   ├── Items/
│   │   │   └── Backgrounds/
│   │   ├── Animations/
│   │   └── VFX/
│   │
│   ├── Audio/                         # 오디오
│   │   ├── BGM/
│   │   ├── SFX/
│   │   └── Voice/
│   │
│   └── Resources/                     # 런타임 로드
│       └── GameData/
│           ├── Items/
│           ├── Skills/
│           ├── Monsters/
│           └── Dungeons/
│
├── StreamingAssets/                   # 빌드 후 수정 가능
│   └── Data/
│       ├── dialogues.json
│       ├── achievements.json
│       └── localization/
│
├── Plugins/                           # 네이티브 플러그인
│   ├── Android/
│   └── iOS/
│
└── TextMesh Pro/                      # TMP 에셋

```

---

## 🏗️ 아키텍처 패턴

### 1. Singleton Pattern (싱글톤)

모든 매니저는 싱글톤으로 구현하여 전역 접근을 허용합니다.

```csharp
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        // 초기화 로직
    }
}
```

### 2. Observer Pattern (이벤트 시스템)

이벤트 기반 통신으로 결합도를 낮춥니다.

```csharp
// EventManager.cs
public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // 이벤트 정의
    public delegate void GoldChangedHandler(long newGold);
    public static event GoldChangedHandler OnGoldChanged;

    public delegate void LevelUpHandler(int newLevel);
    public static event LevelUpHandler OnLevelUp;

    public delegate void ItemAcquiredHandler(string itemId);
    public static event ItemAcquiredHandler OnItemAcquired;

    // 이벤트 발생
    public static void TriggerGoldChanged(long newGold)
    {
        OnGoldChanged?.Invoke(newGold);
    }

    public static void TriggerLevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
    }
}

// 사용 예시
public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.OnGoldChanged += UpdateGoldDisplay;
        EventManager.OnLevelUp += UpdateLevelDisplay;
    }

    private void OnDisable()
    {
        EventManager.OnGoldChanged -= UpdateGoldDisplay;
        EventManager.OnLevelUp -= UpdateLevelDisplay;
    }

    private void UpdateGoldDisplay(long newGold)
    {
        goldText.text = NumberFormatter.Format(newGold);
    }
}
```

### 3. Object Pool Pattern (오브젝트 풀링)

데미지 넘버, 이펙트 등 자주 생성/삭제되는 오브젝트에 사용합니다.

```csharp
public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
            return null;

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}
```

### 4. ScriptableObject Pattern (데이터 관리)

게임 데이터는 ScriptableObject로 관리합니다.

```csharp
// 데이터 정의
[CreateAssetMenu(fileName = "NewItem", menuName = "GameData/Item")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string itemName;
    public Sprite icon;
    // ... 기타 데이터
}

// 데이터베이스
public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance;

    private Dictionary<string, ItemData> items;

    private void Awake()
    {
        LoadAllData();
    }

    private void LoadAllData()
    {
        ItemData[] itemArray = Resources.LoadAll<ItemData>("GameData/Items");
        items = new Dictionary<string, ItemData>();
        foreach (var item in itemArray)
        {
            items[item.itemId] = item;
        }
    }

    public ItemData GetItem(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : null;
    }
}
```

### 5. MVC Pattern (UI)

UI는 MVC 패턴으로 구조화합니다.

```csharp
// Model
public class PlayerData
{
    public int level;
    public long gold;
    public long exp;
}

// View
public class PlayerStatsView : MonoBehaviour
{
    public Text levelText;
    public Text goldText;
    public Slider expSlider;

    public void UpdateView(PlayerData data)
    {
        levelText.text = $"Lv.{data.level}";
        goldText.text = NumberFormatter.Format(data.gold);
        expSlider.value = data.exp / (float)GameManager.Instance.GetRequiredExp(data.level);
    }
}

// Controller
public class PlayerStatsController : MonoBehaviour
{
    private PlayerData model;
    private PlayerStatsView view;

    private void Start()
    {
        model = GameManager.Instance.playerData;
        view = GetComponent<PlayerStatsView>();
        UpdateUI();
    }

    private void OnEnable()
    {
        EventManager.OnLevelUp += OnLevelUp;
        EventManager.OnGoldChanged += OnGoldChanged;
    }

    private void OnLevelUp(int newLevel)
    {
        model.level = newLevel;
        UpdateUI();
    }

    private void OnGoldChanged(long newGold)
    {
        model.gold = newGold;
        UpdateUI();
    }

    private void UpdateUI()
    {
        view.UpdateView(model);
    }
}
```

---

## 🎮 매니저 시스템

### 매니저 초기화 순서

```csharp
// InitializationScene (씬 00)
public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        InitializeManagers();
        LoadMainGame();
    }

    private void InitializeManagers()
    {
        // 1단계: 핵심 매니저 (순서 중요)
        GameManager.Instance.Initialize();        // 게임 상태
        SaveManager.Instance.Initialize();        // 세이브 로드
        GameDatabase.Instance.Initialize();       // 데이터 로드
        AudioManager.Instance.Initialize();       // 오디오 시스템

        // 2단계: 기능 매니저
        EventManager.Instance.Initialize();
        UIManager.Instance.Initialize();

        // 3단계: 게임플레이 매니저
        ClickerManager.Instance.Initialize();
        TimeManager.Instance.Initialize();
        SuspicionManager.Instance.Initialize();
        InventoryManager.Instance.Initialize();
        EquipmentManager.Instance.Initialize();
        SkillTreeManager.Instance.Initialize();
        DungeonManager.Instance.Initialize();
        GachaManager.Instance.Initialize();
        RebirthManager.Instance.Initialize();
        AchievementManager.Instance.Initialize();

        // 4단계: 외부 SDK
        InitializeIAP();
        InitializeAds();
        InitializeAnalytics();
    }
}
```

### 매니저 의존성 다이어그램

```
┌─────────────────┐
│  GameManager    │  ← 최상위 매니저
└────────┬────────┘
         │
    ┌────┴────┬─────────┬─────────┐
    ▼         ▼         ▼         ▼
┌─────────┐┌──────────┐┌────────┐┌──────────┐
│SaveMgr  ││EventMgr  ││AudioMgr││UIMgr     │
└────┬────┘└─────┬────┘└───┬────┘└────┬─────┘
     │           │          │          │
     └───────────┴──────────┴──────────┘
                 │
         ┌───────┴────────┐
         ▼                ▼
    ┌──────────┐    ┌──────────┐
    │GameDB    │    │TimeMgr   │
    └─────┬────┘    └────┬─────┘
          │              │
    ┌─────┴─────┬────────┴────┬──────────┐
    ▼           ▼             ▼          ▼
┌────────┐┌─────────┐┌──────────┐┌──────────┐
│ItemMgr ││SkillMgr ││MonsterMgr││DungeonMgr│
└────────┘└─────────┘└──────────┘└──────────┘
          │              │
    ┌─────┴─────┬────────┴────┬──────────┐
    ▼           ▼             ▼          ▼
┌────────┐┌─────────┐┌──────────┐┌──────────┐
│ClickMgr││InvenMgr ││EquipMgr  ││BattleMgr │
└────────┘└─────────┘└──────────┘└──────────┘
```

### 핵심 매니저 리스트

| 매니저 | 역할 | DontDestroyOnLoad |
|--------|------|-------------------|
| GameManager | 게임 전체 상태 관리 | ✓ |
| SaveManager | 세이브/로드 | ✓ |
| GameDatabase | 데이터 저장소 | ✓ |
| EventManager | 이벤트 시스템 | ✓ |
| AudioManager | 음악/효과음 | ✓ |
| UIManager | UI 공통 기능 | ✓ |
| TimeManager | 시간 시스템 | ✓ |
| ClickerManager | 클릭 로직 | ✓ |
| SuspicionManager | 의심도 관리 | ✓ |
| RebirthManager | 환생 시스템 | ✓ |
| InventoryManager | 인벤토리 | ✓ |
| EquipmentManager | 장비 시스템 | ✓ |
| SkillTreeManager | 스킬 트리 | ✓ |
| DungeonManager | 던전 관리 | ✓ |
| BattleManager | 전투 로직 | ✗ (씬 전용) |
| ShopManager | 상점 | ✗ (씬 전용) |
| GachaManager | 가챠 | ✗ (씬 전용) |
| DialogueManager | 대화 시스템 | ✗ (씬 전용) |

---

## 🎬 씬 구조

### 씬 로딩 흐름

```
[00_Initialization]
    ↓ (매니저 초기화)
[01_Title]
    ↓ (뉴게임/컨티뉴)
[02_MainGame] ← 중심 허브
    ├→ [03_DungeonSelection] → [04_Battle]
    ├→ [05_Inventory]
    ├→ [06_Shop]
    ├→ [07_Gacha]
    ├→ [08_SkillTree]
    ├→ [09_Story]
    └→ [10_Rebirth] → [02_MainGame] (리셋)
```

### 씬 전환 시스템

```csharp
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Transition Settings")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Fade out
        yield return StartCoroutine(Fade(1));

        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            // 로딩 바 업데이트 가능
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        // Fade in
        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }
}
```

---

## 🔄 데이터 흐름

### 세이브 데이터 구조

```csharp
[System.Serializable]
public class SaveData
{
    // 플레이어 기본 정보
    public int playerLevel = 1;
    public long currentGold = 0;
    public long currentExp = 0;
    public int currentGems = 0;

    // 환생 정보
    public int rebirthCount = 0;
    public long totalRebirthPoints = 0;
    public long currentRebirthPoints = 0;

    // 인벤토리
    public List<ItemSaveData> inventory = new List<ItemSaveData>();
    public Dictionary<string, string> equipment = new Dictionary<string, string>();

    // 스킬
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    public int availableSkillPoints = 0;

    // 환생 특전
    public Dictionary<string, int> perkLevels = new Dictionary<string, int>();

    // 던전 진행도
    public List<string> clearedDungeons = new List<string>();
    public int currentEnergy = 100;
    public string lastEnergyRegenTime;

    // 의심도
    public float currentSuspicion = 0;

    // 관계
    public Dictionary<string, int> relationships = new Dictionary<string, int>();

    // 대화
    public List<string> completedDialogues = new List<string>();

    // 업적
    public List<string> completedAchievements = new List<string>();

    // 통계
    public long totalGoldEarned = 0;
    public long totalClicks = 0;
    public int totalMonstersKilled = 0;
    public string firstPlayDate;
    public string lastPlayDate;
    public float totalPlayTime = 0;

    // 설정
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool isPushEnabled = true;
    public string language = "ko";
}

[System.Serializable]
public class ItemSaveData
{
    public string itemId;
    public int count;
}
```

### 세이브/로드 시스템

```csharp
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SAVE_KEY = "GameSaveData";
    private SaveData currentSaveData;

    public void SaveGame()
    {
        // 현재 게임 상태를 SaveData에 저장
        currentSaveData = new SaveData();

        // 플레이어 데이터
        currentSaveData.playerLevel = GameManager.Instance.playerLevel;
        currentSaveData.currentGold = GameManager.Instance.currentGold;
        currentSaveData.currentExp = GameManager.Instance.currentExp;

        // 환생 데이터
        currentSaveData.rebirthCount = RebirthManager.Instance.rebirthCount;
        currentSaveData.totalRebirthPoints = RebirthManager.Instance.totalRebirthPoints;

        // ... 기타 데이터 수집

        // JSON으로 변환
        string json = JsonUtility.ToJson(currentSaveData, true);

        // 암호화 (선택사항)
        string encrypted = EncryptData(json);

        // PlayerPrefs에 저장
        PlayerPrefs.SetString(SAVE_KEY, encrypted);
        PlayerPrefs.Save();

        Debug.Log("Game saved successfully!");
    }

    public bool LoadGame()
    {
        if (!HasSaveData())
            return false;

        string encrypted = PlayerPrefs.GetString(SAVE_KEY);
        string json = DecryptData(encrypted);

        currentSaveData = JsonUtility.FromJson<SaveData>(json);

        // 데이터를 게임에 적용
        GameManager.Instance.playerLevel = currentSaveData.playerLevel;
        GameManager.Instance.currentGold = currentSaveData.currentGold;
        GameManager.Instance.currentExp = currentSaveData.currentExp;

        // ... 기타 데이터 적용

        Debug.Log("Game loaded successfully!");
        return true;
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public void DeleteSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }

    // 간단한 XOR 암호화
    private string EncryptData(string data)
    {
        // 실제로는 더 강력한 암호화 사용 권장
        char[] chars = data.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = (char)(chars[i] ^ 123); // XOR 키
        }
        return new string(chars);
    }

    private string DecryptData(string data)
    {
        return EncryptData(data); // XOR는 대칭
    }

    // 자동 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
```

---

## 📐 코딩 컨벤션

### 네이밍 규칙

```csharp
// 네임스페이스: PascalCase
namespace HiddenGrowth.Core
{
    // 클래스/구조체/인터페이스: PascalCase
    public class PlayerController { }
    public struct PlayerData { }
    public interface IInteractable { }

    // 메서드: PascalCase
    public void AttackEnemy() { }
    private void CalculateDamage() { }

    // 프로퍼티: PascalCase
    public int PlayerLevel { get; set; }
    public string PlayerName { get; private set; }

    // 필드:
    // - public: PascalCase
    public int MaxHealth = 100;

    // - private: camelCase
    private int currentHealth;
    private float attackSpeed;

    // - const: UPPER_SNAKE_CASE
    private const int MAX_INVENTORY_SIZE = 100;
    private const float CRITICAL_MULTIPLIER = 2.0f;

    // 매개변수/지역변수: camelCase
    public void DealDamage(int damageAmount, bool isCritical)
    {
        float finalDamage = damageAmount;
        // ...
    }

    // 이벤트: On + PascalCase
    public event Action OnPlayerDied;
    public delegate void HealthChangedHandler(int newHealth);
}
```

### 주석 규칙

```csharp
/// <summary>
/// 플레이어에게 데미지를 입힙니다.
/// </summary>
/// <param name="damage">데미지 양</param>
/// <param name="isCritical">치명타 여부</param>
/// <returns>실제로 입힌 데미지</returns>
public int TakeDamage(int damage, bool isCritical)
{
    // 치명타 배율 적용
    if (isCritical)
    {
        damage = (int)(damage * CRITICAL_MULTIPLIER);
    }

    // 방어력 계산
    int finalDamage = Mathf.Max(1, damage - defense);

    // 체력 감소
    currentHealth -= finalDamage;

    // TODO: 데미지 이펙트 추가
    // FIXME: 음수 체력 버그 수정 필요

    return finalDamage;
}
```

### 폴더/파일 명명 규칙

```
- Scripts: PascalCase (예: GameManager.cs, ItemData.cs)
- Prefabs: PascalCase (예: PlayerCharacter.prefab, DamageNumber.prefab)
- Scenes: 번호_PascalCase (예: 01_Title.unity, 02_MainGame.unity)
- Sprites: snake_case (예: item_sword_001.png, ui_button_normal.png)
- Audio: snake_case (예: bgm_main_theme.mp3, sfx_click.wav)
```

---

## ⚡ 성능 최적화

### 1. 오브젝트 풀링

자주 생성/삭제되는 오브젝트는 풀링 사용:
- 데미지 넘버
- 이펙트
- UI 요소 (아이템 슬롯 등)
- 몬스터 (전투 씬)

### 2. UI 최적화

```csharp
// ✅ 좋은 예: 변경 시에만 업데이트
private long lastGold = -1;

private void Update()
{
    long currentGold = GameManager.Instance.currentGold;
    if (currentGold != lastGold)
    {
        goldText.text = NumberFormatter.Format(currentGold);
        lastGold = currentGold;
    }
}

// ❌ 나쁜 예: 매 프레임 업데이트
private void Update()
{
    goldText.text = GameManager.Instance.currentGold.ToString();
}
```

### 3. 이벤트 시스템 사용

```csharp
// ✅ 좋은 예: 이벤트 기반
private void OnEnable()
{
    EventManager.OnGoldChanged += UpdateGoldDisplay;
}

private void UpdateGoldDisplay(long newGold)
{
    goldText.text = NumberFormatter.Format(newGold);
}

// ❌ 나쁜 예: 폴링
private void Update()
{
    if (GameManager.Instance.currentGold != lastGold)
    {
        UpdateGoldDisplay();
    }
}
```

### 4. 스프라이트 아틀라스

UI 스프라이트는 아틀라스로 묶어 드로우콜 감소:
```
Assets/Art/Sprites/UI/
├── UI_Atlas_Main.spriteatlas
├── UI_Atlas_Items.spriteatlas
└── UI_Atlas_Characters.spriteatlas
```

### 5. 텍스처 압축

```
Android: ETC2 (RGB/RGBA)
iOS: ASTC (6x6, 8x8)
```

### 6. 코루틴 최적화

```csharp
// ✅ 좋은 예: WaitForSeconds 캐싱
private WaitForSeconds wait1Second = new WaitForSeconds(1f);

private IEnumerator UpdateEverySecond()
{
    while (true)
    {
        yield return wait1Second;
        DoUpdate();
    }
}

// ❌ 나쁜 예: 매번 생성
private IEnumerator UpdateEverySecond()
{
    while (true)
    {
        yield return new WaitForSeconds(1f); // GC 발생
        DoUpdate();
    }
}
```

### 7. 문자열 최적화

```csharp
// ✅ 좋은 예: StringBuilder 사용
public string FormatStats(PlayerData data)
{
    StringBuilder sb = new StringBuilder();
    sb.Append("Level: ").Append(data.level).AppendLine();
    sb.Append("Gold: ").Append(data.gold).AppendLine();
    return sb.ToString();
}

// ❌ 나쁜 예: 문자열 연결
public string FormatStats(PlayerData data)
{
    string result = "";
    result += "Level: " + data.level + "\n"; // 매번 새 문자열 생성
    result += "Gold: " + data.gold + "\n";
    return result;
}
```

---

## 📦 빌드 설정

### Android 빌드 설정

```
Player Settings:
├── Company Name: [회사명]
├── Product Name: 숨겨진 성장의 백수 영웅
├── Package Name: com.[company].hiddengrowth
├── Version: 1.0.0
├── Bundle Version Code: 1
│
├── Other Settings:
│   ├── Scripting Backend: IL2CPP
│   ├── Target Architectures: ARMv7 + ARM64
│   ├── API Level: Minimum 24 (Android 7.0)
│   └── Target API Level: 33 (Android 13)
│
├── Publishing Settings:
│   ├── Minify: Release
│   ├── Use R8: ✓
│   └── Keystore: [프로젝트 키스토어]
│
└── Optimization:
    ├── Managed Stripping Level: Medium
    ├── Vertex Compression: Everything
    └── Optimize Mesh Data: ✓
```

### iOS 빌드 설정

```
Player Settings:
├── Company Name: [회사명]
├── Product Name: 숨겨진 성장의 백수 영웅
├── Bundle Identifier: com.[company].hiddengrowth
├── Version: 1.0.0
├── Build: 1
│
├── Other Settings:
│   ├── Scripting Backend: IL2CPP
│   ├── Target SDK: iOS 13.0
│   └── Architecture: ARM64
│
├── Optimization:
│   ├── Managed Stripping Level: Medium
│   ├── Script Call Optimization: Fast
│   └── Engine Code Stripping: ✓
│
└── Camera Usage Description: "게임 스크린샷 기능에 사용됩니다"
```

### 빌드 크기 최적화

```
목표: 200MB 이하

최적화 방법:
1. 텍스처 압축 (ETC2/ASTC)
2. 오디오 압축 (Vorbis, 품질 70)
3. Addressables로 에셋 분할 (선택)
4. Code Stripping 활성화
5. 사용하지 않는 에셋 제거
6. Asset Bundle 활용
```

---

## 🔧 Unity 프로젝트 설정

### Quality Settings

```
Levels:
├── Low (저사양 기기)
│   ├── VSync: Off
│   ├── Target FPS: 30
│   ├── Anti-Aliasing: Disabled
│   └── Shadows: Disabled
│
├── Medium (중간 사양)
│   ├── VSync: Off
│   ├── Target FPS: 60
│   ├── Anti-Aliasing: 2x
│   └── Shadows: Hard Only
│
└── High (고사양 기기)
    ├── VSync: On
    ├── Target FPS: 60
    ├── Anti-Aliasing: 4x
    └── Shadows: All
```

### Physics 2D Settings

```
Gravity: (0, 0)  // 2D 클리커 게임은 중력 불필요
Default Material: Frictionless
Queries Hit Triggers: ✓
```

### Time Settings

```
Fixed Timestep: 0.02 (50 FPS)
Maximum Allowed Timestep: 0.1
Time Scale: 1 (게임 속도 조절 가능)
```

---

## 📋 체크리스트

### 프로젝트 설정
- [ ] Unity 2022.3 LTS 설치
- [ ] 프로젝트 폴더 구조 생성
- [ ] 필수 패키지 임포트
- [ ] 빌드 설정 구성

### 아키텍처 구현
- [ ] 싱글톤 베이스 클래스
- [ ] 이벤트 시스템
- [ ] 오브젝트 풀
- [ ] ScriptableObject 구조

### 매니저 구현
- [ ] GameManager
- [ ] SaveManager
- [ ] GameDatabase
- [ ] EventManager
- [ ] AudioManager
- [ ] UIManager

### 최적화
- [ ] 스프라이트 아틀라스
- [ ] 오브젝트 풀링
- [ ] 텍스처 압축
- [ ] 코드 최적화

### 빌드
- [ ] Android 빌드 테스트
- [ ] iOS 빌드 테스트
- [ ] 성능 프로파일링
- [ ] 메모리 최적화

---

**작성 완료일**: 2025-11-13
**업데이트 예정**: 개발 진행에 따라 지속적으로 업데이트
**문서 관리**: Git으로 버전 관리
