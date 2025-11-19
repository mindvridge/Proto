using UnityEngine;
using System.Collections.Generic;

namespace HermitHero.LipSync
{
    /// <summary>
    /// 오디오 분석 기반 자동 립싱크 시스템
    /// </summary>
    public class AudioLipSyncAnalyzer : MonoBehaviour
    {
        [Header("Analysis Settings")]
        [Tooltip("FFT 샘플 수 (높을수록 정확하지만 무거움)")]
        public int fftSize = 1024;

        [Tooltip("주파수 대역 수")]
        public int frequencyBands = 8;

        [Tooltip("볼륨 증폭 배율")]
        [Range(1f, 50f)]
        public float volumeAmplifier = 10f;

        [Header("Phoneme Detection")]
        [Tooltip("자음 감지 주파수 범위 (Hz)")]
        public Vector2 consonantFrequencyRange = new Vector2(2000f, 8000f);

        [Tooltip("모음 감지 주파수 범위 (Hz)")]
        public Vector2 vowelFrequencyRange = new Vector2(200f, 2000f);

        [Tooltip("저주파 모음 감지 (Hz)")]
        public Vector2 lowVowelRange = new Vector2(200f, 500f);

        [Tooltip("중주파 모음 감지 (Hz)")]
        public Vector2 midVowelRange = new Vector2(500f, 1200f);

        [Tooltip("고주파 모음 감지 (Hz)")]
        public Vector2 highVowelRange = new Vector2(1200f, 2000f);

        [Header("Detection Thresholds")]
        [Tooltip("모음 'A' 감지 임계값")]
        [Range(0f, 1f)]
        public float aThreshold = 0.6f;

        [Tooltip("모음 'E' 감지 임계값")]
        [Range(0f, 1f)]
        public float eThreshold = 0.5f;

        [Tooltip("모음 'I' 감지 임계값")]
        [Range(0f, 1f)]
        public float iThreshold = 0.4f;

        [Tooltip("모음 'O/U' 감지 임계값")]
        [Range(0f, 1f)]
        public float oThreshold = 0.5f;

        [Tooltip("자음 감지 임계값")]
        [Range(0f, 1f)]
        public float consonantThreshold = 0.3f;

        // Private variables
        private float[] audioSpectrum;
        private float[] frequencyBandBuffer;
        private float[] bandVolumes;
        private AudioSource audioSource;

        // Detection results
        private float lowVowelEnergy = 0f;
        private float midVowelEnergy = 0f;
        private float highVowelEnergy = 0f;
        private float consonantEnergy = 0f;
        private float overallVolume = 0f;

        void Start()
        {
            audioSpectrum = new float[fftSize];
            frequencyBandBuffer = new float[frequencyBands];
            bandVolumes = new float[frequencyBands];

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioLipSyncAnalyzer: AudioSource not found!");
            }
        }

