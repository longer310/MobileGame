// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。HeroExtract
// 文 件 名：HeroExtract.cs
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
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 用户抽取武将信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class HeroExtract : KVEntity
    {
        /// <summary>
        /// 今天铜币抽取的次数
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue CoinExtractNum { get; set; }

        /// <summary>
        /// 今日剩余铜币抽取次数
        /// </summary>
        [JsonIgnore]
        public int CoinExtractLaveNum
        {
            get
            {
                var maxCoinExtract = ConfigHelper.ExtractCfgData.MaxExtracts[(int)ExtractType.Coin - 1];
                var laveNum = maxCoinExtract - (int)CoinExtractNum.Value;
                if (laveNum < 0) return 0;
                return laveNum;
            }
        }

        /// <summary>
        /// 下次免费铜币抽取的时间
        /// </summary>
        [ProtoMember(2)]
        public DateTime CoinExtractEndTime { get; set; }

        /// <summary>
        /// 下次铜币抽取剩余时间
        /// </summary>
        [JsonIgnore]
        public int CoinExtractLaveTime
        {
            get { return CoinExtractEndTime.ToTs(); }
        }

        /// <summary>
        /// 今天钻石抽取的次数
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        public DayZeorValue MoneyExtractNum { get; set; }

        /// <summary>
        /// 今日剩余钻石抽取次数
        /// </summary>
        [JsonIgnore]
        public int MoneyExtractLaveNum
        {
            get
            {
                var maxMoneyExtract = ConfigHelper.ExtractCfgData.MaxExtracts[(int)ExtractType.Money - 1];
                var laveNum = maxMoneyExtract - (int)MoneyExtractNum.Value;
                if (laveNum < 0) return 0;
                return laveNum;
            }
        }

        /// <summary>
        /// 下次免费钻石抽取的时间
        /// </summary>
        [ProtoMember(4)]
        public DateTime MoneyExtractEndTime { get; set; }

        /// <summary>
        /// 下次钻石抽取剩余时间
        /// </summary>
        [JsonIgnore]
        public int MoneyExtractLaveTime
        {
            get { return MoneyExtractEndTime.ToTs(); }
        }

        public override void NewObjectInit()
        {
            CoinExtractEndTime = Util.UnixEpochDateTime;
            MoneyExtractEndTime = Util.UnixEpochDateTime;
        }

        public override void LoadInit()
        {

        }
    }
}
