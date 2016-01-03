using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Tasks;
using Newtonsoft.Json;

namespace MobileGame.tianzi
{
    public class ConfigHelper
    {
        /// <summary>
        /// GMId列表
        /// </summary>
        public static string GMIDList { get { return ParamHelper.Get<string>("GMIDList"); } }
        /// <summary>
        /// 大地图最多匹配玩家个数
        /// </summary>
        public static int BigMapMatchMaxUserNum { get { return ParamHelper.Get<int>("BigMapMatchMaxUserNum", 8); } }
        /// <summary>
        /// 大地图最多匹配NPC个数
        /// </summary>
        public static int BigMapMatchMaxNpcNum { get { return ParamHelper.Get<int>("BigMapMatchMaxNpcNum", 5); } }
        /// <summary>
        /// 刷新装备信息值
        /// </summary>
        public static int RefreshEquipData { get { return ParamHelper.Get<int>("RefreshEquipData"); } }
        /// <summary>
        /// 新手引导战斗NpcId
        /// </summary>
        public static int BigMapGuideBattleNpcId { get { return ParamHelper.Get<int>("BigMapGuideBattleNpcId", 2001); } }
        /// <summary>
        /// 刷新武将信息值
        /// </summary>
        public static int RefreshHeroData { get { return ParamHelper.Get<int>("RefreshHeroData"); } }
        /// <summary>
        /// 竞技场挑战锁定时间
        /// </summary>
        public static int PkAttackLockTime { get { return ParamHelper.Get<int>("PkAttackLockTime", 180); } }
        /// <summary>
        /// 服务器开启时取消竞技场任务数
        /// </summary>
        public static int ClearPkTaskNum { get { return ParamHelper.Get<int>("ClearPkTaskNum", 0); } }
        /// <summary>
        /// 重新匹配的数字（如果比玩家身上存储的大，重新来过）
        /// </summary>
        public static int ClearBigMapNum { get { return ParamHelper.Get<int>("ClearBigMapNum", 0); } }
        /// <summary>
        /// 重新匹配的数字（如果比玩家身上存储的大，重新来过）
        /// </summary>
        public static int ClearBigMapActivityNum { get { return ParamHelper.Get<int>("ClearBigMapActivityNum", 0); } }
        /// <summary>
        /// 重新生成宫殿列表的数字
        /// </summary>
        public static int ClearBuildingNum { get { return ParamHelper.Get<int>("ClearBuildingNum", 0); } }
        /// <summary>
        /// 检测宫殿配置是否存在的数字
        /// </summary>
        public static int CheckBuildingNum { get { return ParamHelper.Get<int>("CheckBuildingNum", 0); } }
        /// <summary>
        /// 可以重新生成的id列表
        /// </summary>
        public static string ClearUserIdList { get { return ParamHelper.Get<string>("ClearUserIdList", "*"); } }
        /// <summary>
        /// 重新检查任务系统的数字
        /// </summary>
        public static int ReLoadTaskNum { get { return ParamHelper.Get<int>("ReLoadTaskNum", 0); } }
        /// <summary>
        /// 升级所加体力系数
        /// </summary>
        public static int UpgradeLevelAddSpModulus { get { return ParamHelper.Get<int>("UpgradeLevelAddSpModulus", 2); } }
        /// <summary>
        /// 购买一次体力所加的值
        /// </summary>
        public static int BuySpAddValue { get { return ParamHelper.Get<int>("BuySpAddValue", 120); } }
        /// <summary>
        /// 购买体力所能达到的最大值
        /// </summary>
        public static int MaxBuySp { get { return ParamHelper.Get<int>("MaxBuySp", 600); } }
        /// <summary>
        /// 武将多少级以下不用低于主公等级
        /// </summary>
        public static int IgnoreUserHeroLevel { get { return ParamHelper.Get<int>("IgnoreUserHeroLevel", 10); } }

        /// <summary>
        /// 初始宫殿的系统宫殿ID列表
        /// </summary>
        public static List<int> InitBuildIdList
        {
            get
            {
                var key = "InitBuildIdList";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<List<int>>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (List<int>)obj;
            }
        }

