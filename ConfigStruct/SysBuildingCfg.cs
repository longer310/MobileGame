using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统宫殿配置
    /// </summary>
    public class SysBuildingCfg : TableCfg<SysBuildingCfg>
    {
        /// <summary>
        /// 宫殿id
        /// </summary>
        public int BuildingId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 宫殿等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 需要主公的等级
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity { get; set; }
        /// <summary>
        /// 妃子个数
        /// </summary>
        public int ConcubineNum { get; set; }
        /// <summary>
        /// 升级需要的木材
        /// </summary>
        public int NeedWood { get; set; }
        /// <summary>
        /// 升级需要的石头
        /// </summary>
        public int NeedStone { get; set; }
        /// <summary>
        /// 升级需要的铁矿
        /// </summary>
        public int NeedIron { get; set; }
        /// <summary>
        /// 升级需要的金币
        /// </summary>
        public int NeedCoin { get; set; }
        /// <summary>
        /// 升级需要的时间（分钟）
        /// </summary>
        public int NeedTime { get; set; }
        /// <summary>
        /// 保护百分比
        /// </summary>
        public int Protect { get; set; }

        /// <summary>
        /// 宫殿生产的资源类型
        /// </summary>
        public MoneyType MoneyType
        {
            get
            {
                int type = (int)(BuildingId / 1000) + 1;
                return (MoneyType)type;
            }
        }
    }
}
