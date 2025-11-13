# Scene 04: 전투 - 개발 명세서

## 📋 씬 개요
**씬 이름**: BattleScene
**씬 목적**: 실시간 몬스터 전투, 클릭커 메커니즘
**개발 시간**: 7일

## 🎨 UI 구성
```
┌─────────────────────────────────┐
│ Wave: 1/10    [⏸일시정지]      │
├─────────────────────────────────┤
│     [몬스터 HP: ████████░░]     │
│       슬라임 Lv.5               │
│                                 │
│      [몬스터 스프라이트]         │
│         [TAP 영역]              │
│                                 │
├─────────────────────────────────┤
│  HP: ████████░░  MP: ██████░░  │
├─────────────────────────────────┤
│ [스킬1] [스킬2] [스킬3] [스킬4] │
├─────────────────────────────────┤
│ 골드: +1,234   경험치: 45/100   │
└─────────────────────────────────┘
```

## 🔧 핵심 시스템

### BattleManager
```csharp
public class BattleManager : MonoBehaviour
{
    [Header("Battle Settings")]
    public int totalWaves = 10;
    public int currentWave = 1;
    public Monster currentMonster;

    [Header("Wave Data")]
    public List<MonsterWaveData> waves;

    [Header("UI")]
    public Text waveText;
    public Slider monsterHPBar;
    public Text monsterNameText;

    private void Start()
    {
        StartBattle();
    }

    public void StartBattle()
    {
        currentWave = 1;
        SpawnMonster(currentWave);
    }

    public void SpawnMonster(int wave)
    {
        // 웨이브 데이터에서 몬스터 생성
        MonsterWaveData waveData = waves[wave - 1];
        currentMonster = MonsterFactory.Create(waveData);

        // UI 업데이트
        UpdateMonsterUI();
    }

    public void OnMonsterClicked()
    {
        if (currentMonster == null || currentMonster.IsDead) return;

        // 데미지 계산
        long damage = CalculateDamage();
        currentMonster.TakeDamage(damage);

        // 이펙트
        ShowDamageEffect(damage);

        // 몬스터 죽음 체크
        if (currentMonster.IsDead)
        {
            OnMonsterKilled();
        }
    }

    private long CalculateDamage()
    {
        long baseDamage = (long)GameManager.Instance.attackPower;

        // 크리티컬
        if (Random.value < GameManager.Instance.criticalChance)
        {
            baseDamage = (long)(baseDamage * GameManager.Instance.criticalMultiplier);
        }

        return baseDamage;
    }

    private void OnMonsterKilled()
    {
        // 보상
        long goldReward = currentMonster.goldDrop;
        long expReward = currentMonster.expDrop;

        GameManager.Instance.AddGold(goldReward);
        GameManager.Instance.AddEXP(expReward);

        // 다음 웨이브
        currentWave++;
        if (currentWave <= totalWaves)
        {
            Invoke("SpawnMonster", 1.0f);
        }
        else
        {
            OnBattleComplete();
        }
    }

    private void OnBattleComplete()
    {
        // 승리 화면
        ShowVictoryScreen();
    }
}
```

### Monster Class
```csharp
[System.Serializable]
public class Monster
{
    public string monsterName;
    public int level;
    public long maxHP;
    public long currentHP;
    public long goldDrop;
    public long expDrop;
    public Sprite sprite;

    public bool IsDead => currentHP <= 0;

    public void TakeDamage(long damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
    }
}
```

### Auto Battle
```csharp
public class AutoBattleController : MonoBehaviour
{
    public bool isAutoBattleActive = false;
    public float autoClickInterval = 0.5f;

    private float timer = 0f;

    private void Update()
    {
        if (!isAutoBattleActive) return;

        timer += Time.deltaTime;
        if (timer >= autoClickInterval)
        {
            BattleManager.Instance.OnMonsterClicked();
            timer = 0f;
        }
    }

    public void ToggleAutoBattle()
    {
        isAutoBattleActive = !isAutoBattleActive;
        // UI 업데이트
    }
}
```

## 📋 체크리스트
- [ ] 몬스터 생성 시스템
- [ ] 클릭 공격
- [ ] 데미지 계산
- [ ] HP 바 업데이트
- [ ] 웨이브 시스템
- [ ] 보상 지급
- [ ] 승리/패배 화면
- [ ] 자동 전투

**개발 시간**: 7일
