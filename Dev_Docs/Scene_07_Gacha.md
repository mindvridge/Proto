# Scene 07: 가챠 - 개발 명세서

## 📋 씬 개요
**씬 이름**: GachaScene
**씬 목적**: 아이템 랜덤 획득
**개발 시간**: 7일

## 🎨 UI 구성
```
┌───────────────────────────────────┐
│        🎁 가챠 상자               │
│       (회전 애니메이션)           │
│                                   │
│  프레스티지 포인트: 500 PP        │
│                                   │
│   [1회 뽑기]     [10회 뽑기]      │
│    300 PP         3,000 PP        │
│                                   │
│  천장까지: 45/100                 │
└───────────────────────────────────┘
```

## 🔧 핵심 시스템

### GachaManager
```csharp
public class GachaManager : MonoBehaviour
{
    [Header("Probabilities")]
    public float commonRate = 0.60f;     // 60%
    public float rareRate = 0.30f;       // 30%
    public float epicRate = 0.09f;       // 9%
    public float legendaryRate = 0.009f; // 0.9%
    public float mythicRate = 0.001f;    // 0.1%

    [Header("Costs")]
    public int singlePullCost = 300;
    public int tenPullCost = 3000;

    [Header("Pity System")]
    public int pityCounter = 0;
    public int pityThreshold = 100;

    public void SinglePull()
    {
        if (!CanPull(singlePullCost)) return;

        GameManager.Instance.prestigePoints -= singlePullCost;
        pityCounter++;

        Item reward = RollGacha();
        ShowRewardAnimation(reward);
    }

    public void TenPull()
    {
        if (!CanPull(tenPullCost)) return;

        GameManager.Instance.prestigePoints -= tenPullCost;

        List<Item> rewards = new List<Item>();
        for (int i = 0; i < 10; i++)
        {
            pityCounter++;
            rewards.Add(RollGacha());
        }

        ShowMultiRewardAnimation(rewards);
    }

    private Item RollGacha()
    {
        // 천장 시스템
        if (pityCounter >= pityThreshold)
        {
            pityCounter = 0;
            return GetGuaranteedLegendary();
        }

        float roll = Random.value;
        float cumulative = 0f;

        cumulative += mythicRate;
        if (roll < cumulative) return GetRandomItem(Rarity.Mythic);

        cumulative += legendaryRate;
        if (roll < cumulative) return GetRandomItem(Rarity.Legendary);

        cumulative += epicRate;
        if (roll < cumulative) return GetRandomItem(Rarity.Epic);

        cumulative += rareRate;
        if (roll < cumulative) return GetRandomItem(Rarity.Rare);

        return GetRandomItem(Rarity.Common);
    }

    private bool CanPull(int cost)
    {
        if (GameManager.Instance.prestigePoints < cost)
        {
            ShowMessage("프레스티지 포인트가 부족합니다!");
            ShowPPShop();
            return false;
        }
        return true;
    }
}
```

### GachaAnimation
```csharp
public class GachaAnimation : MonoBehaviour
{
    public GameObject gachaBox;
    public ParticleSystem[] rarityParticles;

    public IEnumerator PlayGachaAnimation(Item reward)
    {
        // 상자 회전
        float rotationTime = 2.0f;
        float elapsed = 0f;

        while (elapsed < rotationTime)
        {
            gachaBox.transform.Rotate(0, 360 * Time.deltaTime, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 폭발 효과
        PlayParticleByRarity(reward.rarity);

        // 아이템 표시
        yield return new WaitForSeconds(0.5f);
        ShowRewardCard(reward);
    }

    private void PlayParticleByRarity(int rarity)
    {
        if (rarity >= 0 && rarity < rarityParticles.Length)
        {
            rarityParticles[rarity].Play();
        }
    }
}
```

## 📋 체크리스트
- [ ] 가챠 확률 시스템
- [ ] 천장 시스템
- [ ] 단일/10연차 구매
- [ ] 보상 애니메이션
- [ ] 확률 표시 (법적 요구사항)
- [ ] 가챠 히스토리
- [ ] PP 부족 시 상점 연결

**개발 시간**: 7일
