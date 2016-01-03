using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统妃子天赋配置
    /// </summary>
    public class SysConcubineTalentCfg : TableCfg<SysConcubineTalentCfg>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 增加的产量
        /// </summary>
        public int AddProduct { get; set; }
        /// <summary>
        /// 增加妃子的携带量
        /// </summary>
        public int AddCarry { get; set; }
        /// <summary>
        /// 增加保护自身携带资源量
        /// </summary>
        public int AddProtectCarry { get; set; }
        /// <summary>
        /// 增加入住的宫殿存储量
        /// </summary>
        public int AddStorage { get; set; }
        /// <summary>
        /// 增加保护入住宫殿资源量
        /// </summary>
        public int AddProtectStorage { get; set; }
        /// <summary>
        /// 减少宫殿建造CD时间
        /// </summary>
        public int CutBuildTime { get; set; }
        /// <summary>
        /// 减少妃子晋封CD时间
        /// </summary>
        public int CutConcubineTime { get; set; }
        /// <summary>
        /// 天赋描述
        /// </summary>
        public string Introduce { get; set; }
    }
}
