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
    /// 生产的竞技场Npc
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class PkNpc : KVEntity
    {
        /// <summary>
        /// 防守阵型战斗力值
        /// </summary>
        [ProtoMember(2)]
        public int DefendCombat { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [ProtoMember(3)]
        public string NickName { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        [ProtoMember(4)]
        public int Level { get; set; }

        /// <summary>
        /// 头像id
        /// </summary>
        [ProtoMember(5)]
        public int HeadId { get; set; }

        /// <summary>
        /// 生成的武将信息
        /// </summary>
        [ProtoMember(7), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<int> NpcHeroIdList { get; set; }

        /// <summary>
        /// 排行
        /// </summary>
        [ProtoMember(9)]
        public int Rank { get; set; }

        /// <summary>
        /// NPC武将阵型信息
        /// </summary>
        [ProtoMember(10), PropertyPersist(PersistType = PropertyPersistType.Json)]
        public List<FormationDetailItem> FormationHeroItems { get; set; }

        public override void NewObjectInit()
        {
            Rank = Id;
            //根据ID初始化生成阵容、等级、战力值
            NpcHeroIdList = new List<int>();
            FormationHeroItems = new List<FormationDetailItem>();

            var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= Id && o.EndRank >= Id);
            if (sysPkCfg != null)
            {
                var startLevel = sysPkCfg.StarLevel;
                var endLevel = sysPkCfg.EndLevel;
                var list = SysNpcCfg.Items.Where(o => o.Level >= startLevel && o.Level <= endLevel).ToList();
                if (list.Any())
                {
                    var random = Util.GetRandom(0, list.Count() - 1);
                    var sysNpcCfg = list[random];

                    NickName = NameHelper.GetSingleName(0);
                    Level = sysNpcCfg.Level;
                    HeadId = Util.GetRandom(1, 9);

                    var npcHeroList = SysNpcHeroCfg.Items.Where(o => o.NpcId == sysNpcCfg.Id).ToList();
                    foreach (var sysNpcHeroCfg in npcHeroList)
                    {
                        NpcHeroIdList.Add(sysNpcHeroCfg.Id);
                        DefendCombat += sysNpcHeroCfg.Combat;

                        //武将
                        FormationHeroItems.Add(new FormationDetailItem()
                        {
                            Location = sysNpcHeroCfg.Location,
                            HeroId = sysNpcHeroCfg.HeroId,
                            StarLevel = sysNpcHeroCfg.StarLevel,
                            Level = sysNpcHeroCfg.Level
                        });
                    }
                }
            }
        }

        public override void LoadInit()
        {
        }
    }

    /// <summary>
    /// 武将阵型信息【位置、武将id、等级、星级】
    /// </summary>
    [ProtoContract]
    public class FormationDetailItem
    {
        /// <summary>
        /// 位置 11、12、21、22、23、31、32
        /// </summary>
        [Tag(1)]
        [ProtoMember(1)]
        public LocationNumber Location { get; set; }
        /// <summary>
        /// 系统武将id
        /// </summary>
        [Tag(2)]
        [ProtoMember(2)]
        public int HeroId { get; set; }
        /// <summary>
        /// 星级
        /// </summary>
        [Tag(3)]
        [ProtoMember(3)]
        public int StarLevel { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(4)]
        [ProtoMember(4)]
        public int Level { get; set; }
    }
}