        void Update()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                AnalyzeAudio();
            }
        }

        /// <summary>
        /// 오디오 분석 및 음소 감지
        /// </summary>
        private void AnalyzeAudio()
        {
            // FFT 스펙트럼 데이터 가져오기
            audioSource.GetSpectrumData(audioSpectrum, 0, FFTWindow.BlackmanHarris);

            // 주파수 대역별 분석
            AnalyzeFrequencyBands();

            // 음소 감지
            DetectPhonemes();

            // 전체 볼륨 계산
            CalculateOverallVolume();
        }

        /// <summary>
        /// 주파수 대역별 분석
        /// </summary>
        private void AnalyzeFrequencyBands()
        {
            int count = 0;

            for (int i = 0; i < frequencyBands; i++)
            {
                float average = 0f;
                int sampleCount = (int)Mathf.Pow(2, i) * 2;

                if (i == frequencyBands - 1)
                {
                    sampleCount += 2;
                }

                for (int j = 0; j < sampleCount; j++)
                {
                    if (count < audioSpectrum.Length)
                    {
                        average += audioSpectrum[count] * (count + 1);
                        count++;
                    }
                }

                average /= count;

                // 버퍼 처리 (부드러운 전환)
                if (frequencyBandBuffer[i] < average)
                {
                    frequencyBandBuffer[i] = average;
                }
                else
                {
                    frequencyBandBuffer[i] -= Time.deltaTime * 0.5f;
                }

                // 볼륨 정규화
                bandVolumes[i] = frequencyBandBuffer[i] * volumeAmplifier;
            }
        }

        /// <summary>
        /// 음소 감지
        /// </summary>
        private void DetectPhonemes()
        {
            // 저주파 에너지 (A, O 같은 넓은 모음)
            lowVowelEnergy = GetFrequencyRangeEnergy(lowVowelRange.x, lowVowelRange.y);

            // 중주파 에너지 (E 같은 중간 모음)
            midVowelEnergy = GetFrequencyRangeEnergy(midVowelRange.x, midVowelRange.y);

            // 고주파 에너지 (I 같은 좁은 모음)
            highVowelEnergy = GetFrequencyRangeEnergy(highVowelRange.x, highVowelRange.y);

            // 자음 에너지 (S, T, K 같은 자음)
            consonantEnergy = GetFrequencyRangeEnergy(consonantFrequencyRange.x, consonantFrequencyRange.y);
        }

        /// <summary>
        /// 특정 주파수 범위의 에너지 계산
        /// </summary>
        private float GetFrequencyRangeEnergy(float minFreq, float maxFreq)
        {
            int minIndex = FrequencyToSpectrumIndex(minFreq);
            int maxIndex = FrequencyToSpectrumIndex(maxFreq);

            float energy = 0f;
            int count = 0;

            for (int i = minIndex; i < maxIndex && i < audioSpectrum.Length; i++)
            {
                energy += audioSpectrum[i];
                count++;
            }

            return count > 0 ? (energy / count) * volumeAmplifier : 0f;
        }

        /// <summary>
        /// 주파수를 스펙트럼 인덱스로 변환
        /// </summary>
        private int FrequencyToSpectrumIndex(float frequency)
        {
            float sampleRate = AudioSettings.outputSampleRate;
            return Mathf.RoundToInt(frequency / (sampleRate / 2f) * audioSpectrum.Length);
        }

        /// <summary>
        /// 전체 볼륨 계산
        /// </summary>
        private void CalculateOverallVolume()
        {
            overallVolume = 0f;
            foreach (float volume in bandVolumes)
            {
                overallVolume += volume;
            }
            overallVolume /= frequencyBands;
        }

        /// <summary>
        /// 현재 음소 추측
        /// </summary>
        public string GetCurrentPhoneme()
        {
            // 자음이 강하면
            if (consonantEnergy > consonantThreshold)
            {
                if (highVowelEnergy > midVowelEnergy)
                    return "S";  // 치찰음
                else
                    return "M";  // 입술음
            }

            // 모음 감지
            if (lowVowelEnergy > aThreshold)
            {
                return "A";  // 넓은 입
            }
            else if (highVowelEnergy > iThreshold)
            {
                return "I";  // 좁은 입
            }
            else if (midVowelEnergy > eThreshold)
            {
                return "E";  // 중간 입
            }
            else if (lowVowelEnergy > oThreshold && lowVowelEnergy < aThreshold)
            {
                return "O";  // O 모양
            }

            // 기본값 (입 닫힘)
            return "Closed";
        }

        /// <summary>
        /// 현재 입 모양 추측
        /// </summary>
        public MouthShape GetCurrentMouthShape()
        {
            string phoneme = GetCurrentPhoneme();

            switch (phoneme)
            {
                case "A":
                    return overallVolume > 0.8f ? MouthShape.VeryWide : MouthShape.Wide;

                case "E":
                    return MouthShape.Medium;

                case "I":
                    return MouthShape.Narrow;

                case "O":
                case "U":
                    return MouthShape.O_Shape;

                case "M":
                case "Closed":
                    return MouthShape.Closed;

                case "S":
                    return MouthShape.Narrow;

                default:
                    return MouthShape.Medium;
            }
        }

        /// <summary>
        /// 입 벌림 정도 (0.0 ~ 1.0)
        /// </summary>
        public float GetMouthOpenness()
        {
            return Mathf.Clamp01(overallVolume);
        }

        /// <summary>
        /// 립싱크 데이터 자동 생성
        /// </summary>
        public LipSyncData GenerateLipSyncData(AudioClip audioClip, string dialogueId, float samplingInterval = 0.1f)
        {
            if (audioClip == null)
            {
                Debug.LogError("AudioLipSyncAnalyzer: AudioClip is null!");
                return null;
            }

            List<PhonemeData> phonemes = new List<PhonemeData>();

            float duration = audioClip.length;
            float currentTime = 0f;

            // 임시 오디오 소스 생성
            GameObject tempObj = new GameObject("TempAudioAnalyzer");
            AudioSource tempSource = tempObj.AddComponent<AudioSource>();
            tempSource.clip = audioClip;
            tempSource.Play();

            // 샘플링하며 음소 감지
            while (currentTime < duration)
            {
                yield return new WaitForSeconds(samplingInterval);

                string phoneme = GetCurrentPhoneme();
                MouthShape shape = GetCurrentMouthShape();

                // 이전 음소와 다르면 새로운 음소 추가
                if (phonemes.Count == 0 || phonemes[phonemes.Count - 1].phoneme != phoneme)
                {
                    PhonemeData data = new PhonemeData
                    {
                        phoneme = phoneme,
                        shape = shape,
                        startTime = currentTime,
                        duration = samplingInterval
                    };

                    phonemes.Add(data);
                }
                else
                {
                    // 같은 음소면 지속 시간 연장
                    phonemes[phonemes.Count - 1].duration += samplingInterval;
                }

                currentTime += samplingInterval;
            }

            // 임시 오브젝트 제거
            Destroy(tempObj);

            // 립싱크 데이터 생성
            LipSyncData lipSyncData = new LipSyncData
            {
                dialogueId = dialogueId,
                phonemes = phonemes.ToArray(),
                duration = duration
            };

            return lipSyncData;
        }

        /// <summary>
        /// 디버그 정보 표시
        /// </summary>
        void OnGUI()
        {
            if (Debug.isDebugBuild)
            {
                int y = 150;
                GUI.Label(new Rect(10, y, 300, 20), $"Overall Volume: {overallVolume:F3}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"Low Vowel: {lowVowelEnergy:F3}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"Mid Vowel: {midVowelEnergy:F3}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"High Vowel: {highVowelEnergy:F3}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"Consonant: {consonantEnergy:F3}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"Detected Phoneme: {GetCurrentPhoneme()}");
                y += 20;
                GUI.Label(new Rect(10, y, 300, 20), $"Mouth Shape: {GetCurrentMouthShape()}");
            }
        }

        /// <summary>
        /// 주파수 대역 볼륨 가져오기
        /// </summary>
        public float GetBandVolume(int bandIndex)
        {
            if (bandIndex >= 0 && bandIndex < bandVolumes.Length)
            {
                return bandVolumes[bandIndex];
            }
            return 0f;
        }

        /// <summary>
        /// 모든 주파수 대역 볼륨 가져오기
        /// </summary>
        public float[] GetAllBandVolumes()
        {
            return (float[])bandVolumes.Clone();
        }
    }
}
