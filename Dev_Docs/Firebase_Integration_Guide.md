# Firebase 통합 가이드 (Firebase Integration Guide)

## 📋 목차

1. [개요](#1-개요)
2. [Firebase 프로젝트 설정](#2-firebase-프로젝트-설정)
3. [Unity SDK 설치](#3-unity-sdk-설치)
4. [Authentication 구현](#4-authentication-구현)
5. [Cloud Firestore 구현](#5-cloud-firestore-구현)
6. [Cloud Functions 구현](#6-cloud-functions-구현)
7. [Remote Config 구현](#7-remote-config-구현)
8. [Analytics 구현](#8-analytics-구현)
9. [Crashlytics 구현](#9-crashlytics-구현)
10. [Cloud Storage 구현](#10-cloud-storage-구현)
11. [보안 설정](#11-보안-설정)
12. [테스트 및 디버깅](#12-테스트-및-디버깅)
13. [배포 가이드](#13-배포-가이드)
14. [비용 최적화](#14-비용-최적화)
15. [문제 해결](#15-문제-해결)

---

## 1. 개요

### 1.1 Firebase란?

Google의 모바일/웹 앱 개발 플랫폼으로, 백엔드 서비스를 쉽게 통합할 수 있습니다.

### 1.2 현재 프로젝트에서 사용할 기능

```
✅ Authentication (익명 로그인)
✅ Cloud Firestore (클라우드 세이브)
✅ Cloud Functions (영수증 검증, 서버 시간)
✅ Remote Config (라이브 밸런싱)
✅ Analytics (KPI 추적)
✅ Crashlytics (버그 리포트)
```

### 1.3 요구사항

```
Unity: 2020.3 LTS 이상
.NET: .NET 4.x 또는 .NET Standard 2.0
Android: API Level 19 (Android 4.4) 이상
iOS: iOS 11 이상
```

---

## 2. Firebase 프로젝트 설정

### 2.1 Firebase Console 접속

1. https://console.firebase.google.com/ 접속
2. Google 계정으로 로그인
3. "프로젝트 추가" 클릭

### 2.2 프로젝트 생성

```
프로젝트 이름: HiddenGrowthNEETHero
(또는 원하는 이름)

Google Analytics: 사용 설정 (권장)

지역: 대한민국
```

### 2.3 Android 앱 추가

**Step 1: 앱 등록**
```
Android 패키지 이름: com.yourcompany.neet-hero
(Unity Player Settings와 동일해야 함!)

앱 닉네임: NEET Hero Android

디버그 서명 인증서 SHA-1: (선택사항, 나중에 추가 가능)
```

**Step 2: google-services.json 다운로드**
```
다운로드한 파일을 Unity 프로젝트의
Assets/Plugins/Android/ 경로에 복사
```

### 2.4 iOS 앱 추가

**Step 1: 앱 등록**
```
iOS 번들 ID: com.yourcompany.neet-hero
(Unity Player Settings → iOS → Bundle Identifier와 동일)

앱 닉네임: NEET Hero iOS

App Store ID: (나중에 추가 가능)
```

**Step 2: GoogleService-Info.plist 다운로드**
```
다운로드한 파일을 Unity 프로젝트의
Assets/ 루트 폴더에 복사
```

### 2.5 Firebase 요금제 설정

```
1. Firebase Console → 왼쪽 하단 "업그레이드"
2. "Blaze 요금제 선택"
3. 결제 정보 입력
4. 예산 알림 설정 (권장: $50/월)
```

**주의**: 무료 플랜(Spark)으로 시작 가능하지만, Cloud Functions 사용을 위해 Blaze 필요

---

## 3. Unity SDK 설치

### 3.1 SDK 다운로드

**방법 1: Firebase Unity SDK 직접 다운로드 (권장)**

```
1. https://firebase.google.com/download/unity 접속
2. Firebase Unity SDK 다운로드 (최신 버전)
3. 압축 해제
```

**필요한 패키지:**
```
FirebaseAuth.unitypackage
FirebaseFirestore.unitypackage
FirebaseFunctions.unitypackage
FirebaseRemoteConfig.unitypackage
FirebaseAnalytics.unitypackage
FirebaseCrashlytics.unitypackage
FirebaseStorage.unitypackage (선택)
```

### 3.2 Unity에 패키지 Import

```
1. Unity 에디터 열기
2. Assets → Import Package → Custom Package
3. 위 패키지들을 하나씩 Import
4. "Import" 버튼 클릭 (모든 파일 선택)
```

**Import 순서 (중요!):**
```
1. FirebaseAuth.unitypackage (먼저)
2. FirebaseFirestore.unitypackage
3. FirebaseFunctions.unitypackage
4. FirebaseRemoteConfig.unitypackage
5. FirebaseAnalytics.unitypackage
6. FirebaseCrashlytics.unitypackage
```

### 3.3 의존성 해결

**External Dependency Manager 실행:**
```
Unity 메뉴: Assets → External Dependency Manager → Android Resolver → Resolve
Unity 메뉴: Assets → External Dependency Manager → iOS Resolver → Install Cocoapods
```

**자동으로 다운로드되는 라이브러리:**
- Google Play Services
- Firebase Android SDK
- Firebase iOS SDK

### 3.4 Unity 프로젝트 설정

**Player Settings 설정:**

```csharp
// Build Settings → Player Settings

// Android
Bundle Identifier: com.yourcompany.neet-hero
Minimum API Level: API Level 19 (Android 4.4)
Target API Level: API Level 33 (Android 13) 이상
Scripting Backend: IL2CPP (권장)
Target Architectures: ARM64 (필수), ARMv7 (선택)

// iOS
Bundle Identifier: com.yourcompany.neet-hero
Minimum iOS Version: 11.0
Target SDK: Device SDK
Architecture: ARM64

// Other Settings
Scripting Runtime Version: .NET 4.x Equivalent
API Compatibility Level: .NET Standard 2.0
```

---

## 4. Authentication 구현

### 4.1 FirebaseManager 싱글톤 생성

```csharp
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    private FirebaseApp app;
    private FirebaseAuth auth;

    public bool IsInitialized { get; private set; }
    public FirebaseUser CurrentUser => auth?.CurrentUser;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                IsInitialized = true;
                Debug.Log("Firebase 초기화 성공!");

                // 자동 로그인 시도
                AutoSignIn();
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {dependencyStatus}");
                IsInitialized = false;
            }
        });
    }

    // 익명 로그인 (자동)
    private void AutoSignIn()
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log($"이미 로그인됨: {auth.CurrentUser.UserId}");
            OnSignInSuccess();
        }
        else
        {
            SignInAnonymously();
        }
    }

    // 익명 로그인
    public void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("익명 로그인 취소됨");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError($"익명 로그인 실패: {task.Exception}");
                return;
            }

            FirebaseUser user = task.Result;
            Debug.Log($"익명 로그인 성공: {user.UserId}");
            OnSignInSuccess();
        });
    }

    // 소셜 로그인 (선택사항)
    public void SignInWithGoogle()
    {
        // TODO: Google Sign-In SDK 통합 필요
        Debug.Log("Google 로그인은 Google Sign-In SDK가 필요합니다.");
    }

    // 로그인 성공 처리
    private void OnSignInSuccess()
    {
        string userId = auth.CurrentUser.UserId;
        PlayerPrefs.SetString("FirebaseUserId", userId);
        PlayerPrefs.Save();

        // 클라우드 세이브 로드
        CloudSaveManager.Instance?.LoadFromCloud();

        // Analytics 유저 ID 설정
        Firebase.Analytics.FirebaseAnalytics.SetUserId(userId);
    }

    // 로그아웃
    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("로그아웃 완료");
        }
    }

    // 계정 연동 (익명 → 소셜)
    public async Task<bool> LinkAnonymousAccountWithGoogle(Credential credential)
    {
        try
        {
            var result = await auth.CurrentUser.LinkWithCredentialAsync(credential);
            Debug.Log($"계정 연동 성공: {result.User.UserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"계정 연동 실패: {e.Message}");
            return false;
        }
    }
}
```

### 4.2 Authentication 테스트

```csharp
// 테스트 코드
public class AuthTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TestAuth());
    }

    IEnumerator TestAuth()
    {
        // Firebase 초기화 대기
        while (!FirebaseManager.Instance.IsInitialized)
        {
            yield return null;
        }

        Debug.Log("Firebase 준비 완료!");

        // 로그인 상태 확인
        if (FirebaseManager.Instance.CurrentUser != null)
        {
            Debug.Log($"현재 사용자: {FirebaseManager.Instance.CurrentUser.UserId}");
        }
        else
        {
            Debug.Log("로그인 필요");
            FirebaseManager.Instance.SignInAnonymously();
        }
    }
}
```

---

## 5. Cloud Firestore 구현

### 5.1 Firestore 규칙 설정

Firebase Console → Firestore Database → 규칙

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    // 유저 데이터는 본인만 읽기/쓰기 가능
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }

    // 게임 설정은 모두 읽기 가능, 쓰기 불가
    match /config/{document=**} {
      allow read: if true;
      allow write: if false;
    }

    // 리더보드는 모두 읽기 가능, 인증된 유저만 쓰기
    match /leaderboard/{document=**} {
      allow read: if true;
      allow write: if request.auth != null;
    }
  }
}
```

### 5.2 CloudSaveManager 구현

```csharp
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }

    private FirebaseFirestore firestore;
    private string userId => FirebaseManager.Instance.CurrentUser?.UserId;

    // 캐시
    private SaveData cachedSaveData;
    private DateTime lastSaveTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;

        // 오프라인 캐싱 활성화 (비용 절감!)
        firestore.Settings.PersistenceEnabled = true;
    }

    // 클라우드에 저장
    public async Task<bool> SaveToCloud()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("로그인이 필요합니다!");
            return false;
        }

        // 너무 자주 저장하지 않기 (최소 30초 간격)
        if ((DateTime.Now - lastSaveTime).TotalSeconds < 30)
        {
            Debug.Log("저장 쿨다운 중...");
            return false;
        }

        try
        {
            // 세이브 데이터 가져오기
            SaveData data = GameManager.Instance.GetSaveData();

            // 압축 (비용 절감!)
            string json = JsonUtility.ToJson(data);
            byte[] compressed = GZipHelper.Compress(json);
            string base64 = Convert.ToBase64String(compressed);

            // Firestore에 저장
            Dictionary<string, object> saveDoc = new Dictionary<string, object>
            {
                { "data", base64 },
                { "lastModified", FieldValue.ServerTimestamp },
                { "version", Application.version },
                { "platform", Application.platform.ToString() }
            };

            await firestore.Collection("users").Document(userId).SetAsync(saveDoc);

            lastSaveTime = DateTime.Now;
            cachedSaveData = data;

            Debug.Log("클라우드 저장 성공!");

            // Analytics 이벤트
            Firebase.Analytics.FirebaseAnalytics.LogEvent("cloud_save");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 저장 실패: {e.Message}");
            return false;
        }
    }

    // 클라우드에서 로드
    public async Task<SaveData> LoadFromCloud()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("로그인이 필요합니다!");
            return null;
        }

        try
        {
            DocumentSnapshot snapshot = await firestore
                .Collection("users")
                .Document(userId)
                .GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Debug.Log("클라우드 세이브 없음 (신규 유저)");
                return null;
            }

            Dictionary<string, object> data = snapshot.ToDictionary();
            string base64 = data["data"].ToString();

            // 압축 해제
            byte[] compressed = Convert.FromBase64String(base64);
            string json = GZipHelper.Decompress(compressed);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            cachedSaveData = saveData;

            Debug.Log("클라우드 로드 성공!");

            // Analytics 이벤트
            Firebase.Analytics.FirebaseAnalytics.LogEvent("cloud_load");

            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"클라우드 로드 실패: {e.Message}");
            return null;
        }
    }

    // 자동 저장 (5분마다)
    private float autoSaveInterval = 300f; // 5분
    private float autoSaveTimer = 0f;

    void Update()
    {
        autoSaveTimer += Time.deltaTime;

        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            SaveToCloud();
        }
    }

    // 게임 종료 시 저장
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveToCloud();
        }
    }

    void OnApplicationQuit()
    {
        // 동기 저장 (권장하지 않지만 필요시)
        SaveToCloud().Wait();
    }
}
```

### 5.3 GZip 압축 헬퍼

```csharp
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class GZipHelper
{
    public static byte[] Compress(string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        using (MemoryStream ms = new MemoryStream())
        {
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }
            return ms.ToArray();
        }
    }

    public static string Decompress(byte[] compressed)
    {
        using (MemoryStream ms = new MemoryStream(compressed))
        {
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(zip, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
```

### 5.4 SaveData 구조

```csharp
[System.Serializable]
public class SaveData
{
    public int playerLevel;
    public long gold;
    public long exp;
    public int rebirthCount;
    public float suspicion;

    public List<InventoryItem> inventory;
    public List<string> learnedSkills;
    public List<string> unlockedAchievements;
    public List<string> clearedDungeons;

    public DateTime lastPlayTime;
    public int totalPlayTimeSeconds;

    // 기본값
    public SaveData()
    {
        playerLevel = 1;
        gold = 0;
        exp = 0;
        rebirthCount = 0;
        suspicion = 0;

        inventory = new List<InventoryItem>();
        learnedSkills = new List<string>();
        unlockedAchievements = new List<string>();
        clearedDungeons = new List<string>();

        lastPlayTime = DateTime.Now;
        totalPlayTimeSeconds = 0;
    }
}

[System.Serializable]
public class InventoryItem
{
    public string itemId;
    public int count;
    public int enhanceLevel;
}
```

---

## 6. Cloud Functions 구현

### 6.1 Node.js 환경 설정

**Firebase CLI 설치:**
```bash
npm install -g firebase-tools
```

**Firebase 로그인:**
```bash
firebase login
```

**Functions 초기화:**
```bash
cd /your/project/path
firebase init functions

# 선택 사항:
# - Use an existing project: (프로젝트 선택)
# - Language: JavaScript (또는 TypeScript)
# - ESLint: Yes (권장)
# - Install dependencies: Yes
```

### 6.2 영수증 검증 Function

`functions/index.js`:

```javascript
const functions = require('firebase-functions');
const admin = require('firebase-admin');
admin.initializeApp();

// Android 영수증 검증
exports.verifyAndroidReceipt = functions.https.onCall(async (data, context) => {
  // 인증 확인
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', '로그인이 필요합니다.');
  }

  const { packageName, productId, purchaseToken } = data;

  if (!packageName || !productId || !purchaseToken) {
    throw new functions.https.HttpsError('invalid-argument', '필수 파라미터가 누락되었습니다.');
  }

  try {
    // Google Play Developer API 호출
    const { google } = require('googleapis');
    const androidpublisher = google.androidpublisher('v3');

    // Service Account 인증 (Firebase Console에서 설정 필요)
    const auth = new google.auth.GoogleAuth({
      keyFile: 'service-account-key.json',
      scopes: ['https://www.googleapis.com/auth/androidpublisher'],
    });

    const authClient = await auth.getClient();
    google.options({ auth: authClient });

    // 영수증 검증
    const response = await androidpublisher.purchases.products.get({
      packageName: packageName,
      productId: productId,
      token: purchaseToken,
    });

    const purchase = response.data;

    // 검증 확인
    if (purchase.purchaseState === 0) { // 0 = Purchased
      // Firestore에 구매 기록 저장
      await admin.firestore().collection('purchases').add({
        userId: context.auth.uid,
        productId: productId,
        purchaseToken: purchaseToken,
        purchaseTime: admin.firestore.FieldValue.serverTimestamp(),
        verified: true,
      });

      return {
        success: true,
        message: '결제가 확인되었습니다.',
        orderId: purchase.orderId,
      };
    } else {
      throw new functions.https.HttpsError('failed-precondition', '결제가 완료되지 않았습니다.');
    }
  } catch (error) {
    console.error('영수증 검증 실패:', error);
    throw new functions.https.HttpsError('internal', '영수증 검증 중 오류가 발생했습니다.');
  }
});

// iOS 영수증 검증
exports.verifyIOSReceipt = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', '로그인이 필요합니다.');
  }

  const { receiptData, sandbox } = data;

  if (!receiptData) {
    throw new functions.https.HttpsError('invalid-argument', '영수증 데이터가 필요합니다.');
  }

  try {
    const axios = require('axios');

    // Apple 서버에 검증 요청
    const url = sandbox
      ? 'https://sandbox.itunes.apple.com/verifyReceipt'
      : 'https://buy.itunes.apple.com/verifyReceipt';

    const response = await axios.post(url, {
      'receipt-data': receiptData,
      'password': 'YOUR_SHARED_SECRET', // App Store Connect에서 발급
    });

    const result = response.data;

    if (result.status === 0) {
      // 검증 성공
      await admin.firestore().collection('purchases').add({
        userId: context.auth.uid,
        receiptData: receiptData,
        purchaseTime: admin.firestore.FieldValue.serverTimestamp(),
        verified: true,
        platform: 'iOS',
      });

      return {
        success: true,
        message: '결제가 확인되었습니다.',
      };
    } else {
      throw new functions.https.HttpsError('failed-precondition', `검증 실패: ${result.status}`);
    }
  } catch (error) {
    console.error('iOS 영수증 검증 실패:', error);
    throw new functions.https.HttpsError('internal', '영수증 검증 중 오류가 발생했습니다.');
  }
});

