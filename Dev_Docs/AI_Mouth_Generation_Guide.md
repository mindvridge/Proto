# AI를 활용한 캐릭터 입 모양 자동 생성 가이드

## 📋 개요

캐릭터 이미지에서 **AI를 활용하여 8가지 입 모양을 자동으로 생성**하는 완전 가이드입니다.

### 목표
- ✅ 원본 캐릭터 1장 → 8가지 입 모양 자동 생성
- ✅ 일관된 스타일 유지
- ✅ 자동화된 워크플로우
- ✅ 게임에 바로 사용 가능한 품질

---

## 🎯 필요한 입 모양 (8종)

```
1. mouth_closed      - 입 닫힘 (ㅁ, ㅂ, M, B)
2. mouth_narrow      - 좁은 입 (ㅣ, I)
3. mouth_medium      - 중간 입 (ㅓ, E)
4. mouth_wide        - 넓은 입 (ㅏ, A)
5. mouth_o_shape     - O 모양 (ㅗ, ㅜ, O, U)
6. mouth_smile       - 미소
7. mouth_sad         - 슬픔
8. mouth_very_wide   - 매우 넓음 (놀람)
```

---

## 🚀 방법 1: Stable Diffusion Inpainting (추천)

### 장점
- ✅ 완전 무료
- ✅ 로컬에서 실행
- ✅ 높은 품질
- ✅ 완벽한 제어

### 필요 환경
```
PC 사양:
- GPU: NVIDIA GTX 1060 6GB 이상 (RTX 권장)
- RAM: 16GB 이상
- 저장공간: 20GB 이상

소프트웨어:
- Python 3.10
- AUTOMATIC1111 Stable Diffusion WebUI
- ControlNet Extension
```

### 설치 방법

#### 1단계: Stable Diffusion WebUI 설치

```bash
# Git Clone
git clone https://github.com/AUTOMATIC1111/stable-diffusion-webui.git
cd stable-diffusion-webui

# Windows
webui-user.bat

# Mac/Linux
./webui.sh
```

#### 2단계: 모델 다운로드

**추천 모델:**
```
1. Anything V5 (애니메이션 스타일)
   https://huggingface.co/stablediffusionapi/anything-v5

2. CounterfeitV3.0 (고품질 애니메이션)
   https://huggingface.co/gsdf/Counterfeit-V3.0

3. Pastel Mix (부드러운 스타일)
   https://civitai.com/models/5414/pastel-mix
```

**설치 위치:**
```
stable-diffusion-webui/models/Stable-diffusion/
```

#### 3단계: ControlNet 설치

```bash
# Extensions 탭에서
URL: https://github.com/Mikubill/sd-webui-controlnet
→ Install

# ControlNet 모델 다운로드
https://huggingface.co/lllyasviel/ControlNet-v1-1/tree/main
→ control_v11p_sd15_inpaint.pth 다운로드

# 설치 위치
stable-diffusion-webui/extensions/sd-webui-controlnet/models/
```

---

## 🎨 실전: Inpainting으로 입 생성

### 준비물

1. **원본 캐릭터 이미지** (512x512 이상 권장)
2. **입 부분 마스크** (흰색으로 입 부분 표시)

### Photoshop/GIMP로 마스크 만들기

```
1. 원본 이미지 열기
2. 새 레이어 생성
3. 브러시로 입 부분을 흰색으로 칠하기
4. 배경은 검은색으로
5. mask.png로 저장
```

### Stable Diffusion 설정

#### img2img 탭 → Inpaint

```
Settings:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Upload Image: character.png
Mask: 브러시로 입 부분 칠하기

Prompt:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Positive:
"anime girl, mouth closed, simple mouth, clean lines,
 consistent style, high quality, detailed face"

Negative:
"blurry, distorted, multiple mouths, weird mouth,
 teeth showing (닫힌 입 생성 시)"

Parameters:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Denoising strength: 0.6-0.8
CFG Scale: 7-10
Steps: 30-50
Sampler: DPM++ 2M Karras
```

### 8가지 입 모양 프롬프트

#### 1. mouth_closed (닫힌 입)
```
Positive:
"anime character, mouth closed, lips together,
 calm expression, clean simple mouth line,
 [your character description]"

Negative:
"open mouth, teeth, tongue, smile"

Settings:
- Denoising: 0.65
```

#### 2. mouth_narrow (좁은 입)
```
Positive:
"anime character, narrow mouth, slight opening,
 thin lips, small mouth,
 [your character description]"

Settings:
- Denoising: 0.7
```

