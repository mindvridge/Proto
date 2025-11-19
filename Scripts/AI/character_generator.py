#!/usr/bin/env python3
"""
캐릭터 일관성 유지 이미지 생성 스크립트
IP-Adapter + ControlNet을 사용한 다양한 포즈/표정 생성
"""

import os
import sys
import json
import argparse
from pathlib import Path
from typing import List, Dict, Optional
import requests
import base64
from io import BytesIO
from PIL import Image

class CharacterImageGenerator:
    """캐릭터 이미지 생성기 (IP-Adapter + ControlNet)"""

    def __init__(self, api_url="http://127.0.0.1:7860"):
        """
        Args:
            api_url: Automatic1111 WebUI API URL
        """
        self.api_url = api_url
        self.project_root = Path(__file__).parent.parent.parent

    def encode_image(self, image_path):
        """이미지를 base64로 인코딩"""
        with open(image_path, 'rb') as f:
            return base64.b64encode(f.read()).decode('utf-8')

    def decode_image(self, base64_str):
        """base64를 이미지로 디코딩"""
        image_data = base64.b64decode(base64_str)
        return Image.open(BytesIO(image_data))

    def txt2img(self,
                prompt: str,
                negative_prompt: str = "",
                width: int = 512,
                height: int = 512,
                steps: int = 30,
                cfg_scale: float = 7.5,
                seed: int = -1,
                **kwargs) -> Image.Image:
        """텍스트로 이미지 생성"""

        payload = {
            "prompt": prompt,
            "negative_prompt": negative_prompt,
            "width": width,
            "height": height,
            "steps": steps,
            "cfg_scale": cfg_scale,
            "seed": seed,
            "sampler_name": "DPM++ 2M Karras",
            **kwargs
        }

        response = requests.post(
            f"{self.api_url}/sdapi/v1/txt2img",
            json=payload
        )

        if response.status_code == 200:
            result = response.json()
            return self.decode_image(result['images'][0])
        else:
            raise Exception(f"API 오류: {response.status_code} - {response.text}")

    def generate_with_ipadapter(self,
                                 reference_image_path: str,
                                 prompt: str,
                                 pose_image_path: Optional[str] = None,
                                 ipadapter_weight: float = 0.8,
                                 controlnet_weight: float = 0.8,
                                 **kwargs) -> Image.Image:
        """IP-Adapter + ControlNet으로 캐릭터 이미지 생성"""

        # 참조 이미지 인코딩
        ref_image_b64 = self.encode_image(reference_image_path)

        # Alwayson Scripts 설정
        alwayson_scripts = {}

        # IP-Adapter 설정
        alwayson_scripts["ip-adapter"] = {
            "args": [{
                "enabled": True,
                "input_image": ref_image_b64,
                "model": "ip-adapter-plus_sd15",
                "weight": ipadapter_weight,
                "start": 0.0,
                "end": 0.8
            }]
        }

        # ControlNet 설정 (포즈 이미지가 있는 경우)
        if pose_image_path:
            pose_image_b64 = self.encode_image(pose_image_path)

            alwayson_scripts["controlnet"] = {
                "args": [{
                    "enabled": True,
                    "input_image": pose_image_b64,
                    "model": "control_sd15_openpose",
                    "module": "openpose_full",
                    "weight": controlnet_weight,
                    "guidance_start": 0.0,
                    "guidance_end": 1.0,
                    "control_mode": 0,
                    "pixel_perfect": True
                }]
            }

        # 전체 요청 페이로드
        payload = {
            "prompt": prompt,
            "negative_prompt": kwargs.get("negative_prompt", "low quality, blurry, deformed"),
            "width": kwargs.get("width", 512),
            "height": kwargs.get("height", 512),
            "steps": kwargs.get("steps", 30),
            "cfg_scale": kwargs.get("cfg_scale", 7.5),
            "seed": kwargs.get("seed", -1),
            "sampler_name": "DPM++ 2M Karras",
            "alwayson_scripts": alwayson_scripts
        }

        response = requests.post(
            f"{self.api_url}/sdapi/v1/txt2img",
            json=payload
        )

        if response.status_code == 200:
            result = response.json()
            return self.decode_image(result['images'][0])
        else:
            raise Exception(f"API 오류: {response.status_code} - {response.text}")

    def batch_generate_poses(self,
                              reference_image: str,
                              pose_configs: List[Dict],
                              output_folder: str,
                              base_prompt: str = "1character, high quality, masterpiece"):
        """여러 포즈를 배치 생성"""

        output_path = Path(output_folder)
        output_path.mkdir(parents=True, exist_ok=True)

        print(f"\n배치 생성 시작: {len(pose_configs)}개 이미지")
        print(f"참조 이미지: {reference_image}")
        print(f"출력 폴더: {output_folder}\n")

        results = []

        for i, config in enumerate(pose_configs, 1):
            try:
                print(f"[{i}/{len(pose_configs)}] 생성 중: {config['name']}")

                # 프롬프트 구성
                prompt = f"{base_prompt}, {config.get('prompt_addition', '')}"

                # 이미지 생성
                image = self.generate_with_ipadapter(
                    reference_image_path=reference_image,
                    prompt=prompt,
                    pose_image_path=config.get('pose_image'),
                    ipadapter_weight=config.get('ipadapter_weight', 0.8),
                    controlnet_weight=config.get('controlnet_weight', 0.8),
                    seed=config.get('seed', -1)
                )

                # 저장
                output_file = output_path / f"{config['name']}.png"
                image.save(output_file)

                results.append({
                    'name': config['name'],
                    'file': str(output_file),
                    'success': True
                })

                print(f"  ✓ 저장 완료: {output_file}")

            except Exception as e:
                print(f"  ✗ 생성 실패: {e}")
                results.append({
                    'name': config['name'],
                    'error': str(e),
                    'success': False
                })

        # 결과 요약
        success_count = sum(1 for r in results if r['success'])
        print(f"\n{'='*50}")
        print(f"배치 생성 완료: {success_count}/{len(pose_configs)} 성공")
        print(f"{'='*50}")

        return results

    def generate_character_set(self,
                                reference_image: str,
                                character_name: str,
                                output_folder: str,
                                include_poses: bool = True,
                                include_emotions: bool = True,
                                include_angles: bool = True):
        """캐릭터 전체 세트 생성 (포즈, 표정, 각도)"""

        base_prompt = f"{character_name}, anime character, detailed, high quality, masterpiece"

        all_configs = []

        # 포즈 세트
        if include_poses:
            poses = [
                {
                    'name': f'{character_name}_idle',
                    'prompt_addition': 'standing, neutral pose, idle stance',
                    'seed': 12345
                },
                {
                    'name': f'{character_name}_walk',
                    'prompt_addition': 'walking, moving forward, dynamic',
                    'seed': 12346
                },
                {
                    'name': f'{character_name}_run',
                    'prompt_addition': 'running, fast movement, action',
                    'seed': 12347
                },
                {
                    'name': f'{character_name}_attack',
                    'prompt_addition': 'attack pose, punching, action battle',
                    'seed': 12348
                },
                {
                    'name': f'{character_name}_defend',
                    'prompt_addition': 'defensive stance, blocking, guard',
                    'seed': 12349
                },
                {
                    'name': f'{character_name}_victory',
                    'prompt_addition': 'victory pose, celebrating, fist raised',
                    'seed': 12350
                },
                {
                    'name': f'{character_name}_defeat',
                    'prompt_addition': 'defeated, tired, exhausted',
                    'seed': 12351
                },
                {
                    'name': f'{character_name}_jump',
                    'prompt_addition': 'jumping, in air, dynamic movement',
                    'seed': 12352
                },
                {
                    'name': f'{character_name}_sit',
                    'prompt_addition': 'sitting, relaxed, resting',
                    'seed': 12353
                },
            ]
            all_configs.extend(poses)

        # 표정 세트
        if include_emotions:
            emotions = [
                {
                    'name': f'{character_name}_happy',
                    'prompt_addition': 'smiling, happy expression, cheerful',
                    'seed': 22345
                },
                {
                    'name': f'{character_name}_sad',
                    'prompt_addition': 'sad expression, crying, melancholic',
                    'seed': 22346
                },
                {
                    'name': f'{character_name}_angry',
                    'prompt_addition': 'angry expression, furious, intense',
                    'seed': 22347
                },
                {
                    'name': f'{character_name}_surprised',
                    'prompt_addition': 'surprised expression, shocked, amazed',
                    'seed': 22348
                },
                {
                    'name': f'{character_name}_confused',
                    'prompt_addition': 'confused expression, puzzled, questioning',
                    'seed': 22349
                },
                {
                    'name': f'{character_name}_determined',
                    'prompt_addition': 'determined expression, focused, serious',
                    'seed': 22350
                },
            ]
            all_configs.extend(emotions)

        # 각도 세트
        if include_angles:
            angles = [
                {
                    'name': f'{character_name}_front',
                    'prompt_addition': 'front view, facing forward, straight on',
                    'seed': 32345
                },
                {
                    'name': f'{character_name}_side',
                    'prompt_addition': 'side view, profile, from the side',
                    'seed': 32346
                },
                {
                    'name': f'{character_name}_back',
                    'prompt_addition': 'back view, from behind, rear view',
                    'seed': 32347
                },
                {
                    'name': f'{character_name}_three_quarter',
                    'prompt_addition': '3/4 view, slight angle, angled view',
                    'seed': 32348
                },
            ]
            all_configs.extend(angles)

        # 배치 생성
        return self.batch_generate_poses(
            reference_image=reference_image,
            pose_configs=all_configs,
            output_folder=output_folder,
            base_prompt=base_prompt
        )


