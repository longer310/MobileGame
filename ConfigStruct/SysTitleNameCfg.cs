using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统封号名称配置
    /// </summary>
    public class SysTitleNameCfg : TableCfg<SysTitleNameCfg>
    {
        /// <summary>
        /// 伯
        /// </summary>
        public string Bo { get; set; }
        /// <summary>
        /// 亭侯
        /// </summary>
        public string TingHou { get; set; }
        /// <summary>
        /// 乡侯
        /// </summary>
        public string XiangHou { get; set; }
        /// <summary>
        /// 郡公
        /// </summary>
        public string JunGong { get; set; }
        /// <summary>
        /// 国公
        /// </summary>
        public string GuoGong { get; set; }
        /// <summary>
        /// 郡王
        /// </summary>
        public string JunWang { get; set; }
        /// <summary>
        /// 亲王
        /// </summary>
        public string QinWang { get; set; }
    }
}