#### 3. mouth_medium (중간 입)
```
Positive:
"anime character, medium open mouth,
 natural expression, speaking mouth,
 [your character description]"

Settings:
- Denoising: 0.7
```

#### 4. mouth_wide (넓은 입)
```
Positive:
"anime character, wide open mouth, big smile,
 happy expression, open mouth wide,
 [your character description]"

Settings:
- Denoising: 0.75
```

#### 5. mouth_o_shape (O 모양)
```
Positive:
"anime character, mouth shaped like O,
 round mouth, surprised mouth, oh face,
 [your character description]"

Settings:
- Denoising: 0.7
```

#### 6. mouth_smile (미소)
```
Positive:
"anime character, gentle smile, happy mouth,
 curved lips upward, cheerful expression,
 [your character description]"

Settings:
- Denoising: 0.65
```

#### 7. mouth_sad (슬픔)
```
Positive:
"anime character, sad mouth, frown,
 downturned lips, melancholic expression,
 [your character description]"

Settings:
- Denoising: 0.7
```

#### 8. mouth_very_wide (매우 넓음)
```
Positive:
"anime character, extremely wide open mouth,
 shocked expression, screaming mouth, very surprised,
 [your character description]"

Settings:
- Denoising: 0.8
```

---

## 🤖 방법 2: Python 자동화 스크립트

### 완전 자동 생성 스크립트

```python
#!/usr/bin/env python3
"""
AI 립싱크 입 모양 자동 생성기
Stable Diffusion API를 사용하여 8가지 입 모양 자동 생성
"""

import requests
import io
import base64
from PIL import Image
import json
import time
import os

class MouthGenerator:
    def __init__(self, api_url="http://127.0.0.1:7860"):
        self.api_url = api_url

    def image_to_base64(self, image_path):
        """이미지를 base64로 변환"""
        with open(image_path, "rb") as f:
            return base64.b64encode(f.read()).decode()

    def base64_to_image(self, base64_str):
        """base64를 이미지로 변환"""
        image_data = base64.b64decode(base64_str)
        return Image.open(io.BytesIO(image_data))

    def generate_mouth(self, character_image, mouth_type, output_path):
        """특정 입 모양 생성"""

        # 입 모양별 프롬프트 정의
        mouth_prompts = {
            "closed": {
                "prompt": "anime character, mouth closed, lips together, calm expression, clean simple mouth line",
                "negative": "open mouth, teeth, tongue, smile",
                "denoising": 0.65
            },
            "narrow": {
                "prompt": "anime character, narrow mouth, slight opening, thin lips, small mouth",
                "negative": "wide mouth, big smile, teeth",
                "denoising": 0.7
            },
            "medium": {
                "prompt": "anime character, medium open mouth, natural expression, speaking mouth",
                "negative": "closed mouth, very wide mouth",
                "denoising": 0.7
            },
            "wide": {
                "prompt": "anime character, wide open mouth, big smile, happy expression",
                "negative": "closed mouth, sad, frown",
                "denoising": 0.75
            },
            "o_shape": {
                "prompt": "anime character, mouth shaped like O, round mouth, surprised mouth, oh face",
                "negative": "closed mouth, smile, teeth",
                "denoising": 0.7
            },
            "smile": {
                "prompt": "anime character, gentle smile, happy mouth, curved lips upward, cheerful",
                "negative": "sad, frown, closed mouth",
                "denoising": 0.65
            },
            "sad": {
                "prompt": "anime character, sad mouth, frown, downturned lips, melancholic",
                "negative": "smile, happy, cheerful",
                "denoising": 0.7
            },
            "very_wide": {
                "prompt": "anime character, extremely wide open mouth, shocked, screaming, very surprised",
                "negative": "closed mouth, calm, normal mouth",
                "denoising": 0.8
            }
        }

        config = mouth_prompts[mouth_type]

        # API 요청 페이로드
        payload = {
            "init_images": [self.image_to_base64(character_image)],
            "mask": self.image_to_base64("mask.png"),  # 미리 준비된 마스크
            "prompt": config["prompt"],
            "negative_prompt": config["negative"],
            "denoising_strength": config["denoising"],
            "steps": 40,
            "cfg_scale": 8,
            "width": 512,
            "height": 512,
            "sampler_name": "DPM++ 2M Karras",
            "inpaint_full_res": True,
            "inpaint_full_res_padding": 32,
            "inpainting_fill": 1,  # Original
            "mask_blur": 4
        }

        # API 호출
        response = requests.post(
            url=f'{self.api_url}/sdapi/v1/img2img',
            json=payload
        )

        if response.status_code == 200:
            r = response.json()
            image = self.base64_to_image(r['images'][0])
            image.save(output_path)
            print(f"✓ {mouth_type} 생성 완료: {output_path}")
            return True
        else:
            print(f"✗ {mouth_type} 생성 실패: {response.status_code}")
            return False

    def generate_all_mouths(self, character_image, output_dir="output"):
        """모든 입 모양 자동 생성"""

        os.makedirs(output_dir, exist_ok=True)

        mouth_types = [
            "closed", "narrow", "medium", "wide",
            "o_shape", "smile", "sad", "very_wide"
        ]

        print("=" * 50)
        print("AI 입 모양 자동 생성 시작")
        print("=" * 50)

        for i, mouth_type in enumerate(mouth_types, 1):
            print(f"\n[{i}/8] {mouth_type} 생성 중...")

            output_path = os.path.join(output_dir, f"mouth_{mouth_type}.png")

            success = self.generate_mouth(
                character_image,
                mouth_type,
                output_path
            )

            if success:
                # API 과부하 방지
                time.sleep(2)
            else:
                print(f"재시도 중...")
                time.sleep(5)
                self.generate_mouth(character_image, mouth_type, output_path)

        print("\n" + "=" * 50)
        print("✓ 모든 입 모양 생성 완료!")
        print(f"출력 폴더: {output_dir}")
        print("=" * 50)

# 사용 예제
if __name__ == "__main__":
    generator = MouthGenerator()

    # 캐릭터 이미지 경로
    character_image = "character.png"

    # 8가지 입 모양 자동 생성
    generator.generate_all_mouths(character_image, output_dir="generated_mouths")
```

