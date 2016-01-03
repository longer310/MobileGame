// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserBag.cs
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
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 背包的道具项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserToolItem : EntityItem
    {
        /// <summary>
        /// Id(出售、使用传参)
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public int Id { get; set; }
        /// <summary>
        /// 物品Id
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int ItemId { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [ProtoMember(3)]
        [Tag(3)]
        public int Num { get; set; }
        /// <summary>
        /// 品质
        /// </summary>
        [ProtoMember(4)]
        [Tag(4)]
        public ItemQuality Quality { get; set; }
    }

    /// <summary>
    /// 用户背包道具
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserTool : KVEntity
    {
        /// <summary>
        /// 道具列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserToolItem> Items { get; set; }

        /// <summary>
        /// 容量
        /// </summary>
        [ProtoMember(2)]
        public int Capacity { get; set; }

        public override void NewObjectInit()
        {
            Items = new List<UserToolItem>();
            Capacity = 100;
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserToolItem>();
        }

        /// <summary>
        /// 当前列表是否已满
        /// </summary>
        [JsonIgnore]
        public bool IsFull
        {
            get { return Items.Count >= Capacity; }
        }

        /// <summary>
        /// 剩余容量
        /// </summary>
        public int LaveCapacity { get { return Capacity - Items.Count; } }

        /// <summary>
        /// 将不可叠加的物品添加到背包中
        /// </summary>
        /// <param name="itemId">物品Id</param>
        /// <param name="confirm">是否需要判断背包容量</param>
        /// <returns></returns>
        public bool TryAddNonOverlap(int itemId, bool confirm = true)
        {
            var item = SysToolCfg.Find(itemId);
            if (item == null) throw new ApplicationException(string.Format("物品Id[{0}]不存在", itemId));
            if (IsFull && confirm) return false;
            var id = Util.GetSequence(typeof(UserToolItem));
            var bagItem = new UserToolItem { Id = id, ItemId = itemId, Quality = item.Quality };
            bagItem.Pid = Id;
            Items.Add(bagItem);
            return true;
        }

        /// <summary>
        /// 将可叠加物品添加到背包中。
        /// </summary>
        /// <param name="itemId">物品Id</param>
        /// <param name="num">数量</param>
        /// <param name="opCode">记录log源头</param>
        /// <param name="confirm">是否需要判断背包容量</param>
        /// <returns>添加成功返回true，如果容量不够，那么返回false</returns>
        public bool TryAdd(int itemId, int num, int opCode, bool confirm = true)
        {
            if (num <= 0) throw new ArgumentOutOfRangeException("num");
            var item = SysToolCfg.Find(itemId);
            if (item == null) throw new ApplicationException(string.Format("物品Id[{0}]不存在", itemId));

            var bagItem = Items.SingleOrDefault(p => p.ItemId == itemId);
            var startNum = 0;
            if (bagItem == null)
            {
                if (IsFull && confirm) return false;
                var id = Util.GetSequence(typeof(UserToolItem));
                bagItem = new UserToolItem { ItemId = itemId, Id = id, Quality = item.Quality, Num = num };
                bagItem.Pid = Id;
                Items.Add(bagItem);
            }
            else
            {
                startNum = bagItem.Num;
                bagItem.Num += num;
            }
            var endNum = bagItem.Num;

            GameLogManager.ItemLog(Id, itemId, num, opCode, (int)ItemType.Tool, startNum, endNum);
            return true;
        }

        /// <summary>
        /// 删除/使用/出售道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="num"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public bool RemoveTool(int itemId, int num, int opCode)
        {
            var bagItem = Items.SingleOrDefault(p => p.ItemId == itemId);
            if (bagItem == null) return false;
            if (bagItem.Num < num) return false;
            var startNum = bagItem.Num;
            bagItem.Num -= num;
            var endNum = bagItem.Num;

            GameLogManager.ItemLog(Id, itemId, -num, opCode, (int)ItemType.Tool, startNum, endNum);
            return true;
        }
    }
}
