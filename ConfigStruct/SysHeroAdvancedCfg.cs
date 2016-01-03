using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{   
    /// <summary>
    /// 系统武将进阶配置
    /// </summary>
    public class SysHeroAdvancedCfg : TableCfg<SysHeroAdvancedCfg>
    {   
        /// <summary>
        /// 进阶武将id
        /// </summary>
        public int HeroId { get; set; }

        /// <summary>
        /// 开始星级
        /// </summary>
        public int BeginStar { get; set; }

        /// <summary>
        /// 进阶星级
        /// </summary>
        public int UpgradeStar { get; set; }

        /// <summary>
        /// 武成长增加
        /// </summary>
        public float ForceGrowup { get; set; }

        /// <summary>
        /// 智成长增加
        /// </summary>
        public float IntelGrowup { get; set; }

        /// <summary>
        /// 统成长增加
        /// </summary>
        public float CommandGrowup { get; set; }
    }
}