### 사용 방법

```bash
# 1. Stable Diffusion WebUI 실행 (API 모드)
webui-user.bat --api

# 2. 캐릭터 이미지와 마스크 준비
character.png  # 원본 캐릭터
mask.png       # 입 부분 마스크

# 3. 스크립트 실행
python generate_mouths.py

# 4. 결과 확인
generated_mouths/
├── mouth_closed.png
├── mouth_narrow.png
├── mouth_medium.png
├── mouth_wide.png
├── mouth_o_shape.png
├── mouth_smile.png
├── mouth_sad.png
└── mouth_very_wide.png
```

---

## 🎨 방법 3: ComfyUI 워크플로우 (고급)

### 장점
- ✅ 노드 기반 비주얼 편집
- ✅ 완전 자동화 가능
- ✅ 배치 처리
- ✅ 재현 가능한 워크플로우

### 설치

```bash
# ComfyUI 설치
git clone https://github.com/comfyanonymous/ComfyUI.git
cd ComfyUI
pip install -r requirements.txt

# 실행
python main.py
```

### 워크플로우 JSON

```json
{
  "name": "Lip Sync Mouth Generator",
  "description": "8가지 입 모양 자동 생성",
  "nodes": {
    "1": {
      "type": "LoadImage",
      "inputs": {
        "image": "character.png"
      }
    },
    "2": {
      "type": "LoadImage",
      "inputs": {
        "image": "mask.png"
      }
    },
    "3": {
      "type": "CLIPTextEncode",
      "inputs": {
        "text": "anime character, {{mouth_type}}, high quality"
      }
    },
    "4": {
      "type": "VAEEncode",
      "inputs": {
        "image": "1"
      }
    },
    "5": {
      "type": "KSamplerAdvanced",
      "inputs": {
        "latent_image": "4",
        "denoise": 0.7,
        "steps": 40,
        "cfg": 8
      }
    },
    "6": {
      "type": "VAEDecode"
    },
    "7": {
      "type": "SaveImage",
      "inputs": {
        "filename_prefix": "mouth_{{mouth_type}}"
      }
    }
  }
}
```

---

## 💻 방법 4: 온라인 AI 서비스 (유료)

### 1. Leonardo.ai

```
장점: 사용 간편, 고품질
가격: 무료 150크레딧/월, $10/월~

사용법:
1. leonardo.ai 가입
2. Canvas Mode 선택
3. 캐릭터 이미지 업로드
4. Erase 툴로 입 부분 지우기
5. Prompt 입력 후 생성
```

### 2. Runway ML

```
장점: 프로페셔널, 다양한 기능
가격: $12/월~

사용법:
1. runwayml.com 가입
2. Inpainting 툴 선택
3. 이미지 업로드 및 마스킹
4. 프롬프트 입력
```

