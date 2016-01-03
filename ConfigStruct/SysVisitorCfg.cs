using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 大地图访客配置
    /// </summary>
    public class SysVisitorCfg : TableCfg<SysVisitorCfg>
    {
        /// <summary>
        /// 城池ID
        /// </summary>
        public int CityId { get; set; }
        /// <summary>
        /// 来访的武将/妃子ID
        /// </summary>
        public int VisitorId { get; set; }
        /// <summary>
        /// 寻访到访的概率
        /// </summary>
        public int Probability { get; set; }
        /// <summary>
        /// 声望或者魅力不足对话
        /// </summary>
        public string Talk1 { get; set; }
        /// <summary>
        /// 拜访成功对话
        /// </summary>
        public string Talk2 { get; set; }
        /// <summary>
        /// 已拜访对话
        /// </summary>
        public string Talk3 { get; set; }
    }
}
