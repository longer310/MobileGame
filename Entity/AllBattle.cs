using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 全服玩家战斗/副本/大地图产生的战役
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class AllBattle : KVEntity
    {
        /// <summary>
        /// 战役id
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public WarType WarType { get; set; }
        /// <summary>
        /// 攻击方id
        /// </summary>
        [Tag(2)]
        [ProtoMember(2)]
        public int AttackerId { get; set; }
        /// <summary>
        /// 攻击方等级
        /// </summary>
        [Tag(3)]
        [ProtoMember(3)]
        public int AttackerLevel { get; set; }
        /// <summary>
        /// 攻击方头像id
        /// </summary>
        [Tag(4)]
        [ProtoMember(4)]
        public int AttackerHeadId { get; set; }
        /// <summary>
        /// 攻击方昵称
        /// </summary>
        [Tag(5)]
        [ProtoMember(5)]
        public string AttackerName { get; set; }
        /// <summary>
        /// 攻击方英雄列表【1,2,3,4】
        /// </summary>
        //[ProtoMember(6)]
        //public string AttackerHeroIdArray { get; set; }
        /// <summary>
        /// 攻击方位置列表【11,12,22,23】
        /// </summary>
        //[ProtoMember(7)]
        // public string AttackerLocationIdArray { get; set; }
        /// <summary>
        /// 防守方id
        /// </summary>
        [Tag(6)]
        [ProtoMember(8)]
        public int DefenderId { get; set; }
        /// <summary>
        /// 防守方等级
        /// </summary>
        [Tag(7)]
        [ProtoMember(10)]
        public int DefenderLevel { get; set; }
        /// <summary>
        /// 防守方头像id
        /// </summary>
        [Tag(8)]
        [ProtoMember(11)]
        public int DefenderHeadId { get; set; }
        /// <summary>
        /// 防守方昵称
        /// </summary>
        [Tag(9)]
        [ProtoMember(12)]
        public string DefenderName { get; set; }
        /// <summary>
        /// 防守方英雄列表【1,2,3,4】
        /// </summary>
        //[ProtoMember(13)]
        //public string DefenderHeroIdArray { get; set; }
        /// <summary>
        /// 防守方位置列表【11,12,22,23】
        /// </summary>
        //[ProtoMember(14)]
        //public string DefenderLocationIdArray { get; set; }
        /// <summary>
        /// 攻击方是否胜利
        /// </summary>
        [Tag(10)]
        [ProtoMember(15)]
        public int IsWin { get; set; }
        /// <summary>
        /// 参数一【竞技场：攻击方排名；大地图：系统城池ID；大地图复仇：战报ID】
        /// </summary>
        [ProtoMember(16)]
        public int Param1 { get; set; }
        /// <summary>
        /// 参数二【竞技场：防守方排名,大地图：是否已复仇】
        /// </summary>
        [ProtoMember(17)]
        public int Param2 { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Tag(11)]
        [ProtoMember(18)]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 战斗评分
        /// </summary>
        [Tag(12)]
        [ProtoMember(19)]
        public int BattleScore { get; set; }
        ///// <summary>
        ///// 时间帧列表
        ///// </summary>
        //[ProtoMember(20), PropertyPersist(PersistType = PropertyPersistType.Json)]
        //public List<int> TimeFrameIdList { get; set; }
        ///// <summary>
        ///// 时间帧对应的武将列表
        ///// </summary>
        //[ProtoMember(21), PropertyPersist(PersistType = PropertyPersistType.Json)]
        //public List<int> HeroIdList { get; set; }
        /// <summary>
        /// 战役攻击方武将项id列表
        /// </summary>
        [ProtoMember(22), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> BattleAttackerHeroItemIdList { get; set; }
        /// <summary>
        /// 战役防守方武将项id列表
        /// </summary>
        [ProtoMember(23), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> BattleDefenderHeroItemIdList { get; set; }

        /// <summary>
        /// 战役回合id列表
        /// </summary>
        [ProtoMember(24), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> RoundIdList { get; set; }

        /// <summary>
        /// 战役回合id列表
        /// </summary>
        [ProtoMember(25), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ListIntItem> RoundIdListList { get; set; }
        /// <summary>
        /// 战役防守方武将项id列表
        /// </summary>
        [ProtoMember(26), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<ListIntItem> DefenderHeroItemIdListList { get; set; }

        /// <summary>
        /// 攻打领地玩家战役：建筑id列表；攻打城池玩家战役：内政信息列表
        /// </summary>
        [ProtoMember(29), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ListIntParam1 { get; set; }

        /// <summary>
        /// 攻打领地玩家战役：妃子id列表
        /// </summary>
        [ProtoMember(30), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> ListIntParam2 { get; set; }

        public override void NewObjectInit()
        {
            BattleAttackerHeroItemIdList = new List<int>();
            BattleDefenderHeroItemIdList = new List<int>();
            RoundIdListList = new List<ListIntItem>();
            DefenderHeroItemIdListList = new List<ListIntItem>();
            ListIntParam1 = new List<int>();
            ListIntParam2 = new List<int>();
            CreateTime = DateTime.Now;
            IsWin = -1;//初始化
        }

        public override void LoadInit()
        {
            BattleAttackerHeroItemIdList = BattleAttackerHeroItemIdList ?? new List<int>();
            BattleDefenderHeroItemIdList = BattleDefenderHeroItemIdList ?? new List<int>();
            RoundIdListList = RoundIdListList ?? new List<ListIntItem>();
            DefenderHeroItemIdListList = DefenderHeroItemIdListList ?? new List<ListIntItem>();
            ListIntParam1 = ListIntParam1 ?? new List<int>();
            ListIntParam2 = ListIntParam2 ?? new List<int>();
        }

        public void DeleteHeroAndRound()
        {
            var aHeroList = BattleAttackerHeroItemIdList;
            var dHeroList = BattleDefenderHeroItemIdList;
            aHeroList = aHeroList ?? new List<int>();
            dHeroList = dHeroList ?? new List<int>();
            aHeroList.AddRange(dHeroList);
            foreach (var i in aHeroList)
            {
                DataStorage.Current.MarkDeleted<BattleHeroItem>(i);
            }
            RoundIdList = RoundIdList ?? new List<int>();
            foreach (var i in RoundIdList)
            {
                DataStorage.Current.MarkDeleted<BattleRound>(i);
            }
        }
    }

    [ProtoContract]
    public class ListIntItem
    {
        public ListIntItem()
        {
            IdItems = new List<int>();
        }
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> IdItems { get; set; }
    }

    /// <summary>
    /// 战役英雄详情
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class BattleHeroItem : KVEntity
    {
        /// <summary>
        /// 武将Id
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public int HeroId { get; set; }

        /// <summary>
        /// 阵型位置
        /// </summary>
        [Tag(2)]
        [ProtoMember(2)]
        public LocationNumber Location { get; set; }

        /// <summary>
        /// 武将等级
        /// </summary>
        [Tag(3)]
        [ProtoMember(3)]
        public int Level { get; set; }

        /// <summary>
        /// 星级
        /// </summary>
        [Tag(4)]
        [ProtoMember(4)]
        public int StarLevel { get; set; }

        /// <summary>
        /// 士兵等级
        /// </summary>
        [Tag(5)]
        [ProtoMember(5)]
        public int ArmyLevel { get; set; }

        /// <summary>
        /// 技能等级列表
        /// </summary>
        [Tag(6)]
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> SkillLevelList { get; set; }

        /// <summary>
        /// 武
        /// </summary>
        [Tag(10)]
        [ProtoMember(10)]
        public int Force { get; set; }

        /// <summary>
        /// 智
        /// </summary>
        [Tag(11)]
        [ProtoMember(11)]
        public int Intel { get; set; }

        /// <summary>
        /// 统
        /// </summary>
        [Tag(12)]
        [ProtoMember(12)]
        public int Command { get; set; }

        /// <summary>
        /// 物理伤害
        /// </summary>
        [Tag(13)]
        [ProtoMember(13)]
        public int Ad { get; set; }

        /// <summary>
        /// 法术伤害
        /// </summary>
        [Tag(14)]
        [ProtoMember(14)]
        public int Ap { get; set; }

        /// <summary>
        /// 物理护甲
        /// </summary>
        [Tag(15)]
        [ProtoMember(15)]
        public int AdArm { get; set; }

        /// <summary>
        /// 法术护甲
        /// </summary>
        [Tag(16)]
        [ProtoMember(16)]
        public int ApArm { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        [Tag(17)]
        [ProtoMember(17)]
        public int Hp { get; set; }

        /// <summary>
        /// 物理暴击-伤害加倍
        /// </summary>
        [Tag(18)]
        [ProtoMember(18)]
        public int AdCrit { get; set; }

        /// <summary>
        /// 法术暴击-伤害加倍
        /// </summary>
        [Tag(19)]
        [ProtoMember(19)]
        public int ApCrit { get; set; }

        /// <summary>
        /// 物理格挡
        /// </summary>
        [Tag(20)]
        [ProtoMember(20)]
        public int Block { get; set; }

        /// <summary>
        /// 物理吸血
        /// </summary>
        [Tag(21)]
        [ProtoMember(21)]
        public int Blood { get; set; }

        /// <summary>
        /// 攻击间距
        /// </summary>
        [Tag(22)]
        [ProtoMember(22)]
        public int Range { get; set; }

        /// <summary>
        /// 攻击间隔:毫秒 例如800毫秒，则表示每800毫秒攻击一次
        /// </summary>
        [Tag(23)]
        [ProtoMember(23)]
        public int AttackSpeed { get; set; }

        /// <summary>
        /// 移动速度:毫秒 例如600毫秒，则表示每移动一格需要600毫秒
        /// </summary>
        [Tag(24)]
        [ProtoMember(24)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// 生命回复-表示每回合过关回复的生命值
        /// </summary>
        [Tag(25)]
        [ProtoMember(25)]
        public int HpRecovery { get; set; }

        /// <summary>
        /// 气势回复-表示没回合过关回复的气势值
        /// </summary>
        [Tag(26)]
        [ProtoMember(26)]
        public int EnergyRecovery { get; set; }

        /// <summary>
        /// 攻击顺序【1代表普通攻击、2代表二技能、3代表三技能】
        /// </summary>
        //[Tag(27)]
        //[ProtoMember(27)]
        //public int AttackMode { get; set; }

        /// <summary>
        /// 系统骑宠id【为零代表没有骑宠 使用客户端默认的】
        /// </summary>
        [Tag(28)]
        [ProtoMember(28)]
        public int SysPetId { get; set; }

        /// <summary>
        /// 初始气势值
        /// </summary>
        [Tag(29)]
        [ProtoMember(29)]
        public int InitEnergy { get; set; }

        /// <summary>
        /// 战斗力
        /// </summary>
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
        }

        /// <summary>
        /// 士兵类型
        /// </summary>
        public ArmyType ArmyType
        {
            get
            {
                var sysHeroCfg = SysHeroCfg.Items.FirstOrDefault(o => o.Id == HeroId);
                if (sysHeroCfg != null)
                {
                    var sysArmyCfg = SysArmyCfg.Items.FirstOrDefault(o => o.Id == sysHeroCfg.ArmyId);
                    if (sysArmyCfg != null)
                        return sysArmyCfg.ArmyType;
                }
                return ArmyType.Armor;
            }
        }

        /// <summary>
        /// 士兵防御类型
        /// </summary>
        public DefendType DefendType
        {
            get
            {
                var sysHeroCfg = SysHeroCfg.Items.FirstOrDefault(o => o.Id == HeroId);
                if (sysHeroCfg != null)
                {
                    var sysArmyCfg = SysArmyCfg.Items.FirstOrDefault(o => o.Id == sysHeroCfg.ArmyId);
                    if (sysArmyCfg != null)
                        return sysArmyCfg.DefendType;
                }
                return DefendType.Cloth;
            }
        }

        /// <summary>
        /// 加载玩家英雄信息
        /// </summary>
        /// <param name="userHeroItem"></param>
        /// <param name="serverMapCityItem"></param>
        public void LoadDataFromUserHeroItem(UserHeroItem userHeroItem, ServerMapCityItem serverMapCityItem = null)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserHeroItem, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<UserHeroItem, BattleHeroItem>(new[] { "Id" }));
            mapper.Map(userHeroItem, this);

            if (serverMapCityItem != null)
            {
                var bigMapDefenseCfg = ConfigHelper.BigMapDefenseCfgData;
                var dif = 0;
                var m = 0;
                var prec = 0;
                var value1 = 0;
                var value2 = 0;

                //护甲、法抗
                dif = (serverMapCityItem.Defense - 50) > 0
                    ? serverMapCityItem.Defense - 50
                    : 50 - serverMapCityItem.Defense;
                m = (int)(dif * 1.0 / bigMapDefenseCfg.EachPoint);
                if (m > 0)
                {
                    prec = m * ((serverMapCityItem.Defense > 50)
                        ? bigMapDefenseCfg.AddDefense
                        : bigMapDefenseCfg.DescendingDefense);
                }
                value1 = (int)(AdArm * 1.0 * prec / 100);
                value2 = (int)(ApArm * 1.0 * prec / 100);
                if (serverMapCityItem.Defense > 50) { AdArm += value1; ApArm += value2; }
                else { AdArm -= value1; ApArm -= value2; }


                //带兵数
                dif = (serverMapCityItem.Army - 50) > 0
                    ? serverMapCityItem.Army - 50
                    : 50 - serverMapCityItem.Army;
                m = (int)(dif * 1.0 / bigMapDefenseCfg.EachPoint);
                if (m > 0)
                {
                    prec = m * ((serverMapCityItem.Army > 50)
                        ? bigMapDefenseCfg.AddArmy
                        : bigMapDefenseCfg.DescendingArmy);
                }
                value1 = (int)(Hp * 1.0 * prec / 100);
                if (serverMapCityItem.Army > 50) { Hp += value1; }
                else { Hp -= value1; }

                //开场气势
                m = (int)(serverMapCityItem.Morale.Value * 1.0 / bigMapDefenseCfg.EachPoint);
                if (m > 0)
                {
                    value1 = m * bigMapDefenseCfg.AddMorale;
                }
                InitEnergy += value1;
            }
        }

        /// <summary>
        /// 加载副本NPC英雄信息
        /// </summary>
        /// <param name="sysLevelHeroCfg"></param>
        public void LoadDataFromSysNpcHeroCfg(SysLevelHeroCfg sysLevelHeroCfg)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysLevelHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysLevelHeroCfg, BattleHeroItem>(new[] { "Id" }));
            sysLevelHeroCfg.Init();
            mapper.Map(sysLevelHeroCfg, this);

            //var mapper2 =
            //    ObjectMapperManager.DefaultInstance.GetMapper<SysNpcHeroCfg, BattleHeroItem>(
            //        new DefaultMapConfig().IgnoreMembers<SysNpcHeroCfg, BattleHeroItem>(new[] { "Id" }));
            //var sysNpcHeroCfg = sysNpcFormationCfg.GetSysNpcHeroCfg();
            //mapper2.Map(sysNpcHeroCfg, this);

            SkillLevelList = sysLevelHeroCfg.SkillLevelList;
            SysPetId = sysLevelHeroCfg.PetId;

            //if (SysPetId == 0) SysPetId = ConfigHelper.BattleDefaultPetId;
        }

        /// <summary>
        /// 加载暗黑军团活动NPC英雄信息
        /// </summary>
        /// <param name="sysDiabloHeroCfg"></param>
        public void LoadDataFromSysDiabloHeroCfg(SysDiabloHeroCfg sysDiabloHeroCfg)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysDiabloHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysDiabloHeroCfg, BattleHeroItem>(new[] { "Id" }));
            sysDiabloHeroCfg.Init();
            mapper.Map(sysDiabloHeroCfg, this);

            SkillLevelList = sysDiabloHeroCfg.SkillLevelList;
            SysPetId = sysDiabloHeroCfg.PetId;
        }

        /// <summary>
        /// 加载大地图NPC英雄信息
        /// </summary>
        /// <param name="sysBigMapNpcHeroCfg"></param>
        public void LoadDataFromSysNpcHeroCfg(SysBigMapHeroCfg sysBigMapNpcHeroCfg)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysBigMapHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysBigMapHeroCfg, BattleHeroItem>(new[] { "Id" }));

            sysBigMapNpcHeroCfg.Init();
            mapper.Map(sysBigMapNpcHeroCfg, this);

            //var mapper2 =
            //    ObjectMapperManager.DefaultInstance.GetMapper<SysNpcHeroCfg, BattleHeroItem>(
            //        new DefaultMapConfig().IgnoreMembers<SysNpcHeroCfg, BattleHeroItem>(new[] { "Id" }));
            //var sysNpcHeroCfg = sysNpcFormationCfg.GetSysNpcHeroCfg();
            //mapper2.Map(sysNpcHeroCfg, this);

            SkillLevelList = sysBigMapNpcHeroCfg.SkillLevelList;
            SysPetId = sysBigMapNpcHeroCfg.PetId;

            //if (SysPetId == 0) SysPetId = ConfigHelper.BattleDefaultPetId;
        }

        /// <summary>
        /// 加载竞技场NPC英雄信息
        /// </summary>
        /// <param name="sysPkHeroCfg"></param>
        public void LoadDataFromSysNpcHeroCfg(SysPkHeroCfg sysPkHeroCfg)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysPkHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysPkHeroCfg, BattleHeroItem>(new[] { "Id" }));
            sysPkHeroCfg.Init();
            mapper.Map(sysPkHeroCfg, this);

            //var mapper2 =
            //    ObjectMapperManager.DefaultInstance.GetMapper<SysNpcHeroCfg, BattleHeroItem>(
            //        new DefaultMapConfig().IgnoreMembers<SysNpcHeroCfg, BattleHeroItem>(new[] { "Id" }));
            //var sysNpcHeroCfg = sysNpcFormationCfg.GetSysNpcHeroCfg();
            //mapper2.Map(sysNpcHeroCfg, this);

            SkillLevelList = sysPkHeroCfg.SkillLevelList;
            SysPetId = sysPkHeroCfg.PetId;

            //if (SysPetId == 0) SysPetId = ConfigHelper.BattleDefaultPetId;
        }

        /// <summary>
        /// 大地图加载城池NPC英雄信息
        /// </summary>
        /// <param name="defendItem"></param>
        public void LoadDataFromDefendItem(DefendItem defendItem)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<DefendItem, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<DefendItem, BattleHeroItem>(new[] { "Id" }));
            mapper.Map(defendItem, this);

            var mapper2 =
                ObjectMapperManager.DefaultInstance.GetMapper<SysBigMapHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysBigMapHeroCfg, BattleHeroItem>(new[] { "Id" }));
            var sysBigMapNpcHeroCfg = defendItem.GetSysBigMapNpcHeroCfg();
            sysBigMapNpcHeroCfg.Init();
            mapper2.Map(sysBigMapNpcHeroCfg, this);

            SkillLevelList = sysBigMapNpcHeroCfg.SkillLevelList;
            SysPetId = sysBigMapNpcHeroCfg.PetId;

            //if (SysPetId == 0) SysPetId = ConfigHelper.BattleDefaultPetId;
        }
    }

    /// <summary>
    /// 战役回放英雄信息
    /// </summary>
    public class ResBattleHeroItem
    {
        /// <summary>
        /// 武将Id
        /// </summary>
        [Tag(1)]
        public int HeroId { get; set; }

        /// <summary>
        /// 阵型位置
        /// </summary>
        [Tag(2)]
        public LocationNumber Location { get; set; }

        /// <summary>
        /// 武将等级
        /// </summary>
        [Tag(3)]
        public int Level { get; set; }

        /// <summary>
        /// 星级
        /// </summary>
        [Tag(4)]
        public int StarLevel { get; set; }

        /// <summary>
        /// 生命
        /// </summary>
        [Tag(5)]
        public int Hp { get; set; }

        /// <summary>
        /// 系统骑宠id【为零代表没有骑宠 使用客户端默认的】
        /// </summary>
        [Tag(6)]
        public int SysPetId { get; set; }

        /// <summary>
        /// 初始气势值
        /// </summary>
        [Tag(7)]
        public int InitEnergy { get; set; }

        /// <summary>
        /// 技能等级列表
        /// </summary>
        [Tag(8)]
        public List<int> SkillLevelList { get; set; }

        /// <summary>
        /// 生命回复-表示每回合过关回复的生命值
        /// </summary>
        [Tag(9)]
        public int HpRecovery { get; set; }

        /// <summary>
        /// 气势回复-表示没回合过关回复的气势值
        /// </summary>
        [Tag(9)]
        public int EnergyRecovery { get; set; }

        /// <summary>
        /// 战斗力
        /// </summary>
        [Tag(10)]
        public int Combat { get; set; }

        /// <summary>
        /// 攻击速度
        /// </summary>
        [Tag(11)]
        public int AttackSpeed { get; set; }

        public void LoadDataFromBattleHeroItem(BattleHeroItem battleHeroItem)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<BattleHeroItem, ResBattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<BattleHeroItem, ResBattleHeroItem>(new[] { "Id" }));
            mapper.Map(battleHeroItem, this);
            Combat = battleHeroItem.Combat;
            AttackSpeed = battleHeroItem.AttackSpeed;
            SkillLevelList = SkillLevelList ?? new List<int>();
        }
    }

    public class ResBattleRoundItem
    {
        /// <summary>
        /// 回合数
        /// </summary>
        [Tag(1)]
        public int Round { get; set; }
    }
}
