using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AI로 생성된 캐릭터 스프라이트 데이터베이스
/// 포즈, 표정, 각도별로 캐릭터 이미지 관리
/// </summary>
[CreateAssetMenu(fileName = "CharacterSpriteDB", menuName = "Game/Character Sprite Database")]
public class CharacterSpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public class CharacterSprite
    {
        public string spriteId;
        public Sprite sprite;
        public CharacterPose pose;
        public CharacterEmotion emotion;
        public CharacterAngle angle;
        [TextArea(2, 4)]
        public string description;
    }

    [System.Serializable]
    public enum CharacterPose
    {
        None,
        Idle,           // 대기
        Walk,           // 걷기
        Run,            // 달리기
        Attack,         // 공격
        Defend,         // 방어
        Hit,            // 피격
        Victory,        // 승리
        Defeat,         // 패배
        Jump,           // 점프
        Sit,            // 앉기
        Crouch,         // 웅크리기
        Climb,          // 오르기
        Slide,          // 슬라이드
        Cast,           // 마법 시전
        Item,           // 아이템 사용
        Custom          // 커스텀
    }

    [System.Serializable]
    public enum CharacterEmotion
    {
        None,
        Neutral,        // 중립
        Happy,          // 행복
        Sad,            // 슬픔
        Angry,          // 분노
        Surprised,      // 놀람
        Confused,       // 혼란
        Determined,     // 결의
        Tired,          // 피곤
        Excited,        // 흥분
        Disappointed,   // 실망
        Proud,          // 자랑스러움
        Shy,            // 수줍음
        Custom          // 커스텀
    }

    [System.Serializable]
    public enum CharacterAngle
    {
        None,
        Front,          // 정면
        Side,           // 측면
        Back,           // 뒷면
        ThreeQuarter,   // 3/4 각도
        TopDown,        // 위에서 아래
        Custom          // 커스텀
    }

    [Header("캐릭터 정보")]
    public string characterId;
    public string characterName;
    [TextArea(3, 6)]
    public string description;

    [Header("스프라이트 데이터")]
    public List<CharacterSprite> sprites = new List<CharacterSprite>();

    [Header("기본 스프라이트")]
    public Sprite defaultSprite;
    public Sprite portraitSprite;
    public Sprite iconSprite;

    #region 스프라이트 검색

    /// <summary>
    /// 포즈로 스프라이트 찾기
    /// </summary>
    public Sprite GetSpriteByPose(CharacterPose pose)
    {
        var sprite = sprites.FirstOrDefault(s => s.pose == pose);
        return sprite?.sprite ?? defaultSprite;
    }

    /// <summary>
    /// 표정으로 스프라이트 찾기
    /// </summary>
    public Sprite GetSpriteByEmotion(CharacterEmotion emotion)
    {
        var sprite = sprites.FirstOrDefault(s => s.emotion == emotion);
        return sprite?.sprite ?? defaultSprite;
    }

    /// <summary>
    /// 각도로 스프라이트 찾기
    /// </summary>
    public Sprite GetSpriteByAngle(CharacterAngle angle)
    {
        var sprite = sprites.FirstOrDefault(s => s.angle == angle);
        return sprite?.sprite ?? defaultSprite;
    }

    /// <summary>
    /// 포즈 + 표정 조합으로 스프라이트 찾기
    /// </summary>
    public Sprite GetSprite(CharacterPose pose, CharacterEmotion emotion)
    {
        var sprite = sprites.FirstOrDefault(s => s.pose == pose && s.emotion == emotion);
        return sprite?.sprite ?? GetSpriteByPose(pose);
    }

    /// <summary>
    /// 포즈 + 표정 + 각도 조합으로 스프라이트 찾기
    /// </summary>
    public Sprite GetSprite(CharacterPose pose, CharacterEmotion emotion, CharacterAngle angle)
    {
        var sprite = sprites.FirstOrDefault(s =>
            s.pose == pose &&
            s.emotion == emotion &&
            s.angle == angle
        );

        if (sprite != null) return sprite.sprite;

        // 폴백: 포즈 + 표정
        return GetSprite(pose, emotion);
    }

    /// <summary>
    /// ID로 스프라이트 찾기
    /// </summary>
    public Sprite GetSpriteById(string spriteId)
    {
        var sprite = sprites.FirstOrDefault(s => s.spriteId == spriteId);
        return sprite?.sprite ?? defaultSprite;
    }

    /// <summary>
    /// 모든 포즈 스프라이트 가져오기
    /// </summary>
    public List<Sprite> GetAllPoseSprites(CharacterPose pose)
    {
        return sprites
            .Where(s => s.pose == pose)
            .Select(s => s.sprite)
            .ToList();
    }

    /// <summary>
    /// 모든 표정 스프라이트 가져오기
    /// </summary>
    public List<Sprite> GetAllEmotionSprites(CharacterEmotion emotion)
    {
        return sprites
            .Where(s => s.emotion == emotion)
            .Select(s => s.sprite)
            .ToList();
    }

    #endregion

    #region 애니메이션 지원

    /// <summary>
    /// 포즈 애니메이션용 프레임 가져오기
    /// </summary>
    public Sprite[] GetAnimationFrames(CharacterPose pose, int frameCount = -1)
    {
        var frames = GetAllPoseSprites(pose).ToArray();

        if (frameCount > 0 && frames.Length > frameCount)
        {
            return frames.Take(frameCount).ToArray();
        }

        return frames;
    }

    #endregion

    #region 유틸리티

    /// <summary>
    /// 스프라이트 추가
    /// </summary>
    public void AddSprite(string id, Sprite sprite, CharacterPose pose,
                          CharacterEmotion emotion = CharacterEmotion.None,
                          CharacterAngle angle = CharacterAngle.None)
    {
        var characterSprite = new CharacterSprite
        {
            spriteId = id,
            sprite = sprite,
            pose = pose,
            emotion = emotion,
            angle = angle
        };

        sprites.Add(characterSprite);
    }

    /// <summary>
    /// 스프라이트 제거
    /// </summary>
    public bool RemoveSpriteById(string spriteId)
    {
        var sprite = sprites.FirstOrDefault(s => s.spriteId == spriteId);
        if (sprite != null)
        {
            sprites.Remove(sprite);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 스프라이트 존재 확인
    /// </summary>
    public bool HasSprite(CharacterPose pose, CharacterEmotion emotion = CharacterEmotion.None)
    {
        if (emotion == CharacterEmotion.None)
        {
            return sprites.Any(s => s.pose == pose);
        }
        return sprites.Any(s => s.pose == pose && s.emotion == emotion);
    }

    /// <summary>
    /// 통계 정보
    /// </summary>
    public void LogStatistics()
    {
        Debug.Log($"=== {characterName} 스프라이트 통계 ===");
        Debug.Log($"총 스프라이트 수: {sprites.Count}");

        // 포즈별 카운트
        var poseCounts = sprites
            .GroupBy(s => s.pose)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
        Debug.Log("포즈별: " + string.Join(", ", poseCounts));

        // 표정별 카운트
        var emotionCounts = sprites
            .GroupBy(s => s.emotion)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
        Debug.Log("표정별: " + string.Join(", ", emotionCounts));

        // 각도별 카운트
        var angleCounts = sprites
            .GroupBy(s => s.angle)
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();
        Debug.Log("각도별: " + string.Join(", ", angleCounts));
    }

    #endregion

    #region 에디터 지원

#if UNITY_EDITOR
    /// <summary>
    /// 폴더에서 자동으로 스프라이트 임포트
    /// </summary>
    [ContextMenu("Import Sprites from Folder")]
    public void ImportSpritesFromFolder()
    {
        string folderPath = UnityEditor.EditorUtility.OpenFolderPanel(
            "Select Character Sprites Folder",
            "Assets/GeneratedCharacters",
            ""
        );

        if (string.IsNullOrEmpty(folderPath))
            return;

        // Assets 폴더 기준 상대 경로로 변환
        if (!folderPath.StartsWith(Application.dataPath))
        {
            Debug.LogError("폴더는 Assets 폴더 내에 있어야 합니다.");
            return;
        }

        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        // 폴더 내 모든 스프라이트 찾기
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { relativePath });

        sprites.Clear();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                // 파일명에서 포즈, 표정 파싱
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                ParseSpriteInfo(fileName, sprite);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();

        Debug.Log($"✓ {sprites.Count}개 스프라이트 임포트 완료");
        LogStatistics();
    }

    /// <summary>
    /// 파일명에서 포즈/표정 정보 파싱
    /// 예: "minsu_idle_happy_front.png"
    /// </summary>
    private void ParseSpriteInfo(string fileName, Sprite sprite)
    {
        var parts = fileName.ToLower().Split('_');

        CharacterPose pose = CharacterPose.None;
        CharacterEmotion emotion = CharacterEmotion.None;
        CharacterAngle angle = CharacterAngle.None;

        foreach (var part in parts)
        {
            // 포즈 파싱
            if (System.Enum.TryParse(part, true, out CharacterPose parsedPose))
                pose = parsedPose;

            // 표정 파싱
            if (System.Enum.TryParse(part, true, out CharacterEmotion parsedEmotion))
                emotion = parsedEmotion;

            // 각도 파싱
            if (part == "front") angle = CharacterAngle.Front;
            else if (part == "side") angle = CharacterAngle.Side;
            else if (part == "back") angle = CharacterAngle.Back;
            else if (part == "threequarter" || part == "3quarter") angle = CharacterAngle.ThreeQuarter;
        }

        AddSprite(fileName, sprite, pose, emotion, angle);
    }
#endif

    #endregion
}
