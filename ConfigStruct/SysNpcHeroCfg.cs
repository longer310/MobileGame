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
//    /// NPC武将配置
//    /// </summary>
//    public class SysNpcHeroCfg : TableCfg<SysNpcHeroCfg>
//    {
//        /// <summary>
//        /// 武将Id
//        /// </summary>
//        public int HeroId { get; set; }

//        /// <summary>
//        /// 武将等级
//        /// </summary>
//        public int Level { get; set; }

//        /// <summary>
//        /// 星级
//        /// </summary>
//        public int StarLevel { get; set; }

//        /// <summary>
//        /// 士兵等级
//        /// </summary>
//        public int ArmyLevel { get; set; }

//        /// <summary>
//        /// 技能等级列表
//        /// </summary>
//        public string SkillLevels { get; set; }

//        /// <summary>
//        /// 武
//        /// </summary>
//        public int Force { get; set; }

//        /// <summary>
//        /// 智
//        /// </summary>
//        public int Intel { get; set; }

//        /// <summary>
//        /// 统
//        /// </summary>
//        public int Command { get; set; }

//        /// <summary>
//        /// 物理伤害
//        /// </summary>
//        public int Ad { get; set; }

//        /// <summary>
//        /// 法术伤害
//        /// </summary>
//        public int Ap { get; set; }

//        /// <summary>
//        /// 物理护甲
//        /// </summary>
//        public int AdArm { get; set; }

//        /// <summary>
//        /// 法术护甲
//        /// </summary>
//        public int ApArm { get; set; }

//        /// <summary>
//        /// 生命
//        /// </summary>
//        public int Hp { get; set; }

//        /// <summary>
//        /// 物理暴击-伤害加倍
//        /// </summary>
//        public int AdCrit { get; set; }

//        /// <summary>
//        /// 法术暴击-伤害加倍
//        /// </summary>
//        public int ApCrit { get; set; }

//        /// <summary>
//        /// 物理格挡
//        /// </summary>
//        public int Block { get; set; }

//        /// <summary>
//        /// 物理吸血
//        /// </summary>
//        public int Blood { get; set; }

//        /// <summary>
//        /// 攻击距离 1表示前排，2表示中排，3表示后排
//        /// </summary>
//        public int Range { get; set; }

//        /// <summary>
//        /// 攻击间隔:毫秒 例如800毫秒，则表示每800毫秒攻击一次
//        /// </summary>
//        public int AttackSpeed { get; set; }

//        /// <summary>
//        /// 移动速度:毫秒 例如600毫秒，则表示每移动一格需要600毫秒
//        /// </summary>
//        public int MoveSpeed { get; set; }

//        /// <summary>
//        /// 生命回复-表示每回合过关回复的生命值
//        /// </summary>
//        public int HpRecovery { get; set; }

//        /// <summary>
//        /// 气势回复-表示没回合过关回复的气势值
//        /// </summary>
//        public int EnergyRecovery { get; set; }

//        /// <summary>
//        /// 武将配置表
//        /// </summary>
//        public SysHeroCfg SysHeroCfg
//        {
//            get
//            {
//                var cfg = SysHeroCfg.Find(HeroId);
//                if (cfg == null) throw new ApplicationException(string.Format("武将Id[{0}]不存在", HeroId));
//                return cfg;
//            }
//        }

//        /// <summary>
//        /// 攻击顺序
//        /// </summary>
//        public int AttackMode { get { return SysHeroCfg.AttackMode; } }

//        /// <summary>
//        /// 战斗力
//        /// </summary>
//        public int Combat { get; set; }

//        /// <summary>
//        /// 战斗力
//        /// 按最终面板上的五维属性来算  
//        /// 包括装备和小兵加成后
//        /// 物理攻击*10+法术强度*10+护甲*100+法抗*100+生命*1加上技能增加的战斗力   
//        /// skill1lev*100+skill2lev*150+skill3lev*200+skill4lev*250
//        /// </summary>
//        //public int Combat
//        //{
//        //    get
//        //    {
//        //        return Ad * 10 + Ap * 10 + AdArm * 100 + ApArm * 100 + Hp * 1 + SkillLevelList[0] * 100 +
//        //               SkillLevelList[1] * 150 + SkillLevelList[2] * 200 + SkillLevelList[3] * 250;
//        //    }
//        //}

//        /// <summary>
//        /// 技能等级列表
//        /// </summary>
//        public List<int> SkillLevelList { get { return JsonConvert.DeserializeObject<List<int>>(SkillLevels); } }
//    }
//}
