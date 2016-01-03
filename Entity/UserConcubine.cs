using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Common;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 妃子实例
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserConcubineItem : EntityItem
    {
        /// <summary>
        /// 系统妃子ID
        /// </summary>
        [ProtoMember(1)]
        public int ConcubineId { get; set; }
        /// <summary>
        /// 妃子等级
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }
        /// <summary>
        /// 当前好感度【用于封号】
        /// </summary>
        [ProtoMember(3)]
        public int Favor { get; set; }
        /// <summary>
        /// 产量
        /// </summary>
        [ProtoMember(4)]
        public int Product { get; set; }
        /// <summary>
        /// 携带量
        /// </summary>
        //[ProtoMember(5)]
        //public int Carry { get; set; }
        /// <summary>
        /// 经验【用于升星】
        /// </summary>
        //[ProtoMember(6)]
        //public int Exp { get; set; }
        /// <summary>
        /// 妃子封号等级
        /// </summary>
        [ProtoMember(7)]
        public int TitleLevel { get; set; }
        /// <summary>
        /// 还未收获但已经生产出来的量【妃子升级或者封号 改变了生产速度的时候 系统立即收获一次】
        /// </summary>
        [ProtoMember(8)]
        public int OutPut { get; set; }
        /// <summary>
        /// 最后一次后台/真正收获的时间
        /// </summary>
        [ProtoMember(9)]
        public DateTime LastGainTime { get; set; }

        /// <summary>
        /// 妃子状态
        /// </summary>
        [ProtoMember(10)]
        public ConcubineStatus Status { get; set; }

        /// <summary>
        /// 生产周期（分钟）
        /// 周期=（初始时间+（lev-1）*30）
        /// </summary>
        //[ProtoMember(11)]
        public int ProductPeriod { get { return SysConcubineCfg.ProductPeriod + (Level - 1) * 30; } }

        /// <summary>
        /// 入住的宫殿的生产的资源类型
        /// </summary>
        [ProtoMember(12)]
        public MoneyType BuildingMoneyType { get; set; }
        /// <summary>
        /// 最后一次点击收获的时间
        /// </summary>
        //[ProtoMember(13)]
        //public DateTime RealGainTime { get; set; }
        /// <summary>
        /// 升级的截止时间
        /// </summary>
        [ProtoMember(14)]
        public DateTime UpgradeEndTime { get; set; }
        /// <summary>
        /// 升级训练所在的位置（从1开始）
        /// </summary>
        [ProtoMember(15)]
        public int TrainLocation { get; set; }
        /// <summary>
        /// 训练状态
        /// </summary>
        [ProtoMember(16)]
        public ConcubineTrainStatus TrainStatus { get; set; }

        /// <summary>
        /// 特长对应的资源类型
        /// </summary>
        public MoneyType MoneyType { get { return SysConcubineCfg.MoneyType; } }
        /// <summary>
        /// 可以收获的时间点
        /// </summary>
        //public DateTime CanGainTime { get { return RealGainTime.AddMinutes(ProductPeriod); } }

        /// <summary>
        /// 宫殿里面的生产速度（每小时）
        /// </summary>
        public int RealProduct
        {
            get
            {
                if (Level == 0) Level = 1;
                var growth = 0;
                //if (BuildingMoneyType == MoneyType) growth = SysConcubineCfg.Growth;
                growth = SysConcubineCfg.Growth;
                //妃子产量 = 等级*10*妃子天赋
                //var product = growth * 10 * Level;//(int)(Product * 1.0 / 100 * (100 + growth));
                //单位产量=初始产量+（lev-1）*10*天赋
                //初始产量+（lev-1）*5*天赋  modify by hql at 2015.11.26 
                var product = SysConcubineCfg.InitProduct + (Level - 1) * 5 * growth;

                //晋封添加的产量
                if (TitleLevel > 1)
                {
                    var syConcubineTitleCfg = SysConcubineTitleCfg.Find(o => o.Id == TitleLevel - 1);
                    if (syConcubineTitleCfg == null)
                        throw new ApplicationException(string.Format("Not Find SysConcubineTitleCfg:Id{0}", TitleLevel - 1));

                    product = (int)(product * (1 + syConcubineTitleCfg.AddProduct * 1.0 / 100));
                }

                if (product != Product)
                    Product = product;
                return product;
            }
        }
        /// <summary>
        /// 总好感度
        /// </summary>
        public int TotalFavor 
        {
            get
            {
                //加上碎片的好感度，不用玩家自己使用
                var chipFavor = 0;
                var userChip = DataStorage.Current.Load<UserChip>(Pid);
                var userChipItem =
                    userChip.ChipItems.FirstOrDefault(o => o.ItemId == ConcubineId + (int) ToolType.ConcubineChip);
                if (userChipItem != null)
                {
                    var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == userChipItem.ItemId);
                    if (sysToolCfg != null)
                    {
                        chipFavor = userChipItem.Num*sysToolCfg.Param3;
                    }
                }
                return Favor + chipFavor;
            } 
        }

        /// <summary>
        /// 妃子特长加成的产量
        /// </summary>
        public int AddProduct
        {
            get
            {
                return 0;
                var growth = SysConcubineCfg.Growth;
                var product = (int)(Product * 1.0 * growth / 100);
                return product;
            }
        }

        /// <summary>
        /// 获取入住哪类型宫殿的产量
        /// </summary>
        /// <param name="moneyType"></param>
        public int GetAddProduct(MoneyType moneyType)
        {
            return 0;
            var growth = 0;
            if (MoneyType.Equals(moneyType)) growth = SysConcubineCfg.Growth;
            else return 0;
            var product = (int)(Product * 1.0 / 100 * (100 + growth));
            return product - Product;
        }

        /// <summary>
        /// 刷新属性【升级、升星】
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="type">0:升级，1：升星</param>
        public void RefreshProperties(int opCode, int type = 0)
        {
            //var cfg = SysConcubineCfg;
            //if (cfg == null) return;
            ////var sysConcubineTitleList =
            ////    SysConcubineTitleCfg.Items.Where(o => o.Id < TitleLevel)
            ////        .OrderByDescending(o => o.NeedLevel)
            ////        .ToList();
            //var syConcubineTitleCfg = SysConcubineTitleCfg.Find(o => o.Id == TitleLevel);
            //if (syConcubineTitleCfg == null)
            //    throw new ApplicationException(string.Format("Not Find SysConcubineTitleCfg:Id{0}", TitleLevel));

            //收获资源到妃子身上
            if (Status == ConcubineStatus.Produce)
                GainToOutput(opCode);

            //初始、等级加成、封号加成
            Product = RealProduct; //(int)(cfg.InitProduct * (1 + syConcubineTitleCfg.AddProduct * 1.0 / 100));
            //Carry = cfg.InitCarry + (Level - 1) * cfg.Growth * 10 * 3 + sysConcubineTitleList.Sum(o => o.AddCarry);
        }

        /// <summary>
        /// 妃子的生成速度有变化时，收获到妃子身上。[升级、升星操作时候调用]
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="totalMinutes"></param>
        public void GainToOutput(int opCode, double totalMinutes = 0.0)
        {
            var product = RealProduct;
            double minute = totalMinutes;
            if (minute.Equals(0.0))
            {
                minute = DateTime.Now.Subtract(LastGainTime).TotalMinutes;
                if (minute > ProductPeriod) minute = ProductPeriod;
            }
            if (minute > ProductPeriod) minute = ProductPeriod;
            //int outPut = (int)(product * 1.0 * minute / 60);
            //if (outPut < 0) return;
            //OutPut += outPut;
            //var carry = (int)(product * 1.0 * ProductPeriod / 60);
            //int outPut = (int)(product * 1.0 * minute / ProductPeriod);
            int outPut = (int)(product * 1.0 * minute / 60);
            if (outPut < 0) return;
            OutPut += outPut;
            //if (OutPut > product) OutPut = product;

            //加速状态则不需要设置该值
            if (totalMinutes.Equals(0.0)) LastGainTime = DateTime.Now;

            AddProduceFavor(minute, opCode);
        }

        /// <summary>
        /// 添加生产好感度
        /// </summary>
        /// <param name="minute"></param>
        /// <param name="opCode"></param>
        public void AddProduceFavor(double minute, int opCode)
        {
            var produceMinute = ConfigHelper.BuildingCfgData.ProduceMinute;
            var produceFavor = ConfigHelper.BuildingCfgData.ProduceFavor;

            var num = (int)minute / produceMinute;
            var favor = produceFavor * num;

            Favor += favor;
            //AddFavor(favor, opCode);
            //Utility.AddResource(null, ItemType.Favor, opCode, favor, Pid);
        }

        ///// <summary>
        ///// 添加好感度
        ///// </summary>
        ///// <param name="favor"></param>
        ///// <param name="opCode"></param>
        //public void AddFavor(int favor, int opCode)
        //{
        //    //妃子等级不能大于主公等级
        //    var userRole = Storage.Load<UserRole>(Pid);
        //    if (userRole.Level < Level) return;

        //    //已经最等级了 经验加到满则终止
        //    var highestCfg = SysConcubineFavorCfg.Items.OrderByDescending(o => o.Id).FirstOrDefault();
        //    if (highestCfg != null && Level == highestCfg.Id)
        //    {
        //        //已经满级了
        //        Favor = 0;
        //        //Favor += favor;
        //        //if (Favor > highestCfg.Favor) Favor = highestCfg.Favor;
        //        return;
        //    }

        //    int curlevel = Level;
        //    int curfavor = Favor + favor;
        //    var cfg = SysConcubineFavorCfg.Items.FirstOrDefault(o => o.Id == curlevel + 1);
        //    while (cfg != null && curfavor >= cfg.Favor)
        //    {
        //        if (curlevel >= SysConcubineFavorCfg.Items.Max(o => o.Id)) break;
        //        curfavor = curfavor - cfg.Favor;
        //        if (curlevel + 1 < userRole.Level)
        //        {
        //            cfg = SysConcubineFavorCfg.Items.FirstOrDefault(o => o.Id == curlevel + 1);
        //            curlevel += 1;
        //        }
        //        else
        //        {
        //            //等级不能超过主公等级——满经验但是不能升级
        //            curfavor = cfg.Favor;
        //            break;
        //        }
        //    }
        //    //前后等级有改变则计算下数据
        //    if (curlevel != Level) RefreshProperties(opCode);

        //    var startLevel = Level;
        //    Level = curlevel;
        //    var endLevel = Level;
        //    Favor = curfavor;

        //    //已经最等级了 经验加到满则终止
        //    if (highestCfg != null && Level == highestCfg.Id)
        //    {
        //        if (Favor > highestCfg.Favor) Favor = highestCfg.Favor;
        //    }

        //    GameLogManager.ItemLog(Pid, Id, favor, opCode, (int)ItemType.Favor, startLevel, endLevel);
        //}

        /// <summary>
        /// 收获资源到用户身上[妃子收获及妃子休息操作时候调用]
        /// </summary>
        /// <param name="opCode">接口ID</param>
        /// <param name="isForcibly">是否强制收获——妃子休息</param>
        /// <returns></returns>
        public int GainToUser(int opCode, int isForcibly = 0)
        {
            GainToOutput(opCode);

            if (isForcibly == 1)
            {
                //强制收获——妃子休息，只能收获50%
                OutPut = (int)(OutPut * 0.5);
            }

            var outPut = OutPut;

            var userRole = Storage.Load<UserRole>(Pid, true);
            var realNum = outPut;
            switch (BuildingMoneyType)
            {
                case MoneyType.Coin:
                    realNum = Utility.AddResource(userRole, ItemType.Coin, opCode, OutPut); break;
                case MoneyType.Wood:
                    realNum = Utility.AddResource(userRole, ItemType.Wood, opCode, OutPut); break;
                case MoneyType.Stone:
                    realNum = Utility.AddResource(userRole, ItemType.Stone, opCode, OutPut); break;
                case MoneyType.Iron:
                    realNum = Utility.AddResource(userRole, ItemType.Iron, opCode, OutPut); break;
            }
            OutPut = outPut - realNum;
            //RealGainTime = DateTime.Now;
            return realNum;
        }

        /// <summary>
        /// 妃子配置
        /// </summary>
        public SysConcubineCfg SysConcubineCfg
        {
            get
            {
                var cfg = SysConcubineCfg.Find(ConcubineId);
                if (cfg == null)
                {
                    Utility.ClearRdata(Id);
                    //throw new ApplicationException(string.Format("SysConcubineCfg:Id:{0} NOT FIND", ConcubineId));
                }
                return cfg;
            }
        }

        /// <summary>
        /// 碎片数
        /// </summary>
        public int ChipNum
        {
            get
            {
                var userChip = Storage.Load<UserChip>(Pid, true);
                var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == SysConcubineCfg.ChipId);
                if (chipItem != null) return chipItem.Num;
                return 0;
            }
        }

        public override void NewObjectInit()
        {
            TitleLevel = 1;
            LastGainTime = DateTime.Now;
            //RealGainTime = DateTime.Now;
        }

        public override void LoadInit()
        {
            var cfg = SysConcubineCfg;
            if (cfg == null) return;
            if (TitleLevel == 0) TitleLevel = 1;
            if (Product == 0) RefreshProperties(0);
            //if (ProductPeriod == 0) ProductPeriod = cfg.ProductPeriod;
            if (TitleLevel == 0) TitleLevel = 1;

            //if (RealGainTime < DateTime.Now.AddDays(-30))
            //    RealGainTime = LastGainTime;

            if (Level == 0)
            {
                Level = 1;
            }

            if (UpgradeEndTime < DateTime.Now.AddDays(-30))
            {
                UpgradeEndTime = DateTime.Now;
            }


        }
    }

    /// <summary>
    /// 用户妃子
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserConcubine : KVEntity
    {
        /// <summary>
        /// 妃子实例——对内
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserConcubineItem> Items { get; set; }

        /// <summary>
        /// 今日翻牌次数
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue FlopNum { get; set; }

        /// <summary>
        /// 今日已购买的重置翻牌次数
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuyFlopNum { get; set; }

        /// <summary>
        /// 晋封CD时间
        /// </summary>
        [ProtoMember(4)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 晋封妃子ID
        /// </summary>
        [ProtoMember(5)]
        public int JinFengConcubineId { get; set; }

        /// <summary>
        /// 今日已使用翻牌次数
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue UseFlopNum { get; set; }

        /// <summary>
        /// 剩余翻牌次数
        /// </summary>
        public int LaveFlopNum
        {
            get
            {
                //return (int)(ConfigHelper.BuildingCfgData.FlopNum + BuyFlopNum.Value - FlopNum.Value);
                return 3 - (int)UseFlopNum.Value;
            }
        }

        /// <summary>
        /// 妃子实例—对外
        /// </summary>
        public List<UserConcubineItem> ItemList
        {
            get
            {
                foreach (var userConcubineItem in Items)
                {
                    userConcubineItem.LoadInit();
                }
                return Items;
            }
        }

        /// <summary>
        /// 翻牌
        /// </summary>
        public void Flop()
        {
            //if (ConfigHelper.BuildingCfgData.FlopNum <= FlopNum.Value)
            //{
            //    BuyFlopNum -= 1;
            //}
            //else
            //{
            //    FlopNum += 1;
            //}
            UseFlopNum += 1;

            //添加翻牌次数
            Utility.AddDailyTaskGoalData(Id, DailyType.Flop);
        }

        public override void NewObjectInit()
        {
            Items = new List<UserConcubineItem>();
            foreach (var userConcubineItem in Items)
            {
                userConcubineItem.NewObjectInit();
            }
            EndTime = Util.UnixEpochDateTime;
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserConcubineItem>();
            foreach (var userConcubineItem in Items)
            {
                userConcubineItem.LoadInit();
            }
            if (EndTime < new DateTime(1970, 1, 1))
                EndTime = Util.UnixEpochDateTime;
        }

        /// <summary>
        /// 改变后宫新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeNewMsg(UserRole userRole = null)
        {
            //后宫的红点去掉，因为随时可以进入后宫收获资源，红点没必要了 modify by hql at 2015.10.09
            return;
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var hasNewMsg = HasNewMsg();
            var openCfgItem = ConfigHelper.OpenCfgsData.FirstOrDefault(o => o.Type == OpenModuleType.Harem);
            if (openCfgItem == null || userRole.Level >= openCfgItem.Level)
                userRole.SetHasNewMsg((int)NewMsgType.Harem, hasNewMsg);
        }

        /// <summary>
        /// 是否有新消息
        /// </summary>
        /// <returns></returns>
        public int HasNewMsg()
        {
            //var hasNewMsg = Items.Count(o => o.CanGainTime.ToTs() <= 0 && o.Status == ConcubineStatus.Produce) > 0
            //    ? 1
            //    : 0;

            return 0;
        }

        /// <summary>
        /// 添加妃子
        /// </summary>
        /// <param name="concubineId">妃子Id</param>
        /// <param name="opCode">接口id</param>
        /// <returns></returns>
        public UserConcubineItem AddConcubineToUser(int concubineId, int opCode)
        {
            var sysConcubineCfg = SysConcubineCfg.Find(concubineId);
            if (sysConcubineCfg == null) throw new ApplicationException(string.Format("SysConcubineCfg:Id:{0} NOT FIND", concubineId));

            //保证唯一性
            var userConcubineItem = Items.FirstOrDefault(o => o.ConcubineId == concubineId);
            if (userConcubineItem != null)
            {
                //throw new ApplicationException(string.Format("妃子Id:{0}已经存在", concubineId));
                return null;
            }

            //添加新妃子
            userConcubineItem = KVEntity.CreateNew<UserConcubineItem>();
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysConcubineCfg, UserConcubineItem>(
                    new DefaultMapConfig().IgnoreMembers<SysConcubineCfg, UserConcubineItem>(new[] { "Id" }));
            mapper.Map(sysConcubineCfg, userConcubineItem);
            userConcubineItem.ConcubineId = sysConcubineCfg.Id;
            userConcubineItem.Pid = Id;
            userConcubineItem.Level = 1;
            userConcubineItem.TitleLevel = 1;
            userConcubineItem.Product = sysConcubineCfg.InitProduct;
            //userConcubineItem.ProductPeriod = sysConcubineCfg.ProductPeriod;
            userConcubineItem.UpgradeEndTime = DateTime.Now;
            //userConcubineItem.Carry = sysConcubineCfg.InitCarry;
            Items.Add(userConcubineItem);

            //添加任务新达成
            Utility.AddMainLineTaskGoalData(Id, MainLineType.RecruitConcubineNum, Items.Count);

            if (sysConcubineCfg.Quality >= ItemQuality.Purple)
            {
                var userRole = Storage.Load<UserRole>(Id);
                //广播
                var msg = "";
                var color = Utility.GetQualityColor(sysConcubineCfg.Quality);
                switch (opCode)
                {
                    case 4004:
                        //合成
                        msg = LangResource.GetLangResource(ResourceId.R_0000_MixtureConcubineMsg, userRole.Id,
                                                             userRole.NickName, sysConcubineCfg.Id, sysConcubineCfg.Name, color); break;
                    case 5001:
                        //寻访
                        msg = LangResource.GetLangResource(ResourceId.R_0000_ExtractConcubineMsg, userRole.Id,
                                                             userRole.NickName, sysConcubineCfg.Id, sysConcubineCfg.Name, color); break;
                }
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
            }

            GameLogManager.ItemLog(Id, concubineId, 1, opCode, (int)ItemType.Concubine, 0, 0);
            return userConcubineItem;
        }

        public void ClearItems()
        {
            Items = new List<UserConcubineItem>();
        }
    }

    /// <summary>
    /// 妃子状态
    /// </summary>
    public enum ConcubineStatus
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 生产
        /// </summary>
        Produce = 1,
    }

    /// <summary>
    /// 妃子训练状态
    /// </summary>
    public enum ConcubineTrainStatus
    {
        /// <summary>
        /// 还未入住训练位置——未训练
        /// </summary>
        Idle = 0,
        /// <summary>
        /// 入住到训练位置——等待训练
        /// </summary>
        WaitTrain = 1,
        /// <summary>
        /// 入住到训练位置——训练中
        /// </summary>
        Train = 2,
    }
}
