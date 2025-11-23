# Railway 배포 가이드

## 목차
1. [사전 준비](#1-사전-준비)
2. [Railway 프로젝트 생성](#2-railway-프로젝트-생성)
3. [데이터베이스 추가](#3-데이터베이스-추가)
4. [환경변수 설정](#4-환경변수-설정)
5. [배포 확인](#5-배포-확인)
6. [Unity 연동](#6-unity-연동)

---

## 1. 사전 준비

### GitHub에 코드 푸시
```bash
# 이미 완료됨 - 현재 브랜치 확인
git branch
git push origin main  # 또는 해당 브랜치
```

### Railway 계정 생성
1. https://railway.app 접속
2. **GitHub로 로그인** (권장)
3. 이메일 인증

---

## 2. Railway 프로젝트 생성

### Step 1: 새 프로젝트
1. Railway 대시보드에서 **New Project** 클릭
2. **Deploy from GitHub repo** 선택
3. 저장소 선택: `mindvridge/Proto`
4. **Backend** 폴더 선택 (Root Directory 설정)

### Step 2: Root Directory 설정
```
Settings > General > Root Directory
값: Backend
```

### Step 3: 빌드 확인
- Railway가 자동으로 Node.js 감지
- `npm install` → `npm run build` → `npm start` 실행

---

## 3. 데이터베이스 추가

### PostgreSQL 추가
1. 프로젝트 대시보드에서 **+ New** 클릭
2. **Database** > **PostgreSQL** 선택
3. 자동 생성 완료 (약 30초)

### Redis 추가
1. **+ New** > **Database** > **Redis** 선택
2. 자동 생성 완료

### 연결 정보 확인
PostgreSQL 서비스 클릭 > **Variables** 탭:
```
PGHOST=xxx.railway.internal
PGPORT=5432
PGDATABASE=railway
PGUSER=postgres
PGPASSWORD=xxx
DATABASE_URL=postgresql://...
```

---

## 4. 환경변수 설정

### API 서비스 환경변수 설정
1. API 서비스 클릭 > **Variables** 탭
2. **Raw Editor** 클릭 후 아래 내용 붙여넣기:

```env
# Server
NODE_ENV=production
PORT=3000
API_VERSION=v1

# Database (Railway PostgreSQL 참조)
DB_HOST=${{Postgres.PGHOST}}
DB_PORT=${{Postgres.PGPORT}}
DB_NAME=${{Postgres.PGDATABASE}}
DB_USER=${{Postgres.PGUSER}}
DB_PASSWORD=${{Postgres.PGPASSWORD}}

# Redis (Railway Redis 참조)
REDIS_HOST=${{Redis.REDISHOST}}
REDIS_PORT=${{Redis.REDISPORT}}
REDIS_PASSWORD=${{Redis.REDISPASSWORD}}

# JWT (반드시 변경!)
JWT_SECRET=여기에_32자_이상_랜덤_문자열
JWT_EXPIRES_IN=7d
JWT_REFRESH_SECRET=여기에_다른_32자_이상_랜덤_문자열
JWT_REFRESH_EXPIRES_IN=30d

# Security
ENCRYPTION_KEY=your_32_character_encryption_key!
HMAC_SECRET=your_hmac_secret_key

# Rate Limiting
RATE_LIMIT_WINDOW_MS=900000
RATE_LIMIT_MAX_REQUESTS=100

# Logging
LOG_LEVEL=info
```

### JWT 시크릿 생성 (터미널)
```bash
# 랜덤 문자열 생성
openssl rand -base64 32
# 예: K7xP9mN2qR5tY8wE3fG6hJ0kL4oI1uA2
```

---

## 5. 배포 확인

### 도메인 확인
1. API 서비스 > **Settings** > **Networking**
2. **Generate Domain** 클릭
3. 생성된 URL: `https://xxx-production.up.railway.app`

### 테스트
```bash
# Health Check
curl https://your-app.up.railway.app/health

# 응답 예시
{"status":"ok","timestamp":"2024-...","version":"v1"}

# Ping
curl https://your-app.up.railway.app/api/v1/ping

# 게스트 로그인 테스트
curl -X POST https://your-app.up.railway.app/api/v1/auth/login/guest \
  -H "Content-Type: application/json" \
  -d '{"device_id":"test-device-123","platform":"Android"}'
```

### 로그 확인
- Railway 대시보드 > API 서비스 > **Deployments** > **View Logs**

---

## 6. Unity 연동

### NetworkManager.cs 수정
```csharp
[Header("=== Server Settings ===")]
// Railway 배포 URL로 변경
[SerializeField] private string baseUrl = "https://your-app.up.railway.app";
```

### 테스트 순서
1. Unity 에디터에서 게스트 로그인 테스트
2. 로그인 성공 시 토큰 저장 확인
3. 데이터 저장/불러오기 테스트

---

## 비용 관리

### 현재 사용량 확인
- Railway 대시보드 > **Usage**

### 비용 절감
```bash
# 사용 안 할 때 서비스 일시정지
# Settings > Danger Zone > Pause Service

# 또는 스케일 다운
# 자동으로 트래픽 없으면 슬립 모드
```

### 예상 비용
| 구성 | 예상 비용 |
|------|----------|
| API + PostgreSQL + Redis | $30-50/월 |
| API만 (외부 DB 사용) | $10-15/월 |

---

## 문제 해결

### 빌드 실패
```bash
# 로컬에서 먼저 테스트
cd Backend
npm install
npm run build
npm start
```

### DB 연결 실패
- 환경변수 `${{Postgres.PGHOST}}` 형식 확인
- PostgreSQL 서비스가 Running 상태인지 확인

### 타임아웃
- Railway 서버는 미국/유럽에 있어 한국에서 100-200ms 레이턴시
- 게임에 치명적이면 한국 리전 서비스 고려

---

## 빠른 명령어 정리

```bash
# Railway CLI 설치 (선택)
npm install -g @railway/cli

# 로그인
railway login

# 프로젝트 연결
railway link

# 로컬에서 Railway 환경으로 실행
railway run npm start

# 배포
railway up
```

---

## 다음 단계

1. ✅ Railway 배포 완료
2. 🔲 커스텀 도메인 연결 (선택)
3. 🔲 카카오/구글 OAuth 설정
4. 🔲 Unity에서 전체 기능 테스트