// 서버 시간 가져오기
exports.getServerTime = functions.https.onCall(async (data, context) => {
  return {
    serverTime: admin.firestore.Timestamp.now().toMillis(),
    timezone: 'Asia/Seoul',
  };
});

// 일일 보상 지급 (치팅 방지)
exports.claimDailyReward = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', '로그인이 필요합니다.');
  }

  const userId = context.auth.uid;
  const userRef = admin.firestore().collection('users').doc(userId);

  try {
    // Transaction으로 중복 지급 방지
    const result = await admin.firestore().runTransaction(async (transaction) => {
      const doc = await transaction.get(userRef);

      if (!doc.exists) {
        throw new functions.https.HttpsError('not-found', '유저 데이터가 없습니다.');
      }

      const data = doc.data();
      const lastClaimDate = data.lastDailyRewardClaim?.toDate() || new Date(0);
      const now = new Date();

      // 같은 날짜인지 확인
      if (lastClaimDate.toDateString() === now.toDateString()) {
        throw new functions.https.HttpsError('already-exists', '오늘 이미 보상을 받았습니다.');
      }

      // 연속 일수 계산
      const yesterday = new Date(now);
      yesterday.setDate(yesterday.getDate() - 1);

      let consecutiveDays = data.consecutiveDays || 0;
      if (lastClaimDate.toDateString() === yesterday.toDateString()) {
        consecutiveDays++;
      } else {
        consecutiveDays = 1;
      }

      // 보상 계산
      const baseReward = 1000;
      const bonusReward = consecutiveDays * 100;
      const totalReward = baseReward + bonusReward;

      // 데이터 업데이트
      transaction.update(userRef, {
        gold: admin.firestore.FieldValue.increment(totalReward),
        lastDailyRewardClaim: admin.firestore.FieldValue.serverTimestamp(),
        consecutiveDays: consecutiveDays,
      });

      return {
        success: true,
        reward: totalReward,
        consecutiveDays: consecutiveDays,
      };
    });

    return result;
  } catch (error) {
    console.error('일일 보상 지급 실패:', error);
    throw error;
  }
});
```

### 6.3 Functions 배포

```bash
firebase deploy --only functions
```

### 6.4 Unity에서 Functions 호출

```csharp
using Firebase.Functions;
using Firebase.Extensions;

