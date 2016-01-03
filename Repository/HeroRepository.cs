using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    #region 英雄基本信息

    public class ResHeroSimpleInfoItem
    {
        /// <summary>
        /// 英雄Id（系统id）
        /// </summary>
        [Tag(1)]
        public int HeroId { get; set; }

        /// <summary>
        /// 英雄等级 
        /// </summary>
        [Tag(2)]
        public int Level { get; set; }

        /// <summary>
        /// 英雄星级
        /// </summary>
        [Tag(3)]
        public int StarLevel { get; set; }

        /// <summary>
        /// 当前经验 
        /// </summary>
        [Tag(4)]
        public int Exp { get; set; }

        /// <summary>
        /// 武将状态(0:空闲，1：寻访中) 
        /// </summary>
        [Tag(5)]
        public HeroStatus Status { get; set; }

        /// <summary>
        /// 武将状态截止时间
        /// </summary>
        [Tag(6)]
        public DateTime StatusEndTime { get; set; }
    }

    public class ResHeroPropertiesInfoItem
    {
        /// <summary>
        /// 武
        /// </summary>
        [Tag(8)]
        public int Force { get; set; }

        /// <summary>
        /// 智
        /// </summary>
        [Tag(9)]
        public int Intel { get; set; }

        /// <summary>
        /// 统
        /// </summary>
        [Tag(10)]
        public int Command { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        [Tag(11)]
        public int Hp { get; set; }

        /// <summary>
        /// 物理攻击力
        /// </summary>
        [Tag(13)]
        public int Ad { get; set; }

        /// <summary>
        /// 魔法强度
        /// </summary>
        [Tag(14)]
        public int Ap { get; set; }

        /// <summary>
        /// 物理护甲
        /// </summary>
        [Tag(15)]
        public int AdArm { get; set; }

        /// <summary>
        /// 魔法抗性
        /// </summary>
        [Tag(16)]
        public int ApArm { get; set; }

        /// <summary>
        /// 物理暴击
        /// </summary>
        [Tag(17)]
        public int AdCrit { get; set; }

        /// <summary>
        /// 魔法暴击
        /// </summary>
        [Tag(18)]
        public int ApCrit { get; set; }

        /// <summary>
        /// 格挡
        /// </summary>
        [Tag(19)]
        public int Block { get; set; }

        /// <summary>
        /// 物理吸血
        /// </summary>
        [Tag(20)]
        public int Blood { get; set; }

        /// <summary>
        /// 射程
        /// </summary>
        [Tag(21)]
        public int Range { get; set; }

        /// <summary>
        /// 攻击速度:毫秒 例如800毫秒，则表示每800毫秒攻击一次
        /// </summary>
        [Tag(22)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// 移动速度:表示走一格需要的毫秒数
        /// </summary>
        [Tag(23)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// 生命回复
        /// </summary>
        [Tag(24)]
        public int HpRecovery { get; set; }

        /// <summary>
        /// 气势回复
        /// </summary>
        [Tag(25)]
        public int EnergyRecovery { get; set; }
    }

    /// <summary>
    /// 英雄信息
    /// </summary>
    public class ResHeroInfoItem : ResHeroSimpleInfoItem
    {
        public ResHeroInfoItem()
        {
            EquipList = new List<ResEquipItem>();
        }

        ///// <summary>
        ///// 英雄的状态【0：空闲，1：已上阵】
        ///// </summary>
        //[Tag(5)]
        //public HeroStateType State { get; set; }

        /// <summary>
        /// 本武将碎片
        /// </summary>
        [Tag(7)]
        public int ChipNum { get; set; }

        /// <summary>
        /// 四个技能等级列表[1,1,0,0]，为0表示还未解锁；2星解锁二技能，3星三技能，4星四技能【暂定规则】
        /// </summary>
        [Tag(8)]
        public List<int> SkillLevelList { get; set; }

        /// <summary>
        /// 装备&骑宠列表
        /// </summary>
        [Tag(9)]
        public List<ResEquipItem> EquipList { get; set; }

        /// <summary>
        /// 士兵等级【士兵种类从武将配置找】
        /// </summary>
        [Tag(10)]
        public int ArmyLevel { get; set; }

        /// <summary>
        /// 战斗值
        /// </summary>
        [Tag(11)]
        public int Combat { get; set; }

        /// <summary>
        /// 战斗速度
        /// </summary>
        [Tag(12)]
        public int AttackSpeed { get; set; }
    }

    /// <summary>
    /// 装备/骑宠
    /// </summary>
    public class ResEquipItem
    {
        /// <summary>
        /// 装备/骑宠ID
        /// </summary>
        [Tag(1)]
        public int Id { get; set; }

        /// <summary>
        /// 装备类型【1：武器，2：衣服，3：饰品，4：法宝，5：骑宠】
        /// </summary>
        [Tag(2)]
        public EquipType Type { get; set; }

        /// <summary>
        /// 0:没有可穿戴的装备/骑宠；1：有可穿戴的装备/骑宠但等级没有达到；2：有可穿戴的装备/骑宠；其他：装备/骑宠系统id
        /// </summary>
        [Tag(3)]
        public int EquipId { get; set; }

        /// <summary>
        /// 装备等级
        /// </summary>
        [Tag(4)]
        public int Level { get; set; }

        /// <summary>
        /// 装备等级
        /// </summary>
        [Tag(5)]
        public int Star { get; set; }

        /// <summary>
        /// 经验
        /// </summary>
        [Tag(6)]
        public int Exp { get; set; }
    }

    /// <summary>
    /// 佩戴完装备后的返回对象
    /// </summary>
    public class ResHeroEquipItem
    {
        /// <summary>
        /// 英雄Id（系统id）
        /// </summary>
        [Tag(1)]
        public int HeroId { get; set; }

        /// <summary>
        /// 装备列表
        /// </summary>
        [Tag(2)]
        public List<ResEquipItem> EquipList { get; set; }
    }

    #endregion

    #region 2000 获得英雄界面信息

    /// <summary>
    /// 英雄界面信息
    /// </summary>
    public class GetHerosResponse
    {
        public GetHerosResponse()
        {
            Items = new List<ResHeroInfoItem>();
            ChipList = new List<ChipItem>();
            DefFormation = new List<FormationItem>();
            AttFormation = new List<FormationItem>();
        }

        /// <summary>
        /// 已拥有的英雄列表
        /// </summary>
        [Tag(2)]
        public List<ResHeroInfoItem> Items { get; set; }

        /// <summary>
        /// 未招募武将中已拥有的碎片列表
        /// </summary>
        [Tag(3)]
        public List<ChipItem> ChipList { get; set; }

        /// <summary>
        /// 防守布阵
        /// </summary>
        [Tag(4)]
        public List<FormationItem> DefFormation { get; set; }

        /// <summary>
        /// 攻击布阵
        /// </summary>
        [Tag(5)]
        public List<FormationItem> AttFormation { get; set; }

        /// <summary>
        /// 大地图防守布阵
        /// </summary>
        [Tag(6)]
        public List<FormationItem> BigMapDefFormation { get; set; }
    }

    /// <summary>
    /// 获得英雄界面信息
    /// </summary>
    [GameCode(OpCode = 2000, ResponseType = typeof(GetHerosResponse))]
    public class GetHerosRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetHerosResponse();
            UserHero userHero;
            UserChip userChip;
            UserEquip userEquip;
            UserPet userPet;
            UserFormation userFormation;
            Storage.Load(out userHero, out userChip, out userEquip, out userPet, out userFormation, CurrentUserId, true);

            //通用碎片、技能点的信息
            //response.SkillPoint = userHero.SkillPoint.Value;
            //response.NextRecoverTime = userHero.SkillPoint.GetNextRecoverTime();
            //response.BuySkillPointNum = (int)userHero.BuySkillPointNum.Value;

            //未招募的英雄信息
            foreach (var heroChipItem in userChip.ChipItems.Where(o => o.GetToolType() == ToolType.HeroChip).ToList())
                response.ChipList.Add(heroChipItem);

            //已招募的英雄信息
            foreach (var userHeroItem in userHero.Items.Where(o => o.SysHeroType != HeroType.Npc)
                .OrderByDescending(o => o.SysHeroCfg.Quality)
                .ThenByDescending(o => o.Level)
                .ThenByDescending(o => o.StarLevel)
                .ThenByDescending(o => o.Exp).ToList())
            {
                //已拥有的武将从碎片列表里面清空
                var chipId = Utility.GetChipIdById(userHeroItem.HeroId, ToolType.HeroChip);
                var citem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == chipId);
                response.ChipList.Remove(citem);

                var item = new ResHeroInfoItem();
                var mapper =
                    ObjectMapperManager.DefaultInstance.GetMapper<UserHeroItem, ResHeroInfoItem>(
                        new DefaultMapConfig().IgnoreMembers<UserHeroItem, ResHeroInfoItem>(new[] { "Id" }));
                mapper.Map(userHeroItem, item);
                if (item.StatusEndTime.ToTs() <= 0) item.StatusEndTime = DateTime.Now.AddDays(-10);

                item.ChipNum = userHeroItem.GetChipNum(userChip);

                //已招募英雄的装备信息
                item.EquipList = Utility.GetEquipItems(userHeroItem, userEquip, userPet, Request.OpCode);
                //添加到列表
                response.Items.Add(item);
            }

            response.DefFormation = userFormation.DefFormation;
            response.AttFormation = userFormation.AttFormation;
            response.BigMapDefFormation = userFormation.BigMapDefFormation;

            ResultObj = response;
        }
    }

    #endregion

    #region 2001 获取英雄属性面板数据

    /// <summary>
    /// 武将升星
    /// </summary>
    [GameCode(OpCode = 2001, ResponseType = typeof(ResHeroPropertiesInfoItem))]
    public class GetHeroPropertiesRequest : GameHandler
    {
        /// <summary>
        /// 英雄ID（系统id）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            var userHero = Storage.Load<UserHero>(CurrentUserId, true);

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            //返回英雄的属性面板数据
            var response = new ResHeroPropertiesInfoItem();
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserHeroItem, ResHeroPropertiesInfoItem>(
                    new DefaultMapConfig().IgnoreMembers<UserHeroItem, ResHeroPropertiesInfoItem>(new[] { "Id" }));
            mapper.Map(userHeroItem, response);

            ResultObj = response;
        }
    }

    #endregion

    #region 2003 技能升级
    /// <summary>
    /// 技能升级
    /// </summary>
    [GameCode(OpCode = 2003)]
    public class HeroSkillLevelUpRequest : GameHandler
    {
        /// <summary>
        /// 英雄ID（系统id）
        /// </summary>
        public int HeroId { get; set; }

        /// <summary>
        /// 第几个技能 从0开始
        /// </summary>
        public int Index { get; set; }

        public override bool InitParams(GameContext context)
        {
            if (Index > 3) return false;
            return base.InitParams(context);
        }

        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserHero userHero;
            Storage.Load(out userRole, out userHero, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //找不到该武将信息
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var curSkillLevel = userHeroItem.SkillLevelList[Index];
            var curMaxLevel = userHeroItem.Level;
            if (Index > 1)
            {
                curMaxLevel = userHeroItem.Level - (Index - 1) * 20 + 1;
            }
            if (curSkillLevel >= curMaxLevel)
            {
                //技能等级不能超过英雄等级
                //请先升级武将等级
                SetError(ResourceId.R_2003_SkillLevelCannotHigherHero);
                return;
            }
            //技能类型不同 技能等级也不同
            var maxSkillLevel = SysSkillUpgradeCfg.MaxLevel(Index);
            if (curSkillLevel >= maxSkillLevel)
            {
                //该技能已达到最高级
                SetError(ResourceId.R_2003_HeroIsMaxSkill);
                return;
            }

            var cfg = SysSkillUpgradeCfg.Items.FirstOrDefault(o => (int)o.Type == Index && o.Level == curSkillLevel);
            if (cfg == null)
            {
                //找不到相应技能等级的配置信息
                SetError(ResourceId.R_0000_IdNotExist, "SysSkillUpgradeCfg", curSkillLevel);
                return;
            }

            //if (cfg.Coin > userRole.Coin)
            //{
            //    //铜钱不足
            //    SetError(ResourceId.R_0000_CoinNotEnough);
            //    return;
            //}
            //扣铜币 加技能等级
            //Utility.ConsumeResource(userRole, ItemType.Coin, Request.OpCode, cfg.Coin);
            var consumeCoin = cfg.Coin;
            var consumeWood = cfg.Wood;
            var consumeStone = cfg.Stone;
            var consumeIron = cfg.Iron;
            if (!Utility.JudgeResourceEnough(userRole, consumeCoin, consumeWood, consumeStone,
                consumeIron, out errorMsg))
            {
                SetError(errorMsg);
                return;
            }
            //扣资源
            var opCode = Request.OpCode;
            Utility.ConsumeResource(userRole, ItemType.Coin, opCode, consumeCoin);
            Utility.ConsumeResource(userRole, ItemType.Wood, opCode, consumeWood);
            Utility.ConsumeResource(userRole, ItemType.Stone, opCode, consumeStone);
            Utility.ConsumeResource(userRole, ItemType.Iron, opCode, consumeIron);

            var logItem = new GameLogItem();
            logItem.F1 = userHeroItem.HeroId;
            logItem.F2 = userHeroItem.SkillLevelList[Index];

            userHeroItem.SkillLevelList[Index]++;

            //添加武将技能任务次数
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.SkillUpgrade);

            logItem.F3 = userHeroItem.SkillLevelList[Index];
            logItem.F4 = cfg.Coin;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //武将技能属性重新计算一遍——技能不影响面板数值
            userHeroItem.RefreshSkillProperties();
        }
    }

    #endregion

    #region 2004 武将升星

    /// <summary>
    /// 武将升星
    /// </summary>
    [GameCode(OpCode = 2004)]
    public class HeroUpgradeStarRequest : GameHandler
    {
        /// <summary>
        /// 英雄ID（系统id）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserHero userHero;
            UserChip userChip;
            Storage.Load(out userRole, out userHero, out userChip, CurrentUserId, true);

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var sysHeroCfg = SysHeroCfg.Find(HeroId);
            if (sysHeroCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysHeroCfg", HeroId);
                return;
            }

            var star = userHeroItem.StarLevel;
            if (star >= sysHeroCfg.MaxStarLevel)
            {
                //星级已达到最高级
                SetError(ResourceId.R_2004_HeroIsMaxStar);
                return;
            }

            var heroAdvancedCfg = ConfigHelper.HeroAdvancedCfgData.FirstOrDefault(o => o.UpgradeStar == star + 1);
            if (heroAdvancedCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "HeroAdvancedCfgData", userHeroItem.StarLevel + 1);
                return;
            }

            var needChip = heroAdvancedCfg.ChipNum;
            if (userHeroItem.GetChipNum() < needChip)
            {
                //碎片不足
                SetError(ResourceId.R_2004_ChisNotEnough);
                return;
            }

            var logItem = new GameLogItem();
            logItem.F1 = userHeroItem.HeroId;
            logItem.F2 = userHeroItem.StarLevel;

            //扣除武将碎片
            userChip.SubChip(HeroId, needChip, ToolType.HeroChip);

            userHeroItem.StarLevel++;

            if (userHeroItem.StarLevel >= 4)
            {
                //广播
                var color = Utility.GetQualityColor(sysHeroCfg.Quality);
                var msg = LangResource.GetLangResource(ResourceId.R_0000_AdvancedHeroMsg, userRole.Id,
                    userRole.NickName, sysHeroCfg.Id, sysHeroCfg.Name
                    , userHeroItem.StarLevel, color);
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
            }

            logItem.F3 = userHeroItem.StarLevel;
            logItem.F4 = needChip;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //技能开启的星级限制修改：1星对应技能1，2星对应技能2,3星对应技能3,4星对应技能4
            //modify by hql at 2015.10.24
            if (userHeroItem.StarLevel == 2) userHeroItem.SkillLevelList[1] = 1;
            if (userHeroItem.StarLevel == 3) userHeroItem.SkillLevelList[2] = 1;
            if (userHeroItem.StarLevel == 4) userHeroItem.SkillLevelList[3] = 1;
            //武将本身属性重新计算一遍
            userHeroItem.RefreshHeroProperties(1);
        }
    }

    #endregion

    #region 2005 获取使用经验球武将界面

    /// <summary>
    /// 英雄界面信息
    /// </summary>
    public class GetExpHerosesponse
    {
        public GetExpHerosesponse()
        {
            Items = new List<ResHeroSimpleInfoItem>();
        }

        /// <summary>
        /// 已拥有的英雄列表
        /// </summary>
        [Tag(2)]
        public List<ResHeroSimpleInfoItem> Items { get; set; }
    }

    /// <summary>
    /// 获取使用经验球武将界面
    /// </summary>
    [GameCode(OpCode = 2005, ResponseType = typeof(GetExpHerosesponse))]
    public class GetExpHerosRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetExpHerosesponse();
            var userHero = Storage.Load<UserHero>(CurrentUserId, true);

            foreach (var userHeroItem in userHero.Items)
            {
                var item = new ResHeroSimpleInfoItem();
                var mapper =
                    ObjectMapperManager.DefaultInstance.GetMapper<UserHeroItem, ResHeroSimpleInfoItem>(
                        new DefaultMapConfig().IgnoreMembers<UserHeroItem, ResHeroSimpleInfoItem>(new[] { "Id" }));
                mapper.Map(userHeroItem, item);

                //添加到列表
                response.Items.Add(item);
            }

            ResultObj = response;
        }
    }

    #endregion

    #region 2006 使用经验球

    /// <summary>
    /// 使用经验球回调
    /// </summary>
    public class UseExpBallResponse
    {
        public UseExpBallResponse()
        {
            EquipList = new List<ResEquipItem>();
        }

        /// <summary>
        /// 英雄等级
        /// </summary>
        [Tag(1)]
        public int Level { get; set; }

        /// <summary>
        /// 当前经验
        /// </summary>
        [Tag(2)]
        public int Exp { get; set; }

        /// <summary>
        /// 等级有变化时下发！没变化此值为空，装备&骑宠列表
        /// </summary>
        [Tag(3)]
        public List<ResEquipItem> EquipList { get; set; }
    }

    /// <summary>
    /// 使用经验球
    /// </summary>
    [GameCode(OpCode = 2006, ResponseType = typeof(UseExpBallResponse))]
    public class UseExpBallRequest : GameHandler
    {
        /// <summary>
        /// 物品id
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// 使用的个数-长按传相应的数量，点击则传1
        /// </summary>
        public int UseNum { get; set; }

        /// <summary>
        /// 英雄Id（系统id）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            UserHero userHero;
            UserTool userTool;
            UserEquip userEquip;
            UserPet userPet;
            Storage.Load(out userTool, out userHero, out userEquip, out userPet, CurrentUserId, true);

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //英雄不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var sysToolCfg = SysToolCfg.Find(ItemId);
            if (sysToolCfg == null)
            {
                //物品不存在
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", ItemId);
                return;
            }
            var toolItem = userTool.Items.FirstOrDefault(o => o.ItemId == ItemId);
            if (toolItem == null)
            {
                //道具不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserTool", ItemId);
                return;
            }
            if (sysToolCfg.GetToolType() != ToolType.ExpTool)
            {
                //不是经验球道具
                SetError(ResourceId.R_2006_IsNotExpTool);
                return;
            }
            if (toolItem.Num < UseNum)
            {
                //道具个数不足
                SetError(ResourceId.R_2006_ExpBallNotEnough);
                return;
            }
            var logItem = new GameLogItem();
            logItem.F1 = userHeroItem.HeroId;
            logItem.F2 = userHeroItem.Level;

            //经验球 Param1 代表经验
            var exp = UseNum * sysToolCfg.Param1;
            userHeroItem.AddExp(exp, Request.OpCode);

            logItem.F3 = userHeroItem.Level;
            logItem.F4 = toolItem.ItemId;
            logItem.F5 = UseNum;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //toolItem.Num -= UseNum;
            userTool.RemoveTool(toolItem.ItemId, UseNum, Request.OpCode);

            var response = new UseExpBallResponse();
            response.Level = userHeroItem.Level;
            response.Exp = userHeroItem.Exp; //已招募英雄的装备信息

            if (logItem.F3 > logItem.F2)
            {
                //等级有变化时，下发装备列表信息
                response.EquipList = Utility.GetEquipItems(userHeroItem, userEquip, userPet, Request.OpCode);
            }

            //刷新武将本身属性
            userHeroItem.RefreshHeroProperties();

            ResultObj = response;
        }
    }

    #endregion

    #region 2008 获得已招募武将装备信息-在武将详细页面佩戴了装备后返回英雄列表的时候调用

    public class GetHeroEquipResponse
    {
        public GetHeroEquipResponse()
        {
            Items = new List<ResHeroEquipItem>();
        }

        /// <summary>
        /// 武将装备项
        /// </summary>
        [Tag(1)]
        public List<ResHeroEquipItem> Items { get; set; }
    }

    /// <summary>
    /// 获得已招募武将装备信息
    /// </summary>
    [GameCode(OpCode = 2008, ResponseType = typeof(GetHeroEquipResponse))]
    public class GetHeroEquipRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetHeroEquipResponse();
            UserEquip userEquip;
            UserHero userHero;
            UserPet userPet;
            Storage.Load(out userEquip, out userHero, out userPet, CurrentUserId);

            //已招募英雄的装备信息
            foreach (
                var userHeroItem in
                    userHero.Items.OrderByDescending(o => o.Level).ThenByDescending(o => o.StarLevel).ToList())
            {
                var item = new ResHeroEquipItem();
                item.HeroId = userHeroItem.HeroId;

                item.EquipList = Utility.GetEquipItems(userHeroItem, userEquip, userPet, Request.OpCode);
                //添加到列表
                response.Items.Add(item);
            }

            ResultObj = response;
        }
    }

    #endregion

    #region 2009 获取武将士兵信息

    public class HeroArmyItem
    {
        /// <summary>
        /// 生命
        /// </summary>
        [Tag(1)]
        public int Hp { get; set; }

        /// <summary>
        /// 物攻
        /// </summary>
        [Tag(2)]
        public int Ad { get; set; }

        /// <summary>
        /// 法攻
        /// </summary>
        [Tag(31)]
        public int Ap { get; set; }

        /// <summary>
        /// 物防
        /// </summary>
        [Tag(4)]
        public int AdArm { get; set; }

        /// <summary>
        /// 法防
        /// </summary>
        [Tag(5)]
        public int ApArm { get; set; }
    }

    /// <summary>
    /// 获取武将士兵信息
    /// </summary>
    [GameCode(OpCode = 2009)]
    public class GetHeroArmyInfoRequest : GameHandler
    {
        /// <summary>
        /// 武将id（系统）
        /// </summary>
        public int HeroId { get; set; }

        public override bool InitParams(GameContext context)
        {
            if (HeroId == 0) return false;
            return base.InitParams(context);
        }

        public override void Process(GameContext context)
        {
            var userHero = Storage.Load<UserHero>(CurrentUserId, true);
            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserHero:Items:Id", HeroId);
                return;
            }
            var response = new HeroArmyItem();
            int index = (int)PropertiesType.Army;
            response.Hp = userHeroItem.Hps[index];
            response.Ad = userHeroItem.Ads[index];
            response.Ap = userHeroItem.Aps[index];
            response.AdArm = userHeroItem.AdArms[index];
            response.ApArm = userHeroItem.ApArms[index];
            ResultObj = response;
            //var sysHeroCfg = SysHeroCfg.Find(HeroId);
            //if (sysHeroCfg == null)
            //{
            //    SetError(ResourceId.R_0000_IdNotExist, "SysHeroCfg:Id", HeroId);
            //    return;
            //}
            //var armyLevel = userHeroItem.Level;
            //var sysArmyCfg = SysArmyCfg.Items.FirstOrDefault(o => o.Id == sysHeroCfg.ArmyId);
            //if (sysArmyCfg == null)
            //{
            //    SetError(ResourceId.R_0000_IdNotExist, "SysArmyCfg:Id", sysHeroCfg.ArmyId);
            //    return;
            //}
        }
    }

    #endregion

    #region 2010 武将士兵升级

    public class ArmyUpgradeResponse
    {
        /// <summary>
        /// 升级后的等级
        /// </summary>
        [Tag(1)]
        public int CurLevel { get; set; }
    }

    /// <summary>
    /// 武将士兵升级
    /// </summary>
    [GameCode(OpCode = 2010, ResponseType = typeof(ArmyUpgradeResponse))]
    public class HeroArmyUpgradeRequest : GameHandler
    {
        /// <summary>
        /// 武将id
        /// </summary>
        public int HeroId { get; set; }

        /// <summary>
        /// 是否自动升级【0：升级，1：一键升级】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int OneKey { get; set; }

        public override void Process(GameContext context)
        {
            UserRole userRole;
            UserHero userHero;
            Storage.Load(out userRole, out userHero, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //找不到该武将信息
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }
            if (userHeroItem.ArmyLevel >= userHeroItem.Level)
            {
                //士兵等级不能超过英雄等级
                SetError(ResourceId.R_2003_ArmyLevelCannotHigherHero);
                return;
            }
            var sysHeroCfg = SysHeroCfg.Find(HeroId);
            if (sysHeroCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysHeroCfg:Id", HeroId);
                return;
            }

            var logItem = new GameLogItem();
            logItem.F1 = userHeroItem.HeroId;
            logItem.F2 = userHeroItem.ArmyLevel;

            var consumeCoin = 0;
            var consumeWood = 0;
            var consumeStone = 0;
            var consumeIron = 0;
            while (true)
            {
                var cfg =
                    SysArmyUpgradeCfg.Items.FirstOrDefault(
                        o => o.Id == sysHeroCfg.ArmyId && o.Level == userHeroItem.ArmyLevel);
                if (cfg == null)
                {
                    //找不到相应士兵升级等级的配置信息
                    SetError(ResourceId.R_0000_IdNotExist,
                        string.Format("SysArmyUpgradeCfg:{0}", userHeroItem.ArmyLevel), sysHeroCfg.ArmyId);
                    return;
                }

                consumeCoin += cfg.Coin;
                consumeWood += cfg.Wood;
                consumeStone += cfg.Stone;
                consumeIron += cfg.Iron;
                if (!Utility.JudgeResourceEnough(userRole, consumeCoin, consumeWood, consumeStone,
                    consumeIron, out errorMsg))
                {
                    consumeCoin -= cfg.Coin;
                    consumeWood -= cfg.Wood;
                    consumeStone -= cfg.Stone;
                    consumeIron -= cfg.Iron;
                    if (OneKey == 0)
                    {
                        SetError(errorMsg);
                        return;
                    }
                    break;
                }

                //技能点1点 加技能等级
                userHeroItem.ArmyLevel++;

                //士兵等级不能超过武将等级
                if (userHeroItem.ArmyLevel >= userHeroItem.Level) break;
                //升级-升了一级退出循环
                if (OneKey == 0) break;
            }

            //扣资源
            var opCode = Request.OpCode;
            Utility.ConsumeResource(userRole, ItemType.Coin, opCode, consumeCoin);
            Utility.ConsumeResource(userRole, ItemType.Wood, opCode, consumeWood);
            Utility.ConsumeResource(userRole, ItemType.Stone, opCode, consumeStone);
            Utility.ConsumeResource(userRole, ItemType.Iron, opCode, consumeIron);

            logItem.F3 = userHeroItem.ArmyLevel;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //武将士兵属性重新计算一遍
            userHeroItem.RefreshArmyProperties();

            ResultObj = new ArmyUpgradeResponse() { CurLevel = userHeroItem.ArmyLevel };
        }
    }

    #endregion

    #region 2011 获得武将的战斗力

    public class GetHeroCombatResponse
    {
        /// <summary>
        /// 战斗力
        /// </summary>
        [Tag(1)]
        public int Combat { get; set; }
        /// <summary>
        /// 战斗速度
        /// </summary>
        [Tag(2)]
        public int AttackSpeed { get; set; }
    }

    /// <summary>
    /// 获得武将的战斗力
    /// </summary>
    [GameCode(OpCode = 2011, ResponseType = typeof(GetHeroCombatResponse))]
    public class GetHeroCombatRequest : GameHandler
    {
        /// <summary>
        /// 武将id（系统）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            var userHero = Storage.Load<UserHero>(CurrentUserId, true);

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserHero:Id", HeroId);
                return;
            }

            ResultObj = new GetHeroCombatResponse() { Combat = userHeroItem.Combat, AttackSpeed = userHeroItem.AttackSpeed };
        }
    }

    #endregion

    #region 2012 获得大地图各个城池剩余的寻访次数

    public class GetCitysLaveSearchNumResponse
    {
        public GetCitysLaveSearchNumResponse()
        {
            Items = new List<ResCityItem>();
        }

        /// <summary>
        /// 城池信息列表
        /// </summary>
        [Tag(1)]
        public List<ResCityItem> Items { get; set; }

        public class ResCityItem
        {
            /// <summary>
            /// 城池Id
            /// </summary>
            [Tag(1)]
            public int CityId { get; set; }

            /// <summary>
            /// 剩余次数
            /// </summary>
            [Tag(1)]
            public int Num { get; set; }
        }
    }

    /// <summary>
    /// 获得大地图各个城池剩余的寻访次数
    /// </summary>
    [GameCode(OpCode = 2012, ResponseType = typeof(GetCitysLaveSearchNumResponse))]
    public class GetCitysLaveSearchNumRequest : GameHandler
    {
        /// <summary>
        /// 武将id（系统）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            var userCity = Storage.Load<UserCity>(CurrentUserId, true);

            var response = new GetCitysLaveSearchNumResponse();
            var cityIdList = SysVisitorCfg.Items.Where(o => o.VisitorId == HeroId).Select(o => o.CityId);
            foreach (var userCityItem in userCity.CityItems.Where(o => cityIdList.Contains(o.CityId)))
            {
                response.Items.Add(new GetCitysLaveSearchNumResponse.ResCityItem()
                {
                    CityId = userCityItem.CityId,
                    Num = userCityItem.LaveSearchNum,
                });
            }

            ResultObj = response;
        }
    }

    #endregion

    #region 2013 获取阵型上武将战力等级变化

    public class GetAttForCombatResponse
    {
        public GetAttForCombatResponse()
        {
            Items = new List<AttForCombatItem>();
        }
        /// <summary>
        /// 变化的战力列表
        /// </summary>
        [Tag(1)]
        public List<AttForCombatItem> Items { get; set; }

        public class AttForCombatItem
        {
            /// <summary>
            /// 武将id
            /// </summary>
            [Tag(1)]
            public int HeroId { get; set; }

            /// <summary>
            /// 战力
            /// </summary>
            [Tag(2)]
            public int Combat { get; set; }
        }
    }

    /// <summary>
    /// 获取阵型上武将战力等级变化（当等级有变化的时候请求）
    /// </summary>
    [GameCode(OpCode = 2013, ResponseType = typeof(GetAttForCombatResponse))]
    public class GetAttForCombatRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            UserHero userHero;
            UserFormation userFormation;
            Storage.Load(out userHero, out userFormation, CurrentUserId);

            var response = new GetAttForCombatResponse();
            foreach (var formationItem in userFormation.AttFormation)
            {
                var item = new GetAttForCombatResponse.AttForCombatItem();
                item.HeroId = formationItem.HeroId;


                var heroItem = userHero.Items.FirstOrDefault(o => o.HeroId == item.HeroId);
                if(heroItem == null)
                    throw new ApplicationException(string.Format("not find hero id={0}",item.HeroId));
                item.Combat = heroItem.Combat;

                response.Items.Add(item);
            }

            ResultObj = response;
        }
    }

    #endregion

}