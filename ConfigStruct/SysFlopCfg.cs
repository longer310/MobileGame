using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统翻牌对话配置
    /// </summary>
    public class SysFlopCfg : TableCfg<SysFlopCfg>
    {
        /// <summary>
        /// 好感度
        /// </summary>
        public int Favor { get; set; }

        /// <summary>
        /// 魅力值
        /// </summary>
        public int Charm { get; set; }

        /// <summary>
        /// 对话
        /// </summary>
        public string Talk { get; set; }
    }
}