public class CloudFunctionsManager : MonoBehaviour
{
    private FirebaseFunctions functions;

    void Start()
    {
        functions = FirebaseFunctions.DefaultInstance;

        // 한국 리전 사용 (선택사항, 더 빠름)
        // functions = FirebaseFunctions.GetInstance("asia-northeast3");
    }

    // Android 영수증 검증
    public async Task<bool> VerifyAndroidPurchase(string productId, string purchaseToken)
    {
        try
        {
            var callable = functions.GetHttpsCallable("verifyAndroidReceipt");
            var data = new Dictionary<string, object>
            {
                { "packageName", Application.identifier },
                { "productId", productId },
                { "purchaseToken", purchaseToken }
            };

            var result = await callable.CallAsync(data);
            var response = (Dictionary<string, object>)result.Data;

            bool success = (bool)response["success"];
            if (success)
            {
                Debug.Log("결제 검증 성공!");
                return true;
            }
            else
            {
                Debug.LogError("결제 검증 실패!");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"결제 검증 오류: {e.Message}");
            return false;
        }
    }

    // 서버 시간 가져오기
    public async Task<DateTime> GetServerTime()
    {
        try
        {
            var callable = functions.GetHttpsCallable("getServerTime");
            var result = await callable.CallAsync();
            var response = (Dictionary<string, object>)result.Data;

            long timestamp = Convert.ToInt64(response["serverTime"]);
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 시간 가져오기 실패: {e.Message}");
            return DateTime.Now; // 폴백
        }
    }

