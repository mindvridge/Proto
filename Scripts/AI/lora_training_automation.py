#!/usr/bin/env python3
"""
LoRA 학습 자동화 스크립트
캐릭터 이미지를 이용한 일관성 있는 캐릭터 생성을 위한 LoRA 모델 학습
"""

import os
import sys
import json
import argparse
import subprocess
from pathlib import Path
from PIL import Image
import shutil

class LoRATrainingPipeline:
    """LoRA 학습 전체 파이프라인 자동화"""

    def __init__(self, config_path=None):
        self.config = self.load_config(config_path)
        self.project_root = Path(__file__).parent.parent.parent
        self.training_root = self.project_root / "AI_Training" / "LoRA"

    def load_config(self, config_path):
        """설정 파일 로드"""
        default_config = {
            "character_name": "minsu",
            "trigger_word": "minsu",
            "base_model": "animefull_latest.safetensors",
            "network_dim": 32,
            "network_alpha": 16,
            "learning_rate": 0.0001,
            "text_encoder_lr": 0.00005,
            "resolution": "512,512",
            "train_batch_size": 1,
            "max_train_epochs": 15,
            "save_every_n_epochs": 1,
            "mixed_precision": "fp16",
            "optimizer_type": "AdamW8bit",
            "lr_scheduler": "cosine_with_restarts",
            "clip_skip": 2,
            "min_snr_gamma": 5,
            "noise_offset": 0.05,
            "kohya_path": None,  # Kohya_ss 설치 경로
        }

        if config_path and os.path.exists(config_path):
            with open(config_path, 'r', encoding='utf-8') as f:
                user_config = json.load(f)
                default_config.update(user_config)

        return default_config

    def setup_folders(self, character_name):
        """학습용 폴더 구조 생성"""
        char_path = self.training_root / character_name
        folders = {
            'root': char_path,
            'img': char_path / 'img' / f"10_{self.config['trigger_word']}",
            'log': char_path / 'log',
            'model': char_path / 'model',
            'sample': char_path / 'sample'
        }

        for folder in folders.values():
            folder.mkdir(parents=True, exist_ok=True)

        print(f"✓ 폴더 구조 생성 완료: {char_path}")
        return folders

    def prepare_images(self, input_folder, output_folder, min_images=15):
        """이미지 전처리 및 준비"""
        print(f"\n이미지 전처리 중: {input_folder}")

        input_path = Path(input_folder)
        if not input_path.exists():
            raise ValueError(f"입력 폴더가 존재하지 않습니다: {input_folder}")

        # 지원 형식
        image_extensions = ['.png', '.jpg', '.jpeg', '.webp']
        image_files = []
        for ext in image_extensions:
            image_files.extend(input_path.glob(f'*{ext}'))
            image_files.extend(input_path.glob(f'*{ext.upper()}'))

        if len(image_files) < min_images:
            raise ValueError(
                f"이미지가 부족합니다. 최소 {min_images}장 필요, 현재 {len(image_files)}장"
            )

        print(f"발견된 이미지: {len(image_files)}장")

        # 이미지 처리 및 복사
        processed_count = 0
        for i, img_path in enumerate(image_files):
            try:
                # 이미지 로드 및 검증
                img = Image.open(img_path)

                # RGB로 변환 (RGBA 또는 다른 모드 처리)
                if img.mode != 'RGB':
                    img = img.convert('RGB')

                # 크기 확인 및 리사이즈 (필요시)
                target_size = tuple(map(int, self.config['resolution'].split(',')))
                if img.size != target_size:
                    # 비율 유지하며 리사이즈
                    img.thumbnail(target_size, Image.Resampling.LANCZOS)

                    # 정사각형으로 패딩
                    new_img = Image.new('RGB', target_size, (255, 255, 255))
                    paste_x = (target_size[0] - img.size[0]) // 2
                    paste_y = (target_size[1] - img.size[1]) // 2
                    new_img.paste(img, (paste_x, paste_y))
                    img = new_img

                # 저장
                output_path = output_folder / f"image_{i+1:03d}.png"
                img.save(output_path, 'PNG', quality=95)
                processed_count += 1

            except Exception as e:
                print(f"⚠ 이미지 처리 실패 ({img_path.name}): {e}")
                continue

        print(f"✓ 이미지 전처리 완료: {processed_count}장")
        return processed_count

    def generate_captions(self, image_folder, method='wd14', auto_trigger=True):
        """캡션 자동 생성"""
        print(f"\n캡션 생성 중 (방법: {method})")

        if method == 'wd14':
            self._generate_captions_wd14(image_folder, auto_trigger)
        elif method == 'blip':
            self._generate_captions_blip(image_folder, auto_trigger)
        elif method == 'manual':
            self._generate_captions_manual(image_folder)
        else:
            raise ValueError(f"지원하지 않는 캡션 생성 방법: {method}")

        print("✓ 캡션 생성 완료")

    def _generate_captions_wd14(self, image_folder, auto_trigger):
        """WD14 Tagger를 이용한 캡션 생성"""
        try:
            # WD14 Tagger 사용 (Kohya_ss에 포함)
            if self.config.get('kohya_path'):
                kohya_path = Path(self.config['kohya_path'])
                tagger_script = kohya_path / "finetune" / "tag_images_by_wd14_tagger.py"

                if tagger_script.exists():
                    cmd = [
                        sys.executable,
                        str(tagger_script),
                        str(image_folder),
                        "--batch_size", "4",
                        "--thresh", "0.35",
                        "--remove_underscore"
                    ]
                    subprocess.run(cmd, check=True)

                    # 트리거 워드 추가
                    if auto_trigger:
                        self._add_trigger_word(image_folder)
                    return

            # Kohya 없으면 간단한 캡션 생성
            print("⚠ WD14 Tagger 없음. 기본 캡션 사용")
            self._generate_captions_manual(image_folder)

        except Exception as e:
            print(f"⚠ WD14 캡션 생성 실패: {e}")
            print("기본 캡션으로 대체합니다.")
            self._generate_captions_manual(image_folder)

    def _generate_captions_blip(self, image_folder, auto_trigger):
        """BLIP를 이용한 캡션 생성"""
        try:
            from transformers import BlipProcessor, BlipForConditionalGeneration
            import torch
            from PIL import Image

            print("BLIP 모델 로딩 중...")
            processor = BlipProcessor.from_pretrained("Salesforce/blip-image-captioning-base")
            model = BlipForConditionalGeneration.from_pretrained(
                "Salesforce/blip-image-captioning-base"
            )

            if torch.cuda.is_available():
                model = model.to("cuda")

            image_files = list(Path(image_folder).glob("*.png"))

            for img_path in image_files:
                try:
                    img = Image.open(img_path).convert('RGB')

                    inputs = processor(img, return_tensors="pt")
                    if torch.cuda.is_available():
                        inputs = inputs.to("cuda")

                    out = model.generate(**inputs, max_length=50)
                    caption = processor.decode(out[0], skip_special_tokens=True)

                    # 트리거 워드 추가
                    if auto_trigger:
                        caption = f"{self.config['trigger_word']}, {caption}"

                    # 캡션 파일 저장
                    caption_path = img_path.with_suffix('.txt')
                    with open(caption_path, 'w', encoding='utf-8') as f:
                        f.write(caption)

                except Exception as e:
                    print(f"⚠ {img_path.name} 캡션 생성 실패: {e}")
                    continue

        except ImportError:
            print("⚠ transformers 라이브러리 필요: pip install transformers")
            self._generate_captions_manual(image_folder)
        except Exception as e:
            print(f"⚠ BLIP 캡션 생성 실패: {e}")
            self._generate_captions_manual(image_folder)

    def _generate_captions_manual(self, image_folder):
        """수동 템플릿 기반 캡션 생성"""
        trigger = self.config['trigger_word']

        # 기본 템플릿
        templates = [
            f"{trigger}, character, standing, neutral expression, high quality",
            f"{trigger}, character, portrait, detailed face, masterpiece",
            f"{trigger}, character, full body, anime style, best quality",
            f"{trigger}, character, side view, clean background",
            f"{trigger}, character, action pose, dynamic, high quality",
        ]

        image_files = list(Path(image_folder).glob("*.png"))

        for i, img_path in enumerate(image_files):
            caption = templates[i % len(templates)]

            caption_path = img_path.with_suffix('.txt')
            with open(caption_path, 'w', encoding='utf-8') as f:
                f.write(caption)

    def _add_trigger_word(self, image_folder):
        """기존 캡션에 트리거 워드 추가"""
        trigger = self.config['trigger_word']
        caption_files = list(Path(image_folder).glob("*.txt"))

        for caption_path in caption_files:
            with open(caption_path, 'r', encoding='utf-8') as f:
                content = f.read().strip()

            # 트리거 워드가 없으면 추가
            if trigger not in content:
                content = f"{trigger}, {content}"
                with open(caption_path, 'w', encoding='utf-8') as f:
                    f.write(content)

    def create_training_config(self, folders):
        """학습 설정 파일 생성"""
        config_path = folders['root'] / 'training_config.json'

        training_config = {
            "model": {
                "pretrained_model_name_or_path": self.config['base_model'],
                "network_module": "networks.lora",
                "network_dim": self.config['network_dim'],
                "network_alpha": self.config['network_alpha']
            },
            "dataset": {
                "train_data_dir": str(folders['img'].parent),
                "resolution": self.config['resolution'],
                "train_batch_size": self.config['train_batch_size']
            },
            "training": {
                "learning_rate": self.config['learning_rate'],
                "text_encoder_lr": self.config['text_encoder_lr'],
                "max_train_epochs": self.config['max_train_epochs'],
                "save_every_n_epochs": self.config['save_every_n_epochs'],
                "mixed_precision": self.config['mixed_precision'],
                "optimizer_type": self.config['optimizer_type'],
                "lr_scheduler": self.config['lr_scheduler']
            },
            "advanced": {
                "clip_skip": self.config['clip_skip'],
                "min_snr_gamma": self.config['min_snr_gamma'],
                "noise_offset": self.config['noise_offset']
            },
            "output": {
                "output_dir": str(folders['model']),
                "logging_dir": str(folders['log'])
            }
        }

        with open(config_path, 'w', encoding='utf-8') as f:
            json.dump(training_config, f, indent=2, ensure_ascii=False)

        print(f"✓ 학습 설정 저장: {config_path}")
        return config_path

    def start_training(self, config_path, folders):
        """LoRA 학습 시작"""
        print("\n" + "="*50)
        print("LoRA 학습 시작")
        print("="*50)

        if not self.config.get('kohya_path'):
            print("\n⚠ Kohya_ss 경로가 설정되지 않았습니다.")
            print("수동 학습 방법:")
            print(f"1. Kohya_ss GUI 실행")
            print(f"2. 설정 파일 로드: {config_path}")
            print(f"3. 학습 시작")
            return False

        kohya_path = Path(self.config['kohya_path'])
        train_script = kohya_path / "train_network.py"

        if not train_script.exists():
            print(f"⚠ 학습 스크립트를 찾을 수 없습니다: {train_script}")
            return False

        # accelerate를 이용한 학습 실행
        cmd = [
            "accelerate", "launch",
            "--num_cpu_threads_per_process=2",
            str(train_script),
            f"--pretrained_model_name_or_path={self.config['base_model']}",
            f"--train_data_dir={folders['img'].parent}",
            f"--output_dir={folders['model']}",
            f"--logging_dir={folders['log']}",
            "--network_module=networks.lora",
            f"--network_dim={self.config['network_dim']}",
            f"--network_alpha={self.config['network_alpha']}",
            f"--resolution={self.config['resolution']}",
            f"--train_batch_size={self.config['train_batch_size']}",
            f"--learning_rate={self.config['learning_rate']}",
            f"--text_encoder_lr={self.config['text_encoder_lr']}",
            f"--max_train_epochs={self.config['max_train_epochs']}",
            f"--save_every_n_epochs={self.config['save_every_n_epochs']}",
            f"--mixed_precision={self.config['mixed_precision']}",
            "--save_precision=fp16",
            "--cache_latents",
            f"--optimizer_type={self.config['optimizer_type']}",
            f"--lr_scheduler={self.config['lr_scheduler']}",
            f"--clip_skip={self.config['clip_skip']}",
            f"--min_snr_gamma={self.config['min_snr_gamma']}",
            f"--noise_offset={self.config['noise_offset']}"
        ]

        try:
            print("\n학습 명령어:")
            print(" ".join(cmd))
            print("\n학습이 시작됩니다... (30분~2시간 소요)")

            subprocess.run(cmd, check=True, cwd=str(kohya_path))

            print("\n" + "="*50)
            print("✓ LoRA 학습 완료!")
            print("="*50)
            print(f"모델 저장 위치: {folders['model']}")
            return True

        except subprocess.CalledProcessError as e:
            print(f"\n❌ 학습 실패: {e}")
            return False
        except Exception as e:
            print(f"\n❌ 예상치 못한 오류: {e}")
            return False

    def test_lora(self, lora_path, output_folder, test_prompts=None):
        """학습된 LoRA 테스트"""
        print("\n" + "="*50)
        print("LoRA 테스트")
        print("="*50)

        if test_prompts is None:
            trigger = self.config['trigger_word']
            test_prompts = [
                f"{trigger}, standing, smiling, front view",
                f"{trigger}, action pose, determined expression",
                f"{trigger}, sitting, relaxed, side view",
                f"{trigger}, portrait, close up, happy",
                f"{trigger}, full body, dynamic pose"
            ]

        print("테스트 프롬프트:")
        for i, prompt in enumerate(test_prompts, 1):
            print(f"{i}. {prompt}")

        print("\n테스트 생성:")
        print(f"1. Automatic1111 WebUI 또는 ComfyUI 실행")
        print(f"2. LoRA 로드: {lora_path}")
        print(f"3. 위 프롬프트로 이미지 생성")
        print(f"4. 결과 저장: {output_folder}")

    def run_pipeline(self, input_images_folder, caption_method='wd14'):
        """전체 파이프라인 실행"""
        print("\n" + "="*50)
        print("LoRA 학습 파이프라인 시작")
        print("="*50)
        print(f"캐릭터: {self.config['character_name']}")
        print(f"트리거 워드: {self.config['trigger_word']}")
        print("="*50)

        # 1. 폴더 설정
        folders = self.setup_folders(self.config['character_name'])

        # 2. 이미지 전처리
        num_images = self.prepare_images(input_images_folder, folders['img'])

        # 3. 캡션 생성
        self.generate_captions(folders['img'], method=caption_method)

        # 4. 학습 설정 생성
        config_path = self.create_training_config(folders)

        # 5. 학습 시작
        success = self.start_training(config_path, folders)

        if success:
            # 6. 테스트
            lora_files = list(folders['model'].glob("*.safetensors"))
            if lora_files:
                latest_lora = max(lora_files, key=lambda p: p.stat().st_mtime)
                self.test_lora(latest_lora, folders['sample'])

        print("\n" + "="*50)
        print("파이프라인 완료")
        print("="*50)

        return success