        /// <summary>
        /// 战斗默认的骑宠ID
        /// </summary>
        public static int BattleDefaultPetId { get { return ParamHelper.Get<int>("BattleDefaultPetId", 5000103); } }
        /// <summary>
        /// 是否保存打副本的战报
        /// </summary>
        public static int SaveLevelsData { get { return ParamHelper.Get<int>("SaveLevelsData", 0); } }
        /// <summary>
        /// 分享战报间隔时间（秒）
        /// </summary>
        public static int ShareBattleIntervalTime { get { return ParamHelper.Get<int>("ShareBattleIntervalTime", 1800); } }
        /// <summary>
        /// 大地图攻打领地战斗时锁定玩家的时间
        /// </summary>
        public static int BattleExpireTime { get { return ParamHelper.Get<int>("BattleExpireTime", 300); } }
        /// <summary>
        /// 等待玩家点击抢钱抢粮抢妃子按钮的时间
        /// </summary>
        public static int ResultExpireTime { get { return ParamHelper.Get<int>("ResultExpireTime", 180); } }
        /// <summary>
        /// 抢后宫可以持续的秒数（客户端是3分钟）
        /// </summary>
        public static int RobExpireTime { get { return ParamHelper.Get<int>("RobExpireTime", 200); } }
        /// <summary>
        /// 昵称长度
        /// </summary>
        public static int NickNameLength { get { return ParamHelper.Get<int>("NickNameLength", 10); } }
        /// <summary>
        /// 最大的免费修改密码次数
        /// </summary>
        public static int MaxModifyNickNameNum { get { return ParamHelper.Get<int>("MaxModifyNickNameNum", 1); } }
        /// <summary>
        /// 修改昵称需要的元宝
        /// </summary>
        public static int ModifyNickNameNeedIngot { get { return ParamHelper.Get<int>("ModifyNickNameNeedIngot", 100); } }
        /// <summary>
        /// 定时请求的接口列表
        /// </summary>
        public static List<int> TimingOpcodeList { get { return ParamHelper.Get<List<int>>("TimingOpcodeList"); } }


        #region 装备
        public static int EquipLevelPrice { get { return ParamHelper.Get<int>("EquipLevelPrice", 1); } }
        #endregion

        #region 商店配置
        public class ShopCfg
        {
            /// <summary>
            /// 神秘商人出现概率
            /// </summary>
            public int MysteriousProbability { get; set; }
            /// <summary>
            /// 西域商人出现概率
            /// </summary>
            public int WesternProbability { get; set; }
            /// <summary>
            /// 商店总共的物品个数
            /// </summary>
            public int TotalGoodsNum { get; set; }
            /// <summary>
            /// 固定下来的商店一天刷新的时间点列表
            /// </summary>
            public List<int> LuoYangRefreshTimeList { get; set; }
            /// <summary>
            /// 神秘/西域商人逗留的时间
            /// </summary>
            public int BussinessStayTime { get; set; }
            /// <summary>
            /// 神秘/西域商店固定逗留的城池列表
            /// </summary>
            public List<int> BussinessStayCityList { get; set; }
            /// <summary>
            /// 神秘/西域商店固定在城池所需Vip列表
            /// </summary>
            public List<int> StayCityNeedVipList { get; set; }
        }
        /// <summary>
        ///商店配置信息
        ///  </summary>
        public static ShopCfg ShopCfgData
        {
            get
            {
                var key = "ShopCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<ShopCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (ShopCfg)obj;
            }
        }
        #endregion