    // 일일 보상 수령
    public async Task<DailyRewardResult> ClaimDailyReward()
    {
        try
        {
            var callable = functions.GetHttpsCallable("claimDailyReward");
            var result = await callable.CallAsync();
            var response = (Dictionary<string, object>)result.Data;

            return new DailyRewardResult
            {
                success = (bool)response["success"],
                reward = Convert.ToInt32(response["reward"]),
                consecutiveDays = Convert.ToInt32(response["consecutiveDays"])
            };
        }
        catch (FunctionsException e)
        {
            Debug.LogError($"일일 보상 실패: {e.Message}");

            if (e.Code == FunctionsErrorCode.AlreadyExists)
            {
                UIManager.Instance.ShowNotification("오늘 이미 보상을 받았습니다!");
            }

            return new DailyRewardResult { success = false };
        }
    }
}

[System.Serializable]
public class DailyRewardResult
{
    public bool success;
    public int reward;
    public int consecutiveDays;
}
```

---

## 7. Remote Config 구현

### 7.1 Firebase Console에서 설정

```
Firebase Console → Remote Config → 매개변수 추가

매개변수 예시:
- gold_multiplier: 1.0 (기본값)
- exp_multiplier: 1.0
- daily_reward_base: 1000
- shop_discount_rate: 0.0
- maintenance_mode: false
- minimum_version: "1.0.0"
- event_banner_url: ""
```

### 7.2 RemoteConfigManager 구현

```csharp
using UnityEngine;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RemoteConfigManager : MonoBehaviour
{
    public static RemoteConfigManager Instance { get; private set; }

    private bool isFetched = false;

    // 기본값
    private Dictionary<string, object> defaults = new Dictionary<string, object>
    {
        { "gold_multiplier", 1.0 },
        { "exp_multiplier", 1.0 },
        { "daily_reward_base", 1000 },
        { "shop_discount_rate", 0.0 },
        { "maintenance_mode", false },
        { "minimum_version", "1.0.0" },
        { "event_banner_url", "" },
        { "max_daily_dungeon_entries", 5 },
        { "suspicion_increase_rate", 1.0 },
        { "night_bonus_multiplier", 1.3 }
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeRemoteConfig();
    }

    private async void InitializeRemoteConfig()
    {
        // 기본값 설정
        await FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

        // Remote Config Fetch
        await FetchRemoteConfig();
    }

    // Remote Config 가져오기
    public async Task FetchRemoteConfig()
    {
        try
        {
            // 캐시 만료 시간 (개발: 0초, 프로덕션: 3600초)
            TimeSpan cacheExpiration = TimeSpan.FromSeconds(0);
            #if !UNITY_EDITOR
            cacheExpiration = TimeSpan.FromSeconds(3600); // 1시간
            #endif

            await FirebaseRemoteConfig.DefaultInstance.FetchAsync(cacheExpiration);
            await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            isFetched = true;
            Debug.Log("Remote Config 가져오기 성공!");

            // 업데이트된 값 적용
            ApplyRemoteConfig();
        }
        catch (Exception e)
        {
            Debug.LogError($"Remote Config 가져오기 실패: {e.Message}");
        }
    }

    // Remote Config 값 적용
    private void ApplyRemoteConfig()
    {
        // 유지보수 모드 체크
        bool maintenanceMode = GetBool("maintenance_mode");
        if (maintenanceMode)
        {
            ShowMaintenanceScreen();
            return;
        }

        // 최소 버전 체크
        string minVersion = GetString("minimum_version");
        if (IsVersionOlder(Application.version, minVersion))
        {
            ShowUpdateRequiredScreen();
            return;
        }

        // 게임 밸런싱 값 적용
        GameBalance.goldMultiplier = (float)GetDouble("gold_multiplier");
        GameBalance.expMultiplier = (float)GetDouble("exp_multiplier");
        GameBalance.dailyRewardBase = (int)GetLong("daily_reward_base");
        GameBalance.shopDiscountRate = (float)GetDouble("shop_discount_rate");
        GameBalance.suspicionIncreaseRate = (float)GetDouble("suspicion_increase_rate");
        GameBalance.nightBonusMultiplier = (float)GetDouble("night_bonus_multiplier");

        Debug.Log($"밸런싱 적용: Gold {GameBalance.goldMultiplier}x, Exp {GameBalance.expMultiplier}x");

        // 이벤트 배너 표시
        string bannerUrl = GetString("event_banner_url");
        if (!string.IsNullOrEmpty(bannerUrl))
        {
            EventManager.Instance?.ShowEventBanner(bannerUrl);
        }
    }

    // Getter 메서드들
    public string GetString(string key)
    {
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
    }

    public long GetLong(string key)
    {
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
    }

    public double GetDouble(string key)
    {
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
    }

    public bool GetBool(string key)
    {
        return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
    }

    // 버전 비교
    private bool IsVersionOlder(string currentVersion, string minimumVersion)
    {
        try
        {
            Version current = new Version(currentVersion);
            Version minimum = new Version(minimumVersion);
            return current < minimum;
        }
        catch
        {
            return false;
        }
    }

    // 유지보수 화면
    private void ShowMaintenanceScreen()
    {
        UIManager.Instance.ShowPopup("점검 중", "현재 서버 점검 중입니다. 잠시 후 다시 시도해주세요.");
        // 게임 진행 차단
    }

    // 업데이트 필수 화면
    private void ShowUpdateRequiredScreen()
    {
        UIManager.Instance.ShowPopup("업데이트 필요", "최신 버전으로 업데이트해주세요.", () =>
        {
            #if UNITY_ANDROID
            Application.OpenURL("market://details?id=" + Application.identifier);
            #elif UNITY_IOS
            Application.OpenURL("itms-apps://itunes.apple.com/app/YOUR_APP_ID");
            #endif
        });
    }
}

