using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HiddenGrowth.Network
{
    /// <summary>
    /// 랭킹 매니저 - 리더보드 시스템
    /// </summary>
    public class RankingManager : MonoBehaviour
    {
        public static RankingManager Instance { get; private set; }

        #region Events
        public event Action<RankingData> OnRankingLoaded;
        public event Action<int> OnMyRankUpdated;
        public event Action<string> OnRankingError;
        #endregion

        #region Settings
        [Header("=== Ranking Settings ===")]
        [SerializeField] private int pageSize = 50;
        [SerializeField] private float refreshInterval = 300f;  // 자동 갱신 간격
        [SerializeField] private bool enableAutoRefresh = true;
        #endregion

        #region State
        private Dictionary<RankingType, RankingData> cachedRankings = new Dictionary<RankingType, RankingData>();
        private Dictionary<RankingType, float> lastRefreshTime = new Dictionary<RankingType, float>();
        private int myCurrentRank = -1;
        private bool isLoading = false;
        #endregion

        #region Properties
        public int MyRank => myCurrentRank;
        public bool IsLoading => isLoading;
        #endregion

        #region Unity Lifecycle
        private void Awake()
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

        private void Start()
        {
            if (enableAutoRefresh)
            {
                StartCoroutine(AutoRefreshCoroutine());
            }
        }
        #endregion

        #region Auto Refresh
        private IEnumerator AutoRefreshCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(refreshInterval);

                if (AuthManager.Instance?.IsLoggedIn == true)
                {
                    RefreshMyRank();
                }
            }
        }
        #endregion

        #region Get Rankings
        /// <summary>
        /// 랭킹 가져오기
        /// </summary>
        public void GetRanking(RankingType type, int page = 0, Action<RankingData> callback = null)
        {
            // 캐시 확인
            if (cachedRankings.ContainsKey(type) && !IsCacheExpired(type))
            {
                callback?.Invoke(cachedRankings[type]);
                OnRankingLoaded?.Invoke(cachedRankings[type]);
                return;
            }

            string endpoint = $"/ranking/{type.ToString().ToLower()}?page={page}&size={pageSize}";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var rankingData = response.GetData<RankingData>();
                    rankingData.type = type;

                    // 캐시 저장
                    cachedRankings[type] = rankingData;
                    lastRefreshTime[type] = Time.time;

                    callback?.Invoke(rankingData);
                    OnRankingLoaded?.Invoke(rankingData);
                }
                else
                {
                    OnRankingError?.Invoke(response.errorMessage);
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 주간 랭킹 가져오기
        /// </summary>
        public void GetWeeklyRanking(RankingType type, Action<RankingData> callback = null)
        {
            string endpoint = $"/ranking/{type.ToString().ToLower()}/weekly?size={pageSize}";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var rankingData = response.GetData<RankingData>();
                    rankingData.type = type;
                    rankingData.period = RankingPeriod.Weekly;
                    callback?.Invoke(rankingData);
                    OnRankingLoaded?.Invoke(rankingData);
                }
                else
                {
                    OnRankingError?.Invoke(response.errorMessage);
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 캐시 만료 확인
        /// </summary>
        private bool IsCacheExpired(RankingType type)
        {
            if (!lastRefreshTime.ContainsKey(type)) return true;
            return Time.time - lastRefreshTime[type] >= refreshInterval;
        }
        #endregion

        #region My Rank
        /// <summary>
        /// 내 랭킹 갱신
        /// </summary>
        public void RefreshMyRank(RankingType type = RankingType.TotalPower, Action<MyRankInfo> callback = null)
        {
            if (!AuthManager.Instance?.IsLoggedIn == true)
            {
                callback?.Invoke(null);
                return;
            }

            string endpoint = $"/ranking/{type.ToString().ToLower()}/me";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var myRank = response.GetData<MyRankInfo>();
                    myCurrentRank = myRank.rank;
                    OnMyRankUpdated?.Invoke(myCurrentRank);
                    callback?.Invoke(myRank);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 내 랭킹 주변 유저 가져오기
        /// </summary>
        public void GetRankingAroundMe(RankingType type, int range = 5, Action<RankingData> callback = null)
        {
            string endpoint = $"/ranking/{type.ToString().ToLower()}/around?range={range}";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var rankingData = response.GetData<RankingData>();
                    callback?.Invoke(rankingData);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }
        #endregion

        #region Score Submission
        /// <summary>
        /// 점수 제출
        /// </summary>
        public void SubmitScore(RankingType type, long score, Action<bool, int> callback = null)
        {
            var scoreData = new ScoreSubmitRequest
            {
                ranking_type = type.ToString(),
                score = score,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            NetworkManager.Instance?.Post($"/ranking/{type.ToString().ToLower()}/submit", scoreData, (response) =>
            {
                if (response.success)
                {
                    var result = response.GetData<ScoreSubmitResponse>();
                    myCurrentRank = result.new_rank;
                    OnMyRankUpdated?.Invoke(myCurrentRank);

                    // 캐시 무효화
                    if (cachedRankings.ContainsKey(type))
                    {
                        cachedRankings.Remove(type);
                    }

                    callback?.Invoke(true, result.new_rank);
                }
                else
                {
                    callback?.Invoke(false, -1);
                }
            });
        }

        /// <summary>
        /// 스테이지 기록 제출
        /// </summary>
        public void SubmitStageRecord(int stage, Action<bool> callback = null)
        {
            SubmitScore(RankingType.HighestStage, stage, (success, rank) => callback?.Invoke(success));
        }

        /// <summary>
        /// 레벨 기록 제출
        /// </summary>
        public void SubmitLevelRecord(int level, Action<bool> callback = null)
        {
            SubmitScore(RankingType.Level, level, (success, rank) => callback?.Invoke(success));
        }

        /// <summary>
        /// 전투력 기록 제출
        /// </summary>
        public void SubmitPowerRecord(long power, Action<bool> callback = null)
        {
            SubmitScore(RankingType.TotalPower, power, (success, rank) => callback?.Invoke(success));
        }
        #endregion

        #region Friends Ranking
        /// <summary>
        /// 친구 랭킹 가져오기
        /// </summary>
        public void GetFriendsRanking(RankingType type, Action<RankingData> callback = null)
        {
            string endpoint = $"/ranking/{type.ToString().ToLower()}/friends";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var rankingData = response.GetData<RankingData>();
                    rankingData.type = type;
                    rankingData.isFriendsOnly = true;
                    callback?.Invoke(rankingData);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }
        #endregion

        #region Guild Ranking
        /// <summary>
        /// 길드 랭킹 가져오기
        /// </summary>
        public void GetGuildRanking(GuildRankingType type, int page = 0, Action<GuildRankingData> callback = null)
        {
            string endpoint = $"/ranking/guild/{type.ToString().ToLower()}?page={page}&size={pageSize}";

            NetworkManager.Instance?.Get(endpoint, (response) =>
            {
                if (response.success)
                {
                    var rankingData = response.GetData<GuildRankingData>();
                    callback?.Invoke(rankingData);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }
        #endregion

        #region Ranking Rewards
        /// <summary>
        /// 랭킹 보상 정보 가져오기
        /// </summary>
        public void GetRankingRewards(RankingType type, Action<RankingRewardInfo[]> callback)
        {
            NetworkManager.Instance?.Get($"/ranking/{type.ToString().ToLower()}/rewards", (response) =>
            {
                if (response.success)
                {
                    var rewardData = response.GetData<RankingRewardListResponse>();
                    callback?.Invoke(rewardData?.rewards);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// 랭킹 보상 수령
        /// </summary>
        public void ClaimRankingReward(RankingType type, Action<bool, RankingRewardClaim> callback)
        {
            NetworkManager.Instance?.Post($"/ranking/{type.ToString().ToLower()}/claim", null, (response) =>
            {
                if (response.success)
                {
                    var rewardClaim = response.GetData<RankingRewardClaim>();
                    callback?.Invoke(true, rewardClaim);
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            });
        }
        #endregion

        #region Cache Management
        /// <summary>
        /// 캐시 클리어
        /// </summary>
        public void ClearCache()
        {
            cachedRankings.Clear();
            lastRefreshTime.Clear();
        }

        /// <summary>
        /// 특정 타입 캐시 클리어
        /// </summary>
        public void ClearCache(RankingType type)
        {
            if (cachedRankings.ContainsKey(type))
            {
                cachedRankings.Remove(type);
            }
            if (lastRefreshTime.ContainsKey(type))
            {
                lastRefreshTime.Remove(type);
            }
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// 랭킹 타입
    /// </summary>
    public enum RankingType
    {
        TotalPower,     // 전투력
        Level,          // 레벨
        HighestStage,   // 최고 스테이지
        Gold,           // 보유 골드
        TotalDamage,    // 총 데미지
        BossKills,      // 보스 처치
        PlayTime        // 플레이 시간
    }

    /// <summary>
    /// 길드 랭킹 타입
    /// </summary>
    public enum GuildRankingType
    {
        TotalPower,
        MemberCount,
        BossRaids,
        Donations
    }

    /// <summary>
    /// 랭킹 기간
    /// </summary>
    public enum RankingPeriod
    {
        AllTime,
        Weekly,
        Daily,
        Season
    }
    #endregion

    #region Data Classes
    [Serializable]
    public class RankingData
    {
        public RankingType type;
        public RankingPeriod period;
        public RankingEntry[] entries;
        public int totalCount;
        public int currentPage;
        public int totalPages;
        public bool isFriendsOnly;
        public string lastUpdated;
    }

    [Serializable]
    public class RankingEntry
    {
        public int rank;
        public string user_id;
        public string nickname;
        public string profile_image;
        public long score;
        public int level;
        public string guild_name;
        public int rank_change;  // 순위 변동 (+: 상승, -: 하락)
    }

    [Serializable]
    public class MyRankInfo
    {
        public int rank;
        public long score;
        public int percentile;      // 상위 몇 %
        public int rank_change;     // 이전 대비 순위 변동
        public long score_to_next;  // 다음 순위까지 필요 점수
    }

    [Serializable]
    public class GuildRankingData
    {
        public GuildRankingEntry[] entries;
        public int totalCount;
        public int currentPage;
    }

    [Serializable]
    public class GuildRankingEntry
    {
        public int rank;
        public string guild_id;
        public string guild_name;
        public string guild_emblem;
        public int member_count;
        public long total_power;
        public int rank_change;
    }

    [Serializable]
    public class ScoreSubmitRequest
    {
        public string ranking_type;
        public long score;
        public string timestamp;
    }

    [Serializable]
    public class ScoreSubmitResponse
    {
        public bool success;
        public int new_rank;
        public int previous_rank;
        public bool is_new_record;
    }

    [Serializable]
    public class RankingRewardInfo
    {
        public int min_rank;
        public int max_rank;
        public string reward_type;
        public int reward_amount;
        public string description;
    }

    [Serializable]
    public class RankingRewardListResponse
    {
        public RankingRewardInfo[] rewards;
    }

    [Serializable]
    public class RankingRewardClaim
    {
        public int final_rank;
        public string reward_type;
        public int reward_amount;
        public bool success;
    }
    #endregion
}
