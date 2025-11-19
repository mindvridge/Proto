#!/usr/bin/env python3
"""
AI 립싱크 입 모양 자동 생성기
Stable Diffusion API를 사용하여 8가지 입 모양 자동 생성

Requirements:
    pip install requests pillow opencv-python mediapipe numpy

Usage:
    python generate_mouth_sprites.py --input character.png --output generated_mouths
"""

import requests
import io
import base64
from PIL import Image
import json
import time
import os
import argparse
import cv2
import numpy as np

class MouthMaskGenerator:
    """MediaPipe를 사용한 자동 입 마스크 생성기"""

    def __init__(self):
        try:
            import mediapipe as mp
            self.mp_face_mesh = mp.solutions.face_mesh
            self.has_mediapipe = True
        except ImportError:
            print("⚠ MediaPipe not found. Auto mask generation disabled.")
            print("Install: pip install mediapipe")
            self.has_mediapipe = False

    def generate_mask(self, image_path, output_path="mask.png", padding=20):
        """얼굴에서 입 부분 마스크 자동 생성"""

        if not self.has_mediapipe:
            print("✗ MediaPipe not available. Please create mask manually.")
            return False

        # 이미지 로드
        image = cv2.imread(image_path)
        if image is None:
            print(f"✗ Cannot load image: {image_path}")
            return False

        rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

        with self.mp_face_mesh.FaceMesh(
            static_image_mode=True,
            max_num_faces=1,
            refine_landmarks=True,
            min_detection_confidence=0.5
        ) as face_mesh:

            results = face_mesh.process(rgb_image)

            if not results.multi_face_landmarks:
                print("✗ No face detected in image")
                return False

            landmarks = results.multi_face_landmarks[0].landmark

            # 입 주변 랜드마크 인덱스 (FACEMESH_LIPS)
            # 상입술 + 하입술
            mouth_indices = [
                # 외곽선
                61, 146, 91, 181, 84, 17, 314, 405, 321, 375, 291,
                # 내부 윤곽
                78, 95, 88, 178, 87, 14, 317, 402, 318, 324, 308,
                # 추가 포인트
                191, 80, 81, 82, 13, 312, 311, 310, 415
            ]

            h, w = image.shape[:2]

            # 랜드마크를 픽셀 좌표로 변환
            mouth_points = []
            for idx in mouth_indices:
                x = int(landmarks[idx].x * w)
                y = int(landmarks[idx].y * h)
                mouth_points.append([x, y])

            # Convex Hull로 입 영역 정의
            mouth_points = np.array(mouth_points)
            hull = cv2.convexHull(mouth_points)

            # 마스크 생성 (검은 배경)
            mask = np.zeros((h, w), dtype=np.uint8)

            # 입 영역을 흰색으로 채우기
            cv2.fillConvexPoly(mask, hull, 255)

            # 패딩 추가 (입 주변 여유 공간)
            kernel = np.ones((padding, padding), np.uint8)
            mask = cv2.dilate(mask, kernel, iterations=1)

            # 부드럽게
            mask = cv2.GaussianBlur(mask, (5, 5), 0)

            # 저장
            cv2.imwrite(output_path, mask)
            print(f"✓ Mask generated: {output_path}")

            return True