// 게임 밸런싱 전역 변수
public static class GameBalance
{
    public static float goldMultiplier = 1.0f;
    public static float expMultiplier = 1.0f;
    public static int dailyRewardBase = 1000;
    public static float shopDiscountRate = 0.0f;
    public static float suspicionIncreaseRate = 1.0f;
    public static float nightBonusMultiplier = 1.3f;
}
```

### 7.3 Remote Config 활용 예시

```csharp
// 골드 획득 시
public void AddGold(int amount)
{
    int finalAmount = (int)(amount * GameBalance.goldMultiplier);
    playerGold += finalAmount;

    Debug.Log($"골드 획득: {amount} → {finalAmount} (x{GameBalance.goldMultiplier})");
}

// 긴급 이벤트 적용 (앱 업데이트 없이!)
// Firebase Console에서 gold_multiplier를 2.0으로 변경
// → 모든 유저가 다음 앱 실행 시 자동으로 2배 골드 이벤트 적용!
```

---

## 8. Analytics 구현

### 8.1 AnalyticsManager 구현

```csharp
using UnityEngine;
using Firebase.Analytics;
using System.Collections.Generic;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Analytics 초기화는 Firebase 초기화 시 자동으로 됨
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        // 유저 속성 설정
        SetUserProperty("game_version", Application.version);
        SetUserProperty("platform", Application.platform.ToString());
    }

    // 기본 이벤트 로깅
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters == null)
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
        else
        {
            // Dictionary를 Parameter 배열로 변환
            List<Parameter> paramList = new List<Parameter>();
            foreach (var kvp in parameters)
            {
                if (kvp.Value is string)
                    paramList.Add(new Parameter(kvp.Key, (string)kvp.Value));
                else if (kvp.Value is int)
                    paramList.Add(new Parameter(kvp.Key, (int)kvp.Value));
                else if (kvp.Value is long)
                    paramList.Add(new Parameter(kvp.Key, (long)kvp.Value));
                else if (kvp.Value is double)
                    paramList.Add(new Parameter(kvp.Key, (double)kvp.Value));
            }
            FirebaseAnalytics.LogEvent(eventName, paramList.ToArray());
        }
    }

    // 유저 속성 설정
    public void SetUserProperty(string name, string value)
    {
        FirebaseAnalytics.SetUserProperty(name, value);
    }

    // === 게임 특화 이벤트들 ===

    // 레벨업
    public void LogLevelUp(int level)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelUp, new Parameter[] {
            new Parameter(FirebaseAnalytics.ParameterLevel, level)
        });
    }

    // 던전 클리어
    public void LogDungeonClear(string dungeonId, int clearTime, bool firstClear)
    {
        LogEvent("dungeon_clear", new Dictionary<string, object> {
            { "dungeon_id", dungeonId },
            { "clear_time", clearTime },
            { "first_clear", firstClear ? 1 : 0 }
        });
    }

    // 아이템 획득
    public void LogItemObtain(string itemId, int quantity, string source)
    {
        LogEvent("item_obtain", new Dictionary<string, object> {
            { "item_id", itemId },
            { "quantity", quantity },
            { "source", source } // "shop", "dungeon", "quest" 등
        });
    }

    // 스킬 학습
    public void LogSkillLearn(string skillId, int skillLevel)
    {
        LogEvent("skill_learn", new Dictionary<string, object> {
            { "skill_id", skillId },
            { "skill_level", skillLevel }
        });
    }

    // 환생
    public void LogRebirth(int rebirthCount, int finalLevel)
    {
        LogEvent("rebirth", new Dictionary<string, object> {
            { "rebirth_count", rebirthCount },
            { "final_level", finalLevel }
        });
    }

    // 업적 달성
    public void LogAchievement(string achievementId)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventUnlockAchievement, new Parameter[] {
            new Parameter(FirebaseAnalytics.ParameterAchievementId, achievementId)
        });
    }

    // 튜토리얼 완료
    public void LogTutorialComplete()
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);
    }

    // 인앱 구매 (중요!)
    public void LogPurchase(string productId, double price, string currency = "KRW")
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter[] {
            new Parameter(FirebaseAnalytics.ParameterItemId, productId),
            new Parameter(FirebaseAnalytics.ParameterValue, price),
            new Parameter(FirebaseAnalytics.ParameterCurrency, currency)
        });
    }

    // 광고 시청
    public void LogAdWatch(string adType, string placement)
    {
        LogEvent("ad_watch", new Dictionary<string, object> {
            { "ad_type", adType }, // "rewarded", "interstitial", "banner"
            { "placement", placement } // "game_over", "shop", "dungeon" 등
        });
    }

    // 세션 시작
    public void LogSessionStart()
    {
        FirebaseAnalytics.LogEvent("session_start");
    }

    // 세션 종료
    public void LogSessionEnd(int playTimeSeconds)
    {
        LogEvent("session_end", new Dictionary<string, object> {
            { "play_time", playTimeSeconds }
        });
    }
}
```

### 8.2 게임 내 Analytics 호출 예시

```csharp
// GameManager.cs
public class GameManager : MonoBehaviour
{
    void Start()
    {
        AnalyticsManager.Instance.LogSessionStart();
    }

