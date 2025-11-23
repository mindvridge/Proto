using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HiddenGrowth.Data;

namespace HiddenGrowth.Managers
{
    /// <summary>
    /// 전투 매니저 - 몬스터 스폰, 데미지 처리, 보상 시스템
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        #region Events
        public event Action<MonsterData> OnMonsterSpawned;           // 몬스터 스폰
        public event Action<long, long, bool> OnMonsterDamaged;      // currentHP, maxHP, isCritical
        public event Action<MonsterData, BattleRewards> OnMonsterDefeated; // 몬스터 처치
        public event Action<int> OnStageChanged;                      // 스테이지 변경
        public event Action<MonsterData> OnBossEncounter;             // 보스 조우
        public event Action<List<DropResult>> OnItemDropped;          // 아이템 드롭
        public event Action OnBattleStart;
        public event Action OnBattleEnd;
        #endregion

        #region Settings
        [Header("=== JSON Settings ===")]
        [SerializeField] private string monsterJsonFileName = "Monsters";
        [SerializeField] private bool loadFromStreamingAssets = false;

        [Header("=== Stage Settings ===")]
        [SerializeField] private int currentStage = 1;
        [SerializeField] private int monstersPerStage = 10;
        [SerializeField] private int bossEveryNStages = 10;

        [Header("=== Difficulty Settings ===")]
        [SerializeField] private float healthScalePerStage = 1.1f;    // 스테이지당 HP 증가율
        [SerializeField] private float attackScalePerStage = 1.05f;   // 스테이지당 공격력 증가율
        [SerializeField] private float goldScalePerStage = 1.08f;     // 스테이지당 골드 증가율
        [SerializeField] private float expScalePerStage = 1.1f;       // 스테이지당 경험치 증가율

        [Header("=== Monster Attack Settings ===")]
        [SerializeField] private float monsterAttackInterval = 2f;    // 몬스터 공격 간격
        [SerializeField] private bool monsterAttackEnabled = true;    // 몬스터 공격 활성화
        #endregion

        #region State
        private Dictionary<string, MonsterData> monsterDatabase;
        private Dictionary<MonsterGrade, List<MonsterData>> monstersByGrade;
        private MonsterData currentMonster;
        private long currentMonsterHP;
        private long currentMonsterMaxHP;
        private int monstersDefeatedInStage;
        private bool isInitialized = false;
        private bool isBossBattle = false;
        private Coroutine monsterAttackCoroutine;

        // 참조
        private PlayerStats playerStats;
        #endregion

        #region Properties
        public MonsterData CurrentMonster => currentMonster;
        public long CurrentMonsterHP => currentMonsterHP;
        public long CurrentMonsterMaxHP => currentMonsterMaxHP;
        public float MonsterHPPercent => currentMonsterMaxHP > 0 ? (float)currentMonsterHP / currentMonsterMaxHP : 0;
        public int CurrentStage => currentStage;
        public int MonstersDefeatedInStage => monstersDefeatedInStage;
        public bool IsBossBattle => isBossBattle;
        public bool IsInitialized => isInitialized;
        public bool HasMonster => currentMonster != null && currentMonsterHP > 0;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Initialize();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            monsterDatabase = new Dictionary<string, MonsterData>();
            monstersByGrade = new Dictionary<MonsterGrade, List<MonsterData>>();

            LoadMonsterData();
            isInitialized = true;

