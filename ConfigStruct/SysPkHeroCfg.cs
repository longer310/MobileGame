using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.Service;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 副本NPC武将配置
    /// </summary>
    public class SysPkHeroCfg : TableCfg<SysPkHeroCfg>
    {
        /// <summary>
        /// NpcId
        /// </summary>
        public int NpcId { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public LocationNumber Location { get; set; }
        /// <summary>
        /// 武将Id
        /// </summary>
        public int HeroId { get; set; }
        /// <summary>
        /// 出场顺序
        /// </summary>
        public int Turn { get; set; }
        /// <summary>
        /// 武将等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 星级
        /// </summary>
        public int StarLevel { get; set; }

        /// <summary>
        /// 士兵等级
        /// </summary>
        public int ArmyLevel { get; set; }

        /// <summary>
        /// 技能1等级
        /// </summary>
        public int Skill1Level { get; set; }

        /// <summary>
        /// 技能2等级
        /// </summary>
        public int Skill2Level { get; set; }

        /// <summary>
        /// 技能3等级
        /// </summary>
        public int Skill3Level { get; set; }

        /// <summary>
        /// 技能4等级
        /// </summary>
        public int Skill4Level { get; set; }

        /// <summary>
        /// 武器ID
        /// </summary>
        public int Item1Id { get; set; }

        /// <summary>
        /// 武器等级
        /// </summary>
        public int Item1Level { get; set; }

        /// <summary>
        /// 武器星级
        /// </summary>
        public int Item1Star { get; set; }

        /// <summary>
        /// 头盔ID
        /// </summary>
        public int Item2Id { get; set; }

        /// <summary>
        /// 头盔等级
        /// </summary>
        public int Item2Level { get; set; }

        /// <summary>
        /// 头盔星级
        /// </summary>
        public int Item2Star { get; set; }

        /// <summary>
        /// 盔甲ID
        /// </summary>
        public int Item3Id { get; set; }

        /// <summary>
        /// 盔甲等级
        /// </summary>
        public int Item3Level { get; set; }

        /// <summary>
        /// 盔甲星级
        /// </summary>
        public int Item3Star { get; set; }

        /// <summary>
        /// 兵书ID
        /// </summary>
        public int Item4Id { get; set; }

        /// <summary>
        /// 兵书等级
        /// </summary>
        public int Item4Level { get; set; }

        /// <summary>
        /// 兵书星级
        /// </summary>
        public int Item4Star { get; set; }

        /// <summary>
        /// 武
        /// </summary>
        public int Force { get; set; }

        /// <summary>
        /// 智
        /// </summary>
        public int Intel { get; set; }

        /// <summary>
        /// 统
        /// </summary>
        public int Command { get; set; }

        /// <summary>
        /// 物理伤害
        /// </summary>
        public int Ad { get; set; }

        /// <summary>
        /// 法术伤害
        /// </summary>
        public int Ap { get; set; }

        /// <summary>
        /// 物理护甲
        /// </summary>
        public int AdArm { get; set; }

        /// <summary>
        /// 法术护甲
        /// </summary>
        public int ApArm { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        public int Hp { get; set; }

        /// <summary>
        /// 物理暴击-伤害加倍
        /// </summary>
        public int AdCrit { get; set; }

        /// <summary>
        /// 法术暴击-伤害加倍
        /// </summary>
        public int ApCrit { get; set; }

        /// <summary>
        /// 物理格挡
        /// </summary>
        public int Block { get; set; }

        /// <summary>
        /// 物理吸血
        /// </summary>
        public int Blood { get; set; }

        /// <summary>
        /// 攻击距离 1表示前排，2表示中排，3表示后排
        /// </summary>
        public int Range { get; set; }

        /// <summary>
        /// 攻击间隔:毫秒 例如800毫秒，则表示每800毫秒攻击一次
        /// </summary>
        public int AttackSpeed { get; set; }

        /// <summary>
        /// 移动速度:毫秒 例如600毫秒，则表示每移动一格需要600毫秒
        /// </summary>
        public int MoveSpeed { get; set; }

        /// <summary>
        /// 生命回复-表示每回合过关回复的生命值
        /// </summary>
        public int HpRecovery { get; set; }

        /// <summary>
        /// 气势回复-表示没回合过关回复的气势值
        /// </summary>
        public int EnergyRecovery { get; set; }

        /// <summary>
        /// 武将配置表
        /// </summary>
        public SysHeroCfg SysHeroCfg
        {
            get
            {
                var cfg = SysHeroCfg.Find(HeroId);
                if (cfg == null) throw new ApplicationException(string.Format("武将Id[{0}]不存在", HeroId));
                return cfg;
            }
        }

        /// <summary>
        /// 真正的攻击速度
        /// </summary>
        public int RealAttackSpeed
        {
            get
            {
                var attackSpeed = AttackSpeed;
                var sysPetCfg = SysPetCfg.Find(o => o.Id == HeroId);
                if (sysPetCfg != null)
                {
                    attackSpeed -= sysPetCfg.AttackSpeed;
                }
                return attackSpeed;
            }
        }

        /// <summary>
        /// 攻击顺序
        /// </summary>
        public int AttackMode { get { return SysHeroCfg.AttackMode; } }

        /// <summary>
        /// 骑宠ID
        /// </summary>
        public int PetId { get; set; }

        /// <summary>
        /// 战斗力
        /// 按最终面板上的五维属性来算  
        /// 包括装备和小兵加成后
        /// 物理攻击*10+法术强度*10+护甲*100+法抗*100+生命*1加上技能增加的战斗力   
        /// skill1lev*100+skill2lev*150+skill3lev*200+skill4lev*250
        /// </summary>
        public int Combat
        {
            get
            {
                return
                    (int)
                        ((Ad + Ap) * 2.0 / 3 + (AdArm + ApArm) * 2.0 + Hp * 1.0 / 10 + HpRecovery * 1.0 / 16 + EnergyRecovery * 10.0 / 75 +
                         SkillLevelList.Sum() * 2.0 + Block + Blood + AdCrit + ApCrit);
            }
        }

        /// <summary>
        /// 技能等级列表
        /// </summary>
        public List<int> SkillLevelList
        {
            get
            {
                var list = new List<int>();
                list.Add(Skill1Level);
                list.Add(Skill2Level);
                list.Add(Skill3Level);
                list.Add(Skill4Level);
                return list;
            }
        }

        public void Init()
        {
            if (Force == 0)
            {
                var heroItem = Utility.FillSysNpcHeroCfg(HeroId, Level, StarLevel, ArmyLevel,
                    Item1Id, Item1Level, Item1Star,
                    Item2Id, Item2Level, Item2Star,
                    Item3Id, Item3Level, Item3Star,
                    Item4Id, Item4Level, Item4Star,
                    PetId);

                Force = heroItem.Force;
                Intel = heroItem.Intel;
                Command = heroItem.Command;
                Ad = heroItem.Ad;
                Ap = heroItem.Ap;
                AdArm = heroItem.AdArm;
                ApArm = heroItem.ApArm;
                Hp = heroItem.Hp;
                AttackSpeed = heroItem.AttackSpeed;
                MoveSpeed = heroItem.MoveSpeed;
                AdCrit = heroItem.AdCrit;
                Block = heroItem.Block;
                Blood = heroItem.Blood;
                HpRecovery = heroItem.HpRecovery;
                EnergyRecovery = heroItem.EnergyRecovery;
                Range = heroItem.Range;
                ApCrit = heroItem.ApCrit;

                var sql = string.Format("UPDATE SysPkHeroCfg SET `Force`={1},`Intel`={2},`Command`={3},`Ap`={4},`AdArm`={5},`ApArm`={6},`Hp`={7},`AttackSpeed`={8},`MoveSpeed`={9},`AdCrit`={10},`Block`={11},`Blood`={12},`HpRecovery`={13},`EnergyRecovery`={14},`Range`={15},`Ad`={16},`ApCrit`={17},`Combat`={18} WHERE `Id`={0};",
                    Id, Force, Intel, Command, Ap, AdArm, ApArm, Hp, AttackSpeed, MoveSpeed, AdCrit, Block, Blood, HpRecovery, EnergyRecovery, Range, Ad, ApCrit,Combat);

                MySqlHelper.ExecuteNonQuery(ParamHelper.GameServer, CommandType.Text, sql);
            }
        }
    }
}