class ComfyUICharacterGenerator:
    """ComfyUI API를 사용한 캐릭터 생성"""

    def __init__(self, api_url="http://127.0.0.1:8188"):
        self.api_url = api_url
        self.client_id = "character_generator"

    def queue_prompt(self, workflow):
        """워크플로우를 큐에 추가"""
        payload = {
            "prompt": workflow,
            "client_id": self.client_id
        }

        response = requests.post(
            f"{self.api_url}/prompt",
            json=payload
        )

        if response.status_code == 200:
            return response.json()['prompt_id']
        else:
            raise Exception(f"API 오류: {response.status_code}")

    def get_history(self, prompt_id):
        """실행 결과 가져오기"""
        response = requests.get(
            f"{self.api_url}/history/{prompt_id}"
        )

        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(f"API 오류: {response.status_code}")

    def generate_with_workflow(self, workflow_path, params):
        """워크플로우 파일로 이미지 생성"""
        with open(workflow_path, 'r') as f:
            workflow = json.load(f)

        # 파라미터 적용
        for node_id, node_params in params.items():
            if node_id in workflow:
                workflow[node_id]['inputs'].update(node_params)

        # 실행
        prompt_id = self.queue_prompt(workflow)
        print(f"실행 ID: {prompt_id}")

        # 결과 대기 (간단한 폴링)
        import time
        max_wait = 300  # 5분
        wait_time = 0

        while wait_time < max_wait:
            history = self.get_history(prompt_id)
            if prompt_id in history:
                return history[prompt_id]

            time.sleep(2)
            wait_time += 2

        raise Exception("타임아웃: 이미지 생성 시간 초과")


