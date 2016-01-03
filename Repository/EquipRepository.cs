using System;
using System.Collections.Generic;
using System.Linq;
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
    #region 6000 装备升级
    public class EquipUpgradeResponse
    {
        /// <summary>
        /// 升级后的等级
        /// </summary>
        [Tag(1)]
        public int CurLevel { get; set; }
    }
    /// <summary>
    /// 装备升级
    /// </summary>
    [GameCode(OpCode = 6000, ResponseType = typeof(EquipUpgradeResponse))]
    public class EquipUpgradeRequest : GameHandler
    {
        /// <summary>
        /// 装备id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 是否自动升级【0：升级，1：一键升级】
        /// </summary>
        public int OneKey { get; set; }
        public override void Process(GameContext context)
        {
            UserEquip userEquip;
            UserRole userRole;
            Storage.Load(out userEquip, out userRole, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }

            var userEquipItem = userEquip.Items.Find(o => o.Id == Id);
            if (userEquipItem == null)
            {
                //装备不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserEquip", Id);
                return;
            }

            if (userEquipItem.Level >= ConfigHelper.EquipCfgData.MaxLevel)
            {
                //装备等级已达到最高级
                SetError(ResourceId.R_6000_EquipIsMaxLevel);
                return;
            }

            var ignoreUserHeroLevel = ConfigHelper.IgnoreUserHeroLevel;
            var limitLevel = 1;
            if (ignoreUserHeroLevel >= userRole.Level)
            {
                var userHero = Storage.Load<UserHero>(CurrentUserId, true);
                var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == userEquipItem.HeroId);
                if (userHeroItem != null)
                {
                    limitLevel = userHeroItem.Level;
                }
                else
                {
                    limitLevel = ignoreUserHeroLevel;
                }
            }
            else
            {
                limitLevel = userRole.Level;
            }
            if (userEquipItem.Level >= limitLevel)
            {
                //装备等级不能大于主公等级
                SetError(ResourceId.R_6000_EquipLevelCannotUpUserRole);
                return;
            }

            if (userEquipItem.Level >= ConfigHelper.EquipCfgData.MaxLevel)
            {
                //装备等级已达到最高级
                SetError(ResourceId.R_6000_EquipIsMaxLevel);
                return;
            }

            var logItem = new GameLogItem();
            logItem.F1 = userEquipItem.Id;
            logItem.F2 = userEquipItem.ItemId;
            logItem.F3 = userEquipItem.Level;

            var consumeCoin = 0;
            var consumeWood = 0;
            var consumeStone = 0;
            var consumeIron = 0;
            while (true)
            {
                var upgradeCfg =
                    SysEquipUpgradeCfg.Find(
                        o =>
                            o.Level == userEquipItem.Level && o.EquipType == userEquipItem.EquipType &&
                            o.Quality == userEquipItem.Quality);

                if (upgradeCfg == null)
                {
                    //装备升级配置不存在
                    SetError(ResourceId.R_0000_IdNotExist, "SysEquipUpgradeCfg",
                        string.Format("Level:{0},EquipType:{1},Quality:{2}", userEquipItem.Level,
                            userEquipItem.EquipType, (int)userEquipItem.Quality));
                    return;
                }

                consumeCoin += upgradeCfg.Coin;
                consumeWood += upgradeCfg.Wood;
                consumeStone += upgradeCfg.Stone;
                consumeIron += upgradeCfg.Iron;

                //判断资源是否足够
                if (!Utility.JudgeResourceEnough(userRole, consumeCoin, consumeWood, consumeStone,
                    consumeIron, out errorMsg))
                {
                    consumeCoin -= upgradeCfg.Coin;
                    consumeWood -= upgradeCfg.Wood;
                    consumeStone -= upgradeCfg.Stone;
                    consumeIron -= upgradeCfg.Iron;

                    if (OneKey == 0)
                    {
                        SetError(errorMsg);
                        return;
                    }
                    break;
                }
                userEquipItem.Level++;
                //装备等级不能大于主公等级
                if (userEquipItem.Level == limitLevel) break;
                if (userEquipItem.Level > limitLevel)
                {
                    //升级过了！
                    userEquipItem.Level--;

                    consumeCoin -= upgradeCfg.Coin;
                    consumeWood -= upgradeCfg.Wood;
                    consumeStone -= upgradeCfg.Stone;
                    consumeIron -= upgradeCfg.Iron;
                }
                //已升至最高级
                if (userEquipItem.Level >= ConfigHelper.EquipCfgData.MaxLevel) break;
                //升级-升了一级退出循环
                if (OneKey == 0) break;
            }

            //扣除资源
            var opCode = Request.OpCode;
            Utility.ConsumeResource(userRole, ItemType.Coin, opCode, consumeCoin);
            Utility.ConsumeResource(userRole, ItemType.Wood, opCode, consumeWood);
            Utility.ConsumeResource(userRole, ItemType.Stone, opCode, consumeStone);
            Utility.ConsumeResource(userRole, ItemType.Iron, opCode, consumeIron);

            logItem.F4 = userEquipItem.Level;
            GameLogManager.CommonLog(opCode, CurrentUserId, 0, logItem);

            userEquipItem.RefreshProperties();

            var response = new EquipUpgradeResponse();
            response.CurLevel = userEquipItem.Level;
            ResultObj = response;
        }
    }
    #endregion

    #region 6001 装备升星——熔炼
    /// <summary>
    /// 装备升星——熔炼
    /// </summary>
    [GameCode(OpCode = 6001)]
    public class EquipAdvancedRequest : GameHandler
    {
        /// <summary>
        /// 装备id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 道具ID列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string ToolIdArray { get; set; }
        /// <summary>
        /// 道具ID列表
        /// </summary>
        public List<int> ToolIdList;

        /// <summary>
        /// 道具数量列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string ToolNumIdArray { get; set; }
        /// <summary>
        /// 道具数量列表
        /// </summary>
        public List<int> ToolNumIdList;

        /// <summary>
        /// 碎片ID列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string ChipIdArray { get; set; }
        /// <summary>
        /// 碎片ID列表
        /// </summary>
        public List<int> ChipIdList;

        /// <summary>
        /// 碎片数量列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string ChipNumIdArray { get; set; }
        /// <summary>
        /// 碎片数量列表
        /// </summary>
        public List<int> ChipNumIdList;

        /// <summary>
        /// 装备ID列表
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string EquipIdArray { get; set; }
        /// <summary>
        /// 装备ID列表
        /// </summary>
        public List<int> EquipIdList;

        public override bool InitParams(GameContext context)
        {
            if (ToolIdList.Count == 0 && ChipIdList.Count == 0 && EquipIdList.Count == 0)
                return false;
            if (ToolIdList.Count != ToolNumIdList.Count) return false;
            if (ChipIdList.Count != ChipNumIdList.Count) return false;

            return true;
        }

        public override void Process(GameContext context)
        {
            UserEquip userEquip;
            UserRole userRole;
            UserTool userTool;
            UserChip userChip;
            Storage.Load(out userEquip, out userRole, out userTool, out userChip, CurrentUserId, true);

            string errorMsg;
            if (!Utility.JudgeUserBeAttack(userRole, out errorMsg))
            {
                SetError(errorMsg, userRole.BeAttackEndTime.ToTs());
                return;
            }
            var userEquipItem = userEquip.Items.Find(o => o.Id == Id);
            if (userEquipItem == null)
            {
                //装备不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserEquip", Id);
                return;
            }

            if (userEquipItem.Quality == ItemQuality.White)
            {
                //只有绿色及以上品质的装备才能升星
                SetError(ResourceId.R_6002_EquipQualityTooLow);
                return;
            }

            if (userEquipItem.Star >= userEquipItem.MaxStar)
            {
                //装备星级已达到最高级
                SetError(ResourceId.R_6001_EquipIsMaxStar);
                return;
            }

            var opCode = Request.OpCode;
            var totalExp = 0;

            var logItem = new GameLogItem();
            //检查道具并计算出器魂
            var index = 0;
            ToolIdArray = "";
            foreach (var i in ToolIdList)
            {
                var num = ToolNumIdList[index];
                var toolItem = userTool.Items.FirstOrDefault(o => o.Id == i);
                if (toolItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserToolItem:Id", i);
                    return;
                }
                if (toolItem.Num < num)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserToolItem:ItemId", toolItem.ItemId);
                    return;
                }
                var sysToolCfg = SysToolCfg.Find(toolItem.ItemId);
                if (sysToolCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", toolItem.ItemId);
                    return;
                }
                if (sysToolCfg.GetToolType() != ToolType.EquipExpTool)
                {
                    SetError(ResourceId.R_6002_NotEquipExpTool);
                    return;
                }
                if (!userTool.RemoveTool(toolItem.ItemId, num, opCode))
                {
                    SetError(ResourceId.R_0000_IllegalParam);
                    return;
                }
                ToolIdArray += sysToolCfg.Id + ",";
                totalExp += sysToolCfg.Param1 * num;
                index++;
            }
            logItem.F1 = totalExp;

            //检查碎片并计算出器魂
            index = 0;
            foreach (var i in ChipIdList)
            {
                var num = ChipNumIdList[index];
                var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == i);
                if (chipItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "userChip.EquipChipItems", i);
                    return;
                }
                //检查物品是否存在
                var sysToolCfg = SysToolCfg.Find(chipItem.ItemId);//Item.Find(chipItem.ItemId);
                if (sysToolCfg == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", chipItem.ItemId);
                    return;
                }
                var toolType = sysToolCfg.GetToolType();
                if (toolType != ToolType.EquipChip)
                {
                    SetError(ResourceId.R_0000_IllegalParam);
                    return;
                }
                if (num > chipItem.Num)
                {
                    SetError(ResourceId.R_4004_ChipNotEnough);
                    return;
                }
                //减去碎片
                userChip.SubChip(i, num, ToolType.None);
                totalExp += chipItem.MeltingExp * num;
                index++;
            }
            logItem.F2 = totalExp - logItem.F1;

            //检查装备并计算出器魂
            foreach (var i in EquipIdList)
            {
                var equipItem = userEquip.Items.FirstOrDefault(o => o.Id == i);
                if (equipItem != null)
                {
                    if (equipItem.HeroId > 0)
                    {
                        SetError(ResourceId.R_4003_CanNotSellEquipedEquip);
                        return;
                    }
                    totalExp += equipItem.MeltingExp;
                    userEquip.RemoveEquip(equipItem, opCode);
                }
            }

            if (totalExp == 0)
            {
                //没加经验
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }

            var realToalExp = totalExp;
            
            logItem.F3 = totalExp - logItem.F1 - logItem.F2;
            logItem.F4 = totalExp;
            logItem.F5 = userEquipItem.Star;
            logItem.F6 = userEquipItem.Exp;

            //升级装备星级
            userEquipItem.Exp += totalExp;
            do
            {
                var upgradeExp = userEquipItem.UpgradeExp;
                if (userEquipItem.Exp >= upgradeExp)
                {
                    userEquipItem.Exp -= upgradeExp;
                    userEquipItem.Star++;

                    if (userEquipItem.Star >= userEquipItem.MaxStar) break;
                }
                else
                {
                    break;
                }
            } while (true);
            logItem.F7 = userEquipItem.Star;
            logItem.F8 = userEquipItem.Exp;

            //计算真正的需要加的经验值
            if (userEquipItem.Exp > 0 && userEquipItem.Star >= userEquipItem.MaxStar)
            {
                realToalExp -= userEquipItem.Exp;
                userEquipItem.Exp = 0;
            }
            //消耗铜钱熔炼
            var needCoin = realToalExp * ConfigHelper.EquipCfgData.ExpCoinModulus;
            if (userRole.Coin < needCoin)
            {
                SetError(ResourceId.R_0000_CoinNotEnough);
                return;
            }

            if (logItem.F7 > logItem.F5 && logItem.F7 >= 4)
            {
                //广播
                var sysEquipCfg = userEquipItem.SysEquipCfg;
                var color = Utility.GetQualityColor(sysEquipCfg.Quality);
                var msg = LangResource.GetLangResource(ResourceId.R_0000_AdvancedEquipMsg, userRole.Id,
                                                     userRole.NickName, sysEquipCfg.Id, sysEquipCfg.Name
                                                     , userEquipItem.Star, color);
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
            }

            Utility.ConsumeResource(userRole, ItemType.Coin, Request.OpCode, needCoin);
            logItem.F9 = needCoin;
            logItem.F10 = realToalExp;

            logItem.S1 = ToolIdArray;
            logItem.S2 = ToolNumIdArray;
            logItem.S3 = ChipIdArray;
            logItem.S4 = ChipNumIdArray;
            logItem.S5 = EquipIdArray;
            GameLogManager.CommonLog(opCode, CurrentUserId, 0, logItem);

            userEquipItem.RefreshProperties(1);

            //添加装备附魔任务次数
            Utility.AddDailyTaskGoalData(CurrentUserId, DailyType.EquipUpgradeStar);
        }
    }
    #endregion

    #region 6002 穿上/更换装备
    /// <summary>
    /// 穿上装备
    /// </summary>
    [GameCode(OpCode = 6002)]
    public class EquipEquipdRequest : GameHandler
    {
        /// <summary>
        /// 装备id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 英雄id（系统id）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            UserEquip userEquip;
            UserHero userHero;
            Storage.Load(out userEquip, out userHero, CurrentUserId, true);

            var userEquipItem = userEquip.Items.Find(o => o.Id == Id);
            if (userEquipItem == null)
            {
                //装备不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserEquip", Id);
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //英雄不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var equipType = userEquipItem.EquipType;

            var oldEquipId = userHeroItem.EquipIdList[(int)equipType - 1];
            if (oldEquipId == Id)
            {
                //没有改变装备，直接返回
                return;
            }

            if (userHeroItem.Level < userEquipItem.NeedLevel)
            {
                //英雄等级未达到，不能装备
                SetError(ResourceId.R_6002_HeroNotEnoughLevel);
                return;
            }

            if (oldEquipId > 0)
            {
                //先把该武将身上的装备脱下
                var oldEquipItem = userEquip.Items.Find(o => o.Id == oldEquipId);
                if (oldEquipItem != null)
                {
                    oldEquipItem.HeroId = 0;
                }
            }

            if (userEquipItem.HeroId > 0)
            {
                //先从其他武将脱下装备来
                var oldHeroItem = userHero.FindByHeroId(userEquipItem.HeroId);
                if (oldHeroItem != null)
                {
                    oldHeroItem.EquipIdList[(int)equipType - 1] = 0;
                    oldHeroItem.RefreshEquipProperties();
                }
            }

            //穿上
            userHeroItem.EquipIdList[(int)equipType - 1] = userEquipItem.Id;
            userEquipItem.HeroId = userHeroItem.HeroId;
            userHeroItem.RefreshEquipProperties();

            userHero.ChangeNewMsg();
        }
    }
    #endregion

    #region 6003 卸下装备
    /// <summary>
    /// 卸下装备
    /// </summary>
    [GameCode(OpCode = 6003)]
    public class OffEquipdRequest : GameHandler
    {
        /// <summary>
        /// 装备id
        /// </summary>
        public int EquipId { get; set; }
        /// <summary>
        /// 英雄id（系统id）
        /// </summary>
        public int HeroId { get; set; }
        public override void Process(GameContext context)
        {
            UserEquip userEquip;
            UserHero userHero;
            Storage.Load(out userEquip, out userHero, CurrentUserId, true);

            var userEquipItem = userEquip.Items.Find(o => o.Id == EquipId);
            if (userEquipItem == null)
            {
                //装备不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserEquip", EquipId);
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //英雄不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }
            var equipType = userEquipItem.EquipType;

            var oldEquipId = userHeroItem.EquipIdList[(int)equipType - 1];
            if (oldEquipId != EquipId)
            {
                //该英雄未佩戴该装备
                SetError(ResourceId.R_6003_HeroNotEquipThisEqip);
                return;
            }

            userHeroItem.EquipIdList[(int)equipType - 1] = 0;
            userEquipItem.HeroId = 0;
            userHeroItem.RefreshEquipProperties();

            userHero.ChangeNewMsg();
        }
    }
    #endregion

    #region 6004 熔炼装备
    public class MeltingEquipdResponse
    {
        /// <summary>
        /// 获得的铜币
        /// </summary>
        [Tag(1)]
        public int Coin { get; set; }
        /// <summary>
        /// 获得的进阶石
        /// </summary>
        [Tag(2)]
        public int AdvancedStone { get; set; }
    }
    /// <summary>
    /// 熔炼装备
    /// </summary>
    [GameCode(OpCode = 6004, ResponseType = typeof(MeltingEquipdResponse))]
    public class MeltingEquipdRequest : GameHandler
    {
        /// <summary>
        /// 装备id列表（系统id）
        /// </summary>
        public string EquipIdArray { get; set; }
        /// <summary>
        /// 装备id列表（系统id）
        /// </summary>
        public List<int> EquipIdList;

        public override bool InitParams(GameContext context)
        {
            if (EquipIdList.Count == 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            UserEquip userEquip;
            UserRole userRole;
            Storage.Load(out userEquip, out userRole, CurrentUserId, true);

            var needQuality = ConfigHelper.EquipCfgData.NeedQuality;

            var logItem = new GameLogItem();
            logItem.S1 = "";
            logItem.S2 = "";

            var equipList = new List<UserEquipItem>();
            foreach (var i in EquipIdList)
            {
                var userEquipItem = userEquip.Items.Find(o => o.Id == i);
                if (userEquipItem == null)
                {
                    //装备不存在
                    SetError(ResourceId.R_0000_IdNotExist, "UserEquip", i);
                    return;
                }
                if (userEquipItem.HeroId > 0)
                {
                    //不能熔炼已佩戴的装备
                    SetError(ResourceId.R_6005_CanNotMeltingEquipedEquip);
                    return;
                }
                if (userEquipItem.Quality < needQuality)
                {
                    //不能熔炼蓝色品质以下的装备
                    SetError(ResourceId.R_6005_CanNotMeltingUnderBlueEquip);
                    return;
                }
                equipList.Add(userEquipItem);

                logItem.S1 += userEquipItem.ItemId + ",";
                logItem.S2 += userEquipItem.Level + ",";
            }

            var meltingBackPrec = ConfigHelper.EquipCfgData.MeltingBackPrec;
            var backCoin = 0;
            var backAdvancedStone = 0;
            var meltingAdvancedStone = 0;
            foreach (var userEquipItem in equipList)
            {
                //升级消耗
                var upgradeList =
                    SysEquipUpgradeCfg.Items.Where(
                        o =>
                            o.EquipType == userEquipItem.EquipType && o.Quality == userEquipItem.Quality &&
                            o.Level <= userEquipItem.Level).ToList();
                backCoin += upgradeList.Sum(o => o.Coin);

                //升阶消耗
                var advancedList =
                    SysEquipAdvancedCfg.Items.Where(
                        o =>
                            o.EquipType == userEquipItem.EquipType && o.Quality == userEquipItem.Quality).ToList();
                // && o.Rank <= userEquipItem.Rank
                backCoin += advancedList.Sum(o => o.NeedNum2);
                backAdvancedStone += advancedList.Sum(o => o.NeedNum1);

                //熔炼蓝色品质返还配置
                var meltingCfg =
                    SysEquipMeltingCfg.Items.FirstOrDefault(
                        o => o.EquipType == userEquipItem.EquipType && o.Quality == userEquipItem.Quality);
                if (meltingCfg != null)
                {
                    meltingAdvancedStone += meltingCfg.ToolNum;
                }

                //删除装备
                userEquip.RemoveEquip(userEquipItem, Request.OpCode);
            }

            backCoin = (int)(backCoin * 1.0 * meltingBackPrec / 100);
            backAdvancedStone = (int)(backAdvancedStone * 1.0 * backAdvancedStone / 100) + meltingAdvancedStone;

            var opCode = Request.OpCode;
            Utility.AddResource(userRole, ItemType.Coin, opCode, backCoin);

            //添加进阶石
            //var sysToolCfg = SysToolCfg.Find((int)ToolType.EquipAdvabcedStone);
            //if (sysToolCfg == null)
            //{
            //    SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg", (int)ToolType.EquipAdvabcedStone);
            //    return;
            //}

            //sysToolCfg.AddToUser(CurrentUserId, opCode, backAdvancedStone);

            logItem.F1 = backCoin;
            logItem.F2 = backAdvancedStone;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            //返回最后的熔炼返还
            var response = new MeltingEquipdResponse();
            response.Coin = backCoin;
            response.AdvancedStone = backAdvancedStone;
            ResultObj = response;
        }
    }
    #endregion

    #region 6005 穿上/更换骑宠
    /// <summary>
    /// 穿上/更换骑宠
    /// </summary>
    [GameCode(OpCode = 6005)]
    public class EquipPetdRequest : GameHandler
    {
        /// <summary>
        /// 骑宠id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 英雄id（系统id）
        /// </summary>
        public int HeroId { get; set; }

        public override void Process(GameContext context)
        {
            UserPet userPet;
            UserHero userHero;
            Storage.Load(out userPet, out userHero, CurrentUserId, true);

            var userPetItem = userPet.Items.Find(o => o.Id == Id);
            if (userPetItem == null)
            {
                //装备不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserPet", Id);
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //英雄不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var sysPetCfg = SysPetCfg.Find(userPetItem.PetId);
            if (sysPetCfg == null)
            {
                //骑宠不存在
                SetError(ResourceId.R_0000_IdNotExist, "SysPetCfg", userPetItem.PetId);
                return;
            }
            if (sysPetCfg.NeedLevel > userHeroItem.Level)
            {
                //英雄等级不足
                SetError(ResourceId.R_6002_HeroNotEnoughLevel);
                return;
            }

            var oldPetId = userHeroItem.PetId;
            if (oldPetId == Id)
            {
                //没有改变骑宠，直接返回
                return;
            }

            //if (userHeroItem.Level < userPetItem.NeedLevel)
            //{
            //    //英雄等级未达到，不能装备
            //    SetError(ResourceId.R_6002_HeroNotEnoughLevel);
            //    return;
            //}

            if (oldPetId > 0)
            {
                //先把该武将身上的骑宠脱下
                var oldPetItem = userPet.Items.Find(o => o.Id == oldPetId);
                if (oldPetItem != null)
                {
                    oldPetItem.HeroId = 0;
                }
            }

            if (userPetItem.HeroId > 0)
            {
                //先从其他武将脱下骑宠来
                var oldHeroItem = userHero.FindByHeroId(userPetItem.HeroId);
                if (oldHeroItem != null)
                {
                    oldHeroItem.PetId = 0;
                    oldHeroItem.SysPetId = 0;
                    oldHeroItem.RefreshPetProperties();
                }
            }

            //穿上
            userHeroItem.PetId = Id;
            userHeroItem.SysPetId = userPetItem.PetId;
            userPetItem.HeroId = userHeroItem.HeroId;
            userHeroItem.RefreshPetProperties();

            userPetItem.RefreshProperties();

            userHero.ChangeNewMsg();
        }
    }
    #endregion

    #region 6006 卸下骑宠
    /// <summary>
    /// 卸下骑宠
    /// </summary>
    [GameCode(OpCode = 6006)]
    public class OffPetdRequest : GameHandler
    {
        /// <summary>
        /// 骑宠id
        /// </summary>
        public int PetId { get; set; }
        /// <summary>
        /// 英雄id（系统id）
        /// </summary>
        public int HeroId { get; set; }
        public override void Process(GameContext context)
        {
            UserPet userPet;
            UserHero userHero;
            Storage.Load(out userPet, out userHero, CurrentUserId, true);

            var userPetItem = userPet.Items.Find(o => o.Id == PetId);
            if (userPetItem == null)
            {
                //骑宠不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserPet", PetId);
                return;
            }

            var userHeroItem = userHero.FindByHeroId(HeroId);
            if (userHeroItem == null)
            {
                //英雄不存在
                SetError(ResourceId.R_0000_IdNotExist, "UserHero", HeroId);
                return;
            }

            var oldPetId = userHeroItem.PetId;
            if (oldPetId != PetId)
            {
                //该英雄未佩戴该骑宠
                SetError(ResourceId.R_6006_HeroNotEquipThisPet);
                return;
            }

            userHeroItem.PetId = 0;
            userHeroItem.SysPetId = 0;
            userPetItem.HeroId = 0;
            userHeroItem.RefreshPetProperties();

            userPetItem.RefreshProperties();

            userHero.ChangeNewMsg();
        }
    }
    #endregion
}
