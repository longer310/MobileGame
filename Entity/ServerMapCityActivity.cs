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
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 全局城池活动数据信息项
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerMapCityActivityItem : KVEntity
    {
        /// <summary>
        /// 系统城池id【对应SysCityCfg表】
        /// </summary>
        [ProtoMember(1)]
        public int CityId { get; set; }
        /// <summary>
        /// 到访列表
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ActivityVisitorTtem> ActivityVisitorItems { get; set; }

        public override void NewObjectInit()
        {
            ActivityVisitorItems = new List<ActivityVisitorTtem>();
        }

        public override void LoadInit()
        {
            ActivityVisitorItems = ActivityVisitorItems ?? new List<ActivityVisitorTtem>();
        }
    }

    /// <summary>
    /// 全局城池活动数据
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerMapCityActivity : KVEntity//KVListEntity<ServerMapCityActivityItem>
    {
        /// <summary>
        /// 城池活动实例
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ServerMapCityActivityItem> Items { get; set; }

        /// <summary>
        /// 当前数字
        /// </summary>
        [ProtoMember(2)]
        public int CurNum { get; set; }

        public override void NewObjectInit()
        {
            Items = new List<ServerMapCityActivityItem>();
        }

        private bool isInit;
        public override void LoadInit()
        {
            Items = Items ?? new List<ServerMapCityActivityItem>();
            if (!isInit)
            {
                var idList = Items.Select(o => o.Id).ToList();
                DataStorage.Current.LoadList<ServerMapCityActivityItem>(idList.ToArray(), true);
                isInit = true;
            }

            foreach (var serverMapCityActivityItem in Items)
            {
                serverMapCityActivityItem.LoadInit();
            }

            var clearBigMapActivityNum = ConfigHelper.ClearBigMapActivityNum;
            if (clearBigMapActivityNum > CurNum)
            {
                for (var i = Items.Count() - 1; i >= 0; i--)
                {
                    var item = Items[i];
                    DataStorage.Current.MarkDeleted(item);
                }
                Items.Clear();
                Utility.RefreshMapCityActivity();
                CurNum = clearBigMapActivityNum;
            }
        }
    }
}
