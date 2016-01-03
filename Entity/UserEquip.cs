// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserEquip.cs
// 创建标识：2012/11/1 23:39:34 Created by 胡奇龙
// 功能说明：
// 注意事项：
// 
// 更新记录：
// -------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.EmitInvoker;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 背包的装备项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserEquipItem : EntityItem
    {
        /// <summary>
        /// 物品Id
        /// </summary>
        [ProtoMember(2)]
        public int ItemId { get; set; }
        /// <summary>
        /// 等级——初始化为1级
        /// </summary>
        [ProtoMember(3)]
        public int Level { get; set; }
        /// <summary>
        /// 品阶——初始化为0阶
        /// </summary>
        //[ProtoMember(4)]
        //public int Rank { get; set; }
        /// <summary>
        /// 装备到的武将id(系统id)
        /// </summary>
        [ProtoMember(7)]
        public int HeroId { get; set; }

        /// <summary>
        /// 装备的星级
        /// </summary>
        [ProtoMember(8)]
        public int Star { get; set; }

        /// <summary>
        /// 装备当前的器魂
        /// </summary>
        [ProtoMember(9)]
        public int Exp { get; set; }

        #region 属性
        /// <summary>
        /// 生命值
        /// </summary>
        [ProtoMember(20)]
        public int Hp { get; set; }
        /// <summary>
        /// 物攻
        /// </summary>
        [ProtoMember(21)]
        public int Ad { get; set; }
        /// <summary>
        /// 魔攻
        /// </summary>
        [ProtoMember(22)]
        public int Ap { get; set; }
        /// <summary>
        /// 物防
        /// </summary>
        [ProtoMember(23)]
        public int AdArm { get; set; }
        /// <summary>
        /// 法术防御
        /// </summary>
        [ProtoMember(24)]
        public int ApArm { get; set; }
        #endregion

        /// <summary>
        /// 装备配置文件
        /// </summary>
        public SysEquipCfg SysEquipCfg
        {
            get
            {
                var cfg = SysEquipCfg.Find(ItemId);
                if (cfg == null) throw new ApplicationException(string.Format("装备Id[{0}]不存在", ItemId));
                return cfg;
            }
        }

        /// <summary>
        /// 熔炼可以提取的经验
        /// </summary>
        public int MeltingExp
        {
            get
            {
                var equipMixtureCfg = ConfigHelper.EquipMixtureCfgData.FirstOrDefault(o => o.Type == Quality);
                if (equipMixtureCfg == null)
                    throw new ApplicationException(string.Format("EquipMixtureCfgData Type[{0}]不存在", (int)Quality));
                var num = equipMixtureCfg.ChipNum <= 0 ? 1 : equipMixtureCfg.ChipNum;
                //装备本身的经验
                var exp = num * equipMixtureCfg.Exp;

                //装备升星的经验
                if (Star > 0)
                {
                    exp += ConfigHelper.EquipCfgData.MaxStarList.Take(Star).Sum();
                }

                //装备已吸收的经验
                exp += Exp;

                return exp;
            }
        }

        /// <summary>
        /// 装备可以升级到的星级
        /// </summary>
        public int MaxStar
        {
            get
            {
                if (Quality == ItemQuality.White) return 0;
                else
                {
                    var index = (int)Quality;
                    return ConfigHelper.EquipCfgData.MaxStarList[index];
                }
            }
        }

        /// <summary>
        /// 当前星级升级所需经验
        /// </summary>
        public int UpgradeExp
        {
            get
            {
                //if (Quality == ItemQuality.Blue) return 0;
                //else
                //{
                    var index = (int)Quality - 1;
                    return ConfigHelper.EquipCfgData.UpgradeStarExpList[index][Star];
                //}
            }
        }

        /// <summary>
        /// 售价
        /// </summary>
        public int SellPrice
        {
            get { return 0; }
        }

        /// <summary>
        /// 装备位置
        /// </summary>
        public EquipType EquipType
        {
            get { return SysEquipCfg.Type; }
        }

        /// <summary>
        /// 穿戴所需武将等级
        /// </summary>
        public int NeedLevel
        {
            get { return SysEquipCfg.NeedLevel; }
        }

        /// <summary>
        /// 装备品质
        /// </summary>
        public ItemQuality Quality
        {
            get { return SysEquipCfg.Quality; }
        }

        /// <summary>
        /// 刷新属性【装备升级、升阶】
        /// </summary>
        /// <param name="type">0:升级，1：升阶</param>
        public void RefreshProperties(int type = 0)
        {
            var cfg = SysEquipCfg;

            var addPrec = 0;

            if (Star > 0) addPrec = ConfigHelper.EquipCfgData.StarAddPrecList[Star - 1];
            Hp = cfg.Hp + (int)((Level - 1) * cfg.HpGrowup*1.0);
            Ad = cfg.Ad + (int)((Level - 1) * cfg.AdGrowup*1.0);
            Ap = cfg.Ap + (int)((Level - 1) * cfg.ApGrowup*1.0);
            AdArm = cfg.AdArm + (int)((Level - 1) * cfg.AdArmGrowup*1.0);
            ApArm = cfg.ApArm + (int)((Level - 1) * cfg.ApArmGrowup * 1.0);

            if (addPrec > 0)
            {
                Hp = (int)(Hp * 1.0 / 100 * (100 + addPrec));
                Ad = (int)(Ad * 1.0 / 100 * (100 + addPrec));
                Ap = (int)(Ap * 1.0 / 100 * (100 + addPrec));
                AdArm = (int)(AdArm * 1.0 / 100 * (100 + addPrec));
                ApArm = (int)(ApArm * 1.0 / 100 * (100 + addPrec));
            }

            if (HeroId > 0)
            {
                var ueseHero = Storage.Load<UserHero>(Pid, true);
                var userHeroItem = ueseHero.FindByHeroId(HeroId);
                if (userHeroItem != null)
                {
                    //刷新武将信息
                    userHeroItem.RefreshEquipProperties();
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void NewObjectInit()
        {
            Level = 1;
            //Rank = HeroId = 0;
        }

        public override void LoadInit()
        {
        }
    }

    /// <summary>
    /// 用户背包装备
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserEquip : KVEntity
    {
        /// <summary>
        /// 装备列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserEquipItem> Items { get; set; }

        /// <summary>
        /// 装备列表-对外
        /// </summary>
        //public List<UserEquipItem> ItemList
        //{
        //    get
        //    {
        //        foreach (var userEquipItem in Items)
        //        {
        //            userEquipItem.LoadInit();
        //        }
        //        return Items;
        //    }
        //}
        /// <summary>
        /// 容量
        /// </summary>
        [ProtoMember(2)]
        public int Capacity { get; set; }

        /// <summary>
        /// 刷新参数值【和配置文件中比较，小的话需要重新计算】
        /// </summary>
        [ProtoMember(3)]
        public int RefreshEquipData { get; set; }

        /// <summary>
        /// 剩余容量
        /// </summary>
        public int LaveCapacity { get { return Capacity - Items.Count(o => o.HeroId == 0); } }

        public override void NewObjectInit()
        {
            Items = new List<UserEquipItem>();
            Capacity = 100;

            RefreshEquipData = ConfigHelper.RefreshEquipData;
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserEquipItem>();

            var refreshEquipData = ConfigHelper.RefreshEquipData;
            //有更改配置文件后刷新属性
            if (RefreshEquipData < refreshEquipData)
            {
                foreach (var userEquipItem in Items)
                {
                    userEquipItem.RefreshProperties();
                }
                RefreshEquipData = refreshEquipData;
            }
        }

        /// <summary>
        /// 当前列表是否已满
        /// </summary>
        [JsonIgnore]
        public bool IsFull
        {
            get { return Items.Count(o => o.HeroId == 0) >= Capacity; }
        }

        /// <summary>
        /// 添加装备
        /// </summary>
        /// <param name="itemId">物品Id</param>
        /// <param name="opCode">记录log源头id</param>
        /// <param name="confirm">是否需要判断背包容量</param>
        /// <returns></returns>
        public UserEquipItem AddEquipToUser(int itemId, int opCode, bool confirm = true)
        {
            var equip = SysEquipCfg.Find(itemId);
            if (equip == null) throw new ApplicationException(string.Format("物品Id[{0}]不存在", itemId));
            if (IsFull && confirm) return null;
            var equipItem = CreateNew<UserEquipItem>();
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysEquipCfg, UserEquipItem>(
                    new DefaultMapConfig().IgnoreMembers<SysEquipCfg, UserEquipItem>(new[] { "Id" }));
            mapper.Map(equip, equipItem);
            equipItem.ItemId = itemId;
            equipItem.Pid = Id;
            Items.Add(equipItem);

            var userHero = Storage.Load<UserHero>(Id);
            userHero.ChangeNewMsg();

            GameLogManager.ItemLog(Id, itemId, 1, opCode, (int)ItemType.Equip, 0, 0);

            if (equip.Quality >= ItemQuality.Purple)
            {
                //广播
                var userRole = Storage.Load<UserRole>(Id);
                var msg = "";
                var color = Utility.GetQualityColor(equip.Quality);
                switch (opCode)
                {
                    case 4004:
                        //合成
                        msg = LangResource.GetLangResource(ResourceId.R_0000_MixtureEquipMsg, userRole.Id,
                                                             userRole.NickName, equip.Id, equip.Name,color); break;
                    case 5001: 
                        //寻访
                        msg = LangResource.GetLangResource(ResourceId.R_0000_ExtractEquipMsg, userRole.Id,
                                                             userRole.NickName, equip.Id, equip.Name,color); break;
                }
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
            }

            return equipItem;
        }

        /// <summary>
        /// 删除装备
        /// </summary>
        /// <param name="userEquipItem"></param>
        /// <param name="opCode">接口id</param>
        /// <returns></returns>
        public bool RemoveEquip(UserEquipItem userEquipItem, int opCode)
        {
            var userHero = DataStorage.Current.Load<UserHero>(Id);

            Items.Remove(userEquipItem);
            userHero.ChangeNewMsg();
            GameLogManager.ItemLog(Id, userEquipItem.ItemId, -1, opCode, (int)ItemType.Equip, 0, 0);
            return true;
        }
    }
}