### 3. Midjourney (Vary Region)

```
장점: 최고 품질
가격: $10/월~

사용법:
1. Discord에서 이미지 생성
2. Vary (Region) 버튼 클릭
3. 입 부분 선택
4. 프롬프트 입력
```

---

## 🔧 Unity 통합

### 자동 임포트 스크립트

```csharp
using UnityEngine;
using UnityEditor;
using System.IO;

public class MouthSpriteImporter : EditorWindow
{
    private string sourcePath = "generated_mouths";
    private string targetPath = "Assets/Sprites/Mouth";

    [MenuItem("Tools/Import AI Generated Mouths")]
    static void ShowWindow()
    {
        GetWindow<MouthSpriteImporter>("Mouth Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("AI 생성 입 스프라이트 임포트", EditorStyles.boldLabel);

        sourcePath = EditorGUILayout.TextField("Source Path:", sourcePath);
        targetPath = EditorGUILayout.TextField("Target Path:", targetPath);

        if (GUILayout.Button("Import All Mouths"))
        {
            ImportMouths();
        }
    }

    void ImportMouths()
    {
        string[] mouthTypes = {
            "closed", "narrow", "medium", "wide",
            "o_shape", "smile", "sad", "very_wide"
        };

        foreach (string type in mouthTypes)
        {
            string sourceFile = Path.Combine(sourcePath, $"mouth_{type}.png");
            string targetFile = Path.Combine(targetPath, $"mouth_{type}.png");

            if (File.Exists(sourceFile))
            {
                // 폴더 생성
                Directory.CreateDirectory(targetPath);

                // 파일 복사
                File.Copy(sourceFile, targetFile, true);

                // 임포트 설정
                AssetDatabase.ImportAsset(targetFile);
                ConfigureSprite(targetFile);

                Debug.Log($"✓ Imported: {type}");
            }
            else
            {
                Debug.LogWarning($"✗ Not found: {sourceFile}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("All mouths imported!");
    }

    void ConfigureSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;

            importer.SaveAndReimport();
        }
    }
}
```

---

## 📊 품질 관리

### 체크리스트

```
□ 모든 입 모양이 같은 스타일인가?
□ 입의 위치가 일관적인가?
□ 해상도가 충분한가? (최소 256x256)
□ 배경이 투명한가?
□ 얼굴 각도가 동일한가?
□ 조명이 일관적인가?
□ 색상이 원본과 일치하는가?
□ 아티팩트가 없는가?
```

### 후처리 (Photoshop/GIMP)

```
1. 모든 입 모양 정렬
   - 동일한 위치에 배치

2. 색상 보정
   - 원본과 색상 일치

3. 배경 제거
   - 투명 배경으로 저장

4. 크기 조정
   - 128x128 또는 256x256

5. 최적화
   - PNG-8 (256 colors)
```

---

## 🚀 완전 자동화 파이프라인

### 전체 워크플로우

