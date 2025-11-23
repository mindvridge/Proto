using System;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// JSON 대화 데이터를 담는 루트 컨테이너
    /// </summary>
    [Serializable]
    public class DialogueContainer
    {
        public List<DialogueData> dialogues;
    }

    /// <summary>
    /// 개별 대화 데이터
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        public string dialogueId;
        public string dialogueType;      // Tutorial, Story, NPC, Boss, Event, System
        public string characterName;
        public string characterSprite;
        public string text;
        public string textEN;
        public string nextDialogueId;
        public List<DialogueChoice> choices;
        public DialogueEffect effect;
        public DialogueReward reward;
        public string action;
        public string actionParam;

        /// <summary>
        /// 현재 언어 설정에 따른 텍스트 반환
        /// </summary>
        public string GetLocalizedText(bool useEnglish = false)
        {
            if (useEnglish && !string.IsNullOrEmpty(textEN))
                return textEN;
            return text;
        }

        /// <summary>
        /// 선택지가 있는지 확인
        /// </summary>
        public bool HasChoices => choices != null && choices.Count > 0;

        /// <summary>
        /// 다음 대화가 있는지 확인
        /// </summary>
        public bool HasNextDialogue => !string.IsNullOrEmpty(nextDialogueId);

        /// <summary>
        /// 효과가 있는지 확인
        /// </summary>
        public bool HasEffect => effect != null;

        /// <summary>
        /// 보상이 있는지 확인
        /// </summary>
        public bool HasReward => reward != null;

        /// <summary>
        /// 특수 액션이 있는지 확인
        /// </summary>
        public bool HasAction => !string.IsNullOrEmpty(action);
    }

    /// <summary>
    /// 대화 선택지
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string choiceTextEN;
        public string nextDialogueId;

        /// <summary>
        /// 현재 언어 설정에 따른 선택지 텍스트 반환
        /// </summary>
        public string GetLocalizedText(bool useEnglish = false)
        {
            if (useEnglish && !string.IsNullOrEmpty(choiceTextEN))
                return choiceTextEN;
            return choiceText;
        }
    }

    /// <summary>
    /// 대화 효과 (의심도, 버프/디버프 등)
    /// </summary>
    [Serializable]
    public class DialogueEffect
    {
        public float suspicion;              // 의심도 변화량
        public float nightBonus;             // 야간 보너스
        public float suspicionMultiplier;    // 의심도 배율
        public string debuff;                // 디버프 ID

        /// <summary>
        /// 효과가 비어있는지 확인
        /// </summary>
        public bool IsEmpty =>
            suspicion == 0 &&
            nightBonus == 0 &&
            suspicionMultiplier == 0 &&
            string.IsNullOrEmpty(debuff);
    }

    /// <summary>
    /// 대화 보상
    /// </summary>
    [Serializable]
    public class DialogueReward
    {
        public string type;      // Gold, HP, SkillPoint, Item
        public long amount;
        public string itemId;
        public int count;

        /// <summary>
        /// 보상 타입 열거형으로 변환
        /// </summary>
        public RewardType GetRewardType()
        {
            return type?.ToLower() switch
            {
                "gold" => RewardType.Gold,
                "hp" => RewardType.HP,
                "skillpoint" => RewardType.SkillPoint,
                "item" => RewardType.Item,
                _ => RewardType.None
            };
        }
    }

    /// <summary>
    /// 보상 타입
    /// </summary>
    public enum RewardType
    {
        None,
        Gold,
        HP,
        SkillPoint,
        Item
    }

    /// <summary>
    /// 대화 타입
    /// </summary>
    public enum DialogueType
    {
        Tutorial,
        Story,
        NPC,
        Boss,
        Event,
        System
    }

    /// <summary>
    /// DialogueData 확장 메서드
    /// </summary>
    public static class DialogueDataExtensions
    {
        /// <summary>
        /// 문자열을 DialogueType으로 변환
        /// </summary>
        public static DialogueType ToDialogueType(this string typeString)
        {
            return typeString?.ToLower() switch
            {
                "tutorial" => DialogueType.Tutorial,
                "story" => DialogueType.Story,
                "npc" => DialogueType.NPC,
                "boss" => DialogueType.Boss,
                "event" => DialogueType.Event,
                "system" => DialogueType.System,
                _ => DialogueType.NPC
            };
        }
    }
}