            Debug.Log($"[BattleManager] Initialized with {monsterDatabase.Count} monsters");
        }

        /// <summary>
        /// PlayerStats 설정
        /// </summary>
        public void SetPlayerStats(PlayerStats stats)
        {
            playerStats = stats;
        }

        /// <summary>
        /// JSON에서 몬스터 데이터 로드
        /// </summary>
        private void LoadMonsterData()
        {
            string json = LoadJsonData();
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[BattleManager] Failed to load monster JSON");
                return;
            }

            try
            {
                MonsterContainer container = JsonUtility.FromJson<MonsterContainer>(json);
                if (container?.monsters == null)
                {
                    Debug.LogError("[BattleManager] Invalid JSON format");
                    return;
                }

                ProcessMonsterData(container.monsters);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BattleManager] JSON parse error: {e.Message}");
            }
        }

        /// <summary>
        /// JSON 데이터 로드
        /// </summary>
        private string LoadJsonData()
        {
            if (loadFromStreamingAssets)
            {
                string path = Path.Combine(Application.streamingAssetsPath, $"{monsterJsonFileName}.json");
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }

            TextAsset textAsset = Resources.Load<TextAsset>($"GameData/{monsterJsonFileName}");
            if (textAsset != null)
            {
                return textAsset.text;
            }

            Debug.LogWarning($"[BattleManager] Resource not found: GameData/{monsterJsonFileName}");
            return null;
        }

        /// <summary>
        /// 몬스터 데이터 처리 및 인덱싱
        /// </summary>
        private void ProcessMonsterData(List<MonsterData> monsters)
        {
            foreach (var monster in monsters)
            {
                if (string.IsNullOrEmpty(monster.monsterId)) continue;

                // ID로 인덱싱
                monsterDatabase[monster.monsterId] = monster;

                // 등급별 인덱싱
                MonsterGrade grade = monster.GetGrade();
                if (!monstersByGrade.ContainsKey(grade))
                {
                    monstersByGrade[grade] = new List<MonsterData>();
                }
                monstersByGrade[grade].Add(monster);
            }
        }
        #endregion

        #region Battle Control
        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartBattle()
        {
            OnBattleStart?.Invoke();
            SpawnNextMonster();
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        public void EndBattle()
        {
            StopMonsterAttack();
            currentMonster = null;
            currentMonsterHP = 0;
            OnBattleEnd?.Invoke();
        }

        /// <summary>
        /// 다음 몬스터 스폰
        /// </summary>
        public void SpawnNextMonster()
        {
            // 보스 스테이지 체크
            if (monstersDefeatedInStage >= monstersPerStage - 1 &&
                currentStage % bossEveryNStages == 0 &&
                !isBossBattle)
            {
                SpawnBoss();
                return;
            }

            // 일반 몬스터 스폰
            SpawnMonster();
        }

        /// <summary>
        /// 일반 몬스터 스폰
        /// </summary>
        private void SpawnMonster()
        {
            isBossBattle = false;

            // 스테이지에 맞는 등급 결정
            MonsterGrade targetGrade = GetGradeForStage(currentStage);
            MonsterData template = GetRandomMonsterByGrade(targetGrade);

            if (template == null)
            {
                Debug.LogError($"[BattleManager] No monster found for grade: {targetGrade}");
                return;
            }

            SetupCurrentMonster(template);
        }

        /// <summary>
        /// 보스 몬스터 스폰
        /// </summary>
        private void SpawnBoss()
        {
            isBossBattle = true;

            MonsterGrade targetGrade = GetGradeForStage(currentStage);
            MonsterData template = GetRandomBossByGrade(targetGrade);

            if (template == null)
            {
                // 보스가 없으면 일반 몬스터로 대체
                template = GetRandomMonsterByGrade(targetGrade);
            }

            if (template == null)
            {
                Debug.LogError("[BattleManager] No boss monster found");
                return;
            }

            SetupCurrentMonster(template);
            OnBossEncounter?.Invoke(currentMonster);
        }

        /// <summary>
        /// 현재 몬스터 설정
        /// </summary>
        private void SetupCurrentMonster(MonsterData template)
        {
            currentMonster = template;

            // 스테이지 스케일링 적용
            float stageMultiplier = Mathf.Pow(healthScalePerStage, currentStage - 1);
            currentMonsterMaxHP = (long)(template.baseHealth * stageMultiplier);
            currentMonsterHP = currentMonsterMaxHP;

            // 보스는 추가 HP
            if (isBossBattle)
            {
                currentMonsterMaxHP = (long)(currentMonsterMaxHP * 3);
                currentMonsterHP = currentMonsterMaxHP;
            }

            OnMonsterSpawned?.Invoke(currentMonster);

            // 몬스터 공격 시작
            if (monsterAttackEnabled)
            {
                StartMonsterAttack();
            }

            Debug.Log($"[BattleManager] Spawned: {template.monsterName} (HP: {currentMonsterHP})");
        }

        /// <summary>
        /// 특정 몬스터 ID로 스폰
        /// </summary>
        public void SpawnMonsterById(string monsterId)
        {
            if (!monsterDatabase.TryGetValue(monsterId, out MonsterData template))
            {
                Debug.LogError($"[BattleManager] Monster not found: {monsterId}");
                return;
            }

            isBossBattle = template.isBoss;
            SetupCurrentMonster(template);
        }
        #endregion

        #region Damage System
        /// <summary>
        /// 몬스터에게 데미지 입히기
        /// </summary>
        public void DealDamageToMonster(long damage, bool isCritical = false)
        {
            if (currentMonster == null || currentMonsterHP <= 0) return;

            currentMonsterHP = Math.Max(0, currentMonsterHP - damage);
            OnMonsterDamaged?.Invoke(currentMonsterHP, currentMonsterMaxHP, isCritical);

            // 몬스터 처치
            if (currentMonsterHP <= 0)
            {
                OnMonsterKilled();
            }
        }

        /// <summary>
        /// 몬스터 처치 처리
        /// </summary>
        private void OnMonsterKilled()
        {
            StopMonsterAttack();

            // 보상 계산
            BattleRewards rewards = CalculateRewards();

            // 드롭 아이템 계산
            List<DropResult> drops = CalculateDrops();

            // 플레이어에게 보상 지급
            if (playerStats != null)
            {
                playerStats.AddGold(rewards.gold);
                playerStats.AddExp(rewards.exp);
            }

            // 이벤트 발생
            OnMonsterDefeated?.Invoke(currentMonster, rewards);
            if (drops.Count > 0)
            {
                OnItemDropped?.Invoke(drops);
            }

            monstersDefeatedInStage++;

            // 스테이지 클리어 체크
            if (monstersDefeatedInStage >= monstersPerStage || isBossBattle)
            {
                AdvanceStage();
            }
            else
            {
                // 다음 몬스터 스폰 (약간의 딜레이)
                StartCoroutine(SpawnNextMonsterDelayed(0.5f));
            }
        }

        /// <summary>
        /// 보상 계산
        /// </summary>
        private BattleRewards CalculateRewards()
        {
            if (currentMonster == null) return new BattleRewards();

            float stageGoldMult = Mathf.Pow(goldScalePerStage, currentStage - 1);
            float stageExpMult = Mathf.Pow(expScalePerStage, currentStage - 1);

            long baseGold = currentMonster.GetRandomGold();
            long gold = (long)(baseGold * stageGoldMult);
            long exp = (long)(currentMonster.expReward * stageExpMult);

            // 보스 보너스
            if (isBossBattle)
            {
                gold *= 5;
                exp *= 3;
            }

            return new BattleRewards
            {
                gold = gold,
                exp = exp,
                isBoss = isBossBattle
            };
        }

        /// <summary>
        /// 드롭 아이템 계산
        /// </summary>
        private List<DropResult> CalculateDrops()
        {
            List<DropResult> drops = new List<DropResult>();

            if (currentMonster == null || !currentMonster.HasDropTable)
                return drops;

            // 드롭률 보너스 적용
            float dropBonus = playerStats?.DropRateBonus ?? 0f;

            foreach (var dropItem in currentMonster.dropTable)
            {
                float adjustedRate = dropItem.dropRate + dropBonus;

                if (UnityEngine.Random.value <= adjustedRate)
                {
                    drops.Add(new DropResult
                    {
                        itemId = dropItem.itemId,
                        amount = dropItem.GetRandomAmount()
                    });
                }
            }

            return drops;
        }
        #endregion

        #region Stage System
        /// <summary>
        /// 다음 스테이지로 진행
        /// </summary>
        private void AdvanceStage()
        {
            currentStage++;
            monstersDefeatedInStage = 0;
            isBossBattle = false;

            OnStageChanged?.Invoke(currentStage);
            Debug.Log($"[BattleManager] Stage advanced to: {currentStage}");

            // 다음 몬스터 스폰
            StartCoroutine(SpawnNextMonsterDelayed(1f));
        }

        /// <summary>
        /// 특정 스테이지로 이동
        /// </summary>
        public void GoToStage(int stage)
        {
            if (stage < 1) return;

            currentStage = stage;
            monstersDefeatedInStage = 0;
            isBossBattle = false;

            OnStageChanged?.Invoke(currentStage);
            SpawnNextMonster();
        }

        /// <summary>
        /// 스테이지에 맞는 몬스터 등급 결정
        /// </summary>
        private MonsterGrade GetGradeForStage(int stage)
        {
            // 스테이지별 등급 매핑
            if (stage <= 10) return MonsterGrade.F;
            if (stage <= 25) return MonsterGrade.E;
            if (stage <= 50) return MonsterGrade.D;
            if (stage <= 80) return MonsterGrade.C;
            if (stage <= 120) return MonsterGrade.B;
            if (stage <= 180) return MonsterGrade.A;
            if (stage <= 250) return MonsterGrade.S;
            if (stage <= 350) return MonsterGrade.SS;
            if (stage <= 500) return MonsterGrade.SSS;
            if (stage <= 700) return MonsterGrade.EX;
            if (stage <= 900) return MonsterGrade.Mythic;
            return MonsterGrade.Infinite;
        }
        #endregion

        #region Monster Attack
        /// <summary>
        /// 몬스터 공격 시작
        /// </summary>
        private void StartMonsterAttack()
        {
            if (monsterAttackCoroutine != null)
            {
                StopCoroutine(monsterAttackCoroutine);
            }
            monsterAttackCoroutine = StartCoroutine(MonsterAttackCoroutine());
        }

        /// <summary>
        /// 몬스터 공격 중지
        /// </summary>
        private void StopMonsterAttack()
        {
            if (monsterAttackCoroutine != null)
            {
                StopCoroutine(monsterAttackCoroutine);
                monsterAttackCoroutine = null;
            }
        }

        /// <summary>
        /// 몬스터 공격 코루틴
        /// </summary>
        private IEnumerator MonsterAttackCoroutine()
        {
            while (currentMonster != null && currentMonsterHP > 0)
            {
                yield return new WaitForSeconds(monsterAttackInterval);

                if (currentMonster == null || currentMonsterHP <= 0) break;

                // 몬스터 데미지 계산
                float stageMult = Mathf.Pow(attackScalePerStage, currentStage - 1);
                long damage = (long)(currentMonster.baseAttack * stageMult);

                // 보스는 추가 데미지
                if (isBossBattle)
                {
                    damage = (long)(damage * 1.5f);
                }

                // 플레이어에게 데미지
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damage);
                    Debug.Log($"[BattleManager] Monster dealt {damage} damage to player");
                }
            }
        }
        #endregion

        #region Query Methods
        /// <summary>
        /// 등급별 랜덤 몬스터 가져오기
        /// </summary>
        public MonsterData GetRandomMonsterByGrade(MonsterGrade grade)
        {
            if (!monstersByGrade.TryGetValue(grade, out List<MonsterData> list))
                return null;

            // 보스가 아닌 몬스터만 필터링
            var normalMonsters = list.FindAll(m => !m.isBoss);
            if (normalMonsters.Count == 0) return null;

            return normalMonsters[UnityEngine.Random.Range(0, normalMonsters.Count)];
        }

        /// <summary>
        /// 등급별 랜덤 보스 가져오기
        /// </summary>
        public MonsterData GetRandomBossByGrade(MonsterGrade grade)
        {
            if (!monstersByGrade.TryGetValue(grade, out List<MonsterData> list))
                return null;

            var bosses = list.FindAll(m => m.isBoss);
            if (bosses.Count == 0) return null;

            return bosses[UnityEngine.Random.Range(0, bosses.Count)];
        }

        /// <summary>
        /// ID로 몬스터 데이터 가져오기
        /// </summary>
        public MonsterData GetMonsterById(string monsterId)
        {
            monsterDatabase.TryGetValue(monsterId, out MonsterData data);
            return data;
        }

        /// <summary>
        /// 모든 몬스터 ID 가져오기
        /// </summary>
        public List<string> GetAllMonsterIds()
        {
            return new List<string>(monsterDatabase.Keys);
        }
        #endregion

        #region Utility
        /// <summary>
        /// 딜레이 후 몬스터 스폰
        /// </summary>
        private IEnumerator SpawnNextMonsterDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnNextMonster();
        }
        #endregion

        #region Debug
