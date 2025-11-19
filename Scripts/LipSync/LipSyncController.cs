using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace HermitHero.LipSync
{
    /// <summary>
    /// 립싱크 컨트롤러 - 캐릭터의 입 모양을 오디오에 맞춰 애니메이션
    /// </summary>
    public class LipSyncController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("립싱크를 적용할 캐릭터의 입 Sprite Renderer")]
        public SpriteRenderer mouthRenderer;

        [Tooltip("립싱크를 적용할 UI Image (UI 모드일 경우)")]
        public Image mouthImage;

        [Tooltip("오디오 소스")]
        public AudioSource audioSource;

        [Header("Lip Sync Settings")]
        [Tooltip("립싱크 모드")]
        public LipSyncMode syncMode = LipSyncMode.Phoneme;

        [Tooltip("립싱크 데이터베이스")]
        public LipSyncDatabase lipSyncDatabase;

        [Tooltip("기본 입 모양 (닫힌 입)")]
        public Sprite defaultMouth;

        [Tooltip("립싱크 속도 (낮을수록 빠름)")]
        [Range(0.01f, 0.2f)]
        public float lipSyncSpeed = 0.05f;

        [Header("Audio Analysis Settings")]
        [Tooltip("오디오 볼륨 기반 립싱크 사용 여부")]
        public bool useVolumeBasedSync = true;

        [Tooltip("볼륨 임계값 (이 값 이상일 때 입이 벌어짐)")]
        [Range(0f, 1f)]
        public float volumeThreshold = 0.1f;

        [Tooltip("볼륨 증폭 배율")]
        [Range(1f, 10f)]
        public float volumeMultiplier = 3f;

        [Header("Debug")]
        public bool showDebugInfo = false;

        // Private variables
        private LipSyncData currentLipSyncData;
        private Coroutine lipSyncCoroutine;
        private int currentPhonemeIndex = 0;
        private bool isPlaying = false;

        // Audio analysis
        private float[] audioSamples = new float[256];
        private float currentVolume = 0f;

        public enum LipSyncMode
        {
            Phoneme,        // 음소 기반
            Volume,         // 볼륨 기반
            Hybrid          // 혼합 (음소 + 볼륨)
        }

        void Start()
        {
            if (mouthRenderer == null && mouthImage == null)
            {
                Debug.LogError("LipSyncController: Mouth Renderer or Image not assigned!");
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            SetMouthSprite(defaultMouth);
        }

        void Update()
        {
            if (isPlaying && syncMode != LipSyncMode.Phoneme)
            {
                AnalyzeAudio();

                if (syncMode == LipSyncMode.Volume)
                {
                    UpdateVolumeBasedLipSync();
                }
            }

            if (showDebugInfo)
            {
                DebugDisplay();
            }
        }

        /// <summary>
        /// 립싱크 시작
        /// </summary>
        public void StartLipSync(string dialogueId, AudioClip audioClip)
        {
            StopLipSync();

            // 립싱크 데이터 로드
            currentLipSyncData = lipSyncDatabase.GetLipSyncData(dialogueId);

            if (currentLipSyncData == null)
            {
                Debug.LogWarning($"LipSyncController: No lip sync data found for {dialogueId}");

                // 데이터가 없으면 볼륨 기반으로 폴백
                if (useVolumeBasedSync)
                {
                    StartVolumeBasedLipSync(audioClip);
                }
                return;
            }

            // 오디오 재생
            if (audioClip != null && audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }

            // 립싱크 코루틴 시작
            isPlaying = true;
            currentPhonemeIndex = 0;

            switch (syncMode)
            {
                case LipSyncMode.Phoneme:
                    lipSyncCoroutine = StartCoroutine(PhonemeBasedLipSync());
                    break;

                case LipSyncMode.Hybrid:
                    lipSyncCoroutine = StartCoroutine(HybridLipSync());
                    break;

                case LipSyncMode.Volume:
                    StartVolumeBasedLipSync(audioClip);
                    break;
            }
        }

        /// <summary>
        /// 텍스트 기반 립싱크 시작 (오디오 없이)
        /// </summary>
        public void StartLipSyncFromText(string text, float duration)
        {
            StopLipSync();

            // 텍스트에서 음소 생성
            List<PhonemeData> phonemes = lipSyncDatabase.GeneratePhonemesFromText(text, duration);

            currentLipSyncData = new LipSyncData
            {
                dialogueId = "generated",
                phonemes = phonemes.ToArray(),
                duration = duration
            };

            isPlaying = true;
            currentPhonemeIndex = 0;
            lipSyncCoroutine = StartCoroutine(PhonemeBasedLipSync());
        }

        /// <summary>
        /// 립싱크 중지
        /// </summary>
        public void StopLipSync()
        {
            if (lipSyncCoroutine != null)
            {
                StopCoroutine(lipSyncCoroutine);
                lipSyncCoroutine = null;
            }

            isPlaying = false;
            currentPhonemeIndex = 0;
            currentLipSyncData = null;

            SetMouthSprite(defaultMouth);
        }

        /// <summary>
        /// 음소 기반 립싱크
        /// </summary>
        private IEnumerator PhonemeBasedLipSync()
        {
            float startTime = Time.time;

            while (currentPhonemeIndex < currentLipSyncData.phonemes.Length)
            {
                PhonemeData phoneme = currentLipSyncData.phonemes[currentPhonemeIndex];
                float currentTime = Time.time - startTime;

                // 음소 타이밍 체크
                if (currentTime >= phoneme.startTime)
                {
                    // 입 모양 변경
                    Sprite mouthSprite = lipSyncDatabase.GetMouthSprite(phoneme.phoneme);
                    SetMouthSprite(mouthSprite);

                    currentPhonemeIndex++;
                }

                yield return null;
            }

            // 립싱크 종료
            yield return new WaitForSeconds(0.2f);
            SetMouthSprite(defaultMouth);
            isPlaying = false;
        }

        /// <summary>
        /// 볼륨 기반 립싱크 시작
        /// </summary>
        private void StartVolumeBasedLipSync(AudioClip audioClip)
        {
            if (audioClip != null && audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }

            isPlaying = true;
            lipSyncCoroutine = StartCoroutine(VolumeBasedLipSyncCoroutine(audioClip != null ? audioClip.length : 3f));
        }

        /// <summary>
        /// 볼륨 기반 립싱크 코루틴
        /// </summary>
        private IEnumerator VolumeBasedLipSyncCoroutine(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetMouthSprite(defaultMouth);
            isPlaying = false;
        }

        /// <summary>
        /// 하이브리드 립싱크 (음소 + 볼륨)
        /// </summary>
        private IEnumerator HybridLipSync()
        {
            float startTime = Time.time;

            while (currentPhonemeIndex < currentLipSyncData.phonemes.Length)
            {
                PhonemeData phoneme = currentLipSyncData.phonemes[currentPhonemeIndex];
                float currentTime = Time.time - startTime;

                if (currentTime >= phoneme.startTime)
                {
                    Sprite mouthSprite = lipSyncDatabase.GetMouthSprite(phoneme.phoneme);

                    // 볼륨에 따라 입 크기 조절
                    float scale = 1f + (currentVolume * volumeMultiplier * 0.2f);
                    if (mouthRenderer != null)
                    {
                        mouthRenderer.transform.localScale = Vector3.one * scale;
                    }
                    else if (mouthImage != null)
                    {
                        mouthImage.transform.localScale = Vector3.one * scale;
                    }

                    SetMouthSprite(mouthSprite);
                    currentPhonemeIndex++;
                }

                yield return null;
            }

            yield return new WaitForSeconds(0.2f);
            ResetMouthScale();
            SetMouthSprite(defaultMouth);
            isPlaying = false;
        }

        /// <summary>
        /// 볼륨 기반 립싱크 업데이트
        /// </summary>
        private void UpdateVolumeBasedLipSync()
        {
            if (currentVolume > volumeThreshold)
            {
                // 볼륨에 따라 입 모양 선택
                MouthShape shape = GetMouthShapeFromVolume(currentVolume);
                Sprite mouthSprite = lipSyncDatabase.GetMouthSpriteByShape(shape);
                SetMouthSprite(mouthSprite);
            }
            else
            {
                SetMouthSprite(defaultMouth);
            }
        }

        /// <summary>
        /// 오디오 분석
        /// </summary>
        private void AnalyzeAudio()
        {
            if (audioSource == null || !audioSource.isPlaying)
            {
                currentVolume = 0f;
                return;
            }

            // 오디오 샘플 가져오기
            audioSource.GetOutputData(audioSamples, 0);

            // RMS (Root Mean Square) 계산
            float sum = 0f;
            for (int i = 0; i < audioSamples.Length; i++)
            {
                sum += audioSamples[i] * audioSamples[i];
            }

            currentVolume = Mathf.Sqrt(sum / audioSamples.Length);
        }

        /// <summary>
        /// 볼륨에 따른 입 모양 결정
        /// </summary>
        private MouthShape GetMouthShapeFromVolume(float volume)
        {
            float normalizedVolume = volume * volumeMultiplier;

            if (normalizedVolume < 0.2f)
                return MouthShape.Closed;
            else if (normalizedVolume < 0.4f)
                return MouthShape.Narrow;
            else if (normalizedVolume < 0.6f)
                return MouthShape.Medium;
            else if (normalizedVolume < 0.8f)
                return MouthShape.Wide;
            else
                return MouthShape.VeryWide;
        }

        /// <summary>
        /// 입 스프라이트 설정
        /// </summary>
        private void SetMouthSprite(Sprite sprite)
        {
            if (sprite == null) return;

            if (mouthRenderer != null)
            {
                mouthRenderer.sprite = sprite;
            }
            else if (mouthImage != null)
            {
                mouthImage.sprite = sprite;
            }
        }

        /// <summary>
        /// 입 크기 초기화
        /// </summary>
        private void ResetMouthScale()
        {
            if (mouthRenderer != null)
            {
                mouthRenderer.transform.localScale = Vector3.one;
            }
            else if (mouthImage != null)
            {
                mouthImage.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 디버그 정보 표시
        /// </summary>
        private void DebugDisplay()
        {
            if (isPlaying)
            {
                Debug.Log($"LipSync - Playing: {isPlaying}, Volume: {currentVolume:F3}, Phoneme: {currentPhonemeIndex}");
            }
        }

        void OnGUI()
        {
            if (showDebugInfo && isPlaying)
            {
                GUI.Box(new Rect(10, 10, 200, 100), "Lip Sync Debug");
                GUI.Label(new Rect(20, 30, 180, 20), $"Mode: {syncMode}");
                GUI.Label(new Rect(20, 50, 180, 20), $"Volume: {currentVolume:F3}");
                GUI.Label(new Rect(20, 70, 180, 20), $"Phoneme: {currentPhonemeIndex}");
            }
        }
    }

    /// <summary>
    /// 입 모양 열거형
    /// </summary>
    public enum MouthShape
    {
        Closed,         // 닫힌 입
        Narrow,         // 좁은 입
        Medium,         // 중간 입
        Wide,           // 넓은 입
        VeryWide,       // 매우 넓은 입
        O_Shape,        // O 모양
        Smile,          // 미소
        Sad             // 슬픔
    }

    /// <summary>
    /// 음소 데이터
    /// </summary>
    [System.Serializable]
    public class PhonemeData
    {
        public string phoneme;      // 음소 (예: "A", "E", "I", "O", "U")
        public float startTime;     // 시작 시간 (초)
        public float duration;      // 지속 시간 (초)
        public MouthShape shape;    // 입 모양
    }

    /// <summary>
    /// 립싱크 데이터
    /// </summary>
    [System.Serializable]
    public class LipSyncData
    {
        public string dialogueId;
        public PhonemeData[] phonemes;
        public float duration;
    }
}
