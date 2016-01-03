using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 系统大地图区域配置
    /// </summary>
    public class SysAreaCfg : TableCfg<SysAreaCfg>
    {
        /// <summary>
        /// 城池/领地ID列表
        /// </summary>
        public string CityIds { get; set; }

        /// <summary>
        /// 城池/领地ID列表
        /// </summary>
        public List<int> CityIdList { get { return JsonConvert.DeserializeObject<List<int>>(CityIds); } }

        /// <summary>
        /// 需求主公等级
        /// </summary>
        public int NeedLevel { get; set; }

        /// <summary>
        /// 探索花费
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 云层编号
        /// </summary>
        public string Cloud { get; set; }
    }
}
