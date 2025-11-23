using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HiddenGrowth.Data
{
    /// <summary>
    /// 대화 데이터베이스 - JSON 로드 및 검색
    /// </summary>
    public class DialogueDatabase : MonoBehaviour
    {
        public static DialogueDatabase Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string jsonFileName = "Dialogues";
        [SerializeField] private bool loadFromStreamingAssets = false;

        private Dictionary<string, DialogueData> dialogueDictionary;
        private Dictionary<string, List<DialogueData>> dialoguesByType;
        private Dictionary<string, List<DialogueData>> dialoguesByCharacter;
        private bool isInitialized = false;

        public bool IsInitialized => isInitialized;
        public int TotalDialogueCount => dialogueDictionary?.Count ?? 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 데이터베이스 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            dialogueDictionary = new Dictionary<string, DialogueData>();
            dialoguesByType = new Dictionary<string, List<DialogueData>>();
            dialoguesByCharacter = new Dictionary<string, List<DialogueData>>();

            LoadDialogues();
            isInitialized = true;

            Debug.Log($"[DialogueDatabase] Initialized with {TotalDialogueCount} dialogues");
        }

        /// <summary>
        /// JSON 파일에서 대화 데이터 로드
        /// </summary>
        private void LoadDialogues()
        {
            string json = LoadJsonData();
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[DialogueDatabase] Failed to load dialogue JSON");
                return;
            }

            try
            {
                DialogueContainer container = JsonUtility.FromJson<DialogueContainer>(json);
                if (container?.dialogues == null)
                {
                    Debug.LogError("[DialogueDatabase] Invalid JSON format");
                    return;
                }

                ProcessDialogues(container.dialogues);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueDatabase] JSON parse error: {e.Message}");
            }
        }

        /// <summary>
        /// JSON 데이터 로드 (Resources 또는 StreamingAssets)
        /// </summary>
        private string LoadJsonData()
        {
            if (loadFromStreamingAssets)
            {
                string path = Path.Combine(Application.streamingAssetsPath, $"{jsonFileName}.json");
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
                Debug.LogWarning($"[DialogueDatabase] File not found: {path}");
            }

            // Resources 폴더에서 로드
            TextAsset textAsset = Resources.Load<TextAsset>($"GameData/{jsonFileName}");
            if (textAsset != null)
            {
                return textAsset.text;
            }

            Debug.LogWarning($"[DialogueDatabase] Resource not found: GameData/{jsonFileName}");
            return null;
        }

        /// <summary>
        /// 대화 데이터 처리 및 인덱싱
        /// </summary>
        private void ProcessDialogues(List<DialogueData> dialogues)
        {
            foreach (var dialogue in dialogues)
            {
                if (string.IsNullOrEmpty(dialogue.dialogueId))
                {
                    Debug.LogWarning("[DialogueDatabase] Skipping dialogue with empty ID");
                    continue;
                }

                // ID로 인덱싱
                if (dialogueDictionary.ContainsKey(dialogue.dialogueId))
                {
                    Debug.LogWarning($"[DialogueDatabase] Duplicate ID: {dialogue.dialogueId}");
                    continue;
                }
                dialogueDictionary[dialogue.dialogueId] = dialogue;

                // 타입별 인덱싱
                string type = dialogue.dialogueType ?? "Unknown";
                if (!dialoguesByType.ContainsKey(type))
                    dialoguesByType[type] = new List<DialogueData>();
                dialoguesByType[type].Add(dialogue);

                // 캐릭터별 인덱싱
                string character = dialogue.characterName ?? "Unknown";
                if (!dialoguesByCharacter.ContainsKey(character))
                    dialoguesByCharacter[character] = new List<DialogueData>();
                dialoguesByCharacter[character].Add(dialogue);
            }
        }

        #region Public Query Methods

        /// <summary>
        /// ID로 대화 데이터 가져오기
        /// </summary>
        public DialogueData GetDialogue(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId))
                return null;

            dialogueDictionary.TryGetValue(dialogueId, out DialogueData data);
            return data;
        }

        /// <summary>
        /// 타입별 대화 목록 가져오기
        /// </summary>
        public List<DialogueData> GetDialoguesByType(string type)
        {
            if (dialoguesByType.TryGetValue(type, out List<DialogueData> list))
                return new List<DialogueData>(list);
            return new List<DialogueData>();
        }

        /// <summary>
        /// 타입별 대화 목록 가져오기 (Enum)
        /// </summary>
        public List<DialogueData> GetDialoguesByType(DialogueType type)
        {
            return GetDialoguesByType(type.ToString());
        }

        /// <summary>
        /// 캐릭터별 대화 목록 가져오기
        /// </summary>
        public List<DialogueData> GetDialoguesByCharacter(string characterName)
        {
            if (dialoguesByCharacter.TryGetValue(characterName, out List<DialogueData> list))
                return new List<DialogueData>(list);
            return new List<DialogueData>();
        }

        /// <summary>
        /// 모든 대화 ID 목록 가져오기
        /// </summary>
        public List<string> GetAllDialogueIds()
        {
            return dialogueDictionary.Keys.ToList();
        }

        /// <summary>
        /// 모든 캐릭터 이름 목록 가져오기
        /// </summary>
        public List<string> GetAllCharacterNames()
        {
            return dialoguesByCharacter.Keys.ToList();
        }

        /// <summary>
        /// 대화가 존재하는지 확인
        /// </summary>
        public bool HasDialogue(string dialogueId)
        {
            return !string.IsNullOrEmpty(dialogueId) && dialogueDictionary.ContainsKey(dialogueId);
        }

        /// <summary>
        /// 랜덤 대화 가져오기 (타입별)
        /// </summary>
        public DialogueData GetRandomDialogue(string type)
        {
            var list = GetDialoguesByType(type);
            if (list.Count == 0) return null;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// 조건에 맞는 대화 검색
        /// </summary>
        public List<DialogueData> SearchDialogues(Func<DialogueData, bool> predicate)
        {
            return dialogueDictionary.Values.Where(predicate).ToList();
        }

        #endregion

        #region Runtime Reload

        /// <summary>
        /// 데이터베이스 리로드 (에디터/디버그용)
        /// </summary>
        public void Reload()
        {
            isInitialized = false;
            dialogueDictionary?.Clear();
            dialoguesByType?.Clear();
            dialoguesByCharacter?.Clear();
            Initialize();
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 통계 출력
        /// </summary>
        [ContextMenu("Print Statistics")]
        private void PrintStatistics()
        {
            Debug.Log("=== Dialogue Database Statistics ===");
            Debug.Log($"Total Dialogues: {TotalDialogueCount}");
            Debug.Log("--- By Type ---");
            foreach (var kvp in dialoguesByType)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.Count}");
            }
            Debug.Log("--- By Character ---");
            foreach (var kvp in dialoguesByCharacter)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value.Count}");
            }
        }
#endif
    }
}
