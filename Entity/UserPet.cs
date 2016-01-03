// -------------------------------------------------------
// Copyright (C) 胡奇龙 版权所有。
// 文 件 名：UserEquip.cs
// 创建标识：2012/11/1 23:39:34 Created by 胡奇龙
// 功能说明：
// 注意事项：
// 
// 更新记录：
// -------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using Newtonsoft.Json;
using ProtoBuf;

namespace MobileGame.tianzi.Entity
{
    /// <summary>
    /// 骑宠项信息
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserPetItem : EntityItem
    {
        /// <summary>
        /// 系统骑宠id
        /// </summary>
        [ProtoMember(1)]
        public int PetId { get; set; }
        /// <summary>
        /// 等级——初始化为1级
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }
        /// <summary>
        /// 装备到的武将id(系统id)
        /// </summary>
        [ProtoMember(3)]
        public int HeroId { get; set; }

        #region 属性
        /// <summary>
        /// 武
        /// </summary>
        [ProtoMember(20)]
        public int Force { get; set; }
        /// <summary>
        /// 智
        /// </summary>
        [ProtoMember(21)]
        public int Intel { get; set; }
        /// <summary>
        /// 统
        /// </summary>
        [ProtoMember(22)]
        public int Command { get; set; }
        /// <summary>
        /// 移动速度
        /// </summary>
        //[ProtoMember(23)]
        //public int MoveSpeed { get; set; }
        /// <summary>
        /// 攻击速度（加到武将身上）
        /// </summary>
        public int AttackSpeed
        {
            get
            {
                return SysPetCfg.AttackSpeed;
            }
        }

        /// <summary>
        /// 系统骑宠配置
        /// </summary>
        public SysPetCfg SysPetCfg
        {
            get
            {
                var cfg = SysPetCfg.Find(PetId);
                if (cfg == null) throw new Exception(string.Format("Not Find SysPetCfg:Id:{0}", PetId));
                return cfg;
            }
        }

        /// <summary>
        /// 穿戴所需武将等级
        /// </summary>
        public int NeedLevel
        {
            get { return SysPetCfg.NeedLevel; }
        }
        #endregion

        /// <summary>
        /// 刷新属性【骑宠升级】
        /// </summary>
        /// <param name="type">0:升级，1：升阶</param>
        public void RefreshProperties(int type = 0)
        {
            var cfg = SysPetCfg;

            Force = 0;
            Intel = 0;
            Command = 0;
            //MoveSpeed = 0;

            //var addMultiple = Level - 1;

            Force = cfg.Force;
            Intel = cfg.Intel;
            Command = cfg.Command;
            //MoveSpeed = cfg.MoveSpeed;

            if (HeroId > 0)
            {
                var ueseHero = Storage.Load<UserHero>(Pid, true);
                var userHeroItem = ueseHero.FindByHeroId(HeroId);
                if (userHeroItem != null)
                {
                    //刷新武将信息
                    userHeroItem.RefreshPetProperties();
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public override void NewObjectInit()
        {
            Level = 1;
            //Rank = HeroId = 0;
        }

        public override void LoadInit()
        {
        }
    }

    /// <summary>
    /// 用户骑宠
    /// </summary>
    [ProtoContract]
    [EntityPersist]
    public class UserPet : KVEntity
    {
        /// <summary>
        /// 骑宠列表
        /// </summary>
        [ProtoMember(1), PropertyPersist(PersistType = PropertyPersistType.List)]
        public List<UserPetItem> Items { get; set; }

        /// <summary>
        /// 容量
        /// </summary>
        [ProtoMember(2)]
        public int Capacity { get; set; }

        /// <summary>
        /// 骑宠列表-对外
        /// </summary>
        //public List<UserPetItem> ItemList
        //{
        //    get
        //    {
        //        foreach (var userPetItem in Items)
        //        {
        //            userPetItem.LoadInit();
        //        }
        //        return Items;
        //    }
        //}

        /// <summary>
        /// 剩余容量
        /// </summary>
        public int LaveCapacity { get { return Capacity - Items.Count(o => o.HeroId == 0); } }

        public override void NewObjectInit()
        {
            Items = new List<UserPetItem>();
            Capacity = 10;
        }

        public override void LoadInit()
        {
            Items = Items ?? new List<UserPetItem>();
        }

        /// <summary>
        /// 当前列表是否已满
        /// </summary>
        [JsonIgnore]
        public bool IsFull
        {
            get { return Items.Count(o=>o.HeroId == 0) >= Capacity; }
        }

        /// <summary>
        /// 添加骑宠
        /// </summary>
        /// <param name="petId">骑宠Id</param>
        /// <param name="opCode">接口id</param>
        /// <param name="confirm">是否需要判断背包容量</param>
        /// <returns></returns>
        public UserPetItem AddPetToUser(int petId, int opCode, bool confirm = true)
        {
            var sysPetCfg = SysPetCfg.Find(petId);
            if (sysPetCfg == null) throw new ApplicationException(string.Format("SysPetCfg:Id:{0} NOT FIND", petId));
            if (IsFull && confirm) return null;
            var petItem = CreateNew<UserPetItem>();
            var mapper =
                ObjectMapperManager.DefaultInstance.GetMapper<SysPetCfg, UserPetItem>(
                    new DefaultMapConfig().IgnoreMembers<SysPetCfg, UserPetItem>(new[] { "Id" }));
            mapper.Map(sysPetCfg, petItem);
            petItem.Pid = Id;
            petItem.PetId = petId;
            Items.Add(petItem);

            var userHero = DataStorage.Current.Load<UserHero>(Id);
            userHero.ChangeNewMsg();
            GameLogManager.ItemLog(Id, petId, 1, opCode, (int)ItemType.Pet, 0, 0);
            return petItem;
        }

        /// <summary>
        /// 删除骑宠
        /// </summary>
        /// <param name="userPetItem"></param>
        /// <param name="opCode">接口</param>
        /// <returns></returns>
        public bool RemovePet(UserPetItem userPetItem, int opCode)
        {
            Items.Remove(userPetItem);

            var userHero = DataStorage.Current.Load<UserHero>(Id);
            userHero.ChangeNewMsg();
            GameLogManager.ItemLog(Id, userPetItem.PetId, -1, opCode, (int)ItemType.Pet, 0, 0);
            return true;
        }
    }
}