        #region 武将抽取
        public class ExtractCfg
        {
            /// <summary>
            /// 合成各品质武将所需碎片（绿色、蓝色、紫色、橙色）
            /// </summary>
            //public List<int> MixtureHeroChips { get; set; }
            /// <summary>
            /// 获取已有的各品质武将所得碎片（绿色、蓝色、紫色、橙色）
            /// </summary>
            //public List<int> ExchangeHeroChips { get; set; }
            /// <summary>
            /// 一次抽取价格（铜币、钻石）
            /// </summary>
            public List<int> OneExtractPrices { get; set; }
            /// <summary>
            /// 十次抽取价格（铜币、钻石）
            /// </summary>
            public List<int> TenExtractPrices { get; set; }
            /// <summary>
            /// 每日免费抽取次数（铜币、钻石）
            /// </summary>
            public List<int> MaxExtracts { get; set; }
            /// <summary>
            /// 抽取间隔时间（铜币、钻石）
            /// </summary>
            public List<int> ExtractIntervals { get; set; }
            /// <summary>
            /// 合成各品质妃子所需碎片（绿色、蓝色、紫色、橙色）
            /// </summary>
            //public List<int> MixtureConcubineChips { get; set; }
            /// <summary>
            /// 获取已有的各品质妃子所得碎片（绿色、蓝色、紫色、橙色）
            /// </summary>
            //public List<int> ExchangeConcubineChips { get; set; }
        }
        /// <summary>
        /// 武将抽取配置信息
        ///  </summary>
        public static ExtractCfg ExtractCfgData
        {
            get
            {
                var key = "ExtractCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<ExtractCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (ExtractCfg)obj;
            }
        }
        #endregion

        #region 武将功能配置
        public class HeroCfg
        {
            /// <summary>
            /// 最大技能点
            /// </summary>
            public int MaxSkillPoint { get; set; }
            /// <summary>
            /// 技能恢复间隔（秒）
            /// </summary>
            public int SkillInterval { get; set; }
            /// <summary>
            /// 最大使用通用碎片的百分比
            /// </summary>
            public int MaxUseGenerPrec { get; set; }
        }
        /// <summary>
        /// 武将功能配置信息
        ///  </summary>
        public static HeroCfg HeroCfgData
        {
            get
            {
                var key = "HeroCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<HeroCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (HeroCfg)obj;
            }
        }
        #endregion

        #region 装备配置
        public class EquipCfg
        {
            /// <summary>
            /// 需要的品质
            /// </summary>
            public ItemQuality NeedQuality { get; set; }
            /// <summary>
            /// 最高等级
            /// </summary>
            public int MaxLevel { get; set; }
            /// <summary>
            /// 最高星级列表【白、绿、蓝、紫、橙】
            /// </summary>
            public List<int> MaxStarList { get; set; }
            /// <summary>
            /// 各个颜色升级星级所需经验的列表【[[20],[30,50,80],[60,100,160,300,500],[100,160,300,500,750]]】
            /// </summary>
            public List<List<int>> UpgradeStarExpList { get; set; }
            /// <summary>
            /// 星级所加属性百分比列表【[20,30,50,80,130]】
            /// </summary>
            public List<int> StarAddPrecList { get; set; }
            /// <summary>
            /// 熔炼返还百分比
            /// </summary>
            public int MeltingBackPrec { get; set; }
            /// <summary>
            /// 生星级经验消耗铜钱系数
            /// </summary>
            public int ExpCoinModulus { get; set; }
        }
        /// <summary>
        /// 装备配置
        ///  </summary>
        public static EquipCfg EquipCfgData
        {
            get
            {
                var key = "EquipCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<EquipCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (EquipCfg)obj;
            }
        }
        #endregion

        #region 竞技场功能配置
        public class PkCfg
        {
            /// <summary>
            /// 竞技场Npc个数
            /// </summary>
            public int NpcNum { get; set; }
            /// <summary>
            /// 最大挑战次数
            /// </summary>
            public int MaxChallengeNum { get; set; }
            /// <summary>
            /// 清除挑战冷却时间价格（元宝）
            /// </summary>
            //public int ClearTimeMoney { get; set; }
            /// <summary>
            /// 排名间隔列表
            /// </summary>
            public List<List<int>> RangePrecsList { get; set; }
            /// <summary>
            /// 冷却时间（秒）
            /// </summary>
            public int CoolTime { get; set; }
            /// <summary>
            /// 最多存储的记录数量
            /// </summary>
            public int MaxRecordNum { get; set; }
            /// <summary>
            /// 获取的最大排名名次
            /// </summary>
            public int GetMaxRankNum { get; set; }
            /// <summary>
            /// 最多累计的星星数量
            /// </summary>
            public int MaxStarNum { get; set; }
            /// <summary>
            /// 胜利武将经验系数
            /// </summary>
            public int WinHeroExpModulus { get; set; }
        }
        /// <summary>
        /// 竞技场功能配置信息
        ///  </summary>
        public static PkCfg PkCfgData
        {
            get
            {
                var key = "PkCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<PkCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (PkCfg)obj;
            }
        }
        #endregion

