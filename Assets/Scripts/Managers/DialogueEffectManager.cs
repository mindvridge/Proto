using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 대화 연출 효과 매니저 - 화면 흔들림, 페이드, 플래시 등
    /// </summary>
    public class DialogueEffectManager : MonoBehaviour
    {
        public static DialogueEffectManager Instance { get; private set; }

        #region UI References
        [Header("=== UI References ===")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private Image flashImage;
        [SerializeField] private CanvasGroup dialogueCanvasGroup;
        #endregion

        #region Settings
        [Header("=== Default Settings ===")]
        [SerializeField] private float defaultFadeDuration = 0.5f;
        [SerializeField] private float defaultShakeDuration = 0.5f;
        [SerializeField] private float defaultShakeIntensity = 0.1f;
        [SerializeField] private float defaultFlashDuration = 0.3f;
        #endregion

        #region State
        private Transform cameraTransform;
        private Vector3 originalCameraPosition;
        private Coroutine currentShakeCoroutine;
        private Coroutine currentFadeCoroutine;
        private Coroutine currentFlashCoroutine;
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
            InitializeCamera();
            InitializeUI();
        }

        private void InitializeCamera()
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                originalCameraPosition = cameraTransform.localPosition;
            }
        }

        private void InitializeUI()
        {
            if (fadeImage != null)
            {
                fadeImage.gameObject.SetActive(false);
                SetImageAlpha(fadeImage, 0);
            }

            if (flashImage != null)
            {
                flashImage.gameObject.SetActive(false);
                SetImageAlpha(flashImage, 0);
            }
        }

        #region Fade Effects

        /// <summary>
        /// 페이드 인 (검은 화면에서 밝아짐)
        /// </summary>
        public void FadeIn(float duration = -1)
        {
            if (duration < 0) duration = defaultFadeDuration;
            StopFade();
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(1, 0, duration));
        }

        /// <summary>
        /// 페이드 아웃 (화면이 검게 됨)
        /// </summary>
        public void FadeOut(float duration = -1)
        {
            if (duration < 0) duration = defaultFadeDuration;
            StopFade();
            currentFadeCoroutine = StartCoroutine(FadeCoroutine(0, 1, duration));
        }

        /// <summary>
        /// 페이드 인/아웃 (검게 되었다가 밝아짐)
        /// </summary>
        public void FadeInOut(float duration = -1, System.Action onMidpoint = null)
        {
            if (duration < 0) duration = defaultFadeDuration * 2;
            StopFade();
            currentFadeCoroutine = StartCoroutine(FadeInOutCoroutine(duration, onMidpoint));
        }

        /// <summary>
        /// 페이드 중지
        /// </summary>
        public void StopFade()
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
        }

        private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
        {
            if (fadeImage == null) yield break;

            fadeImage.gameObject.SetActive(true);
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                SetImageAlpha(fadeImage, alpha);
                yield return null;
            }

            SetImageAlpha(fadeImage, endAlpha);

            if (endAlpha <= 0)
            {
                fadeImage.gameObject.SetActive(false);
            }

            currentFadeCoroutine = null;
        }

        private IEnumerator FadeInOutCoroutine(float totalDuration, System.Action onMidpoint)
        {
            float halfDuration = totalDuration / 2;

            // 페이드 아웃
            yield return FadeCoroutine(0, 1, halfDuration);

            // 중간 지점 콜백
            onMidpoint?.Invoke();

            // 페이드 인
            yield return FadeCoroutine(1, 0, halfDuration);

            currentFadeCoroutine = null;
        }

        #endregion

        #region Shake Effects

        /// <summary>
        /// 화면 흔들기
        /// </summary>
        public void Shake(float duration = -1, float intensity = -1)
        {
            if (duration < 0) duration = defaultShakeDuration;
            if (intensity < 0) intensity = defaultShakeIntensity;

            StopShake();
            currentShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
        }

        /// <summary>
        /// 강한 화면 흔들기
        /// </summary>
        public void HeavyShake(float duration = -1)
        {
            if (duration < 0) duration = defaultShakeDuration;
            Shake(duration, defaultShakeIntensity * 2);
        }

        /// <summary>
        /// 화면 흔들기 중지
        /// </summary>
        public void StopShake()
        {
            if (currentShakeCoroutine != null)
            {
                StopCoroutine(currentShakeCoroutine);
                currentShakeCoroutine = null;

                // 원래 위치로 복귀
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = originalCameraPosition;
                }
            }
        }

        private IEnumerator ShakeCoroutine(float duration, float intensity)
        {
            if (cameraTransform == null) yield break;

            float elapsed = 0;

            while (elapsed < duration)
            {
                float x = originalCameraPosition.x + Random.Range(-intensity, intensity);
                float y = originalCameraPosition.y + Random.Range(-intensity, intensity);

                cameraTransform.localPosition = new Vector3(x, y, originalCameraPosition.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cameraTransform.localPosition = originalCameraPosition;
            currentShakeCoroutine = null;
        }

        #endregion

        #region Flash Effects

        /// <summary>
        /// 화면 플래시 (하얀색)
        /// </summary>
        public void Flash(float duration = -1)
        {
            if (duration < 0) duration = defaultFlashDuration;
            FlashColor(Color.white, duration);
        }

        /// <summary>
        /// 빨간색 플래시 (데미지 효과)
        /// </summary>
        public void DamageFlash(float duration = -1)
        {
            if (duration < 0) duration = defaultFlashDuration;
            FlashColor(new Color(1, 0, 0, 0.3f), duration);
        }

        /// <summary>
        /// 지정 색상으로 플래시
        /// </summary>
        public void FlashColor(Color color, float duration = -1)
        {
            if (duration < 0) duration = defaultFlashDuration;
            StopFlash();
            currentFlashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
        }

        /// <summary>
        /// 플래시 중지
        /// </summary>
        public void StopFlash()
        {
            if (currentFlashCoroutine != null)
            {
                StopCoroutine(currentFlashCoroutine);
                currentFlashCoroutine = null;

                if (flashImage != null)
                {
                    flashImage.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator FlashCoroutine(Color color, float duration)
        {
            if (flashImage == null) yield break;

            flashImage.color = color;
            flashImage.gameObject.SetActive(true);

            float elapsed = 0;
            Color startColor = color;
            Color endColor = new Color(color.r, color.g, color.b, 0);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                flashImage.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }

            flashImage.gameObject.SetActive(false);
            currentFlashCoroutine = null;
        }

        #endregion

        #region Dialogue Panel Effects

        /// <summary>
        /// 대화창 펀치 효과 (강조)
        /// </summary>
        public void PunchDialoguePanel()
        {
            if (dialogueCanvasGroup == null) return;
            StartCoroutine(PunchScaleCoroutine(dialogueCanvasGroup.transform));
        }

        private IEnumerator PunchScaleCoroutine(Transform target)
        {
            Vector3 originalScale = target.localScale;
            Vector3 punchScale = originalScale * 1.05f;

            float duration = 0.1f;
            float elapsed = 0;

            // 확대
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / duration);
                yield return null;
            }

            elapsed = 0;

            // 축소 (원래대로)
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                target.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// 대화창 슬라이드 인
        /// </summary>
        public void SlideInDialoguePanel(float fromY = -200, float duration = 0.3f)
        {
            if (dialogueCanvasGroup == null) return;
            StartCoroutine(SlideCoroutine(dialogueCanvasGroup.transform, fromY, 0, duration));
        }

        /// <summary>
        /// 대화창 슬라이드 아웃
        /// </summary>
        public void SlideOutDialoguePanel(float toY = -200, float duration = 0.3f)
        {
            if (dialogueCanvasGroup == null) return;
            StartCoroutine(SlideCoroutine(dialogueCanvasGroup.transform, 0, toY, duration));
        }

        private IEnumerator SlideCoroutine(Transform target, float fromY, float toY, float duration)
        {
            RectTransform rectTransform = target as RectTransform;
            if (rectTransform == null) yield break;

            Vector2 anchoredPos = rectTransform.anchoredPosition;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float y = Mathf.Lerp(fromY, toY, Mathf.SmoothStep(0, 1, elapsed / duration));
                rectTransform.anchoredPosition = new Vector2(anchoredPos.x, y);
                yield return null;
            }

            rectTransform.anchoredPosition = new Vector2(anchoredPos.x, toY);
        }

        #endregion

        #region Character Effects

        /// <summary>
        /// 캐릭터 이미지 흔들기
        /// </summary>
        public void ShakeCharacter(Image characterImage, float duration = 0.3f, float intensity = 10f)
        {
            if (characterImage == null) return;
            StartCoroutine(ShakeUICoroutine(characterImage.rectTransform, duration, intensity));
        }

        /// <summary>
        /// 캐릭터 등장 효과
        /// </summary>
        public void CharacterAppear(Image characterImage, float duration = 0.3f)
        {
            if (characterImage == null) return;
            StartCoroutine(CharacterAppearCoroutine(characterImage, duration));
        }

        private IEnumerator ShakeUICoroutine(RectTransform target, float duration, float intensity)
        {
            Vector2 originalPos = target.anchoredPosition;
            float elapsed = 0;

            while (elapsed < duration)
            {
                float x = originalPos.x + Random.Range(-intensity, intensity);
                float y = originalPos.y + Random.Range(-intensity, intensity);
                target.anchoredPosition = new Vector2(x, y);

                elapsed += Time.deltaTime;
                yield return null;
            }

            target.anchoredPosition = originalPos;
        }

        private IEnumerator CharacterAppearCoroutine(Image target, float duration)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
            }

            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }

        #endregion

        #region Combo Effects

        /// <summary>
        /// 충격 효과 (흔들림 + 플래시)
        /// </summary>
        public void ImpactEffect()
        {
            Shake();
            Flash();
        }

        /// <summary>
        /// 드라마틱 전환 (페이드 + 흔들림)
        /// </summary>
        public void DramaticTransition(System.Action onMidpoint = null)
        {
            FadeInOut(1.0f, () =>
            {
                onMidpoint?.Invoke();
                Shake(0.3f, 0.15f);
            });
        }

        /// <summary>
        /// 보스 등장 효과
        /// </summary>
        public void BossAppearEffect()
        {
            StartCoroutine(BossAppearSequence());
        }

        private IEnumerator BossAppearSequence()
        {
            // 화면 어두워짐
            yield return FadeCoroutine(0, 0.7f, 0.5f);

            // 흔들림
            Shake(0.5f, 0.2f);

            yield return new WaitForSeconds(0.5f);

            // 플래시와 함께 밝아짐
            Flash(0.2f);
            yield return FadeCoroutine(0.7f, 0, 0.3f);
        }

        #endregion

        #region Utility

        private void SetImageAlpha(Image image, float alpha)
        {
            if (image == null) return;
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        /// <summary>
        /// 모든 효과 중지
        /// </summary>
        public void StopAllEffects()
        {
            StopFade();
            StopShake();
            StopFlash();
            StopAllCoroutines();
        }

        #endregion
    }
}