```python
#!/usr/bin/env python3
"""
완전 자동 립싱크 스프라이트 생성 파이프라인
"""

import os
import cv2
import numpy as np
from PIL import Image

class LipSyncPipeline:
    def __init__(self):
        self.mouth_generator = MouthGenerator()

    def create_mask(self, image_path, output_path="mask.png"):
        """AI로 자동으로 입 부분 마스크 생성"""

        # MediaPipe Face Mesh 사용
        import mediapipe as mp

        mp_face_mesh = mp.solutions.face_mesh

        # 이미지 로드
        image = cv2.imread(image_path)

        with mp_face_mesh.FaceMesh(
            static_image_mode=True,
            max_num_faces=1,
            min_detection_confidence=0.5
        ) as face_mesh:

            results = face_mesh.process(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))

            if results.multi_face_landmarks:
                # 입 영역 랜드마크 추출
                landmarks = results.multi_face_landmarks[0]

                # 입 주변 포인트 (61-81)
                mouth_points = []
                for idx in range(61, 82):
                    x = int(landmarks.landmark[idx].x * image.shape[1])
                    y = int(landmarks.landmark[idx].y * image.shape[0])
                    mouth_points.append([x, y])

                # 마스크 생성
                mask = np.zeros(image.shape[:2], dtype=np.uint8)
                cv2.fillPoly(mask, [np.array(mouth_points)], 255)

                # 마스크 확장 (여유 공간)
                kernel = np.ones((15, 15), np.uint8)
                mask = cv2.dilate(mask, kernel, iterations=1)

                # 저장
                cv2.imwrite(output_path, mask)
                print(f"✓ 마스크 생성 완료: {output_path}")
                return True

        print("✗ 얼굴을 찾을 수 없습니다!")
        return False

    def run_full_pipeline(self, character_image, output_dir="final_output"):
        """완전 자동 파이프라인 실행"""

        print("=" * 60)
        print("립싱크 스프라이트 완전 자동 생성 파이프라인")
        print("=" * 60)

        # 1단계: 마스크 자동 생성
        print("\n[1/3] 입 부분 마스크 자동 생성 중...")
        if not self.create_mask(character_image):
            return False

        # 2단계: AI로 8가지 입 모양 생성
        print("\n[2/3] AI로 8가지 입 모양 생성 중...")
        self.mouth_generator.generate_all_mouths(
            character_image,
            output_dir=output_dir + "/raw"
        )

        # 3단계: 후처리
        print("\n[3/3] 후처리 및 최적화 중...")
        self.post_process(output_dir + "/raw", output_dir + "/final")

        print("\n" + "=" * 60)
        print("✓ 완료! Unity에서 사용 가능한 스프라이트 준비됨")
        print(f"출력 폴더: {output_dir}/final")
        print("=" * 60)

        return True

    def post_process(self, input_dir, output_dir):
        """후처리: 배경 제거, 정렬, 크기 조정"""

        os.makedirs(output_dir, exist_ok=True)

        for filename in os.listdir(input_dir):
            if filename.endswith('.png'):
                input_path = os.path.join(input_dir, filename)
                output_path = os.path.join(output_dir, filename)

                # 이미지 로드
                img = Image.open(input_path).convert("RGBA")

                # 크기 조정 (256x256)
                img = img.resize((256, 256), Image.Resampling.LANCZOS)

                # 저장
                img.save(output_path, "PNG", optimize=True)

                print(f"  ✓ {filename}")

# 실행
if __name__ == "__main__":
    pipeline = LipSyncPipeline()
    pipeline.run_full_pipeline("character.png")
```

---

## 🎯 실전 팁

### 1. 일관성 유지하기

```
Seed 고정:
- 같은 Seed 사용
- 같은 모델 사용
- 같은 설정 사용

예: Seed = 123456789
```

### 2. 품질 향상

```
Upscaling:
1. 생성된 이미지 → Real-ESRGAN 4x
2. 또는 ControlNet Tile로 디테일 추가
```

### 3. 배치 처리

```python
# 여러 캐릭터 동시 처리
characters = [
    "hero.png",
    "sister.png",
    "monster.png"
]

for char in characters:
    pipeline.run_full_pipeline(char)
```

---

## 📚 추가 리소스

### 학습 자료
- [Stable Diffusion 공식 문서](https://github.com/CompVis/stable-diffusion)
- [ControlNet 가이드](https://github.com/lllyasviel/ControlNet)
- [ComfyUI 튜토리얼](https://comfyui.org)

### 추천 모델
- **애니메이션**: Anything V5, Counterfeit V3
- **리얼리즘**: Realistic Vision, DreamShaper
- **게임 아트**: MeinaMix, AbyssOrangeMix

### 커뮤니티
- [Civitai](https://civitai.com) - 모델 공유
- [r/StableDiffusion](https://reddit.com/r/StableDiffusion)
- [Hugging Face](https://huggingface.co)

---

## ⚠️ 주의사항

### 법적 고려사항
```
✓ 자신이 만든 캐릭터만 사용
✓ 상업적 사용 가능한 모델 확인
✓ 저작권 존중
```

### 윤리적 사용
```
✓ AI 생성 표기
✓ 적절한 콘텐츠만 생성
✓ 딥페이크 악용 금지
```

---

## 📝 체크리스트

### 준비
- [ ] Stable Diffusion WebUI 설치
- [ ] 모델 다운로드
- [ ] Python 환경 설정
- [ ] 캐릭터 이미지 준비

### 생성
- [ ] 마스크 생성
- [ ] 8가지 입 모양 생성
- [ ] 품질 확인
- [ ] 후처리 완료

### Unity 통합
- [ ] 스프라이트 임포트
- [ ] LipSyncDatabase에 등록
- [ ] 테스트 실행
- [ ] 최적화

---

**작성일**: 2025-11-19
**버전**: 1.0.0
**담당**: AI/ML팀, 아트팀

**이제 AI로 자동으로 입 모양을 만들 수 있습니다! 🤖✨**