#if UNITY_EDITOR
        [ContextMenu("Debug: Spawn Next Monster")]
        private void DebugSpawnMonster()
        {
            SpawnNextMonster();
        }

        [ContextMenu("Debug: Kill Current Monster")]
        private void DebugKillMonster()
        {
            if (currentMonster != null)
            {
                currentMonsterHP = 0;
                OnMonsterKilled();
            }
        }

        [ContextMenu("Debug: Advance Stage")]
        private void DebugAdvanceStage()
        {
            AdvanceStage();
        }

        [ContextMenu("Debug: Spawn Boss")]
        private void DebugSpawnBoss()
        {
            SpawnBoss();
        }

        [ContextMenu("Print Monster Database")]
        private void PrintMonsterDatabase()
        {
            Debug.Log("=== Monster Database ===");
            foreach (var kvp in monstersByGrade)
            {
                Debug.Log($"Grade {kvp.Key}: {kvp.Value.Count} monsters");
            }
        }
#endif
        #endregion
    }

    /// <summary>
    /// 전투 보상
    /// </summary>
    [Serializable]
    public struct BattleRewards
    {
        public long gold;
        public long exp;
        public bool isBoss;
    }

    /// <summary>
    /// 드롭 결과
    /// </summary>
    [Serializable]
    public struct DropResult
    {
        public string itemId;
        public int amount;
    }
}
