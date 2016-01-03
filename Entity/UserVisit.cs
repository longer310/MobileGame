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
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 城池项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserVisitItem : EntityItem
    {
        /// <summary>
        /// 系统城池id【对应SysCityCfg表】
        /// </summary>
        [ProtoMember(1)]
        public int CityId { get; set; }

        /// <summary>
        /// 来访武将/妃子id
        /// </summary>
        [ProtoMember(2)]
        public int VisitorId { get; set; }

        /// <summary>
        /// 拜访次数
        /// </summary>
        [ProtoMember(3)]
        public int VisitTimes { get; set; }

        /// <summary>
        /// 离开时间
        /// </summary>
        [ProtoMember(4)]
        public DateTime LeaveTime { get; set; }

        /// <summary>
        /// 城池配置信息
        /// </summary>
        public SysCityCfg SysCityCfg
        {
            get
            {
                var sysCityCfg = SysCityCfg.Find(CityId);
                if (sysCityCfg == null)
                    throw new ApplicationException(string.Format("SysCityCfg:Id:{0} NOT FIND", CityId));
                return sysCityCfg;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void NewObjectInit()
        {
        }

        public override void LoadInit()
        {
        }
    }

    /// <summary>
    /// 用户城池拜访
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserVisit : KVEntity
    {
        /// <summary>
        /// 拜访列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserVisitItem> VisitItems { get; set; }



        /// <summary>
        /// 当前的数字（用于重新匹配！！！）
        /// </summary>
        [ProtoMember(7)]
        public int CurNum { get; set; }

        /// <summary>
        /// 事件列表
        /// </summary>
        [ProtoMember(8), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<EventItem> EventItems { get; set; }

        public override void NewObjectInit()
        {
        }

        public override void LoadInit()
        {
        }
    }
}
