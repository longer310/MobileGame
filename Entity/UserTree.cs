using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{

    public enum TreeState
    {
        Idle = 1,
        Growing = 2,
        CanHarving = 3
    }

    [ProtoContract]
    public class UserTree : KVEntity
    {   
        /// <summary>
        /// 等级
        /// </summary>
        [ProtoMember(1)]
        public int Level { get; set; }
        
        
        /// <summary>
        /// 当前
        /// </summary>
        [ProtoMember(2)]
        public int CurExp { get; set; }

        
        /// <summary>
        /// 当前树的状态
        /// </summary>
        [ProtoMember(3)]
        public TreeState State { get; set; }
        
 
        /// <summary>
        /// 当前肥料 0普通 1中级 2高级 
        /// </summary>
        [ProtoMember(4)]
        public int CurPlant { get; set; }


        /// <summary>
        /// 下一次收获时间
        /// </summary>
        [ProtoMember(5)]
        public DateTime NextHarvestTime { get; set; }


        public override void NewObjectInit()
        {
            Level = 1;
            NextHarvestTime = DateTime.Now;
        }

        public override void LoadInit()
        {
            //TODO 初始化
        }

        
    }
       
}
