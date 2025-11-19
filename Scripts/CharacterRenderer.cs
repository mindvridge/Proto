using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// AI 생성 캐릭터 렌더러
/// 포즈, 표정, 각도를 동적으로 변경하며 렌더링
/// </summary>
[RequireComponent(typeof(Image))]
public class CharacterRenderer : MonoBehaviour
{
    [Header("캐릭터 데이터")]
    [SerializeField] private CharacterSpriteDatabase spriteDatabase;

    [Header("현재 상태")]
    [SerializeField] private CharacterSpriteDatabase.CharacterPose currentPose = CharacterSpriteDatabase.CharacterPose.Idle;
    [SerializeField] private CharacterSpriteDatabase.CharacterEmotion currentEmotion = CharacterSpriteDatabase.CharacterEmotion.Neutral;
    [SerializeField] private CharacterSpriteDatabase.CharacterAngle currentAngle = CharacterSpriteDatabase.CharacterAngle.Front;

    [Header("애니메이션 설정")]
    [SerializeField] private bool autoAnimate = true;
    [SerializeField] private float frameRate = 10f; // FPS
    [SerializeField] private bool loop = true;

    [Header("전환 효과")]
    [SerializeField] private bool useFadeTransition = true;
    [SerializeField] private float transitionDuration = 0.2f;