def main():
    parser = argparse.ArgumentParser(
        description="LoRA 학습 자동화 스크립트"
    )
    parser.add_argument(
        "--input",
        required=True,
        help="입력 이미지 폴더 경로"
    )
    parser.add_argument(
        "--character",
        required=True,
        help="캐릭터 이름 (예: minsu, minji)"
    )
    parser.add_argument(
        "--trigger",
        help="트리거 워드 (기본값: 캐릭터 이름과 동일)"
    )
    parser.add_argument(
        "--config",
        help="설정 파일 경로 (JSON)"
    )
    parser.add_argument(
        "--caption-method",
        choices=['wd14', 'blip', 'manual'],
        default='manual',
        help="캡션 생성 방법"
    )
    parser.add_argument(
        "--kohya-path",
        help="Kohya_ss 설치 경로"
    )
    parser.add_argument(
        "--epochs",
        type=int,
        default=15,
        help="학습 에폭 수"
    )
    parser.add_argument(
        "--dim",
        type=int,
        default=32,
        help="Network Dimension"
    )

    args = parser.parse_args()

    # 설정 오버라이드
    config_overrides = {
        "character_name": args.character,
        "trigger_word": args.trigger or args.character,
        "max_train_epochs": args.epochs,
        "network_dim": args.dim
    }

    if args.kohya_path:
        config_overrides["kohya_path"] = args.kohya_path

    # 설정 파일이 있으면 로드
    pipeline = LoRATrainingPipeline(args.config)

    # 명령줄 인자로 오버라이드
    pipeline.config.update(config_overrides)

    # 파이프라인 실행
    try:
        success = pipeline.run_pipeline(
            args.input,
            caption_method=args.caption_method
        )

        sys.exit(0 if success else 1)

    except Exception as e:
        print(f"\n❌ 오류 발생: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
