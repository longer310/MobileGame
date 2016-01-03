using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.Jianghu.ConfigStruct
{
    /// <summary>
    /// 系统南征北战副本配置
    /// </summary>
    public class SysCivilWarCfg : TableCfg<SysCivilWarCfg>
    {
        /// <summary>
        /// 怪物类别
        /// </summary>
        public CivilWarType Type { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 第几关
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// 南征北战副本怪物类别
    /// </summary>
    public enum CivilWarType
    {
        /// <summary>
        /// 暴民
        /// </summary>
        Mob = 1,
        /// <summary>
        /// 山贼
        /// </summary>
        Cateran = 2,
        /// <summary>
        /// 流匪
        /// </summary>
        Bandit = 3,
        /// <summary>
        /// 叛军
        /// </summary>
        RebelArmy = 4,
        /// <summary>
        /// 异族
        /// </summary>
        DifRace = 5,
        /// <summary>
        /// 部落
        /// </summary>
        Tribe = 6,
        /// <summary>
        /// 军团
        /// </summary>
        Corps = 7,
        /// <summary>
        /// 势力
        /// </summary>
        Power = 8,
        /// <summary>
        /// 诸侯
        /// </summary>
        Prince = 9,
        /// <summary>
        /// 联盟
        /// </summary>
        Union = 10
    }
}
