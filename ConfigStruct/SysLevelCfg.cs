using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using Newtonsoft.Json;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 普通副本配置表
    /// </summary>
    public class SysLevelCfg : TableCfg<SysLevelCfg>
    {
        /// <summary>
        /// 战役id
        /// </summary>
        public int BattleId { get; set; }

        /// <summary>
        /// 战役的关卡id
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 关卡名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 关卡NPC名称
        /// </summary>
        public string GuardName { get; set; }

        /// <summary>
        /// 关卡NPC等级
        /// </summary>
        public int GuardLevel { get; set; }

        /// <summary>
        /// 关卡描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 所需的体力
        /// </summary>
        public int NeedSp { get; set; }

        /// <summary>
        /// 今日免费次数
        /// </summary>
        public int Times { get; set; }

        /// <summary>
        /// 武将的经验
        /// </summary>
        public int HeroExp { get; set; }

        /// <summary>
        /// 奖励的铜钱
        /// </summary>
        public int Coin { get; set; }

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

        ///// <summary>
        ///// npc列表
        ///// </summary>
        //public string NpcIds { get; set; }

        ///// <summary>
        ///// NPCid列表
        ///// </summary>
        //public List<int> NpcList { get { return JsonConvert.DeserializeObject<List<int>>(NpcIds); } }
        /// <summary>
        /// npcid
        /// </summary>
        public int NpcId { get; set; }

        /// <summary>
        /// 关卡表现图片
        /// </summary>
        public int GuardPic { get; set; }

        /// <summary>
        /// 扫荡道具列表
        /// </summary>
        public string SweepingToolNums { get; set; }

        /// <summary>
        /// 扫荡得到的道具id列表
        /// </summary>
        public List<int> SweepingToolNumsList
        {
            get
            {
                if (string.IsNullOrEmpty(SweepingToolNums))
                    return new List<int>();
                else return JsonConvert.DeserializeObject<List<int>>(SweepingToolNums);
            }
        }
    }
}
