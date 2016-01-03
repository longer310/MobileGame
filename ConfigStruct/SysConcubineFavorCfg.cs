using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统妃子好感度配置
    /// </summary>
    public class SysConcubineFavorCfg : TableCfg<SysConcubineFavorCfg>
    {
        /// <summary>
        /// 好感度
        /// </summary>
        public int Favor { get; set; }
    }
}
