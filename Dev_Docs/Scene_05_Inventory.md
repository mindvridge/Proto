# Scene 05: 인벤토리 & 장비 - 개발 명세서

## 📋 씬 개요
**씬 이름**: InventoryScene
**씬 목적**: 아이템 관리, 장비 착용
**개발 시간**: 6일

## 🎨 UI 구성
```
┌───────────────────────────────────┐
│ [전체] [무기] [방어구] [소비]    │
├─────────────┬─────────────────────┤
│ ┌─┬─┬─┬─┐  │  📋 아이템 상세      │
│ │⚔│🛡│💎│ │  │                     │
│ ├─┼─┼─┼─┤  │  전설의 검          │
│ │ │ │ │ │  │  공격력: +500       │
│ ├─┼─┼─┼─┤  │  크리티컬: +10%     │
│ │ │ │ │ │  │                     │
│ └─┴─┴─┴─┘  │  [장착]  [판매]     │
├─────────────┴─────────────────────┤
│ 소지: 45/100   골드: 1,234,567    │
└───────────────────────────────────┘
```

## 🔧 핵심 시스템

### InventoryManager
```csharp
public class InventoryManager : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int maxSlots = 100;

    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            ShowMessage("인벤토리가 가득 찼습니다!");
            return false;
        }

        items.Add(item);
        UpdateUI();
        return true;
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
        UpdateUI();
    }

    public void EquipItem(Equipment equipment)
    {
        EquipmentManager.Instance.Equip(equipment);
        UpdateStats();
    }

    public void SellItem(Item item)
    {
        long sellPrice = (long)(item.price * 0.5f);
        GameManager.Instance.AddGold(sellPrice);
        RemoveItem(item);
    }
}

[System.Serializable]
public class Item
{
    public string itemId;
    public string itemName;
    public ItemType type;
    public int rarity; // 1-5
    public long price;
    public Sprite icon;
}

[System.Serializable]
public class Equipment : Item
{
    public int attackBonus;
    public int defenseBonus;
    public float criticalChanceBonus;
    // ... 기타 스탯
}
```

### Equipment System
```csharp
public class EquipmentManager : MonoBehaviour
{
    public Equipment weapon;
    public Equipment helmet;
    public Equipment armor;
    public Equipment gloves;
    public Equipment boots;

    public void Equip(Equipment equipment)
    {
        switch (equipment.type)
        {
            case ItemType.Weapon:
                if (weapon != null)
                    InventoryManager.Instance.AddItem(weapon);
                weapon = equipment;
                break;
            case ItemType.Armor:
                if (armor != null)
                    InventoryManager.Instance.AddItem(armor);
                armor = equipment;
                break;
            // ... 기타 슬롯
        }

        InventoryManager.Instance.RemoveItem(equipment);
        UpdatePlayerStats();
    }

    private void UpdatePlayerStats()
    {
        int totalAttack = 0;
        int totalDefense = 0;

        if (weapon != null) totalAttack += weapon.attackBonus;
        if (armor != null) totalDefense += armor.defenseBonus;
        // ... 기타 장비

        GameManager.Instance.attackPower = totalAttack;
        GameManager.Instance.defensePower = totalDefense;
    }
}
```

## 📋 체크리스트
- [ ] 인벤토리 그리드 UI
- [ ] 아이템 필터
- [ ] 아이템 상세 정보
- [ ] 장비 착용 시스템
- [ ] 스탯 업데이트
- [ ] 아이템 판매
- [ ] 정렬 기능

**개발 시간**: 6일
