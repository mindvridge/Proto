# Scene 06: 상점 - 개발 명세서

## 📋 씬 개요
**씬 이름**: ShopScene
**씬 목적**: 아이템 구매, IAP 결제
**개발 시간**: 8일

## 🎨 UI 구성
```
┌───────────────────────────────────┐
│ [일반상점] [암시장] [프리미엄]    │
├───────────────────────────────────┤
│ ┌──────────┐  ┌──────────┐       │
│ │  ⚔️검    │  │ 🛡️갑옷   │       │
│ │ 10,000G  │  │ 15,000G  │       │
│ │  [구매]  │  │  [구매]  │       │
│ └──────────┘  └──────────┘       │
├───────────────────────────────────┤
│ 보유 골드: 1,234,567              │
└───────────────────────────────────┘
```

## 🔧 핵심 시스템

### ShopManager
```csharp
public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public Item item;
        public long price;
        public bool isLimited;
        public int stock;
    }

    public List<ShopItem> normalShopItems;
    public List<ShopItem> blackMarketItems;
    public List<ShopItem> premiumItems;

    public void BuyItem(ShopItem shopItem)
    {
        // 골드 체크
        if (GameManager.Instance.currentGold < shopItem.price)
        {
            ShowMessage("골드가 부족합니다!");
            return;
        }

        // 재고 체크
        if (shopItem.isLimited && shopItem.stock <= 0)
        {
            ShowMessage("품절입니다!");
            return;
        }

        // 구매
        GameManager.Instance.currentGold -= shopItem.price;
        InventoryManager.Instance.AddItem(shopItem.item);

        if (shopItem.isLimited)
        {
            shopItem.stock--;
        }

        // 의심도 증가 (비싼 아이템)
        if (shopItem.price > 100000)
        {
            SuspicionManager.Instance.AddSuspicion(5f, "Expensive purchase");
        }

        UpdateUI();
    }
}
```

### IAP (In-App Purchase)
```csharp
public class IAPManager : MonoBehaviour
{
    public void BuyGoldPackage(string productId)
    {
#if UNITY_PURCHASING
        // Unity IAP 사용
        IAPButton.Instance.OnPurchaseComplete += OnPurchaseSuccess;
        IAPButton.Instance.OnPurchaseFailed += OnPurchaseFailed;
#endif
    }

    private void OnPurchaseSuccess(Product product)
    {
        switch (product.definition.id)
        {
            case "gold_small":
                GameManager.Instance.AddGold(100000);
                break;
            case "gold_medium":
                GameManager.Instance.AddGold(600000);
                break;
            // ... 기타 상품
        }
    }
}
```

### 광고 보상
```csharp
public class AdRewardShop : MonoBehaviour
{
    public void WatchAdForItem(Item rewardItem)
    {
        AdManager.Instance.ShowRewardedAd(() =>
        {
            InventoryManager.Instance.AddItem(rewardItem);
            ShowMessage($"{rewardItem.itemName} 획득!");
        });
    }
}
```

## 📋 체크리스트
- [ ] 상점 카테고리 (일반/암시장/프리미엄)
- [ ] 아이템 그리드
- [ ] 구매 시스템
- [ ] IAP 연동
- [ ] 광고 보상
- [ ] 한정 상품 타이머
- [ ] 재고 시스템

**개발 시간**: 8일
