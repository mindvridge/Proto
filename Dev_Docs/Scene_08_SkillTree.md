# Scene 08: 스킬 트리 - 개발 명세서

## 📋 씬 개요
**씬 이름**: SkillTreeScene
**씬 목적**: 스킬 업그레이드, 능력 강화
**개발 시간**: 6일

## 🎨 UI 구성
```
┌───────────────────────────────────┐
│ 스킬 포인트: 15  [초기화 ₩5,500]│
├───────────────────────────────────┤
│        [은둔] [전투] [성장]       │
├───────────────────────────────────┤
│         ┌─[Lv.5]─┐               │
│         │기척소거 │               │
│         └────┬────┘               │
│              │                    │
│         ┌────┴────┐               │
│    ┌─[Lv.3]─┐ ┌─[Lv.0]─┐         │
│    │그림자  │ │유령모드│         │
│    │ 이동   │ │(잠금)  │         │
│    └────────┘ └────────┘         │
└───────────────────────────────────┘
```

## 🔧 핵심 시스템

### SkillTreeManager
```csharp
[System.Serializable]
public class SkillNode
{
    public string skillId;
    public string skillName;
    public int currentLevel;
    public int maxLevel;
    public int pointCost;
    public string[] prerequisites; // 필수 선행 스킬
    public SkillEffect effect;
}

public class SkillTreeManager : MonoBehaviour
{
    public int availableSkillPoints = 0;
    public List<SkillNode> stealthSkills;
    public List<SkillNode> combatSkills;
    public List<SkillNode> growthSkills;

    public bool CanUpgradeSkill(SkillNode skill)
    {
        // 포인트 체크
        if (availableSkillPoints < skill.pointCost)
        {
            ShowMessage("스킬 포인트가 부족합니다!");
            return false;
        }

        // 최대 레벨 체크
        if (skill.currentLevel >= skill.maxLevel)
        {
            ShowMessage("최대 레벨입니다!");
            return false;
        }

        // 선행 스킬 체크
        foreach (string prereq in skill.prerequisites)
        {
            SkillNode prereqSkill = FindSkill(prereq);
            if (prereqSkill.currentLevel <= 0)
            {
                ShowMessage($"{prereqSkill.skillName}을(를) 먼저 배워야 합니다!");
                return false;
            }
        }

        return true;
    }

    public void UpgradeSkill(SkillNode skill)
    {
        if (!CanUpgradeSkill(skill)) return;

        availableSkillPoints -= skill.pointCost;
        skill.currentLevel++;

        ApplySkillEffect(skill);
        UpdateUI();

        // 업적 체크
        AchievementManager.Instance.CheckSkillMastery();
    }

    private void ApplySkillEffect(SkillNode skill)
    {
        switch (skill.effect)
        {
            case SkillEffect.ClickPowerBoost:
                GameManager.Instance.clickPowerMultiplier += 0.1f * skill.currentLevel;
                break;
            case SkillEffect.SuspicionReduction:
                SuspicionManager.Instance.suspicionGainMultiplier -= 0.05f * skill.currentLevel;
                break;
            // ... 기타 효과
        }
    }

    public void ResetSkillTree()
    {
        // IAP 또는 골드 소모
        if (!PayForReset()) return;

        // 모든 스킬 초기화
        foreach (var skill in stealthSkills)
            skill.currentLevel = 0;
        foreach (var skill in combatSkills)
            skill.currentLevel = 0;
        foreach (var skill in growthSkills)
            skill.currentLevel = 0;

        // 포인트 환불
        RecalculateSkillPoints();
        UpdateUI();
    }

    private bool PayForReset()
    {
        // 예: ₩5,500 결제 또는 100만 골드
        return IAPManager.Instance.PurchaseSkillReset();
    }
}
```

### SkillNodeUI
```csharp
public class SkillNodeUI : MonoBehaviour
{
    public SkillNode skillData;
    public Image skillIcon;
    public Text levelText;
    public GameObject lockedOverlay;
    public Button upgradeButton;

    public void SetData(SkillNode skill)
    {
        skillData = skill;

        levelText.text = $"Lv.{skill.currentLevel}/{skill.maxLevel}";

        // 잠금 상태
        bool isLocked = !CheckPrerequisites(skill);
        lockedOverlay.SetActive(isLocked);
        upgradeButton.interactable = !isLocked && skill.currentLevel < skill.maxLevel;
    }

    public void OnUpgradeClicked()
    {
        SkillTreeManager.Instance.UpgradeSkill(skillData);
    }

    private bool CheckPrerequisites(SkillNode skill)
    {
        foreach (string prereq in skill.prerequisites)
        {
            SkillNode prereqSkill = SkillTreeManager.Instance.FindSkill(prereq);
            if (prereqSkill.currentLevel <= 0)
                return false;
        }
        return true;
    }
}
```

## 📋 스킬 카테고리

### 은둔 스킬
```
- 기척 소거 (Lv.1~10): 의심도 증가율 -5%씩
- 그림자 이동 (Lv.1~5): 던전 이동 시간 감소
- 유령 모드 (Lv.1~3): 목격 위험 -10%씩
- 변장의 달인 (Lv.1~5): 정체 숨기기 효율 증가
```

### 전투 스킬
```
- 강화 타격 (Lv.1~10): 공격력 +10%씩
- 치명타 숙련 (Lv.1~5): 치명타 확률 +2%씩
- 마나 회복 (Lv.1~5): MP 재생 +10%씩
- 연속 공격 (Lv.1~3): 공격 속도 증가
```

### 성장 스킬
```
- 경험치 부스트 (Lv.1~10): 경험치 획득 +5%씩
- 골드 수집 (Lv.1~10): 골드 획득 +5%씩
- 방치 효율 (Lv.1~5): 오프라인 수익 +10%씩
- 빠른 성장 (Lv.1~5): 레벨업 속도 증가
```

## 📋 체크리스트
- [ ] 스킬 트리 데이터 구조
- [ ] 스킬 노드 UI
- [ ] 업그레이드 시스템
- [ ] 선행 스킬 체크
- [ ] 스킬 효과 적용
- [ ] 스킬 초기화 기능
- [ ] 3개 카테고리 구현

**개발 시간**: 6일
