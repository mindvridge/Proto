# 캐릭터 일관성 유지 AI 이미지 생성 가이드

## 📋 목차
1. [개요](#개요)
2. [방법 비교](#방법-비교)
3. [방법 1: LoRA Training (가장 추천)](#방법-1-lora-training)
4. [방법 2: IP-Adapter + ControlNet](#방법-2-ip-adapter--controlnet)
5. [방법 3: InstantID (얼굴 일관성)](#방법-3-instantid)
6. [방법 4: Consistent Character (Fooocus)](#방법-4-consistent-character)
7. [통합 워크플로우](#통합-워크플로우)
8. [Unity 통합](#unity-통합)
9. [품질 관리](#품질-관리)

---

## 개요

**목표**: 캐릭터 이미지 한 장(또는 소수)을 기반으로 다양한 포즈, 표정, 각도에서 일관성 있는 캐릭터 이미지를 생성

**주요 도전 과제**:
- 얼굴 특징 일관성
- 복장/의상 유지
- 헤어스타일 일관성
- 캐릭터 고유의 디테일 보존
- 다양한 포즈/표정 생성

**게임 개발 요구사항**:
- 배틀 스프라이트 (공격, 방어, 피격 등)
- 감정 표현 (기쁨, 슬픔, 분노 등)
- UI 초상화
- 스토리 컷씬
- 레벨업/진화 이미지

---

## 방법 비교

### 종합 비교표

| 방법 | 일관성 | 학습시간 | 필요이미지 | 유연성 | 난이도 | 추천용도 |
|------|--------|----------|------------|--------|--------|----------|
| **LoRA Training** | ⭐⭐⭐⭐⭐ | 30분-2시간 | 15-30장 | ⭐⭐⭐⭐⭐ | 중간 | **메인 캐릭터** |
| **IP-Adapter** | ⭐⭐⭐⭐ | 없음 | 1-3장 | ⭐⭐⭐⭐ | 쉬움 | **빠른 프로토타입** |
| **InstantID** | ⭐⭐⭐⭐ | 없음 | 1장 (얼굴) | ⭐⭐⭐ | 쉬움 | **얼굴 중심 캐릭터** |
| **ControlNet Ref** | ⭐⭐⭐ | 없음 | 1장 | ⭐⭐⭐ | 쉬움 | **스타일 참조** |
| **Fooocus Consistent** | ⭐⭐⭐⭐ | 없음 | 1장 | ⭐⭐⭐ | 매우쉬움 | **초보자/테스트** |

### 추천 전략

**시나리오별 최적 방법**:

1. **메인 캐릭터 (김민수, 김민지 등)**
   - **방법**: LoRA Training
   - **이유**: 최고 수준의 일관성, 완전한 제어
   - **투자**: 이미지 20-30장 준비 + 1시간 학습

2. **몬스터/적 캐릭터 (다양한 종류)**
   - **방법**: IP-Adapter + ControlNet
   - **이유**: 학습 없이 빠른 생성, 다양한 변형 가능
   - **투자**: 참조 이미지 1-2장만 필요

3. **NPC/서브 캐릭터**
   - **방법**: IP-Adapter 또는 Fooocus
   - **이유**: 빠른 생성, 높은 수준의 일관성
   - **투자**: 최소한의 이미지

4. **프로토타입/테스트**
   - **방법**: Fooocus Consistent Character
   - **이유**: 가장 쉽고 빠름, 설치 간단
   - **투자**: 거의 없음

---

## 방법 1: LoRA Training

### 개요
**LoRA (Low-Rank Adaptation)**는 Stable Diffusion 모델을 특정 캐릭터에 맞게 미세 조정하는 기술입니다.

### 장점
- ✅ 최고 수준의 캐릭터 일관성
- ✅ 다양한 포즈, 표정, 각도 생성 가능
- ✅ 복장, 헤어, 얼굴 특징 완벽 재현
- ✅ 텍스트 프롬프트로 완전한 제어
- ✅ 작은 파일 크기 (10-200MB)
- ✅ 다른 LoRA와 조합 가능

### 단점
- ❌ 학습 시간 필요 (30분-2시간)
- ❌ 학습용 이미지 15-30장 필요
- ❌ GPU 필요 (최소 6GB VRAM)
- ❌ 기술적 지식 필요

### 필요 환경

#### 하드웨어
- **최소**: NVIDIA GPU 6GB VRAM (GTX 1660 Ti)
- **권장**: NVIDIA GPU 12GB+ VRAM (RTX 3060 이상)
- **최적**: NVIDIA GPU 24GB VRAM (RTX 4090)
- RAM: 16GB+
- 저장공간: 50GB+

#### 소프트웨어
```bash
# 1. Python 3.10 설치
# 2. Git 설치
# 3. CUDA 11.8+ 설치

# 4. Kohya_ss GUI 설치 (추천)
git clone https://github.com/bmaltais/kohya_ss.git
cd kohya_ss
python -m venv venv
venv\Scripts\activate  # Windows
source venv/bin/activate  # Linux/Mac
pip install -r requirements.txt

# 또는 자동 설치 스크립트 실행
./setup.bat  # Windows
./setup.sh   # Linux/Mac
```

### 학습 데이터 준비

#### 1. 이미지 수집
```
필요 이미지: 15-30장
품질: 512x512 이상, 고해상도
다양성: 필수!

필수 포함:
- 정면, 측면, 3/4 각도 (각 3-5장)
- 다양한 표정 (웃음, 중립, 슬픔, 분노 등)
- 다양한 포즈 (서있기, 앉기, 동작)
- 전신, 반신, 클로즈업 (다양한 구도)
- 기본 의상 + 변형 의상 (선택)

피해야 할 것:
- 너무 어두운/밝은 이미지
- 흐릿한 이미지
- 다른 캐릭터와 겹친 이미지
- 극단적으로 변형된 각도
```

#### 2. 폴더 구조
```
lora_training/
├── character_name/
│   ├── img/
│   │   ├── 10_character_name/          # 반복 횟수_트리거 워드
│   │   │   ├── image_001.png
│   │   │   ├── image_002.png
│   │   │   ├── ...
│   │   │   ├── image_001.txt           # 캡션 (선택)
│   │   │   └── image_002.txt
│   ├── log/                             # 학습 로그
│   └── model/                           # 출력 모델
```

#### 3. 캡션 작성 (선택이지만 권장)

각 이미지에 대응하는 `.txt` 파일:

```txt
# image_001.txt (정면 웃는 얼굴)
minsu, male character, front view, smiling, happy expression, white shirt, blue pants, brown hair, detailed face

# image_002.txt (측면 중립)
minsu, male character, side view, neutral expression, profile, white shirt, blue pants, brown hair

# image_003.txt (전투 포즈)
minsu, male character, action pose, determined expression, raising fist, white shirt, blue pants, brown hair, dynamic

# 트리거 워드: "minsu" - 반드시 포함!
# 일관된 설명 사용
# 포즈, 표정, 의상 등 명확히 설명
```

**자동 캡션 생성** (BLIP/WD14 Tagger):
```bash
# Kohya_ss GUI에서:
# Utilities > Captioning > WD14 Tagger
# - 이미지 폴더 선택
# - "Remove underscore" 체크
# - "Use CPU" (GPU 사용 가능하면 해제)
# - "Caption" 클릭

# 또는 Python 스크립트 사용 (Scripts/AI/auto_caption.py 참조)
```

### LoRA 학습 설정

#### Kohya_ss GUI 사용 (초보자 추천)

```bash
# Kohya_ss 실행
cd kohya_ss
./gui.bat  # Windows
./gui.sh   # Linux/Mac

# 브라우저에서 http://localhost:7860 열기
```

**학습 설정 (LoRA 탭)**:

1. **Model 탭**:
   ```
   Base Model: animefull_latest.safetensors (또는 선호 모델)
   LoRA Type: Standard
   Network Rank (Dimension): 32 (권장: 16-128)
   Network Alpha: 16 (일반적으로 Rank의 절반)
   ```

2. **Dataset 탭**:
   ```
   Image Folder: /path/to/lora_training/character_name/img
   Output Folder: /path/to/lora_training/character_name/model
   Logging Folder: /path/to/lora_training/character_name/log

   Resolution: 512,512 (또는 768,768)
   Batch Size: 1-4 (VRAM에 따라)
   ```

3. **Training Parameters**:
   ```
   Learning Rate: 0.0001 (1e-4)
   Text Encoder LR: 0.00005 (5e-5)

   Train Batch Size: 1
   Epochs: 10-20
   Save Every N Epochs: 1

   Optimizer: AdamW8bit
   Scheduler: cosine_with_restarts

   Mixed Precision: fp16
   Cache Latents: Yes
   ```

4. **고급 설정**:
   ```
   Clip Skip: 2
   Min SNR Gamma: 5
   Noise Offset: 0.05

   Enable Sample Generation: Yes (진행상황 확인용)
   Sample Prompts: "minsu, standing, smiling"
   ```

**학습 시작**:
```
"Start Training" 버튼 클릭
진행상황: Terminal/로그 파일 확인
샘플 이미지: log 폴더에 자동 생성
```

#### 명령줄 사용 (고급 사용자)

```bash
# train_lora.sh
accelerate launch --num_cpu_threads_per_process=2 \
  train_network.py \
  --pretrained_model_name_or_path="./models/animefull_latest.safetensors" \
  --train_data_dir="./lora_training/minsu/img" \
  --output_dir="./lora_training/minsu/model" \
  --logging_dir="./lora_training/minsu/log" \
  --network_module=networks.lora \
  --network_dim=32 \
  --network_alpha=16 \
  --resolution="512,512" \
  --train_batch_size=1 \
  --learning_rate=0.0001 \
  --text_encoder_lr=0.00005 \
  --max_train_epochs=15 \
  --save_every_n_epochs=1 \
  --mixed_precision="fp16" \
  --save_precision="fp16" \
  --cache_latents \
  --optimizer_type="AdamW8bit" \
  --lr_scheduler="cosine_with_restarts" \
  --clip_skip=2 \
  --min_snr_gamma=5 \
  --noise_offset=0.05
```

### LoRA 사용법

#### Automatic1111 WebUI
```python
# 프롬프트 예시
Positive Prompt:
"minsu, male character, standing pose, confident smile, raising fist, battle ready,
white shirt, blue pants, brown hair, detailed face, high quality, masterpiece,
detailed background, outdoor"

# LoRA 적용 문법
<lora:character_minsu:0.8>

# 완전한 프롬프트
"<lora:character_minsu:0.8>, minsu, male character, action pose, determined expression,
dynamic movement, white shirt, masterpiece, best quality"

Negative Prompt:
"low quality, blurry, deformed, multiple people, wrong anatomy, bad hands"

Settings:
- Sampling Method: DPM++ 2M Karras
- Steps: 30-40
- CFG Scale: 7-8
- LoRA Weight: 0.6-1.0 (조절 가능)
```

#### ComfyUI
```
노드 구성:
Load Checkpoint → Load LoRA → Positive CLIP → KSampler → VAE Decode → Save

Load LoRA 노드:
- lora_name: character_minsu.safetensors
- strength_model: 0.8
- strength_clip: 0.8
```

#### Python (Diffusers)
```python
from diffusers import StableDiffusionPipeline
import torch

# 파이프라인 로드
pipe = StableDiffusionPipeline.from_pretrained(
    "runwayml/stable-diffusion-v1-5",
    torch_dtype=torch.float16
).to("cuda")

# LoRA 가중치 로드
pipe.load_lora_weights("./lora_training/minsu/model/character_minsu.safetensors")

# 이미지 생성
prompt = "minsu, male character, confident smile, action pose, white shirt, masterpiece"
negative = "low quality, blurry, deformed"

image = pipe(
    prompt=prompt,
    negative_prompt=negative,
    num_inference_steps=30,
    guidance_scale=7.5,
    cross_attention_kwargs={"scale": 0.8}  # LoRA 강도
).images[0]

image.save("minsu_generated.png")
```

### 학습 팁 & 트러블슈팅

#### 최적의 결과를 위한 팁

1. **이미지 품질**:
   - 고해상도 원본 사용
   - 일관된 아트 스타일
   - 명확한 얼굴/디테일

2. **다양성**:
   - 최소 3가지 각도
   - 5가지 이상 표정
   - 다양한 포즈

3. **학습 파라미터**:
   - Rank 32: 일반적인 캐릭터
   - Rank 64-128: 복잡한 디테일/의상
   - Learning Rate 너무 높으면 과적합

4. **반복 횟수**:
   - 이미지 20장 → 10 epochs (200 steps)
   - 이미지 30장 → 15 epochs (450 steps)
   - Total Steps: 300-500 권장

#### 문제 해결

**문제**: 캐릭터가 정확히 재현되지 않음
```
해결책:
- LoRA 가중치 증가 (0.8 → 1.0)
- Network Rank 증가 (32 → 64)
- 더 많은 학습 이미지 추가
- 캡션에 트리거 워드 확실히 포함
```

**문제**: 과적합 (같은 포즈만 생성)
```
해결책:
- Epochs 감소 (20 → 10)
- Learning Rate 감소
- 더 다양한 학습 이미지 추가
- Noise Offset 증가 (0.1)
```

**문제**: 의상이 바뀜
```
해결책:
- 캡션에 의상 설명 강화
- 의상 일관성 있는 이미지만 사용
- 프롬프트에 의상 명시적으로 기술
```

**문제**: 얼굴은 비슷하지만 다른 사람처럼 보임
```
해결책:
- 얼굴 클로즈업 이미지 추가
- 다양한 각도의 얼굴 이미지
- Text Encoder LR 증가
```

---

## 방법 2: IP-Adapter + ControlNet

### 개요
**IP-Adapter (Image Prompt Adapter)**는 이미지를 프롬프트처럼 사용하여 학습 없이 캐릭터 일관성을 유지합니다.

### 장점
- ✅ 학습 불필요 (즉시 사용)
- ✅ 참조 이미지 1-3장만 필요
- ✅ ControlNet과 조합으로 포즈 제어
- ✅ 빠른 생성 속도
- ✅ 다양한 스타일 적용 가능

### 단점
- ❌ LoRA보다 일관성 낮음
- ❌ 복잡한 의상/디테일 재현 어려움
- ❌ 참조 이미지 품질에 크게 의존

### 필요 모델

```bash
# IP-Adapter 모델 다운로드
# ComfyUI/models/ipadapter/

ip-adapter_sd15.safetensors          # SD 1.5용
ip-adapter_sd15_light.safetensors    # 경량 버전
ip-adapter_sdxl.safetensors          # SDXL용
ip-adapter-plus_sd15.safetensors     # 강화 버전 (추천)
ip-adapter-plus-face_sd15.safetensors # 얼굴 특화

# CLIP Vision 모델 (필수)
# ComfyUI/models/clip_vision/
model.safetensors                     # OpenAI CLIP ViT-H/14

# ControlNet 모델
# ComfyUI/models/controlnet/
control_sd15_openpose.pth             # 포즈 제어
control_sd15_depth.pth                # 깊이 제어
control_sd15_canny.pth                # 엣지 제어
```

### 사용법

#### ComfyUI (추천)

**기본 워크플로우**:
```
Load Image (참조) → IPAdapter → Load Checkpoint → CLIP → KSampler → VAE Decode → Save
                                      ↓
                                 ControlNet (포즈)
```

**상세 노드 구성**:

1. **참조 이미지 로드**:
```
LoadImage Node:
- 캐릭터 참조 이미지
- 512x512 이상 권장
```

2. **IP-Adapter 적용**:
```
IPAdapter Node:
- Model: ip-adapter-plus_sd15.safetensors
- Weight: 0.7-0.9
- Start: 0.0
- End: 0.8 (생성 과정의 80%까지만 적용)
```

3. **ControlNet 포즈 제어** (선택):
```
Load ControlNet → Apply ControlNet

ControlNet Settings:
- Model: control_sd15_openpose
- Strength: 0.8
- Pose Image: OpenPose 스켈레톤 이미지
```

4. **프롬프트**:
```python
Positive:
"1girl, standing pose, smiling, white dress, outdoor, high quality, masterpiece"

# 참조 이미지의 스타일을 따르므로 간단한 프롬프트 사용
# 캐릭터 특징은 IP-Adapter가 처리

Negative:
"low quality, blurry, deformed, multiple people"
```

#### Automatic1111 WebUI

```
# IP-Adapter Extension 설치 필요
Extensions → Install from URL
https://github.com/toshiaki1729/stable-diffusion-webui-ip-adapter

# 사용법
1. txt2img 또는 img2img 탭
2. IP-Adapter 섹션 확장
3. Enable 체크
4. 참조 이미지 업로드
5. Weight: 0.7-0.9
6. 생성
```

### IP-Adapter + ControlNet 조합

**최고의 조합**: IP-Adapter (캐릭터) + OpenPose ControlNet (포즈)

```python
# 워크플로우
1. 참조 캐릭터 이미지 준비
2. 원하는 포즈의 OpenPose 이미지 생성/다운로드
3. IP-Adapter로 캐릭터 특징 입력
4. ControlNet으로 포즈 제어
5. 텍스트 프롬프트로 배경/분위기 조정

# 결과: 일관된 캐릭터 + 원하는 포즈
```

**OpenPose 이미지 생성**:

옵션 1: 직접 그리기
- OpenPose Editor 사용
- 스틱맨 형태로 포즈 그리기

옵션 2: 기존 이미지에서 추출
```python
# ControlNet Preprocessor 사용
Input Image → OpenPose Preprocessor → Pose Skeleton

# 또는 ComfyUI
LoadImage → DWPose Estimator → Save
```

옵션 3: 3D 소프트웨어
- Blender + OpenPose Plugin
- DAZ Studio
- Character Creator

### 실전 예제

```python
# 예제 1: 다양한 포즈의 김민수 생성

참조 이미지: minsu_reference.png (정면, 기본 포즈)

생성 1 - 전투 포즈:
- IP-Adapter: minsu_reference.png (weight: 0.8)
- ControlNet OpenPose: punch_pose.png
- Prompt: "male character, action pose, determined, outdoor"
- Result: 김민수가 펀치 포즈

생성 2 - 앉아있는 포즈:
- IP-Adapter: minsu_reference.png (weight: 0.8)
- ControlNet OpenPose: sitting_pose.png
- Prompt: "male character, sitting, relaxed, indoor"
- Result: 김민수가 앉아있음

생성 3 - 점프:
- IP-Adapter: minsu_reference.png (weight: 0.9)
- ControlNet OpenPose: jump_pose.png
- Prompt: "male character, jumping, excited, outdoor"
- Result: 김민수가 점프
```

### 고급 기법

#### 1. Multi-Reference (여러 참조 이미지)
```
참조 이미지 1: 얼굴 클로즈업 (weight: 0.6)
참조 이미지 2: 전신 의상 (weight: 0.4)

→ 얼굴과 의상 모두 일관성 유지
```

#### 2. IP-Adapter Face + Plus 조합
```
IPAdapter Face: 얼굴 특징 (weight: 0.7)
IPAdapter Plus: 전체 스타일 (weight: 0.5)

→ 얼굴은 정확히, 전체는 스타일 유지
```

#### 3. 지역별 ControlNet
```
ControlNet 1 (OpenPose): 전체 포즈
ControlNet 2 (Canny): 얼굴 디테일

→ 포즈와 디테일 동시 제어
```

---

## 방법 3: InstantID

### 개요
**InstantID**는 단일 얼굴 이미지로 캐릭터 일관성을 유지하는 최신 기술입니다.

### 장점
- ✅ 얼굴 이미지 1장만 필요
- ✅ 매우 빠른 생성
- ✅ 높은 얼굴 일관성
- ✅ ControlNet과 자연스러운 통합

### 단점
- ❌ 얼굴 중심 (의상 일관성 낮음)
- ❌ ComfyUI만 지원 (현재)
- ❌ 추가 모델 다운로드 필요

### 설치 및 사용

```bash
# ComfyUI Custom Nodes
cd ComfyUI/custom_nodes/
git clone https://github.com/cubiq/ComfyUI_InstantID.git
cd ComfyUI_InstantID
pip install -r requirements.txt

# 모델 다운로드
# models/instantid/
ip-adapter.bin
ControlNetModel/diffusion_pytorch_model.safetensors

# models/insightface/
antelopev2 (폴더)
```

**ComfyUI 노드**:
```
Load Face Image → InstantID → KSampler
                      ↓
                  ControlNet
```

**설정**:
```
InstantID:
- Weight: 0.8
- Noise: 0.3

ControlNet:
- Model: instantid_controlnet
- Strength: 0.8
```

---

## 방법 4: Consistent Character (Fooocus)

### 개요
**Fooocus Consistent Character**는 가장 사용하기 쉬운 방법입니다.

### 장점
- ✅ 매우 쉬운 사용법
- ✅ 별도 학습/설정 불필요
- ✅ 좋은 결과 품질
- ✅ 빠른 설치

### 단점
- ❌ 제어권 제한적
- ❌ 배치 처리 어려움
- ❌ 자동화 제한적

### 사용법

```bash
# Fooocus 설치
git clone https://github.com/lllyasviel/Fooocus.git
cd Fooocus
python launch.py

# 브라우저에서 http://localhost:7865
```

**Consistent Character 사용**:
1. Input Image 탭에서 캐릭터 이미지 업로드
2. "FaceSwap" 또는 "Image Prompt" 활성화
3. 프롬프트 입력
4. Advanced → Image Prompt → Weight 조절 (0.7-0.9)
5. Generate

---

## 통합 워크플로우

### 게임 개발 권장 워크플로우

```
1. 프로토타입 단계:
   → Fooocus 또는 IP-Adapter
   → 빠르게 다양한 디자인 테스트

2. 메인 캐릭터 확정:
   → LoRA 학습
   → 20-30장 이미지 준비
   → 1-2시간 학습
   → 완벽한 일관성 확보

3. 서브 캐릭터/몬스터:
   → IP-Adapter + ControlNet
   → 빠른 생성, 충분한 품질

4. 배치 생성:
   → Python 자동화 스크립트
   → 수백 장 자동 생성
```

### 하이브리드 접근

**최고의 결과를 위한 조합**:

```python
# 메인 캐릭터 (김민수)
1. LoRA 학습 → 기본 캐릭터 확립
2. IP-Adapter → 빠른 변형 생성
3. ControlNet → 정확한 포즈 제어

# 생성 파이프라인
LoRA (캐릭터) + IP-Adapter (보조) + ControlNet (포즈) + 텍스트 프롬프트 (디테일)

# 결과: 최고 수준의 일관성 + 완전한 제어
```

---

## Unity 통합

### 생성된 이미지 자동 임포트

```csharp
// Scripts/Editor/CharacterImageImporter.cs

using UnityEngine;
using UnityEditor;
using System.IO;

public class CharacterImageImporter : AssetPostprocessor
{
    // AI 생성 이미지 자동 감지 및 설정
    void OnPreprocessTexture()
    {
        if (assetPath.Contains("GeneratedCharacters"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;

            // 스프라이트 설정
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePivot = new Vector2(0.5f, 0f); // 하단 중앙

            // 압축 설정
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 2048;

            // 필터 모드
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
        }
    }
}
```

### 캐릭터 스프라이트 관리자

```csharp
// Scripts/CharacterSpriteManager.cs

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterSpriteSet", menuName = "Game/Character Sprite Set")]
public class CharacterSpriteSet : ScriptableObject
{
    [System.Serializable]
    public class PoseSprites
    {
        public string poseName;
        public Sprite[] sprites; // 애니메이션 프레임들
    }

    [System.Serializable]
    public class EmotionSprites
    {
        public string emotionName;
        public Sprite sprite;
    }

    public string characterId;
    public string characterName;

    [Header("포즈별 스프라이트")]
    public List<PoseSprites> poses = new List<PoseSprites>();

    [Header("표정별 스프라이트")]
    public List<EmotionSprites> emotions = new List<EmotionSprites>();

    [Header("UI 초상화")]
    public Sprite portraitNeutral;
    public Sprite portraitHappy;
    public Sprite portraitSad;
    public Sprite portraitAngry;

    // 포즈 스프라이트 가져오기
    public Sprite[] GetPoseSprites(string poseName)
    {
        PoseSprites pose = poses.Find(p => p.poseName == poseName);
        return pose?.sprites;
    }

    // 표정 스프라이트 가져오기
    public Sprite GetEmotionSprite(string emotionName)
    {
        EmotionSprites emotion = emotions.Find(e => e.emotionName == emotionName);
        return emotion?.sprite;
    }
}
```

### 동적 캐릭터 표시

```csharp
// Scripts/DynamicCharacterDisplay.cs

using UnityEngine;
using UnityEngine.UI;

public class DynamicCharacterDisplay : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private CharacterSpriteSet spriteSet;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 포즈 변경
    public void SetPose(string poseName)
    {
        Sprite[] poseSprites = spriteSet.GetPoseSprites(poseName);
        if (poseSprites != null && poseSprites.Length > 0)
        {
            characterImage.sprite = poseSprites[0];

            // 애니메이션 있으면 재생
            if (poseSprites.Length > 1)
            {
                StartCoroutine(AnimatePose(poseSprites));
            }
        }
    }

    // 표정 변경
    public void SetEmotion(string emotionName)
    {
        Sprite emotionSprite = spriteSet.GetEmotionSprite(emotionName);
        if (emotionSprite != null)
        {
            characterImage.sprite = emotionSprite;
        }
    }

    // 포즈 애니메이션
    private System.Collections.IEnumerator AnimatePose(Sprite[] frames)
    {
        int frameIndex = 0;
        float frameDuration = 0.1f; // 100ms per frame

        while (true)
        {
            characterImage.sprite = frames[frameIndex];
            frameIndex = (frameIndex + 1) % frames.Length;
            yield return new WaitForSeconds(frameDuration);
        }
    }

    // AI 생성 이미지 로드 (런타임)
    public void LoadGeneratedSprite(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0f)
            );

            characterImage.sprite = sprite;
        }
    }
}
```

---

## 품질 관리

### 체크리스트

#### LoRA 학습 품질
```
□ 학습 이미지
  □ 최소 15장 이상
  □ 다양한 각도 (정면, 측면, 3/4)
  □ 다양한 표정 (5가지 이상)
  □ 다양한 포즈
  □ 고해상도 (512x512+)
  □ 명확한 얼굴/디테일

□ 학습 설정
  □ Network Rank: 32-64
  □ Learning Rate: 1e-4
  □ Epochs: 10-20
  □ 캡션 파일 작성

□ 결과 확인
  □ 얼굴 특징 정확히 재현
  □ 의상 일관성
  □ 다양한 프롬프트 테스트
  □ 과적합 여부 확인
```

#### IP-Adapter 품질
```
□ 참조 이미지
  □ 고해상도
  □ 명확한 캐릭터
  □ 좋은 조명
  □ 클린한 배경

□ 설정
  □ Weight: 0.7-0.9
  □ ControlNet 조합 (포즈)
  □ 적절한 프롬프트

□ 결과 확인
  □ 캐릭터 유사성
  □ 원하는 포즈 재현
  □ 배경 자연스러움
```

### 배치 품질 검사

```python
# Scripts/AI/quality_check.py

import cv2
import numpy as np
from pathlib import Path

class CharacterQualityChecker:
    """생성된 캐릭터 이미지 품질 자동 검사"""

    def __init__(self, reference_image_path):
        self.reference = cv2.imread(reference_image_path)

    def check_similarity(self, generated_image_path, threshold=0.7):
        """참조 이미지와 유사도 확인"""
        generated = cv2.imread(generated_image_path)

        # 히스토그램 비교
        hist_ref = cv2.calcHist([self.reference], [0,1,2], None,
                                [8,8,8], [0,256,0,256,0,256])
        hist_gen = cv2.calcHist([generated], [0,1,2], None,
                                [8,8,8], [0,256,0,256,0,256])

        similarity = cv2.compareHist(hist_ref, hist_gen, cv2.HISTCMP_CORREL)

        return similarity >= threshold

    def check_quality(self, image_path):
        """이미지 품질 확인 (흐림, 노이즈 등)"""
        img = cv2.imread(image_path, cv2.IMREAD_GRAYSCALE)

        # Laplacian variance (선명도)
        variance = cv2.Laplacian(img, cv2.CV_64F).var()

        # 100 이상이면 선명, 이하면 흐림
        return variance >= 100

    def batch_check(self, folder_path):
        """폴더 내 모든 이미지 검사"""
        results = {
            'passed': [],
            'failed_similarity': [],
            'failed_quality': []
        }

        for img_path in Path(folder_path).glob('*.png'):
            similarity_ok = self.check_similarity(str(img_path))
            quality_ok = self.check_quality(str(img_path))

            if similarity_ok and quality_ok:
                results['passed'].append(str(img_path))
            elif not similarity_ok:
                results['failed_similarity'].append(str(img_path))
            elif not quality_ok:
                results['failed_quality'].append(str(img_path))

        return results

# 사용 예시
if __name__ == "__main__":
    checker = CharacterQualityChecker("reference/minsu_base.png")
    results = checker.batch_check("generated/minsu_poses/")

    print(f"통과: {len(results['passed'])}")
    print(f"유사도 실패: {len(results['failed_similarity'])}")
    print(f"품질 실패: {len(results['failed_quality'])}")
```

---

## 다음 단계

1. **자동화 스크립트 개발** → `Scripts/AI/character_generator.py`
2. **ComfyUI 워크플로우** → `ComfyUI_CharacterGeneration_Workflow.json`
3. **Unity 통합 시스템** → 전체 파이프라인
4. **배치 생성 도구** → 수백 장 자동 생성

**이 가이드는 `Dev_Docs/Character_Consistency_Guide.md`에 저장되었습니다.**
