# 숨겨진 성장의 백수 영웅 - 배포 가이드

## 목차
1. [사전 준비](#1-사전-준비)
2. [OAuth 앱 등록](#2-oauth-앱-등록)
3. [AWS EC2 배포](#3-aws-ec2-배포)
4. [도메인 및 SSL 설정](#4-도메인-및-ssl-설정)
5. [Unity 클라이언트 연동](#5-unity-클라이언트-연동)

---

## 1. 사전 준비

### 필수 계정
- AWS 계정 (https://aws.amazon.com)
- 도메인 (가비아, 카페24, Cloudflare 등)
- 카카오 개발자 계정 (https://developers.kakao.com)
- 구글 클라우드 계정 (https://console.cloud.google.com)

### 로컬 테스트
```bash
cd Backend
cp .env.example .env
# .env 파일 수정

# Docker로 실행
docker-compose up -d

# 테스트
curl http://localhost:3000/health
curl http://localhost:3000/api/v1/ping
```

---

## 2. OAuth 앱 등록

### 카카오 로그인 설정

#### Step 1: 앱 생성
1. https://developers.kakao.com 접속
2. **내 애플리케이션** > **앱 추가하기**
3. 앱 이름: `숨겨진성장의백수영웅`

#### Step 2: 앱 키 확인
1. **앱 키** 메뉴에서 **REST API 키** 복사
2. `.env`의 `KAKAO_REST_API_KEY`에 입력

#### Step 3: 플랫폼 등록
1. **플랫폼** 메뉴
2. **Android** 추가:
   - 패키지명: `com.yourcompany.hiddengrowth`
   - 키 해시: 아래 명령으로 생성
   ```bash
   keytool -exportcert -alias androiddebugkey -keystore ~/.android/debug.keystore | openssl sha1 -binary | openssl base64
   ```
3. **iOS** 추가:
   - 번들 ID: `com.yourcompany.hiddengrowth`

#### Step 4: 카카오 로그인 활성화
1. **카카오 로그인** 메뉴
2. **활성화 설정** ON
3. **동의항목** 설정:
   - 닉네임: 필수
   - 프로필 사진: 선택
   - 카카오계정(이메일): 선택

---

### 구글 로그인 설정

#### Step 1: 프로젝트 생성
1. https://console.cloud.google.com 접속
2. **새 프로젝트** 생성: `HiddenGrowth`

#### Step 2: OAuth 동의 화면
1. **API 및 서비스** > **OAuth 동의 화면**
2. User Type: **외부**
3. 앱 정보 입력:
   - 앱 이름: `숨겨진 성장의 백수 영웅`
   - 사용자 지원 이메일: 본인 이메일
   - 개발자 연락처: 본인 이메일

#### Step 3: OAuth 클라이언트 ID 생성
1. **사용자 인증 정보** > **사용자 인증 정보 만들기** > **OAuth 클라이언트 ID**
2. **애플리케이션 유형**: 웹 애플리케이션
3. **승인된 리디렉션 URI**:
   ```
   https://api.yourgame.com/api/v1/auth/callback/google
   ```
4. 생성된 **클라이언트 ID**와 **클라이언트 보안 비밀번호** 복사
5. `.env`에 입력

#### Step 4: Android용 클라이언트 ID (Unity용)
1. **사용자 인증 정보 만들기** > **OAuth 클라이언트 ID**
2. **애플리케이션 유형**: Android
3. 패키지 이름: `com.yourcompany.hiddengrowth`
4. SHA-1 인증서 지문:
   ```bash
   keytool -list -v -keystore ~/.android/debug.keystore -alias androiddebugkey
   ```

---

## 3. AWS EC2 배포

### Step 1: EC2 인스턴스 생성

1. AWS 콘솔 > EC2 > **인스턴스 시작**
2. 설정:
   - AMI: **Amazon Linux 2023**
   - 인스턴스 유형: **t3.small** (운영) 또는 **t3.micro** (테스트)
   - 키 페어: 새로 생성 또는 기존 사용
   - 보안 그룹:
     - SSH (22): 내 IP
     - HTTP (80): 0.0.0.0/0
     - HTTPS (443): 0.0.0.0/0
     - Custom TCP (3000): 0.0.0.0/0 (개발용)
   - 스토리지: 20GB gp3

### Step 2: 서버 접속 및 환경 설정

```bash
# SSH 접속
ssh -i your-key.pem ec2-user@your-ec2-ip

# Docker 설치
sudo yum update -y
sudo yum install docker -y
sudo systemctl start docker
sudo systemctl enable docker
sudo usermod -aG docker ec2-user

# Docker Compose 설치
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 재접속 (docker 그룹 적용)
exit
ssh -i your-key.pem ec2-user@your-ec2-ip

# 확인
docker --version
docker-compose --version
```

### Step 3: 코드 배포

```bash
# Git 설치
sudo yum install git -y

# 코드 클론
git clone https://github.com/your-repo/Proto.git
cd Proto/Backend

# 환경 설정
cp .env.production .env
nano .env  # 또는 vim .env

# 실제 값으로 수정:
# - DB_PASSWORD
# - JWT_SECRET
# - JWT_REFRESH_SECRET
# - KAKAO_REST_API_KEY
# - GOOGLE_CLIENT_ID
# - 등등...
```

### Step 4: Docker 실행

```bash
# 백그라운드 실행
docker-compose up -d

# 로그 확인
docker-compose logs -f api

# 상태 확인
docker-compose ps
curl http://localhost:3000/health
```

### Step 5: 자동 시작 설정

```bash
# systemd 서비스 생성
sudo nano /etc/systemd/system/hidden-growth.service
```

```ini
[Unit]
Description=Hidden Growth Backend
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/home/ec2-user/Proto/Backend
ExecStart=/usr/local/bin/docker-compose up -d
ExecStop=/usr/local/bin/docker-compose down
User=ec2-user
Group=docker

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable hidden-growth
```

---

## 4. 도메인 및 SSL 설정

### Step 1: 도메인 구매 및 DNS 설정

1. 도메인 구매 (예: `yourgame.com`)
2. DNS 설정:
   ```
   A    api.yourgame.com    -> EC2 탄력적 IP
   ```

### Step 2: 탄력적 IP 할당

1. AWS EC2 > **탄력적 IP** > **탄력적 IP 주소 할당**
2. **작업** > **탄력적 IP 주소 연결** > EC2 인스턴스 선택

### Step 3: Nginx + Let's Encrypt SSL

```bash
# Nginx 설치
sudo yum install nginx -y
sudo systemctl start nginx
sudo systemctl enable nginx

# Certbot 설치
sudo yum install certbot python3-certbot-nginx -y

# SSL 인증서 발급
sudo certbot --nginx -d api.yourgame.com

# 자동 갱신 설정
sudo crontab -e
# 추가:
0 0 1 * * /usr/bin/certbot renew --quiet
```

### Step 4: Nginx 설정

```bash
sudo nano /etc/nginx/conf.d/api.conf
```

```nginx
server {
    listen 80;
    server_name api.yourgame.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.yourgame.com;

    ssl_certificate /etc/letsencrypt/live/api.yourgame.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourgame.com/privkey.pem;

    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256;
    ssl_prefer_server_ciphers off;

    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

```bash
# Nginx 재시작
sudo nginx -t
sudo systemctl reload nginx

# 테스트
curl https://api.yourgame.com/health
```

---

## 5. Unity 클라이언트 연동

### NetworkManager.cs 수정

`Assets/Scripts/Network/NetworkManager.cs` 파일에서:

```csharp
[Header("=== Server Settings ===")]
// 개발 환경
// [SerializeField] private string baseUrl = "http://localhost:3000";

// 운영 환경
[SerializeField] private string baseUrl = "https://api.yourgame.com";
```

### AuthManager.cs - 게스트 로그인 개선

`Assets/Scripts/Network/AuthManager.cs`의 게스트 로그인 호출 수정:

```csharp
public void LoginAsGuest(Action<bool, string> callback = null)
{
    if (isLoggingIn)
    {
        callback?.Invoke(false, "이미 로그인 중입니다.");
        return;
    }

    isLoggingIn = true;

    var guestData = new GuestLoginRequest
    {
        device_id = SystemInfo.deviceUniqueIdentifier,
        platform = Application.platform.ToString(),
        // 추가 기기 정보
        device_fingerprint = GenerateDeviceFingerprint(),
        device_model = SystemInfo.deviceModel,
        os_version = SystemInfo.operatingSystem,
        app_version = Application.version
    };

    NetworkManager.Instance?.Post("/auth/login/guest", guestData, (response) =>
    {
        isLoggingIn = false;

        if (response.success)
        {
            HandleLoginResponse(response, LoginType.Guest);

            // 신규 가입 여부 확인
            var loginResponse = response.GetData<GuestLoginResponse>();
            if (loginResponse.is_new_user)
            {
                Debug.Log("[AuthManager] New guest user registered!");
            }

            callback?.Invoke(true, null);
        }
        else
        {
            OnLoginFailed?.Invoke(response.errorMessage ?? "게스트 로그인 실패");
            callback?.Invoke(false, response.errorMessage);
        }
    }, false);
}

private string GenerateDeviceFingerprint()
{
    string raw = $"{SystemInfo.deviceUniqueIdentifier}_{SystemInfo.deviceModel}_{SystemInfo.operatingSystem}";
    using (var sha256 = System.Security.Cryptography.SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
        return System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}
```

### GuestLoginRequest 클래스 수정

```csharp
[Serializable]
public class GuestLoginRequest
{
    public string device_id;
    public string platform;
    public string device_fingerprint;
    public string device_model;
    public string os_version;
    public string app_version;
}

[Serializable]
public class GuestLoginResponse
{
    public string access_token;
    public string refresh_token;
    public int expires_in;
    public UserInfo user;
    public bool is_new_user;  // 신규 가입 여부
}
```

---

## 배포 체크리스트

- [ ] AWS EC2 인스턴스 생성
- [ ] Docker 및 Docker Compose 설치
- [ ] 코드 배포 및 .env 설정
- [ ] docker-compose up -d 실행
- [ ] 도메인 DNS 설정
- [ ] 탄력적 IP 연결
- [ ] Nginx 설치 및 설정
- [ ] SSL 인증서 발급
- [ ] https://api.yourgame.com/health 테스트
- [ ] Unity baseUrl 변경
- [ ] 게스트 로그인 테스트
- [ ] 카카오/구글 로그인 테스트

---

## 문제 해결

### Docker 로그 확인
```bash
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f redis
```

### 데이터베이스 접속
```bash
docker exec -it hidden-growth-db psql -U postgres -d hidden_growth
```

### Redis 접속
```bash
docker exec -it hidden-growth-redis redis-cli
```

### 서버 재시작
```bash
docker-compose down
docker-compose up -d
```
