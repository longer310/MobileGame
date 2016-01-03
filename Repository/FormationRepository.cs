using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi;
using MobileGame.tianzi.ConfigStruct;

namespace MobileGame.tianzi.Repository
{
    #region 8000 获得布阵界面信息
    /// <summary>
    /// 获得布阵界面信息
    /// </summary>
    public class GetFormationResponse
    {
        public GetFormationResponse()
        {
            OwnItem = new UserItem();
            TargetItem = new UserDetailItem();
            //UnLockLocationNumbers = new List<LocationNumber>();
            HeroItems = new List<FormationHeroItem>();
        }
        /// <summary>
        /// 自己布阵
        /// </summary>
        [Tag(1)]
        public UserItem OwnItem { get; set; }

        /// <summary>
        /// 敌方信息及其布阵【UserId是否大于零判断是否存在敌方阵型】
        /// </summary>
        [Tag(2)]
        public UserDetailItem TargetItem { get; set; }

        /// <summary>
        /// 解锁位置编号列表
        /// </summary>
        //[Tag(3)]
        //public List<LocationNumber> UnLockLocationNumbers { get; set; }

        /// <summary>
        /// 自己武将列表
        /// </summary>
        [Tag(4)]
        public List<FormationHeroItem> HeroItems { get; set; }

        public class UserDetailItem
        {
            public UserDetailItem()
            {
                NickName = "";
                DetailFitems = new List<FormationDetailItem>();
            }
            /// <summary>
            /// 用户战力
            /// </summary>
            [Tag(1)]
            public int Combat { get; set; }
            /// <summary>
            /// 布阵列表
            /// </summary>
            [Tag(2)]
            public List<FormationDetailItem> DetailFitems { get; set; }
            /// <summary>
            /// 用户id
            /// </summary>
            [Tag(3)]
            public int UserId { get; set; }
            /// <summary>
            /// 用户头像id
            /// </summary>
            [Tag(4)]
            public int HeadId { get; set; }
            /// <summary>
            /// 用户等级
            /// </summary>
            [Tag(5)]
            public int Level { get; set; }
            /// <summary>
            /// 用户昵称
            /// </summary>
            [Tag(6)]
            public string NickName { get; set; }
        }

        public class UserItem
        {
            public UserItem()
            {
                Fitems = new List<FormationItem>();
            }
            /// <summary>
            /// 用户战力
            /// </summary>
            [Tag(1)]
            public int Combat { get; set; }
            /// <summary>
            /// 布阵列表
            /// </summary>
            [Tag(2)]
            public List<FormationItem> Fitems { get; set; }
        }

        public class FormationHeroItem
        {
            /// <summary>
            /// 系统武将id
            /// </summary>
            [Tag(1)]
            public int HeroId { get; set; }
            /// <summary>
            /// 星级
            /// </summary>
            [Tag(2)]
            public int StarLevel { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            [Tag(3)]
            public int Level { get; set; }
            /// <summary>
            /// 战斗力
            /// </summary>
            [Tag(4)]
            public int Combat { get; set; }
        }
    }
    /// <summary>
    /// 获得布阵界面信息
    /// </summary>
    [GameCode(OpCode = 8000, ResponseType = typeof(GetFormationResponse))]
    public class GetFormationRequest : GameHandler
    {
        /// <summary>
        /// 布阵场景类型 目前只能传1
        /// </summary>
        public WarType WarType { get; set; }

        /// <summary>
        /// 对手id、关卡id【可不传】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public int Id { get; set; }