    private Image imageComponent;
    private Coroutine animationCoroutine;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        imageComponent = GetComponent<Image>();
    }

    void Start()
    {
        if (spriteDatabase != null)
        {
            UpdateSprite();
        }
    }

    #region 공개 API

    /// <summary>
    /// 스프라이트 데이터베이스 설정
    /// </summary>
    public void SetDatabase(CharacterSpriteDatabase database)
    {
        spriteDatabase = database;
        UpdateSprite();
    }

    /// <summary>
    /// 포즈 변경
    /// </summary>
    public void SetPose(CharacterSpriteDatabase.CharacterPose pose, bool animate = true)
    {
        if (currentPose == pose) return;

        currentPose = pose;

        if (animate && autoAnimate)
        {
            PlayPoseAnimation(pose);
        }
        else
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 표정 변경
    /// </summary>
    public void SetEmotion(CharacterSpriteDatabase.CharacterEmotion emotion, bool smooth = true)
    {
        if (currentEmotion == emotion) return;

        currentEmotion = emotion;

        if (smooth && useFadeTransition)
        {
            TransitionToSprite(GetCurrentSprite());
        }
        else
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 각도 변경
    /// </summary>
    public void SetAngle(CharacterSpriteDatabase.CharacterAngle angle, bool smooth = true)
    {
        if (currentAngle == angle) return;

        currentAngle = angle;

        if (smooth && useFadeTransition)
        {
            TransitionToSprite(GetCurrentSprite());
        }
        else
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 포즈 + 표정 동시 변경
    /// </summary>
    public void SetState(CharacterSpriteDatabase.CharacterPose pose,
                         CharacterSpriteDatabase.CharacterEmotion emotion,
                         bool animate = true)
    {
        currentPose = pose;
        currentEmotion = emotion;

        if (animate && autoAnimate)
        {
            PlayPoseAnimation(pose);
        }
        else
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 포즈 + 표정 + 각도 동시 변경
    /// </summary>
    public void SetState(CharacterSpriteDatabase.CharacterPose pose,
                         CharacterSpriteDatabase.CharacterEmotion emotion,
                         CharacterSpriteDatabase.CharacterAngle angle,
                         bool animate = true)
    {
        currentPose = pose;
        currentEmotion = emotion;
        currentAngle = angle;

        if (animate && autoAnimate)
        {
            PlayPoseAnimation(pose);
        }
        else
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 특정 스프라이트 ID로 직접 설정
    /// </summary>
    public void SetSpriteById(string spriteId, bool smooth = true)
    {
        if (spriteDatabase == null) return;

        Sprite sprite = spriteDatabase.GetSpriteById(spriteId);

        if (smooth && useFadeTransition)
        {
            TransitionToSprite(sprite);
        }
        else
        {
            imageComponent.sprite = sprite;
        }
    }

    /// <summary>
    /// 포즈 애니메이션 재생
    /// </summary>
    public void PlayPoseAnimation(CharacterSpriteDatabase.CharacterPose pose)
    {
        if (spriteDatabase == null) return;

        // 기존 애니메이션 중지
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimatePose(pose));
    }

    /// <summary>
    /// 애니메이션 중지
    /// </summary>
    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        UpdateSprite();
    }

    #endregion

    #region 내부 메서드

    /// <summary>
    /// 현재 상태에 맞는 스프라이트 가져오기
    /// </summary>
    private Sprite GetCurrentSprite()
    {
        if (spriteDatabase == null)
        {
            Debug.LogWarning("CharacterSpriteDatabase가 설정되지 않았습니다.");
            return null;
        }

        return spriteDatabase.GetSprite(currentPose, currentEmotion, currentAngle);
    }

    /// <summary>
    /// 스프라이트 업데이트
    /// </summary>
    private void UpdateSprite()
    {
        Sprite sprite = GetCurrentSprite();
        if (sprite != null)
        {
            imageComponent.sprite = sprite;
        }
    }

    /// <summary>
    /// 부드러운 전환 효과
    /// </summary>
    private void TransitionToSprite(Sprite targetSprite)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(FadeTransition(targetSprite));
    }

    #endregion

    #region 코루틴

    /// <summary>
    /// 포즈 애니메이션 코루틴
    /// </summary>
    private IEnumerator AnimatePose(CharacterSpriteDatabase.CharacterPose pose)
    {
        Sprite[] frames = spriteDatabase.GetAnimationFrames(pose);

        if (frames == null || frames.Length == 0)
        {
            // 프레임이 없으면 정적 이미지로 표시
            UpdateSprite();
            yield break;
        }

        float frameDuration = 1f / frameRate;
        int frameIndex = 0;

        do
        {
            imageComponent.sprite = frames[frameIndex];
            frameIndex = (frameIndex + 1) % frames.Length;

            yield return new WaitForSeconds(frameDuration);
        }
        while (loop);

        animationCoroutine = null;
    }

    /// <summary>
    /// 페이드 전환 코루틴
    /// </summary>
    private IEnumerator FadeTransition(Sprite targetSprite)
    {
        if (targetSprite == null)
        {
            transitionCoroutine = null;
            yield break;
        }

        Color originalColor = imageComponent.color;
        float elapsed = 0f;

        // 페이드 아웃
        while (elapsed < transitionDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / (transitionDuration / 2f));
            imageComponent.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha
            );
            yield return null;
        }

        // 스프라이트 변경
        imageComponent.sprite = targetSprite;

        elapsed = 0f;

        // 페이드 인
        while (elapsed < transitionDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / (transitionDuration / 2f);
            imageComponent.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                alpha
            );
            yield return null;
        }

        // 원래 색상으로 복원
        imageComponent.color = originalColor;
        transitionCoroutine = null;
    }

    #endregion

    #region 편의 메서드

    /// <summary>
    /// 전투 상태 표시
    /// </summary>
    public void ShowBattleState(string action)
    {
        switch (action.ToLower())
        {
            case "attack":
                SetPose(CharacterSpriteDatabase.CharacterPose.Attack);
                break;
            case "defend":
                SetPose(CharacterSpriteDatabase.CharacterPose.Defend);
                break;
            case "hit":
                SetPose(CharacterSpriteDatabase.CharacterPose.Hit);
                break;
            case "victory":
                SetState(CharacterSpriteDatabase.CharacterPose.Victory,
                        CharacterSpriteDatabase.CharacterEmotion.Happy);
                break;
            case "defeat":
                SetState(CharacterSpriteDatabase.CharacterPose.Defeat,
                        CharacterSpriteDatabase.CharacterEmotion.Sad);
                break;
            default:
                SetPose(CharacterSpriteDatabase.CharacterPose.Idle);
                break;
        }
    }

    /// <summary>
    /// 감정 표현
    /// </summary>
    public void ExpressEmotion(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "happy":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Happy);
                break;
            case "sad":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Sad);
                break;
            case "angry":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Angry);
                break;
            case "surprised":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Surprised);
                break;
            case "confused":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Confused);
                break;
            case "determined":
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Determined);
                break;
            default:
                SetEmotion(CharacterSpriteDatabase.CharacterEmotion.Neutral);
                break;
        }
    }

    /// <summary>
    /// 대기 상태로 복귀
    /// </summary>
    public void ResetToIdle()
    {
        SetState(
            CharacterSpriteDatabase.CharacterPose.Idle,
            CharacterSpriteDatabase.CharacterEmotion.Neutral,
            false
        );
    }

    #endregion

    #region 디버그

    /// <summary>
    /// 현재 상태 로그
    /// </summary>
    [ContextMenu("Log Current State")]
    public void LogCurrentState()
    {
        Debug.Log($"=== {spriteDatabase?.characterName ?? "Unknown"} 상태 ===");
        Debug.Log($"포즈: {currentPose}");
        Debug.Log($"표정: {currentEmotion}");
        Debug.Log($"각도: {currentAngle}");
        Debug.Log($"스프라이트: {imageComponent.sprite?.name ?? "None"}");
    }

    /// <summary>
    /// 모든 포즈 순차 테스트
    /// </summary>
    [ContextMenu("Test All Poses")]
    public void TestAllPoses()
    {
        StartCoroutine(TestPosesSequence());
    }

    private IEnumerator TestPosesSequence()
    {
        var poses = System.Enum.GetValues(typeof(CharacterSpriteDatabase.CharacterPose));

        foreach (CharacterSpriteDatabase.CharacterPose pose in poses)
        {
            if (pose == CharacterSpriteDatabase.CharacterPose.None) continue;

            Debug.Log($"테스트 중: {pose}");
            SetPose(pose, true);

            yield return new WaitForSeconds(2f);
        }

        ResetToIdle();
        Debug.Log("포즈 테스트 완료");
    }

    #endregion
}
