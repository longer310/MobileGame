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
    /// 装备/武将/妃子碎片项信息
    /// </summary>
    [ProtoContract]
    public class ChipItem
    {
        /// <summary>
        /// 碎片Id【需要去除数字的首位得到相应装备/武将/妃子id，比如碎片id为71000903，去除“7”得到的1000903即为系统武将id】
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public int ItemId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int Num { get; set; }

        /// <summary>
        /// 道具配置表
        /// </summary>
        public SysToolCfg GetSysToolCfg()
        {
            var cfg = SysToolCfg.Find(ItemId);
            if (cfg == null)
            {
                throw new ApplicationException(string.Format("碎片Id[{0}]不存在", ItemId));
            }
            return cfg;
        }

        /// <summary>
        /// 熔炼可以提取的经验
        /// </summary>
        public int MeltingExp
        {
            get
            {
                if (GetToolType() != ToolType.EquipChip) return 0;
                else
                {
                    var sysToolCfg = GetSysToolCfg();
                    if (sysToolCfg == null) return 0;
                    var quality = sysToolCfg.Quality;
                    var equipMixtureCfg = ConfigHelper.EquipMixtureCfgData.FirstOrDefault(o => o.Type == quality);
                    if (equipMixtureCfg == null)
                        throw new ApplicationException(string.Format("EquipMixtureCfgData Type[{0}]不存在", (int)quality));
                    return equipMixtureCfg.Exp;
                }
            }
        }

        /// <summary>
        /// 碎片类型
        /// </summary>
        public ToolType GetToolType()
        {
            return GetSysToolCfg().GetToolType();
        }
    }

    /// <summary>
    /// 用户碎片
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserChip : KVEntity
    {
        /// <summary>
        /// 碎片列表
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ChipItem> ChipItems { get; set; }

        public override void NewObjectInit()
        {
            ChipItems = new List<ChipItem>();
        }

        public override void LoadInit()
        {
            ChipItems = ChipItems ?? new List<ChipItem>();

            
        }
        /// <summary>
        /// 添加碎片
        /// </summary>
        /// <param name="itemId">物品Id</param>
        /// <param name="num">物品数量</param>
        /// <param name="type">道具类型</param>
        /// <param name="opCode">记录log源头</param>
        /// <returns></returns>
        public bool AddChip(int itemId, int num, ToolType type, int opCode)
        {
            var ttype = ItemType.EquipChip;
            var startNum = 0;
            var endNum = 0;

            var userChipItem = ChipItems.SingleOrDefault(p => p.ItemId == itemId);
            if (userChipItem == null)
            {
                userChipItem = new ChipItem() { ItemId = itemId, Num = num };
                ChipItems.Add(userChipItem);
                endNum = num;
            }
            else
            {
                startNum = userChipItem.Num;
                userChipItem.Num += num;
                endNum = userChipItem.Num;
            }

            if (type == ToolType.EquipChip)
            {
                ttype = ItemType.EquipChip;
            }
            else if (type == ToolType.HeroChip)
            {
                ttype = ItemType.HeroChip;

                var userHero = DataStorage.Current.Load<UserHero>(Id);
                userHero.ChangeNewMsg();
            }
            else if (type == ToolType.ConcubineChip)
            {
                ttype = ItemType.ConcubineChip;
            }

            GameLogManager.ItemLog(Id, itemId, num, opCode, (int)ttype, startNum, endNum);
            return true;
        }

        /// <summary>
        /// 减去碎片
        /// </summary>
        /// <param name="idOrChipId"></param>
        /// <param name="num"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool SubChip(int idOrChipId, int num, ToolType type)
        {
            var chipId = idOrChipId;
            if (chipId < 9999999)
            {
                //武将、妃子、装备 本身的系统ID
                if (ToolType.None == type) throw new ApplicationException("SubChip Not Find Chip.");
                chipId = Utility.GetChipIdById(chipId, type);
            }

            var item = ChipItems.FirstOrDefault(o => o.ItemId == chipId);
            if (item != null && item.Num >= num)
            {
                item.Num -= num;

                if (chipId > (int)ToolType.HeroChip && chipId < (int)ToolType.ConcubineChip)
                {
                    var userHero = DataStorage.Current.Load<UserHero>(Id);
                    userHero.ChangeNewMsg();
                }
                return true;
            }

            return false;
        }
    }
}
