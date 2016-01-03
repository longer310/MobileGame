// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserChip.cs
// 创建标识：2012/11/1 23:39:34 Created by 胡奇龙
// 功能说明：
// 注意事项：
// 
// 更新记录：
// -------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using MobileGame.Core;
using MobileGame.tianzi;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 副本关卡项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserLevelsItem : EntityItem
    {
        /// <summary>
        /// 关卡id
        /// </summary>
        [ProtoMember(1)]
        public int LevelId { get; set; }

        /// <summary>
        /// 关卡星级
        /// </summary>
        [ProtoMember(2)]
        public int LevelStar { get; set; }

        /// <summary>
        /// 今日购买次数
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue BuyNum { get; set; }

        /// <summary>
        /// 今日已使用次数
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue UseNum { get; set; }

        /// <summary>
        /// 总共购买的次数（统计使用）
        /// </summary>
        [ProtoMember(5)]
        public int BuyNums { get; set; }

        /// <summary>
        /// 剩余挑战次数
        /// </summary>
        public int LaveNum
        {
            get
            {
                return (int)(SysLevelCfg.Times - UseNum.Value);
            }
        }

        /// <summary>
        /// 关卡配置表
        /// </summary>
        public SysLevelCfg SysLevelCfg
        {
            get
            {
                var cfg = SysLevelCfg.Find(LevelId);
                if (cfg == null) throw new ApplicationException(string.Format("SysLevelCfg:Id[{0}]不存在", LevelId));
                return cfg;
            }
        }
    }

    /// <summary>
    /// 用户副本表
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserLevels : KVEntity
    {
        /// <summary>
        /// 关卡列表—对外
        /// </summary>
        //public List<UserLevelsItem> ItemList
        //{
        //    get
        //    {
        //        foreach (var userLevelsItem in Items)
        //        {
        //            userLevelsItem.LoadInit();
        //        }
        //        return Items;
        //    }
        //}

        /// <summary>
        /// 关卡列表【不能直接被访问，这里写成public是因为private属性没法同步到数据库。】
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserLevelsItem> Items { get; set; }

        /// <summary>
        /// 开启的最后一个关卡的Id
        /// </summary>
        [ProtoMember(3)]
        public int OpenedMaxLevelId { get; set; }

        /// <summary>
        /// 副本战斗信息列表
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<CommonBattleItem> BattleList { get; set; }

        /// <summary>
        /// 开启新的关卡
        /// </summary>
        /// <param name="sysLevelCfg"></param>
        public void OpenNewLevel(SysLevelCfg sysLevelCfg)
        {
            if (sysLevelCfg == null) return;
            if (sysLevelCfg.Times > 0 && !Items.Exists(o => o.LevelId == sysLevelCfg.Id))
            {
                AddLevelsItem(sysLevelCfg.Id);
            }
            if (OpenedMaxLevelId == 0) OpenedMaxLevelId = sysLevelCfg.Id;
            else if (sysLevelCfg.Id != OpenedMaxLevelId)
            {
                var lastLevelCfg = SysLevelCfg.Find(OpenedMaxLevelId);
                if (lastLevelCfg == null)
                    throw new ApplicationException(string.Format("普通副本关卡Id[{0}]不存在", OpenedMaxLevelId));
                if (lastLevelCfg.BattleId * 100 + lastLevelCfg.Index < sysLevelCfg.BattleId * 100 + sysLevelCfg.Index)
                {
                    //前进了
                    OpenedMaxLevelId = sysLevelCfg.Id;
                }
            }
            else
            {
                var nextLevelCfg =
                    SysLevelCfg.Items.FirstOrDefault(
                        o => o.BattleId == sysLevelCfg.BattleId && o.Index == sysLevelCfg.Index + 1);
                if (nextLevelCfg == null)
                {
                    nextLevelCfg =
                    SysLevelCfg.Items.FirstOrDefault(
                        o => o.BattleId == sysLevelCfg.BattleId + 1 && o.Index == 1);
                }
                if (nextLevelCfg == null)
                    return;
                    //throw new ApplicationException(string.Format("普通副本关卡BattleId[{0}]，Index[{1}]不存在",
                    //    sysLevelCfg.BattleId + 1, 1));
                OpenedMaxLevelId = nextLevelCfg.Id;
                if (nextLevelCfg.Times > 0 && !Items.Exists(o => o.LevelId == nextLevelCfg.Id))
                {
                    AddLevelsItem(nextLevelCfg.Id);
                }
            }
        }

        /// <summary>
        /// 添加项
        /// </summary>
        /// <param name="levelId"></param>
        /// <returns></returns>
        public UserLevelsItem AddLevelsItem(int levelId,int star = 0)
        {
            //Util.GetSequence(typeof (UserLevelsItem));
            var userLevelsItem = KVEntity.CreateNew<UserLevelsItem>();
            userLevelsItem.LevelId = levelId;
            userLevelsItem.LevelStar = star;
            Items.Add(userLevelsItem);

            return userLevelsItem;
        }

        /// <summary>
        /// 添加副本战役
        /// </summary>
        /// <param name="battleId"></param>
        public void AddBattle(int battleId)
        {
            var saveLevelsData = ConfigHelper.SaveLevelsData;
            if (saveLevelsData == 0)
            {
                //记录大于最多保存条数，先删除
                if (BattleList.Count >= 10)//ConfigHelper.PkCfgData.MaxRecordNum
                {
                    var removeId = BattleList[0].BattleId;

                    //从redis和数据库中删除战报
                    Utility.DelBattle(removeId);
                    BattleList.RemoveAt(0);
                }
            }
            BattleList.Add(new CommonBattleItem() { BattleId = battleId, CreateTime = DateTime.Now });
        }

        public void InitOpenLevelId()
        {
            var sysLevelCfg = SysLevelCfg.Items.FirstOrDefault(o => o.BattleId == 1 && o.Index == 1);
            if (sysLevelCfg == null)
                throw new ApplicationException(string.Format("普通副本关卡BattleId[{0}]，Index[{1}]不存在", 1, 1));

            OpenNewLevel(sysLevelCfg);
        }

        public override void NewObjectInit()
        {
            Items = new List<UserLevelsItem>();
            InitOpenLevelId();
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserLevelsItem>();

            if (OpenedMaxLevelId == 0)
            {
                InitOpenLevelId();
            }

            BattleList = BattleList ?? new List<CommonBattleItem>();
            var saveLevelsData = ConfigHelper.SaveLevelsData;
            if (saveLevelsData == 0)
            {
                BattleList = BattleList.OrderBy(o => o.CreateTime).ToList();
                for (int i = BattleList.Count - 1; i >= 0; i--)
                {
                    if (BattleList[i].CreateTime.AddDays(3) < DateTime.Now)
                    {
                        var removeId = BattleList[i].BattleId;
                        Utility.DelBattle(removeId);
                        BattleList.RemoveAt(i);
                    }
                }

                while (BattleList.Count >= 10)
                {
                    var removeId = BattleList[0].BattleId;
                    Utility.DelBattle(removeId);
                    BattleList.RemoveAt(0);
                }
            }
        }
    }

    [ProtoContract]
    public class CommonBattleItem
    {
        /// <summary>
        /// 战役id
        /// </summary>
        [ProtoMember(1)]
        public int BattleId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [ProtoMember(2)]
        public DateTime CreateTime { get; set; }
    }
}
