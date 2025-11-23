using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HiddenGrowth.Data;
using TMPro;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 대화 시스템 매니저 - 대화 표시, 선택지, 자동/스킵 모드, 보이스 지원
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        #region UI References
        [Header("=== UI References ===")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image characterImage;
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private Transform choiceButtonContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Control Buttons")]
        [SerializeField] private Button autoButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Button logButton;
        [SerializeField] private Image autoButtonHighlight;
        #endregion

        #region Settings
        [Header("=== Settings ===")]
        [SerializeField] private float textSpeed = 0.03f;
        [SerializeField] private float autoModeDelay = 2.0f;
        [SerializeField] private float skipModeSpeed = 0.005f;
        [SerializeField] private bool useEnglish = false;

        [Header("Audio")]
        [SerializeField] private AudioSource voiceAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;
        [SerializeField] private AudioClip typingSound;
        [SerializeField] private float typingSoundInterval = 3;
        #endregion

        #region Events
        [Header("=== Events ===")]
        public UnityEvent<DialogueData> OnDialogueStart;
        public UnityEvent<DialogueData> OnDialogueEnd;
        public UnityEvent<DialogueChoice> OnChoiceSelected;
        public UnityEvent<DialogueEffect> OnEffectApplied;
        public UnityEvent<DialogueReward> OnRewardGiven;
        public UnityEvent<string, string> OnActionTriggered;
        #endregion

        #region State
        private DialogueData currentDialogue;
        private Coroutine typewriterCoroutine;
        private bool isTyping = false;
        private bool isAutoMode = false;
        private bool isSkipMode = false;
        private bool isDialogueActive = false;
        private int typingSoundCounter = 0;
        private List<string> seenDialogues = new List<string>();
        private List<DialogueLogEntry> dialogueLog = new List<DialogueLogEntry>();
        #endregion

        #region Properties
        public bool IsDialogueActive => isDialogueActive;
        public bool IsAutoMode => isAutoMode;
        public bool IsSkipMode => isSkipMode;
        public DialogueData CurrentDialogue => currentDialogue;
        public List<DialogueLogEntry> DialogueLog => dialogueLog;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (choicePanel != null)
                choicePanel.SetActive(false);

            // 버튼 이벤트 연결
            autoButton?.onClick.AddListener(ToggleAutoMode);
            skipButton?.onClick.AddListener(OnSkipClicked);
            logButton?.onClick.AddListener(ShowLogPanel);

            UpdateAutoButtonVisual();
        }

        #region Public Methods - Start/Stop Dialogue

        /// <summary>
        /// 대화 시작
        /// </summary>
        public void StartDialogue(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId))
            {
                Debug.LogWarning("[DialogueManager] Empty dialogue ID");
                return;
            }

            DialogueData dialogue = DialogueDatabase.Instance?.GetDialogue(dialogueId);
            if (dialogue == null)
            {
                Debug.LogError($"[DialogueManager] Dialogue not found: {dialogueId}");
                return;
            }

            StartDialogue(dialogue);
        }

        /// <summary>
        /// 대화 시작 (DialogueData 직접 전달)
        /// </summary>
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null) return;

            currentDialogue = dialogue;
            isDialogueActive = true;

            // UI 활성화
            dialoguePanel?.SetActive(true);
            choicePanel?.SetActive(false);

            // 캐릭터 정보 표시
            ShowCharacterInfo(dialogue);

            // 타이핑 효과 시작
            StartTypewriter(dialogue.GetLocalizedText(useEnglish));

            // 보이스 재생
            PlayVoice(dialogue);

            // 이벤트 발생
            OnDialogueStart?.Invoke(dialogue);

            // 대화 로그에 추가
            AddToLog(dialogue);

            Debug.Log($"[DialogueManager] Started dialogue: {dialogue.dialogueId}");
        }

        /// <summary>
        /// 대화 강제 종료
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive) return;

            StopTypewriter();
            StopVoice();

            // 본 대화 목록에 추가
            if (currentDialogue != null && !seenDialogues.Contains(currentDialogue.dialogueId))
            {
                seenDialogues.Add(currentDialogue.dialogueId);
            }

            // 이벤트 발생
            OnDialogueEnd?.Invoke(currentDialogue);

            // UI 비활성화
            dialoguePanel?.SetActive(false);
            choicePanel?.SetActive(false);

            currentDialogue = null;
            isDialogueActive = false;
            isAutoMode = false;
            isSkipMode = false;

            UpdateAutoButtonVisual();

            Debug.Log("[DialogueManager] Dialogue ended");
        }

        #endregion

        #region Dialogue Click/Progress

        /// <summary>
        /// 대화창 클릭 시 처리
        /// </summary>
        public void OnDialogueClicked()
        {
            if (!isDialogueActive) return;

            if (isTyping)
            {
                // 타이핑 중이면 즉시 완료
                CompleteTypewriter();
            }
            else
            {
                // 다음 대화로 진행
                ProgressDialogue();
            }
        }

        /// <summary>
        /// 대화 진행
        /// </summary>
        private void ProgressDialogue()
        {
            if (currentDialogue == null) return;

            // 효과 적용
            ApplyEffect(currentDialogue.effect);

            // 보상 지급
            GiveReward(currentDialogue.reward);

            // 특수 액션 실행
            if (currentDialogue.HasAction)
            {
                TriggerAction(currentDialogue.action, currentDialogue.actionParam);
            }

            // 선택지 있으면 표시
            if (currentDialogue.HasChoices)
            {
                ShowChoices();
                return;
            }

            // 다음 대화로 이동
            if (currentDialogue.HasNextDialogue)
            {
                StartDialogue(currentDialogue.nextDialogueId);
            }
            else
            {
                EndDialogue();
            }
        }

        #endregion

        #region Choice System

        /// <summary>
        /// 선택지 표시
        /// </summary>
        private void ShowChoices()
        {
            if (currentDialogue?.choices == null) return;

            // 기존 선택지 버튼 제거
            ClearChoiceButtons();

            // 선택지 버튼 생성
            foreach (var choice in currentDialogue.choices)
            {
                CreateChoiceButton(choice);
            }

            choicePanel?.SetActive(true);
        }

        /// <summary>
        /// 선택지 버튼 생성
        /// </summary>
        private void CreateChoiceButton(DialogueChoice choice)
        {
            if (choiceButtonPrefab == null || choiceButtonContainer == null) return;

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonContainer);

            // 텍스트 설정
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = choice.GetLocalizedText(useEnglish);
            }

            // 클릭 이벤트 설정
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnChoiceClicked(choice));
            }
        }

        /// <summary>
        /// 선택지 버튼 모두 제거
        /// </summary>
        private void ClearChoiceButtons()
        {
            if (choiceButtonContainer == null) return;

            foreach (Transform child in choiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 선택지 클릭 처리
        /// </summary>
        private void OnChoiceClicked(DialogueChoice choice)
        {
            if (choice == null) return;

            choicePanel?.SetActive(false);

            // 이벤트 발생
            OnChoiceSelected?.Invoke(choice);

            // 로그에 추가
            AddChoiceToLog(choice);

            // 다음 대화로 이동
            if (!string.IsNullOrEmpty(choice.nextDialogueId))
            {
                StartDialogue(choice.nextDialogueId);
            }
            else
            {
                EndDialogue();
            }
        }

        #endregion

        #region Auto/Skip Mode

        /// <summary>
        /// 자동 모드 토글
        /// </summary>
        public void ToggleAutoMode()
        {
            isAutoMode = !isAutoMode;
            isSkipMode = false;
            UpdateAutoButtonVisual();

            if (isAutoMode && !isTyping && isDialogueActive)
            {
                StartCoroutine(AutoModeProgress());
            }

            Debug.Log($"[DialogueManager] Auto mode: {isAutoMode}");
        }

        /// <summary>
        /// 스킵 버튼 클릭
        /// </summary>
        public void OnSkipClicked()
        {
            if (!isDialogueActive) return;

            // 이미 본 대화만 스킵 가능
            if (currentDialogue != null && seenDialogues.Contains(currentDialogue.dialogueId))
            {
                isSkipMode = true;
                isAutoMode = false;

                if (isTyping)
                {
                    CompleteTypewriter();
                }
                ProgressDialogue();
            }
            else
            {
                Debug.Log("[DialogueManager] Can only skip seen dialogues");
                // TODO: UI에 메시지 표시
            }
        }

        /// <summary>
        /// 자동 모드 진행 코루틴
        /// </summary>
        private IEnumerator AutoModeProgress()
        {
            yield return new WaitForSeconds(autoModeDelay);

            if (isAutoMode && !isTyping && isDialogueActive && currentDialogue != null && !currentDialogue.HasChoices)
            {
                ProgressDialogue();
            }
        }

        /// <summary>
        /// 자동 버튼 비주얼 업데이트
        /// </summary>
        private void UpdateAutoButtonVisual()
        {
            if (autoButtonHighlight != null)
            {
                autoButtonHighlight.enabled = isAutoMode;
            }
        }

        #endregion

        #region Typewriter Effect

        /// <summary>
        /// 타이핑 효과 시작
        /// </summary>
        private void StartTypewriter(string text)
        {
            StopTypewriter();
            typewriterCoroutine = StartCoroutine(TypewriterCoroutine(text));
        }

        /// <summary>
        /// 타이핑 효과 중지
        /// </summary>
        private void StopTypewriter()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;
        }

        /// <summary>
        /// 타이핑 즉시 완료
        /// </summary>
        private void CompleteTypewriter()
        {
            StopTypewriter();
            if (dialogueText != null && currentDialogue != null)
            {
                dialogueText.text = currentDialogue.GetLocalizedText(useEnglish);
            }
        }

        /// <summary>
        /// 타이핑 효과 코루틴
        /// </summary>
        private IEnumerator TypewriterCoroutine(string text)
        {
            isTyping = true;
            typingSoundCounter = 0;

            if (dialogueText != null)
            {
                dialogueText.text = "";

                float currentSpeed = isSkipMode ? skipModeSpeed : textSpeed;

                foreach (char c in text)
                {
                    dialogueText.text += c;

                    // 타이핑 사운드
                    typingSoundCounter++;
                    if (typingSoundCounter >= typingSoundInterval)
                    {
                        PlayTypingSound();
                        typingSoundCounter = 0;
                    }

                    yield return new WaitForSeconds(currentSpeed);
                }
            }

            isTyping = false;

            // 자동 모드면 자동 진행
            if (isAutoMode && currentDialogue != null && !currentDialogue.HasChoices)
            {
                StartCoroutine(AutoModeProgress());
            }
        }

        #endregion

        #region Character Display

        /// <summary>
        /// 캐릭터 정보 표시
        /// </summary>
        private void ShowCharacterInfo(DialogueData dialogue)
        {
            // 캐릭터 이름
            if (characterNameText != null)
            {
                characterNameText.text = dialogue.characterName ?? "";
            }

            // 캐릭터 이미지
            if (characterImage != null && !string.IsNullOrEmpty(dialogue.characterSprite))
            {
                Sprite sprite = LoadCharacterSprite(dialogue.characterSprite);
                if (sprite != null)
                {
                    characterImage.sprite = sprite;
                    characterImage.enabled = true;
                }
                else
                {
                    characterImage.enabled = false;
                }
            }
            else if (characterImage != null)
            {
                characterImage.enabled = false;
            }
        }

        /// <summary>
        /// 캐릭터 스프라이트 로드
        /// </summary>
        private Sprite LoadCharacterSprite(string spriteName)
        {
            return Resources.Load<Sprite>($"Sprites/Characters/{spriteName}");
        }

        #endregion

        #region Voice System

        /// <summary>
        /// 보이스 재생
        /// </summary>
        private void PlayVoice(DialogueData dialogue)
        {
            if (voiceAudioSource == null) return;

            // 보이스 클립 로드
            AudioClip voiceClip = LoadVoiceClip(dialogue.dialogueId);
            if (voiceClip != null)
            {
                voiceAudioSource.clip = voiceClip;
                voiceAudioSource.Play();
            }
        }

        /// <summary>
        /// 보이스 중지
        /// </summary>
        private void StopVoice()
        {
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }

        /// <summary>
        /// 보이스 클립 로드
        /// </summary>
        private AudioClip LoadVoiceClip(string dialogueId)
        {
            return Resources.Load<AudioClip>($"Audio/Voice/{dialogueId}");
        }

        /// <summary>
        /// 타이핑 사운드 재생
        /// </summary>
        private void PlayTypingSound()
        {
            if (sfxAudioSource != null && typingSound != null)
            {
                sfxAudioSource.PlayOneShot(typingSound);
            }
        }

        #endregion

        #region Effect & Reward

        /// <summary>
        /// 효과 적용
        /// </summary>
        private void ApplyEffect(DialogueEffect effect)
        {
            if (effect == null || effect.IsEmpty) return;

            // 의심도 변화
            if (effect.suspicion != 0)
            {
                // SuspicionManager.Instance?.AddSuspicion(effect.suspicion);
                Debug.Log($"[DialogueManager] Suspicion change: {effect.suspicion}");
            }

            // 야간 보너스
            if (effect.nightBonus != 0)
            {
                // TimeManager.Instance?.AddNightBonus(effect.nightBonus);
                Debug.Log($"[DialogueManager] Night bonus: {effect.nightBonus}");
            }

            // 디버프
            if (!string.IsNullOrEmpty(effect.debuff))
            {
                // DebuffManager.Instance?.ApplyDebuff(effect.debuff);
                Debug.Log($"[DialogueManager] Debuff applied: {effect.debuff}");
            }

            OnEffectApplied?.Invoke(effect);
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        private void GiveReward(DialogueReward reward)
        {
            if (reward == null) return;

            switch (reward.GetRewardType())
            {
                case RewardType.Gold:
                    // GameManager.Instance?.AddGold(reward.amount);
                    Debug.Log($"[DialogueManager] Gold reward: {reward.amount}");
                    break;

                case RewardType.HP:
                    // GameManager.Instance?.AddHP(reward.amount);
                    Debug.Log($"[DialogueManager] HP reward: {reward.amount}");
                    break;

                case RewardType.SkillPoint:
                    // SkillManager.Instance?.AddSkillPoints((int)reward.amount);
                    Debug.Log($"[DialogueManager] SkillPoint reward: {reward.amount}");
                    break;

                case RewardType.Item:
                    // InventoryManager.Instance?.AddItem(reward.itemId, reward.count);
                    Debug.Log($"[DialogueManager] Item reward: {reward.itemId} x{reward.count}");
                    break;
            }

            OnRewardGiven?.Invoke(reward);
        }

        /// <summary>
        /// 특수 액션 실행
        /// </summary>
        private void TriggerAction(string action, string param)
        {
            Debug.Log($"[DialogueManager] Action triggered: {action}, param: {param}");

            switch (action)
            {
                case "OpenShop":
                    // ShopManager.Instance?.OpenShop(param);
                    break;

                case "GameOver":
                    // GameManager.Instance?.TriggerGameOver(param);
                    break;

                case "ShowCredits":
                    // UIManager.Instance?.ShowCredits();
                    break;

                default:
                    Debug.LogWarning($"[DialogueManager] Unknown action: {action}");
                    break;
            }

            OnActionTriggered?.Invoke(action, param);
        }

        #endregion

        #region Dialogue Log

        /// <summary>
        /// 대화 로그에 추가
        /// </summary>
        private void AddToLog(DialogueData dialogue)
        {
            dialogueLog.Add(new DialogueLogEntry
            {
                speakerName = dialogue.characterName,
                dialogueText = dialogue.GetLocalizedText(useEnglish),
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });

            // 로그 최대 개수 제한
            if (dialogueLog.Count > 100)
            {
                dialogueLog.RemoveAt(0);
            }
        }

        /// <summary>
        /// 선택지를 로그에 추가
        /// </summary>
        private void AddChoiceToLog(DialogueChoice choice)
        {
            dialogueLog.Add(new DialogueLogEntry
            {
                speakerName = ">> 선택",
                dialogueText = choice.GetLocalizedText(useEnglish),
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        /// <summary>
        /// 로그 패널 표시
        /// </summary>
        public void ShowLogPanel()
        {
            // TODO: 로그 패널 UI 구현
            Debug.Log("[DialogueManager] Show log panel");
        }

        /// <summary>
        /// 로그 클리어
        /// </summary>
        public void ClearLog()
        {
            dialogueLog.Clear();
        }

        #endregion

        #region Utility

        /// <summary>
        /// 대화를 본 적 있는지 확인
        /// </summary>
        public bool HasSeenDialogue(string dialogueId)
        {
            return seenDialogues.Contains(dialogueId);
        }

        /// <summary>
        /// 언어 설정
        /// </summary>
        public void SetLanguage(bool english)
        {
            useEnglish = english;
        }

        /// <summary>
        /// 텍스트 속도 설정
        /// </summary>
        public void SetTextSpeed(float speed)
        {
            textSpeed = Mathf.Clamp(speed, 0.01f, 0.1f);
        }

        /// <summary>
        /// 자동 모드 딜레이 설정
        /// </summary>
        public void SetAutoModeDelay(float delay)
        {
            autoModeDelay = Mathf.Clamp(delay, 0.5f, 5.0f);
        }

        #endregion
    }

    /// <summary>
    /// 대화 로그 엔트리
    /// </summary>
    [Serializable]
    public class DialogueLogEntry
    {
        public string speakerName;
        public string dialogueText;
        public string timestamp;
    }
}
