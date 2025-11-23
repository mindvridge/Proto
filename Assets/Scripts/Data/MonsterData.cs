using System;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// JSON 몬스터 데이터를 담는 루트 컨테이너
    /// </summary>
    [Serializable]
    public class MonsterContainer
    {
        public List<MonsterData> monsters;
    }

    /// <summary>
    /// 개별 몬스터 데이터
    /// </summary>
    [Serializable]
    public class MonsterData
    {
        public string monsterId;
        public string monsterName;
        public string grade;              // F, E, D, C, B, A, S, SS, SSS, EX, Mythic, Infinite
        public string monsterType;        // Normal, Elite, Boss, WorldBoss, Hidden, Event
        public string element;            // None, Fire, Water, Ice, Thunder, Wind, Earth, Nature, Light, Dark, Chaos
        public string biome;              // Slime, Beast, Insect, Plant, Undead, Demon, Dragon, Aquatic, Elemental, Angel, Machine
        public long baseHealth;
        public long baseAttack;
        public long goldMin;
        public long goldMax;
        public long expReward;
        public float dropChance;
        public List<MonsterDropItem> dropTable;
        public bool isBoss;
        public List<string> specialDialogue;

        /// <summary>
        /// 등급을 Enum으로 변환
        /// </summary>
        public MonsterGrade GetGrade()
        {
            return grade?.ToUpper() switch
            {
                "F" => MonsterGrade.F,
                "E" => MonsterGrade.E,
                "D" => MonsterGrade.D,
                "C" => MonsterGrade.C,
                "B" => MonsterGrade.B,
                "A" => MonsterGrade.A,
                "S" => MonsterGrade.S,
                "SS" => MonsterGrade.SS,
                "SSS" => MonsterGrade.SSS,
                "EX" => MonsterGrade.EX,
                "MYTHIC" => MonsterGrade.Mythic,
                "INFINITE" => MonsterGrade.Infinite,
                _ => MonsterGrade.F
            };
        }

        /// <summary>
        /// 몬스터 타입을 Enum으로 변환
        /// </summary>
        public MonsterType GetMonsterType()
        {
            return monsterType?.ToLower() switch
            {
                "normal" => MonsterType.Normal,
                "elite" => MonsterType.Elite,
                "boss" => MonsterType.Boss,
                "worldboss" => MonsterType.WorldBoss,
                "hidden" => MonsterType.Hidden,
                "event" => MonsterType.Event,
                _ => MonsterType.Normal
            };
        }

        /// <summary>
        /// 속성을 Enum으로 변환
        /// </summary>
        public ElementType GetElement()
        {
            return element?.ToLower() switch
            {
                "fire" => ElementType.Fire,
                "water" => ElementType.Water,
                "ice" => ElementType.Ice,
                "thunder" => ElementType.Thunder,
                "wind" => ElementType.Wind,
                "earth" => ElementType.Earth,
                "nature" => ElementType.Nature,
                "light" => ElementType.Light,
                "dark" => ElementType.Dark,
                "chaos" => ElementType.Chaos,
                _ => ElementType.None
            };
        }

        /// <summary>
        /// 랜덤 골드 보상 계산
        /// </summary>
        public long GetRandomGold()
        {
            return UnityEngine.Random.Range((int)goldMin, (int)goldMax + 1);
        }

        /// <summary>
        /// 드롭 테이블이 있는지 확인
        /// </summary>
        public bool HasDropTable => dropTable != null && dropTable.Count > 0;
    }

    /// <summary>
    /// 몬스터 드롭 아이템
    /// </summary>
    [Serializable]
    public class MonsterDropItem
    {
        public string itemId;
        public float dropRate;
        public int minAmount;
        public int maxAmount;

        /// <summary>
        /// 드롭 여부 확인
        /// </summary>
        public bool RollDrop()
        {
            return UnityEngine.Random.value <= dropRate;
        }

        /// <summary>
        /// 랜덤 수량 계산
        /// </summary>
        public int GetRandomAmount()
        {
            if (maxAmount <= 0) return 1;
            return UnityEngine.Random.Range(Mathf.Max(1, minAmount), maxAmount + 1);
        }
    }

    /// <summary>
    /// 몬스터 등급
    /// </summary>
    public enum MonsterGrade
    {
        F = 0,
        E = 1,
        D = 2,
        C = 3,
        B = 4,
        A = 5,
        S = 6,
        SS = 7,
        SSS = 8,
        EX = 9,
        Mythic = 10,
        Infinite = 11
    }

    /// <summary>
    /// 몬스터 타입
    /// </summary>
    public enum MonsterType
    {
        Normal,
        Elite,
        Boss,
        WorldBoss,
        Hidden,
        Event
    }

    /// <summary>
    /// 속성 타입
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Ice,
        Thunder,
        Wind,
        Earth,
        Nature,
        Light,
        Dark,
        Chaos
    }

    /// <summary>
    /// 바이옴 타입
    /// </summary>
    public enum BiomeType
    {
        Slime,
        Beast,
        Insect,
        Plant,
        Undead,
        Demon,
        Dragon,
        Aquatic,
        Elemental,
        Angel,
        Machine
    }

    /// <summary>
    /// 등급별 색상 유틸리티
    /// </summary>
    public static class MonsterGradeColors
    {
        public static Color GetColor(MonsterGrade grade)
        {
            return grade switch
            {
                MonsterGrade.F => new Color(0.5f, 0.5f, 0.5f),       // 회색
                MonsterGrade.E => new Color(1f, 1f, 1f),             // 흰색
                MonsterGrade.D => new Color(0.5f, 1f, 0.5f),         // 연두색
                MonsterGrade.C => new Color(0.4f, 0.8f, 1f),         // 하늘색
                MonsterGrade.B => new Color(0.6f, 0.4f, 1f),         // 보라색
                MonsterGrade.A => new Color(1f, 0.8f, 0.2f),         // 금색
                MonsterGrade.S => new Color(1f, 0.5f, 0.2f),         // 주황색
                MonsterGrade.SS => new Color(1f, 0.3f, 0.3f),        // 빨간색
                MonsterGrade.SSS => new Color(1f, 0.2f, 0.6f),       // 분홍색
                MonsterGrade.EX => new Color(0.8f, 0.2f, 1f),        // 마젠타
                MonsterGrade.Mythic => new Color(1f, 0.9f, 0.4f),    // 황금색
                MonsterGrade.Infinite => new Color(0.3f, 1f, 0.9f),  // 시안
                _ => Color.white
            };
        }

        public static string GetColorHex(MonsterGrade grade)
        {
            Color color = GetColor(grade);
            return ColorUtility.ToHtmlStringRGB(color);
        }
    }
}
