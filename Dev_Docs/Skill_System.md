# 스킬 시스템 명세서

**프로젝트**: 숨겨진 성장의 백수 영웅
**작성일**: 2025-11-13
**버전**: 1.0

---

## 📋 목차

1. [시스템 개요](#시스템-개요)
2. [스킬 카테고리](#스킬-카테고리)
3. [스킬 트리 구조](#스킬-트리-구조)
4. [스킬 포인트](#스킬-포인트)
5. [스킬 배우기](#스킬-배우기)
6. [스킬 효과](#스킬-효과)
7. [스킬 리셋](#스킬-리셋)
8. [특수 스킬](#특수-스킬)
9. [코드 구현](#코드-구현)
10. [밸런싱](#밸런싱)

---

## 🎯 시스템 개요

### 핵심 개념

**스킬 트리**: 플레이어가 성장 방향을 선택할 수 있는 능력 시스템
**3개 카테고리**: 은둔 (Stealth), 전투 (Combat), 성장 (Growth)
**선행 스킬**: 특정 스킬을 배우려면 이전 스킬 필요
**스킬 레벨**: 각 스킬은 1~10레벨까지 강화 가능

### 특징

- ✅ **자유로운 선택**: 3개 카테고리 중 원하는 방향 선택
- ✅ **순차적 성장**: 선행 스킬을 먼저 배워야 함
- ✅ **레벨별 강화**: 스킬 레벨을 올려 효과 증가
- ✅ **리셋 가능**: 스킬 포인트를 환불받고 재분배
- ✅ **환생 연동**: 환생 시 스킬은 유지 (코어 스킬만)

---

## 🌳 스킬 카테고리

### 1. 은둔 (Stealth) - 녹색

**테마**: 의심도 관리, 은밀한 활동
**색상**: 녹색 (#00FF88)
**아이콘**: 후드, 그림자

**핵심 스킬**:
- 의심도 감소
- 밤 시간대 보너스
- 무인 던전 효율 증가
- 자동 골드 수집

**플레이 스타일**:
- 조용히 강해지는 은둔형 플레이
- 가족에게 들키지 않고 성장
- 안정적이고 리스크 없는 플레이

---

### 2. 전투 (Combat) - 빨강

**테마**: 공격력, 전투 효율
**색상**: 빨강 (#FF4444)
**아이콘**: 검, 방패

**핵심 스킬**:
- 클릭 파워 증가
- 치명타 확률/데미지
- 자동 전투
- 방어력 증가

**플레이 스타일**:
- 강력한 딜러 플레이
- 빠른 던전 클리어
- 보스전 특화

---

### 3. 성장 (Growth) - 파랑

**테마**: 골드, 경험치, 효율
**색상**: 파랑 (#4488FF)
**아이콘**: 동전, 화살표

**핵심 스킬**:
- 골드 획득 증가
- 경험치 획득 증가
- 방치 수익 증가
- 오프라인 보너스

**플레이 스타일**:
- 효율적인 파밍
- 빠른 레벨업
- 방치형 플레이

---

## 🌲 스킬 트리 구조

### 은둔 (Stealth) 스킬 트리

```
                 [평범한 척]
                 Lv 1-10
                 의심도 -5%~23%
                      |
        ┌─────────────┴─────────────┐
        ▼                           ▼
    [은밀한 행동]              [야간 활동]
    Lv 10 필요                Lv 5 필요
    의심도 -10%~37%           밤 드롭 +10%~55%
        |                           |
        ▼                           ▼
    [완벽한 위장]              [무인 던전 전문가]
    Lv 25 필요                Lv 15 필요
    의심도 -15%~35%           무인 던전 보상 +20%~40%
        |                           |
        ▼                           ▼
    [그림자 은신]              [시간 왜곡]
    Lv 40 필요                Lv 30 필요
    의심도 증가율 -50%        시간 보너스 2배
        |                           |
        └─────────────┬─────────────┘
                      ▼
                [은둔의 대가]
                Lv 50 필요
                골드 자동 수집 해금
```

### 전투 (Combat) 스킬 트리

```
                 [기본 무술]
                 Lv 1-10
                 클릭 파워 +10%~55%
                      |
        ┌─────────────┴─────────────┐
        ▼                           ▼
    [강화된 주먹]              [치명타]
    Lv 5 필요                 Lv 5 필요
    클릭 파워 +20%~110%       치명타율 +5%~23%
        |                           |
        ▼                           ▼
    [연속 타격]                [치명타 강화]
    Lv 15 필요                Lv 15 필요
    클릭 시 3회 연타          치명타 데미지 +50%~275%
        |                           |
        ▼                           ▼
    [분노]                     [급소 공격]
    Lv 30 필요                Lv 30 필요
    체력 50% 이하 시 데미지 2배  치명타 시 골드 2배
        |                           |
        └─────────────┬─────────────┘
                      ▼
                [자동 전투]
                Lv 50 필요
                자동 전투 해금 + 효율 증가
```

### 성장 (Growth) 스킬 트리

```
                 [효율적 학습]
                 Lv 1-10
                 경험치 +10%~55%
                      |
        ┌─────────────┴─────────────┐
        ▼                           ▼
    [황금 손가락]              [방치 수익]
    Lv 1-10                   Lv 5 필요
    골드 +10%~55%             방치 골드 +20%~110%
        |                           |
        ▼                           ▼
    [골드 자석]                [오프라인 수익]
    Lv 15 필요                Lv 15 필요
    골드 자동 획득 범위 증가   오프라인 +25%~85%
        |                           |
        ▼                           ▼
    [황금비]                   [시간 압축]
    Lv 30 필요                Lv 30 필요
    골드 획득 시 10% 추가     방치 속도 2배
        |                           |
        └─────────────┬─────────────┘
                      ▼
                [무한 성장]
                Lv 50 필요
                레벨당 모든 보너스 +0.5%
```

---

## 💎 스킬 포인트

### 획득 방법

| 방법 | 획득량 | 조건 |
|------|--------|------|
| 레벨업 | 1 | 모든 레벨업 |
| 마일스톤 | 2-5 | 레벨 10, 20, 50, 100 |
| 퀘스트 | 1-3 | 특정 퀘스트 완료 |
| 업적 | 1-5 | 업적 달성 |
| 환생 | 5 | 환생 시 보너스 |
| 일일 보상 | 1 | 7일 출석 |
| 이벤트 | 1-10 | 기간 한정 이벤트 |
| 상점 구매 | 1 | 젬 100개 |

### 예상 스킬 포인트 (레벨별)

| 레벨 | 기본 포인트 | 마일스톤 | 총 포인트 |
|------|-------------|----------|-----------|
| 10 | 10 | +2 | 12 |
| 20 | 20 | +5 | 25 |
| 30 | 30 | +0 | 30 |
| 50 | 50 | +10 | 60 |
| 100 | 100 | +20 | 120 |
| 200 | 200 | +50 | 250 |

### 스킬 포인트 관리

```csharp
public class SkillPointManager
{
    public int totalEarnedPoints = 0;   // 총 획득 포인트
    public int usedPoints = 0;          // 사용한 포인트
    public int availablePoints = 0;     // 사용 가능 포인트

    public void EarnSkillPoint(int amount, string source)
    {
        totalEarnedPoints += amount;
        availablePoints += amount;

        UIManager.Instance.ShowNotification($"스킬 포인트 +{amount}!");
        EventManager.TriggerSkillPointChanged(availablePoints);

        // 업적 체크
        AchievementManager.Instance.CheckSkillPointAchievements(totalEarnedPoints);
    }

    public bool SpendSkillPoint(int amount)
    {
        if (availablePoints < amount)
            return false;

        availablePoints -= amount;
        usedPoints += amount;

        EventManager.TriggerSkillPointChanged(availablePoints);
        return true;
    }

    public void RefundSkillPoints(int amount)
    {
        availablePoints += amount;
        usedPoints -= amount;

        EventManager.TriggerSkillPointChanged(availablePoints);
    }
}
```

---

## 📖 스킬 배우기

### 배우기 조건

1. **스킬 포인트**: 충분한 포인트 보유
2. **선행 스킬**: 필수 선행 스킬 습득
3. **플레이어 레벨**: 최소 레벨 달성
4. **환생 횟수**: 특정 스킬은 환생 필요

### 스킬 습득 과정

```
1. 스킬 트리 열기
   ↓
2. 스킬 선택 (탭)
   ↓
3. 스킬 정보 확인
   - 현재 효과
   - 다음 레벨 효과
   - 필요 스킬 포인트
   - 선행 스킬 충족 여부
   ↓
4. [배우기] 버튼 탭
   ↓
5. 스킬 획득 애니메이션
   ↓
6. 스킬 효과 즉시 적용
```

### 스킬 레벨업

각 스킬은 1~10레벨까지 강화 가능합니다.

| 레벨 | 필요 포인트 | 누적 포인트 |
|------|-------------|-------------|
| 1 | 1 | 1 |
| 2 | 1 | 2 |
| 3 | 2 | 4 |
| 4 | 2 | 6 |
| 5 | 3 | 9 |
| 6 | 3 | 12 |
| 7 | 4 | 16 |
| 8 | 4 | 20 |
| 9 | 5 | 25 |
| 10 | 5 | 30 |

**총합**: 1개 스킬을 최대 레벨로 올리려면 30 포인트 필요

---

## ⚡ 스킬 효과

### 효과 타입

```csharp
public enum SkillEffectType
{
    // 전투
    IncreaseClickPower,      // 클릭 파워 증가
    IncreaseCriticalChance,  // 치명타 확률
    IncreaseCriticalDamage,  // 치명타 데미지
    IncreaseDamage,          // 전체 데미지
    IncreaseDefense,         // 방어력

    // 성장
    IncreaseGoldGain,        // 골드 획득
    IncreaseExpGain,         // 경험치 획득
    IncreaseIdleGain,        // 방치 수익
    IncreaseOfflineGain,     // 오프라인 수익

    // 은둔
    ReduceSuspicionGain,     // 의심도 감소
    IncreaseNightBonus,      // 밤 보너스
    IncreaseSafeZoneReward,  // 안전지대 보상

    // 특수
    AutoCollectGold,         // 자동 수집
    AutoBattle,              // 자동 전투
    UnlockFeature            // 기능 해금
}
```

### 패시브 스킬 vs 액티브 스킬

#### 패시브 스킬 (Passive)
- 배우면 **자동으로 항상 적용**
- 대부분의 스킬이 패시브
- 예: 클릭 파워 +10%, 골드 획득 +20%

#### 액티브 스킬 (Active)
- 사용자가 **직접 활성화** 필요
- 쿨타임 존재
- 예: 자동 전투, 골드 부스트

---

## 🔄 스킬 리셋

### 리셋 방법

1. **스킬 초기화 주문서** (아이템)
   - 상점에서 구매: 10,000 골드
   - 던전 드롭
   - 100% 환불

2. **젬으로 리셋**
   - 1회: 100 젬
   - 2회: 200 젬
   - 3회+: 300 젬
   - 100% 환불

3. **환생 시 자동 리셋**
   - 코어 스킬만 유지
   - 나머지는 자동 환불

### 리셋 시스템

```csharp
public class SkillResetSystem
{
    public int resetCount = 0;

    public bool ResetSkills(bool keepCoreSkills = false)
    {
        int totalRefund = 0;

        foreach (var skill in SkillTreeManager.Instance.learnedSkills)
        {
            // 코어 스킬 유지 옵션
            if (keepCoreSkills && skill.isCoreSkill)
                continue;

            // 사용한 포인트 계산
            totalRefund += skill.totalSpentPoints;

            // 스킬 레벨 초기화
            skill.currentLevel = 0;
        }

        // 스킬 포인트 환불
        SkillPointManager.Instance.RefundSkillPoints(totalRefund);

        // 리셋 횟수 증가
        resetCount++;

        // 스탯 재계산
        RecalculateAllSkillEffects();

        UIManager.Instance.ShowNotification($"스킬이 초기화되었습니다! {totalRefund} 포인트 환불");

        return true;
    }

    public long GetResetCost()
    {
        // 리셋 비용 (골드)
        return 10000 * (1 + resetCount);
    }

    public int GetResetGemCost()
    {
        // 리셋 비용 (젬)
        if (resetCount == 0) return 100;
        if (resetCount == 1) return 200;
        return 300;
    }
}
```

---

## 🌟 특수 스킬

### 해금 스킬 (Unlock Skills)

특정 기능을 해금하는 스킬입니다.

| 스킬 이름 | 해금 기능 | 필요 레벨 | 포인트 |
|-----------|-----------|-----------|--------|
| 자동 전투 | 전투 자동화 | 20 | 10 |
| 골드 자동 수집 | 골드 자동 픽업 | 30 | 20 |
| 빠른 이동 | 던전 즉시 이동 | 25 | 15 |
| 스킬 연계 | 2개 스킬 동시 발동 | 40 | 25 |

### 조건부 스킬 (Conditional Skills)

특정 조건에서만 발동하는 스킬입니다.

| 스킬 이름 | 조건 | 효과 |
|-----------|------|------|
| 분노 | HP 50% 이하 | 데미지 2배 |
| 위기 극복 | 의심도 80% 이상 | 골드 +100% |
| 야행성 | 밤 시간대 (22:00-04:00) | 모든 능력치 +50% |
| 고독한 전사 | 혼자 플레이 시 | 클릭 파워 +30% |

### 시너지 스킬 (Synergy Skills)

여러 스킬을 함께 배우면 추가 효과가 발동합니다.

| 조합 | 시너지 효과 |
|------|-------------|
| 은둔 5개 + 성장 5개 | 의심도 없이 골드 +50% |
| 전투 10개 | 치명타 확률 +10% 추가 |
| 모든 카테고리 5개씩 | 전체 능력치 +20% |

---

## 💻 코드 구현

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

    [Header("Category")]
    public SkillCategory category;

    [Header("Level Info")]
    public int maxLevel = 10;
    public int[] skillPointCost;  // 레벨당 필요 포인트

    [Header("Prerequisites")]
    public string[] prerequisiteSkills;
    public int requiredPlayerLevel;
    public int requiredRebirthCount;

    [Header("Effects")]
    public SkillEffectType effectType;
    public float baseValue;
    public float valuePerLevel;
    public bool isPassive = true;
    public float cooldown = 0f;      // 액티브 스킬 쿨타임
    public float duration = 0f;      // 버프 지속 시간

    [Header("Special")]
    public bool isCoreSkill = false;  // 환생 시 유지 여부
    public bool isUnlockSkill = false; // 기능 해금 스킬

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
    Stealth,    // 은둔
    Combat,     // 전투
    Growth      // 성장
}
```

### SkillTreeManager.cs

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public static SkillTreeManager Instance;

    [System.Serializable]
    public class LearnedSkill
    {
        public string skillId;
        public SkillData skillData;
        public int currentLevel;
        public int totalSpentPoints;

        public LearnedSkill(string id, SkillData data)
        {
            skillId = id;
            skillData = data;
            currentLevel = 0;
            totalSpentPoints = 0;
        }
    }

    [Header("Learned Skills")]
    public Dictionary<string, LearnedSkill> learnedSkills = new Dictionary<string, LearnedSkill>();

    [Header("Skill Points")]
    public int availableSkillPoints = 0;
    public int totalEarnedPoints = 0;
    public int usedPoints = 0;

    [Header("Skill Effects")]
    public float totalClickPowerBonus = 0f;
    public float totalGoldGainBonus = 0f;
    public float totalExpGainBonus = 0f;
    public float totalCriticalChanceBonus = 0f;
    public float totalCriticalDamageBonus = 0f;
    public float totalSuspicionReduction = 0f;
    public float totalIdleGainBonus = 0f;
    public float totalOfflineGainBonus = 0f;

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

    // 스킬 배우기
    public bool LearnSkill(string skillId)
    {
        SkillData skillData = GameDatabase.Instance.GetSkill(skillId);
        if (skillData == null)
        {
            Debug.LogError($"Skill not found: {skillId}");
            return false;
        }

        // 이미 배운 스킬인지 확인
        if (learnedSkills.ContainsKey(skillId))
        {
            // 레벨업
            return LevelUpSkill(skillId);
        }

        // 조건 체크
        if (!CanLearnSkill(skillData))
            return false;

        // 스킬 포인트 체크
        int cost = skillData.skillPointCost[0];
        if (availableSkillPoints < cost)
        {
            UIManager.Instance.ShowNotification("스킬 포인트가 부족합니다!");
            return false;
        }

        // 스킬 배우기
        LearnedSkill newSkill = new LearnedSkill(skillId, skillData);
        newSkill.currentLevel = 1;
        newSkill.totalSpentPoints = cost;

        learnedSkills[skillId] = newSkill;

        // 스킬 포인트 소모
        availableSkillPoints -= cost;
        usedPoints += cost;

        // 효과 재계산
        RecalculateSkillEffects();

        // 이벤트 발생
        EventManager.TriggerSkillLearned(skillId);

        // UI 업데이트
        UIManager.Instance.ShowNotification($"{skillData.skillName} 습득!");
        PlaySkillLearnEffect();

        // 업적 체크
        AchievementManager.Instance.CheckSkillAchievements(learnedSkills.Count);

        return true;
    }

    // 스킬 레벨업
    public bool LevelUpSkill(string skillId)
    {
        if (!learnedSkills.ContainsKey(skillId))
            return false;

        LearnedSkill skill = learnedSkills[skillId];
        SkillData skillData = skill.skillData;

        // 최대 레벨 체크
        if (skill.currentLevel >= skillData.maxLevel)
        {
            UIManager.Instance.ShowNotification("최대 레벨입니다!");
            return false;
        }

        // 스킬 포인트 체크
        int cost = skillData.skillPointCost[skill.currentLevel];
        if (availableSkillPoints < cost)
        {
            UIManager.Instance.ShowNotification("스킬 포인트가 부족합니다!");
            return false;
        }

        // 레벨업
        skill.currentLevel++;
        skill.totalSpentPoints += cost;

        // 스킬 포인트 소모
        availableSkillPoints -= cost;
        usedPoints += cost;

        // 효과 재계산
        RecalculateSkillEffects();

        // UI 업데이트
        UIManager.Instance.ShowNotification($"{skillData.skillName} Lv.{skill.currentLevel}!");

        return true;
    }

    // 스킬 배울 수 있는지 확인
    private bool CanLearnSkill(SkillData skillData)
    {
        // 레벨 요구사항
        if (GameManager.Instance.playerLevel < skillData.requiredPlayerLevel)
        {
            UIManager.Instance.ShowNotification($"레벨 {skillData.requiredPlayerLevel} 이상 필요합니다!");
            return false;
        }

        // 환생 요구사항
        if (RebirthManager.Instance.rebirthCount < skillData.requiredRebirthCount)
        {
            UIManager.Instance.ShowNotification($"환생 {skillData.requiredRebirthCount}회 이상 필요합니다!");
            return false;
        }

        // 선행 스킬 체크
        foreach (string prereqId in skillData.prerequisiteSkills)
        {
            if (!learnedSkills.ContainsKey(prereqId))
            {
                SkillData prereqSkill = GameDatabase.Instance.GetSkill(prereqId);
                UIManager.Instance.ShowNotification($"{prereqSkill.skillName}을(를) 먼저 배워야 합니다!");
                return false;
            }
        }

        return true;
    }

    // 스킬 효과 재계산
    private void RecalculateSkillEffects()
    {
        // 초기화
        totalClickPowerBonus = 0f;
        totalGoldGainBonus = 0f;
        totalExpGainBonus = 0f;
        totalCriticalChanceBonus = 0f;
        totalCriticalDamageBonus = 0f;
        totalSuspicionReduction = 0f;
        totalIdleGainBonus = 0f;
        totalOfflineGainBonus = 0f;

        // 모든 배운 스킬의 효과 합산
        foreach (var skill in learnedSkills.Values)
        {
            SkillData skillData = skill.skillData;
            float effectValue = skillData.GetValueAtLevel(skill.currentLevel);

            switch (skillData.effectType)
            {
                case SkillEffectType.IncreaseClickPower:
                    totalClickPowerBonus += effectValue;
                    break;

                case SkillEffectType.IncreaseGoldGain:
                    totalGoldGainBonus += effectValue;
                    break;

                case SkillEffectType.IncreaseExpGain:
                    totalExpGainBonus += effectValue;
                    break;

                case SkillEffectType.IncreaseCriticalChance:
                    totalCriticalChanceBonus += effectValue;
                    break;

                case SkillEffectType.IncreaseCriticalDamage:
                    totalCriticalDamageBonus += effectValue;
                    break;

                case SkillEffectType.ReduceSuspicionGain:
                    totalSuspicionReduction += effectValue;
                    break;

                case SkillEffectType.IncreaseIdleGain:
                    totalIdleGainBonus += effectValue;
                    break;

                case SkillEffectType.IncreaseOfflineGain:
                    totalOfflineGainBonus += effectValue;
                    break;

                // ... 기타 효과
            }
        }

        // 게임 매니저에 반영
        ApplySkillEffectsToGame();

        Debug.Log($"Skills recalculated - Click: +{totalClickPowerBonus * 100:F1}%, Gold: +{totalGoldGainBonus * 100:F1}%");
    }

    private void ApplySkillEffectsToGame()
    {
        GameManager gm = GameManager.Instance;

        gm.skillClickPowerBonus = totalClickPowerBonus;
        gm.skillGoldGainBonus = totalGoldGainBonus;
        gm.skillExpGainBonus = totalExpGainBonus;
        gm.skillCriticalChanceBonus = totalCriticalChanceBonus;
        gm.skillCriticalDamageBonus = totalCriticalDamageBonus;

        SuspicionManager.Instance.suspicionReductionBonus = totalSuspicionReduction;
        ClickerManager.Instance.idleGainBonus = totalIdleGainBonus;
        ClickerManager.Instance.offlineGainBonus = totalOfflineGainBonus;
    }

    // 스킬 포인트 추가
    public void AddSkillPoint(int amount, string source = "")
    {
        availableSkillPoints += amount;
        totalEarnedPoints += amount;

        EventManager.TriggerSkillPointChanged(availableSkillPoints);

        UIManager.Instance.ShowNotification($"스킬 포인트 +{amount}!");

        // 알림 뱃지 표시
        UIManager.Instance.ShowSkillTreeNotification(true);
    }

    // 스킬 초기화
    public bool ResetSkillTree(bool keepCoreSkills = false)
    {
        int totalRefund = 0;
        List<string> skillsToRemove = new List<string>();

        foreach (var skill in learnedSkills.Values)
        {
            // 코어 스킬 유지
            if (keepCoreSkills && skill.skillData.isCoreSkill)
                continue;

            totalRefund += skill.totalSpentPoints;
            skillsToRemove.Add(skill.skillId);
        }

        // 스킬 제거
        foreach (string skillId in skillsToRemove)
        {
            learnedSkills.Remove(skillId);
        }

        // 포인트 환불
        availableSkillPoints += totalRefund;
        usedPoints -= totalRefund;

        // 효과 재계산
        RecalculateSkillEffects();

        UIManager.Instance.ShowNotification($"스킬 초기화 완료! {totalRefund} 포인트 환불");

        return true;
    }

    // 특정 카테고리의 배운 스킬 수
    public int GetSkillCountByCategory(SkillCategory category)
    {
        return learnedSkills.Values.Count(s => s.skillData.category == category);
    }

    private void PlaySkillLearnEffect()
    {
        AudioManager.Instance.PlaySFX("skill_learn");
        // 파티클 효과 재생
    }

    // 저장/로드
    public SkillSaveData GetSaveData()
    {
        SkillSaveData saveData = new SkillSaveData();
        saveData.availableSkillPoints = availableSkillPoints;
        saveData.totalEarnedPoints = totalEarnedPoints;

        foreach (var skill in learnedSkills.Values)
        {
            saveData.learnedSkills.Add(new SkillSaveEntry
            {
                skillId = skill.skillId,
                currentLevel = skill.currentLevel,
                totalSpentPoints = skill.totalSpentPoints
            });
        }

        return saveData;
    }

    public void LoadSaveData(SkillSaveData saveData)
    {
        availableSkillPoints = saveData.availableSkillPoints;
        totalEarnedPoints = saveData.totalEarnedPoints;

        learnedSkills.Clear();

        foreach (var skillEntry in saveData.learnedSkills)
        {
            SkillData skillData = GameDatabase.Instance.GetSkill(skillEntry.skillId);
            if (skillData != null)
            {
                LearnedSkill skill = new LearnedSkill(skillEntry.skillId, skillData);
                skill.currentLevel = skillEntry.currentLevel;
                skill.totalSpentPoints = skillEntry.totalSpentPoints;

                learnedSkills[skillEntry.skillId] = skill;
            }
        }

        RecalculateSkillEffects();
    }
}

[System.Serializable]
public class SkillSaveData
{
    public int availableSkillPoints;
    public int totalEarnedPoints;
    public List<SkillSaveEntry> learnedSkills = new List<SkillSaveEntry>();
}

[System.Serializable]
public class SkillSaveEntry
{
    public string skillId;
    public int currentLevel;
    public int totalSpentPoints;
}
```

---

## 📊 밸런싱

### 스킬 포인트 분배 (레벨 100 기준)

**총 획득 가능 포인트**: 약 120 포인트

**추천 분배**:
- 1개 카테고리 집중: 80 포인트 (핵심 스킬 극대화)
- 나머지 분산: 40 포인트 (유틸리티 스킬)

**밸런스형 분배**:
- 각 카테고리: 40 포인트씩
- 모든 스킬 골고루 습득

### 레벨별 권장 스킬

| 레벨 | 권장 스킬 | 이유 |
|------|-----------|------|
| 1-10 | 기본 무술, 효율적 학습 | 초반 성장 가속 |
| 11-20 | 황금 손가락, 평범한 척 | 골드 & 의심도 관리 |
| 21-30 | 치명타, 방치 수익 | 전투력 & 방치 수익 |
| 31-50 | 자동 전투, 오프라인 수익 | 편의성 증가 |
| 51-100 | 전문화 스킬 | 플레이 스타일에 맞춰 특화 |

### 카테고리별 효율

| 카테고리 | 초반 (1-30) | 중반 (31-60) | 후반 (61-100) |
|----------|-------------|--------------|---------------|
| 은둔 | ★★★ | ★★★ | ★★ |
| 전투 | ★★ | ★★★ | ★★★ |
| 성장 | ★★★ | ★★ | ★★★ |

**초반**: 골드/경험치 확보가 중요 → 성장, 은둔
**중반**: 던전 클리어 속도 → 전투
**후반**: 효율적 파밍 → 성장, 전투

---

## 📋 체크리스트

### 데이터
- [ ] 스킬 데이터 50개 작성
- [ ] 스킬 트리 구조 설계
- [ ] 선행 스킬 관계 정의
- [ ] 스킬 밸런싱

### 시스템 구현
- [ ] SkillTreeManager
- [ ] 스킬 배우기 로직
- [ ] 스킬 레벨업 로직
- [ ] 스킬 효과 적용
- [ ] 스킬 리셋 시스템

### UI 구현
- [ ] 스킬 트리 UI (3 카테고리)
- [ ] 스킬 아이콘
- [ ] 스킬 선 연결 (prerequisite)
- [ ] 스킬 상세 정보 팝업
- [ ] 스킬 포인트 표시
- [ ] 배움/레벨업 애니메이션

### 기능
- [ ] 스킬 조건 체크
- [ ] 스킬 효과 계산
- [ ] 스킬 포인트 관리
- [ ] 스킬 리셋
- [ ] 스킬 프리셋 (저장/로드)

### 테스트
- [ ] 모든 스킬 학습 테스트
- [ ] 선행 스킬 체크 테스트
- [ ] 스킬 효과 검증
- [ ] 밸런싱 테스트
- [ ] 리셋 기능 테스트

---

**작성 완료일**: 2025-11-13
**예상 개발 시간**: 8일
**담당**: 시스템 프로그래머, UI 디자이너, 밸런서