        #region 武将进阶所需碎片配置
        public class HeroAdvancedCfg
        {
            /// <summary>
            /// 进阶星级
            /// </summary>
            public int UpgradeStar { get; set; }
            /// <summary>
            /// 碎片数量
            /// </summary>
            public int ChipNum { get; set; }
        }
        /// <summary>
        /// 武将升星所需碎片配置
        ///  </summary>
        public static List<HeroAdvancedCfg> HeroAdvancedCfgData
        {
            get
            {
                var key = "HeroAdvancedCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<List<HeroAdvancedCfg>>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (List<HeroAdvancedCfg>)obj;
            }
        }
        #endregion

        #region 装备合成所需碎片配置
        public class EquipMixtureCfg
        {
            /// <summary>
            /// 星级
            /// </summary>
            public ItemQuality Type { get; set; }
            /// <summary>
            /// 碎片数量
            /// </summary>
            public int ChipNum { get; set; }
            /// <summary>
            /// 经验值
            /// </summary>
            public int Exp { get; set; }
        }
        /// <summary>
        /// 装备合成所需碎片配置
        ///  </summary>
        public static List<EquipMixtureCfg> EquipMixtureCfgData
        {
            get
            {
                var key = "EquipMixtureCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<List<EquipMixtureCfg>>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (List<EquipMixtureCfg>)obj;
            }
        }
        #endregion

        #region 最后一次执行竞技场每日奖励的时间
        public class DayRewardDateTime
        {
            /// <summary>
            /// 最后一次执行竞技场每日奖励的时间
            /// </summary>
            public DateTime LastDateTime { get; set; }
            /// <summary>
            /// 最后一次执行竞技场每日奖励的任务id
            /// </summary>
            public int TaskId { get; set; }
        }
        /// <summary>
        /// 最后一次执行竞技场每日奖励的时间
        ///  </summary>
        public static DayRewardDateTime DayRewardDateTimeData
        {
            get
            {
                var key = "DayRewardDateTime";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    var temp = new DayRewardDateTime();
                    temp.LastDateTime = DateTime.MinValue;
                    GameCache.Put(key, temp);
                    obj = temp;
                }
                return (DayRewardDateTime)obj;
            }
        }

        /// <summary>
        /// 设置最后一次执行竞技场每日奖励的时间
        /// </summary>
        /// <param name="dayRewardDateTime"></param>
        public static void SetDayRewardDateTimeData(DayRewardDateTime dayRewardDateTime)
        {
            var key = "DayRewardDateTime";
            GameCache.Put(key, dayRewardDateTime);
        }
        #endregion

        #region 建筑配置
        public class BuildingCfg
        {
            /// <summary>
            /// 初始铜钱容量
            /// </summary>
            public int InitCoinCapacity { get; set; }
            /// <summary>
            /// 初始木材容量
            /// </summary>
            public int InitWoodCapacity { get; set; }
            /// <summary>
            /// 初始石头容量
            /// </summary>
            public int InitStoneCapacity { get; set; }
            /// <summary>
            /// 初始铁矿容量
            /// </summary>
            public int InitIronCapacity { get; set; }
            /// <summary>
            /// 翻牌得到的好感度
            /// </summary>
            public int FlopFavor { get; set; }
            /// <summary>
            /// 每几分钟
            /// </summary>
            public int ProduceMinute { get; set; }
            /// <summary>
            /// 每5分钟生产得到的好感度
            /// </summary>
            public int ProduceFavor { get; set; }
            /// <summary>
            /// 每天免费翻牌次数
            /// </summary>
            public int FlopNum { get; set; }
            ///// <summary>
            ///// 初始铜钱
            ///// </summary>
            //public int InitCoin { get; set; }
            ///// <summary>
            ///// 初始木材
            ///// </summary>
            //public int InitWood { get; set; }
            ///// <summary>
            ///// 初始石头
            ///// </summary>
            //public int InitStone { get; set; }
            ///// <summary>
            ///// 初始铁矿
            ///// </summary>
            //public int InitIron { get; set; }
        }