    public void OnPlayerLevelUp(int newLevel)
    {
        AnalyticsManager.Instance.LogLevelUp(newLevel);
    }

    public void OnDungeonCleared(string dungeonId, int clearTime, bool isFirstClear)
    {
        AnalyticsManager.Instance.LogDungeonClear(dungeonId, clearTime, isFirstClear);
    }

    void OnApplicationQuit()
    {
        int playTime = (int)(Time.realtimeSinceStartup);
        AnalyticsManager.Instance.LogSessionEnd(playTime);
    }
}

// PurchaseManager.cs
public void OnPurchaseSuccess(string productId, double price)
{
    AnalyticsManager.Instance.LogPurchase(productId, price, "KRW");
}
```

---

## 9. Crashlytics 구현

### 9.1 Crashlytics 설정

```csharp
using Firebase.Crashlytics;

public class CrashlyticsManager : MonoBehaviour
{
    void Start()
    {
        // Crashlytics 초기화
        Crashlytics.ReportUncaughtExceptionsAsFatal = true;

        // 커스텀 로그
        Crashlytics.Log("게임 시작");

        // 유저 ID 설정
        string userId = FirebaseManager.Instance.CurrentUser?.UserId;
        if (!string.IsNullOrEmpty(userId))
        {
            Crashlytics.SetUserId(userId);
        }

        // 커스텀 키-값
        Crashlytics.SetCustomKey("player_level", PlayerManager.Instance.GetLevel());
        Crashlytics.SetCustomKey("gold", PlayerManager.Instance.GetGold());
    }

    // 예외 처리 예시
    public void DoSomething()
    {
        try
        {
            // 위험한 작업
            LoadData();
        }
        catch (Exception e)
        {
            // Crashlytics에 보고
            Crashlytics.LogException(e);

            // 유저에게 알림
            UIManager.Instance.ShowError("데이터 로드 실패");
        }
    }

    // 강제 크래시 테스트 (테스트용만!)
    public void TestCrash()
    {
        Crashlytics.IsCrashlyticsCollectionEnabled = true;
        throw new System.Exception("테스트 크래시");
    }
}
```

---

## 10. Cloud Storage 구현

### 10.1 StorageManager 구현 (선택사항)

```csharp
using Firebase.Storage;
using System;
using System.Threading.Tasks;

