using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Common;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 英雄实例
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserHeroItem : EntityItem
    {
        /// <summary>
        /// 系统英雄ID
        /// </summary>
        [ProtoMember(1)]
        public int HeroId { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }

        /// <summary>
        /// 经验
        /// </summary>
        [ProtoMember(3)]
        public int Exp { get; set; }

        /// <summary>
        /// 星级-初始看具体武将配置
        /// </summary>
        [ProtoMember(4)]
        public int StarLevel { get; set; }

        /// <summary>
        /// 英雄状态
        /// </summary>
        //[ProtoMember(5)]
        //public HeroStateType State { get; set; }

        /// <summary>
        /// 三个技能等级列表
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> SkillLevelList { get; set; }

        /// <summary>
        /// 玩家装备id列表
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> EquipIdList { get; set; }

        /// <summary>
        /// 士兵等级
        /// </summary>
        [ProtoMember(8)]
        public int ArmyLevel { get; set; }

        /// <summary>
        /// 玩家骑宠id
        /// </summary>
        [ProtoMember(9)]
        public int PetId { get; set; }

        /// <summary>
        /// 系统骑宠id
        /// </summary>
        [ProtoMember(10)]
        public int SysPetId { get; set; }

        /// <summary>
        /// 武将状态
        /// </summary>
        [ProtoMember(11)]
        public HeroStatus Status { get; set; }

        /// <summary>
        /// 武将状态截止时间
        /// </summary>
        [ProtoMember(12)]
        public DateTime StatusEndTime { get; set; }

        #region 武将属性
        /// <summary>
        /// 武
        /// </summary>
        [ProtoMember(51)]
        public int Force
        {
            get
            {
                Forces = Forces ?? new List<int>();
                return Forces.Sum();
            }
            set { }
        }
        /// <summary>
        /// 武列表-PropertiesType
        /// </summary>
        [ProtoMember(20), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Forces { get; set; }

        /// <summary>
        /// 智
        /// </summary>
        [ProtoMember(52)]
        public int Intel
        {
            get
            {
                Intels = Intels ?? new List<int>();
                return Intels.Sum();
            }
            set { }
        }
        /// <summary>
        /// 智列表-PropertiesType
        /// </summary>
        [ProtoMember(21), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Intels { get; set; }

        /// <summary>
        /// 统
        /// </summary>
        [ProtoMember(53)]
        public int Command
        {
            get
            {
                Commands = Commands ?? new List<int>();
                return Commands.Sum();
            }
            set { }
        }
        /// <summary>
        /// 统列表-PropertiesType
        /// </summary>
        [ProtoMember(22), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Commands { get; set; }

        /// <summary>
        /// 物理攻击
        /// 物理攻击力=主属性+统帅×0.4+初始值（废除）
        /// 物理攻击力=武力*0.4+统帅×2
        /// </summary>
        [ProtoMember(54)]
        public int Ad
        {
            get
            {
                Ads = Ads ?? new List<int>();
                return Ads.Sum() + (int)(Force * 0.4) + Command * 2;
            }
            set { }
        }
        /// <summary>
        /// 攻击列表-PropertiesType
        /// </summary>
        [ProtoMember(23), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Ads { get; set; }

        /// <summary>
        /// 魔法攻击
        /// 法术强度=智力×2.4+初始值（废除）
        /// 法术强度=智力×2.4		
        /// </summary>
        [ProtoMember(55)]
        public int Ap
        {
            get
            {
                Aps = Aps ?? new List<int>();
                return Aps.Sum() + (int)(Intel * 2.4);
            }
            set { }
        }
        /// <summary>
        /// 魔法攻击列表-PropertiesType
        /// </summary>
        [ProtoMember(24), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Aps { get; set; }

        /// <summary>
        /// 物理防御
        /// 物理护甲=武力÷7+统帅÷14
        /// Modify by hql at 2015.7.30 物理护甲=武力÷5+统帅÷10
        /// </summary>
        [ProtoMember(56)]
        public int AdArm
        {
            get
            {
                AdArms = AdArms ?? new List<int>();
                return AdArms.Sum() + (int)(Force * 1.0 / 5 + Command * 1.0 / 10);
            }
            set { }
        }
        /// <summary>
        /// 物理防御列表-PropertiesType
        /// </summary>
        [ProtoMember(25), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> AdArms { get; set; }

        /// <summary>
        /// 法术防御
        /// 法术抗性=智力÷10
        /// Modify by hql at 2015.7.30 物理护甲=智力÷5 + 统率÷10
        /// </summary>
        [ProtoMember(57)]
        public int ApArm
        {
            get
            {
                ApArms = ApArms ?? new List<int>();
                return ApArms.Sum() + (int)(Intel * 1.0 / 5 + Command * 1.0 / 10);
            }
            set { }
        }
        /// <summary>
        /// 法术防御列表-PropertiesType
        /// </summary>
        [ProtoMember(26), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ApArms { get; set; }

        /// <summary>
        /// 血量
        /// 最大生命值=武力×10
        /// </summary>
        [ProtoMember(58)]
        public int Hp
        {
            get
            {
                Hps = Hps ?? new List<int>();
                return Hps.Sum() + Force * 10;
            }
            set { }
        }
        /// <summary>
        /// 血量列表-PropertiesType
        /// </summary>
        [ProtoMember(27), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Hps { get; set; }

        /// <summary>
        /// 法术暴击
        /// </summary>
        [ProtoMember(59)]
        public int ApCrit
        {
            get
            {
                ApCrits = ApCrits ?? new List<int>();
                return ApCrits.Sum();
            }
            set { }
        }
        /// <summary>
        /// 法术暴击比率-PropertiesType
        /// </summary>
        [ProtoMember(28), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ApCrits { get; set; }

        /// <summary>
        /// 物理格挡
        /// </summary>
        [ProtoMember(60)]
        public int Block
        {
            get
            {
                Blocks = Blocks ?? new List<int>();
                return Blocks.Sum();
            }
            set { }
        }
        /// <summary>
        /// 物理格挡列表-PropertiesType
        /// </summary>
        [ProtoMember(29), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Blocks { get; set; }

        /// <summary>
        /// 物理吸血
        /// </summary>
        [ProtoMember(61)]
        public int Blood
        {
            get
            {
                Bloods = Bloods ?? new List<int>();
                return Bloods.Sum();
            }
            set { }
        }
        /// <summary>
        /// 物理吸血列表-PropertiesType
        /// </summary>
        [ProtoMember(30), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Bloods { get; set; }

        /// <summary>
        /// 射程
        /// </summary>
        [ProtoMember(62)]
        public int Range
        {
            get
            {
                Ranges = Ranges ?? new List<int>();
                return Ranges.Sum();
            }
            set { }
        }
        /// <summary>
        /// 射程列表-PropertiesType
        /// </summary>
        [ProtoMember(31), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> Ranges { get; set; }

        /// <summary>
        /// 攻击速度:毫秒 例如800毫秒，则表示每800毫秒攻击一次
        /// </summary>
        [ProtoMember(63)]
        public int AttackSpeed
        {
            get
            {
                AttackSpeeds = AttackSpeeds ?? new List<int>();
                return AttackSpeeds.Sum();
            }
            set { }
        }
        /// <summary>
        /// 攻击速度列表-PropertiesType
        /// </summary>
        [ProtoMember(32), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> AttackSpeeds { get; set; }

        /// <summary>
        /// 移动速度:表示走一格需要的毫秒数
        /// </summary>
        [ProtoMember(64)]
        public int MoveSpeed
        {
            get
            {
                MoveSpeeds = MoveSpeeds ?? new List<int>();
                return MoveSpeeds.Sum();
            }
            set { }
        }
        /// <summary>
        /// 移动速度列表-PropertiesType
        /// </summary>
        [ProtoMember(33), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> MoveSpeeds { get; set; }

        /// <summary>
        /// 生命回复-表示每回合过关回复的生命值
        /// </summary>
        [ProtoMember(65)]
        public int HpRecovery
        {
            get
            {
                HpRecoverys = HpRecoverys ?? new List<int>();
                return HpRecoverys.Sum();
            }
            set { }
        }
        /// <summary>
        /// 生命回复列表-PropertiesType
        /// </summary>
        [ProtoMember(34), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> HpRecoverys { get; set; }

        /// <summary>
        /// 气势回复-表示没回合过关回复的气势值
        /// </summary>
        [ProtoMember(66)]
        public int EnergyRecovery
        {
            get
            {
                EnergyRecoverys = EnergyRecoverys ?? new List<int>();
                return EnergyRecoverys.Sum();
            }
            set { }
        }
        /// <summary>
        /// 气势回复列表-PropertiesType
        /// </summary>
        [ProtoMember(35), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> EnergyRecoverys { get; set; }

        /// <summary>
        /// 物理暴击
        /// </summary>
        [ProtoMember(67)]
        public int AdCrit
        {
            get
            {
                AdCrits = AdCrits ?? new List<int>();
                return AdCrits.Sum();
            }
            set { }
        }
        /// <summary>
        /// 物理暴击列表-PropertiesType
        /// </summary>
        [ProtoMember(36), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> AdCrits { get; set; }

        /// <summary>
        /// 主属性
        /// </summary>
        public int MainProperty
        {
            get
            {
                var cfg = SysHeroCfg;
                if (cfg.Type == HeroType.Wu)
                    return cfg.Force;
                else if (cfg.Type == HeroType.Zhi)
                    return cfg.Intel;
                else if (cfg.Type == HeroType.Tong)
                    return cfg.Command;
                return 0;
            }
        }

        /// <summary>
        /// 战斗力
        /// 按最终面板上的五维属性来算  
        /// 包括装备和小兵加成后
        /// 物理攻击*10+法术强度*10+护甲*100+法抗*100+生命*1加上技能增加的战斗力   
        /// skill1lev*100+skill2lev*150+skill3lev*200+skill4lev*250
        /// </summary>
        [ProtoMember(68)]
        public int Combat
        {
            get
            {
                //return Ad * 10 + Ap * 10 + AdArm * 100 + ApArm * 100 + Hp * 1 + SkillLevelList[0] * 100 +
                //       SkillLevelList[1] * 150 + SkillLevelList[2] * 200 + SkillLevelList[3] * 250;
                return
                    (int)
                        ((Ad + Ap) * 2.0 / 3 + (AdArm + ApArm) * 2.0 + Hp * 1.0 / 10 + HpRecovery * 1.0 / 16 + EnergyRecovery * 10.0 / 75 +
                         SkillLevelList.Sum() * 2.0 + Block + Blood + AdCrit + ApCrit);
            }
            set { }
        }
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
        /// 士兵配置表
        /// </summary>
        public SysArmyCfg SysArmyCfg
        {
            get
            {
                var cfg = SysArmyCfg.Find(SysHeroCfg.ArmyId);
                if (cfg == null)
                    throw new ApplicationException(string.Format("系统武将id[{1}]士兵Id[{0}]不存在", SysHeroCfg.ArmyId,
                        SysHeroCfg.Id));
                return cfg;
            }
        }
        /// <summary>
        /// 英雄类型
        /// </summary>
        public HeroType SysHeroType
        {
            get { return SysHeroCfg.Type; }
        }
        #endregion

        /// <summary>
        /// 获取自身拥有的碎片数
        /// </summary>
        public int GetChipNum(UserChip userChip = null)
        {
            var cfg = SysHeroCfg;

            if (userChip == null) userChip = Storage.Load<UserChip>(Pid, true);
            var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == cfg.ChipId);
            if (chipItem != null) return chipItem.Num;
            return 0;
        }

        /// <summary>
        /// 刷新全部属性
        /// </summary>
        public void RefreshProperties()
        {
            RefreshHeroProperties();
            RefreshEquipProperties();
            RefreshSkillProperties();
            RefreshArmyProperties();
            RefreshPetProperties();
        }

        /// <summary>
        /// 判断武将是否符合
        /// </summary>
        /// <param name="type"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool JudgeHeroConform(BattleType type, out string errorMsg)
        {
            errorMsg = "";
            var sysHeroCfg = SysHeroCfg;
            var sysArmyCfg = SysArmyCfg;
            switch (type)
            {
                case BattleType.Woman:
                    if (sysHeroCfg.Gender == GenderType.Female) return true;
                    errorMsg = ResourceId.R_NotFemaleHero;
                    break;
                case BattleType.Wu:
                    if (sysHeroCfg.Type == HeroType.Wu) return true;
                    errorMsg = ResourceId.R_NotWuHero;
                    break;
                case BattleType.Zhi:
                    if (sysHeroCfg.Type == HeroType.Zhi) return true;
                    errorMsg = ResourceId.R_NotZhiero;
                    break;
                case BattleType.Tong:
                    if (sysHeroCfg.Type == HeroType.Tong) return true;
                    errorMsg = ResourceId.R_NotTongHero;
                    break;
                case BattleType.Rider:
                    if (sysArmyCfg.ArmyType == ArmyType.Rider) return true;
                    errorMsg = ResourceId.R_TakeArmyNotRider;
                    break;
                case BattleType.Infantry:
                    if (sysArmyCfg.ArmyType == ArmyType.Infantry) return true;
                    errorMsg = ResourceId.R_TakeArmyNotInfantry;
                    break;
                case BattleType.Armor:
                    if (sysArmyCfg.ArmyType == ArmyType.Armor) return true;
                    errorMsg = ResourceId.R_TakeArmyNotArmor;
                    break;
                case BattleType.Spell:
                    if (sysArmyCfg.ArmyType == ArmyType.Spell) return true;
                    errorMsg = ResourceId.R_TakeArmyNotSpell;
                    break;
                case BattleType.Machinery:
                    if (sysArmyCfg.ArmyType == ArmyType.Machinery) return true;
                    errorMsg = ResourceId.R_TakeArmyNotMachinery;
                    break;
                case BattleType.LightArmor:
                    if (sysArmyCfg.DefendType == DefendType.LightArmor) return true;
                    errorMsg = ResourceId.R_TakeArmyNotCloth;
                    break;
                case BattleType.Cloth:
                    if (sysArmyCfg.DefendType == DefendType.Cloth) return true;
                    errorMsg = ResourceId.R_TakeArmyNotLightArmor;
                    break;
                case BattleType.HeavyArmor:
                    if (sysArmyCfg.DefendType == DefendType.HeavyArmor) return true;
                    errorMsg = ResourceId.R_TakeArmyNotHeavyArmor;
                    break;
                case BattleType.Front:
                    if (sysHeroCfg.Range <= 1) return true;
                    errorMsg = ResourceId.R_NotFrontHero;
                    break;
                case BattleType.After:
                    if (sysHeroCfg.Range > 1) return true;
                    errorMsg = ResourceId.R_NotAfterHero;
                    break;
                default: return true;
            }
            //if (!string.IsNullOrEmpty(errorMsg))
            //{
            //    errorMsg = string.Format("【{0}】{1}", sysHeroCfg.Name, errorMsg);
            //}
            return false;
        }

        /// <summary>
        /// 刷新属性【武将升级、升星】
        /// </summary>
        /// <param name="type">0:升级，1：升星</param>
        public void RefreshHeroProperties(int type = 0)
        {
            var cfg = SysHeroCfg;

            int index = (int)PropertiesType.Hero;

            //升级 武、智、统 成长值
            var aforce = 0.0;
            var aintel = 0.0;
            var acommand = 0.0;
            //var list = SysHeroAdvancedCfg.Items.Where(o => o.HeroId == HeroId && o.UpgradeStar <= StarLevel).ToList();
            var sysHeroAdvancedCfg = SysHeroAdvancedCfg.Items.FirstOrDefault(o => o.HeroId == HeroId && o.UpgradeStar == StarLevel);
            if (sysHeroAdvancedCfg != null)
            {
                //aforce = list.Sum(o => o.ForceGrowup);
                //aintel = list.Sum(o => o.IntelGrowup);
                //acommand = list.Sum(o => o.CommandGrowup);
                aforce = sysHeroAdvancedCfg.ForceGrowup;
                aintel = sysHeroAdvancedCfg.IntelGrowup;
                acommand = sysHeroAdvancedCfg.CommandGrowup;
            }
            else
            {
                aforce = cfg.ForceGrowup;
                aintel = cfg.IntelGrowup;
                acommand = cfg.CommandGrowup;
            }

            var addMultiple = Level - 1;
            Forces[index] = cfg.Force + (int)(addMultiple * aforce);
            Intels[index] = cfg.Intel + (int)(addMultiple * aintel);
            Commands[index] = cfg.Command + (int)(addMultiple * acommand);

            //Ads[index] = cfg.Ad;
            //Aps[index] = cfg.Ap;
            //AdArms[index] = cfg.AdArm;
            //ApArms[index] = cfg.ApArm;
            //Hps[index] = cfg.Hp;
            Ads[index] = 0;
            Aps[index] = 0;
            AdArms[index] = 0;
            ApArms[index] = 0;
            Hps[index] = 0;
            AdCrits[index] = cfg.AdCrit;
            ApCrits[index] = cfg.ApCrit;
            Blocks[index] = cfg.Block;
            Bloods[index] = cfg.Blood;
            Ranges[index] = cfg.Range;
            AttackSpeeds[index] = cfg.AttackSpeed;
            MoveSpeeds[index] = cfg.MoveSpeed;
            HpRecoverys[index] = cfg.HpRecovery;
            EnergyRecoverys[index] = cfg.EnergyRecovery;

            //技能开启的星级限制修改：1星对应技能1，2星对应技能2,3星对应技能3,4星对应技能4
            //modify by hql at 2015.10.24
            SkillLevelList = SkillLevelList ?? new List<int>() { 1, 0, 0, 0 };
            while (SkillLevelList.Count < 4) SkillLevelList.Add(0);
            if (StarLevel >= 1 && SkillLevelList[0] <= 0) SkillLevelList[0] = 1; 
            if (StarLevel >= 2 && SkillLevelList[1] <= 0) SkillLevelList[1] = 1;
            if (StarLevel >= 3 && SkillLevelList[2] <= 0) SkillLevelList[2] = 1;
            if (StarLevel >= 4 && SkillLevelList[3] <= 0) SkillLevelList[3] = 1;
        }

        /// <summary>
        /// 刷新属性【装备更换、装备、卸下、升级、升阶；】
        /// </summary>
        public void RefreshEquipProperties()
        {
            var userEquip = DataStorage.Current.Load<UserEquip>(Pid);

            int index = (int)PropertiesType.Equip;

            if (Ads == null || Ads.Count < (int)PropertiesType.Max) LoadInit();

            Ads[index] = 0;
            Aps[index] = 0;
            AdArms[index] = 0;
            ApArms[index] = 0;
            Hps[index] = 0;

            foreach (var i in EquipIdList)
            {
                if (i > 0)
                {
                    var userEquipItem = userEquip.Items.Find(o => o.Id == i);
                    if (userEquipItem != null)
                    {
                        Hps[index] += userEquipItem.Hp;
                        Ads[index] += userEquipItem.Ad;
                        Aps[index] += userEquipItem.Ap;
                        AdArms[index] += userEquipItem.AdArm;
                        ApArms[index] += userEquipItem.ApArm;
                    }
                }
            }
        }

        /// <summary>
        /// 刷新属性【技能升级】
        /// </summary>
        public void RefreshSkillProperties()
        {
            //attacktype=3 & target=0的才需要显示在面板上！modify by hql at 2015.11.15
            int pindex = (int)PropertiesType.Skill;
            Ads[pindex] = 0;
            Aps[pindex] = 0;
            AdArms[pindex] = 0;
            ApArms[pindex] = 0;
            Forces[pindex] = 0;
            Intels[pindex] = 0;
            Commands[pindex] = 0;
            AdCrits[pindex] = 0;
            ApCrits[pindex] = 0;
            Bloods[pindex] = 0;
            Blocks[pindex] = 0;

            var index = 0;
            foreach (var i in SkillLevelList)
            {
                if (i > 0)
                {
                    var cfg = SysHeroCfg.Find(HeroId);
                    if (cfg == null) throw new Exception(string.Format("SysHeroCfg:Id:{0}", HeroId));
                    var skillId = cfg.SkillIdList[index];
                    var sysSkillCfg = SysSkillCfg.Items.FirstOrDefault(o => o.Id == skillId);
                    if (sysSkillCfg == null) throw new Exception(string.Format("SysSkillCfg:Id:{0}", skillId));

                    if (sysSkillCfg.AttackType == SkillAttackType.Ring && sysSkillCfg.Target == SkillTargetType.Own)
                    {
                        var ringType = sysSkillCfg.RingsType;
                        var attackValueGrowup = sysSkillCfg.AttackValueGrowup;
                        var attackValue = sysSkillCfg.AttackValue;
                        switch (ringType)
                        {
                            case SkillRingsType.AdAura://增加物攻
                                Ads[pindex] += attackValue + (i - 1)*attackValueGrowup;
                                break;
                            case SkillRingsType.ApAura://增加法攻
                                Aps[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.AdArmAura://增加护甲
                                AdArms[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.ApArmAura://增加法抗
                                ApArms[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.ForceAura://增加武力
                                Forces[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.IntelAura://增加智力
                                Intels[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.CommandAura://增加统帅
                                Commands[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.AdCrit://增加物理暴击
                                AdCrits[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.ApCrit://增加法术暴击
                                ApCrits[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.Blood://增加吸血
                                Bloods[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            case SkillRingsType.Block://增加格挡
                                Blocks[pindex] += attackValue + (i - 1) * attackValueGrowup;
                                break;
                            default: break;
                        }
                    }
                }
                index++;
            }
            //DOTO:技能影响武将的属性 在战前战中计算，不实时同步到武将身上！
        }

        /// <summary>
        /// 刷新属性【士兵初始化、升级】
        /// </summary>
        public void RefreshArmyProperties()
        {
            var sysArmyCfg = SysArmyCfg;

            int index = (int)PropertiesType.Army;

            Ads[index] = 0;
            Aps[index] = 0;
            AdArms[index] = 0;
            ApArms[index] = 0;
            Hps[index] = 0;

            var addMultiple = ArmyLevel - 1;
            Ads[index] = sysArmyCfg.Ad + addMultiple * sysArmyCfg.AdGrowup;
            Aps[index] = sysArmyCfg.Ap + addMultiple * sysArmyCfg.ApGrowup;
            AdArms[index] = sysArmyCfg.AdArm + addMultiple * sysArmyCfg.AdArmGrowup;
            ApArms[index] = sysArmyCfg.ApArm + addMultiple * sysArmyCfg.ApArmGrowup;
            Hps[index] = sysArmyCfg.Hp + addMultiple * sysArmyCfg.HpGrowup;
        }

        /// <summary>
        /// 刷新属性【骑宠更换、装备、卸下、升级】
        /// </summary>
        public void RefreshPetProperties()
        {
            int index = (int)PropertiesType.Pet;
            Forces[index] = 0;
            Intels[index] = 0;
            Commands[index] = 0;
            MoveSpeeds[index] = 0;
            AttackSpeeds[index] = 0;
            if (PetId == 0)
            {
                return;
            }
            else
            {
                var userPet = Storage.Load<UserPet>(Pid);
                var userPetItem = userPet.Items.FirstOrDefault(o => o.Id == PetId);
                if (userPetItem != null)
                {
                    SysPetId = userPetItem.PetId;

                    Forces[index] = 0;
                    Intels[index] = 0;
                    Commands[index] = 0;
                    //MoveSpeeds[index] = 0;
                    AttackSpeeds[index] = 0;

                    Forces[index] = userPetItem.Force;
                    Intels[index] = userPetItem.Intel;
                    Commands[index] = userPetItem.Command;
                    //越小移动越快
                    //MoveSpeeds[index] = -userPetItem.MoveSpeed;
                    //越小攻击速度越快
                    AttackSpeeds[index] -= userPetItem.AttackSpeed;
                }
            }
        }

        /// <summary>
        /// 添加武将经验
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="opCode"></param>
        public void AddExp(int exp, int opCode)
        {
            var ignoreUserHeroLevel = ConfigHelper.IgnoreUserHeroLevel;
            //武将等级不能大于主公等级
            var userRole = Storage.Load<UserRole>(Pid);
            int curlevel = Level;
            var userLevel = userRole.Level;
            if (Level >= ignoreUserHeroLevel && userLevel < Level) return;

            //经验已满
            var cfg = SysHeroUpgradeCfg.Items.FirstOrDefault(o => o.Id == curlevel);
            if (cfg == null) throw new ApplicationException(string.Format("SysHeroUpgradeCfg:Id:[{0}]Not Find.", curlevel));
            if (userLevel == Level && Exp >= cfg.NextLvExp)
            {
                Exp = cfg.NextLvExp;
                return;
            }

            //已经最高等级了 经验加到满则终止
            var maxLevel = SysHeroUpgradeCfg.Items.Max(o => o.Id);
            var highestCfg = SysHeroUpgradeCfg.Find(maxLevel);
            if (highestCfg != null && Level == highestCfg.Id)
            {
                Exp += exp;
                if (Exp > highestCfg.NextLvExp) Exp = highestCfg.NextLvExp;
                return;
            }

            int curexp = Exp + exp;
            while (curexp >= cfg.NextLvExp)
            {
                if (curlevel >= maxLevel)
                {
                    curexp = cfg.NextLvExp;
                    break;
                }
                curlevel += 1;
                curexp = curexp - cfg.NextLvExp;
                if (curlevel > ignoreUserHeroLevel)
                {
                    //10级之后 才和主公等级挂钩
                    if (curlevel > maxLevel)
                    {
                        //不能大于最高等级
                        curlevel--;
                        curexp = curexp + cfg.NextLvExp;
                        break;
                    }
                    else if (curlevel > userLevel)
                    {
                        //不能大于主公等级
                        curlevel--;
                        curexp = curexp + cfg.NextLvExp;
                        break;
                    }
                }

                cfg = SysHeroUpgradeCfg.Items.FirstOrDefault(o => o.Id == curlevel);
                if (cfg == null) throw new ApplicationException(string.Format("SysHeroUpgradeCfg:Id:[{0}]Not Find.", curlevel));
            }
            curexp = curexp >= cfg.NextLvExp ? cfg.NextLvExp : curexp;
            //前后等级有改变则计算下数据
            if (curlevel != Level) RefreshProperties();

            var startLevel = Level;
            Level = curlevel;
            var endLevel = Level;
            Exp = curexp;

            //已经最等级了 经验加到满则终止
            if (highestCfg != null && Level == highestCfg.Id)
            {
                if (Exp > highestCfg.NextLvExp) Exp = highestCfg.NextLvExp;
            }

            GameLogManager.ItemLog(Pid, Id, exp, opCode, (int)ItemType.HeroExp, startLevel, endLevel);
        }

        public override void NewObjectInit()
        {
            //初始化技能等级  1星开1,2  三星开3 5星开4
            SkillLevelList = new List<int>() { 1, 0, 0, 0 };
            //State = HeroStateType.Free;
            Level = 1;
            ArmyLevel = 1;
            EquipIdList = new List<int>() { 0, 0, 0, 0 };

            Forces = new List<int>();
            Intels = new List<int>();
            Commands = new List<int>();

            Hps = new List<int>();
            Ads = new List<int>();
            Aps = new List<int>();
            AdArms = new List<int>();
            ApArms = new List<int>();
            AdCrits = new List<int>();
            ApCrits = new List<int>();
            Blocks = new List<int>();
            Bloods = new List<int>();
            Ranges = new List<int>();
            AttackSpeeds = new List<int>();
            MoveSpeeds = new List<int>();
            HpRecoverys = new List<int>();
            EnergyRecoverys = new List<int>();
        }

        public override void LoadInit()
        {
            //技能开启的星级限制修改：1星对应技能1，2星对应技能2,3星对应技能3,4星对应技能4
            //modify by hql at 2015.10.24
            SkillLevelList = SkillLevelList ?? new List<int>() { 1, 0, 0, 0 };
            while (SkillLevelList.Count < 4) SkillLevelList.Add(0);
            if (StarLevel >= 2 && SkillLevelList[1] == 0) SkillLevelList[1] = 1;
            if (StarLevel >= 3 && SkillLevelList[2] == 0) SkillLevelList[2] = 1;
            if (StarLevel >= 4 && SkillLevelList[3] == 0) SkillLevelList[3] = 1;
            if (StarLevel < 2 && SkillLevelList[1] > 0) SkillLevelList[1] = 0;
            if (StarLevel < 3 && SkillLevelList[2] > 0) SkillLevelList[2] = 0;
            if (StarLevel < 4 && SkillLevelList[3] > 0) SkillLevelList[3] = 0;
            EquipIdList = EquipIdList ?? new List<int>() { 0, 0, 0, 0 };

            Forces = Forces ?? new List<int>();
            Intels = Intels ?? new List<int>();
            Commands = Commands ?? new List<int>();

            Hps = Hps ?? new List<int>();
            Ads = Ads ?? new List<int>();
            Aps = Aps ?? new List<int>();
            AdArms = AdArms ?? new List<int>();
            ApArms = ApArms ?? new List<int>();
            AdCrits = AdCrits ?? new List<int>();
            ApCrits = ApCrits ?? new List<int>();
            Blocks = Blocks ?? new List<int>();
            Bloods = Bloods ?? new List<int>();
            Ranges = Ranges ?? new List<int>();
            AttackSpeeds = AttackSpeeds ?? new List<int>();
            MoveSpeeds = MoveSpeeds ?? new List<int>();
            HpRecoverys = HpRecoverys ?? new List<int>();
            EnergyRecoverys = EnergyRecoverys ?? new List<int>();

            while (Forces.Count < (int)PropertiesType.Max + 1) Forces.Add(0);
            while (Intels.Count < (int)PropertiesType.Max + 1) Intels.Add(0);
            while (Commands.Count < (int)PropertiesType.Max + 1) Commands.Add(0);

            while (Hps.Count < (int)PropertiesType.Max + 1) Hps.Add(0);
            while (Ads.Count < (int)PropertiesType.Max + 1) Ads.Add(0);
            while (Aps.Count < (int)PropertiesType.Max + 1) Aps.Add(0);
            while (AdArms.Count < (int)PropertiesType.Max + 1) AdArms.Add(0);
            while (ApArms.Count < (int)PropertiesType.Max + 1) ApArms.Add(0);
            while (AdCrits.Count < (int)PropertiesType.Max + 1) AdCrits.Add(0);
            while (ApCrits.Count < (int)PropertiesType.Max + 1) ApCrits.Add(0);
            while (Blocks.Count < (int)PropertiesType.Max + 1) Blocks.Add(0);
            while (Bloods.Count < (int)PropertiesType.Max + 1) Bloods.Add(0);
            while (Ranges.Count < (int)PropertiesType.Max + 1) Ranges.Add(0);
            while (AttackSpeeds.Count < (int)PropertiesType.Max + 1) AttackSpeeds.Add(0);
            while (MoveSpeeds.Count < (int)PropertiesType.Max + 1) MoveSpeeds.Add(0);
            while (HpRecoverys.Count < (int)PropertiesType.Max + 1) HpRecoverys.Add(0);
            while (EnergyRecoverys.Count < (int)PropertiesType.Max + 1) EnergyRecoverys.Add(0);

            //士兵等级初始化为一级
            if (ArmyLevel == 0) ArmyLevel = 1;
            if (Hps[(int)PropertiesType.Army] == 0)
                RefreshArmyProperties();
            if (Hps[(int)PropertiesType.Hero] == 0)
                RefreshHeroProperties();

            if (StatusEndTime.ToTs() <= 0) Status = HeroStatus.Idle;
        }

        public void ClientLoadInit()
        {
            //技能开启的星级限制修改：1星对应技能1，2星对应技能2,3星对应技能3,4星对应技能4
            //modify by hql at 2015.10.24
            SkillLevelList = SkillLevelList ?? new List<int>() { 1, 0, 0, 0 };
            while (SkillLevelList.Count < 4) SkillLevelList.Add(0);
            if (StarLevel >= 2 && SkillLevelList[1] == 0) SkillLevelList[1] = 1;
            if (StarLevel < 2 && SkillLevelList[1] > 0) SkillLevelList[1] = 0;
            if (StarLevel < 3 && SkillLevelList[2] > 0) SkillLevelList[2] = 0;
            if (StarLevel < 4 && SkillLevelList[3] > 0) SkillLevelList[3] = 0;
            EquipIdList = EquipIdList ?? new List<int>() { 0, 0, 0, 0 };

            Forces = Forces ?? new List<int>();
            Intels = Intels ?? new List<int>();
            Commands = Commands ?? new List<int>();

            Hps = Hps ?? new List<int>();
            Ads = Ads ?? new List<int>();
            Aps = Aps ?? new List<int>();
            AdArms = AdArms ?? new List<int>();
            ApArms = ApArms ?? new List<int>();
            AdCrits = AdCrits ?? new List<int>();
            ApCrits = ApCrits ?? new List<int>();
            Blocks = Blocks ?? new List<int>();
            Bloods = Bloods ?? new List<int>();
            Ranges = Ranges ?? new List<int>();
            AttackSpeeds = AttackSpeeds ?? new List<int>();
            MoveSpeeds = MoveSpeeds ?? new List<int>();
            HpRecoverys = HpRecoverys ?? new List<int>();
            EnergyRecoverys = EnergyRecoverys ?? new List<int>();

            while (Forces.Count < (int)PropertiesType.Max + 1) Forces.Add(0);
            while (Intels.Count < (int)PropertiesType.Max + 1) Intels.Add(0);
            while (Commands.Count < (int)PropertiesType.Max + 1) Commands.Add(0);

            while (Hps.Count < (int)PropertiesType.Max + 1) Hps.Add(0);
            while (Ads.Count < (int)PropertiesType.Max + 1) Ads.Add(0);
            while (Aps.Count < (int)PropertiesType.Max + 1) Aps.Add(0);
            while (AdArms.Count < (int)PropertiesType.Max + 1) AdArms.Add(0);
            while (ApArms.Count < (int)PropertiesType.Max + 1) ApArms.Add(0);
            while (AdCrits.Count < (int)PropertiesType.Max + 1) AdCrits.Add(0);
            while (ApCrits.Count < (int)PropertiesType.Max + 1) ApCrits.Add(0);
            while (Blocks.Count < (int)PropertiesType.Max + 1) Blocks.Add(0);
            while (Bloods.Count < (int)PropertiesType.Max + 1) Bloods.Add(0);
            while (Ranges.Count < (int)PropertiesType.Max + 1) Ranges.Add(0);
            while (AttackSpeeds.Count < (int)PropertiesType.Max + 1) AttackSpeeds.Add(0);
            while (MoveSpeeds.Count < (int)PropertiesType.Max + 1) MoveSpeeds.Add(0);
            while (HpRecoverys.Count < (int)PropertiesType.Max + 1) HpRecoverys.Add(0);
            while (EnergyRecoverys.Count < (int)PropertiesType.Max + 1) EnergyRecoverys.Add(0);
        }
    }

    /// <summary>
    /// 用户英雄
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserHero : KVEntity
    {
        /// <summary>
        /// 英雄实例—对外
        /// </summary>
        //public List<UserHeroItem> ItemList
        //{
        //    get
        //    {
        //        foreach (var userHeroItem in Items)
        //        {
        //            userHeroItem.LoadInit();
        //        }
        //        return Items;
        //    }
        //}

        /// <summary>
        /// 英雄实例【不能直接被访问，这里写成public是因为private属性没法同步到数据库。】
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserHeroItem> Items { get; set; }

        /// <summary>
        /// 今日技能点购买次数
        /// </summary>
        //[ProtoMember(4), PropertyPersist(PersistType = PropertyPersistType.Expand)]
        //public DayZeorValue BuySkillPointNum { get; set; }

        /// <summary>
        /// 刷新参数值【和配置文件中比较，小的话需要重新计算】
        /// </summary>
        [ProtoMember(5)]
        public int RefreshHeroData { get; set; }

        public override void NewObjectInit()
        {
            //var maxSkillPoint = ConfigHelper.HeroCfgData.MaxSkillPoint;
            //var skillInterval = ConfigHelper.HeroCfgData.SkillInterval;
            //SkillPoint = new RecoverValue(1, maxSkillPoint, 0, skillInterval, maxSkillPoint);
            Items = new List<UserHeroItem>();
            RefreshHeroData = ConfigHelper.RefreshHeroData;
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserHeroItem>();
            //去除除武智统类型的武将
            Items.RemoveAll(o => o.SysHeroType == HeroType.Npc);
            //万一有改变 则加载重新计算
            //var maxSkillPoint = ConfigHelper.HeroCfgData.MaxSkillPoint;
            //var skillInterval = ConfigHelper.HeroCfgData.SkillInterval;
            //if (SkillPoint.Max != maxSkillPoint || SkillPoint.Interval != skillInterval)
            //{
            //    SkillPoint = new RecoverValue(1, maxSkillPoint, 0, skillInterval, maxSkillPoint);
            //}

            foreach (var userHeroItem in Items)
            {
                userHeroItem.LoadInit();
            }

            //有更改配置文件后刷新属性
            if (RefreshHeroData < ConfigHelper.RefreshHeroData)
            {
                foreach (var userHeroItem in Items)
                {
                    userHeroItem.RefreshProperties();
                }
                RefreshHeroData = ConfigHelper.RefreshHeroData;
            }
        }

        /// <summary>
        /// 更新武将是否有新信息（可招募、可升阶）
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="userChip"></param>
        /// <returns></returns>
        public int UpdateHeroNewMsg(int heroId, UserChip userChip = null)
        {
            var chipId = heroId + (int)ToolType.HeroChip;
            var chipNum = 0;
            userChip = userChip ?? DataStorage.Current.Load<UserChip>(Id);
            var chipItem = userChip.ChipItems.FirstOrDefault(o => o.ItemId == chipId);
            if (chipItem != null) chipNum = chipItem.Num;

            var userHeroItem = Items.Find(o => o.HeroId == heroId);
            if (userHeroItem == null)
            {
                //判断是否可招募
                var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == chipId);
                if (sysToolCfg != null && chipNum > sysToolCfg.Param2)
                {
                    return 1;
                }
            }
            else
            {
                //判断是否可升阶
                var star = userHeroItem.StarLevel;
                var heroAdvancedCfg =
                    ConfigHelper.HeroAdvancedCfgData.FirstOrDefault(o => o.UpgradeStar == star + 1);
                if (heroAdvancedCfg != null)
                {
                    var needChip = heroAdvancedCfg.ChipNum;
                    if (chipNum >= needChip)
                    {
                        return 1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 更新武将装备是否有新消息（可佩戴）
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="userEquip"></param>
        /// <param name="userPet"></param>
        /// <returns></returns>
        public int UpdateHeroEquipNewMsg(int heroId, UserEquip userEquip = null, UserPet userPet = null)
        {
            var userHeroItem = Items.Find(o => o.HeroId == heroId);
            if (userHeroItem != null)
            {
                if (userEquip == null || userPet == null)
                {
                    DataStorage.Current.Load(out userEquip, out userPet, Id);
                }
                var equipTypes = new List<EquipType>()
                {
                    EquipType.Weapons,
                    EquipType.Clothes,
                    EquipType.Jewelry,
                    EquipType.Magic,
                    EquipType.Pet
                };
                foreach (var iequipType in equipTypes)
                {
                    if (iequipType == EquipType.Weapons || iequipType == EquipType.Clothes ||
                        iequipType == EquipType.Jewelry || iequipType == EquipType.Magic)
                    {
                        var id = userHeroItem.EquipIdList[(int)iequipType - 1];
                        if (id == 0)
                        {
                            var unEquipedList =
                                userEquip.Items.Where(
                                    o => o.EquipType == iequipType && o.HeroId == 0 && o.NeedLevel <= userHeroItem.Level)
                                    .ToList();
                            if (unEquipedList.Count > 0)
                            {
                                return 1;
                            }
                        }
                    }
                    else if (iequipType == EquipType.Pet)
                    {
                        if (userHeroItem.PetId == 0)
                        {
                            var unEquipedList =
                                   userPet.Items.Where(o => o.HeroId == 0 && o.NeedLevel <= userHeroItem.Level).ToList();
                            if (unEquipedList.Count > 0)
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// 改变将军府新消息
        /// </summary>
        /// <param name="userRole"></param>
        public void ChangeNewMsg(UserRole userRole = null)
        {
            userRole = userRole ?? DataStorage.Current.Load<UserRole>(Id, true);
            var hasNewMsg = HasNewMsg();
            userRole.SetHasNewMsg((int)NewMsgType.Manor, hasNewMsg);
        }

        /// <summary>
        /// 是否有新消息
        /// </summary>
        /// <returns></returns>
        public int HasNewMsg()
        {
            UserChip userChip;
            UserEquip userEquip;
            UserPet userPet;
            DataStorage.Current.Load(out userChip, out userEquip, out userPet, Id);
            foreach (var userHeroItem in Items.OrderByDescending(o => o.Level).ToList())
            {
                var hasNewMsg = UpdateHeroNewMsg(userHeroItem.HeroId, userChip);
                if (hasNewMsg == 1) return 1;
                hasNewMsg = UpdateHeroEquipNewMsg(userHeroItem.HeroId, userEquip, userPet);
                if (hasNewMsg == 1) return 1;
            }

            return 0;
        }

        /// <summary>
        /// 获取武将信息
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public UserHeroItem FindByHeroId(int heroId)
        {
            UserHeroItem item = Items.Find(o => o.HeroId == heroId);
            return item;
        }

        /// <summary>
        /// 添加武将
        /// </summary>
        /// <param name="heroId">武将Id</param>
        /// <param name="opCode">接口id</param>
        /// <returns></returns>
        public UserHeroItem AddHeroToUser(int heroId, int opCode)
        {
            var sysHeroCfg = SysHeroCfg.Find(heroId);
            if (sysHeroCfg == null) throw new ApplicationException(string.Format("sysHeroCfg:Id:{0} NOT FIND", heroId));
            var haveHeroItem = Items.FirstOrDefault(o => o.HeroId == heroId);
            if (haveHeroItem != null)
            {
                return null;
            }
            //添加新武将
            var heroItem = KVEntity.CreateNew<UserHeroItem>();
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysHeroCfg, UserHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysHeroCfg, UserHeroItem>(new[] { "Id" }));
            mapper.Map(sysHeroCfg, heroItem);
            heroItem.HeroId = sysHeroCfg.Id;
            heroItem.StarLevel = sysHeroCfg.InitStarLevel;
            heroItem.Pid = Id;
            Items.Add(heroItem);

            //添加任务新达成
            Utility.AddMainLineTaskGoalData(Id, MainLineType.RecruitHeroNum, Items.Count);

            if (sysHeroCfg.Quality >= ItemQuality.Purple)
            {
                var userRole = Storage.Load<UserRole>(Id);
                //广播
                var msg = "";
                var color = Utility.GetQualityColor(sysHeroCfg.Quality);
                switch (opCode)
                {
                    case 4004:
                        //合成
                        msg = LangResource.GetLangResource(ResourceId.R_0000_MixtureHeroMsg, userRole.Id,
                                                             userRole.NickName, sysHeroCfg.Id, sysHeroCfg.Name, color); break;
                    case 5001:
                        //寻访
                        msg = LangResource.GetLangResource(ResourceId.R_0000_ExtractHeroMsg, userRole.Id,
                                                             userRole.NickName, sysHeroCfg.Id, sysHeroCfg.Name, color); break;
                }
                if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
            }

            GameLogManager.ItemLog(Id, heroId, 1, opCode, (int)ItemType.Hero, 0, 0);
            return heroItem;
        }
    }

    /// <summary>
    /// 武将属性的类型
    /// </summary>
    public enum PropertiesType
    {
        /// <summary>
        /// 武将本身【等级、星级】
        /// </summary>
        Hero = 0,
        /// <summary>
        /// 武将装备【等级、阶级】
        /// </summary>
        Equip = 1,
        /// <summary>
        /// 武将技能【等级】
        /// </summary>
        Skill = 2,
        /// <summary>
        /// 士兵【等级】
        /// </summary>
        Army = 3,
        /// <summary>
        /// 骑宠【等级】
        /// </summary>
        Pet = 4,

        /// <summary>
        /// 类型最大值
        /// </summary>
        Max = 4,
    }
}
