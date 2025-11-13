# 게임 데이터베이스 설계서

**프로젝트**: 숨겨진 성장의 백수 영웅
**작성일**: 2025-11-13
**버전**: 1.0

---

## 📋 목차

1. [데이터 저장 방식](#데이터-저장-방식)
2. [아이템 데이터](#아이템-데이터)
3. [스킬 데이터](#스킬-데이터)
4. [몬스터 데이터](#몬스터-데이터)
5. [던전 데이터](#던전-데이터)
6. [환생 특전 데이터](#환생-특전-데이터)
7. [대화 데이터](#대화-데이터)
8. [업적 데이터](#업적-데이터)
9. [상점 데이터](#상점-데이터)
10. [데이터 로딩 시스템](#데이터-로딩-시스템)

---

## 📦 데이터 저장 방식

### Unity ScriptableObject (권장)
```csharp
// Assets/Resources/GameData/ 폴더에 저장
// 장점: Unity 에디터에서 직접 편집 가능, 타입 안정성
// 용도: 정적 데이터 (아이템, 스킬, 몬스터 등)
```

### JSON 파일 (보조)
```csharp
// Assets/StreamingAssets/Data/ 폴더에 저장
// 장점: 빌드 후 수정 가능, 외부 툴로 편집
// 용도: 다국어, 이벤트 데이터, 서버 연동 데이터
```

### PlayerPrefs (세이브 데이터)
```csharp
// 용도: 플레이어 진행도, 설정값
// 주의: 중요 데이터는 암호화 필요
```

---

## 🗡️ 아이템 데이터

### ItemData.cs (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "GameData/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;              // 고유 ID
    public string itemName;            // 아이템 이름
    public string itemNameEN;          // 영문 이름
    [TextArea(3, 5)]
    public string description;         // 설명
    public Sprite icon;                // 아이콘

    [Header("Type")]
    public ItemType itemType;          // 아이템 타입
    public ItemGrade grade;            // 등급
    public EquipmentSlot equipSlot;    // 장비 슬롯 (장비만)

    [Header("Stats")]
    public long basePrice;             // 기본 가격
    public int maxStack = 99;          // 최대 겹침 수
    public bool isTradeable = true;    // 거래 가능 여부
    public bool isDroppable = true;    // 드롭 가능 여부

    [Header("Equipment Stats")]
    public int attackPower = 0;        // 공격력
    public int defense = 0;            // 방어력
    public int health = 0;             // 체력
    public float criticalChance = 0f;  // 치명타 확률
    public float criticalDamage = 0f;  // 치명타 데미지
    public float goldBonus = 0f;       // 골드 보너스
    public float expBonus = 0f;        // 경험치 보너스
    public float clickPower = 0f;      // 클릭 파워

    [Header("Consumable Effect")]
    public ConsumableEffect[] effects; // 소비 효과

    [Header("Requirements")]
    public int requiredLevel = 1;      // 필요 레벨
    public int requiredRebirth = 0;    // 필요 환생 횟수

    [Header("Special")]
    public ItemSpecialEffect specialEffect; // 특수 효과
    public float specialEffectValue;   // 특수 효과 수치
}

public enum ItemType
{
    Weapon,      // 무기
    Armor,       // 방어구
    Accessory,   // 악세서리
    Consumable,  // 소비품
    Material,    // 재료
    QuestItem,   // 퀘스트 아이템
    Special      // 특수 아이템
}

public enum ItemGrade
{
    Common,      // 일반 (회색)
    Rare,        // 희귀 (녹색)
    Epic,        // 영웅 (파랑)
    Legendary,   // 전설 (보라)
    Mythic       // 신화 (금색)
}

public enum EquipmentSlot
{
    None,
    Weapon,      // 무기
    Helmet,      // 투구
    Armor,       // 갑옷
    Gloves,      // 장갑
    Boots,       // 신발
    Necklace,    // 목걸이
    Ring1,       // 반지1
    Ring2        // 반지2
}

public enum ItemSpecialEffect
{
    None,
    ReduceSuspicion,    // 의심도 감소
    OfflineBonus,       // 오프라인 보너스
    AutoCollect,        // 자동 수집
    DoubleGold,         // 골드 2배 (일정 시간)
    DoubleExp,          // 경험치 2배
    InstantRebirth,     // 즉시 환생
    LuckBoost           // 행운 증가
}

[System.Serializable]
public class ConsumableEffect
{
    public ConsumableEffectType type;
    public float value;
    public float duration; // 지속 시간 (초), 0이면 즉시 효과
}

public enum ConsumableEffectType
{
    RestoreHP,          // 체력 회복
    RestoreEnergy,      // 에너지 회복
    BuffClickPower,     // 클릭 파워 증가
    BuffGoldGain,       // 골드 획득량 증가
    BuffExpGain,        // 경험치 획득량 증가
    ReduceSuspicion,    // 의심도 감소
    ResetSkills,        // 스킬 초기화
    ResetPerks          // 환생 특전 초기화
}
```

### 아이템 데이터 예시 (JSON)

```json
{
  "itemId": "weapon_sword_001",
  "itemName": "녹슨 검",
  "itemNameEN": "Rusty Sword",
  "description": "오래되어 녹슨 검. 그래도 없는 것보단 낫다.",
  "itemType": "Weapon",
  "grade": "Common",
  "equipSlot": "Weapon",
  "basePrice": 100,
  "maxStack": 1,
  "attackPower": 10,
  "requiredLevel": 1,
  "specialEffect": "None"
}
```

```json
{
  "itemId": "weapon_sword_legendary_001",
  "itemName": "은둔자의 검",
  "itemNameEN": "Hermit's Blade",
  "description": "진정한 힘을 숨기는 자만이 사용할 수 있는 전설의 검.",
  "itemType": "Weapon",
  "grade": "Legendary",
  "equipSlot": "Weapon",
  "basePrice": 10000000,
  "maxStack": 1,
  "attackPower": 500,
  "criticalChance": 0.15,
  "criticalDamage": 2.0,
  "requiredLevel": 50,
  "requiredRebirth": 1,
  "specialEffect": "ReduceSuspicion",
  "specialEffectValue": 0.2
}
```

```json
{
  "itemId": "consumable_potion_gold_001",
  "itemName": "골드 부스트 물약",
  "itemNameEN": "Gold Boost Potion",
  "description": "30분간 골드 획득량이 50% 증가합니다.",
  "itemType": "Consumable",
  "grade": "Rare",
  "basePrice": 5000,
  "maxStack": 99,
  "effects": [
    {
      "type": "BuffGoldGain",
      "value": 0.5,
      "duration": 1800
    }
  ]
}
```

### 전체 아이템 리스트 (샘플 50개)

#### 무기 (10개)
| ID | 이름 | 등급 | 공격력 | 가격 | 레벨 |
|----|------|------|--------|------|------|
| weapon_sword_001 | 녹슨 검 | Common | 10 | 100 | 1 |
| weapon_sword_002 | 철 검 | Common | 25 | 500 | 5 |
| weapon_sword_003 | 강철 검 | Rare | 50 | 2,000 | 10 |
| weapon_sword_004 | 미스릴 검 | Rare | 120 | 10,000 | 20 |
| weapon_sword_005 | 마법 검 | Epic | 250 | 50,000 | 30 |
| weapon_sword_006 | 드래곤 검 | Epic | 500 | 200,000 | 40 |
| weapon_sword_007 | 은둔자의 검 | Legendary | 1,000 | 1,000,000 | 50 |
| weapon_sword_008 | 암살자의 단검 | Legendary | 800 | 800,000 | 45 |
| weapon_sword_009 | 시간의 검 | Mythic | 2,500 | 10,000,000 | 70 |
| weapon_sword_010 | 무한의 검 | Mythic | 5,000 | 100,000,000 | 100 |

#### 방어구 (10개)
| ID | 이름 | 등급 | 방어력 | 체력 | 가격 | 레벨 |
|----|------|------|--------|------|------|------|
| armor_cloth_001 | 낡은 천 갑옷 | Common | 5 | 50 | 100 | 1 |
| armor_leather_001 | 가죽 갑옷 | Common | 15 | 100 | 500 | 5 |
| armor_chain_001 | 사슬 갑옷 | Rare | 35 | 250 | 2,000 | 10 |
| armor_plate_001 | 판금 갑옷 | Rare | 80 | 500 | 10,000 | 20 |
| armor_magic_001 | 마법 로브 | Epic | 150 | 1,000 | 50,000 | 30 |
| armor_dragon_001 | 드래곤 갑옷 | Epic | 300 | 2,000 | 200,000 | 40 |
| armor_stealth_001 | 은신 갑옷 | Legendary | 600 | 4,000 | 1,000,000 | 50 |
| armor_time_001 | 시간의 갑옷 | Legendary | 1,200 | 8,000 | 5,000,000 | 60 |
| armor_infinite_001 | 무한의 갑옷 | Mythic | 3,000 | 20,000 | 50,000,000 | 80 |
| armor_god_001 | 신의 갑옷 | Mythic | 6,000 | 50,000 | 500,000,000 | 100 |

#### 악세서리 (10개)
| ID | 이름 | 등급 | 효과 | 가격 |
|----|------|------|------|------|
| accessory_ring_001 | 힘의 반지 | Rare | 공격력 +10% | 5,000 |
| accessory_ring_002 | 부의 반지 | Rare | 골드 +20% | 5,000 |
| accessory_ring_003 | 성장의 반지 | Epic | 경험치 +30% | 50,000 |
| accessory_necklace_001 | 은둔의 목걸이 | Epic | 의심도 -10% | 100,000 |
| accessory_necklace_002 | 행운의 목걸이 | Legendary | 치명타 +15% | 500,000 |
| accessory_ring_004 | 흡혈 반지 | Legendary | 공격력 +25%, 체력 회복 | 800,000 |
| accessory_ring_005 | 시간 반지 | Mythic | 오프라인 수익 +50% | 5,000,000 |
| accessory_necklace_003 | 무한 목걸이 | Mythic | 모든 능력치 +30% | 20,000,000 |
| accessory_earring_001 | 신의 귀걸이 | Mythic | 클릭 파워 +100% | 50,000,000 |
| accessory_bracelet_001 | 초월의 팔찌 | Mythic | 환생 보너스 +20% | 100,000,000 |

#### 소비품 (15개)
| ID | 이름 | 등급 | 효과 | 가격 |
|----|------|------|------|------|
| potion_hp_small | 작은 체력 물약 | Common | HP 100 회복 | 50 |
| potion_hp_medium | 체력 물약 | Rare | HP 500 회복 | 200 |
| potion_hp_large | 큰 체력 물약 | Epic | HP 2,000 회복 | 1,000 |
| potion_energy_small | 에너지 물약 | Common | 에너지 10 회복 | 100 |
| potion_energy_full | 완전 회복 물약 | Epic | 에너지 완전 회복 | 5,000 |
| boost_gold_30min | 골드 부스트 (30분) | Rare | 골드 +50% (30분) | 3,000 |
| boost_gold_1hour | 골드 부스트 (1시간) | Epic | 골드 +50% (1시간) | 5,000 |
| boost_exp_30min | 경험치 부스트 (30분) | Rare | 경험치 +50% (30분) | 3,000 |
| boost_exp_1hour | 경험치 부스트 (1시간) | Epic | 경험치 +50% (1시간) | 5,000 |
| boost_click_30min | 클릭 부스트 (30분) | Rare | 클릭 파워 +100% (30분) | 4,000 |
| scroll_skill_reset | 스킬 초기화 주문서 | Epic | 스킬 포인트 초기화 | 10,000 |
| scroll_perk_reset | 특전 초기화 주문서 | Legendary | 환생 특전 초기화 | 50,000 |
| ticket_dungeon | 던전 입장권 | Rare | 에너지 없이 던전 입장 | 2,000 |
| ticket_gacha | 가챠 티켓 | Epic | 무료 가챠 1회 | 10,000 |
| suspicion_down | 의심 감소 물약 | Epic | 의심도 -30 | 8,000 |

#### 재료 (5개)
| ID | 이름 | 등급 | 설명 |
|----|------|------|------|
| material_slime_gel | 슬라임 젤리 | Common | 슬라임이 떨어뜨린 젤리 |
| material_wolf_fang | 늑대 송곳니 | Rare | 늑대의 날카로운 송곳니 |
| material_dragon_scale | 드래곤 비늘 | Epic | 드래곤의 단단한 비늘 |
| material_magic_crystal | 마법 수정 | Legendary | 마력이 응축된 수정 |
| material_time_fragment | 시간의 파편 | Mythic | 시간의 흐름이 담긴 신비한 파편 |

---

## ⚔️ 스킬 데이터

### SkillData.cs (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "GameData/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillId;
    public string skillName;
    public string skillNameEN;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Skill Type")]
    public SkillCategory category;
    public SkillTargetType targetType;

    [Header("Level Info")]
    public int maxLevel = 10;
    public int[] skillPointCost;    // 레벨당 필요 스킬 포인트

    [Header("Prerequisites")]
    public string[] prerequisiteSkills; // 선행 스킬 ID
    public int requiredPlayerLevel;
    public int requiredRebirthCount;

    [Header("Effects (Level 1 values)")]
    public SkillEffectType effectType;
    public float baseValue;          // 기본 수치
    public float valuePerLevel;      // 레벨당 증가량
    public float duration;           // 지속 시간 (0 = 영구)
    public float cooldown;           // 쿨타임

    [Header("Special")]
    public bool isPassive = true;    // 패시브/액티브
    public bool isPermanent = true;  // 영구 효과

    public float GetValueAtLevel(int level)
    {
        return baseValue + (valuePerLevel * (level - 1));
    }

    public int GetTotalCost(int targetLevel)
    {
        int total = 0;
        for (int i = 0; i < targetLevel && i < skillPointCost.Length; i++)
        {
            total += skillPointCost[i];
        }
        return total;
    }
}

public enum SkillCategory
{
    Stealth,     // 은둔 (의심도 관리)
    Combat,      // 전투 (공격/방어)
    Growth       // 성장 (골드/경험치)
}

public enum SkillTargetType
{
    Self,        // 자신
    Enemy,       // 적
    All          // 전체
}

public enum SkillEffectType
{
    IncreaseClickPower,      // 클릭 파워 증가
    IncreaseGoldGain,        // 골드 획득 증가
    IncreaseExpGain,         // 경험치 획득 증가
    IncreaseCriticalChance,  // 치명타 확률 증가
    IncreaseCriticalDamage,  // 치명타 데미지 증가
    ReduceSuspicionGain,     // 의심도 증가 감소
    IncreaseOfflineGain,     // 오프라인 수익 증가
    IncreaseIdleGain,        // 방치 수익 증가
    ReduceEnergyCost,        // 에너지 소모 감소
    IncreaseLootChance,      // 드롭률 증가
    ReduceShopPrice,         // 상점 가격 감소
    IncreaseSkillPoints,     // 스킬 포인트 획득 증가
    AutoCollectGold,         // 골드 자동 수집
    AutoBattle,              // 자동 전투
    DamageReflect           // 데미지 반사
}
```

### 스킬 트리 데이터 (50개 스킬)

#### 은둔 카테고리 (15개)
```json
[
  {
    "skillId": "stealth_001",
    "skillName": "평범한 척",
    "category": "Stealth",
    "effectType": "ReduceSuspicionGain",
    "baseValue": 0.05,
    "valuePerLevel": 0.02,
    "maxLevel": 10,
    "skillPointCost": [1, 1, 2, 2, 3, 3, 4, 4, 5, 5],
    "description": "의심도 증가율이 감소합니다."
  },
  {
    "skillId": "stealth_002",
    "skillName": "은밀한 행동",
    "category": "Stealth",
    "effectType": "ReduceSuspicionGain",
    "baseValue": 0.10,
    "valuePerLevel": 0.03,
    "maxLevel": 10,
    "prerequisiteSkills": ["stealth_001"],
    "requiredPlayerLevel": 10,
    "skillPointCost": [2, 2, 3, 3, 4, 4, 5, 5, 6, 6]
  },
  {
    "skillId": "stealth_003",
    "skillName": "완벽한 위장",
    "category": "Stealth",
    "effectType": "ReduceSuspicionGain",
    "baseValue": 0.15,
    "valuePerLevel": 0.05,
    "maxLevel": 5,
    "prerequisiteSkills": ["stealth_002"],
    "requiredPlayerLevel": 25,
    "skillPointCost": [5, 5, 10, 10, 15]
  },
  {
    "skillId": "stealth_004",
    "skillName": "야간 활동",
    "category": "Stealth",
    "effectType": "IncreaseLootChance",
    "baseValue": 0.10,
    "valuePerLevel": 0.05,
    "maxLevel": 10,
    "description": "밤 시간대 드롭률이 증가합니다."
  },
  {
    "skillId": "stealth_005",
    "skillName": "무인 던전 전문가",
    "category": "Stealth",
    "effectType": "IncreaseLootChance",
    "baseValue": 0.20,
    "valuePerLevel": 0.05,
    "maxLevel": 5,
    "prerequisiteSkills": ["stealth_004"],
    "description": "무인 던전에서 보상이 증가합니다."
  }
]
```

#### 전투 카테고리 (20개)
```json
[
  {
    "skillId": "combat_001",
    "skillName": "기본 무술",
    "category": "Combat",
    "effectType": "IncreaseClickPower",
    "baseValue": 0.10,
    "valuePerLevel": 0.05,
    "maxLevel": 10,
    "skillPointCost": [1, 1, 2, 2, 3, 3, 4, 4, 5, 5]
  },
  {
    "skillId": "combat_002",
    "skillName": "강화된 주먹",
    "category": "Combat",
    "effectType": "IncreaseClickPower",
    "baseValue": 0.20,
    "valuePerLevel": 0.10,
    "maxLevel": 10,
    "prerequisiteSkills": ["combat_001"],
    "skillPointCost": [2, 2, 3, 3, 4, 4, 5, 5, 6, 6]
  },
  {
    "skillId": "combat_003",
    "skillName": "치명타",
    "category": "Combat",
    "effectType": "IncreaseCriticalChance",
    "baseValue": 0.05,
    "valuePerLevel": 0.02,
    "maxLevel": 10,
    "skillPointCost": [2, 2, 3, 3, 4, 4, 5, 5, 6, 6]
  },
  {
    "skillId": "combat_004",
    "skillName": "치명타 강화",
    "category": "Combat",
    "effectType": "IncreaseCriticalDamage",
    "baseValue": 0.50,
    "valuePerLevel": 0.25,
    "maxLevel": 10,
    "prerequisiteSkills": ["combat_003"],
    "skillPointCost": [3, 3, 4, 4, 5, 5, 6, 6, 7, 7]
  },
  {
    "skillId": "combat_005",
    "skillName": "자동 전투",
    "category": "Combat",
    "effectType": "AutoBattle",
    "baseValue": 1,
    "valuePerLevel": 0.10,
    "maxLevel": 5,
    "requiredPlayerLevel": 20,
    "skillPointCost": [10, 10, 15, 15, 20],
    "description": "자동 전투 기능이 해금됩니다."
  }
]
```

#### 성장 카테고리 (15개)
```json
[
  {
    "skillId": "growth_001",
    "skillName": "효율적 학습",
    "category": "Growth",
    "effectType": "IncreaseExpGain",
    "baseValue": 0.10,
    "valuePerLevel": 0.05,
    "maxLevel": 10,
    "skillPointCost": [1, 1, 2, 2, 3, 3, 4, 4, 5, 5]
  },
  {
    "skillId": "growth_002",
    "skillName": "황금 손가락",
    "category": "Growth",
    "effectType": "IncreaseGoldGain",
    "baseValue": 0.10,
    "valuePerLevel": 0.05,
    "maxLevel": 10,
    "skillPointCost": [1, 1, 2, 2, 3, 3, 4, 4, 5, 5]
  },
  {
    "skillId": "growth_003",
    "skillName": "방치 수익",
    "category": "Growth",
    "effectType": "IncreaseIdleGain",
    "baseValue": 0.20,
    "valuePerLevel": 0.10,
    "maxLevel": 10,
    "skillPointCost": [2, 2, 3, 3, 4, 4, 5, 5, 6, 6]
  },
  {
    "skillId": "growth_004",
    "skillName": "오프라인 수익",
    "category": "Growth",
    "effectType": "IncreaseOfflineGain",
    "baseValue": 0.25,
    "valuePerLevel": 0.15,
    "maxLevel": 5,
    "requiredPlayerLevel": 15,
    "skillPointCost": [5, 5, 10, 10, 15]
  },
  {
    "skillId": "growth_005",
    "skillName": "골드 자동 수집",
    "category": "Growth",
    "effectType": "AutoCollectGold",
    "baseValue": 1,
    "maxLevel": 1,
    "requiredPlayerLevel": 30,
    "skillPointCost": [20],
    "description": "골드를 자동으로 수집합니다."
  }
]
```

---

## 👾 몬스터 데이터

### MonsterData.cs (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonster", menuName = "GameData/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterId;
    public string monsterName;
    public string monsterNameEN;
    [TextArea(2, 4)]
    public string description;
    public Sprite sprite;

    [Header("Monster Type")]
    public MonsterGrade grade;
    public MonsterType monsterType;
    public ElementType element;
    public BiomeType biome;

    [Header("Stats")]
    public long baseHealth;
    public long baseAttack;
    public float attackSpeed = 1.0f;
    public int defense = 0;
    public float moveSpeed = 1.0f;

    [Header("Rewards")]
    public long goldMin;
    public long goldMax;
    public long expReward;
    public float dropChance = 0.1f;
    public DropTable[] dropTable;

    [Header("Behavior")]
    public MonsterPattern[] attackPatterns;
    public float aggroRange = 5.0f;

    [Header("Special")]
    public bool isBoss = false;
    public bool isElite = false;
    public string[] specialDialogue;
}

public enum MonsterGrade
{
    F, E, D, C, B, A, S, SS, SSS,
    EX, Mythic, Transcendent, Divine, Cosmic, Infinite
}

public enum MonsterType
{
    Normal,      // 일반
    Elite,       // 정예
    Boss,        // 보스
    WorldBoss,   // 월드 보스
    Hidden,      // 히든 보스
    Event        // 이벤트
}

public enum ElementType
{
    None, Fire, Water, Earth, Wind, Light, Dark,
    Thunder, Ice, Nature, Chaos
}

public enum BiomeType
{
    Slime, Undead, Beast, Dragon, Demon, Angel,
    Machine, Plant, Insect, Aquatic, Elemental
}

[System.Serializable]
public class DropTable
{
    public string itemId;
    public float dropRate;    // 0.0 ~ 1.0
    public int minAmount = 1;
    public int maxAmount = 1;
}

[System.Serializable]
public class MonsterPattern
{
    public string patternName;
    public float damageMultiplier = 1.0f;
    public float castTime = 0f;
    public float cooldown = 5.0f;
    public string animationTrigger;
}
```

### 몬스터 데이터 예시 (등급별 대표 몬스터)

```json
[
  {
    "monsterId": "slime_f_001",
    "monsterName": "슬라임",
    "grade": "F",
    "monsterType": "Normal",
    "element": "None",
    "biome": "Slime",
    "baseHealth": 50,
    "baseAttack": 5,
    "goldMin": 10,
    "goldMax": 20,
    "expReward": 5,
    "dropChance": 0.3,
    "dropTable": [
      {"itemId": "material_slime_gel", "dropRate": 0.5, "minAmount": 1, "maxAmount": 3}
    ]
  },
  {
    "monsterId": "goblin_e_001",
    "monsterName": "고블린",
    "grade": "E",
    "monsterType": "Normal",
    "element": "None",
    "biome": "Beast",
    "baseHealth": 150,
    "baseAttack": 15,
    "goldMin": 30,
    "goldMax": 60,
    "expReward": 15,
    "dropChance": 0.25,
    "dropTable": [
      {"itemId": "weapon_sword_001", "dropRate": 0.1}
    ]
  },
  {
    "monsterId": "dragon_ss_001",
    "monsterName": "레드 드래곤",
    "grade": "SS",
    "monsterType": "Boss",
    "element": "Fire",
    "biome": "Dragon",
    "baseHealth": 1000000,
    "baseAttack": 5000,
    "goldMin": 100000,
    "goldMax": 200000,
    "expReward": 50000,
    "isBoss": true,
    "dropChance": 0.8,
    "dropTable": [
      {"itemId": "material_dragon_scale", "dropRate": 0.5, "minAmount": 1, "maxAmount": 5},
      {"itemId": "weapon_sword_006", "dropRate": 0.1},
      {"itemId": "armor_dragon_001", "dropRate": 0.1}
    ],
    "attackPatterns": [
      {"patternName": "브레스", "damageMultiplier": 3.0, "castTime": 2.0, "cooldown": 10.0},
      {"patternName": "꼬리 휘두르기", "damageMultiplier": 1.5, "cooldown": 5.0}
    ]
  }
]
```

---

## 🏰 던전 데이터

### DungeonData.cs (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewDungeon", menuName = "GameData/Dungeon")]
public class DungeonData : ScriptableObject
{
    [Header("Basic Info")]
    public string dungeonId;
    public string dungeonName;
    public string dungeonNameEN;
    [TextArea(3, 5)]
    public string description;
    public Sprite thumbnail;

    [Header("Requirements")]
    public int minLevel;
    public int recommendedLevel;
    public int maxLevel = 999;
    public int energyCost = 10;

    [Header("Dungeon Type")]
    public DungeonCategory category;
    public DungeonDifficulty difficulty;
    public TimeOfDay[] availableTimes;

    [Header("Waves")]
    public int totalWaves = 10;
    public DungeonWave[] waves;

    [Header("Rewards")]
    public long baseGoldReward;
    public long baseExpReward;
    public float rewardMultiplier = 1.0f;
    public string[] guaranteedDrops;
    public string[] possibleDrops;

    [Header("Special")]
    public float witnessRisk = 0.1f;   // 목격 위험도 (의심도 증가)
    public bool isUnmanned = false;     // 무인 던전
    public bool isNightOnly = false;    // 야간 전용
    public bool isSafezone = false;     // 안전 지역
    public bool isEventDungeon = false;
    public int dailyEntryLimit = 0;     // 0 = 무제한
}

public enum DungeonCategory
{
    Normal,      // 일반 던전
    Special,     // 특수 던전
    Boss,        // 보스 던전
    Event,       // 이벤트 던전
    Challenge,   // 챌린지 던전
    Infinite     // 무한 던전
}

public enum DungeonDifficulty
{
    Easy, Normal, Hard, Expert, Master, Infinite
}

public enum TimeOfDay
{
    Dawn,        // 새벽 (04:00 - 07:59)
    Morning,     // 아침 (08:00 - 11:59)
    Afternoon,   // 오후 (12:00 - 17:59)
    Evening,     // 저녁 (18:00 - 21:59)
    Night,       // 밤 (22:00 - 03:59)
    AllDay       // 하루 종일
}

[System.Serializable]
public class DungeonWave
{
    public int waveNumber;
    public SpawnInfo[] spawns;
    public long bonusGold;
    public long bonusExp;
}

[System.Serializable]
public class SpawnInfo
{
    public string monsterId;
    public int count = 1;
    public float levelMultiplier = 1.0f;
}
```

### 던전 데이터 예시

```json
[
  {
    "dungeonId": "dungeon_beginner_001",
    "dungeonName": "슬라임 동굴",
    "dungeonNameEN": "Slime Cave",
    "description": "초보자를 위한 쉬운 던전. 슬라임들이 득실거린다.",
    "category": "Normal",
    "difficulty": "Easy",
    "minLevel": 1,
    "recommendedLevel": 5,
    "energyCost": 5,
    "totalWaves": 5,
    "baseGoldReward": 100,
    "baseExpReward": 50,
    "witnessRisk": 0.05,
    "isUnmanned": false,
    "availableTimes": ["AllDay"]
  },
  {
    "dungeonId": "dungeon_night_001",
    "dungeonName": "버려진 공장",
    "dungeonNameEN": "Abandoned Factory",
    "description": "밤에만 입장 가능한 무인 던전. 목격 위험이 없다.",
    "category": "Special",
    "difficulty": "Normal",
    "minLevel": 20,
    "recommendedLevel": 30,
    "energyCost": 15,
    "totalWaves": 10,
    "baseGoldReward": 5000,
    "baseExpReward": 2000,
    "rewardMultiplier": 1.5,
    "witnessRisk": 0,
    "isUnmanned": true,
    "isNightOnly": true,
    "availableTimes": ["Night"]
  },
  {
    "dungeonId": "dungeon_boss_dragon",
    "dungeonName": "드래곤의 둥지",
    "dungeonNameEN": "Dragon's Nest",
    "description": "강력한 드래곤이 지키는 던전. 최고 난이도.",
    "category": "Boss",
    "difficulty": "Master",
    "minLevel": 50,
    "recommendedLevel": 70,
    "energyCost": 50,
    "totalWaves": 1,
    "baseGoldReward": 100000,
    "baseExpReward": 50000,
    "witnessRisk": 0.8,
    "dailyEntryLimit": 3,
    "guaranteedDrops": ["material_dragon_scale"],
    "possibleDrops": ["weapon_sword_006", "armor_dragon_001"]
  }
]
```

---

## 🎁 환생 특전 데이터

### 환생 특전 리스트 (20개)

```json
[
  {
    "perkId": "perk_click_001",
    "perkName": "강화된 클릭",
    "category": "Combat",
    "description": "클릭 파워가 영구적으로 10% 증가합니다.",
    "cost": 100,
    "maxLevel": 10,
    "effectType": "ClickPower",
    "effectValue": 0.1,
    "costMultiplier": 1.5
  },
  {
    "perkId": "perk_gold_001",
    "perkName": "황금 손가락",
    "category": "Growth",
    "description": "골드 획득량이 영구적으로 10% 증가합니다.",
    "cost": 100,
    "maxLevel": 10,
    "effectType": "GoldGain",
    "effectValue": 0.1,
    "costMultiplier": 1.5
  },
  {
    "perkId": "perk_start_gold",
    "perkName": "시작 자본",
    "category": "Growth",
    "description": "환생 시 10,000 골드를 가지고 시작합니다.",
    "cost": 500,
    "maxLevel": 5,
    "effectType": "StartingGold",
    "effectValue": 10000,
    "costMultiplier": 2.0
  },
  {
    "perkId": "perk_start_level",
    "perkName": "빠른 시작",
    "category": "Growth",
    "description": "환생 시 레벨 5로 시작합니다.",
    "cost": 1000,
    "maxLevel": 5,
    "effectType": "StartingLevel",
    "effectValue": 5,
    "costMultiplier": 2.0
  },
  {
    "perkId": "perk_offline",
    "perkName": "효율적인 방치",
    "category": "Growth",
    "description": "오프라인 수익이 20% 증가합니다.",
    "cost": 300,
    "maxLevel": 5,
    "effectType": "OfflineGain",
    "effectValue": 0.2,
    "costMultiplier": 1.8
  }
]
```

---

## 💬 대화 데이터

### DialogueData 구조 (JSON)

```json
{
  "dialogues": [
    {
      "dialogueId": "mother_morning_001",
      "speakerName": "어머니",
      "speakerSprite": "mother_neutral",
      "triggerConditions": {
        "timeOfDay": "Morning",
        "suspicionMin": 0,
        "suspicionMax": 30
      },
      "lines": [
        "민수야, 아침이야. 일어나렴.",
        "오늘은 뭐 할 거니?"
      ],
      "choices": [
        {
          "choiceText": "집에서 쉴래요",
          "suspicionChange": 5,
          "relationshipChange": -5,
          "nextDialogueId": null
        },
        {
          "choiceText": "알바 구하러 나갈게요",
          "suspicionChange": -10,
          "relationshipChange": 10,
          "nextDialogueId": "mother_morning_001_happy"
        }
      ]
    },
    {
      "dialogueId": "sister_suspicious_001",
      "speakerName": "여동생",
      "speakerSprite": "sister_suspicious",
      "triggerConditions": {
        "suspicionMin": 50,
        "playerLevelMin": 30
      },
      "lines": [
        "오빠... 요즘 이상한데?",
        "밤에 어디 다녀오는 거야?",
        "혹시 뭔가 숨기는 거 아니야?"
      ],
      "choices": [
        {
          "choiceText": "아무것도 아니야",
          "suspicionChange": 10,
          "relationshipChange": -10
        },
        {
          "choiceText": "...비밀이야",
          "suspicionChange": 15,
          "relationshipChange": -5
        },
        {
          "choiceText": "(진실을 말한다)",
          "suspicionChange": -50,
          "relationshipChange": 30,
          "nextDialogueId": "sister_truth_reveal",
          "requirements": {
            "relationshipMin": 80
          }
        }
      ]
    }
  ]
}
```

---

## 🏆 업적 데이터

### AchievementData 구조

```json
{
  "achievements": [
    {
      "achievementId": "ach_first_click",
      "achievementName": "첫 클릭",
      "description": "첫 클릭을 했습니다.",
      "category": "Basic",
      "isHidden": false,
      "requirements": {
        "type": "ClickCount",
        "value": 1
      },
      "rewards": {
        "gold": 100,
        "gems": 10
      }
    },
    {
      "achievementId": "ach_level_100",
      "achievementName": "레벨 100 달성",
      "description": "레벨 100에 도달했습니다.",
      "category": "Level",
      "requirements": {
        "type": "PlayerLevel",
        "value": 100
      },
      "rewards": {
        "gold": 100000,
        "gems": 100,
        "item": "ticket_gacha"
      }
    },
    {
      "achievementId": "ach_first_rebirth",
      "achievementName": "새로운 시작",
      "description": "첫 환생을 완료했습니다.",
      "category": "Rebirth",
      "requirements": {
        "type": "RebirthCount",
        "value": 1
      },
      "rewards": {
        "rebirthPoints": 1000,
        "gems": 500
      }
    },
    {
      "achievementId": "ach_perfect_stealth",
      "achievementName": "완벽한 은둔",
      "description": "의심도를 0으로 유지한 채 레벨 50에 도달했습니다.",
      "category": "Challenge",
      "isHidden": true,
      "requirements": {
        "type": "LevelWithSuspicion",
        "levelRequired": 50,
        "maxSuspicion": 0
      },
      "rewards": {
        "gold": 500000,
        "gems": 1000,
        "item": "accessory_necklace_001"
      }
    }
  ]
}
```

---

## 🛒 상점 데이터

### ShopData 구조

```json
{
  "shops": [
    {
      "shopId": "shop_normal",
      "shopName": "일반 상점",
      "shopType": "Normal",
      "refreshInterval": 86400,
      "items": [
        {
          "itemId": "weapon_sword_002",
          "price": 500,
          "currency": "Gold",
          "stock": -1
        },
        {
          "itemId": "potion_hp_small",
          "price": 50,
          "currency": "Gold",
          "stock": -1
        }
      ]
    },
    {
      "shopId": "shop_premium",
      "shopName": "프리미엄 상점",
      "shopType": "Premium",
      "items": [
        {
          "itemId": "boost_gold_1hour",
          "price": 100,
          "currency": "Gem",
          "stock": -1
        },
        {
          "itemId": "ticket_gacha",
          "price": 300,
          "currency": "Gem",
          "stock": 10,
          "refreshType": "Daily"
        }
      ]
    },
    {
      "shopId": "shop_iap",
      "shopName": "결제 상점",
      "shopType": "IAP",
      "packages": [
        {
          "packageId": "iap_beginner",
          "packageName": "초보자 패키지",
          "price": 1100,
          "currency": "KRW",
          "contents": [
            {"type": "Gold", "amount": 100000},
            {"type": "Gem", "amount": 500},
            {"type": "Item", "itemId": "boost_gold_1hour", "amount": 5}
          ],
          "isPurchaseOnce": true
        }
      ]
    }
  ]
}
```

---

## 🔧 데이터 로딩 시스템

### GameDatabase.cs (전체 데이터 관리)

```csharp
using System.Collections.Generic;
using UnityEngine;

public class GameDatabase : MonoBehaviour
{
    public static GameDatabase Instance;

    [Header("Data Collections")]
    public Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
    public Dictionary<string, SkillData> skills = new Dictionary<string, SkillData>();
    public Dictionary<string, MonsterData> monsters = new Dictionary<string, MonsterData>();
    public Dictionary<string, DungeonData> dungeons = new Dictionary<string, DungeonData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllData()
    {
        LoadItems();
        LoadSkills();
        LoadMonsters();
        LoadDungeons();
    }

    private void LoadItems()
    {
        ItemData[] itemArray = Resources.LoadAll<ItemData>("GameData/Items");
        foreach (var item in itemArray)
        {
            items[item.itemId] = item;
        }
        Debug.Log($"Loaded {items.Count} items");
    }

    private void LoadSkills()
    {
        SkillData[] skillArray = Resources.LoadAll<SkillData>("GameData/Skills");
        foreach (var skill in skillArray)
        {
            skills[skill.skillId] = skill;
        }
        Debug.Log($"Loaded {skills.Count} skills");
    }

    private void LoadMonsters()
    {
        MonsterData[] monsterArray = Resources.LoadAll<MonsterData>("GameData/Monsters");
        foreach (var monster in monsterArray)
        {
            monsters[monster.monsterId] = monster;
        }
        Debug.Log($"Loaded {monsters.Count} monsters");
    }

    private void LoadDungeons()
    {
        DungeonData[] dungeonArray = Resources.LoadAll<DungeonData>("GameData/Dungeons");
        foreach (var dungeon in dungeonArray)
        {
            dungeons[dungeon.dungeonId] = dungeon;
        }
        Debug.Log($"Loaded {dungeons.Count} dungeons");
    }

    // Get methods
    public ItemData GetItem(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : null;
    }

    public SkillData GetSkill(string skillId)
    {
        return skills.ContainsKey(skillId) ? skills[skillId] : null;
    }

    public MonsterData GetMonster(string monsterId)
    {
        return monsters.ContainsKey(monsterId) ? monsters[monsterId] : null;
    }

    public DungeonData GetDungeon(string dungeonId)
    {
        return dungeons.ContainsKey(dungeonId) ? dungeons[dungeonId] : null;
    }

    // Filter methods
    public List<ItemData> GetItemsByType(ItemType type)
    {
        List<ItemData> result = new List<ItemData>();
        foreach (var item in items.Values)
        {
            if (item.itemType == type)
                result.Add(item);
        }
        return result;
    }

    public List<MonsterData> GetMonstersByGrade(MonsterGrade grade)
    {
        List<MonsterData> result = new List<MonsterData>();
        foreach (var monster in monsters.Values)
        {
            if (monster.grade == grade)
                result.Add(monster);
        }
        return result;
    }
}
```

---

## 📁 Unity 프로젝트 데이터 폴더 구조

```
Assets/
├── Resources/
│   └── GameData/
│       ├── Items/
│       │   ├── Weapons/
│       │   │   ├── weapon_sword_001.asset
│       │   │   ├── weapon_sword_002.asset
│       │   │   └── ...
│       │   ├── Armors/
│       │   ├── Accessories/
│       │   ├── Consumables/
│       │   └── Materials/
│       ├── Skills/
│       │   ├── Stealth/
│       │   ├── Combat/
│       │   └── Growth/
│       ├── Monsters/
│       │   ├── F_Grade/
│       │   ├── E_Grade/
│       │   └── ...
│       └── Dungeons/
│           ├── Normal/
│           ├── Special/
│           └── Boss/
├── StreamingAssets/
│   └── Data/
│       ├── dialogues.json
│       ├── achievements.json
│       ├── shops.json
│       └── localization/
│           ├── ko_KR.json
│           └── en_US.json
└── Scripts/
    └── Data/
        ├── ItemData.cs
        ├── SkillData.cs
        ├── MonsterData.cs
        ├── DungeonData.cs
        └── GameDatabase.cs
```

---

## 📋 체크리스트

### ScriptableObject 클래스 구현
- [ ] ItemData.cs
- [ ] SkillData.cs
- [ ] MonsterData.cs
- [ ] DungeonData.cs
- [ ] RebirthPerkData.cs

### 데이터 생성
- [ ] 아이템 50개 이상
- [ ] 스킬 50개
- [ ] 몬스터 100개 이상
- [ ] 던전 30개 이상

### JSON 데이터 작성
- [ ] 대화 데이터 (30개 이상)
- [ ] 업적 데이터 (50개 이상)
- [ ] 상점 데이터

### 데이터베이스 시스템
- [ ] GameDatabase 싱글톤
- [ ] 데이터 로딩 시스템
- [ ] Get/Filter 메서드

### 테스트
- [ ] 모든 데이터 로딩 확인
- [ ] ID 중복 체크
- [ ] 참조 무결성 확인

---

**작성 완료일**: 2025-11-13
**다음 단계**: 밸런싱 스프레드시트 작성
