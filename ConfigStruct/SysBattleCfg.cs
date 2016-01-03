using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 战役
    /// </summary>
    public class SysBattleCfg : TableCfg<SysBattleCfg>
    {
        /// <summary>
        /// 战役名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 开放所需的主公等级
        /// </summary>
        public int NeedLevel { get; set; }
    }
}