def create_default_config():
    """기본 설정 파일 생성"""
    config = {
        "api_type": "automatic1111",  # 또는 "comfyui"
        "api_url": "http://127.0.0.1:7860",
        "default_settings": {
            "width": 512,
            "height": 512,
            "steps": 30,
            "cfg_scale": 7.5,
            "ipadapter_weight": 0.8,
            "controlnet_weight": 0.8
        },
        "output": {
            "base_folder": "GeneratedCharacters",
            "format": "png",
            "quality": 95
        }
    }

    config_path = Path(__file__).parent.parent.parent / "AI_Training" / "character_gen_config.json"
    config_path.parent.mkdir(parents=True, exist_ok=True)

    with open(config_path, 'w', encoding='utf-8') as f:
        json.dump(config, f, indent=2, ensure_ascii=False)

    print(f"기본 설정 파일 생성: {config_path}")
    return config_path


def main():
    parser = argparse.ArgumentParser(
        description="캐릭터 일관성 유지 이미지 생성"
    )
    parser.add_argument(
        "--reference",
        required=True,
        help="참조 캐릭터 이미지 경로"
    )
    parser.add_argument(
        "--output",
        required=True,
        help="출력 폴더 경로"
    )
    parser.add_argument(
        "--character",
        required=True,
        help="캐릭터 이름"
    )
    parser.add_argument(
        "--api-url",
        default="http://127.0.0.1:7860",
        help="API URL (Automatic1111 WebUI)"
    )
    parser.add_argument(
        "--poses",
        action="store_true",
        help="포즈 세트 생성"
    )
    parser.add_argument(
        "--emotions",
        action="store_true",
        help="표정 세트 생성"
    )
    parser.add_argument(
        "--angles",
        action="store_true",
        help="각도 세트 생성"
    )
    parser.add_argument(
        "--all",
        action="store_true",
        help="전체 세트 생성 (포즈+표정+각도)"
    )
    parser.add_argument(
        "--custom-config",
        help="커스텀 포즈 설정 JSON 파일"
    )

    args = parser.parse_args()

    # 기본값: --all이 지정되지 않으면 모두 생성
    if not any([args.poses, args.emotions, args.angles, args.all]):
        args.all = True

    include_poses = args.poses or args.all
    include_emotions = args.emotions or args.all
    include_angles = args.angles or args.all

    try:
        # 생성기 초기화
        generator = CharacterImageGenerator(api_url=args.api_url)

        # API 연결 확인
        try:
            response = requests.get(f"{args.api_url}/sdapi/v1/sd-models")
            if response.status_code != 200:
                raise Exception("API 연결 실패")
            print("✓ API 연결 확인")
        except Exception as e:
            print(f"⚠ API 연결 실패: {e}")
            print("Automatic1111 WebUI가 실행 중인지 확인하세요.")
            print(f"API URL: {args.api_url}")
            sys.exit(1)

        # 커스텀 설정이 있으면 사용
        if args.custom_config:
            with open(args.custom_config, 'r', encoding='utf-8') as f:
                custom_configs = json.load(f)

            results = generator.batch_generate_poses(
                reference_image=args.reference,
                pose_configs=custom_configs,
                output_folder=args.output
            )
        else:
            # 캐릭터 세트 생성
            results = generator.generate_character_set(
                reference_image=args.reference,
                character_name=args.character,
                output_folder=args.output,
                include_poses=include_poses,
                include_emotions=include_emotions,
                include_angles=include_angles
            )

        # 결과 저장
        results_file = Path(args.output) / "generation_results.json"
        with open(results_file, 'w', encoding='utf-8') as f:
            json.dump(results, f, indent=2, ensure_ascii=False)

        print(f"\n결과 파일 저장: {results_file}")

    except Exception as e:
        print(f"\n❌ 오류 발생: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    # 설정 파일이 없으면 생성
    config_path = Path(__file__).parent.parent.parent / "AI_Training" / "character_gen_config.json"
    if not config_path.exists():
        print("설정 파일이 없습니다. 기본 설정 생성 중...")
        create_default_config()

    main()
