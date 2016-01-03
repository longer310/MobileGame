using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Repository;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 服务器公用数据
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class ServerData : KVEntity
    {
        /// <summary>
        /// int数据列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> IntData { get; set; }

        /// <summary>
        /// string数据列表
        /// </summary>
        [ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<string> StringData { get; set; }

        /// <summary>
        /// long数据列表
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<long> LongData { get; set; }

        /// <summary>
        /// 今天是否下发了竞技场奖励
        /// </summary>
        [ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue TodayHasIssuedPkReward { get; set; }


        public override void NewObjectInit()
        {
            IntData = new List<int>();
            StringData = new List<string>();
            LongData = new List<long>();

            int index = -1;
            if (Id == (int)ServerDataIdType.Pk) index = (int)ServerDataPkIntType.MaxIndex;
            if (Id == (int)ServerDataIdType.BigMapCity) index = (int)ServerDataBigMapIntType.MaxIndex;
            if (Id == (int)ServerDataIdType.SignEdition) index = (int)ServerDataSignIntType.MaxIndex;
            while (IntData.Count < index + 1)
            {
                IntData.Add(0);
            }
            while (LongData.Count < 1)
            {
                LongData.Add(0);
            }
            //int lIndex = -1;
            //if (Id == (int)ServerDataIdType.Pk) lIndex = (int)ServerDataPkLongType.MaxIndex;
            //while (LongData.Count < lIndex)
            //{
            //    LongData.Add(0);
            //}
        }

        public override void LoadInit()
        {
            IntData = IntData ?? new List<int>();
            StringData = StringData ?? new List<string>();
            LongData = LongData ?? new List<long>();

            int index = -1;
            if (Id == (int)ServerDataIdType.Pk) index = (int)ServerDataPkIntType.MaxIndex;
            if (Id == (int)ServerDataIdType.BigMapCity) index = (int)ServerDataBigMapIntType.MaxIndex;
            if (Id == (int)ServerDataIdType.SignEdition) index = (int)ServerDataSignIntType.MaxIndex;
            while (IntData.Count < index + 1)
            {
                IntData.Add(0);
            }
            //int lIndex = -1;
            //if (Id == (int)ServerDataIdType.Pk) lIndex = (int)ServerDataPkLongType.MaxIndex;
            //while (LongData.Count < lIndex)
            //{
            //    LongData.Add(0);
            //}
        }
    }

    /// <summary>
    /// 服务器公用数据int型数据类型
    /// </summary>
    public enum ServerDataPkIntType
    {
        /// <summary>
        /// 取消竞技场任务Num
        /// 执行任务时和配置ClearPkTaskNum对比,小则取消
        /// </summary>
        ClearPkTaskNum = 0,
        /// <summary>
        /// 竞技场定时下发奖励的任务id
        /// </summary>
        PkTimerTaskId = 1,
        /// <summary>
        /// 竞技场NPC的数量
        /// 调用Utility.GetAndSetPkRank时与ConfigHelper.PkCfgData.NpcNum对比
        /// 如果不等，则清空竞技场排名，并在排行中插入ConfigHelper.PkCfgData.NpcNum数量的NPC，数据暂时不生成！
        /// </summary>
        PkNpcNum = 2,

        MaxIndex = 2,
    }

    /// <summary>
    /// 服务器公用数据Long型数据类型
    /// </summary>
    public enum ServerDataPkLongType
    {
        /// <summary>
        /// 执行定时竞技场下发的上次时间
        /// </summary>
        LastExecTimeNum = 0,

        MaxIndex = 1,
    }
    /// <summary>
    /// 服务器公用数据int型数据类型
    /// </summary>
    public enum ServerDataSignIntType
    {
        /// <summary>
        /// 签到奖励版本号
        /// </summary>
        Edition = 0,
        /// <summary>
        /// 最后一次更新版本号时间
        /// </summary>
        LastTime = 1,

        MaxIndex = 1,
    }
    /// <summary>
    /// 服务器公用数据int型数据类型
    /// </summary>
    public enum ServerDataBigMapIntType
    {
        /// <summary>
        /// 取消大地图任务Num
        /// </summary>
        ClearBigMapNum = 0,

        MaxIndex = 0,
    }

    ////////////////////////////////全局的///////////////////////////////
    /// <summary>
    /// 全服数据的获取ID
    /// </summary>
    public enum ServerDataIdType
    {
        /// <summary>
        /// 竞技场
        /// </summary>
        Pk = 1,
        /// <summary>
        /// 大地图城池
        /// </summary>
        BigMapCity = 2,
        /// <summary>
        /// 已将兑换过的兑换码列表
        /// </summary>
        RedCode = 3,
        /// <summary>
        /// 签到版本信息
        /// </summary>
        SignEdition = 4,
    }

    /// <summary>
    /// 生成唯一游客id的类
    /// </summary>
    public class VisitorId
    {
    }
}
