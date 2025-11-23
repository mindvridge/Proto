using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HiddenGrowth.Data;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 클리커 매니저 - 터치/클릭 공격 처리
    /// </summary>
    public class ClickerManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static ClickerManager Instance { get; private set; }

        #region Events
        public event Action<DamageResult> OnTapAttack;              // 탭 공격 시
        public event Action<DamageResult> OnAutoAttack;             // 자동 공격 시
        public event Action<int> OnComboChanged;                    // 콤보 변경 시
        public event Action<Vector2> OnTapPosition;                 // 탭 위치 (이펙트용)
        public event Action OnTapHoldStart;                         // 터치 홀드 시작
        public event Action OnTapHoldEnd;                           // 터치 홀드 종료
        #endregion

        #region Settings
        [Header("=== Tap Settings ===")]
        [SerializeField] private float tapCooldown = 0.05f;         // 탭 간 최소 간격
        [SerializeField] private int maxTapsPerSecond = 20;         // 초당 최대 탭 수
        [SerializeField] private bool enableMultiTouch = true;      // 멀티터치 허용

        [Header("=== Combo Settings ===")]
        [SerializeField] private float comboResetTime = 2f;         // 콤보 리셋 시간
        [SerializeField] private int maxCombo = 9999;               // 최대 콤보
        [SerializeField] private float comboDamageBonus = 0.01f;    // 콤보당 데미지 보너스 (1%)

        [Header("=== Auto Attack Settings ===")]
        [SerializeField] private float autoAttackInterval = 1f;     // 자동 공격 간격
        [SerializeField] private bool autoAttackEnabled = false;    // 자동 공격 활성화

        [Header("=== Hold Attack Settings ===")]
        [SerializeField] private float holdAttackInterval = 0.1f;   // 홀드 시 공격 간격
        [SerializeField] private bool holdAttackEnabled = true;     // 홀드 공격 활성화
        #endregion

        #region State
        private float lastTapTime;
        private int tapCountThisSecond;
        private float tapCountResetTime;
        private int currentCombo;
        private float lastComboTime;
        private bool isHolding;
        private Coroutine holdAttackCoroutine;
        private Coroutine autoAttackCoroutine;

        // 참조
        private PlayerStats playerStats;
        private BattleManager battleManager;
        #endregion

        #region Properties
        public int CurrentCombo => currentCombo;
        public bool IsHolding => isHolding;
        public bool AutoAttackEnabled
        {
            get => autoAttackEnabled;
            set
            {
                autoAttackEnabled = value;
                if (autoAttackEnabled)
                    StartAutoAttack();
                else
                    StopAutoAttack();
            }
        }

        /// <summary>
        /// 현재 콤보 데미지 배율
        /// </summary>
        public float ComboDamageMultiplier => 1f + (currentCombo * comboDamageBonus);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // 참조 초기화
            battleManager = BattleManager.Instance;

            // 자동 공격 시작 (활성화된 경우)
            if (autoAttackEnabled)
            {
                StartAutoAttack();
            }
        }

        private void Update()
        {
            // 콤보 리셋 체크
            CheckComboReset();

            // 탭 카운트 리셋 (초당)
            CheckTapCountReset();

            // 키보드 입력 처리 (PC 테스트용)
            HandleKeyboardInput();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// PlayerStats 설정
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            playerStats = stats;
        }

        /// <summary>
        /// 수동 탭 공격 실행
        /// </summary>
        public void ExecuteTapAttack(Vector2 screenPosition)
        {
            if (!CanTap()) return;

            lastTapTime = Time.time;
            tapCountThisSecond++;

            // 데미지 계산
            DamageResult result = CalculateTapDamage();

            // 콤보 증가
            IncrementCombo();

            // 이벤트 발생
            OnTapPosition?.Invoke(screenPosition);
            OnTapAttack?.Invoke(result);

            // BattleManager에 데미지 전달
            if (battleManager != null)
            {
                battleManager.DealDamageToMonster(result.damage, result.isCritical);
            }
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                currentCombo = 0;
                OnComboChanged?.Invoke(currentCombo);
            }
        }

        /// <summary>
        /// 자동 공격 시작
        /// </summary>
        public void StartAutoAttack()
        {
            if (autoAttackCoroutine != null)
            {
                StopCoroutine(autoAttackCoroutine);
            }
            autoAttackCoroutine = StartCoroutine(AutoAttackCoroutine());
        }

        /// <summary>
        /// 자동 공격 중지
        /// </summary>
        public void StopAutoAttack()
        {
            if (autoAttackCoroutine != null)
            {
                StopCoroutine(autoAttackCoroutine);
                autoAttackCoroutine = null;
            }
        }

        /// <summary>
        /// 자동 공격 간격 설정
        /// </summary>
        public void SetAutoAttackInterval(float interval)
        {
            autoAttackInterval = Mathf.Max(0.1f, interval);

            // 자동 공격 재시작
            if (autoAttackEnabled)
            {
                StopAutoAttack();
                StartAutoAttack();
            }
        }
        #endregion

        #region Pointer Events
        public void OnPointerDown(PointerEventData eventData)
        {
            // UI 위에서의 클릭 무시 (필요시)
            // if (EventSystem.current.IsPointerOverGameObject()) return;

            // 탭 공격 실행
            ExecuteTapAttack(eventData.position);

            // 홀드 공격 시작
            if (holdAttackEnabled)
            {
                StartHoldAttack(eventData.position);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopHoldAttack();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 탭 가능 여부 확인
        /// </summary>
        private bool CanTap()
        {
            // 쿨다운 체크
            if (Time.time - lastTapTime < tapCooldown)
                return false;

            // 초당 탭 수 제한 체크
            if (tapCountThisSecond >= maxTapsPerSecond)
                return false;

            return true;
        }

        /// <summary>
        /// 탭 데미지 계산
        /// </summary>
        private DamageResult CalculateTapDamage()
        {
            DamageResult result;

            if (playerStats != null)
            {
                result = playerStats.CalculateDamage();
            }
            else
            {
                // 기본 데미지 (PlayerStats가 없는 경우)
                result = new DamageResult
                {
                    damage = 10,
                    isCritical = false
                };
            }

            // 콤보 보너스 적용
            result.damage = (long)(result.damage * ComboDamageMultiplier);

            return result;
        }

        /// <summary>
        /// 콤보 증가
        /// </summary>
        private void IncrementCombo()
        {
            lastComboTime = Time.time;

            if (currentCombo < maxCombo)
            {
                currentCombo++;
                OnComboChanged?.Invoke(currentCombo);
            }
        }

        /// <summary>
        /// 콤보 리셋 체크
        /// </summary>
        private void CheckComboReset()
        {
            if (currentCombo > 0 && Time.time - lastComboTime > comboResetTime)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 초당 탭 카운트 리셋 체크
        /// </summary>
        private void CheckTapCountReset()
        {
            if (Time.time - tapCountResetTime >= 1f)
            {
                tapCountThisSecond = 0;
                tapCountResetTime = Time.time;
            }
        }

        /// <summary>
        /// 키보드 입력 처리 (PC 테스트용)
        /// </summary>
        private void HandleKeyboardInput()
        {
            // Space 키로 공격
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ExecuteTapAttack(new Vector2(Screen.width / 2f, Screen.height / 2f));
            }

            // A 키로 자동 공격 토글
            if (Input.GetKeyDown(KeyCode.A))
            {
                AutoAttackEnabled = !AutoAttackEnabled;
                Debug.Log($"[ClickerManager] Auto Attack: {AutoAttackEnabled}");
            }
        }

        /// <summary>
        /// 홀드 공격 시작
        /// </summary>
        private void StartHoldAttack(Vector2 position)
        {
            if (isHolding) return;

            isHolding = true;
            OnTapHoldStart?.Invoke();

            if (holdAttackCoroutine != null)
            {
                StopCoroutine(holdAttackCoroutine);
            }
            holdAttackCoroutine = StartCoroutine(HoldAttackCoroutine(position));
        }

        /// <summary>
        /// 홀드 공격 중지
        /// </summary>
        private void StopHoldAttack()
        {
            if (!isHolding) return;

            isHolding = false;
            OnTapHoldEnd?.Invoke();

            if (holdAttackCoroutine != null)
            {
                StopCoroutine(holdAttackCoroutine);
                holdAttackCoroutine = null;
            }
        }
        #endregion

        #region Coroutines
        /// <summary>
        /// 홀드 공격 코루틴
        /// </summary>
        private IEnumerator HoldAttackCoroutine(Vector2 position)
        {
            // 첫 탭은 이미 처리됨, 대기 후 연속 공격
            yield return new WaitForSeconds(holdAttackInterval);

            while (isHolding)
            {
                if (CanTap())
                {
                    ExecuteTapAttack(position);
                }
                yield return new WaitForSeconds(holdAttackInterval);
            }
        }

        /// <summary>
        /// 자동 공격 코루틴
        /// </summary>
        private IEnumerator AutoAttackCoroutine()
        {
            while (autoAttackEnabled)
            {
                yield return new WaitForSeconds(autoAttackInterval);

                if (!autoAttackEnabled) break;

                // 자동 공격 데미지 계산 (콤보 미적용)
                DamageResult result;
                if (playerStats != null)
                {
                    result = playerStats.CalculateDamage();
                }
                else
                {
                    result = new DamageResult { damage = 10, isCritical = false };
                }

                // 이벤트 발생
                OnAutoAttack?.Invoke(result);

                // BattleManager에 데미지 전달
                if (battleManager != null)
                {
                    battleManager.DealDamageToMonster(result.damage, result.isCritical);
                }
            }
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Debug: Execute Tap")]
        private void DebugExecuteTap()
        {
            ExecuteTapAttack(new Vector2(Screen.width / 2f, Screen.height / 2f));
        }

        [ContextMenu("Debug: Toggle Auto Attack")]
        private void DebugToggleAutoAttack()
        {
            AutoAttackEnabled = !AutoAttackEnabled;
        }

        [ContextMenu("Debug: Add 100 Combo")]
        private void DebugAddCombo()
        {
            for (int i = 0; i < 100; i++)
            {
                IncrementCombo();
            }
        }
#endif
        #endregion
    }
}
