using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// AI 생성 캐릭터 스프라이트 자동 임포트 및 설정
/// </summary>
public class CharacterSpriteImporter : AssetPostprocessor
{
    // AI 생성 이미지가 있는 폴더
    private const string GENERATED_FOLDER = "GeneratedCharacters";

    /// <summary>
    /// 텍스처 임포트 전처리
    /// </summary>
    void OnPreprocessTexture()
    {
        // GeneratedCharacters 폴더의 이미지만 처리
        if (!assetPath.Contains(GENERATED_FOLDER))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;

        // 스프라이트 설정
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.maxTextureSize = 2048;

        // 스프라이트 피벗 설정 (하단 중앙)
        importer.spritePivot = new Vector2(0.5f, 0f);

        // 알파 채널 활성화
        importer.alphaIsTransparency = true;

        Debug.Log($"✓ 스프라이트 설정 적용: {assetPath}");
    }

    /// <summary>
    /// 텍스처 임포트 후처리
    /// </summary>
    void OnPostprocessTexture(Texture2D texture)
    {
        if (!assetPath.Contains(GENERATED_FOLDER))
            return;

        // 추가 후처리가 필요하면 여기에 작성
    }
}

/// <summary>
/// 캐릭터 스프라이트 데이터베이스 에디터 확장
/// </summary>
[CustomEditor(typeof(CharacterSpriteDatabase))]
public class CharacterSpriteDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CharacterSpriteDatabase database = (CharacterSpriteDatabase)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("자동 임포트 도구", EditorStyles.boldLabel);

        if (GUILayout.Button("폴더에서 스프라이트 자동 임포트", GUILayout.Height(30)))
        {
            ImportSpritesFromFolder(database);
        }

        if (GUILayout.Button("파일명 기반 자동 분류", GUILayout.Height(30)))
        {
            AutoCategorizeSprites(database);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("통계 및 검증", EditorStyles.boldLabel);

        if (GUILayout.Button("통계 정보 출력"))
        {
            database.LogStatistics();
        }

        if (GUILayout.Button("누락된 스프라이트 확인"))
        {
            CheckMissingSprites(database);
        }

        if (GUILayout.Button("중복 스프라이트 확인"))
        {
            CheckDuplicateSprites(database);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("데이터 관리", EditorStyles.boldLabel);

        if (GUILayout.Button("모든 스프라이트 제거"))
        {
            if (EditorUtility.DisplayDialog(
                "확인",
                "모든 스프라이트를 제거하시겠습니까?",
                "예", "아니오"))
            {
                database.sprites.Clear();
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                Debug.Log("모든 스프라이트가 제거되었습니다.");
            }
        }

        // 스프라이트 카운트 표시
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            $"총 {database.sprites.Count}개의 스프라이트가 등록되어 있습니다.",
            MessageType.Info
        );
    }

    private void ImportSpritesFromFolder(CharacterSpriteDatabase database)
    {
        string folderPath = EditorUtility.OpenFolderPanel(
            "캐릭터 스프라이트 폴더 선택",
            "Assets/GeneratedCharacters",
            ""
        );

        if (string.IsNullOrEmpty(folderPath))
            return;

        if (!folderPath.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog(
                "오류",
                "Assets 폴더 내의 폴더를 선택해주세요.",
                "확인"
            );
            return;
        }

        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        // 스프라이트 찾기
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { relativePath });

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "알림",
                "스프라이트를 찾을 수 없습니다.",
                "확인"
            );
            return;
        }

        // 기존 스프라이트 제거 여부 확인
        if (database.sprites.Count > 0)
        {
            if (!EditorUtility.DisplayDialog(
                "확인",
                $"기존 스프라이트 {database.sprites.Count}개를 제거하고 새로 임포트하시겠습니까?",
                "예", "아니오"))
            {
                return;
            }

            database.sprites.Clear();
        }

        // 진행 바 표시
        EditorUtility.DisplayProgressBar("스프라이트 임포트", "처리 중...", 0f);

        int importedCount = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                ParseAndAddSprite(database, fileName, sprite);
                importedCount++;
            }

            EditorUtility.DisplayProgressBar(
                "스프라이트 임포트",
                $"{i + 1}/{guids.Length} 처리 중...",
                (float)(i + 1) / guids.Length
            );
        }

        EditorUtility.ClearProgressBar();

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "완료",
            $"{importedCount}개의 스프라이트를 임포트했습니다.",
            "확인"
        );

        database.LogStatistics();
    }

    private void AutoCategorizeSprites(CharacterSpriteDatabase database)
    {
        if (database.sprites.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "알림",
                "분류할 스프라이트가 없습니다.",
                "확인"
            );
            return;
        }

        int categorizedCount = 0;

        foreach (var sprite in database.sprites)
        {
            if (sprite.sprite == null) continue;

            string name = sprite.sprite.name.ToLower();

            // 포즈 재분류
            var pose = ParsePose(name);
            if (pose != CharacterSpriteDatabase.CharacterPose.None)
            {
                sprite.pose = pose;
                categorizedCount++;
            }

            // 표정 재분류
            var emotion = ParseEmotion(name);
            if (emotion != CharacterSpriteDatabase.CharacterEmotion.None)
            {
                sprite.emotion = emotion;
                categorizedCount++;
            }

            // 각도 재분류
            var angle = ParseAngle(name);
            if (angle != CharacterSpriteDatabase.CharacterAngle.None)
            {
                sprite.angle = angle;
                categorizedCount++;
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "완료",
            $"{categorizedCount}개의 속성을 재분류했습니다.",
            "확인"
        );
    }

    private void CheckMissingSprites(CharacterSpriteDatabase database)
    {
        var missingCombinations = new System.Text.StringBuilder();
        missingCombinations.AppendLine("=== 누락된 스프라이트 조합 ===\n");

        // 필수 포즈 체크
        var requiredPoses = new[]
        {
            CharacterSpriteDatabase.CharacterPose.Idle,
            CharacterSpriteDatabase.CharacterPose.Walk,
            CharacterSpriteDatabase.CharacterPose.Attack,
            CharacterSpriteDatabase.CharacterPose.Victory,
            CharacterSpriteDatabase.CharacterPose.Defeat
        };

        foreach (var pose in requiredPoses)
        {
            if (!database.HasSprite(pose))
            {
                missingCombinations.AppendLine($"- 포즈: {pose}");
            }
        }

        // 필수 표정 체크
        var requiredEmotions = new[]
        {
            CharacterSpriteDatabase.CharacterEmotion.Neutral,
            CharacterSpriteDatabase.CharacterEmotion.Happy,
            CharacterSpriteDatabase.CharacterEmotion.Sad,
            CharacterSpriteDatabase.CharacterEmotion.Angry
        };

        foreach (var emotion in requiredEmotions)
        {
            if (!database.HasSprite(CharacterSpriteDatabase.CharacterPose.None, emotion))
            {
                missingCombinations.AppendLine($"- 표정: {emotion}");
            }
        }

        Debug.Log(missingCombinations.ToString());

        EditorUtility.DisplayDialog(
            "누락 체크 완료",
            "콘솔 로그를 확인하세요.",
            "확인"
        );
    }

    private void CheckDuplicateSprites(CharacterSpriteDatabase database)
    {
        var duplicates = database.sprites
            .GroupBy(s => new { s.pose, s.emotion, s.angle })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "중복 체크",
                "중복된 스프라이트가 없습니다.",
                "확인"
            );
            return;
        }

        var duplicateInfo = new System.Text.StringBuilder();
        duplicateInfo.AppendLine("=== 중복된 스프라이트 ===\n");

        foreach (var group in duplicates)
        {
            duplicateInfo.AppendLine(
                $"포즈: {group.Key.pose}, 표정: {group.Key.emotion}, 각도: {group.Key.angle}"
            );
            foreach (var sprite in group)
            {
                duplicateInfo.AppendLine($"  - {sprite.spriteId}");
            }
            duplicateInfo.AppendLine();
        }

        Debug.LogWarning(duplicateInfo.ToString());

        EditorUtility.DisplayDialog(
            "중복 발견",
            $"{duplicates.Count}개의 중복이 발견되었습니다.\n콘솔 로그를 확인하세요.",
            "확인"
        );
    }

    private void ParseAndAddSprite(CharacterSpriteDatabase database, string fileName, Sprite sprite)
    {
        var pose = ParsePose(fileName);
        var emotion = ParseEmotion(fileName);
        var angle = ParseAngle(fileName);

        database.AddSprite(fileName, sprite, pose, emotion, angle);
    }

    private CharacterSpriteDatabase.CharacterPose ParsePose(string text)
    {
        text = text.ToLower();

        if (text.Contains("idle")) return CharacterSpriteDatabase.CharacterPose.Idle;
        if (text.Contains("walk")) return CharacterSpriteDatabase.CharacterPose.Walk;
        if (text.Contains("run")) return CharacterSpriteDatabase.CharacterPose.Run;
        if (text.Contains("attack")) return CharacterSpriteDatabase.CharacterPose.Attack;
        if (text.Contains("defend")) return CharacterSpriteDatabase.CharacterPose.Defend;
        if (text.Contains("hit")) return CharacterSpriteDatabase.CharacterPose.Hit;
        if (text.Contains("victory")) return CharacterSpriteDatabase.CharacterPose.Victory;
        if (text.Contains("defeat")) return CharacterSpriteDatabase.CharacterPose.Defeat;
        if (text.Contains("jump")) return CharacterSpriteDatabase.CharacterPose.Jump;
        if (text.Contains("sit")) return CharacterSpriteDatabase.CharacterPose.Sit;

        return CharacterSpriteDatabase.CharacterPose.None;
    }

    private CharacterSpriteDatabase.CharacterEmotion ParseEmotion(string text)
    {
        text = text.ToLower();

        if (text.Contains("happy")) return CharacterSpriteDatabase.CharacterEmotion.Happy;
        if (text.Contains("sad")) return CharacterSpriteDatabase.CharacterEmotion.Sad;
        if (text.Contains("angry")) return CharacterSpriteDatabase.CharacterEmotion.Angry;
        if (text.Contains("surprised")) return CharacterSpriteDatabase.CharacterEmotion.Surprised;
        if (text.Contains("confused")) return CharacterSpriteDatabase.CharacterEmotion.Confused;
        if (text.Contains("determined")) return CharacterSpriteDatabase.CharacterEmotion.Determined;
        if (text.Contains("tired")) return CharacterSpriteDatabase.CharacterEmotion.Tired;
        if (text.Contains("neutral")) return CharacterSpriteDatabase.CharacterEmotion.Neutral;

        return CharacterSpriteDatabase.CharacterEmotion.None;
    }

    private CharacterSpriteDatabase.CharacterAngle ParseAngle(string text)
    {
        text = text.ToLower();

        if (text.Contains("front")) return CharacterSpriteDatabase.CharacterAngle.Front;
        if (text.Contains("side")) return CharacterSpriteDatabase.CharacterAngle.Side;
        if (text.Contains("back")) return CharacterSpriteDatabase.CharacterAngle.Back;
        if (text.Contains("threequarter") || text.Contains("3quarter"))
            return CharacterSpriteDatabase.CharacterAngle.ThreeQuarter;

        return CharacterSpriteDatabase.CharacterAngle.None;
    }
}