        /// <summary>
        /// 建筑配置
        ///  </summary>
        public static BuildingCfg BuildingCfgData
        {
            get
            {
                var key = "BuildingCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<BuildingCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (BuildingCfg)obj;
            }
        }
        #endregion

        #region 大地图配置
        public class BigMapCfg
        {
            /// <summary>
            /// 城池最多匹配的人数
            /// </summary>
            public int CityMaxMatchCount { get; set; }
            /// <summary>
            /// 匹配玩家到领地的概率
            /// </summary>
            public int MatchUserPrec { get; set; }
            /// <summary>
            /// 领地匹配玩家或者NPC间隔小时数
            /// </summary>
            public int MatchIntervalHour { get; set; }
            /// <summary>
            /// 征收领地&城池资源间隔小时数
            /// </summary>
            public int GainIntervalHour { get; set; }
            /// <summary>
            /// 被玩家占领扣的声望-10
            /// </summary>
            public int BeRobedRepute { get; set; }
            /// <summary>
            /// 攻击玩家获得的声望20
            /// </summary>
            public int AttackUserRepute { get; set; }
            /// <summary>
            /// 攻击NPC获得的声望10
            /// </summary>
            public int AttackNpcRepute { get; set; }
            /// <summary>
            /// 侦查保存的时间（分钟）
            /// </summary>
            public int InvestigateSaveSenconds { get; set; }
            /// <summary>
            /// 抢夺建筑百分比
            /// </summary>
            public int RobBuildPrec { get; set; }
            /// <summary>
            /// 抢夺妃子百分比
            /// </summary>
            public int RobConcubinePrec { get; set; }
            /// <summary>
            /// 侦查铜钱系数
            /// </summary>
            public int InvestigateCoinModulus { get; set; }
            /// <summary>
            /// 城池升级所加的战斗力 TODO:后续是多个值
            /// </summary>
            public int UpgradeAddCombat { get; set; }
            /// <summary>
            /// 等级列表
            /// </summary>
            public List<int> LevelList { get; set; }
            /// <summary>
            /// 英雄个数列表
            /// </summary>
            public List<int> HeroNumList { get; set; }
            /// <summary>
            /// 初始化的领地ID列表
            /// </summary>
            public List<int> InitDomainIdList { get; set; }
            /// <summary>
            /// 初始化的城池ID列表
            /// </summary>
            public List<int> InitCityIdList { get; set; }
            /// <summary>
            /// 事件保存的天数
            /// </summary>
            public int EventSaveDay { get; set; }
            /// <summary>
            /// 事件保存的数量
            /// </summary>
            public int EventSaveNum { get; set; }
            /// <summary>
            /// 战斗所需精力
            /// </summary>
            public int BattleNeedSp { get; set; }
            /// <summary>
            /// 战斗武将经验系数
            /// </summary>
            public int BattleHeroExpModulus { get; set; }
            /// <summary>
            /// 抢劫的资源系统赠送系数
            /// </summary>
            public int MinRobResModulus { get; set; }
            /// <summary>
            /// 每天最多可以征收领地资源的个数
            /// </summary>
            public int MaxGainDomainResNum { get; set; }
        }

        /// <summary>
        /// 加速CD价格配置【每元宝加速的秒数】
        ///  </summary>
        public static BigMapCfg BigMapCfgData
        {
            get
            {
                var key = "BigMapCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<BigMapCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (BigMapCfg)obj;
            }
        }
        #endregion

        #region 任务配置
        public class TaskCfg
        {
            /// <summary>
            /// 获得体力时间列表
            /// </summary>
            public int GainSpTime { get; set; }
        }

        /// <summary>
        /// 任务配置
        ///  </summary>
        public static TaskCfg TaskCfgData
        {
            get
            {
                var key = "TaskCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<TaskCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (TaskCfg)obj;
            }
        }
        #endregion

