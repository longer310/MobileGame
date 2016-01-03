using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using MobileGame.Core.Dapper;
using MobileGame.Core.Logging;

namespace MobileGame.tianzi.Repository
{
    #region 添加的定时任务执行
    public class AllAddDbTask : ScheduleTaskBase
    {
        public override void DoWork(HashDict<string> param)
        {
            DbTaskType type = (DbTaskType)Convert.ToInt32(param["Type"]);
            int userId = 0;
            userId = Convert.ToInt32(param["UserId"]);
            GameLogItem logItem = new GameLogItem();
            logItem.F1 = (int)type;
            switch (type)
            {
                case DbTaskType.ManageAddIngot://管理员给特定人添加元宝 改变/不改变vip
                    {
                        int ingotValue2 = Convert.ToInt32(param["IngotValue"]);
                        int cardType = 2;//影响vip且算作正常充值！！！不然合服元宝合并会出问题。
                        if (param["AffectVip"] != null && !Convert.ToBoolean(param["AffectVip"])) cardType = 3;

                        UserRole userRole = Storage.Load<UserRole>(userId, true);
                        RoleManager.AddGiveMoney(userRole, ingotValue2, cardType);

                        logItem.F2 = ingotValue2;
                        logItem.F3 = cardType;

                        var s2 = GameApplication.Instance.FindSessionByUserId(userId);
                        if (s2 != null) s2.Notify();
                    }
                    break;
                case DbTaskType.AddItems://添加妃子、武将、骑宠、装备、道具碎片
                    {
                        BagItemType bagItemType = (BagItemType)Convert.ToInt32(param["ItemType"]);
                        List<ItemPair> items = JsonConvert.DeserializeObject<List<ItemPair>>(Convert.ToString(param["Items"]));

                        foreach (var itemPair in items)
                        {
                            Utility.AddItemToUser(userId, itemPair, 1000000);
                        }

                        logItem.F1 = (int)type;
                        logItem.F2 = (int)bagItemType;
                        logItem.S1 = JsonConvert.SerializeObject(items);
                    }
                    break;
            }
            GameLogManager.CommonLog((int)SpecialLogType.GameTask, userId, 0, logItem);
        }
    }
    #endregion

    #region 登陆 1000
    public class LoginResponse
    {
        /// <summary>
        /// SessionId
        /// </summary>
        [Tag(1)]
        public string Sid { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        [Tag(2)]
        public int UserId { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        [Tag(3)]
        public string NickName { get; set; }
        /// <summary>
        /// 头像Id
        /// </summary>
        [Tag(4)]
        public int HeadId { get; set; }
        /// <summary>
        /// 服务器Id
        /// </summary>
        [Tag(5)]
        public int ServerId { get; set; }
        /// <summary>
        /// 是否是游客[0:非游客，1：游客]
        /// </summary>
        [Tag(6)]
        public int IsVisitor { get; set; }
        /// <summary>
        /// 定时请求的接口列表
        /// </summary>
        [Tag(7)]
        public List<int> TimingOpcodeList { get; set; }
    }
    /// <summary>
    /// 登陆 1000
    /// </summary>
    [GameCode(OpCode = 1000, ResponseType = typeof(LoginResponse))]
    public class LoginRequest : GameHandler
    {
        public LoginRequest()
        {
            NeedAuthenticate = false;
        }

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// 平台
        /// </summary>
        public MobileType MobileType { get; set; }
        /// <summary>
        /// 客户端版本号1
        /// </summary>
        public int V1 { get; set; }
        /// <summary>
        /// 客户端版本号2
        /// </summary>
        public int V2 { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// 渠道Id
        /// </summary>
        public string ChannelId { get; set; }

        public override void Process(GameContext context)
        {
            //var task = new MidnightTask();
            //task.TestCompute();
            //return;

            BusinessUtil.LoginProcess(context, UserName, Pwd, UniqueId, V1, V2, MobileType, ChannelId);
            if (Response.StatusCode != StatusCode.Success) return;

            UserRole userRole;
            UserTask userTask;
            GMMail gmMail;
            Storage.Load(out userRole, out userTask, out gmMail, CurrentUserId, true);

            if (userRole.ModifyErrorNum < 10)
            {
                ////////修复错误 竞技场！！！！！！
                Utility.ModifyErrorData(CurrentUserId);
                ///////
            }

            //封号异常，重新检测一次
            if(userRole.TitleName == null)
                BigMapCommon.CheckAndSetTitleLevel(userRole);

            userTask.ChangeDailyNewMsg(userRole);//每日任务新信息

            //改变新消息
            gmMail.ChangeNewMsg(userRole);

            ResultObj = new LoginResponse
            {
                Sid = context.Session.SessionId,
                UserId = context.Session.UserId,
                NickName = context.Session.NickName,
                HeadId = userRole.HeadId,
                ServerId = ParamHelper.ServerId,
                IsVisitor = RoleManager.IsVisitor(CurrentUserId) ? 1 : 0,
                TimingOpcodeList = ConfigHelper.TimingOpcodeList,
            };
            var logItem = new GameLogItem();
            logItem.F1 = 1;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }

    #endregion

    #region 创建角色 1001
    /// <summary>
    /// 创建角色 1001
    /// </summary>
    [GameCode(OpCode = 1001)]
    public class CreateRoleRequest : GameHandler
    {
        public CreateRoleRequest()
        {
            NeedAuthenticate = false;
        }

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 性别：0：未知，1：男，2：女
        /// </summary>
        public Gender Gender { get; set; }
        /// <summary>
        /// 头像Id
        /// </summary>
        public int HeadId { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// 渠道Id
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 平台类型
        /// </summary>
        public MobileType MobileType { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        //public Country Country { get; set; }
        /// <summary>
        /// 第三方帐号：帐号类型
        /// </summary>
        [ParamCheck(Ignore = true)]
        public AccountType AccountType { get; set; }
        /// <summary>
        /// 第三方帐号：帐号Id
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string UserIdentity { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string teleno { get; set; }

        public override bool InitParams(GameContext context)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Pwd)) return false;
            return true;
        }

        public override void Process(GameContext context)
        {
            bool isLegal = true;

            Regex reg = new Regex("/^[\u4e00-\u9fa5]+$/");
            foreach (char c in NickName)
            {
                Match m = reg.Match(NickName);
                if ((!char.IsLetter(c)) && (!char.IsNumber(c)) && !m.Success)  //既不是字母又不是数字的又不是汉字的就认为是特殊字符
                { isLegal = false; }
            }

            if (!isLegal)
            {
                SetError(ResourceId.NickNameHasBadWord);
                return;
            }

            if (Util.BanedWord.HasBadWord(NickName))
            {
                SetError(ResourceId.NickNameHasBadWord);
                return;
            }
            if (teleno == null) teleno = "";
            BusinessUtil.CreateRoleProcess(context, UserName, Pwd, NickName, MobileType, true, UniqueId, ChannelId, HeadId, Gender, AccountType, UserIdentity, teleno);
            if (Response.StatusCode != StatusCode.Success)
            {
                return;
            }
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            userRole.Level = 1;
            userRole.MaxCoin = ConfigHelper.BuildingCfgData.InitCoinCapacity;
            userRole.MaxWood = ConfigHelper.BuildingCfgData.InitWoodCapacity;
            userRole.MaxStone = ConfigHelper.BuildingCfgData.InitStoneCapacity;
            userRole.MaxIron = ConfigHelper.BuildingCfgData.InitIronCapacity;
            //userRole.Coin = ConfigHelper.BuildingCfgData.InitCoin;
            //userRole.Wood = ConfigHelper.BuildingCfgData.InitWood;
            //userRole.Stone = ConfigHelper.BuildingCfgData.InitStone;
            //userRole.Iron = ConfigHelper.BuildingCfgData.InitIron;
            //TODO 初始化 角色
        }
    }
    #endregion

    #region 获取一个可用于注册昵称 1100
    [ProtoContract]
    public class GetRndNameResponse
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(1)]
        public string NickName { get; set; }
    }
    /// <summary>
    /// 获取一个可用于注册昵称 1100
    /// </summary>
    [GameCode(OpCode = 1100, ResponseType = typeof(GetRndNameResponse), NeedTransaction = false)]
    public class GetRndNameRequest : GameHandler
    {
        public GetRndNameRequest()
        {
            NeedAuthenticate = false;
            Gender = Gender.Male;
        }

        /// <summary>
        /// 性别
        /// </summary>
        [ParamCheck(Ignore = true)]
        public Gender Gender { get; set; }

        public override void Process(GameContext context)
        {
            var rndName = NameHelper.GetSingleName((int)Gender);
            rndName = Utility.SuffixNickName(rndName);

            ResultObj = new GetRndNameResponse { NickName = rndName };
        }
    }
    #endregion

    #region 快速注册 1101
    /// <summary>
    /// 快速注册返回值
    /// </summary>
    public class QuickRegisterResponse
    {
        /// <summary>
        /// 账号
        /// </summary>
        [Tag(1)]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        [Tag(2)]
        public string Pwd { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        [Tag(3)]
        public int UserId { get; set; }
        /// <summary>
        /// 用户id 字符串
        /// </summary>
        [Tag(4)]
        public string strUserId { get; set; }
    }
    /// <summary>
    /// 快速注册
    /// </summary>
    [GameCode(OpCode = 1101, ResponseType = typeof(QuickRegisterResponse))]
    public class QuickRegisterRequest : GameHandler
    {
        public QuickRegisterRequest()
        {
            NeedAuthenticate = false;
        }
        /// <summary>
        /// 设备号
        /// </summary>
        public string UniqueId { get; set; }
        /// <summary>
        /// 渠道Id
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 平台类型
        /// </summary>
        public MobileType MobileType { get; set; }
        /// <summary>
        /// 电话号码【可不传】
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string teleno { get; set; }
        public override void Process(GameContext context)
        {
            var logItem = new GameLogItem();

            DateTime now = DateTime.Now;
            string userName = "游客@" + 10000000 + Util.GetSequence(typeof(VisitorId));
            var letter = "";
            for (int i = 0; i < 4; i++)
            {
                var bors = Util.GetRandom(0, 2);
                if (bors == 1) letter += (char)Util.GetRandom(65, 91);//65-90 A-Z
                else letter += (char)Util.GetRandom(97, 123);//97-122 a-z
            }
            string pwd = Util.GetRandom(111111, 999999) + letter;

            logItem.S1 = userName;
            logItem.S2 = pwd;

            RegAccountRequest regAccountRequest = new RegAccountRequest();
            regAccountRequest.NeedAuthenticate = false;
            regAccountRequest.UserName = userName;
            regAccountRequest.Pwd = pwd;
            regAccountRequest.IsVisitor = 1;//游客注册
            regAccountRequest.Process(context);

            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            CreateRoleRequest createRoleRequest = new CreateRoleRequest();
            createRoleRequest.UserName = userName;
            createRoleRequest.Pwd = pwd;
            createRoleRequest.Gender = (Gender)Util.GetRandom(1, 3);//1-2男女随机
            createRoleRequest.NickName = NameHelper.GetSingleName((int)createRoleRequest.Gender);
            if (createRoleRequest.Gender == Gender.Male)
                createRoleRequest.HeadId = Util.GetRandom(1, 5);//男头像id 1-4 如果有改动 这里需要改
            else
                createRoleRequest.HeadId = Util.GetRandom(5, 9);//女头像id 5-8 如果有改动 这里需要改
            createRoleRequest.UniqueId = UniqueId;
            createRoleRequest.ChannelId = ChannelId;
            createRoleRequest.MobileType = MobileType;
            createRoleRequest.teleno = teleno;
            createRoleRequest.Process(context);

            QuickRegisterResponse response = new QuickRegisterResponse();
            response.UserName = userName;
            response.Pwd = pwd;
            response.UserId = CurrentUserId;
            response.strUserId = CurrentUserId.ToString();



            ResultObj = response;
        }
    }
    #endregion

    #region 获取主公详细消息 1102
    public class GetUserInfoResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Tag(1)]
        public int UserId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(2)]
        public string NickName { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(3)]
        public int Level { get; set; }
        /// <summary>
        /// Vip等级
        /// </summary>
        [Tag(4)]
        public int VipLevel { get; set; }
        /// <summary>
        /// 经验值
        /// </summary>
        [Tag(5)]
        public int Exp { get; set; }
        /// <summary>
        /// 头像ID
        /// </summary>
        [Tag(6)]
        public int HeadId { get; set; }
        /// <summary>
        /// 声望——爵位系统 团队战获得
        /// </summary>
        [Tag(7)]
        public int Repute { get; set; }
        /// <summary>
        /// 魅力值
        /// </summary>
        [Tag(8)]
        public int Charm { get; set; }
        /// <summary>
        /// 爵位
        /// </summary>
        [Tag(9)]
        public TitleType TitleType { get; set; }