        //public override bool InitParams(GameContext context)
        //{
        //    if (WarType != WarType.PkDefend) return false;
        //    return base.InitParams(context);
        //}
        public override void Process(GameContext context)
        {
            var logItem = new GameLogItem();
            logItem.F1 = (int)WarType;
            logItem.F2 = Id;

            var response = new GetFormationResponse();
            UserFormation userFormation;
            UserHero userHero;
            Storage.Load(out userFormation, out userHero, CurrentUserId, true);

            //下方的武将列表
            foreach (var userHeroItem in userHero.Items)
            {
                response.HeroItems.Add(new GetFormationResponse.FormationHeroItem()
                {
                    HeroId = userHeroItem.HeroId,
                    StarLevel = userHeroItem.StarLevel,
                    Level = userHeroItem.Level,
                    Combat = userHeroItem.Combat
                });
            }
            //解锁的位置列表
            //response.UnLockLocationNumbers = new List<LocationNumber>() { LocationNumber.Ln11, LocationNumber.Ln12 };

            //我方阵型
            if (WarType == WarType.PkDefend)
            {
                //布阵
                response.OwnItem.Combat = userFormation.DefCombat;
                response.OwnItem.Fitems = userFormation.DefFormation;
            }
            else
            {
                response.OwnItem.Combat = userFormation.AttCombat;
                response.OwnItem.Fitems = userFormation.AttFormation;
            }

            //敌方阵型
            if (WarType == WarType.PkUser || WarType == WarType.PkNpc)
            {
                //看到阵型则扣除一次竞技场次数
                var userPk = Storage.Load<UserPk>(CurrentUserId, true);
                if (userPk.LaveNum <= 0)
                {
                    SetError(ResourceId.R_0000_BattleNumNotEnough);
                    return;
                }
                if (userPk.LaveTime > 0)
                {
                    //需先清除冷却时间
                    SetError(ResourceId.R_7000_HaveCoolTimeCannotBuy);
                    return;
                }
                userPk.AddUseNum();
                if (WarType == WarType.PkUser)
                {
                    UserRole targetUserRole;
                    UserHero targetUserHero;
                    UserFormation targetUserFormation;
                    Storage.Load(out targetUserFormation, out targetUserRole, out targetUserHero, Id);

                    response.TargetItem.UserId = targetUserRole.Id;
                    response.TargetItem.HeadId = targetUserRole.HeadId; ;
                    response.TargetItem.Level = targetUserRole.Level;
                    response.TargetItem.NickName = targetUserRole.NickName;
                    response.TargetItem.Combat = targetUserFormation.DefCombat;

                    //布阵
                    foreach (var formationItem in targetUserFormation.DefFormation)
                    {
                        var targetHeroItem = targetUserHero.FindByHeroId(formationItem.HeroId);
                        if (targetHeroItem == null)
                        {
                            SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", formationItem.HeroId);
                            return;
                        }
                        response.TargetItem.DetailFitems.Add(new FormationDetailItem()
                        {
                            Location = formationItem.Location,
                            HeroId = formationItem.HeroId,
                            Level = targetHeroItem.Level,
                            StarLevel = targetHeroItem.StarLevel
                        });
                    }
                }
                else if (WarType == WarType.PkNpc)
                {
                    var targetPkNpc = Storage.Load<ServerPkNpc>(Id);

                    response.TargetItem.UserId = targetPkNpc.Id;
                    response.TargetItem.HeadId = targetPkNpc.HeadId; ;
                    response.TargetItem.Level = targetPkNpc.Level;
                    response.TargetItem.NickName = targetPkNpc.NickName;
                    response.TargetItem.Combat = targetPkNpc.DefendCombat;
                    response.TargetItem.DetailFitems = targetPkNpc.FormationHeroItems;
                }
            }

            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion

    #region 8001 保存竞技场/大地图/城池防守阵型
    /// <summary>
    /// 保存竞技场防守阵型
    /// </summary>
    [GameCode(OpCode = 8001)]
    public class SaveFormationRequest : GameHandler
    {
        /// <summary>
        /// 阵型类型
        /// </summary>
        [ParamCheck(Ignore = true)]
        public FormationType FormationType { get; set; }
        /// <summary>
        /// 上场武将列表
        /// </summary>
        public string HeroIdArray { get; set; }

        /// <summary>
        /// 上场武将列表
        /// </summary>
        public List<int> HeroIdList;

        /// <summary>
        /// 武将位置列表
        /// </summary>
        public string LocationIdArray { get; set; }

        /// <summary>
        /// 武将位置列表
        /// </summary>
        public List<int> LocationIdList;

        public override bool InitParams(GameContext context)
        {
            if (HeroIdList.Count != LocationIdList.Count || LocationIdList.Count == 0) return false;
            return true;
        }

        public override void Process(GameContext context)
        {
            var userFormation = Storage.Load<UserFormation>(CurrentUserId, true);

            var defFormation = new List<FormationItem>();
            var index = 0;
            foreach (var i in HeroIdList)
            {
                defFormation.Add(new FormationItem()
                {
                    Location = (LocationNumber)LocationIdList[index],
                    HeroId = i
                });

                index++;
            }

            if (FormationType == FormationType.PkDefend)
            {
                //保存竞技场防御阵型
                userFormation.DefFormation = new List<FormationItem>();
                userFormation.DefFormation = defFormation;
            }
            else if (FormationType == FormationType.BigMapDefend)
            {
                //保存大地图防御阵型
                userFormation.BigMapDefFormation = defFormation;
            }

        }
    }
    #endregion
}
