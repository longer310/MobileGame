using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 用户阵型
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserFormation : KVEntity
    {
        /// <summary>
        /// 解锁位置编号列表
        /// </summary>
        //[ProtoMember(2), PropertyPersist(PersistType = PropertyPersistType.Json)]
        //public List<LocationNumber> LockLocationNumbers { get; set; }

        /// <summary>
        /// 最后一次攻击阵型
        /// </summary>
        [ProtoMember(3), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<FormationItem> AttFormation { get; set; }

        /// <summary>
        /// 最后一次攻击阵型战斗力值
        /// </summary>
        public int AttCombat
        {
            get
            {
                AttFormation = AttFormation ?? new List<FormationItem>();
                if (AttFormation.Count > 0)
                {
                    var userHero = DataStorage.Current.Load<UserHero>(Id);
                    return userHero.Items.Where(o => AttFormation.Select(p => p.HeroId).Contains(o.HeroId))
                        .Sum(o => o.Combat);
                }
                return 0;
            }
        }

        /// <summary>
        /// 攻击历史阵型中最大战斗力值
        /// </summary>
        [ProtoMember(4)]
        public int AttMaxCombat { get; set; }

        /// <summary>
        /// 最强的攻击阵型
        /// </summary>
        [ProtoMember(5), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<FormationItem> StrongestFormation { get; set; }

        /// <summary>
        /// 竞技场防守阵型
        /// </summary>
        [ProtoMember(6), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<FormationItem> DefFormation { get; set; }

        /// <summary>
        /// 大地图防守阵型
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<FormationItem> BigMapDefFormation { get; set; }

        /// <summary>
        /// 大地图攻击阵型类型列表
        /// </summary>
        [ProtoMember(8), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<CityFormationItem> BigMapAttFormationItems { get; set; }

        /// <summary>
        /// 防守阵型战斗力值
        /// </summary>
        public int DefCombat
        {
            get
            {
                DefFormation = DefFormation ?? new List<FormationItem>();
                if (DefFormation.Count > 0)
                {
                    var userHero = DataStorage.Current.Load<UserHero>(Id);
                    return userHero.Items.Where(o => DefFormation.Select(p => p.HeroId).Contains(o.HeroId))
                        .Sum(o => o.Combat);
                }
                return 0;
            }
        }

        /// <summary>
        /// 获取大地图不同城池类型的攻击阵型
        /// </summary>
        /// <param name="battleType"></param>
        /// <returns></returns>
        public List<FormationItem> GetFormationItemsByBattleType(BattleType battleType)
        {
            var item = BigMapAttFormationItems.FirstOrDefault(o => o.BattleType == battleType);
            if (item == null) return new List<FormationItem>();
            else
            {
                return item.FormationItems;
            }
        }

        /// <summary>
        /// 设置最强战力和战力阵型
        /// </summary>
        /// <param name="maxAttCombat"></param>
        /// <param name="strongestFormation"></param>
        public void SetMaxComatAndFormation(int maxAttCombat, List<FormationItem> strongestFormation)
        {
            StrongestFormation = strongestFormation;
            AttMaxCombat = maxAttCombat;

            //加入排行榜
            Utility.SetCombatScore(Id, AttMaxCombat);
        }

        /// <summary>
        /// 设置最后一次/大地图不同城池类型的攻击类型
        /// </summary>
        /// <param name="battleType"></param>
        /// <param name="heroIdList"></param>
        /// <param name="formationList"></param>
        /// <param name="newTotalCombat"></param>
        public void SetAttFormations(BattleType battleType, List<int> heroIdList, List<int> formationList, int newTotalCombat)
        {
            if (battleType == BattleType.None)
            {
                AttFormation.Clear();

                var index = 0;
                foreach (var i in heroIdList)
                {
                    var formation = formationList[index];

                    AttFormation.Add(new FormationItem()
                    {
                        HeroId = i,
                        Location = (LocationNumber)formation,
                    });
                    index++;
                }

                if (newTotalCombat > AttMaxCombat)
                {
                    StrongestFormation = AttFormation;
                    AttMaxCombat = newTotalCombat;
                    //加入排行榜
                    Utility.SetCombatScore(Id, AttMaxCombat);
                }
            }
            else
            {
                BigMapAttFormationItems.RemoveAll(o => o.BattleType == battleType);
                var item = new CityFormationItem();
                var index = 0;
                item.BattleType = battleType;
                foreach (var i in heroIdList)
                {
                    var formation = formationList[index];

                    item.FormationItems.Add(new FormationItem()
                    {
                        HeroId = i,
                        Location = (LocationNumber)formation,
                    });
                    index++;
                }
                BigMapAttFormationItems.Add(item);
            }
        }

        public override void NewObjectInit()
        {
            StrongestFormation = new List<FormationItem>();
            AttFormation = new List<FormationItem>();
            DefFormation = new List<FormationItem>();
            BigMapDefFormation = new List<FormationItem>();
            BigMapAttFormationItems = new List<CityFormationItem>();
        }

        public override void LoadInit()
        {
            StrongestFormation = StrongestFormation ?? new List<FormationItem>();
            AttFormation = AttFormation ?? new List<FormationItem>();
            DefFormation = DefFormation ?? new List<FormationItem>();
            BigMapDefFormation = BigMapDefFormation ?? new List<FormationItem>();
            BigMapAttFormationItems = BigMapAttFormationItems ?? new List<CityFormationItem>();
        }
    }

    [ProtoContract]
    public class FormationItem
    {
        /// <summary>
        /// 位置 11、12、21、22、23、31、32
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public LocationNumber Location { get; set; }
        /// <summary>
        /// 系统英雄id
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public int HeroId { get; set; }
    }

    /// <summary>
    /// 大地图 城池 攻击阵型存储
    /// </summary>
    [ProtoContract]
    public class CityFormationItem
    {
        public CityFormationItem()
        {
            FormationItems = new List<FormationItem>();
            BattleType = BattleType.None;
        }
        /// <summary>
        /// 城池战斗类型
        /// </summary>
        [ProtoMember(1)]
        [Tag(1)]
        public BattleType BattleType { get; set; }
        /// <summary>
        /// 阵型列表
        /// </summary>
        [ProtoMember(2)]
        [Tag(2)]
        public List<FormationItem> FormationItems { get; set; }
    }

    public enum LocationNumber
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 11
        /// </summary>
        Ln11 = 11,
        /// <summary>
        /// 12
        /// </summary>
        Ln12 = 12,
        /// <summary>
        /// 21
        /// </summary>
        Ln21 = 21,
        /// <summary>
        /// 22
        /// </summary>
        Ln22 = 22,
        /// <summary>
        /// 23
        /// </summary>
        Ln23 = 23,
        /// <summary>
        /// 31
        /// </summary>
        Ln31 = 31,
        /// <summary>
        /// 32
        /// </summary>
        Ln32 = 32,
    }
}