        /// <summary>
        /// 封号名称
        /// </summary>
        [Tag(10)]
        public string TitleName { get; set; }
    }
    /// <summary>
    /// 获取主公详细消息
    /// </summary>
    [GameCode(OpCode = 1102, ResponseType = typeof(GetUserInfoResponse))]
    public class GetUserInfoRequest : GameHandler
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int Id { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (Id <= 0) return false;
            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            var response = new GetUserInfoResponse();
            var userRole = Storage.Load<UserRole>(Id);
            if (userRole.IsNew)
            {
                SetError(ResourceId.R_0000_IdNotExist, "UserRole:Id", Id);
                return;
            }

            response.UserId = Id;
            response.NickName = userRole.NickName;
            response.Level = userRole.Level;
            response.VipLevel = userRole.VipLevel;
            if (CurrentUserId == Id) response.Exp = userRole.Exp;
            response.HeadId = userRole.HeadId;
            response.Repute = userRole.Repute;
            response.Charm = userRole.Charm;
            response.TitleType = (TitleType)userRole.TitleLevel;
            response.TitleName = userRole.TitleName;

            ResultObj = response;
        }
    }
    #endregion

    #region 更换头像 1103
    /// <summary>
    /// 更换头像
    /// </summary>
    [GameCode(OpCode = 1103)]
    public class SetUserHeadRequest : GameHandler
    {
        /// <summary>
        /// 头像ID
        /// </summary>
        public int HeadId { get; set; }
        public override void Process(GameContext context)
        {
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            if (HeadId == userRole.HeadId) return;

            if (HeadId >= 1 && HeadId <= 8)
            {
            }
            else
            {
                var userHero = Storage.Load<UserHero>(CurrentUserId);
                var userHeroItem = userHero.Items.FirstOrDefault(o => o.HeroId == HeadId);
                if (userHeroItem == null)
                {
                    SetError(ResourceId.R_0000_IdNotExist, "UserHero:HeroId", HeadId);
                    return;
                }

                if (userHeroItem.StarLevel < 5)
                {
                    SetError(ResourceId.R_0000_HeroStarLevelNotEnough);
                    return;
                }
            }
            userRole.HeadId = HeadId;
        }
    }
    #endregion

    #region 打开修改昵称界面 1104
    [ProtoContract]
    public class GetModifyNameInfoResponse
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(1)]
        public string NickName { get; set; }
        /// <summary>
        /// 所需的元宝
        /// </summary>
        [Tag(2)]
        public int NeedMoney { get; set; }
    }
    /// <summary>
    /// 打开修改昵称界面 1104
    /// </summary>
    [GameCode(OpCode = 1104, ResponseType = typeof(GetModifyNameInfoResponse), NeedTransaction = false)]
    public class GetModifyNameInfoRequest : GameHandler
    {
        /// <summary>
        /// 性别
        /// </summary>
        [ParamCheck(Ignore = true)]
        public Gender Gender { get; set; }

        public override void Process(GameContext context)
        {
            var rndName = NameHelper.GetSingleName((int)Gender);
            rndName = Utility.SuffixNickName(rndName);

            var userRole = Storage.Load<UserRole>(CurrentUserId);
            var needMoney = 0;
            if (userRole.ModifyedNickNameNum >= ConfigHelper.MaxModifyNickNameNum)
            {
                needMoney = ConfigHelper.ModifyNickNameNeedIngot;
            }

            ResultObj = new GetModifyNameInfoResponse { NickName = rndName, NeedMoney = needMoney };
        }
    }
    #endregion

    #region 修改昵称 1105
    /// <summary>
    ///  修改昵称 1105
    /// </summary>
    [GameCode(OpCode = 1105)]
    public class ModifyNickNameRequest : GameHandler
    {
        /// <summary>
        /// 新昵称
        /// </summary>
        public string NickName { get; set; }
        public override void Process(GameContext context)
        {
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            //屏蔽关键词
            NickName = Util.BanedWord.ReplaceBadWord(NickName);
            if (NickName.IndexOf('*') >= 0)
            {
                SetError(ResourceId.R_0000_IllegalName);
                return;
            }

            if (userRole.NickName.Equals(NickName))
            {
                //昵称不变
                SetError(ResourceId.R_0000_NickNameNotChange);
                return;
            }
            if (Utility.GetLength(NickName) > ConfigHelper.NickNameLength)
            {
                //昵称太长
                SetError(ResourceId.R_0000_NickNameTooLong);
                return;
            }
            if (RoleManager.GetUserIdByNickName(NickName) > 0)
            {
                //昵称重复
                SetError(ResourceId.R_0000_NickNameDuplex);
                return;
            }
            if (userRole.ModifyedNickNameNum >= ConfigHelper.MaxModifyNickNameNum)
            {
                var needMoney = ConfigHelper.ModifyNickNameNeedIngot;
                if (userRole.TotalMoney < needMoney)
                {//元宝不足
                    SetError(ResourceId.R_0000_MoneyNotEnough);
                    return;
                }
                int subMoney, subGiveMoney;
                RoleManager.Consume(userRole, SpecialStoreId.ModifyNickname, needMoney, 1, out subMoney, out subGiveMoney);
            }
            userRole.ModifyedNickNameNum++;

            //修改昵称
            RoleManager.ChangeNickName(CurrentUserId, NickName);
        }
    }
    #endregion

    #region 修改密码 1106
    /// <summary>
    ///  修改密码 1106
    /// </summary>
    [GameCode(OpCode = 1106)]
    public class ModifyPwdRequest : GameHandler
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        public string OldPwd { get; set; }
        /// <summary>
        /// 新密码
        /// </summary>
        public string NewPwd { get; set; }
        public override void Process(GameContext context)
        {
            if (string.IsNullOrEmpty(NewPwd))
            {
                //新密码不能为空
                SetError(ResourceId.R_0000_NewPwdNotEmpty);
                return;
            }

            var status = RoleManager.ChangePwd(CurrentUserId, OldPwd, NewPwd, true);
            if (status == ChangePwdStatus.OldPwdNotSame)
            {
                //原密码错误
                SetError(ResourceId.R_0000_OldPwdError);
                return;
            }
        }
    }
    #endregion

    #region 1107 游客设置账号密码
    /// <summary>
    /// 游客设置账号密码
    /// </summary>
    [GameCode(OpCode = 1107)]
    public class VisitorSetUserNamePwdRequest : GameHandler
    {
        /// <summary>
        /// 账号名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码 客户端判断两次输入的密码是否一致
        /// </summary>
        public string Pwd { get; set; }
        public override bool InitParams(GameContext context)
        {
            UserName = Util.FilterString(UserName);
            if (UserName.Length < 4 || UserName.Length > 20)
            {
                Response.StatusCode = StatusCode.CommonError;
                Response.Description = LangResource.GetLangResource(BaseResourceId.UserNameInvalid); return false;
            }

            Pwd = Util.FilterString(Pwd);
            if (Pwd.Length < 6 || Pwd.Length > 20)
            {
                Response.StatusCode = StatusCode.CommonError;
                Response.Description = LangResource.GetLangResource(BaseResourceId.PwdInvalid); return false;
            }

            return base.InitParams(context);
        }
        public override void Process(GameContext context)
        {
            RoleManager.VisitorSetUserNamePwd(CurrentUserId, UserName, Pwd);
        }
    }
    #endregion

    #region 发送世界聊天消息 1200
    /// <summary>
    /// 发送世界聊天消息
    /// </summary>
    [GameCode(OpCode = 1200, ResponseType = typeof(GetChatResponse), NeedTransaction = false)]
    public class SendChatRequest : GameHandler
    {
        /// <summary>
        /// 聊天类型
        /// </summary>
        public ChatType ChatType { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 检查当前玩家是否是GM
        /// </summary>
        /// <returns></returns>
        private bool CheckIsGm()
        {
            if (ConfigHelper.GMIDList == "*") return true;
            if (!string.IsNullOrEmpty(ConfigHelper.GMIDList))
            {
                var list = ConfigHelper.GMIDList.Split(new char[] { ',' }).ToList();
                if (list.Contains(CurrentUserId.ToString())) return true;
            }
            Response.StatusCode = StatusCode.CommonError;
            Response.Description = "~!@#$%^&*()_+=-|`＠＼―♂♀※→←↑↓●◎";
            return false;
        }

        public override bool InitParams(GameContext context)
        {
            if (ChatType == ChatType.System) return false;
            return true;
        }

        public override void Process(GameContext context)
        {
            var response = new GetChatResponse();
            var userRole = Storage.Load<UserRole>(CurrentUserId);
            if (userRole.Level < 5)
            {
                ResultObj = response;
                return;
            }
            if (Msg.StartsWith("GM:", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!CheckIsGm()) return;
                DoGM();
            }
            else
            {
                if (ChatType == ChatType.World)
                {
                    ChatManager.SendChatToWorld(CurrentUserId, Msg);
                }
            }

            response.FillPrivateChat(CurrentUserId);
            response.FillWorldChat(CurrentUserId);

            ResultObj = response;
        }
        //gm指令
        private void DoGM()
        {
            Storage.BeginTransaction();

            var args = Msg.Substring(3).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var cmd = args[0];
            var uid = Convert.ToInt32(args[1]);
            if (uid == 0) uid = CurrentUserId;
            var userRole = Storage.Load<UserRole>(uid, true);
            var logItem = new GameLogItem();
            switch (cmd)
            {
                #region MyRegion

                #region 添加道具
                case "additem":
                    {
                        var itemid = Convert.ToInt32(args[2]);
                        var num = Convert.ToInt32(args[3]);
                        var item = SysToolCfg.Find(itemid);
                        if (item != null)
                        {
                            item.AddToUser(uid, Request.OpCode, num);
                        }
                        break;
                    }
                #endregion

                #region 添加装备
                case "addequip":
                    {
                        var itemid = Convert.ToInt32(args[2]);
                        var item = SysEquipCfg.Find(itemid);
                        if (item != null)
                        {
                            var userEquip = Storage.Load<UserEquip>(uid, true);
                            userEquip.AddEquipToUser(itemid, Request.OpCode);
                        }
                        break;
                    }
                #endregion

                #region 添加用户经验
                case "addexp":
                    {
                        var exp = Convert.ToInt32(args[2]);
                        Utility.AddUserExp(userRole, exp, Request.OpCode);
                        break;
                    }
                #endregion

                #region 添加人民币
                case "addmoney":
                    {
                        var num = Convert.ToInt32(args[2]);
                        RoleManager.AddGiveMoney(userRole, num, 2);
                        var s = GameApplication.Instance.FindSessionByUserId(uid);
                        if (s != null)
                            s.Notify();
                        break;
                    }
                #endregion

                #region 添加武将经验
                case "addheroexp":
                    {
                        var index = Convert.ToInt32(args[2]);
                        UserHero userHero = Storage.Load<UserHero>(uid, true);
                        if (userHero.Items.Count - 1 < index)
                        {
                            SetError("第几个武将传参错误，从零开始");
                            return;
                        }
                        UserHeroItem userHeroItem = userHero.Items.OrderByDescending(o => o.Level).ToList()[index];
                        userHeroItem.LoadInit();
                        var exp = Convert.ToInt32(args[3]);
                        userHeroItem.AddExp(exp, Request.OpCode);
                        break;
                    }
                #endregion

                #region 增加铜钱、木材、石头、铁矿
                case "addcoin":
                    {
                        var coin = Convert.ToInt32(args[2]);
                        var wood = Convert.ToInt32(args[3]);
                        var stone = Convert.ToInt32(args[4]);
                        var iron = Convert.ToInt32(args[5]);

                        userRole.Coin += (coin);
                        userRole.Wood += (wood);
                        userRole.Stone += (stone);
                        userRole.Iron += (iron);
                        //Utility.AddResource(userRole, ItemType.Coin, Request.OpCode, coin);
                        //Utility.AddResource(userRole, ItemType.Wood, Request.OpCode, wood);
                        //Utility.AddResource(userRole, ItemType.Stone, Request.OpCode, stone);
                        //Utility.AddResource(userRole, ItemType.Iron, Request.OpCode, iron);
                        break;
                    }
                #endregion

                #region 增加竞技场次数
                case "addpknum":
                    {
                        var userPk = Storage.Load<UserPk>(uid, true);
                        var num = Convert.ToInt32(args[2]);
                        userPk.BuyNum += num;
                        break;
                    }
                #endregion

                #region 添加武将
                case "addhero":
                    {
                        var heroId = Convert.ToInt32(args[2]);
                        UserHero userHero = Storage.Load<UserHero>(uid);
                        if (userHero.FindByHeroId(heroId) == null)
                        {
                            if (userHero.AddHeroToUser(heroId, Request.OpCode) == null)
                            {
                                SetError(ResourceId.R_0000_IllegalParam);
                                return;
                            }
                        }
                        break;
                    }
                #endregion

                #region 添加骑宠
                case "addpet":
                    {
                        var petId = Convert.ToInt32(args[2]);
                        var userPet = Storage.Load<UserPet>(uid);
                        if (userPet.AddPetToUser(petId, Request.OpCode) == null)
                        {
                            SetError(ResourceId.R_0000_IllegalParam);
                            return;
                        }
                        break;
                    }
                #endregion

                #region 添加妃子
                case "addconcubine":
                    {
                        var concubineId = Convert.ToInt32(args[2]);
                        var userConcubine = Storage.Load<UserConcubine>(uid, true);
                        userConcubine.AddConcubineToUser(concubineId, Request.OpCode);
                        break;
                    }
                #endregion

                #region 添加翻牌次数
                case "addflopnum":
                    {
                        var num = Convert.ToInt32(args[2]);
                        var userConcubine = Storage.Load<UserConcubine>(uid, true);
                        userConcubine.BuyFlopNum += num;
                        break;
                    }
                #endregion

                #region 清空玩家妃子
                case "clearconcubine":
                    {
                        var userConcubine = Storage.Load<UserConcubine>(uid, true);
                        userConcubine.ClearItems();
                        break;
                    }
                #endregion

                #region 清空副本开启关卡
                case "clearopenedlevel":
                    {
                        var userLevels = Storage.Load<UserLevels>(uid, true);
                        userLevels.Items.Clear();
                        userLevels.OpenedMaxLevelId = 0;
                        break;
                    }
                #endregion

                #region 添加体力
                case "addsp":
                    {
                        var sp = Convert.ToInt32(args[2]);
                        //Utility.AddResource(userRole, ItemType.Sp, Request.OpCode, sp);
                        userRole.Sp += sp;
                        break;
                    }
                #endregion

                #region 清空碎片&道具中的垃圾数据
                case "clearchipdata":
                    {
                        Utility.ClearRdata(uid);
                        break;
                    }
                #endregion

                #region 大地图全服活动执行
                case "bigmapactivity":
                    {
                        Utility.RefreshMapCityActivity();
                        break;
                    }
                #endregion

                #region 删除战斗回合数以及武将信息
                case "deletebattleroundandhero":
                    {
                        var battleId = Convert.ToInt32(args[2]);
                        var battle = Storage.Load<AllBattle>(battleId, true);

                        battle.DeleteHeroAndRound();
                        break;
                    }
                #endregion

                #region 添加声望
                case "addrepute":
                    {
                        var repute = Convert.ToInt32(args[2]);
                        Utility.AddResource(userRole, ItemType.Repute, Request.OpCode, repute);
                        break;
                    }
                #endregion

                #region 添加魅力
                case "addcharm":
                    {
                        var charm = Convert.ToInt32(args[2]);
                        Utility.AddResource(userRole, ItemType.Charm, Request.OpCode, charm);
                        break;
                    }
                #endregion

                #region 设置领地里面为玩家
                case "setdomainuser":
                    {
                        var targetUserId = 136;
                        if (args.Count() >= 3)
                        {
                            var nickName = args[2];
                            targetUserId = RoleManager.GetUserIdByNickName(nickName);
                            if (targetUserId == 0) targetUserId = 136;
                        }
                        var userCity = Storage.Load<UserCity>(uid, true);
                        var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == 1);
                        if (userDomainItem != null)
                        {
                            targetUserId = uid == targetUserId ? 151 : targetUserId;

                            var targetUserRole = Storage.Load<UserRole>(targetUserId, true);
                            targetUserRole.BeAttackEndTime = DateTime.Now;
                            targetUserRole.ProtectEndTime = DateTime.Now;
                            userDomainItem.InvestigateTime = DateTime.Now.AddSeconds(-7200);

                            userDomainItem.OwnerId = targetUserId;
                            userDomainItem.OwnerType = OwnerType.User;
                        }
                        break;
                    }
                #endregion

                #region 把副本全通关
                case "setlevelfinish":
                    {
                        var userLevels = Storage.Load<UserLevels>(uid, true);
                        userLevels.Items = new List<UserLevelsItem>();

                        var list = SysLevelCfg.Items.OrderBy(o => o.BattleId).ThenBy(o => o.Index).ToList();
                        foreach (var sysLevelCfg in list)
                        {
                            userLevels.AddLevelsItem(sysLevelCfg.Id, 3);
                        }
                        userLevels.OpenedMaxLevelId = SysLevelCfg.Items.Max(o => o.Id);
                        break;
                    }
                #endregion

                #region 重置爵位奖励
                case "resettitlereward":
                    {
                        var userCity = Storage.Load<UserCity>(uid, true);
                        userCity.HasGetTitleReward -= userCity.HasGetTitleReward.Value;
                        break;
                    }
                #endregion

                #region 设置竞技场星星数
                case "setpkstarnum":
                    {
                        var userPk = Storage.Load<UserPk>(uid, true);
                        userPk.StarNum -= userPk.StarNum.Value;
                        userPk.StarNum += 4;
                        break;
                    }
                #endregion

                #region 添加荣誉
                case "addhonor":
                    {
                        var honor = Convert.ToInt32(args[2]);
                        Utility.AddResource(userRole, ItemType.Honor, Request.OpCode, honor);
                        break;
                    }
                #endregion

                #region 刷新缓存
                case "resetcache":
                    {
                        GameCache.Reset();
                        break;
                    }
                #endregion

                #region 设置领地里面为NPC
                case "setdomainnpc":
                    {
                        var npcId = Convert.ToInt32(args[2]);
                        var userCity = Storage.Load<UserCity>(uid, true);
                        var userDomainItem = userCity.DomainItems.FirstOrDefault(o => o.CityId == 1);
                        if (userDomainItem != null)
                        {
                            userDomainItem.InvestigateTime = DateTime.Now.AddSeconds(-7200);
                            userDomainItem.OwnerId = npcId;
                            userDomainItem.OwnerType = OwnerType.Npc;
                        }
                        break;
                    }
                #endregion

                #region 初始化排行榜数据
                case "initrankdata":
                    {
                        Utility.InitAllUserReputeCharmRank();
                        break;
                    }
                #endregion

                #region 下发当天排行榜奖励
                case "issuejjcreward":
                    {
                        var maxRank = SysPkCfg.Items.Max(o => o.EndRank);
                        KeyValuePair<Utility.PkRankKey, double>[] rankList =
                    Storage.SortedSets.Range<Utility.PkRankKey>(Utility.PkSetKey, 1.0, maxRank);
                        rankList = rankList.Where(o => o.Key.Type == (int)BattleTargetType.User).ToArray();

                        var hour = ParamHelper.Get<int>("PkDayRewardDueTime", 21);

                        logItem.S1 = DateTime.Now.ToLongDateString();
                        logItem.S2 = "";
                        logItem.S3 = "";
                        logItem.S4 = "";
                        logItem.S5 = JsonConvert.SerializeObject(rankList);
                        foreach (var keyValuePair in rankList)
                        {
                            var rank = (int)keyValuePair.Value;
                            var userId = keyValuePair.Key.UserId;
                            var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= rank && o.EndRank >= rank);
                            if (sysPkCfg != null)
                            {
                                var attach = new List<ItemPair>();

                                attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Money, Num = sysPkCfg.Money });
                                attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Coin, Num = sysPkCfg.Coin });
                                //attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Charm, Num = sysPkCfg.Charm });
                                attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Honor, Num = sysPkCfg.Honor });
                                attach.Add(new ItemPair() { ItemId = sysPkCfg.ToolId, Num = sysPkCfg.ToolNum });
                                var msg = LangResource.GetLangResource(ResourceId.R_DayRankMsg,
                                                               hour, rank);
                                Utility.SendGmMailToTarget(userId, "竞技场每日排名奖励", msg, attach, DateTime.Now.ToString());

                                logItem.S2 += rank + ",";
                            }
                            else
                            {
                                logItem.S2 += ",";
                            }
                            logItem.S3 += userId + ",";
                            logItem.S4 += rank + ",";
                        }
                        GameLogManager.CommonLog((int)SpecialLogType.PkDayRewardLog, 0, 0, logItem);
                        break;
                    }
                #endregion

                #region 修改VIP等级
                case "setuserviplevel":
                    {
                        var vipLevel = Convert.ToInt32(args[2]);
                        userRole.VipLevel = vipLevel;
                        break;
                    }
                #endregion

                    #endregion
            }

            Storage.Commit();

            logItem.S1 = Msg;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 获取世界聊天&系统广播消息 1201
    public class GetChatResponse
    {
        public GetChatResponse()
        {
            Items = new List<ResChatItem>();
        }
        /// <summary>
        /// 聊天消息列表
        /// </summary>
        [Tag(1)]
        public List<ResChatItem> Items { get; set; }

        /// <summary>
        /// 各个模块有新消息列表【0：代表无消息，大于零代表有】
        /// No.1代表竞技场、No.2代表邮件、No.3代表大地图、No.4代表抽奖
        /// </summary>
        //[Tag(2)]
        //public List<int> HasNewMsgList { get; set; }

        public class ResChatItem
        {
            public ResChatItem()
            {
                Msg = "";
                SenderName = "";
                TitleName = "";
            }
            /// <summary>
            /// 发送者Id
            /// </summary>
            [Tag(1)]
            public int SenderId { get; set; }
            /// <summary>
            /// 发送者名称
            /// </summary>
            [Tag(2)]
            public string SenderName { get; set; }
            /// <summary>
            /// Vip等级
            /// </summary>
            [Tag(3)]
            public int Vip { get; set; }
            /// <summary>
            /// 消息
            /// </summary>
            [Tag(4)]
            public string Msg { get; set; }
            /// <summary>
            /// 发送时间
            /// </summary>
            [Tag(5)]
            public int SendTime { get; set; }
            /// <summary>
            /// 消息类型
            /// </summary>
            [Tag(6)]
            public ChatType ChatType { get; set; }
            /// <summary>
            /// 头像ID
            /// </summary>
            [Tag(7)]
            public int HeadId { get; set; }
            /// <summary>
            /// 等级
            /// </summary>
            [Tag(8)]
            public int Level { get; set; }
            /// <summary>
            /// 爵位封号
            /// </summary>
            [Tag(9)]
            public string TitleName { get; set; }
            /// <summary>
            /// 爵位等级
            /// </summary>
            [Tag(10)]
            public TitleType TitleType { get; set; }
        }

        /// <summary>
        /// 填充具体信息
        /// </summary>
        /// <param name="response"></param>
        /// <param name="chats"></param>
        /// <param name="chatType"></param>
        private void Fill(List<ChatItem> chats, ChatType chatType)
        {
            if (chats.Count > 0)
            {
                var userRoleList =
                    DataStorage.Current.LoadList<UserRole>(
                        chats.Where(o => o.SenderId > 0).Select(p => p.SenderId).ToArray());
                for (int i = 0; i < chats.Count; i++)
                {
                    var chatItem = chats[i];
                    var item = new GetChatResponse.ResChatItem();
                    item.ChatType = chatType;
                    item.Msg = Util.BanedWord.ReplaceBadWord(chatItem.Msg);
                    item.SendTime = (int)chatItem.SendTime.ToUnixTime();
                    item.SenderId = chatItem.SenderId;
                    if (chatItem.SenderId != 0)
                    {
                        var role = userRoleList.FirstOrDefault(o => o.Id == chatItem.SenderId);
                        if (role != null)
                        {
                            item.SenderName = role.NickName;
                            item.Vip = role.RealVipLevel;
                            item.HeadId = role.HeadId;
                            item.Level = role.Level;
                            item.TitleName = role.TitleName;
                            item.TitleType = (TitleType)role.TitleLevel;
                        }
                    }
                    else
                    {
                        item.ChatType = ChatType.System;
                    }
                    Items.Add(item);
                }
            }
        }
        /// <summary>
        /// 世界聊天
        /// </summary>
        /// <param name="currentUserId"></param>
        public void FillWorldChat(int currentUserId)
        {
            var list = ChatManager.GetWorldChatMsgList(currentUserId);
            Fill(list, ChatType.World);
        }
        /// <summary>
        /// 系统广播【只有在线玩家能看到】
        /// </summary>
        /// <param name="currentUserId"></param>
        public void FillPrivateChat(int currentUserId)
        {
            var list = ChatManager.GetPrivateChatMsgList(currentUserId);
            Fill(list, ChatType.System);
        }
    }
    /// <summary>
    /// 获取聊天消息 1201
    /// </summary>
    [GameCode(OpCode = 1201, ResponseType = typeof(GetChatResponse))]
    public class GetChatRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetChatResponse();

            response.FillPrivateChat(CurrentUserId);
            response.FillWorldChat(CurrentUserId);

            //修改新的定时消息
            UserRole userRole;
            UserCity userCity;
            UserExtract userExtract;
            UserTask userTask;
            //UserConcubine userConcubine;, out userConcubine
            GMMail gmMail;
            DataStorage.Current.Load(out userRole, out userCity, out userExtract, out userTask,
                out gmMail, CurrentUserId, true);

            if (userRole.ModifyErrorNum < 10)
            {
                ////////修复错误 竞技场！！！！！！
                Utility.ModifyErrorData(CurrentUserId);
                ///////
            }

            userCity.ChangeNewMsg(userRole);//大地图新消息
            userExtract.ChangeNewMsg(userRole);//抽奖新信息
            userTask.ChangeDailyNewMsg(userRole);//每日任务新信息
            //userConcubine.ChangeNewMsg(userRole);//后宫信息
            gmMail.ChangeNewMsg(userRole);

            ResultObj = response;
        }
    }
    #endregion

    #region 获取系统邮件 1301
    public class GetGMMailResponse
    {
        public GetGMMailResponse()
        {
            Items = new List<MailItem>();
        }
        /// <summary>
        /// 邮件列表
        /// </summary>
        [Tag(1)]
        public List<MailItem> Items { get; set; }

        public class MailItem
        {
            /// <summary>
            /// 消息Id
            /// </summary>
            [Tag(1)]
            public int MsgId { get; set; }
            /// <summary>
            /// 标题
            /// </summary>
            [Tag(2)]
            public string Title { get; set; }
            /// <summary>
            /// 消息内容
            /// </summary>
            [Tag(3)]
            public string Msg { get; set; }
            /// <summary>
            /// 发送时间
            /// </summary>
            [Tag(4)]
            public DateTime SendTime { get; set; }
            /// <summary>
            /// 消息是否已读取过
            /// </summary>
            [Tag(5)]
            public bool HasRead { get; set; }
            /// <summary>
            /// 附件列表【元宝、金币、魅力 10000002-10000004】
            /// </summary>
            [Tag(6)]
            public List<ItemPair> AttachList { get; set; }
            /// <summary>
            /// 邮件类型
            /// </summary>
            [Tag(7)]
            public MailType MailType { get; set; }
            /// <summary>
            /// 是否已领取过附件
            /// </summary>
            [Tag(8)]
            public bool HasGainAttach { get; set; }
        }
    }
    /// <summary>
    /// 获取系统邮件 1301
    /// </summary>
    [GameCode(OpCode = 1301, ResponseType = typeof(GetGMMailResponse))]
    public class GetGMMailRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var response = new GetGMMailResponse();

            GMMail gmMail;
            UserRole userRole;
            Storage.Load(out gmMail, out userRole, CurrentUserId, true);

            //系统邮件
            var list = gmMail.GetMailList(0, 200);
            foreach (var mailItem in list)
            {
                if (mailItem.SendTime.AddDays(30) < DateTime.Now)
                {
                    //30天之前的信息删除
                    gmMail.Delete(mailItem.MsgId);
                    continue;
                }
                var item = new GetGMMailResponse.MailItem();
                item.HasRead = mailItem.HasRead;
                item.Msg = mailItem.Msg;
                item.MsgId = mailItem.MsgId;
                item.SendTime = mailItem.SendTime;
                item.Title = mailItem.Title;
                item.AttachList = new List<ItemPair>();
                if (mailItem.Attach != null) item.AttachList = (List<ItemPair>)mailItem.Attach;
                item.MailType = MailType.Pk;
                item.HasGainAttach = false;

                response.Items.Add(item);
            }

            //公告
            var announcementList = Announcement.Announcements;
            foreach (var announcement in announcementList)
            {
                var item = new GetGMMailResponse.MailItem();
                item.HasRead = false;
                item.Msg = announcement.Msg;
                item.Msg = item.Msg.Replace("\\n", "\n");
                item.MsgId = announcement.Id;
                item.SendTime = announcement.SendTime;
                item.Title = announcement.Title;
                item.AttachList = new List<ItemPair>();
                if (announcement.AttachList != null) item.AttachList = announcement.AttachList;

                if (userRole.ReadedAnnouncementIdList.Contains(announcement.Id))
                {
                    item.HasRead = true;
                }
                item.MailType = MailType.Announcement;
                item.HasGainAttach = gmMail.AnnIdList.Contains(announcement.Id);

                response.Items.Add(item);
            }

            response.Items = response.Items.OrderBy(o => o.HasRead).ThenByDescending(o => o.SendTime).ToList();

            ResultObj = response;
        }
    }
    #endregion

    #region 设置系统邮件已读 1302
    /// <summary>
    /// 设置GM邮件已读 1302
    /// </summary>
    [GameCode(OpCode = 1302)]
    public class SetGMMailReadRequest : GameHandler
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// 邮件类型
        /// </summary>
        public MailType MailType { get; set; }

        public override void Process(GameContext context)
        {
            if (MessageId == 0)
            {
                //不能全部已读
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            if (MailType == MailType.Pk)
            {
                var gmMail = Storage.Load<GMMail>(CurrentUserId, true);
                gmMail.SetRead(MessageId);
            }
            else
            {
                UserRole userRole;
                GMMail gmMail;
                Storage.Load(out userRole, out gmMail, CurrentUserId, true);
                if (!userRole.ReadedAnnouncementIdList.Contains(MessageId))
                {
                    userRole.ReadedAnnouncementIdList.Add(MessageId);
                    //改变新消息
                    gmMail.ChangeNewMsg(userRole);
                }
            }
        }
    }

    #endregion

    #region 领取系统邮件附件【领取完后自动删除】 1303
    /// <summary>
    /// 领取系统邮件附件【领取完后自动删除】 1303
    /// </summary>
    [GameCode(OpCode = 1303)]
    public class DeleteGMMailRequest : GameHandler
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// 邮件类型
        /// </summary>
        [ParamCheck(Ignore = true)]
        public MailType MailType { get; set; }

        public override void Process(GameContext context)
        {
            if (MailType == MailType.None) MailType = MailType.Pk;
            GMMail gmMail = Storage.Load<GMMail>(CurrentUserId, true);
            var attach = new List<ItemPair>();
            if (MailType == MailType.Pk)
            {
                attach = (List<ItemPair>)gmMail.GetAttach(MessageId);
                if (attach == null)
                {
                    SetError(ResourceId.R_1303_MailHaveNotAttach);
                    return;
                }
                gmMail.Delete(MessageId);
            }
            else if (MailType == MailType.Announcement)
            {
                var announcement = Announcement.Announcements.FirstOrDefault(o => o.Id == MessageId);
                if (announcement != null)
                {
                    if (gmMail.AnnIdList.Contains(MessageId))
                    {
                        SetError(ResourceId.R_1303_AnnGetedAttach);
                        return;
                    }
                    gmMail.AnnIdList.Add(MessageId);
                    attach = announcement.AttachList;
                }
            }
            foreach (var itemPair in attach)
            {
                var result = Utility.AddItemToUser(CurrentUserId, itemPair, Request.OpCode);
                if (result != 0)
                {
                    SetError(ResourceId.R_0000_IdNotExist, string.Format("type:{0},id:", result), itemPair.ItemId);
                    return;
                }
                //var itemType = Utility.GetItemType(itemPair.ItemId);
                //var sysToolCfg = SysToolCfg.Find(itemPair.ItemId);
                //if (sysToolCfg == null)
                //{
                //    var sysEquipCfg = SysEquipCfg.Items.FirstOrDefault(o => o.Id == itemPair.ItemId);
                //    if (sysEquipCfg != null)
                //    {
                //        var userEquip = Storage.Load<UserEquip>(CurrentUserId, true);
                //        userEquip.AddEquipToUser(sysEquipCfg.Id, Request.OpCode);
                //    }
                //}
                //else
                //{
                //    sysToolCfg.AddToUser(CurrentUserId, Request.OpCode, itemPair.Num);
                //}
            }


            var logItem = new GameLogItem();
            logItem.F1 = MessageId;
            logItem.S1 = JsonConvert.SerializeObject(attach);
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 获取用户邮件列表 1310
    public class GetUserMailResponse
    {
        public GetUserMailResponse()
        {
            Items = new List<UserMailInfo>();
            MailItems = new List<MailItem>();
        }
        /// <summary>
        /// 会话列表
        /// </summary>
        [Tag(1)]
        public List<UserMailInfo> Items { get; set; }

        /// <summary>
        /// 第一个会话的聊天记录
        /// </summary>
        [Tag(2)]
        public List<MailItem> MailItems { get; set; }

        public class UserMailInfo
        {
            /// <summary>
            /// 聊天用户id
            /// </summary>
            [Tag(1)]
            public int TargetUserId { get; set; }
            /// <summary>
            /// 聊天用户昵称
            /// </summary>
            [Tag(2)]
            public string TargetNickName { get; set; }
            /// <summary>
            /// 聊天用户头像ID
            /// </summary>
            [Tag(3)]
            public int TargetUserHeadId { get; set; }
            /// <summary>
            /// 最后一次消息发送时间
            /// </summary>
            [Tag(4)]
            public DateTime SendTime { get; set; }
            /// <summary>
            /// 未读邮件条数
            /// </summary>
            [Tag(5)]
            public int UnReadNum { get; set; }
        }

        public class MailItem
        {
            /// <summary>
            /// 发送者id
            /// </summary>
            [Tag(1)]
            public int SenderId { get; set; }
            /// <summary>
            /// 发送时间
            /// </summary>
            [Tag(2)]
            public DateTime SendTime { get; set; }
            /// <summary>
            /// 消息内容
            /// </summary>
            [Tag(3)]
            public string Msg { get; set; }
        }
    }
    /// <summary>
    /// 获取用户邮件列表
    /// </summary>
    [GameCode(OpCode = 1310, ResponseType = typeof(GetUserMailResponse))]
    public class GetUserMailRequest : GameHandler
    {
        /// <summary>
        /// 传0就可以了
        /// </summary>
        public int Skip { get; set; }
        /// <summary>
        /// 消息的条数
        /// </summary>
        public int Take { get; set; }

        public override void Process(GameContext context)
        {
            var response = new GetUserMailResponse();
            var userMail = Storage.Load<UserMail>(CurrentUserId);
            var list = userMail.GetMailList(Skip, Take);
            if (list.Count > 0)
            {
                //会话列表
                var userRoleList = Storage.LoadList<UserRole>(list.Select(o => o.TargetUserId).ToArray());
                foreach (var userMailInfo in list)
                {
                    var item = new GetUserMailResponse.UserMailInfo();
                    item.SendTime = userMailInfo.SendTime;
                    item.TargetUserId = userMailInfo.TargetUserId;
                    item.UnReadNum = userMailInfo.UnReadNum;

                    var userRole = userRoleList.FirstOrDefault(o => o.Id == userMailInfo.TargetUserId);
                    if (userRole != null)
                    {
                        item.TargetNickName = userRole.NickName;
                        item.TargetUserHeadId = userRole.HeadId;
                    }

                    response.Items.Add(item);
                }

                //第一个会话的聊天列表
                var firstUserMailInfo = list[0];
                var listAll = firstUserMailInfo.GetAllMsg();
                listAll.Reverse();
                foreach (var mailItem in listAll)
                {
                    var sitem = new GetUserMailResponse.MailItem();
                    sitem.Msg = mailItem.Msg;
                    sitem.SendTime = mailItem.SendTime;
                    sitem.SenderId = mailItem.SenderId;

                    response.MailItems.Add(sitem);
                }
                //设置第一个会话已读
                userMail.ResetUnRead(firstUserMailInfo);
            }
            ResultObj = response;
        }
    }
    #endregion

    #region 获取用户会话聊天列表 1311
    public class GetUserMailSessionResponse
    {
        public GetUserMailSessionResponse()
        {
            MailItems = new List<GetUserMailResponse.MailItem>();
        }
        /// <summary>
        /// 会话的聊天记录
        /// </summary>
        [Tag(2)]
        public List<GetUserMailResponse.MailItem> MailItems { get; set; }
    }
    /// <summary>
    /// 获取用户会话聊天列表
    /// </summary>
    [GameCode(OpCode = 1311, ResponseType = typeof(GetUserMailSessionResponse))]
    public class GetUserMailSessionRequest : GameHandler
    {
        /// <summary>
        /// 聊天用户id
        /// </summary>
        public int TargetUserId { get; set; }

        public override void Process(GameContext context)
        {
            var response = new GetUserMailSessionResponse();

            var userMail = Storage.Load<UserMail>(CurrentUserId);
            var userInfo = userMail.GetMailInfo(TargetUserId);
            var listAll = userInfo.GetAllMsg();
            listAll.Reverse();
            foreach (var mailItem in listAll)
            {
                var sitem = new GetUserMailResponse.MailItem();
                sitem.Msg = mailItem.Msg;
                sitem.SendTime = mailItem.SendTime;
                sitem.SenderId = mailItem.SenderId;

                response.MailItems.Add(sitem);
            }
            //设置为已读
            userMail.ResetUnRead(userInfo);

            ResultObj = response;
        }
    }
    #endregion

    #region 发送用户聊天 1312
    /// <summary>
    /// 发送用户聊天 
    /// </summary>
    [GameCode(OpCode = 1312)]
    public class SendUserMailRequest : GameHandler
    {
        /// <summary>
        /// 标题【可选参数】
        /// </summary>
        //[ParamCheck(Ignore = true)]
        //public string Title { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 目标用户
        /// </summary>
        public int TargetUserId { get; set; }

        public override void Process(GameContext context)
        {
            var userMail = Storage.Load<UserMail>(CurrentUserId, true);
            userMail.SendTo(TargetUserId, "", Msg);
        }
    }
    #endregion

    #region 删除用户会话 1313
    /// <summary>
    /// 删除用户会话
    /// </summary>
    [GameCode(OpCode = 1313)]
    public class DelUserSessionRequest : GameHandler
    {
        /// <summary>
        /// 要删除的聊天用户id
        /// </summary>
        public int TargetUserId { get; set; }

        public override void Process(GameContext context)
        {
            var userMail = Storage.Load<UserMail>(CurrentUserId, true);
            userMail.Delete(TargetUserId);

            var logItem = new GameLogItem();
            logItem.F1 = TargetUserId;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 获取公告邮件 1320
    public class GetAnnouncementResponse
    {
        public GetAnnouncementResponse()
        {
            Items = new List<Announcement>();
        }
        /// <summary>
        /// 公告列表
        /// </summary>
        [Tag(1)]
        public List<Announcement> Items { get; set; }
    }
    /// <summary>
    /// 获取公告邮件
    /// </summary>
    [GameCode(OpCode = 1320, ResponseType = typeof(GetAnnouncementResponse))]
    public class GetAnnouncementRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            //GetAnnouncementResponse response = new GetAnnouncementResponse();

            //response.Items = Announcement.Announcements;
            //foreach (var announcement in response.Items)
            //{
            //    //统一保持富文本形式 易于公告带不同颜色扩展
            //    if (!announcement.MsgContent.Contains("<label>"))
            //        announcement.MsgContent = "<label>" + announcement.MsgContent + "</label>";
            //}

            //ResultObj = response;

        }
    }
    #endregion

    #region 加速任务 1400
    /// <summary>
    /// 加速任务
    /// </summary>
    [GameCode(OpCode = 1400)]
    public class AccTaskRequest : GameHandler
    {
        /// <summary>
        /// 任务Id
        /// </summary>
        public int TaskId { get; set; }

        public override void Process(GameContext context)
        {
            if (TaskId <= 0)
            {
                SetError(ResourceId.R_0000_IllegalParam);
                return;
            }
            var task = Schedule.GetTask(TaskId);
            if (task == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "TaskId", TaskId);
                return;
            }
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var needMoney = 0;
            var accTaskId = SpecialStoreId.AccTask;
            var accTime = task.ExecTime.ToTs();
            switch (task.Type)
            {
                default:
                    return;
            }

            var logItem = new GameLogItem();
            logItem.F1 = needMoney;
            logItem.F2 = userRole.TotalMoney;
            if (userRole.TotalMoney < needMoney)
            {//元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, accTaskId);
            //加速
            Schedule.AccTask(TaskId, accTime);

            logItem.S1 = task.Type;
            logItem.F3 = userRole.TotalMoney;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 加速倒计时 1401
    /// <summary>
    /// 加速倒计时
    /// </summary>
    [GameCode(OpCode = 1401)]
    public class AccEndTimeRequest : GameHandler
    {
        /// <summary>
        /// 加速倒计时类型
        /// </summary>
        public AccEndTimeType Type { get; set; }

        public override void Process(GameContext context)
        {
            var needMoney = 0;
            var specialStoreId = SpecialStoreId.AccTask;
            switch (Type)
            {   //竞技场清除冷却时间
                case AccEndTimeType.Pk:
                    needMoney = ConfigHelper.ClearCdCfgData.Pk;
                    specialStoreId = (int)AccEndTimeType.Pk;
                    break;
                case AccEndTimeType.BuildingUpgrade:
                    var userBuilding = Storage.Load<UserBuilding>(CurrentUserId, true);
                    var totalSecond = userBuilding.EndTime.ToTs();
                    if (totalSecond == 0) return;
                    needMoney = (int)(totalSecond * 1.0 / ConfigHelper.ClearCdCfgData.BuildingUpgrade);
                    if (needMoney == 0) needMoney = 1;
                    specialStoreId = (int)AccEndTimeType.BuildingUpgrade;
                    break;
                case AccEndTimeType.ConcubineJinFeng:
                    var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                    var totalSecond2 = userConcubine.EndTime.ToTs();
                    if (totalSecond2 == 0) return;
                    needMoney = (int)(totalSecond2 * 1.0 / ConfigHelper.ClearCdCfgData.ConcubineJinFeng);
                    if (needMoney == 0) needMoney = 1;
                    specialStoreId = (int)AccEndTimeType.ConcubineJinFeng;
                    break;
                default:
                    return;
            }

            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var logItem = new GameLogItem();
            logItem.F1 = needMoney;
            logItem.F2 = userRole.TotalMoney;
            if (userRole.TotalMoney < needMoney)
            {//元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, specialStoreId);

            //重置时间
            switch (Type)
            {
                case AccEndTimeType.Pk:
                    var userPk = Storage.Load<UserPk>(CurrentUserId, true);
                    userPk.NextChallengeTime = DateTime.Now;
                    break;
                case AccEndTimeType.BuildingUpgrade:
                    var userBuilding = Storage.Load<UserBuilding>(CurrentUserId, true);
                    userBuilding.EndTime = DateTime.Now;
                    break;
                case AccEndTimeType.ConcubineJinFeng:
                    var userConcubine = Storage.Load<UserConcubine>(CurrentUserId, true);
                    userConcubine.EndTime = DateTime.Now;
                    break;
                default: break;
            }

            logItem.F3 = userRole.TotalMoney;
            logItem.F4 = specialStoreId;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 购买体力 1500
    /// <summary>
    /// 购买体力
    /// </summary>
    [GameCode(OpCode = 1500)]
    public class BuySpRequest : GameHandler
    {
        public override void Process(GameContext context)
        {
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var maxBuySp = ConfigHelper.MaxBuySp;

            if (userRole.Sp >= maxBuySp)
            {
                //体力太多，不让购买了，以防玩家一直存储体力一次性爆发！
                SetError(ResourceId.R_0000_SpIsFull);
                return;
            }

            var vipLevel = userRole.RealVipLevel;
            var sysVipCfg = SysVipCfg.Find(o => o.VipLevel == vipLevel);
            if (sysVipCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysVipCfg", vipLevel);
                return;
            }
            var curBuyNum = (int)userRole.BuySpNum.Value;
            if (curBuyNum >= sysVipCfg.SpBuyNum)
            {
                //购买次数已用完
                SetError(ResourceId.R_0000_NoBuyNum);
                return;
            }
            var sysBuyNumCfg = SysBuyNumCfg.Find(curBuyNum + 1);
            if (sysBuyNumCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysBuyNumCfg", curBuyNum + 1);
                return;
            }
            var needMoney = sysBuyNumCfg.BuySpNumMoney;
            if (userRole.TotalMoney < needMoney)
            {
                //元宝不足
                SetError(ResourceId.R_0000_MoneyNotEnough);
                return;
            }
            //消费
            Utility.Concume(userRole, needMoney, SpecialStoreId.BuySpNum);

            //添加体力 及 购买体力次数
            Utility.AddResource(userRole, ItemType.Sp, Request.OpCode, ConfigHelper.BuySpAddValue);
            userRole.BuySpNum += 1;

            var logItem = new GameLogItem();
            logItem.F1 = 1;
            logItem.F2 = needMoney;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);
        }
    }
    #endregion

    #region 兑换码兑换礼包 1501

    public class RedCodeGetToolResponse
    {
        public RedCodeGetToolResponse()
        {
            Items = new List<ItemPair>();
        }
        /// <summary>
        /// 获得的资源列表[铜钱、木材、石头、铁矿、体力]
        /// </summary>
        [Tag(1)]
        public List<ItemPair> Items { get; set; }
    }
    /// <summary>
    /// 兑换码兑换礼包
    /// </summary>
    [GameCode(OpCode = 1501, ResponseType = typeof(RedCodeGetToolResponse))]
    public class RedCodeGetToolRequest : GameHandler
    {
        /// <summary>
        /// 兑换码
        /// </summary>
        public string Code { get; set; }
        public override bool InitParams(GameContext context)
        {
            if (string.IsNullOrEmpty(Code)) return false;
            return true;
        }
        public override void Process(GameContext context)
        {
            var userRole = Storage.Load<UserRole>(CurrentUserId, true);
            var sysRedCodeCfg = SysRedCodeCfg.Items.FirstOrDefault(o => o.Code == Code);
            if (sysRedCodeCfg == null)
            {
                SetError(ResourceId.R_1501_RedCodeNotExist);
                return;
            }

            var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.RedCode, true);
            if (serverData.StringData.Contains(Code))
            {
                SetError(ResourceId.R_1501_RedCodeIsGeted);
                return;
            }

            var type = (int)sysRedCodeCfg.Type;
            if (userRole.RedCodeTypeList.Contains(type))
            {
                SetError(ResourceId.R_1501_RedCodeTypeIsGeted);
                return;
            }

            var redCodeCfg = ConfigHelper.RedCodeCfgData.Items.FirstOrDefault(o => (int)o.Type == type);
            if (redCodeCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "ConfigHelper:RedCodeCfgData:Type", type);
                return;
            }
            var toolId = redCodeCfg.ToolId;
            var sysToolCfg = SysToolCfg.Find(o => o.Id == toolId);
            if (sysToolCfg == null)
            {
                SetError(ResourceId.R_0000_IdNotExist, "SysToolCfg:Id", toolId);
                return;
            }

            userRole.RedCodeTypeList.Add(type);
            serverData.StringData.Add(Code);

            var opCode = Request.OpCode;
            var response = new RedCodeGetToolResponse();
            response.Items = sysToolCfg.UseToUser(opCode, userRole, 1);

            var logItem = new GameLogItem();
            logItem.S1 = Code;
            GameLogManager.CommonLog(Request.OpCode, CurrentUserId, 0, logItem);

            ResultObj = response;
        }
    }
    #endregion
}