# 아이템 시스템 명세서

**프로젝트**: 숨겨진 성장의 백수 영웅
**작성일**: 2025-11-13
**버전**: 1.0

---

## 📋 목차

1. [시스템 개요](#시스템-개요)
2. [아이템 타입](#아이템-타입)
3. [아이템 등급](#아이템-등급)
4. [인벤토리 시스템](#인벤토리-시스템)
5. [장비 시스템](#장비-시스템)
6. [아이템 획득](#아이템-획득)
7. [아이템 강화](#아이템-강화)
8. [아이템 합성](#아이템-합성)
9. [아이템 거래](#아이템-거래)
10. [코드 구현](#코드-구현)

---

## 🎯 시스템 개요

### 핵심 기능

1. **인벤토리 관리** - 100칸 그리드 시스템
2. **장비 착용** - 8개 슬롯 (무기, 방어구, 악세서리)
3. **아이템 획득** - 던전, 상점, 가챠, 퀘스트
4. **아이템 강화** - +0 ~ +15 강화 시스템
5. **아이템 합성** - 재료 조합으로 상위 아이템 제작
6. **아이템 판매** - 골드 획득

### 기술 스택

```
Unity C#
ScriptableObject (데이터)
JSON (저장/로드)
Dictionary (빠른 검색)
Object Pooling (UI 슬롯)
```

---

## 📦 아이템 타입

### 1. 무기 (Weapon)

**장비 슬롯**: Weapon
**주요 스탯**: 공격력, 치명타

```csharp
public enum WeaponType
{
    Sword,      // 검
    Dagger,     // 단검
    Spear,      // 창
    Axe,        // 도끼
    Bow,        // 활
    Staff,      // 지팡이
    Fist        // 격투
}
```

**무기 예시**:

| 이름 | 등급 | 공격력 | 치명타율 | 특수 효과 |
|------|------|--------|----------|-----------|
| 녹슨 검 | Common | 10 | 0% | - |
| 철 검 | Common | 25 | 0% | - |
| 강철 검 | Rare | 50 | 5% | - |
| 마법 검 | Epic | 250 | 10% | 마나 +50 |
| 은둔자의 검 | Legendary | 1,000 | 15% | 의심도 -20% |
| 무한의 검 | Mythic | 5,000 | 25% | 모든 능력치 +50% |

### 2. 방어구 (Armor)

**장비 슬롯**: Helmet, Armor, Gloves, Boots
**주요 스탯**: 방어력, 체력

```csharp
public enum ArmorType
{
    Helmet,     // 투구
    Chest,      // 갑옷
    Gloves,     // 장갑
    Boots       // 신발
}
```

**방어구 예시**:

| 이름 | 등급 | 부위 | 방어력 | 체력 | 특수 효과 |
|------|------|------|--------|------|-----------|
| 천 투구 | Common | Helmet | 5 | 50 | - |
| 가죽 갑옷 | Common | Chest | 15 | 100 | - |
| 판금 장갑 | Rare | Gloves | 35 | 250 | - |
| 드래곤 갑옷 | Epic | Chest | 300 | 2,000 | 화염 저항 +30% |
| 은신 갑옷 | Legendary | Chest | 600 | 4,000 | 의심도 -30% |
| 신의 갑옷 | Mythic | Chest | 6,000 | 50,000 | 받는 데미지 -50% |

### 3. 악세서리 (Accessory)

**장비 슬롯**: Necklace, Ring1, Ring2
**주요 스탯**: 특수 보너스

```csharp
public enum AccessoryType
{
    Necklace,   // 목걸이
    Ring,       // 반지
    Earring,    // 귀걸이
    Bracelet    // 팔찌
}
```

**악세서리 예시**:

| 이름 | 등급 | 부위 | 효과 |
|------|------|------|------|
| 힘의 반지 | Rare | Ring | 공격력 +10% |
| 부의 반지 | Rare | Ring | 골드 획득 +20% |
| 성장의 반지 | Epic | Ring | 경험치 획득 +30% |
| 은둔의 목걸이 | Epic | Necklace | 의심도 감소 -10% |
| 행운의 목걸이 | Legendary | Necklace | 치명타 확률 +15% |
| 시간 반지 | Mythic | Ring | 오프라인 수익 +50% |

### 4. 소비품 (Consumable)

**겹침**: 99개까지
**효과**: 즉시 또는 버프

```csharp
public enum ConsumableType
{
    Potion,     // 물약
    Scroll,     // 주문서
    Ticket,     // 티켓
    Boost,      // 부스터
    Food        // 음식
}
```

**소비품 예시**:

| 이름 | 등급 | 효과 | 지속 시간 |
|------|------|------|-----------|
| 작은 체력 물약 | Common | HP 100 회복 | 즉시 |
| 골드 부스트 | Rare | 골드 +50% | 30분 |
| 경험치 부스트 | Rare | 경험치 +50% | 30분 |
| 클릭 부스트 | Epic | 클릭 파워 +100% | 30분 |
| 스킬 초기화 주문서 | Epic | 스킬 포인트 초기화 | 즉시 |
| 의심 감소 물약 | Epic | 의심도 -30 | 즉시 |

### 5. 재료 (Material)

**용도**: 강화, 합성
**겹침**: 999개까지

| 이름 | 등급 | 용도 |
|------|------|------|
| 슬라임 젤리 | Common | 합성 재료 |
| 늑대 송곳니 | Rare | 무기 강화 |
| 드래곤 비늘 | Epic | 방어구 강화 |
| 마법 수정 | Legendary | 고급 강화 |
| 시간의 파편 | Mythic | 최상급 합성 |

### 6. 특수 아이템 (Special)

| 이름 | 효과 | 획득 방법 |
|------|------|-----------|
| 환생의 증표 | 환생 시 필요 | 레벨 100 달성 |
| 황금 열쇠 | 특별 던전 입장 | 일일 보상 |
| 가챠 티켓 | 무료 가챠 1회 | 이벤트 |
| VIP 티켓 | VIP 1일권 | 상점 구매 |

---

## ⭐ 아이템 등급

### 등급 시스템

| 등급 | 이름 | 색상 | 확률 (가챠) | 판매가 배율 |
|------|------|------|-------------|-------------|
| 1 | Common (일반) | 회색 | 60% | ×1 |
| 2 | Rare (희귀) | 녹색 | 30% | ×5 |
| 3 | Epic (영웅) | 파랑 | 9% | ×25 |
| 4 | Legendary (전설) | 보라 | 0.9% | ×100 |
| 5 | Mythic (신화) | 금색 | 0.1% | ×500 |

### 등급별 특징

#### Common (일반)
- 가장 기본적인 아이템
- 초반 레벨업용
- 쉽게 획득 가능
- 강화 최대 +5

#### Rare (희귀)
- 중반 레벨 (10-30)용
- 특수 효과 1개
- 던전에서 드롭
- 강화 최대 +10

#### Epic (영웅)
- 중후반 레벨 (30-60)용
- 특수 효과 2개
- 보스 던전 드롭
- 강화 최대 +12

#### Legendary (전설)
- 후반 레벨 (60-100)용
- 특수 효과 3개
- 가챠 또는 히든 보스
- 강화 최대 +15
- 발광 이펙트

#### Mythic (신화)
- 엔드 게임용
- 특수 효과 5개
- 극악의 확률
- 강화 최대 +20
- 화려한 이펙트

### 등급별 스탯 배율

| 등급 | 기본 스탯 | 강화 효율 | 특수 효과 수 |
|------|-----------|-----------|--------------|
| Common | ×1 | +5% | 0 |
| Rare | ×5 | +10% | 1 |
| Epic | ×20 | +15% | 2 |
| Legendary | ×100 | +20% | 3 |
| Mythic | ×500 | +25% | 5 |

---

## 🎒 인벤토리 시스템

### 인벤토리 사양

```
총 칸 수: 100칸 (10×10 그리드)
확장: VIP 레벨별 +10칸 (최대 200칸)
정렬: 타입, 등급, 이름, 획득 순
필터: 전체, 장비, 소비품, 재료
```

### InventoryManager.cs

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Settings")]
    public int maxSlots = 100;
    public int usedSlots = 0;

    [Header("Items")]
    private Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();

    [System.Serializable]
    public class InventoryItem
    {
        public string itemId;
        public ItemData itemData;
        public int count;
        public int enhanceLevel;        // 강화 레벨
        public Dictionary<string, float> randomStats; // 랜덤 옵션

        public InventoryItem(string id, ItemData data, int amount = 1)
        {
            itemId = id;
            itemData = data;
            count = amount;
            enhanceLevel = 0;
            randomStats = new Dictionary<string, float>();
        }
    }

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

    // 아이템 추가
    public bool AddItem(string itemId, int count = 1, int enhanceLevel = 0)
    {
        ItemData itemData = GameDatabase.Instance.GetItem(itemId);
        if (itemData == null)
        {
            Debug.LogError($"Item not found: {itemId}");
            return false;
        }

        // 겹침 가능한 아이템
        if (itemData.maxStack > 1)
        {
            if (items.ContainsKey(itemId))
            {
                // 기존 아이템에 추가
                items[itemId].count += count;
                EventManager.TriggerItemAdded(itemId, count);
                UpdateUI();
                return true;
            }
        }

        // 인벤토리 공간 확인
        if (usedSlots >= maxSlots)
        {
            UIManager.Instance.ShowNotification("인벤토리가 가득 찼습니다!");
            return false;
        }

        // 새 아이템 추가
        InventoryItem newItem = new InventoryItem(itemId, itemData, count);
        newItem.enhanceLevel = enhanceLevel;

        // 고유 ID 생성 (장비는 각각 별도 슬롯)
        string uniqueId = itemId;
        if (itemData.maxStack == 1)
        {
            uniqueId = $"{itemId}_{System.Guid.NewGuid()}";
        }

        items[uniqueId] = newItem;
        usedSlots++;

        // 이벤트 발생
        EventManager.TriggerItemAdded(itemId, count);

        // UI 업데이트
        UpdateUI();

        // 업적 체크
        AchievementManager.Instance.CheckItemAchievements(itemId, itemData.grade);

        return true;
    }

    // 아이템 제거
    public bool RemoveItem(string uniqueId, int count = 1)
    {
        if (!items.ContainsKey(uniqueId))
            return false;

        InventoryItem item = items[uniqueId];

        if (item.count > count)
        {
            // 일부만 제거
            item.count -= count;
        }
        else
        {
            // 전체 제거
            items.Remove(uniqueId);
            usedSlots--;
        }

        EventManager.TriggerItemRemoved(item.itemId, count);
        UpdateUI();
        return true;
    }

    // 아이템 보유 확인
    public bool HasItem(string itemId, int requiredCount = 1)
    {
        int totalCount = 0;

        foreach (var item in items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.count;
            }
        }

        return totalCount >= requiredCount;
    }

    // 아이템 개수 가져오기
    public int GetItemCount(string itemId)
    {
        int totalCount = 0;

        foreach (var item in items.Values)
        {
            if (item.itemId == itemId)
            {
                totalCount += item.count;
            }
        }

        return totalCount;
    }

    // 아이템 사용
    public bool UseItem(string uniqueId)
    {
        if (!items.ContainsKey(uniqueId))
            return false;

        InventoryItem item = items[uniqueId];
        ItemData itemData = item.itemData;

        if (itemData.itemType != ItemType.Consumable)
        {
            Debug.Log("This item cannot be used");
            return false;
        }

        // 소비품 효과 적용
        ApplyConsumableEffects(itemData);

        // 아이템 1개 제거
        RemoveItem(uniqueId, 1);

        return true;
    }

    private void ApplyConsumableEffects(ItemData itemData)
    {
        foreach (var effect in itemData.effects)
        {
            switch (effect.type)
            {
                case ConsumableEffectType.RestoreHP:
                    GameManager.Instance.RestoreHP((int)effect.value);
                    break;

                case ConsumableEffectType.RestoreEnergy:
                    DungeonManager.Instance.RestoreEnergy((int)effect.value);
                    break;

                case ConsumableEffectType.BuffGoldGain:
                    BuffManager.Instance.AddBuff("gold_gain", effect.value, effect.duration);
                    break;

                case ConsumableEffectType.BuffExpGain:
                    BuffManager.Instance.AddBuff("exp_gain", effect.value, effect.duration);
                    break;

                case ConsumableEffectType.BuffClickPower:
                    BuffManager.Instance.AddBuff("click_power", effect.value, effect.duration);
                    break;

                case ConsumableEffectType.ReduceSuspicion:
                    SuspicionManager.Instance.AddSuspicion(-effect.value);
                    break;

                case ConsumableEffectType.ResetSkills:
                    SkillTreeManager.Instance.ResetSkillTree();
                    break;

                case ConsumableEffectType.ResetPerks:
                    RebirthPerkManager.Instance.ResetPerks();
                    break;
            }
        }

        UIManager.Instance.ShowNotification($"{itemData.itemName} 사용!");
    }

    // 아이템 판매
    public bool SellItem(string uniqueId, int count = 1)
    {
        if (!items.ContainsKey(uniqueId))
            return false;

        InventoryItem item = items[uniqueId];
        ItemData itemData = item.itemData;

        if (!itemData.isTradeable)
        {
            UIManager.Instance.ShowNotification("이 아이템은 판매할 수 없습니다!");
            return false;
        }

        // 판매 가격 계산 (기본 가격의 50%)
        long sellPrice = itemData.basePrice / 2;

        // 강화 레벨에 따른 가격 증가
        if (item.enhanceLevel > 0)
        {
            sellPrice += (long)(sellPrice * item.enhanceLevel * 0.1f);
        }

        // 총 판매 금액
        long totalPrice = sellPrice * count;

        // 골드 지급
        GameManager.Instance.AddGold(totalPrice);

        // 아이템 제거
        RemoveItem(uniqueId, count);

        UIManager.Instance.ShowNotification($"{itemData.itemName} 판매! +{NumberFormatter.Format(totalPrice)} 골드");

        return true;
    }

    // 인벤토리 정렬
    public void SortInventory(SortType sortType)
    {
        List<InventoryItem> itemList = items.Values.ToList();

        switch (sortType)
        {
            case SortType.ByType:
                itemList = itemList.OrderBy(i => i.itemData.itemType).ToList();
                break;

            case SortType.ByGrade:
                itemList = itemList.OrderByDescending(i => i.itemData.grade).ToList();
                break;

            case SortType.ByName:
                itemList = itemList.OrderBy(i => i.itemData.itemName).ToList();
                break;

            case SortType.ByAcquisition:
                // 기본 순서 유지
                break;
        }

        UpdateUI();
    }

    // 인벤토리 필터
    public List<InventoryItem> GetFilteredItems(ItemType filterType)
    {
        if (filterType == ItemType.Special) // "전체" 필터
        {
            return items.Values.ToList();
        }

        return items.Values.Where(i => i.itemData.itemType == filterType).ToList();
    }

    // 인벤토리 확장
    public bool ExpandInventory(int additionalSlots)
    {
        maxSlots += additionalSlots;
        UpdateUI();
        return true;
    }

    // 인벤토리 전체 삭제 (환생 시)
    public void ClearInventory(bool keepSpecialItems = false)
    {
        if (keepSpecialItems)
        {
            // 특수 아이템만 유지
            var itemsToRemove = items.Where(kvp => kvp.Value.itemData.itemType != ItemType.Special).ToList();
            foreach (var item in itemsToRemove)
            {
                items.Remove(item.Key);
            }
        }
        else
        {
            items.Clear();
        }

        usedSlots = items.Count;
        UpdateUI();
    }

    private void UpdateUI()
    {
        EventManager.TriggerInventoryChanged();
    }

    // 저장/로드
    public InventorySaveData GetSaveData()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var item in items.Values)
        {
            saveData.items.Add(new ItemSaveData
            {
                uniqueId = item.itemId,
                itemId = item.itemId,
                count = item.count,
                enhanceLevel = item.enhanceLevel
            });
        }

        return saveData;
    }

    public void LoadSaveData(InventorySaveData saveData)
    {
        items.Clear();

        foreach (var itemData in saveData.items)
        {
            AddItem(itemData.itemId, itemData.count, itemData.enhanceLevel);
        }
    }
}

public enum SortType
{
    ByType,
    ByGrade,
    ByName,
    ByAcquisition
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

[System.Serializable]
public class ItemSaveData
{
    public string uniqueId;
    public string itemId;
    public int count;
    public int enhanceLevel;
}
```

---

## ⚔️ 장비 시스템

### 장비 슬롯

```csharp
public enum EquipmentSlot
{
    Weapon,      // 무기
    Helmet,      // 투구
    Armor,       // 갑옷
    Gloves,      // 장갑
    Boots,       // 신발
    Necklace,    // 목걸이
    Ring1,       // 반지 1
    Ring2        // 반지 2
}
```

### EquipmentManager.cs

```csharp
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Slots")]
    private Dictionary<EquipmentSlot, InventoryManager.InventoryItem> equippedItems =
        new Dictionary<EquipmentSlot, InventoryManager.InventoryItem>();

    [Header("Stats")]
    public int totalAttackPower = 0;
    public int totalDefense = 0;
    public int totalHealth = 0;
    public float totalCriticalChance = 0f;
    public float totalCriticalDamage = 0f;
    public float totalGoldBonus = 0f;
    public float totalExpBonus = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSlots();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSlots()
    {
        // 모든 슬롯 초기화
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            equippedItems[slot] = null;
        }
    }

    // 장비 착용
    public bool EquipItem(string uniqueId)
    {
        InventoryManager.InventoryItem item = InventoryManager.Instance.items[uniqueId];
        if (item == null)
            return false;

        ItemData itemData = item.itemData;

        // 장비 아이템인지 확인
        if (itemData.equipSlot == EquipmentSlot.None)
        {
            UIManager.Instance.ShowNotification("장착할 수 없는 아이템입니다!");
            return false;
        }

        // 레벨 요구사항 확인
        if (GameManager.Instance.playerLevel < itemData.requiredLevel)
        {
            UIManager.Instance.ShowNotification($"레벨 {itemData.requiredLevel} 이상 필요합니다!");
            return false;
        }

        // 환생 요구사항 확인
        if (RebirthManager.Instance.rebirthCount < itemData.requiredRebirth)
        {
            UIManager.Instance.ShowNotification($"환생 {itemData.requiredRebirth}회 이상 필요합니다!");
            return false;
        }

        // 기존 장비가 있으면 해제
        if (equippedItems[itemData.equipSlot] != null)
        {
            UnequipItem(itemData.equipSlot);
        }

        // 장비 착용
        equippedItems[itemData.equipSlot] = item;

        // 인벤토리에서 제거 (장착 상태 표시)
        // 실제로는 제거하지 않고 플래그만 설정

        // 스탯 재계산
        RecalculateStats();

        // UI 업데이트
        EventManager.TriggerEquipmentChanged();

        UIManager.Instance.ShowNotification($"{itemData.itemName} 장착!");

        return true;
    }

    // 장비 해제
    public bool UnequipItem(EquipmentSlot slot)
    {
        if (equippedItems[slot] == null)
            return false;

        InventoryManager.InventoryItem item = equippedItems[slot];
        equippedItems[slot] = null;

        // 스탯 재계산
        RecalculateStats();

        // UI 업데이트
        EventManager.TriggerEquipmentChanged();

        UIManager.Instance.ShowNotification($"{item.itemData.itemName} 해제!");

        return true;
    }

    // 모든 장비 해제 (환생 시)
    public void UnequipAll()
    {
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (equippedItems[slot] != null)
            {
                UnequipItem(slot);
            }
        }
    }

    // 스탯 재계산
    private void RecalculateStats()
    {
        // 초기화
        totalAttackPower = 0;
        totalDefense = 0;
        totalHealth = 0;
        totalCriticalChance = 0f;
        totalCriticalDamage = 0f;
        totalGoldBonus = 0f;
        totalExpBonus = 0f;

        // 모든 장비의 스탯 합산
        foreach (var item in equippedItems.Values)
        {
            if (item == null)
                continue;

            ItemData itemData = item.itemData;

            // 기본 스탯
            totalAttackPower += itemData.attackPower;
            totalDefense += itemData.defense;
            totalHealth += itemData.health;
            totalCriticalChance += itemData.criticalChance;
            totalCriticalDamage += itemData.criticalDamage;
            totalGoldBonus += itemData.goldBonus;
            totalExpBonus += itemData.expBonus;

            // 강화 레벨에 따른 추가 스탯
            if (item.enhanceLevel > 0)
            {
                float enhanceMultiplier = 1f + (item.enhanceLevel * 0.1f);
                totalAttackPower += (int)(itemData.attackPower * enhanceMultiplier);
                totalDefense += (int)(itemData.defense * enhanceMultiplier);
            }
        }

        // 게임 매니저에 반영
        GameManager.Instance.UpdateEquipmentStats(this);

        Debug.Log($"Stats recalculated - ATK: {totalAttackPower}, DEF: {totalDefense}");
    }

    // 특정 슬롯의 장비 가져오기
    public InventoryManager.InventoryItem GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
    }

    // 세트 효과 확인 (추후 확장)
    public void CheckSetEffects()
    {
        // 같은 세트의 장비를 여러 개 착용 시 추가 보너스
        // 예: 드래곤 세트 3개 착용 시 화염 데미지 +30%
    }
}
```

---

## 🎁 아이템 획득

### 획득 방법

| 방법 | 설명 | 주요 아이템 |
|------|------|-------------|
| 던전 드롭 | 몬스터 처치 시 확률 드롭 | 장비, 재료 |
| 상점 구매 | 골드/젬으로 구매 | 소비품, 장비 |
| 가챠 | 확률형 뽑기 | 고급 장비 |
| 퀘스트 보상 | 미션 완료 시 | 특수 아이템 |
| 업적 보상 | 업적 달성 시 | 희귀 아이템 |
| 일일 출석 | 매일 로그인 | 소비품 |
| 이벤트 | 기간 한정 | 이벤트 전용 |
| 제작/합성 | 재료 조합 | 고급 장비 |

### 드롭 시스템

```csharp
public class DropSystem
{
    public static List<ItemDrop> CalculateDrops(MonsterData monster)
    {
        List<ItemDrop> drops = new List<ItemDrop>();

        // 기본 골드는 항상 드롭
        long gold = Random.Range(monster.goldMin, monster.goldMax + 1);
        drops.Add(new ItemDrop { type = DropType.Gold, amount = gold });

        // 경험치도 항상 획득
        long exp = monster.expReward;
        drops.Add(new ItemDrop { type = DropType.Exp, amount = exp });

        // 아이템 드롭 확률 체크
        if (Random.value < monster.dropChance)
        {
            // 드롭 테이블에서 아이템 선택
            ItemData droppedItem = RollDropTable(monster.dropTable);
            if (droppedItem != null)
            {
                drops.Add(new ItemDrop
                {
                    type = DropType.Item,
                    itemId = droppedItem.itemId,
                    amount = 1
                });
            }
        }

        // 보스 몬스터는 추가 드롭
        if (monster.isBoss)
        {
            // 100% 확률로 희귀 아이템
            ItemData bossItem = RollDropTable(monster.dropTable, guaranteedRare: true);
            if (bossItem != null)
            {
                drops.Add(new ItemDrop
                {
                    type = DropType.Item,
                    itemId = bossItem.itemId,
                    amount = 1
                });
            }
        }

        return drops;
    }

    private static ItemData RollDropTable(DropTable[] dropTable, bool guaranteedRare = false)
    {
        if (dropTable == null || dropTable.Length == 0)
            return null;

        // 드롭 확률 계산
        float totalWeight = 0f;
        foreach (var entry in dropTable)
        {
            totalWeight += entry.dropRate;
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;

        foreach (var entry in dropTable)
        {
            cumulative += entry.dropRate;
            if (roll <= cumulative)
            {
                ItemData item = GameDatabase.Instance.GetItem(entry.itemId);

                // 희귀 보장 체크
                if (guaranteedRare && item.grade < ItemGrade.Rare)
                    continue;

                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                return item;
            }
        }

        return null;
    }
}

public class ItemDrop
{
    public enum DropType { Gold, Exp, Item }
    public DropType type;
    public string itemId;
    public long amount;
}
```

---

## ⚡ 아이템 강화

### 강화 시스템

**강화 레벨**: +0 ~ +20
**성공 확률**: 레벨이 높아질수록 감소
**실패 시**: 강화 레벨 유지 또는 하락

### 강화 확률표

| 강화 레벨 | 성공 확률 | 실패 시 | 비용 (골드) |
|-----------|-----------|---------|-------------|
| +0 → +1 | 100% | - | 1,000 |
| +1 → +2 | 100% | - | 2,000 |
| +2 → +3 | 100% | - | 4,000 |
| +3 → +4 | 90% | 유지 | 8,000 |
| +4 → +5 | 80% | 유지 | 16,000 |
| +5 → +6 | 70% | 유지 | 32,000 |
| +6 → +7 | 60% | 유지 | 64,000 |
| +7 → +8 | 50% | -1 레벨 | 128,000 |
| +8 → +9 | 40% | -1 레벨 | 256,000 |
| +9 → +10 | 30% | -2 레벨 | 512,000 |
| +10 → +11 | 20% | -2 레벨 | 1,024,000 |
| +11 → +12 | 15% | -3 레벨 | 2,048,000 |
| +12 → +13 | 10% | -3 레벨 | 4,096,000 |
| +13 → +14 | 5% | -4 레벨 | 8,192,000 |
| +14 → +15 | 3% | -5 레벨 | 16,384,000 |
| +15 → +20 | 1% | -10 레벨 | 100,000,000 |

### EnhancementManager.cs

```csharp
public class EnhancementManager : MonoBehaviour
{
    public static EnhancementManager Instance;

    [System.Serializable]
    public class EnhancementData
    {
        public int level;
        public float successRate;
        public int failPenalty;     // 실패 시 하락 레벨
        public long cost;
        public string requiredMaterial;
        public int materialCount;
    }

    public EnhancementData[] enhancementTable;

    public bool EnhanceItem(string uniqueId)
    {
        InventoryManager.InventoryItem item = InventoryManager.Instance.items[uniqueId];
        if (item == null)
            return false;

        int currentLevel = item.enhanceLevel;
        int maxLevel = GetMaxEnhanceLevel(item.itemData.grade);

        if (currentLevel >= maxLevel)
        {
            UIManager.Instance.ShowNotification("최대 강화 레벨입니다!");
            return false;
        }

        EnhancementData enhanceData = enhancementTable[currentLevel];

        // 골드 확인
        if (GameManager.Instance.currentGold < enhanceData.cost)
        {
            UIManager.Instance.ShowNotification("골드가 부족합니다!");
            return false;
        }

        // 재료 확인
        if (!string.IsNullOrEmpty(enhanceData.requiredMaterial))
        {
            if (!InventoryManager.Instance.HasItem(enhanceData.requiredMaterial, enhanceData.materialCount))
            {
                UIManager.Instance.ShowNotification("재료가 부족합니다!");
                return false;
            }
        }

        // 골드 차감
        GameManager.Instance.AddGold(-enhanceData.cost);

        // 재료 차감
        if (!string.IsNullOrEmpty(enhanceData.requiredMaterial))
        {
            InventoryManager.Instance.RemoveItem(enhanceData.requiredMaterial, enhanceData.materialCount);
        }

        // 강화 시도
        float roll = Random.value;
        bool success = roll < enhanceData.successRate;

        if (success)
        {
            // 성공!
            item.enhanceLevel++;
            UIManager.Instance.ShowEnhancementResult(true, item);
            PlaySuccessEffect();
        }
        else
        {
            // 실패...
            int penalty = enhanceData.failPenalty;
            item.enhanceLevel = Mathf.Max(0, item.enhanceLevel - penalty);
            UIManager.Instance.ShowEnhancementResult(false, item, penalty);
            PlayFailEffect();
        }

        // 스탯 재계산
        EquipmentManager.Instance.RecalculateStats();

        return success;
    }

    private int GetMaxEnhanceLevel(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.Common: return 5;
            case ItemGrade.Rare: return 10;
            case ItemGrade.Epic: return 12;
            case ItemGrade.Legendary: return 15;
            case ItemGrade.Mythic: return 20;
            default: return 0;
        }
    }

    private void PlaySuccessEffect()
    {
        // 성공 이펙트 재생
        AudioManager.Instance.PlaySFX("enhance_success");
        // 파티클 재생
    }

    private void PlayFailEffect()
    {
        // 실패 이펙트 재생
        AudioManager.Instance.PlaySFX("enhance_fail");
        // 화면 흔들림
    }
}
```

---

## 🔨 아이템 합성

### 합성 시스템

**용도**: 재료를 조합하여 상위 아이템 제작

### 합성 레시피 예시

| 결과물 | 필요 재료 | 성공 확률 |
|--------|-----------|-----------|
| 철 검 | 슬라임 젤리 ×10 + 골드 5,000 | 100% |
| 강철 검 | 늑대 송곳니 ×5 + 철 검 ×1 | 80% |
| 마법 검 | 마법 수정 ×3 + 강철 검 ×1 | 60% |
| 드래곤 검 | 드래곤 비늘 ×10 + 마법 검 ×1 | 40% |

### CraftingManager.cs

```csharp
public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [System.Serializable]
    public class CraftingRecipe
    {
        public string recipeId;
        public string resultItemId;
        public int resultCount;
        public float successRate;
        public long goldCost;
        public List<RecipeMaterial> materials;

        [System.Serializable]
        public class RecipeMaterial
        {
            public string itemId;
            public int count;
        }
    }

    public List<CraftingRecipe> recipes;

    public bool CraftItem(string recipeId)
    {
        CraftingRecipe recipe = recipes.Find(r => r.recipeId == recipeId);
        if (recipe == null)
            return false;

        // 재료 확인
        foreach (var material in recipe.materials)
        {
            if (!InventoryManager.Instance.HasItem(material.itemId, material.count))
            {
                UIManager.Instance.ShowNotification("재료가 부족합니다!");
                return false;
            }
        }

        // 골드 확인
        if (GameManager.Instance.currentGold < recipe.goldCost)
        {
            UIManager.Instance.ShowNotification("골드가 부족합니다!");
            return false;
        }

        // 재료 소모
        foreach (var material in recipe.materials)
        {
            InventoryManager.Instance.RemoveItem(material.itemId, material.count);
        }

        // 골드 소모
        GameManager.Instance.AddGold(-recipe.goldCost);

        // 합성 시도
        float roll = Random.value;
        bool success = roll < recipe.successRate;

        if (success)
        {
            // 성공!
            InventoryManager.Instance.AddItem(recipe.resultItemId, recipe.resultCount);
            ItemData resultItem = GameDatabase.Instance.GetItem(recipe.resultItemId);
            UIManager.Instance.ShowNotification($"{resultItem.itemName} 제작 성공!");
            AudioManager.Instance.PlaySFX("craft_success");
            return true;
        }
        else
        {
            // 실패...
            UIManager.Instance.ShowNotification("제작에 실패했습니다...");
            AudioManager.Instance.PlaySFX("craft_fail");
            return false;
        }
    }
}
```

---

## 📋 체크리스트

### 데이터
- [ ] 아이템 데이터 50개 이상 작성
- [ ] 드롭 테이블 설정
- [ ] 강화 테이블 설정
- [ ] 합성 레시피 설정

### 시스템 구현
- [ ] InventoryManager
- [ ] EquipmentManager
- [ ] EnhancementManager
- [ ] CraftingManager
- [ ] DropSystem

### UI 구현
- [ ] 인벤토리 UI (그리드)
- [ ] 장비 UI (슬롯)
- [ ] 강화 UI
- [ ] 합성 UI
- [ ] 아이템 툴팁

### 기능
- [ ] 아이템 정렬/필터
- [ ] 아이템 사용
- [ ] 아이템 판매
- [ ] 장비 착용/해제
- [ ] 아이템 강화
- [ ] 아이템 합성

### 테스트
- [ ] 아이템 획득 테스트
- [ ] 인벤토리 가득 참 테스트
- [ ] 장비 착용 테스트
- [ ] 강화 시스템 테스트
- [ ] 합성 시스템 테스트

---

**작성 완료일**: 2025-11-13
**예상 개발 시간**: 10일
**담당**: 시스템 프로그래머, UI 프로그래머