        #region 新手引导获取到的ID（武将、妃子、道具）列表
        /// <summary>
        /// 新手引导取到的ID（武将、妃子、道具）列表
        ///  </summary>
        public static List<int> GuideGetIdList
        {
            get
            {
                var key = "GuideGetIdList";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<List<int>>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (List<int>)obj;
            }
        }
        #endregion

        #region 开启的模块等级配置
        public class OpenCfgItem
        {
            /// <summary>
            /// 模块类型
            /// </summary>
            public OpenModuleType Type { get; set; }
            /// <summary>
            /// 开启等级
            /// </summary>
            public int Level { get; set; }
        }
        /// <summary>
        /// 新手引导取到的ID（武将、妃子、道具）列表
        ///  </summary>
        public static List<OpenCfgItem> OpenCfgsData
        {
            get
            {
                var key = "OpenCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<List<OpenCfgItem>>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (List<OpenCfgItem>)obj;
            }
        }
        #endregion

        #region 清除CD配置 每元宝加速的秒数/固定花费的元宝数
        public class ClearCdCfg
        {
            /// <summary>
            /// 竞技场
            /// </summary>
            public int Pk { get; set; }
            /// <summary>
            /// 建筑升级
            /// </summary>
            public int BuildingUpgrade { get; set; }
            /// <summary>
            /// 晋封
            /// </summary>
            public int ConcubineJinFeng { get; set; }
            /// <summary>
            /// 寻访
            /// </summary>
            public int HeroXunFang { get; set; }
        }

        /// <summary>
        /// 加速CD价格配置【每元宝加速的秒数/固定花费的元宝数】
        ///  </summary>
        public static ClearCdCfg ClearCdCfgData
        {
            get
            {
                var key = "ClearCdCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<ClearCdCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (ClearCdCfg)obj;
            }
        }
        #endregion

        #region 大地图城防配置
        public class BigMapDefenseCfg
        {
            /// <summary>
            /// 内政每次加的点数
            /// </summary>
            public int EachAdd { get; set; }
            /// <summary>
            /// 每天降低的治安数
            /// </summary>
            public int DescendingZhiAn { get; set; }
            /// <summary>
            /// 战斗时降低的城防、军备
            /// 死亡2个武将 降低5点城防和军备
            /// 死亡3个武将 降低8点城防和军备
            /// 死亡4个武将 降低10点城防和军备
            /// </summary>
            public List<int> DescendingDefenseArmyList { get; set; }
            /// <summary>
            /// 点数的配置
            /// </summary>
            public int EachPoint { get; set; }
            /// <summary>
            /// 增加城防的点数
            /// </summary>
            public int AddDefense { get; set; }
            /// <summary>
            /// 降低城防的点数
            /// </summary>
            public int DescendingDefense { get; set; }
            /// <summary>
            /// 增加军备的点数
            /// </summary>
            public int AddArmy { get; set; }
            /// <summary>
            /// 降低军备的点数
            /// </summary>
            public int DescendingArmy { get; set; }
            /// <summary>
            /// 增加怒气的点数
            /// </summary>
            public int AddMorale { get; set; }
            /// <summary>
            /// 内政一次需要的体力
            /// </summary>
            public int NeedSp { get; set; }
        }
        /// <summary>
        /// 加速CD价格配置【每元宝加速的秒数/固定花费的元宝数】
        ///  </summary>
        public static BigMapDefenseCfg BigMapDefenseCfgData
        {
            get
            {
                var key = "BigMapDefenseCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<BigMapDefenseCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (BigMapDefenseCfg)obj;
            }
        }
        #endregion

        #region 兑换码配置
        public class RedCodeCfg
        {
            /// <summary>
            /// 兑换码类型对应的道具列表
            /// </summary>
            public List<RedCodeItem> Items { get; set; }
        }
        public class RedCodeItem
        {
            /// <summary>
            /// 兑换码类型
            /// </summary>
            public RedCodeType Type { get; set; }
            /// <summary>
            /// 对应的道具id
            /// </summary>
            public int ToolId { get; set; }
        }
        /// <summary>
        /// 兑换码配置
        ///  </summary>
        public static RedCodeCfg RedCodeCfgData
        {
            get
            {
                var key = "RedCodeCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<RedCodeCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (RedCodeCfg)obj;
            }
        }
        #endregion

