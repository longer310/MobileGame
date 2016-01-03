using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi;
using MobileGame.tianzi.ConfigStruct;

namespace MobileGame.tianzi.Repository
{
    #region 武将出征相关信息
    /// <summary>
    /// 挑战相关信息
    /// </summary>
    public abstract class BattleHandler : GameHandler
    {
        /// <summary>
        /// 目标Id(竞技场：对手id，副本关卡：关卡id,大地图：系统城ID，大地图复仇：复仇战报Id，大地图新手引导战斗：2001）,暗黑军团活动：0
        /// </summary>
        public int TargetId { get; set; }
        /// <summary>
        /// 挑战的武将Id列表
        /// </summary>
        public string HeroIdArray { get; set; }
        /// <summary>
        /// 挑战的武将Id列表
        /// </summary>
        public List<int> HeroIdList;
        /// <summary>
        /// 武将位置Id列表
        /// </summary>
        public string LocationIdArray { get; set; }
        /// <summary>
        /// 武将位置Id列表
        /// </summary>
        public List<int> LocationIdList;

        public override bool InitParams(GameContext context)
        {
            if (HeroIdList.Count == 0 || LocationIdList.Count == 0 || HeroIdList.Count != LocationIdList.Count) return false;
            return true;
        }
    }
    #endregion

    #region 7000 获得竞技场界面信息
    /// <summary>
    /// 获得竞技场界面信息
    /// </summary>
    public class GetPkResponse
    {
        public GetPkResponse()
        {
            Items = new List<PkUserItem>();
        }
        /// <summary>
        /// 排名
        /// </summary>
        [Tag(1)]
        public int Rank { get; set; }

        /// <summary>
        /// 防守战斗力
        /// </summary>
        [Tag(2)]
        public int DefendCombat { get; set; }

        /// <summary>
        /// 剩余挑战次数
        /// </summary>
        [Tag(3)]
        public int LaveNum { get; set; }

        /// <summary>
        /// 最大挑战次数
        /// </summary>
        [Tag(4)]
        public int MaxNum { get; set; }

        /// <summary>
        /// 挑战冷却时间
        /// </summary>
        [Tag(5)]
        public int LaveTime { get; set; }

        /// <summary>
        /// 清除挑战冷却时间元宝
        /// </summary>
        [Tag(6)]
        public int ClearTimeMoney { get; set; }

        /// <summary>
        /// 今日购买的次数【根据该值和客户端配置文件得到购买所需元宝】
        /// </summary>
        [Tag(7)]
        public int BuyNum { get; set; }

        /// <summary>
        /// 是否有新战斗记录
        /// </summary>
        [Tag(9)]
        public int HaveNewMsg { get; set; }

        /// <summary>
        /// 星星数量，为负数则代表今日已领取过
        /// </summary>
        [Tag(10)]
        public int StarNum { get; set; }

        /// <summary>
        /// 目标列表
        /// </summary>
        [Tag(11)]
        public List<PkUserItem> Items { get; set; }
    }
    /// <summary>
    /// 获得竞技场界面信息
    /// </summary>
    [GameCode(OpCode = 7000, ResponseType = typeof(GetPkResponse))]
    public class GetPkRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetPkResponse();
            UserFormation userFormation;
            UserPk userPk;
            Storage.Load(out userFormation, out userPk, CurrentUserId, true);

            response.Rank = userPk.Rank;
            response.DefendCombat = userFormation.DefCombat;
            response.LaveNum = userPk.LaveNum;
            response.MaxNum = userPk.MaxNum;
            response.LaveTime = userPk.LaveTime;
            response.BuyNum = (int)userPk.BuyNum.Value;
            response.HaveNewMsg = userPk.HasNewMsg;
            response.StarNum = (int)userPk.StarNum.Value;

            response.ClearTimeMoney = ConfigHelper.ClearCdCfgData.Pk;//ConfigHelper.PkCfgData.ClearTimeMoney;

            //获取目标列表
            response.Items = userPk.GetTargetList();

