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
    /// 战役
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class Battle : KVEntity
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
        /// 参数一【竞技场：攻击方排名】
        /// </summary>
        [ProtoMember(16)]
        public int Param1 { get; set; }
        /// <summary>
        /// 参数二【竞技场：防守方排名】
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
        /// <summary>
        /// 时间帧列表
        /// </summary>
        [Tag(13)]
        [ProtoMember(20), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> TimeFrameIdList { get; set; }
        /// <summary>
        /// 时间帧对应的武将列表
        /// </summary>
        [Tag(14)]
        [ProtoMember(21), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> HeroIdList { get; set; }
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

        public override void NewObjectInit()
        {
            //AttackerName = AttackerHeroIdArray = AttackerLocationIdArray = "";
            //DefenderName = DefenderHeroIdArray = DefenderLocationIdArray = "";
            TimeFrameIdList = TimeFrameIdList ?? new List<int>();
            HeroIdList = HeroIdList ?? new List<int>();
            BattleAttackerHeroItemIdList = BattleAttackerHeroItemIdList ?? new List<int>();
            BattleDefenderHeroItemIdList = BattleDefenderHeroItemIdList ?? new List<int>();
            CreateTime = DateTime.Now;
            IsWin = -1;//初始化
        }
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

        public void LoadDataFromUserHeroItem(UserHeroItem userHeroItem)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<UserHeroItem, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<UserHeroItem, BattleHeroItem>(new[] { "Id" }));
            mapper.Map(userHeroItem, this);

            if (SysPetId == 0) SysPetId = 5000001;
        }

        public void LoadDataFromSysNpcHeroCfg(SysNpcHeroCfg sysNpcHeroCfg)
        {
            //映射属性
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysNpcHeroCfg, BattleHeroItem>(
                    new DefaultMapConfig().IgnoreMembers<SysNpcHeroCfg, BattleHeroItem>(new[] { "Id" }));
            mapper.Map(sysNpcHeroCfg, this);

            SkillLevelList = sysNpcHeroCfg.SkillLevelList;

            if (SysPetId == 0) SysPetId = 5000001;
        }
    }

    /// <summary>
    /// 战斗用户信息项
    /// </summary>
    public class BattleUserItem
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Tag(2)]
        public int UserId { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        [Tag(3)]
        public int Level { get; set; }
        /// <summary>
        /// 用户头像id
        /// </summary>
        [Tag(4)]
        public int HeadId { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        [Tag(5)]
        public string NickName { get; set; }
    }
}
