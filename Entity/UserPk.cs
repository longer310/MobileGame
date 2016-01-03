using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Repository;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 用户竞技场
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserPk : KVEntity
    {
        /// <summary>
        /// 排名
        /// </summary>
        [ProtoMember(1)]
        public int Rank { get; set; }

        /// <summary>
        /// 购买次数
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuyNum { get; set; }

        /// <summary>
        /// 今日已使用次数
        /// </summary>
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue UseNum { get; set; }

        /// <summary>
        /// 挑战CD时间
        /// </summary>
        [ProtoMember(6)]
        public DateTime NextChallengeTime { get; set; }

        /// <summary>
        /// 下一次挑战任务id
        /// </summary>
        //[ProtoMember(7)]
        //public int NextChallengeTaskId { get; set; }

        /// <summary>
        /// 总共购买次数（统计使用）
        /// </summary>
        [ProtoMember(8)]
        public int BuyNums { get; set; }

        /// <summary>
        /// 是否有新战报记录
        /// </summary>
        [ProtoMember(10)]
        public int HasNewMsg { get; set; }

        /// <summary>
        /// 对战记录id列表
        /// </summary>
        [ProtoMember(11), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> BattleIdList { get; set; }

        /// <summary>
        /// 最高排名
        /// </summary>
        [ProtoMember(12)]
        public int HighestRank { get; set; }

        /// <summary>
        /// 星星数
        /// </summary>
        [ProtoMember(14), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue StarNum { get; set; }

        /// <summary>
        /// 今日购买次数【当天不变】
        /// </summary>
        //[ProtoMember(13), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        //public DayZeorValue TodayBuyNum { get; set; }

        /// <summary>
        /// 最多挑战次数
        /// </summary>
        public int MaxNum
        {
            get
            {
                return ConfigHelper.PkCfgData.MaxChallengeNum;
            }
        }

        /// <summary>
        /// 剩余挑战次数
        /// </summary>
        public int LaveNum
        {
            get
            {
                return (int)(ConfigHelper.PkCfgData.MaxChallengeNum - UseNum.Value);
            }
        }

        /// <summary>
        /// 下次挑战剩余时间
        /// </summary>
        public int LaveTime
        {
            get
            {
                return NextChallengeTime.ToTs();
            }
        }

        /// <summary>
        /// 添加使用挑战次数
        /// </summary>
        /// <returns></returns>
        public bool AddUseNum()
        {
            if (LaveNum > 0)
            {
                //if (ConfigHelper.PkCfgData.MaxChallengeNum <= UseNum.Value)
                //    BuyNum -= 1;
                //else

                UseNum += 1;
                NextChallengeTime = DateTime.Now.AddSeconds(ConfigHelper.PkCfgData.CoolTime);

                //添加竞技场任务次数
                Utility.AddDailyTaskGoalData(Id, DailyType.Pk);
            }
            else return false;
            return true;
        }

        /// <summary>
        /// 添加宝箱星星数量
        /// </summary>
        /// <returns></returns>
        public void AddStarNum()
        {
            var starNum = (int) StarNum.Value;
            if (starNum < 0) return;
            if (starNum >= ConfigHelper.PkCfgData.MaxStarNum) return;
            StarNum += 1;
        }

        /// <summary>
        /// 添加对战记录id
        /// </summary>
        /// <param name="battleId"></param>
        /// <param name="isDefender"></param>
        public void AddBattleId(int battleId, bool isDefender = false)
        {
            //记录大于最多保存条数，先删除
            if (BattleIdList.Count >= ConfigHelper.PkCfgData.MaxRecordNum)
            {
                var removeId = BattleIdList[0];

                //从redis和数据库中删除战报
                Utility.DelBattle(removeId);
                BattleIdList.RemoveAt(0);
            }
            BattleIdList.Add(battleId);
            if (isDefender)
            {
                HasNewMsg = 1;
                ChangeNewMsg();
            }
        }

        /// <summary>
        /// 改变竞技场新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var openCfgItem = ConfigHelper.OpenCfgsData.FirstOrDefault(o => o.Type == OpenModuleType.Pk);
            if (openCfgItem == null || userRole.Level >= openCfgItem.Level)
                userRole.SetHasNewMsg((int)NewMsgType.Pk, HasNewMsg > 0 ? 1 : 0);
        }

        /// <summary>
        /// 按照规律 随机获取对手
        /// </summary>
        /// <returns></returns>
        public List<PkUserItem> GetTargetList()
        {
            var logItem = new GameLogItem();
            logItem.S1 = "";
            logItem.S2 = "";
            logItem.S3 = "";
            logItem.S4 = "";
            logItem.S5 = "";

            //var serverPkBeAttacking = DataStorage.Current.Load<ServerPkBeAttacking>(Utility.ServerPkBeAttackingId, true);
            //var beAttackingRankList = serverPkBeAttacking.Items.Select(o => o.Rank).ToList();

            var list = new List<PkUserItem>();
            var startRange = 1;
            var targetList = new List<int>();
            var rangePreClist = new List<int>() { 30, 50, 80, 90 };
            if (Rank > 200) rangePreClist = ConfigHelper.PkCfgData.RangePrecsList[0];
            else if (Rank > 100) rangePreClist = ConfigHelper.PkCfgData.RangePrecsList[1];
            else if (Rank > 50) rangePreClist = ConfigHelper.PkCfgData.RangePrecsList[2];
            else if (Rank > 15) rangePreClist = ConfigHelper.PkCfgData.RangePrecsList[3];
            var targetRankList = new List<int>();
            var minRank = 15;
            if (Rank <= minRank)
            {
                for (var i = 1; i < (Rank * 3); i++)
                {
                    var r = Util.GetRandom(1, Rank);
                    var rank = Rank - r;

                    logItem.S1 += rank + ","; ;

                    if (rank > 0 && !targetRankList.Contains(rank)) targetRankList.Add(rank);
                    if (targetRankList.Count == 4) break;
                }
                if (targetRankList.Count < 4)
                {
                    for (var i = 1; i < 30; i++)
                    {
                        var r = Util.GetRandom(1, minRank - Rank);
                        var rank = Rank + r;

                        logItem.S2 += rank + ","; ;

                        if (!targetRankList.Contains(rank)) targetRankList.Add(rank);
                        if (targetRankList.Count == 4) break;
                    }
                }
                if (targetRankList.Count < 4)
                {
                    for (var i = 1; i < 20; i++)
                    {
                        var r = Util.GetRandom(1, minRank + 10);
                        var rank = minRank + r;

                        logItem.S2 += rank + ","; ;

                        if (!targetRankList.Contains(rank)) targetRankList.Add(rank);
                        if (targetRankList.Count == 4) break;
                    }
                }
            }
            logItem.S3 = JsonConvert.SerializeObject(targetRankList);

            var index = 0;
            foreach (var i in rangePreClist)
            {
                var endRange = (int)(Math.Floor(Rank * i * 1.0 / 100) < 1 ? 1 : Math.Floor(Rank * i * 1.0 / 100));
                if (endRange <= startRange) endRange = startRange + 2;
                var maxFindCount = (endRange - startRange) <= 0 ? 1 : endRange - startRange;
                maxFindCount = maxFindCount * 2;
                var findCount = 0;
                var findOut = false;
                while (findCount < maxFindCount && !findOut)
                {
                    var rangeRank = Util.GetRandom(startRange, endRange + 1);
                    var targetRank = Rank - rangeRank;
                    if (targetRank <= 0) targetRank = Rank + rangeRank;

                    if (Rank <= 15)
                    {
                        targetRank = targetRankList[index];
                    }

                    if (targetRank > 0 && !targetList.Contains(targetRank))
                    {

                        logItem.S4 += targetRank + ",";
                        var roleList = DataStorage.Current.SortedSets.Range<Utility.PkRankKey>(Utility.PkSetKey, targetRank,
                        targetRank, true, true, true, 0, 1);

                        //var serverPkBeAttackingItem =
                        //    serverPkBeAttacking.Items.FirstOrDefault(
                        //        o => o.Type == (int)roleList[0].Key.Type && o.UserId == roleList[0].Key.UserId);
                        if (roleList.Length > 0) // && serverPkBeAttackingItem == nullroleList[0].Key.AttackEndTimestamp < DateTime.Now.ToUnixTime())
                        {
                            logItem.S5 += roleList[0].Key.UserId + ",";

                            //找到对手
                            var type = roleList[0].Key.Type;
                            var userId = roleList[0].Key.UserId;

                            var targetUserItem = new PkUserItem();
                            targetUserItem.UserId = userId;
                            targetUserItem.BattleTargetType = (BattleTargetType)type;
                            targetUserItem.Rank = targetRank;

                            if (type == (int)BattleTargetType.Npc)
                            {
                                //对手是NPC
                                var targetPkNpc = Storage.Load<ServerPkNpc>(userId, true);

                                targetUserItem.HeadId = targetPkNpc.HeadId;
                                targetUserItem.Level = targetPkNpc.Level;
                                targetUserItem.NickName = targetPkNpc.NickName;
                                targetUserItem.Combat = targetPkNpc.DefendCombat;
                                targetUserItem.HeroList = targetPkNpc.FormationHeroItems;
                            }
                            else
                            {
                                //对手是玩家
                                UserFormation targetUserFormation;
                                UserRole targetUserRole;
                                UserHero userHero;
                                Storage.Load(out targetUserFormation, out targetUserRole, out userHero, userId, true);

                                targetUserItem.HeadId = targetUserRole.HeadId;
                                targetUserItem.Level = targetUserRole.Level;
                                targetUserItem.NickName = targetUserRole.NickName;
                                targetUserItem.Combat = targetUserFormation.DefCombat;

                                foreach (
                                    var formationItem in
                                        targetUserFormation.DefFormation.Where(o => o.HeroId > 0).ToList())
                                {
                                    var targetHeroItem = new FormationDetailItem();
                                    var heroItem = userHero.Items.Find(o => o.HeroId == formationItem.HeroId);

                                    targetHeroItem.HeroId = formationItem.HeroId;
                                    targetHeroItem.StarLevel = heroItem.StarLevel;
                                    targetHeroItem.Level = heroItem.Level;
                                    targetHeroItem.Location = formationItem.Location;

                                    //攻击速度，判断出手顺序 add by hql at 2015.11.19
                                    targetHeroItem.AttackSpeed = heroItem.AttackSpeed;

                                    //防守武将列表
                                    targetUserItem.HeroList.Add(targetHeroItem);
                                }
                            }
                            //添加目标用户
                            list.Add(targetUserItem);
                            targetList.Add(targetRank);
                            findOut = true;
                            index++;
                        }
                    }
                    findCount++;
                }

                startRange = endRange;
            }
            list = list.OrderByDescending(o => o.Rank).ToList();

            if (list.Count < 4)
            {
                GameLogManager.CommonLog((int)SpecialLogType.RefreshPkListLog, 0, 0, logItem);
            }
            return list;
        }

        public override void NewObjectInit()
        {
            Rank = Utility.GetAndSetPkRank(Id, Rank);
            HighestRank = Rank;
        }

        public override void LoadInit()
        {
            Rank = Utility.GetAndSetPkRank(Id, Rank);
            BattleIdList = BattleIdList ?? new List<int>();
            if (HighestRank == 0) HighestRank = Rank;
        }
    }

    /// <summary>
    /// 客户端竞技场需要的对手数据
    /// </summary>
    public class PkUserItem
    {
        public PkUserItem()
        {
            HeroList = new List<FormationDetailItem>();
            NickName = "";
        }
        /// <summary>
        /// 用户id
        /// </summary>
        [Tag(1)]
        public int UserId { get; set; }
        /// <summary>
        /// 用户类型【1：Npc，2：玩家】
        /// </summary>
        [Tag(2)]
        public BattleTargetType BattleTargetType { get; set; }
        /// <summary>
        /// 头像id
        /// </summary>
        [Tag(3)]
        public int HeadId { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(4)]
        public int Level { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(5)]
        public string NickName { get; set; }
        /// <summary>
        /// 排名
        /// </summary>
        [Tag(6)]
        public int Rank { get; set; }
        /// <summary>
        /// 战斗力
        /// </summary>
        [Tag(7)]
        public int Combat { get; set; }
        /// <summary>
        /// 防守阵型武将列表
        /// </summary>
        [Tag(8)]
        public List<FormationDetailItem> HeroList { get; set; }
    }

    /// <summary>
    /// 客户端大地图需要的对手数据
    /// </summary>
    public class BattleUserItem
    {
        public BattleUserItem()
        {
            HeroList = new List<FormationDetailItem>();
            NickName = "";
        }
        /// <summary>
        /// 系统城ID，Id为零则为领地复仇
        /// </summary>
        [Tag(1)]
        public int CityId { get; set; }
        /// <summary>
        /// 头像id
        /// </summary>
        [Tag(2)]
        public int HeadId { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(3)]
        public int Level { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(4)]
        public string NickName { get; set; }
        /// <summary>
        /// 战斗力
        /// </summary>
        [Tag(5)]
        public int Combat { get; set; }
        /// <summary>
        /// 防守阵型武将列表
        /// </summary>
        [Tag(6)]
        public List<FormationDetailItem> HeroList { get; set; }
    }
}
