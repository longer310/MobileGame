//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MobileGame.Core;
//using MobileGame.tianzi.Entity;
//using Newtonsoft.Json;
//using ProtoBuf;

//namespace MobileGame.tianzi.ConfigStruct
//{
//    /// <summary>
//    /// NPC阵型配置
//    /// </summary>
//    public class SysNpcFormationCfg : TableCfg<SysNpcFormationCfg>
//    {
//        /// <summary>
//        /// NpcId
//        /// </summary>
//        public int NpcId { get; set; }

//        /// <summary>
//        /// 武将Id
//        /// </summary>
//        public int HeroId { get; set; }

//        /// <summary>
//        /// 阵型位置
//        /// </summary>
//        public LocationNumber Location { get; set; }

//        /// <summary>
//        /// 星级
//        /// </summary>
//        public int StarLevel
//        {
//            get
//            {
//                var cfg = GetSysNpcHeroCfg();
//                if (cfg == null) return 0;
//                else return cfg.StarLevel;
//            }
//        }

//        /// <summary>
//        /// 战斗力
//        /// </summary>
//        public int Combat
//        {
//            get { return GetSysNpcHeroCfg().Combat; }
//        }

//        /// <summary>
//        /// 武将等级
//        /// </summary>
//        public int Level { get; set; }

//        /// <summary>
//        /// 出场顺序
//        /// </summary>
//        public int Turn { get; set; }

//        public SysNpcHeroCfg GetSysNpcHeroCfg()
//        {
//            var cfg = SysNpcHeroCfg.Find(o => o.HeroId == HeroId && o.Level == Level);
//            if (cfg == null)
//                throw new ApplicationException(string.Format("SysNpcHeroCfg找不到数据:HeroId[{0}],Level[{1}]", HeroId, Level));
//            return cfg;
//        }
//    }
//}
