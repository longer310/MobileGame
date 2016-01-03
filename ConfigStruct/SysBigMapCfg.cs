using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 大地图NPC配置表
    /// </summary>
    public class SysBigMapCfg : TableCfg<SysBigMapCfg>
    {
        /// <summary>
        /// Npc等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// NPC名称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// NPC头像
        /// </summary>
        public int HeadId { get; set; }

        /// <summary>
        /// 所需的体力
        /// </summary>
        public int NeedSp { get; set; }

        /// <summary>
        /// 武将的经验
        /// </summary>
        public int HeroExp { get; set; }

        /// <summary>
        /// 奖励的铜钱
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// 所需木材
        /// </summary>
        public int Wood { get; set; }

        /// <summary>
        /// 所需石头
        /// </summary>
        public int Stone { get; set; }

        /// <summary>
        /// 所需铁矿
        /// </summary>
        public int Iron { get; set; }

        /// <summary>
        /// 概率掉落
        /// </summary>
        public int Probability { get; set; }

        /// <summary>
        /// 道具列表
        /// </summary>
        public string ToolIds { get; set; }

        /// <summary>
        /// 道具id列表
        /// </summary>
        public List<int> ToolList { get { return JsonConvert.DeserializeObject<List<int>>(ToolIds); } }
    }
}