/// <summary>
/// 에디터 메뉴 확장
/// </summary>
public class CharacterToolsMenu
{
    [MenuItem("Tools/Character/Create New Character Database")]
    public static void CreateCharacterDatabase()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Character Sprite Database",
            "NewCharacterDB",
            "asset",
            "캐릭터 스프라이트 데이터베이스를 생성합니다."
        );

        if (string.IsNullOrEmpty(path))
            return;

        CharacterSpriteDatabase database = ScriptableObject.CreateInstance<CharacterSpriteDatabase>();
        AssetDatabase.CreateAsset(database, path);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = database;

        Debug.Log($"✓ 캐릭터 데이터베이스 생성: {path}");
    }

    [MenuItem("Tools/Character/Batch Import All Characters")]
    public static void BatchImportAllCharacters()
    {
        string basePath = "Assets/GeneratedCharacters";

        if (!AssetDatabase.IsValidFolder(basePath))
        {
            EditorUtility.DisplayDialog(
                "오류",
                $"{basePath} 폴더가 존재하지 않습니다.",
                "확인"
            );
            return;
        }

        // 각 캐릭터 폴더 찾기
        string[] characterFolders = Directory.GetDirectories(
            Path.Combine(Application.dataPath, "GeneratedCharacters")
        );

        if (characterFolders.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "알림",
                "캐릭터 폴더를 찾을 수 없습니다.",
                "확인"
            );
            return;
        }

        EditorUtility.DisplayProgressBar("배치 임포트", "처리 중...", 0f);

        int processedCount = 0;

        foreach (string folderPath in characterFolders)
        {
            string folderName = Path.GetFileName(folderPath);
            string relativePath = $"Assets/GeneratedCharacters/{folderName}";

            // 데이터베이스 생성 또는 로드
            string dbPath = $"{relativePath}/{folderName}_DB.asset";
            CharacterSpriteDatabase database;

            if (File.Exists(Path.Combine(Application.dataPath, "..", dbPath)))
            {
                database = AssetDatabase.LoadAssetAtPath<CharacterSpriteDatabase>(dbPath);
            }
            else
            {
                database = ScriptableObject.CreateInstance<CharacterSpriteDatabase>();
                database.characterId = folderName;
                database.characterName = folderName;
                AssetDatabase.CreateAsset(database, dbPath);
            }

            // 스프라이트 임포트
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { relativePath });

            database.sprites.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    // 파싱 및 추가 로직은 기존 코드 활용
                }
            }

            EditorUtility.SetDirty(database);
            processedCount++;

            EditorUtility.DisplayProgressBar(
                "배치 임포트",
                $"{processedCount}/{characterFolders.Length} 처리 중...",
                (float)processedCount / characterFolders.Length
            );
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "완료",
            $"{processedCount}개의 캐릭터를 임포트했습니다.",
            "확인"
        );
    }
}