class StableDiffusionMouthGenerator:
    """Stable Diffusion API를 사용한 입 모양 생성기"""

    def __init__(self, api_url="http://127.0.0.1:7860"):
        self.api_url = api_url
        self.check_api()

    def check_api(self):
        """API 연결 확인"""
        try:
            response = requests.get(f"{self.api_url}/sdapi/v1/sd-models", timeout=5)
            if response.status_code == 200:
                print(f"✓ Connected to Stable Diffusion API at {self.api_url}")
                models = response.json()
                if models:
                    print(f"  Current model: {models[0]['title']}")
            else:
                print(f"⚠ API responded with status {response.status_code}")
        except requests.exceptions.RequestException as e:
            print(f"✗ Cannot connect to API at {self.api_url}")
            print(f"  Error: {e}")
            print("\n  Make sure Stable Diffusion WebUI is running with --api flag:")
            print("  webui-user.bat --api")
            exit(1)

    def image_to_base64(self, image_path):
        """이미지 파일을 base64로 인코딩"""
        with open(image_path, "rb") as f:
            return base64.b64encode(f.read()).decode()

    def base64_to_image(self, base64_str):
        """base64 문자열을 PIL Image로 디코딩"""
        image_data = base64.b64decode(base64_str)
        return Image.open(io.BytesIO(image_data))

    def get_mouth_config(self, mouth_type):
        """입 모양별 설정 반환"""

        configs = {
            "closed": {
                "prompt": "anime character, closed mouth, lips together, calm neutral expression, simple clean mouth line, detailed face",
                "negative": "open mouth, teeth showing, tongue visible, smile, multiple mouths",
                "denoising": 0.65
            },
            "narrow": {
                "prompt": "anime character, narrow mouth, slightly open, thin lips, small mouth opening, speaking naturally",
                "negative": "wide mouth, big smile, teeth, very open",
                "denoising": 0.7
            },
            "medium": {
                "prompt": "anime character, medium open mouth, natural speaking expression, moderately open lips",
                "negative": "closed mouth, very wide mouth, extreme expression",
                "denoising": 0.7
            },
            "wide": {
                "prompt": "anime character, wide open mouth, happy expression, big cheerful smile, mouth open wide",
                "negative": "closed mouth, sad, frown, neutral face",
                "denoising": 0.75
            },
            "o_shape": {
                "prompt": "anime character, mouth shaped like letter O, round mouth, surprised oh expression, circular lips",
                "negative": "closed mouth, smile, wide grin, teeth",
                "denoising": 0.7
            },
            "smile": {
                "prompt": "anime character, gentle smile, happy smiling mouth, upward curved lips, cheerful pleasant expression",
                "negative": "sad, frown, neutral, crying, angry",
                "denoising": 0.65
            },
            "sad": {
                "prompt": "anime character, sad mouth, frown, downturned lips, melancholic disappointed expression",
                "negative": "smile, happy, cheerful, grin",
                "denoising": 0.7
            },
            "very_wide": {
                "prompt": "anime character, extremely wide open mouth, shocked surprised expression, mouth open very wide, yelling screaming",
                "negative": "closed mouth, calm, normal expression, slight smile",
                "denoising": 0.8
            }
        }

        return configs.get(mouth_type, configs["closed"])

    def generate_mouth(self, character_image, mask_image, mouth_type, output_path,
                      additional_prompt="", seed=-1):
        """특정 입 모양 생성"""

        config = self.get_mouth_config(mouth_type)

        # 프롬프트 결합
        full_prompt = config["prompt"]
        if additional_prompt:
            full_prompt = f"{full_prompt}, {additional_prompt}"

        # Base64 인코딩
        init_image_b64 = self.image_to_base64(character_image)
        mask_b64 = self.image_to_base64(mask_image)

        # API 페이로드
        payload = {
            "init_images": [init_image_b64],
            "mask": mask_b64,
            "prompt": full_prompt,
            "negative_prompt": config["negative"],
            "denoising_strength": config["denoising"],
            "steps": 40,
            "cfg_scale": 8,
            "width": 512,
            "height": 512,
            "sampler_name": "DPM++ 2M Karras",
            "seed": seed,
            "inpaint_full_res": True,
            "inpaint_full_res_padding": 32,
            "inpainting_fill": 1,  # Original
            "mask_blur": 4
        }

        print(f"  Generating: {mouth_type}...")
        print(f"  Prompt: {full_prompt[:80]}...")

        try:
            response = requests.post(
                url=f'{self.api_url}/sdapi/v1/img2img',
                json=payload,
                timeout=300  # 5 minutes timeout
            )

            if response.status_code == 200:
                r = response.json()

                # 이미지 저장
                image = self.base64_to_image(r['images'][0])
                image.save(output_path, 'PNG')

                print(f"  ✓ Saved: {output_path}")

                # Seed 정보 반환
                info = json.loads(r.get('info', '{}'))
                used_seed = info.get('seed', -1)
                print(f"  Seed: {used_seed}")

                return True, used_seed

            else:
                print(f"  ✗ API Error: {response.status_code}")
                print(f"  {response.text[:200]}")
                return False, -1

        except requests.exceptions.Timeout:
            print(f"  ✗ Timeout error")
            return False, -1
        except Exception as e:
            print(f"  ✗ Error: {e}")
            return False, -1

    def generate_all_mouths(self, character_image, mask_image, output_dir,
                           additional_prompt="", seed=-1):
        """8가지 입 모양 모두 생성"""

        os.makedirs(output_dir, exist_ok=True)

        mouth_types = [
            "closed", "narrow", "medium", "wide",
            "o_shape", "smile", "sad", "very_wide"
        ]

        print("\n" + "=" * 70)
        print("AI Mouth Sprite Generation Started")
        print("=" * 70)
        print(f"Character: {character_image}")
        print(f"Mask: {mask_image}")
        print(f"Output: {output_dir}")
        if additional_prompt:
            print(f"Additional prompt: {additional_prompt}")
        if seed != -1:
            print(f"Seed: {seed}")
        print("=" * 70)

        results = {}
        first_seed = seed

        for i, mouth_type in enumerate(mouth_types, 1):
            print(f"\n[{i}/8] {mouth_type.upper()}")
            print("-" * 70)

            output_path = os.path.join(output_dir, f"mouth_{mouth_type}.png")

            success, used_seed = self.generate_mouth(
                character_image,
                mask_image,
                mouth_type,
                output_path,
                additional_prompt,
                seed=first_seed
            )

            results[mouth_type] = {
                "success": success,
                "path": output_path if success else None,
                "seed": used_seed
            }

            # 첫 번째 성공 시 seed 기록 (일관성을 위해)
            if success and first_seed == -1:
                first_seed = used_seed
                print(f"  Using seed {first_seed} for consistency")

            # API 과부하 방지
            if i < len(mouth_types):
                print("  Waiting 3 seconds...")
                time.sleep(3)

        # 결과 요약
        print("\n" + "=" * 70)
        print("GENERATION COMPLETE")
        print("=" * 70)

        success_count = sum(1 for r in results.values() if r["success"])
        print(f"Success: {success_count}/8")

        if success_count < 8:
            print("\nFailed:")
            for mouth_type, result in results.items():
                if not result["success"]:
                    print(f"  - {mouth_type}")

        print(f"\nOutput directory: {output_dir}")
        print("=" * 70 + "\n")

        return results


