using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HermitHero.LipSync
{
    /// <summary>
    /// 립싱크 데이터베이스 - 음소와 입 모양 매핑 관리
    /// </summary>
    [CreateAssetMenu(fileName = "LipSyncDatabase", menuName = "Hermit Hero/Lip Sync Database")]
    public class LipSyncDatabase : ScriptableObject
    {
        [Header("Mouth Sprites")]
        [Tooltip("입 모양 스프라이트 매핑")]
        public List<MouthSpriteMapping> mouthSprites = new List<MouthSpriteMapping>();

        [Header("Phoneme Mappings")]
        [Tooltip("음소-입모양 매핑 (한국어)")]
        public List<PhonemeMapping> koreanPhonemeMappings = new List<PhonemeMapping>();

        [Tooltip("음소-입모양 매핑 (영어)")]
        public List<PhonemeMapping> englishPhonemeMappings = new List<PhonemeMapping>();

        [Header("Lip Sync Data")]
        [Tooltip("사전 정의된 립싱크 데이터")]
        public List<LipSyncData> lipSyncDataList = new List<LipSyncData>();

        [Header("Auto Generation Settings")]
        [Tooltip("자동 생성 시 기본 음소 지속 시간")]
        public float defaultPhonemeDuration = 0.1f;

        private Dictionary<string, LipSyncData> lipSyncDataCache;
        private Dictionary<string, Sprite> mouthSpriteCache;
        private Dictionary<MouthShape, Sprite> shapeToSpriteCache;

        void OnEnable()
        {
            InitializeCaches();
        }

        /// <summary>
        /// 캐시 초기화
        /// </summary>
        private void InitializeCaches()
        {
            // 립싱크 데이터 캐시
            lipSyncDataCache = new Dictionary<string, LipSyncData>();
            foreach (var data in lipSyncDataList)
            {
                if (!lipSyncDataCache.ContainsKey(data.dialogueId))
                {
                    lipSyncDataCache[data.dialogueId] = data;
                }
            }

            // 입 스프라이트 캐시
            mouthSpriteCache = new Dictionary<string, Sprite>();
            shapeToSpriteCache = new Dictionary<MouthShape, Sprite>();

            foreach (var mapping in mouthSprites)
            {
                if (!string.IsNullOrEmpty(mapping.phoneme) && mapping.sprite != null)
                {
                    mouthSpriteCache[mapping.phoneme] = mapping.sprite;
                }

                if (mapping.sprite != null)
                {
                    shapeToSpriteCache[mapping.shape] = mapping.sprite;
                }
            }
        }

        /// <summary>
        /// 립싱크 데이터 가져오기
        /// </summary>
        public LipSyncData GetLipSyncData(string dialogueId)
        {
            if (lipSyncDataCache == null)
            {
                InitializeCaches();
            }

            if (lipSyncDataCache.TryGetValue(dialogueId, out LipSyncData data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        /// 음소에 해당하는 입 스프라이트 가져오기
        /// </summary>
        public Sprite GetMouthSprite(string phoneme)
        {
            if (mouthSpriteCache == null)
            {
                InitializeCaches();
            }

            if (mouthSpriteCache.TryGetValue(phoneme, out Sprite sprite))
            {
                return sprite;
            }

            // 기본 스프라이트 반환
            return mouthSprites.Count > 0 ? mouthSprites[0].sprite : null;
        }

        /// <summary>
        /// 입 모양에 해당하는 스프라이트 가져오기
        /// </summary>
        public Sprite GetMouthSpriteByShape(MouthShape shape)
        {
            if (shapeToSpriteCache == null)
            {
                InitializeCaches();
            }

            if (shapeToSpriteCache.TryGetValue(shape, out Sprite sprite))
            {
                return sprite;
            }

            return mouthSprites.Count > 0 ? mouthSprites[0].sprite : null;
        }

        /// <summary>
        /// 텍스트에서 음소 자동 생성
        /// </summary>
        public List<PhonemeData> GeneratePhonemesFromText(string text, float duration)
        {
            List<PhonemeData> phonemes = new List<PhonemeData>();

            if (string.IsNullOrEmpty(text))
            {
                return phonemes;
            }

            // 텍스트를 음소로 변환
            List<string> phonemeList = TextToPhonemes(text);

            if (phonemeList.Count == 0)
            {
                return phonemes;
            }

            // 각 음소에 타이밍 할당
            float timePerPhoneme = duration / phonemeList.Count;
            float currentTime = 0f;

            foreach (string phoneme in phonemeList)
            {
                PhonemeData data = new PhonemeData
                {
                    phoneme = phoneme,
                    startTime = currentTime,
                    duration = timePerPhoneme,
                    shape = GetMouthShapeForPhoneme(phoneme)
                };

                phonemes.Add(data);
                currentTime += timePerPhoneme;
            }

            return phonemes;
        }

        /// <summary>
        /// 텍스트를 음소 리스트로 변환
        /// </summary>
        private List<string> TextToPhonemes(string text)
        {
            List<string> phonemes = new List<string>();

            // 한글 체크
            bool isKorean = text.Any(c => (c >= '가' && c <= '힣') || (c >= 'ㄱ' && c <= 'ㅎ') || (c >= 'ㅏ' && c <= 'ㅣ'));

            if (isKorean)
            {
                phonemes = ConvertKoreanToPhonemes(text);
            }
            else
            {
                phonemes = ConvertEnglishToPhonemes(text);
            }

            return phonemes;
        }

        /// <summary>
        /// 한글을 음소로 변환
        /// </summary>
        private List<string> ConvertKoreanToPhonemes(string text)
        {
            List<string> phonemes = new List<string>();

            // 한글 초성, 중성, 종성 분리
            char[] cho = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
            char[] jung = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
            char[] jong = { ' ', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

            foreach (char c in text)
            {
                if (c >= '가' && c <= '힣')
                {
                    int unicode = c - 0xAC00;
                    int choIndex = unicode / (21 * 28);
                    int jungIndex = (unicode % (21 * 28)) / 28;
                    int jongIndex = unicode % 28;

                    // 초성
                    string choPhoneme = ConvertConsonantToPhoneme(cho[choIndex]);
                    if (!string.IsNullOrEmpty(choPhoneme))
                    {
                        phonemes.Add(choPhoneme);
                    }

                    // 중성 (모음)
                    string jungPhoneme = ConvertVowelToPhoneme(jung[jungIndex]);
                    if (!string.IsNullOrEmpty(jungPhoneme))
                    {
                        phonemes.Add(jungPhoneme);
                    }

                    // 종성
                    if (jongIndex > 0)
                    {
                        string jongPhoneme = ConvertConsonantToPhoneme(jong[jongIndex]);
                        if (!string.IsNullOrEmpty(jongPhoneme))
                        {
                            phonemes.Add(jongPhoneme);
                        }
                    }
                }
                else if (c != ' ')
                {
                    // 기타 문자는 기본 음소로
                    phonemes.Add("A");
                }
            }

            return phonemes;
        }

        /// <summary>
        /// 영어를 음소로 변환 (간단한 버전)
        /// </summary>
        private List<string> ConvertEnglishToPhonemes(string text)
        {
            List<string> phonemes = new List<string>();

            text = text.ToLower();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    string phoneme = ConvertEnglishCharToPhoneme(c);
                    if (!string.IsNullOrEmpty(phoneme))
                    {
                        phonemes.Add(phoneme);
                    }
                }
            }

            return phonemes;
        }

        /// <summary>
        /// 자음을 음소로 변환
        /// </summary>
        private string ConvertConsonantToPhoneme(char consonant)
        {
            switch (consonant)
            {
                case 'ㅁ':
                case 'ㅂ':
                case 'ㅃ':
                case 'ㅍ':
                    return "M";  // 입술 닫힘

                case 'ㄴ':
                case 'ㄷ':
                case 'ㄸ':
                case 'ㅌ':
                    return "D";  // 혀 위치

                case 'ㄱ':
                case 'ㄲ':
                case 'ㅋ':
                    return "G";  // 목구멍

                case 'ㅅ':
                case 'ㅆ':
                case 'ㅈ':
                case 'ㅉ':
                case 'ㅊ':
                    return "S";  // 치찰음

                case 'ㅎ':
                    return "H";  // 기식음

                default:
                    return "A";
            }
        }

        /// <summary>
        /// 모음을 음소로 변환
        /// </summary>
        private string ConvertVowelToPhoneme(char vowel)
        {
            switch (vowel)
            {
                case 'ㅏ':
                case 'ㅐ':
                case 'ㅑ':
                case 'ㅒ':
                    return "A";

                case 'ㅓ':
                case 'ㅔ':
                case 'ㅕ':
                case 'ㅖ':
                    return "E";

                case 'ㅗ':
                case 'ㅘ':
                case 'ㅙ':
                case 'ㅚ':
                case 'ㅛ':
                    return "O";

                case 'ㅜ':
                case 'ㅝ':
                case 'ㅞ':
                case 'ㅟ':
                case 'ㅠ':
                    return "U";

                case 'ㅡ':
                    return "EU";

                case 'ㅣ':
                case 'ㅢ':
                    return "I";

                default:
                    return "A";
            }
        }

        /// <summary>
        /// 영어 문자를 음소로 변환
        /// </summary>
        private string ConvertEnglishCharToPhoneme(char c)
        {
            // 모음
            if ("aeiou".Contains(c))
            {
                switch (c)
                {
                    case 'a': return "A";
                    case 'e': return "E";
                    case 'i': return "I";
                    case 'o': return "O";
                    case 'u': return "U";
                }
            }

            // 자음
            if ("bpm".Contains(c))
                return "M";  // 입술음
            else if ("fv".Contains(c))
                return "F";  // 치음
            else if ("td".Contains(c))
                return "D";  // 치경음
            else if ("sz".Contains(c))
                return "S";  // 치찰음
            else if ("gk".Contains(c))
                return "G";  // 연구개음
            else if ("lr".Contains(c))
                return "L";  // 유음
            else
                return "A";  // 기본
        }

        /// <summary>
        /// 음소에 해당하는 입 모양 가져오기
        /// </summary>
        private MouthShape GetMouthShapeForPhoneme(string phoneme)
        {
            switch (phoneme)
            {
                case "A":
                    return MouthShape.Wide;

                case "E":
                    return MouthShape.Medium;

                case "I":
                    return MouthShape.Narrow;

                case "O":
                case "U":
                    return MouthShape.O_Shape;

                case "M":
                    return MouthShape.Closed;

                default:
                    return MouthShape.Medium;
            }
        }

        /// <summary>
        /// 립싱크 데이터 추가
        /// </summary>
        public void AddLipSyncData(LipSyncData data)
        {
            if (!lipSyncDataList.Contains(data))
            {
                lipSyncDataList.Add(data);
            }

            // 캐시 업데이트
            if (lipSyncDataCache == null)
            {
                InitializeCaches();
            }

            lipSyncDataCache[data.dialogueId] = data;
        }

        /// <summary>
        /// 립싱크 데이터 제거
        /// </summary>
        public void RemoveLipSyncData(string dialogueId)
        {
            LipSyncData data = lipSyncDataList.Find(d => d.dialogueId == dialogueId);
            if (data != null)
            {
                lipSyncDataList.Remove(data);
            }

            if (lipSyncDataCache != null && lipSyncDataCache.ContainsKey(dialogueId))
            {
                lipSyncDataCache.Remove(dialogueId);
            }
        }
    }

    /// <summary>
    /// 입 스프라이트 매핑
    /// </summary>
    [System.Serializable]
    public class MouthSpriteMapping
    {
        [Tooltip("음소 문자")]
        public string phoneme;

        [Tooltip("입 모양")]
        public MouthShape shape;

        [Tooltip("입 스프라이트")]
        public Sprite sprite;
    }

    /// <summary>
    /// 음소 매핑
    /// </summary>
    [System.Serializable]
    public class PhonemeMapping
    {
        [Tooltip("음소")]
        public string phoneme;

        [Tooltip("입 모양")]
        public MouthShape mouthShape;

        [Tooltip("설명")]
        public string description;
    }
}
