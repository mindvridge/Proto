# 숨겨진 성장의 백수 영웅 - Backend Server

모바일 클리커 RPG 게임을 위한 백엔드 서버입니다.

## 기술 스택

- **Runtime**: Node.js 18+
- **Language**: TypeScript
- **Framework**: Express.js
- **Database**: PostgreSQL 15
- **Cache**: Redis 7
- **Container**: Docker

## 기능

- 인증 (이메일, 카카오, 구글, 게스트)
- 클라우드 세이브
- 랭킹 시스템
- 인앱 결제 검증
- 해킹 방지

## 빠른 시작

### 1. 환경 설정

```bash
cp .env.example .env
# .env 파일을 수정하여 설정값 입력
```

### 2. Docker로 실행 (권장)

```bash
# 프로덕션
docker-compose up -d

# 개발 (핫 리로드 + 관리 도구)
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### 3. 로컬 개발

```bash
# 의존성 설치
npm install

# PostgreSQL & Redis 실행 (Docker)
docker-compose up -d postgres redis

# 개발 서버 실행
npm run dev
```

### 4. 데이터베이스 마이그레이션

```bash
npm run db:migrate
```

## API 엔드포인트

### 인증 (Auth)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register/email` | 이메일 회원가입 |
| POST | `/api/v1/auth/login/email` | 이메일 로그인 |
| POST | `/api/v1/auth/login/social` | 소셜 로그인 |
| POST | `/api/v1/auth/login/guest` | 게스트 로그인 |
| POST | `/api/v1/auth/refresh` | 토큰 갱신 |
| POST | `/api/v1/auth/logout` | 로그아웃 |
| GET | `/api/v1/auth/me` | 내 정보 |

### 세이브 (Save)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/save/upload` | 저장 |
| GET | `/api/v1/save/download` | 불러오기 |
| GET | `/api/v1/save/backups` | 백업 목록 |
| POST | `/api/v1/save/restore/:id` | 복원 |

### 랭킹 (Ranking)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/ranking/:type` | 랭킹 조회 |
| GET | `/api/v1/ranking/:type/me` | 내 순위 |
| POST | `/api/v1/ranking/:type/submit` | 점수 제출 |

### 결제 (IAP)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/iap/validate` | 결제 검증 |
| GET | `/api/v1/iap/history` | 결제 내역 |

### 보안 (Security)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/security/integrity-check` | 무결성 검증 |
| POST | `/api/v1/security/validate-device` | 기기 검증 |

## 배포

### AWS EC2 배포

```bash
# 1. EC2 인스턴스에 Docker 설치
sudo yum update -y
sudo yum install docker -y
sudo systemctl start docker
sudo usermod -aG docker ec2-user

# 2. Docker Compose 설치
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 3. 코드 배포 & 실행
git clone <repo-url>
cd Backend
cp .env.example .env
# .env 수정
docker-compose up -d
```

### AWS RDS + ElastiCache 사용시

```bash
# docker-compose.yml에서 postgres, redis 서비스 제거 후
# .env에서 외부 DB 연결 정보 설정

DB_HOST=your-rds-endpoint.ap-northeast-2.rds.amazonaws.com
DB_PASSWORD=your_rds_password
REDIS_HOST=your-elasticache-endpoint.cache.amazonaws.com
```

### SSL 인증서 (Let's Encrypt)

```bash
# certbot 설치
sudo yum install certbot -y

# 인증서 발급
sudo certbot certonly --standalone -d api.yourgame.com

# nginx 설정에 SSL 추가
```

## 환경 변수

| 변수 | 설명 | 기본값 |
|------|------|--------|
| `NODE_ENV` | 실행 환경 | development |
| `PORT` | 서버 포트 | 3000 |
| `DB_HOST` | PostgreSQL 호스트 | localhost |
| `DB_PASSWORD` | PostgreSQL 비밀번호 | - |
| `REDIS_HOST` | Redis 호스트 | localhost |
| `JWT_SECRET` | JWT 시크릿 키 | - |
| `KAKAO_REST_API_KEY` | 카카오 REST API 키 | - |
| `GOOGLE_CLIENT_ID` | 구글 클라이언트 ID | - |

## 모니터링

### 로그 확인

```bash
# Docker 로그
docker logs hidden-growth-api -f

# 파일 로그
tail -f logs/combined.log
tail -f logs/error.log
```

### 상태 확인

```bash
# Health check
curl http://localhost:3000/health

# API ping
curl http://localhost:3000/api/v1/ping
```

## Unity 클라이언트 연동

`Assets/Scripts/Network/` 폴더의 매니저들이 이 서버와 통신합니다.

```csharp
// NetworkManager.cs의 baseUrl을 서버 주소로 변경
[SerializeField] private string baseUrl = "https://api.yourgame.com";
```

## 라이선스

Private - All rights reserved