def main():
    parser = argparse.ArgumentParser(
        description="AI-powered Lip Sync Mouth Sprite Generator"
    )

    parser.add_argument(
        "--input", "-i",
        required=True,
        help="Input character image path"
    )

    parser.add_argument(
        "--mask", "-m",
        default=None,
        help="Mask image path (auto-generated if not provided)"
    )

    parser.add_argument(
        "--output", "-o",
        default="generated_mouths",
        help="Output directory (default: generated_mouths)"
    )

    parser.add_argument(
        "--prompt", "-p",
        default="",
        help="Additional prompt to add to all generations"
    )

    parser.add_argument(
        "--seed", "-s",
        type=int,
        default=-1,
        help="Seed for reproducibility (-1 for random)"
    )

    parser.add_argument(
        "--api",
        default="http://127.0.0.1:7860",
        help="Stable Diffusion API URL"
    )

    args = parser.parse_args()

    # 입력 파일 확인
    if not os.path.exists(args.input):
        print(f"✗ Input file not found: {args.input}")
        return

    # 마스크 생성 또는 로드
    mask_path = args.mask

    if mask_path is None:
        print("\n🎭 Auto-generating mouth mask...")
        mask_generator = MouthMaskGenerator()
        mask_path = "auto_mask.png"

        if not mask_generator.generate_mask(args.input, mask_path):
            print("\n⚠ Auto mask generation failed.")
            print("Please provide a mask manually with --mask argument")
            return

    elif not os.path.exists(mask_path):
        print(f"✗ Mask file not found: {mask_path}")
        return

    # 입 모양 생성
    generator = StableDiffusionMouthGenerator(api_url=args.api)

    generator.generate_all_mouths(
        character_image=args.input,
        mask_image=mask_path,
        output_dir=args.output,
        additional_prompt=args.prompt,
        seed=args.seed
    )

    print("\n✓ All done! Import sprites into Unity.")
    print(f"  See: {args.output}/")


if __name__ == "__main__":
    main()