        #region 竞技场宝箱奖励
        public class PkBoxCfg
        {
            /// <summary>
            /// 元宝基数
            /// </summary>
            public int Money { get; set; }
            /// <summary>
            /// 元宝倍数
            /// </summary>
            public int Mnum { get; set; }
            /// <summary>
            /// 铜钱基数
            /// </summary>
            public int Coin { get; set; }
            /// <summary>
            /// 铜钱倍数
            /// </summary>
            public int Cnum { get; set; }
            /// <summary>
            /// 道具基数
            /// </summary>
            public int ToolId { get; set; }
            /// <summary>
            /// 道具倍数
            /// </summary>
            public int Tnum { get; set; }
        }
        /// <summary>
        /// 竞技场宝箱奖励
        ///  </summary>
        public static PkBoxCfg PkBoxCfgData
        {
            get
            {
                var key = "PkBoxCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<PkBoxCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (PkBoxCfg)obj;
            }
        }
        #endregion

        #region 妃子训练配置
        public class TrainConcubineCfg
        {
            /// <summary>
            /// 妃子训练配置列表
            /// </summary>
            public List<TrainConcubineItem> Items { get; set; }
        }
        public class TrainConcubineItem
        {
            /// <summary>
            /// 训练位置编号
            /// </summary>
            [Tag(1)]
            public int Location { get; set; }
            /// <summary>
            /// 开启所需主公等级
            /// </summary>
            [Tag(2)]
            public int NeedLevel { get; set; }
            /// <summary>
            /// 开启所需vip等级
            /// </summary>
            [Tag(3)]
            public int NeedVip { get; set; }
        }
        /// <summary>
        /// 妃子训练配置
        ///  </summary>
        public static TrainConcubineCfg TrainConcubineCfgData
        {
            get
            {
                var key = "TrainConcubineCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<TrainConcubineCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (TrainConcubineCfg)obj;
            }
        }
        #endregion


        #region  用户采花配置

        //{Count:5,ListCount:6,CoolTime:600,RecordCount:50}
        public class DeflowCfg
        {
            /// <summary>
            /// 采花次数
            /// </summary>
            public int Count { get; set; }

            /// <summary>
            /// 采花人数
            /// </summary>
            public int ListCount { get; set; }

            /// <summary>
            /// 冷却时间
            /// </summary>
            public int CoolTime { get; set; }

            /// <summary>
            /// 记录条数
            /// </summary>
            public int RecordCount { get; set; }
        }

        /// <summary>
        /// 获取采花配置
        /// </summary>
        public static DeflowCfg DeflowCfgData
        {
            get
            {
                var key = "DeflowerCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<DeflowCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (DeflowCfg)obj;
            }
        }

        #endregion


        #region 生命树配置
        public class TreeCfg
        {
            public class PlantCfg
            {   
                /// <summary>
                /// 肥料ID
                /// </summary>
                public int Id { get; set; }
                
                /// <summary>
                /// 产量
                /// </summary>
                public int Product { get; set; }

                /// <summary>
                /// 消耗元宝
                /// </summary>
                public int CostMoney { get; set; }

                /// <summary>
                /// 获得概率
                /// </summary>
                public int Pro { get; set; }
            }

            /// <summary>
            /// 肥料列表
            /// </summary>
            public List<PlantCfg> Plants { get; set; }

            /// <summary>
            /// 收获过程时间
            /// </summary>
            public int Time { get; set; }

            /// <summary>
            /// 多少秒消耗一元宝
            /// </summary>
            public int Money { get; set; }
        }

        /// <summary>
        /// 生命书配置
        /// </summary>
        public static TreeCfg TreeData
        {
            get
            {
                var key = "TreeCfg";
                var obj = GameCache.Get(key);
                if (obj == null)
                {
                    obj = JsonConvert.DeserializeObject<TreeCfg>(ParamHelper.Get<string>(key));
                    GameCache.Put(key, obj);
                }
                return (TreeCfg)obj;
            }
        }


        
        #endregion

    }
}