public class CloudStorageManager : MonoBehaviour
{
    private FirebaseStorage storage;
    private StorageReference storageRef;

    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://your-project-id.appspot.com");
    }

    // 스크린샷 업로드
    public async Task<string> UploadScreenshot(byte[] imageData, string fileName)
    {
        try
        {
            string userId = FirebaseManager.Instance.CurrentUser.UserId;
            StorageReference fileRef = storageRef.Child($"screenshots/{userId}/{fileName}");

            await fileRef.PutBytesAsync(imageData);

            // 다운로드 URL 가져오기
            string downloadUrl = await fileRef.GetDownloadUrlAsync();
            return downloadUrl;
        }
        catch (Exception e)
        {
            Debug.LogError($"업로드 실패: {e.Message}");
            return null;
        }
    }

    // 파일 다운로드
    public async Task<byte[]> DownloadFile(string path)
    {
        try
        {
            StorageReference fileRef = storageRef.Child(path);
            byte[] fileBytes = await fileRef.GetBytesAsync(1024 * 1024 * 10); // 10MB 제한
            return fileBytes;
        }
        catch (Exception e)
        {
            Debug.LogError($"다운로드 실패: {e.Message}");
            return null;
        }
    }
}
```

---

## 11. 보안 설정

### 11.1 Firestore 보안 규칙 (상세)

```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {

    // 헬퍼 함수들
    function isSignedIn() {
      return request.auth != null;
    }

    function isOwner(userId) {
      return isSignedIn() && request.auth.uid == userId;
    }

    function isValidSaveData(data) {
      return data.keys().hasAll(['playerLevel', 'gold', 'exp'])
        && data.playerLevel is int
        && data.gold is int
        && data.exp is int
        && data.playerLevel >= 0
        && data.playerLevel <= 10000
        && data.gold >= 0;
    }

    // 유저 데이터
    match /users/{userId} {
      allow read: if isOwner(userId);
      allow create: if isOwner(userId) && isValidSaveData(request.resource.data);
      allow update: if isOwner(userId) && isValidSaveData(request.resource.data);
      allow delete: if false; // 삭제 불가
    }

    // 구매 기록 (서버만 쓰기 가능)
    match /purchases/{purchaseId} {
      allow read: if isOwner(resource.data.userId);
      allow write: if false; // Cloud Functions만 쓰기 가능
    }

    // 리더보드 (모두 읽기, 본인만 쓰기)
    match /leaderboard/{userId} {
      allow read: if true;
      allow create, update: if isOwner(userId)
        && request.resource.data.score is int
        && request.resource.data.score >= 0
        && request.resource.data.score <= 999999999;
      allow delete: if false;
    }

    // 게임 설정 (읽기 전용)
    match /config/{document=**} {
      allow read: if true;
      allow write: if false;
    }
  }
}
```

### 11.2 Storage 보안 규칙

```javascript
rules_version = '2';
service firebase.storage {
  match /b/{bucket}/o {
    // 스크린샷 (본인만 읽기/쓰기, 최대 5MB)
    match /screenshots/{userId}/{fileName} {
      allow read, write: if request.auth != null
        && request.auth.uid == userId
        && request.resource.size < 5 * 1024 * 1024;
    }

    // 공개 이미지 (모두 읽기, 쓰기 불가)
    match /public/{allPaths=**} {
      allow read: if true;
      allow write: if false;
    }
  }
}
```

### 11.3 Unity 코드 난독화

```csharp
// IL2CPP + Code Stripping 사용
// Build Settings → Player Settings → Other Settings
// - Scripting Backend: IL2CPP
// - Managed Stripping Level: High
// - Strip Engine Code: 체크

// API 키 숨기기 (BuildConfig 사용)
public static class SecureConfig
{
    private const string ENCRYPTED_KEY = "enc_3kf93kf..."; // 암호화된 키

    public static string GetApiKey()
    {
        // 런타임에 복호화
        return Decrypt(ENCRYPTED_KEY);
    }

    private static string Decrypt(string encrypted)
    {
        // AES 복호화 등
        return encrypted; // 실제로는 복호화 로직 구현
    }
}
```

---

## 12. 테스트 및 디버깅

### 12.1 Firebase 디버그 로깅 활성화

```csharp
// 디버그 모드
#if UNITY_EDITOR || DEVELOPMENT_BUILD
Firebase.LogLevel = Firebase.LogLevel.Debug;
#else
Firebase.LogLevel = Firebase.LogLevel.Warning;
#endif
```

### 12.2 테스트 시나리오

```csharp
public class FirebaseTestSuite : MonoBehaviour
{
    public async void TestAll()
    {
        Debug.Log("=== Firebase 통합 테스트 시작 ===");

        // 1. 초기화 테스트
        while (!FirebaseManager.Instance.IsInitialized)
        {
            await Task.Delay(100);
        }
        Debug.Log("✓ Firebase 초기화 완료");

        // 2. 인증 테스트
        if (FirebaseManager.Instance.CurrentUser == null)
        {
            FirebaseManager.Instance.SignInAnonymously();
            await Task.Delay(2000);
        }
        Debug.Log($"✓ 로그인 완료: {FirebaseManager.Instance.CurrentUser.UserId}");

        // 3. Firestore 쓰기 테스트
        await CloudSaveManager.Instance.SaveToCloud();
        Debug.Log("✓ Firestore 저장 완료");

        // 4. Firestore 읽기 테스트
        var data = await CloudSaveManager.Instance.LoadFromCloud();
        Debug.Log($"✓ Firestore 로드 완료: {data != null}");

        // 5. Remote Config 테스트
        await RemoteConfigManager.Instance.FetchRemoteConfig();
        Debug.Log($"✓ Remote Config: gold_multiplier = {RemoteConfigManager.Instance.GetDouble("gold_multiplier")}");

        // 6. Analytics 테스트
        AnalyticsManager.Instance.LogEvent("test_event");
        Debug.Log("✓ Analytics 이벤트 전송");

        // 7. Functions 테스트
        var serverTime = await CloudFunctionsManager.Instance.GetServerTime();
        Debug.Log($"✓ Server Time: {serverTime}");

        Debug.Log("=== 모든 테스트 통과! ===");
    }
}
```

### 12.3 Firebase Console에서 확인

```
1. Authentication → Users: 익명 유저 생성 확인
2. Firestore → users/{userId}: 데이터 저장 확인
3. Remote Config → 값 변경 후 앱 재시작해서 적용 확인
4. Analytics → Events: 이벤트 발생 확인 (최대 24시간 지연)
5. Crashlytics → Crashes: 크래시 리포트 확인
```

---

## 13. 배포 가이드

### 13.1 빌드 전 체크리스트

```
✅ google-services.json 포함 (Android)
✅ GoogleService-Info.plist 포함 (iOS)
✅ Firebase SDK 최신 버전
✅ Firestore 규칙 배포
✅ Cloud Functions 배포
✅ Remote Config 기본값 설정
✅ Analytics 이벤트 테스트
✅ 디버그 로그 제거
✅ API 키 보안 확인
```

### 13.2 Android 빌드

```
1. Build Settings → Android
2. Player Settings:
   - Package Name 확인 (Firebase와 동일)
   - Minimum API Level: 19
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64 필수
3. Build
```

### 13.3 iOS 빌드

```
1. Build Settings → iOS
2. Player Settings:
   - Bundle Identifier 확인 (Firebase와 동일)
   - Minimum iOS Version: 11.0