            if (response.Items.Count == 0)
            {
                //再来一次
                response.Items = userPk.GetTargetList();
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 7001 购买竞技场次数
    /// <summary>
    /// 购买竞技场次数
    /// </summary>
    [GameCode(OpCode = 7001)]
    public class BuyPkNumRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserPk userPk;
            UserRole userRole;
            Storage.Load(out userRole, out userPk, CurrentUserId, true);

            if (userPk.LaveNum > 0)
            {
                //次数还有剩余，无需购买
                SetError(ResourceId.R_7000_HavePkNumCannotBuy);
                return;
            }

            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == userRole.VipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", userRole.VipLevel);
                return;
            }
            if (sysVipCfg.PkBuyNum <= userPk.BuyNum.Value)
            {
                //购买次数已用完
                SetError(ResourceId.R_0000_NoBuyNum);
                return;
            }
            var sysBuyNumCfg = SysBuyNumCfg.Find((int)userPk.BuyNum.Value + 1);
            if (sysBuyNumCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuyNumCfg", (int)userPk.BuyNum.Value + 1);
                return;
            }
            var needMoney = sysBuyNumCfg.BuyPkNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuyPkNum);

            //添加挑战次数
            userPk.BuyNum += 1;
            userPk.UseNum -= userPk.UseNum.Value;
            //userPk.TodayBuyNum += 1;
            userPk.BuyNums++;

            var logItem = new GameLogItem();
            logItem.F1 = 1;
            logItem.F2 = needMoney;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 7002 挑战竞技场对手
    /// <summary>
    /// 挑战需要下发的数据
    /// </summary>
    public class PkResponse
    {
        public PkResponse()
        {
            AttackerHeroItems = new List<BattleHeroItem>();
            DefenderHeroItems = new List<BattleHeroItem>();
        }
        /// <summary>
        /// 战役id
        /// </summary>
        [Tag(1)]
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方武将列表
        /// </summary>
        [Tag(2)]
        public List<BattleHeroItem> AttackerHeroItems { get; set; }

        /// <summary>
        /// 防守方武将列表
        /// </summary>
        [Tag(3)]
        public List<BattleHeroItem> DefenderHeroItems { get; set; }
    }
    /// <summary>
    /// 挑战对手
    /// </summary>
    [GameCode(OpCode = 7002, ResponseType = typeof(PkResponse))]
    public class PkRequest : BattleHandler
    {
        /// <summary>
        /// 战斗类型 不能传布阵类型
        /// </summary>
        public WarType WarType { get; set; }

        /// <summary>
        /// 可传参数一【竞技场：对手排名】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int Param1 { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (WarType != WarType.PkUser && WarType != WarType.PkNpc) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new PkResponse();

            //战役ID合服的时候 可以丢弃 不用全服统一ID
            var battleId = Util.GetSequence(typeof(AllBattle), 0);
            var battle = Storage.Load<AllBattle>(battleId, true);
            //var battle = KVEntity.CreateNew<AllBattle>();//TIP:需要保存的实体 使用load createnew适合实体中的列表项！！！
            battle.WarType = WarType;

            if (WarType == WarType.PkUser || WarType == WarType.PkNpc)
            {
                //攻击方武将详细信息
                UserHero userHero;
                UserRole userRole;
                UserPk userPk;
                UserFormation userFormation;
                Storage.Load(out userHero, out userRole, out userPk, out userFormation, CurrentUserId, true);
                var index = 0;
                userFormation.AttFormation.Clear();
                var newTotalCombat = 0;
                foreach (var i in HeroIdList)
                {
                    var userHeroItem = userHero.FindByHeroId(i);
                    if (userHeroItem == null)
                    {
                        SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", i);
                        return;
                    }
                    newTotalCombat += userHeroItem.Combat;

                    //战役中的武将信息ID合服的时候 可以丢弃 不用全服统一ID
                    int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                    var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                    item.LoadDataFromUserHeroItem(userHeroItem);
                    item.Location = (LocationNumber)LocationIdList[index];

                    battle.BattleAttackerHeroItemIdList.Add(battleHeroItemId);

                    response.AttackerHeroItems.Add(item);
                    index++;

                    //保存最新的阵容列表
                    userFormation.AttFormation.Add(new FormationItem() { HeroId = i, Location = item.Location });
                }

                if (newTotalCombat > userFormation.AttMaxCombat)
                {
                    userFormation.SetMaxComatAndFormation(newTotalCombat, userFormation.AttFormation);
                }

                battle.AttackerId = CurrentUserId;
                battle.AttackerLevel = userRole.Level;
                battle.AttackerHeadId = userRole.HeadId;
                battle.AttackerName = userRole.NickName;
                battle.Param1 = userPk.Rank;

                battle.DefenderId = TargetId;

                if (WarType == WarType.PkUser)
                {
                    //真实玩家防守阵型
                    UserPk targetUserPk;
                    UserHero targetUserHero;
                    UserRole targetUserRole;
                    UserFormation targetUserFormation;
                    Storage.Load(out targetUserFormation, out targetUserPk, out targetUserHero, out targetUserRole, TargetId, true);

                    foreach (var dItem in targetUserFormation.DefFormation)
                    {
                        var targetHeroItem = targetUserHero.FindByHeroId(dItem.HeroId);
                        if (targetHeroItem == null)
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", dItem.HeroId);
                            return;
                        }
                        int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                        var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                        item.LoadDataFromUserHeroItem(targetHeroItem);
                        item.Location = dItem.Location;

                        battle.BattleDefenderHeroItemIdList.Add(battleHeroItemId);

                        response.DefenderHeroItems.Add(item);
                    }

                    battle.DefenderLevel = targetUserRole.Level;
                    battle.DefenderHeadId = targetUserRole.HeadId;
                    battle.DefenderName = targetUserRole.NickName;
                    battle.Param2 = targetUserPk.Rank;
                }
                else
                {
                    //NPC防守阵型
                    var battleDefenderHeroItemIdList = new ListIntItem();
                    var targetPkNpc = Storage.Load<ServerPkNpc>(TargetId);
                    var npcHeroList = SysPkHeroCfg.Items.Where(o => o.NpcId == targetPkNpc.NpcId).ToList();
                    foreach (var sysPkHeroCfg in npcHeroList)
                    {
                        int battleHeroItemId = Util.GetSequence(typeof(BattleHeroItem), 0);
                        var item = Storage.Load<BattleHeroItem>(battleHeroItemId, true);
                        item.LoadDataFromSysNpcHeroCfg(sysPkHeroCfg);

                        battle.BattleDefenderHeroItemIdList.Add(battleHeroItemId);
                        battleDefenderHeroItemIdList.IdItems.Add(battleHeroItemId);

                        response.DefenderHeroItems.Add(item);
                    }
                    battle.DefenderHeroItemIdListList.Add(battleDefenderHeroItemIdList);

                    battle.DefenderLevel = targetPkNpc.Level;
                    battle.DefenderHeadId = targetPkNpc.HeadId;
                    battle.DefenderName = targetPkNpc.NickName;
                    battle.Param2 = targetPkNpc.Rank;
                }
            }

            //增加战斗技能光环效果
            Utility.AddBattleSkillRing(response.AttackerHeroItems, response.DefenderHeroItems);

            response.BattleId = battle.Id;
            ResultObj = response;
        }
    }
    #endregion

    #region 7003 换一批
    /// <summary>
    /// 换一批
    /// </summary>
    public class NextPkTargetResponse
    {
        public NextPkTargetResponse()
        {
            Items = new List<PkUserItem>();
        }
        /// <summary>
        /// 目标列表
        /// </summary>
        [Tag(1)]
        public List<PkUserItem> Items { get; set; }
    }
    /// <summary>
    /// 换一批
    /// </summary>
    [GameCode(OpCode = 7003, ResponseType = typeof(NextPkTargetResponse))]
    public class NextPkTargetRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new NextPkTargetResponse();
            var userPk = Storage.Load<UserPk>(CurrentUserId, true);

            //获取目标列表
            response.Items = userPk.GetTargetList();
            if (response.Items.Count == 0)
            {
                //再来一次
                response.Items = userPk.GetTargetList();
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 7004 提交竞技场挑战结果
    public class SubmitPkResultResponse
    {
        /// <summary>
        /// 历史最高排名
        /// </summary>
        [Tag(1)]
        public int HighestRank { get; set; }
        /// <summary>
        /// 当前排名
        /// </summary>
        [Tag(2)]
        public int Rank { get; set; }
        /// <summary>
        /// 历史最高排名提升奖励的元宝
        /// </summary>
        [Tag(3)]
        public int RewardMoney { get; set; }
        /// <summary>
        /// 获得的英雄经验
        /// </summary>
        [Tag(4)]
        public int HeroExp { get; set; }
    }
    /// <summary>
    /// 提交竞技场/副本挑战结果
    /// </summary>
    [GameCode(OpCode = 7004, ResponseType = typeof(SubmitPkResultResponse))]
    public class SubmitPkResultRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 攻击方是否胜利
        /// </summary>
        public int IsWin { get; set; }
        /// <summary>
        /// 战斗评分[0、1、2、3]
        /// </summary>
        public int BattleScore { get; set; }
        /// <summary>
        /// 回合字符串列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string RoundsStr { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0 || (IsWin != 0 && IsWin != 1)) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var logItem = new GameLogItem();
            var response = new SubmitPkResultResponse();

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "BattleId", BattleId);
                return;
            }
            if (battle.IsWin != -1)
            {
                SetError(ResourceId.R_0000_BattleHaveResult);
                return;
            }
            if (battle.AttackerId != CurrentUserId)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            if (battle.WarType != WarType.PkNpc && battle.WarType != WarType.PkUser)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            try
            {
                //保存回合过程
                var roundIdList = Utility.AddBattleRound(RoundsStr);
                if (roundIdList.Count > 0)
                {
                    battle.RoundIdList = roundIdList[0].IdItems;
                    battle.RoundIdListList = roundIdList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            battle.IsWin = IsWin;
            battle.BattleScore = BattleScore;

            #region 竞技场
            var userPk = Storage.Load<UserPk>(battle.AttackerId, true);
            userPk.AddBattleId(battle.Id);

            UserPk targetUserPk = null;
            ServerPkNpc targetPkNpc = null;
            if (battle.WarType == WarType.PkUser)
            {
                targetUserPk = Storage.Load<UserPk>(battle.DefenderId, true);
            }
            else
            {
                targetPkNpc = Storage.Load<ServerPkNpc>(battle.DefenderId, true);
            }
            if (IsWin == 1)
            {
                //战斗胜利
                userPk.AddStarNum();

                //以提交一刻的排名为准！
                battle.Param1 = userPk.Rank;
                if (battle.WarType == WarType.PkUser)
                {
                    if (targetUserPk == null)
                        throw new ApplicationException(string.Format("pk not find UserPk:{0}", battle.DefenderId));
                    battle.Param2 = targetUserPk.Rank;
                }
                else
                {
                    if (targetPkNpc == null)
                        throw new ApplicationException(string.Format("pk not find ServerPkNpc:{0}", battle.DefenderId));
                    battle.Param2 = targetPkNpc.Rank;
                }

                if (battle.Param1 > battle.Param2 && battle.Param2 < userPk.HighestRank)
                {
                    //攻击方排名大于防守方并且相比历史最高排名相比排名有提升
                    var difRank = userPk.HighestRank - battle.Param2;
                    var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= userPk.Rank && o.EndRank >= userPk.Rank);
                    if (sysPkCfg == null) sysPkCfg = SysPkCfg.Items.OrderByDescending(o => o.StarRank).FirstOrDefault();
                    if (sysPkCfg != null)
                    {
                        var rewardMoney = (int)(sysPkCfg.UpOneRewardMoney * difRank);
                        if (rewardMoney == 0) rewardMoney = 1;
                        response.RewardMoney = rewardMoney;

                        //竞技场最高排名奖励——发附件邮件
                        var attach = new List<ItemPair>();
                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Money, Num = rewardMoney });
                        var msg = LangResource.GetLangResource(ResourceId.R_HighestRankMsg,
                                                       battle.Param2, difRank);
                        Utility.SendGmMailToTarget(CurrentUserId, "竞技场最高排名奖励", msg, attach);
                    }

                    response.HighestRank = userPk.HighestRank;
                    userPk.HighestRank = battle.Param2;
                }
                //打排名靠前的人
                if (battle.Param1 > battle.Param2)
                {
                    userPk.Rank = battle.Param2;
                    logItem.F3 = userPk.Rank;
                    Utility.SetPkRank(CurrentUserId, userPk.Rank, BattleTargetType.User);
                }
                response.Rank = userPk.Rank;

                //添加武将经验
                var userHero = Storage.Load<UserHero>(battle.AttackerId, true);
                var heroList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
                var heroExp = battle.DefenderLevel * ConfigHelper.PkCfgData.WinHeroExpModulus;
                foreach (var i in heroList)
                {
                    var heroItem = userHero.FindByHeroId(i.HeroId);
                    heroItem.AddExp(heroExp, Request.OpCode);
                }
                response.HeroExp = heroExp;
            }

            if (battle.WarType == WarType.PkUser)
            {
                if (targetUserPk == null)
                    throw new ApplicationException(string.Format("pk not find UserPk:{0}", battle.DefenderId));
                //var defendUserPk = Storage.Load<UserPk>(battle.DefenderId, true);
                targetUserPk.AddBattleId(battle.Id, true);
                if (IsWin == 1 && battle.Param1 > battle.Param2)
                {
                    targetUserPk.Rank = battle.Param1;
                    logItem.F4 = targetUserPk.Rank;
                    Utility.SetPkRank(battle.DefenderId, targetUserPk.Rank, BattleTargetType.User);
                }
            }
            else
            {
                //var defendPkNpc = Storage.Load<ServerPkNpc>(battle.DefenderId, true);
                if (IsWin == 1 && battle.Param1 > battle.Param2)
                {
                    if (targetPkNpc == null)
                        throw new ApplicationException(string.Format("pk not find ServerPkNpc:{0}", battle.DefenderId));
                    targetPkNpc.Rank = battle.Param1;
                    logItem.F4 = targetPkNpc.Rank;
                    Utility.SetPkRank(battle.DefenderId, targetPkNpc.Rank, BattleTargetType.Npc);
                }
            }
            #endregion

            logItem.F1 = BattleId;
            logItem.F2 = IsWin;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 7005 获取竞技场对战记录

    public class GetPkRecordResponse
    {
        public GetPkRecordResponse()
        {
            Items = new List<PkRecordItem>();
        }
        /// <summary>
        /// 对战记录列表 
        /// </summary>
        [Tag(1)]
        public List<PkRecordItem> Items { get; set; }
        public class PkRecordItem
        {
            /// <summary>
            /// 战役id
            /// </summary>
            [Tag(1)]
            public int BattleId { get; set; }
            /// <summary>
            /// 当前玩家是否胜利
            /// </summary>
            [Tag(2)]
            public int IsWin { get; set; }
            /// <summary>
            /// 正整数，为零则代表没变化【根据IsWin判断是输是赢】
            /// </summary>
            [Tag(3)]
            public int ChangeRank { get; set; }
            /// <summary>
            /// 敌方等级
            /// </summary>
            [Tag(5)]
            public int EnemyLevel { get; set; }
            /// <summary>
            /// 敌方头像id
            /// </summary>
            [Tag(6)]
            public int EnemyHeadId { get; set; }
            /// <summary>
            /// 敌方昵称
            /// </summary>
            [Tag(7)]
            public string EnemyName { get; set; }
            /// <summary>
            /// 创建时间
            /// </summary>
            [Tag(8)]
            public DateTime CreateTime { get; set; }
            /// <summary>
            /// 战报是否可以回放
            /// </summary>
            [Tag(9)]
            public int IsCanPalyback { get; set; }

            public PkRecordItem(AllBattle battle, int curUserId)
            {
                IsCanPalyback = 1;
                BattleId = battle.Id;
                IsWin = curUserId == battle.AttackerId ? battle.IsWin : (battle.IsWin == 1 ? 0 : 1);
                //攻击方赢了
                if (battle.IsWin == 1 && battle.Param1 > battle.Param2)
                {
                    ChangeRank = battle.Param1 - battle.Param2;
                    //if (battle.AttackerId == curUserId)
                    //{
                    //    //主动攻击 排名靠后打靠前的
                    //    if (battle.Param1 > battle.Param2)
                    //        ChangeRank = battle.Param1 - battle.Param2;
                    //}
                    //else
                    //{
                    //    //防守 排名靠后打靠前的
                    //    if (battle.Param1 > battle.Param2)
                    //        ChangeRank = Math.Abs(battle.Param1 - battle.Param2);
                    //}
                }
                CreateTime = battle.CreateTime;
                if (curUserId == battle.AttackerId)
                {
                    EnemyLevel = battle.DefenderLevel;
                    EnemyHeadId = battle.DefenderHeadId;
                    EnemyName = battle.DefenderName;
                }
                else
                {
                    EnemyLevel = battle.AttackerLevel;
                    EnemyHeadId = battle.AttackerHeadId;
                    EnemyName = battle.AttackerName;
                }
                //不存在则表示被删除了记录，此战报不可回放了。
                if (battle.BattleAttackerHeroItemIdList == null || battle.BattleAttackerHeroItemIdList.Count == 0)
                {
                    IsCanPalyback = 0;
                }
                else
                {
                    var heroId = battle.BattleAttackerHeroItemIdList[0];
                    var battleAttackerHeroItem = DataStorage.Current.Load<BattleHeroItem>(heroId);
                    if (battleAttackerHeroItem.IsNew) IsCanPalyback = 0;
                }
            }
        }
    }
    /// <summary>
    /// 获取竞技场对战记录
    /// </summary>
    [GameCode(OpCode = 7005, ResponseType = typeof(GetPkRecordResponse))]
    public class GetPkRecordRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetPkRecordResponse();
            UserPk userPk;
            UserRole userRole;
            Storage.Load(out userPk, out userRole, CurrentUserId, true);
            var battleList = Storage.LoadList<AllBattle>(userPk.BattleIdList.ToArray());
            for (int i = userPk.BattleIdList.Count - 1; i >= 0; i--)
            {
                var battleId = userPk.BattleIdList[i];
                var battle = battleList.FirstOrDefault(o => o.Id == battleId);
                if (battle != null)
                {
                    if (!battle.IsNew)
                    {
                        var item = new GetPkRecordResponse.PkRecordItem(battle, CurrentUserId);
                        response.Items.Add(item);
                    }
                    else
                    {
                        userPk.BattleIdList.Remove(battleId);
                    }
                }
            }
            userPk.HasNewMsg = 0;
            userPk.ChangeNewMsg(userRole);

            ResultObj = response;
        }
    }
    #endregion

    #region 7006 获取竞技场排行榜信息
    public class GetPkRankListResponse
    {
        public GetPkRankListResponse()
        {
            Items = new List<RankUserItem>();
        }
        /// <summary>
        /// 排名列表信息
        /// </summary>
        [Tag(1)]
        public List<RankUserItem> Items { get; set; }/// <summary>
        /// 排名
        /// </summary>
        [Tag(2)]
        public int Rank { get; set; }

        /// <summary>
        /// 防守战斗力
        /// </summary>
        [Tag(3)]
        public int DefendCombat { get; set; }
        public class RankUserItem
        {
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
        }
    }
    /// <summary>
    ///获取竞技场排行榜信息
    /// </summary>
    [GameCode(OpCode = 7006, ResponseType = typeof(GetPkRankListResponse))]
    public class GetPkRankListRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetPkRankListResponse();
            UserPk userPk;
            UserFormation userFormation;
            Storage.Load(out userFormation, out userPk, CurrentUserId, true);
            response.Rank = userPk.Rank;
            response.DefendCombat = userFormation.DefCombat;

            //获取列表中的前几条数据:升序（排名从小到大）
            KeyValuePair<Utility.PkRankKey, double>[] rankList =
                Storage.SortedSets.Range<Utility.PkRankKey>(Utility.PkSetKey, 1.0, int.MaxValue, true, true, true, 0,
                    ConfigHelper.PkCfgData.GetMaxRankNum);

            foreach (var keyValuePair in rankList)
            {
                var item = new GetPkRankListResponse.RankUserItem();
                item.BattleTargetType = (BattleTargetType)keyValuePair.Key.Type;
                item.UserId = keyValuePair.Key.UserId;
                item.Rank = (int)keyValuePair.Value;

                response.Items.Add(item);
            }

            //排行榜中的用户
            var userIdList = response.Items.Where(o => o.BattleTargetType == BattleTargetType.User).Select(o => o.UserId).ToList();
            if (userIdList.Count > 0)
            {
                var userRoleList = Storage.LoadList<UserRole>(userIdList.ToArray());
                foreach (var userRole in userRoleList)
                {
                    var item = response.Items.FirstOrDefault(o => o.UserId == userRole.Id && o.BattleTargetType == BattleTargetType.User);
                    if (item != null)
                    {
                        item.Level = userRole.Level;
                        item.HeadId = userRole.HeadId;
                        item.NickName = userRole.NickName;
                    }
                }
            }

            //排行榜中的NPC
            var npcIdList = response.Items.Where(o => o.BattleTargetType == BattleTargetType.Npc).Select(o => o.UserId).ToList();
            if (npcIdList.Count > 0)
            {
                var pkNpcList = Storage.LoadList<ServerPkNpc>(npcIdList.ToArray(), true);
                foreach (var pkNpc in pkNpcList)
                {
                    var item = response.Items.FirstOrDefault(o => o.UserId == pkNpc.Id && o.BattleTargetType == BattleTargetType.Npc);
                    if (item != null)
                    {
                        item.Level = pkNpc.Level;
                        item.HeadId = pkNpc.HeadId;
                        item.NickName = pkNpc.NickName;
                    }
                }
            }
            response.Items = response.Items.OrderBy(o => o.Rank).ToList();
            ResultObj = response;
        }
    }
    #endregion

    #region 7007 获取排行榜玩家详细信息
    /// <summary>
    /// 获取排行榜玩家详细信息
    /// </summary>
    [GameCode(OpCode = 7007, ResponseType = typeof(PkUserItem))]
    public class PkRankUserDetailRequest : GameHandler
    {
        /// <summary>
        /// 目标id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 竞技场目标类型
        /// </summary>
        public BattleTargetType PkTargetType { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (UserId == 0 || (PkTargetType != BattleTargetType.Npc && PkTargetType != BattleTargetType.User)) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new PkUserItem();

            response.UserId = UserId;
            response.BattleTargetType = PkTargetType;

            if (PkTargetType == BattleTargetType.Npc)
            {
                //对手是NPC
                var targetPkNpc = Storage.Load<ServerPkNpc>(UserId, true);

                response.HeadId = targetPkNpc.HeadId;
                response.Level = targetPkNpc.Level;
                response.NickName = targetPkNpc.NickName;
                response.Combat = targetPkNpc.DefendCombat;
                response.HeroList = targetPkNpc.FormationHeroItems;
                response.Rank = targetPkNpc.Rank;
            }
            else
            {
                //对手是玩家
                UserPk targetUserPk;
                UserRole targetUserRole;
                UserHero userHero;
                UserFormation targetUserFormation;
                Storage.Load(out targetUserPk, out targetUserRole, out userHero, out targetUserFormation, UserId, true);

                response.HeadId = targetUserRole.HeadId;
                response.Level = targetUserRole.Level;
                response.NickName = targetUserRole.NickName;
                response.Combat = targetUserFormation.DefCombat;
                response.Rank = targetUserPk.Rank;

                foreach (var formationItem in targetUserFormation.DefFormation.Where(o => o.HeroId > 0).ToList())
                {
                    var targetHeroItem = new FormationDetailItem();
                    var heroItem = userHero.Items.Find(o => o.HeroId == formationItem.HeroId);

                    targetHeroItem.HeroId = formationItem.HeroId;
                    targetHeroItem.StarLevel = heroItem.StarLevel;
                    targetHeroItem.Level = heroItem.Level;
                    targetHeroItem.Location = formationItem.Location;

                    //防守武将列表
                    response.HeroList.Add(targetHeroItem);
                }
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 7008 领取宝箱奖励
    public class GetBoxRewardResponse
    {
        public GetBoxRewardResponse()
        {
            ItemPairList = new List<ItemPair>();
        }
        /// <summary>
        /// 道具(12000001:元宝、12000002:铜钱等)列表
        /// </summary>
        [Tag(1)]
        public List<ItemPair> ItemPairList { get; set; }
    }
    /// <summary>
    /// 领取宝箱奖励
    /// </summary>
    [GameCode(OpCode = 7008, ResponseType = typeof(GetBoxRewardResponse))]
    public class GetBoxRewardRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserPk userPk;
            UserRole userRole;
            UserTool userTool;
            Storage.Load(out userPk, out userRole, out userTool, CurrentUserId, true);
            if ((int)userPk.StarNum.Value < ConfigHelper.PkCfgData.MaxStarNum)
            {
                SetError(ResourceId.R_7008_StarNumNotEnough);
                return;
            }

            //var sysPkBoxCfg =
            //    SysPkBoxCfg.Items.FirstOrDefault(o => o.StarRank <= userPk.Rank && o.EndRank >= userPk.Rank);
            //if (sysPkBoxCfg == null)
            //{
            //    SetError(ResourceId.R_0000_IdNotExist, "SysPkBoxCfg", userPk.Rank);
            //    return;
            //}

            var pkBoxCfgData = ConfigHelper.PkBoxCfgData;
            var mnum = Util.GetRandom(1, pkBoxCfgData.Mnum + 1);
            var cnum = Util.GetRandom(1, pkBoxCfgData.Cnum + 1);
            var tnum = Util.GetRandom(1, pkBoxCfgData.Tnum + 1);
            //添加进阶石
            var sysToolCfg = SysToolCfg.Find(pkBoxCfgData.ToolId);
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", pkBoxCfgData.ToolId);
                return;
            }

            //添加道具
            sysToolCfg.AddToUser(CurrentUserId, Request.OpCode, tnum);
            //添加元宝
            var getMoney = pkBoxCfgData.Money * mnum;
            RoleManager.AddGiveMoney(userRole, getMoney);
            //添加铜钱
            var getCoin = pkBoxCfgData.Coin * cnum;
            Utility.AddResource(userRole, ItemType.Coin, Request.OpCode, getCoin);
            //设置为今日已领取过[-1]
            userPk.StarNum -= userPk.StarNum.Value + 1;

            var response = new GetBoxRewardResponse();
            response.ItemPairList.Add(new ItemPair()
            {
                ItemId = (int)SpecialToolId.Coin,
                Num = getCoin
            });
            response.ItemPairList.Add(new ItemPair()
            {
                ItemId = (int)SpecialToolId.Money,
                Num = getMoney
            });
            response.ItemPairList.Add(new ItemPair()
            {
                ItemId = pkBoxCfgData.ToolId,
                Num = tnum
            });

            var logItem = new GameLogItem();
            logItem.F1 = getMoney;
            logItem.F2 = getCoin;
            logItem.F3 = sysToolCfg.Id;
            logItem.F4 = tnum;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 7009 PVP【竞技场/大地图抢玩家城池领地】战报回放
    /// <summary>
    /// PVP【竞技场/大地图抢玩家城池领地】战报回放
    /// </summary>
    public class PkBackResponse
    {
        public PkBackResponse()
        {
            AttackerHeroItems = new List<ResBattleHeroItem>();
            DefenderHeroItems = new List<ResBattleHeroItem>();
        }
        /// <summary>
        /// 战役信息
        /// </summary>
        [Tag(1)]
        public AllBattle AllBattle { get; set; }
        /// <summary>
        /// 攻击方武将列表
        /// </summary>
        [Tag(2)]
        public List<ResBattleHeroItem> AttackerHeroItems { get; set; }
        /// <summary>
        /// 防守方武将列表
        /// </summary>
        [Tag(3)]
        public List<ResBattleHeroItem> DefenderHeroItems { get; set; }

        /// <summary>
        /// 战斗的回合列表
        /// </summary>
        [Tag(4)]
        public List<BattleRound> BattleRounds { get; set; }
    }
    /// <summary>
    /// 战报回放
    /// </summary>
    [GameCode(OpCode = 7009, ResponseType = typeof(PkBackResponse))]
    public class PkBackRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new PkBackResponse();

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle", BattleId);
                return;
            }

            response.AllBattle = battle;

            var battleAttackerHeroItemList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
            foreach (var battleHeroItem in battleAttackerHeroItemList)
            {
                var resBattleHeroItem = new ResBattleHeroItem();
                resBattleHeroItem.LoadDataFromBattleHeroItem(battleHeroItem);
                response.AttackerHeroItems.Add(resBattleHeroItem);
            }
            var battleDefenderHeroItemList = Storage.LoadList<BattleHeroItem>(battle.BattleDefenderHeroItemIdList.ToArray());
            foreach (var battleHeroItem in battleDefenderHeroItemList)
            {
                var resBattleHeroItem = new ResBattleHeroItem();
                resBattleHeroItem.LoadDataFromBattleHeroItem(battleHeroItem);
                response.DefenderHeroItems.Add(resBattleHeroItem);
            }

            response.BattleRounds = Storage.LoadList<BattleRound>(battle.RoundIdList.ToArray());

            ResultObj = response;
        }
    }
    #endregion

    #region 7010 进入排兵布阵界面（扣除PK次数，PK进入冷却时间）
    /// <summary>
    /// 进入排兵布阵界面（扣除PK次数，PK进入冷却时间）
    /// </summary>
    [GameCode(OpCode = 7010)]
    public class EnterPkFormationRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            //看到阵型则扣除一次竞技场次数
            var userPk = Storage.Load<UserPk>(CurrentUserId, true);
            if (userPk.LaveNum <= 0)
            {
                SetError(ResourceId.R_0000_BattleNumNotEnough);
                return;
            }
            if (userPk.LaveTime > 0)
            {
                //需先清除冷却时间
                SetError(ResourceId.R_7000_HaveCoolTimeCannotBuy);
                return;
            }
            userPk.AddUseNum();
        }
    }
    #endregion

    #region 7011 战报回放【PK/大地图PVP/大地图PV'E'】
    /// <summary>
    /// PVP【竞技场/大地图抢玩家城池领地】战报回放
    /// </summary>
    public class BattleBackResponse
    {
        public BattleBackResponse()
        {
            AttackerHeroItems = new List<ResBattleHeroItem>();
            DefenderHeroItemsList = new List<ResBattleHeroListItem>();
            FormationRoundItems = new List<FormationRoundItem>();
        }
        /// <summary>
        /// 战役信息
        /// </summary>
        [Tag(1)]
        public AllBattle AllBattle { get; set; }
        /// <summary>
        /// 攻击方武将列表
        /// </summary>
        [Tag(2)]
        public List<ResBattleHeroItem> AttackerHeroItems { get; set; }
        /// <summary>
        /// 防守方阵型武将列表【多个阵型】
        /// </summary>
        [Tag(3)]
        public List<ResBattleHeroListItem> DefenderHeroItemsList { get; set; }

        /// <summary>
        /// 阵型回合列表【多个阵型】
        /// </summary>
        [Tag(4)]
        public List<FormationRoundItem> FormationRoundItems { get; set; }
    }

    public class ResBattleHeroListItem
    {
        public ResBattleHeroListItem()
        {
            HeroItems = new List<ResBattleHeroItem>();
        }
        /// <summary>
        /// 防守方武将列表
        /// </summary>
        [Tag(1)]
        public List<ResBattleHeroItem> HeroItems { get; set; }
    }

    public class FormationRoundItem
    {
        public FormationRoundItem()
        {
            RoundItems = new List<BattleRound>();
        }
        /// <summary>
        /// 战斗回合
        /// </summary>
        [Tag(1)]
        public List<BattleRound> RoundItems { get; set; }
    }
    /// <summary>
    /// 战报回放【PK/大地图PVP/大地图PV'E'】
    /// </summary>
    [GameCode(OpCode = 7011, ResponseType = typeof(BattleBackResponse))]
    public class BattleBackRequest : GameHandler
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new BattleBackResponse();

            var battle = Storage.Load<AllBattle>(BattleId, true);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle", BattleId);
                return;
            }

            response.AllBattle = battle;

            var isError = 0;
            var battleAttackerHeroItemList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
            isError = battleAttackerHeroItemList.Count > 0 ? 0 : 1;
            foreach (var battleHeroItem in battleAttackerHeroItemList)
            {
                var resBattleHeroItem = new ResBattleHeroItem();
                resBattleHeroItem.LoadDataFromBattleHeroItem(battleHeroItem);
                response.AttackerHeroItems.Add(resBattleHeroItem);
            }
            battle.DefenderHeroItemIdListList = battle.DefenderHeroItemIdListList ?? new List<ListIntItem>();
            if (battle.DefenderHeroItemIdListList.Count == 0 && battle.BattleDefenderHeroItemIdList.Count > 0)
            {
                var item = new ListIntItem();
                foreach (var i in battle.BattleDefenderHeroItemIdList)
                {
                    item.IdItems.Add(i);
                }
                battle.DefenderHeroItemIdListList.Add(item);
            }
            isError = battle.DefenderHeroItemIdListList.Count > 0 ? 0 : 1;
            foreach (var battleDefenderHeroItemIdList in battle.DefenderHeroItemIdListList)
            {
                var battleDefenderHeroItemList = Storage.LoadList<BattleHeroItem>(battleDefenderHeroItemIdList.IdItems.ToArray());
                var defenderFormationHeroItem = new ResBattleHeroListItem();
                isError = battle.DefenderHeroItemIdListList.Count > 0 ? 0 : 1;
                foreach (var battleHeroItem in battleDefenderHeroItemList)
                {
                    var resBattleHeroItem = new ResBattleHeroItem();
                    resBattleHeroItem.LoadDataFromBattleHeroItem(battleHeroItem);
                    defenderFormationHeroItem.HeroItems.Add(resBattleHeroItem);
                }
                response.DefenderHeroItemsList.Add(defenderFormationHeroItem);
            }

            battle.RoundIdListList = battle.RoundIdListList ?? new List<ListIntItem>();
            isError = battle.RoundIdListList.Count > 0 ? 0 : 1;
            foreach (var roundIdList in battle.RoundIdListList)
            {
                var items = Storage.LoadList<BattleRound>(roundIdList.IdItems.ToArray());
                response.FormationRoundItems.Add(new FormationRoundItem() { RoundItems = items });
            }

            //if (isError == 1)
            //{
            //    //战报数据异常无法回放
            //    SetError(ResourceId.R_0000_BattleDataError);
            //    return;
            //}

            ResultObj = response;
        }
    }
    #endregion

    #region 7012 分享战报
    /// <summary>
    /// 分享战报
    /// </summary>
    [GameCode(OpCode = 7012)]
    public class ShareBattleRequest : GameHandler
    {
        /// <summary>
        /// 战报Id
        /// </summary>
        public int BattleId { get; set; }
        /// <summary>
        /// 留言
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string Msg { get; set; }

        /// <summary>
        /// 发送到哪里，世界还是联盟啥的，可不传默认世界
        /// </summary>
        [ParamCheck(Ignore = true)]
        public ChatType ChatType { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var battle = Storage.Load<AllBattle>(BattleId);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle", BattleId);
                return;
            }
            if (battle.IsWin == -1)
            {
                //战斗未结束，不能分享
                SetError(ResourceId.R_0000_BattleNotEnd);
                return;
            }
            if (battle.WarType == WarType.Levels)
            {
                //该类型战报不可分享
                SetError(ResourceId.R_0000_BattleCanNotShare);
                return;
            }
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var shareBattleIntervalTime = ConfigHelper.ShareBattleIntervalTime;
            var nextCanShareTime = userRole.NextCanShareTime;
            if (nextCanShareTime > DateTime.Now)
            {
                var difTime = nextCanShareTime.Subtract(DateTime.Now);
                SetError(ResourceId.R_0000_ShareCooling, difTime.Minutes, difTime.Seconds);
                return;
            }

            var title = "[" + battle.AttackerName + " VS " + battle.DefenderName + "]";
            if (battle.WarType == WarType.PkUser || battle.WarType == WarType.PkNpc)
            {
                title = title + "(" + LangResource.GetLangResource(ResourceId.R_0000_PkName) + ")";
            }
            else
            {
                title = title + "(" + LangResource.GetLangResource(ResourceId.R_0000_BigMapName) + ")";
            }

            if (string.IsNullOrEmpty(Msg))
            {
                Msg = "我分享了有趣的战报，快来围观吧";
            }
            var msg = LangResource.GetLangResource(ResourceId.R_0000_ShareBattleMsg, BattleId, title, Msg);
            if (!string.IsNullOrEmpty(msg))// GameApplication.Instance.Broadcast(msg);
            {
                if (ChatType == ChatType.World)
                {
                    ChatManager.SendChatToWorld(CurrentUserId, msg);
                }
            }

            nextCanShareTime = DateTime.Now.AddSeconds(shareBattleIntervalTime);
            userRole.NextCanShareTime = nextCanShareTime;
        }
    }
    #endregion

    #region 7013 点击战报分享的title弹框请求

    public class GetBattleMsgResponse
    {
        /// <summary>
        /// 攻击方
        /// </summary>
        [Tag(1)]
        public ResUserItem Attacker { get; set; }
        /// <summary>
        /// 防守方
        /// </summary>
        [Tag(2)]
        public ResUserItem Defender { get; set; }

        /// <summary>
        /// 攻击方是否赢
        /// </summary>
        [Tag(3)]
        public int IsWin { get; set; }

        /// <summary>
        /// 客户端竞技场需要的对手数据
        /// </summary>
        public class ResUserItem
        {
            public ResUserItem()
            {
                HeroList = new List<ResHeronItem>();
                NickName = "";
            }
            /// <summary>
            /// 用户id
            /// </summary>
            [Tag(1)]
            public int UserId { get; set; }
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
            /// 武将列表
            /// </summary>
            [Tag(6)]
            public List<ResHeronItem> HeroList { get; set; }

            public void InitData(int userId, int headId, int level, string nickName, List<BattleHeroItem> heroList)
            {
                UserId = userId;
                HeadId = headId;
                Level = level;
                NickName = nickName;

                Combat = 0;
                foreach (var battleHeroItem in heroList)
                {
                    var item = new ResHeronItem();
                    item.HeroId = battleHeroItem.HeroId;
                    item.Level = battleHeroItem.Level;
                    item.StarLevel = battleHeroItem.StarLevel;

                    HeroList.Add(item);
                    Combat += battleHeroItem.Combat;
                }
            }
        }
        public class ResHeronItem
        {
            /// <summary>
            /// 系统武将id
            /// </summary>
            [Tag(1)]
            public int HeroId { get; set; }
            /// <summary>
            /// 星级
            /// </summary>
            [Tag(2)]
            public int StarLevel { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
        }
    }
    /// <summary>
    /// 点击战报分享的title弹框请求
    /// </summary>
    [GameCode(OpCode = 7013, ResponseType = typeof(GetBattleMsgResponse))]
    public class GetBattleMsgRequest : GameHandler
    {
        /// <summary>
        /// 战报Id
        /// </summary>
        public int BattleId { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (BattleId == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new GetBattleMsgResponse();

            var battle = Storage.Load<AllBattle>(BattleId);
            if (battle.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "AllBattle", BattleId);
                return;
            }
            if (battle.IsWin == -1)
            {
                //战斗未结束，不能分享
                SetError(ResourceId.R_0000_BattleNotEnd);
                return;
            }
            if (battle.WarType == WarType.Levels)
            {
                //该类型战报不可分享
                SetError(ResourceId.R_0000_BattleCanNotShare);
                return;
            }

            response.IsWin = battle.IsWin;

            //攻击方
            var battleAttackerHeroItemList = Storage.LoadList<BattleHeroItem>(battle.BattleAttackerHeroItemIdList.ToArray());
            var attacker = new GetBattleMsgResponse.ResUserItem();
            attacker.InitData(battle.AttackerId, battle.AttackerHeadId, battle.AttackerLevel, battle.AttackerName,
                battleAttackerHeroItemList);
            response.Attacker = attacker;

            //防守方
            var battleDefenderHeroItemIdList = new List<int>();
            if (battle.BattleDefenderHeroItemIdList != null && battle.BattleDefenderHeroItemIdList.Count > 0)
                battleDefenderHeroItemIdList = battle.BattleDefenderHeroItemIdList;
            else if (battle.DefenderHeroItemIdListList != null && battle.DefenderHeroItemIdListList.Count > 0)
            {
                battleDefenderHeroItemIdList = battle.DefenderHeroItemIdListList[0].IdItems;
            }
            var battleDefenderHeroItemList = Storage.LoadList<BattleHeroItem>(battleDefenderHeroItemIdList.ToArray());
            var defender = new GetBattleMsgResponse.ResUserItem();
            defender.InitData(battle.DefenderId, battle.DefenderHeadId, battle.DefenderLevel, battle.DefenderName,
                battleDefenderHeroItemList);
            response.Defender = defender;

            ResultObj = response;
        }
    }
    #endregion

    #region 7014 获取最强战力、魅力、声望排行榜信息
    public class GetRankListResponse
    {
        public GetRankListResponse()
        {
            Items = new List<RankUserItem>();
        }
        /// <summary>
        /// 排名列表信息
        /// </summary>
        [Tag(1)]
        public List<RankUserItem> Items { get; set; }
        /// <summary>
        /// 排名（小于等于零说明1000名以后）
        /// </summary>
        [Tag(2)]
        public int Rank { get; set; }

        /// <summary>
        /// 自己的值（最强战力、声望、魅力）
        /// </summary>
        [Tag(3)]
        public int Value { get; set; }
        public class RankUserItem
        {
            /// <summary>
            /// 用户id
            /// </summary>
            [Tag(1)]
            public int UserId { get; set; }
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
            /// 值（最强战力、声望、魅力）
            /// </summary>
            [Tag(6)]
            public int Value { get; set; }
            /// <summary>
            /// 第几名
            /// </summary>
            [Tag(7)]
            public int Rank { get; set; }
        }
    }

    public enum RankType
    {
        /// <summary>
        /// 最强战力
        /// </summary>
        Combat = 1,
        /// <summary>
        /// 声望
        /// </summary>
        Repute = 2,
        /// <summary>
        /// 魅力
        /// </summary>
        Charm = 3,
    }
    /// <summary>
    ///获取最强战力、魅力、声望排行榜信息
    /// </summary>
    [GameCode(OpCode = 7014, ResponseType = typeof(GetRankListResponse))]
    public class GetRankListRequest : GameHandler
    {
        /// <summary>
        /// 排行类型
        /// </summary>
        public RankType RankType { get; set; }
        public override void Process(GameContext context)
        {
            var response = new GetRankListResponse();

            if (RankType == RankType.Combat)
            {
                var userFormation = Storage.Load<UserFormation>(CurrentUserId);
                response.Value = userFormation.AttMaxCombat;
            }
            else if (RankType == RankType.Repute)
            {
                var userRole = Storage.Load<UserRole>(CurrentUserId);
                response.Value = userRole.Repute;
            }
            else if (RankType == RankType.Charm)
            {
                var userRole = Storage.Load<UserRole>(CurrentUserId);
                response.Value = userRole.Charm;
            }

            var rankKey = Utility.CombatSetKey;
            if (RankType == RankType.Repute) rankKey = Utility.ReputeSetKey;
            else if (RankType == RankType.Charm) rankKey = Utility.CharmSetKey;

            response.Rank = (int)Storage.SortedSets.Rank(rankKey, CurrentUserId, false) + 1;

            var startRank = 0;
            var userids = Storage.SortedSets.Range<int>(rankKey, startRank, 49, false).Select(p => p.Key).ToList();
            if (userids.Count == 0)
            {
                Utility.InitAllUserReputeCharmRank();
                userids = Storage.SortedSets.Range<int>(rankKey, startRank, 49, false).Select(p => p.Key).ToList();
            }
            var userRoleList = Storage.LoadList<UserRole>(userids.ToArray());
            var userFormationList = new List<UserFormation>();
            if (RankType == RankType.Combat) userFormationList = Storage.LoadList<UserFormation>(userids.ToArray());

            for (int i = 0; i < userids.Count(); i++)
            {
                startRank++;

                var userRole = userRoleList[i];
                var item = new GetRankListResponse.RankUserItem();
                item.UserId = userRole.Id;
                item.NickName = userRole.NickName;
                item.HeadId = userRole.HeadId;
                item.Level = userRole.Level;
                item.Rank = startRank;

                if (RankType == RankType.Combat)
                {
                    var ruserFormation = userFormationList[i];
                    item.Value = ruserFormation.AttMaxCombat;
                }
                else if (RankType == RankType.Repute)
                {
                    item.Value = userRole.Repute;
                }
                else if (RankType == RankType.Charm)
                {
                    item.Value = userRole.Charm;
                }

                response.Items.Add(item);
            }

            ResultObj = response;
        }
    }
    #endregion

    #region 7015 获取战力排行榜玩家详细信息
    public class CombatRankUserDetailResponse
    {
        public CombatRankUserDetailResponse()
        {
            HeroList = new List<FormationDetailItem>();
        }
        /// <summary>
        /// 防守阵型武将列表
        /// </summary>
        [Tag(1)]
        public List<FormationDetailItem> HeroList { get; set; }
    }
    /// <summary>
    /// 获取战力排行榜玩家详细信息
    /// </summary>
    [GameCode(OpCode = 7015, ResponseType = typeof(CombatRankUserDetailResponse))]
    public class CombatRankUserDetailRequest : GameHandler
    {
        /// <summary>
        /// 目标id
        /// </summary>
        public int UserId { get; set; }
        public override void Process(GameContext context)
        {
            var response = new CombatRankUserDetailResponse();


            UserHero userHero;
            UserFormation userFormation;
            Storage.Load(out userFormation, out userHero, UserId, true);
            foreach (var formationItem in userFormation.StrongestFormation.ToList())
            {
                var targetHeroItem = new FormationDetailItem();
                var heroItem = userHero.Items.Find(o => o.HeroId == formationItem.HeroId);

                targetHeroItem.HeroId = formationItem.HeroId;
                targetHeroItem.StarLevel = heroItem.StarLevel;
                targetHeroItem.Level = heroItem.Level;
                targetHeroItem.Location = formationItem.Location;

                //防守武将列表
                response.HeroList.Add(targetHeroItem);
            }

            ResultObj = response;
        }
    }
    #endregion
}