3. Build
4. Xcode에서:
   - Push Notifications Capability 추가 (선택)
   - GoogleService-Info.plist 포함 확인
```

### 13.4 첫 출시 후 모니터링

```
Firebase Console에서 확인:
1. Authentication: 신규 유저 수
2. Firestore: 저장 용량, 읽기/쓰기 횟수
3. Analytics: DAU, 리텐션, 주요 이벤트
4. Crashlytics: 크래시 발생률
5. Performance: 앱 시작 시간, 네트워크 지연
6. 비용: Blaze 플랜 사용량
```

---

## 14. 비용 최적화

### 14.1 최적화 체크리스트

```
✅ Firestore 오프라인 캐싱 활성화
✅ 불필요한 읽기/쓰기 제거
✅ 배치 쓰기 사용
✅ 데이터 압축 (GZip)
✅ Remote Config 활용 (무료!)
✅ 로컬 캐싱 적극 활용
✅ 너무 자주 저장하지 않기 (5분 간격)
✅ Analytics 샘플링 (선택)
```

### 14.2 비용 알림 설정

```
1. Google Cloud Console 접속
2. 결제 → 예산 및 알림
3. 예산 생성:
   - 이름: Firebase Monthly Budget
   - 금액: $50 (또는 원하는 금액)
   - 알림 임계값: 80%, 100%, 120%
   - 이메일 알림 설정
```

---

## 15. 문제 해결

### 15.1 자주 발생하는 오류

**오류 1: DllNotFoundException: firebase_app**
```
해결: External Dependency Manager 실행
Assets → External Dependency Manager → Android Resolver → Resolve
```

**오류 2: google-services.json 인식 안 됨**
```
해결:
1. Assets/Plugins/Android/ 경로 확인
2. Package Name이 Firebase Console과 동일한지 확인
3. Unity 재시작
```

**오류 3: iOS 빌드 시 Firebase 프레임워크 오류**
```
해결:
1. Assets → External Dependency Manager → iOS Resolver → Install Cocoapods
2. Xcode에서 Pods 프로젝트 확인
3. Deployment Target 확인 (11.0 이상)
```

**오류 4: Firestore 권한 오류 (PERMISSION_DENIED)**
```
해결:
1. Firestore 규칙 확인
2. 로그인 상태 확인
3. userId 일치 확인
```

**오류 5: Functions 호출 실패**
```
해결:
1. Blaze 플랜 확인
2. Functions 배포 확인 (firebase deploy --only functions)
3. 네트워크 연결 확인
4. CORS 설정 확인 (웹의 경우)
```

### 15.2 디버깅 팁

```csharp
// Firebase 디버그 정보 출력
Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;

// Firestore 오프라인 모드 확인
bool isOffline = !await FirebaseFirestore.DefaultInstance
    .Collection("test").Document("ping").GetSnapshotAsync()
    .ContinueWith(task => task.IsCompletedSuccessfully);

if (isOffline)
{
    Debug.LogWarning("오프라인 모드 (캐시 사용 중)");
}
```

### 15.3 지원 받기

```
Firebase 공식 문서:
https://firebase.google.com/docs/unity/setup

Unity 포럼:
https://forum.unity.com/forums/firebase.654/

Stack Overflow:
태그: [firebase] [unity3d]

Firebase Support:
Firebase Console → Support 탭
```

---

## 16. 다음 단계

### 완료한 것 ✅
- Firebase 프로젝트 설정
- Unity SDK 통합
- Authentication (익명 로그인)
- Cloud Firestore (클라우드 세이브)
- Cloud Functions (영수증 검증, 서버 시간)
- Remote Config (라이브 밸런싱)
- Analytics (KPI 추적)
- Crashlytics (버그 리포트)

### 추가로 고려할 것
- [ ] 소셜 로그인 (Google, Apple)
- [ ] Push 알림 (FCM)
- [ ] Dynamic Links (딥링크)
- [ ] A/B Testing
- [ ] Performance Monitoring
- [ ] In-App Messaging
- [ ] Test Lab (자동 테스트)

---

## 부록 A: 완전한 예제 프로젝트 구조

```
Assets/
├── Plugins/
│   └── Android/
│       └── google-services.json
├── GoogleService-Info.plist (iOS용)
├── Scripts/
│   ├── Firebase/
│   │   ├── FirebaseManager.cs
│   │   ├── CloudSaveManager.cs
│   │   ├── CloudFunctionsManager.cs
│   │   ├── RemoteConfigManager.cs
│   │   ├── AnalyticsManager.cs
│   │   └── CrashlyticsManager.cs
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── PlayerManager.cs
│   │   └── UIManager.cs
│   └── Utils/
│       └── GZipHelper.cs
└── Resources/
    └── GameData/
        ├── Items.json
        ├── Skills.json
        └── ...
```

---

## 부록 B: 참고 자료

**공식 문서:**
- Firebase Unity SDK: https://firebase.google.com/docs/unity/setup
- Firestore: https://firebase.google.com/docs/firestore
- Cloud Functions: https://firebase.google.com/docs/functions
- Remote Config: https://firebase.google.com/docs/remote-config

**샘플 프로젝트:**
- Firebase Unity Samples: https://github.com/firebase/quickstart-unity

**커뮤니티:**
- Unity 포럼: https://forum.unity.com/forums/firebase.654/
- Firebase Discord: https://discord.gg/firebase

---

**작성일:** 2025-11-13
**버전:** 1.0
**작성자:** AI Assistant
**최종 수정:** 2025-11-13
