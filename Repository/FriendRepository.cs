using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi;

namespace MobileGame.Jianghu.Repository
{
    #region 获取好友列表 3000

    /// <summary>
    /// 用户基础信息
    /// </summary>
    public class BasicInfo
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [Tag(1)]
        public int UserId { get; set; }
        /// <summary>
        /// 头像Id
        /// </summary>
        [Tag(2)]
        public int HeadId { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Tag(3)]
        public string NickName { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        [Tag(4)]
        public int Level { get; set; }

    }

    /// <summary>
    /// 填充用户的基本信息
    /// </summary>
    public class FillBasicInfo
    {
        public static void setBasicInfo(BasicInfo info, UserRole Role)
        {
            info.HeadId = Role.HeadId;
            info.UserId = Role.Id;
            info.NickName = Role.NickName;
            info.Level = Role.Level;
        }
    }
    public class GetUserFriendsResponse
    {
        public GetUserFriendsResponse()
        {
            Items = new List<BasicInfo>();
        }
        /// <summary>
        /// 好友数量
        /// </summary>
        [Tag(1)]
        public int FriendCount { get; set; }
        /// <summary>
        /// 好友列表
        /// </summary>
        [Tag(2)]
        public List<BasicInfo> Items { get; set; }

    }
    /// <summary>
    /// 获取好友列表 3000
    /// </summary>
    [GameCode(OpCode = 3000, ResponseType = typeof(GetUserFriendsResponse), NeedTransaction = false)]
    public class GetUserFriendsRequest : GameHandler
    {
        /// <summary>
        /// 好友ID
        /// </summary>
        public int findid { get; set; }
        /// <summary>
        /// 好友昵称
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string findname { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public override void Process(GameContext context)
        {
            if (findname == null)
            {
                findname = "";
            }
            GetUserFriendsResponse response = new GetUserFriendsResponse();
            var userFriend = Storage.Load<UserFriend>(CurrentUserId, true);
            var ids = userFriend.Friends.Skip(Skip).Take(Take);
            var userlist = Storage.LoadList<UserRole>(ids.ToArray());

            foreach (var id in ids)
            {
                var info = new BasicInfo();
                if (findid > 0)
                {
                    if (findid == id)
                    {
                        response.Items.Add(info);
                        var userrole = userlist.Find(o => o.Id == id);
                        FillBasicInfo.setBasicInfo(info, userrole);

                        break;
                    }
                }
                else
                {
                    var userrole = userlist.Find(o => o.Id == id);
                    FillBasicInfo.setBasicInfo(info, userrole);
                    if (findname != "")
                    {
                        if (findname == info.NickName)
                        {
                            response.Items.Add(info);
                            break;
                        }
                    }
                    else
                        response.Items.Add(info);
                }
            }
            response.FriendCount = userFriend.Friends.Count;
            ResultObj = response;
        }
    }

    #endregion

    #region 删除好友 3001
    /// <summary>
    /// 删除好友 3001
    /// </summary>
    [GameCode(OpCode = 3001)]
    public class RemoveFriendRequest : GameHandler
    {
        /// <summary>
        /// 要被删除的用户Id
        /// </summary>
        public int ToBeRemove { get; set; }

        public override void Process(GameContext context)
        {
            var userFriend = Storage.Load<UserFriend>(CurrentUserId, true);
            userFriend.Friends.Remove(ToBeRemove);
            var targetFriend = Storage.Load<UserFriend>(ToBeRemove, true);
            targetFriend.Friends.Remove(CurrentUserId);
        }
    }

    #endregion

    #region 提交添加好友申请 3002
    /// <summary>
    /// 提交添加好友申请 3002
    /// </summary>
    [GameCode(OpCode = 3002)]
    public class SubmitRequest : GameHandler
    {
        /// <summary>
        /// 要添加的用户Id
        /// </summary>
        public int TargetUserId { get; set; }

        public override void Process(GameContext context)
        {
            var userFriend = Storage.Load<UserFriend>(TargetUserId, true);
            bool isbeg = userFriend.Begs.Exists(o => o == CurrentUserId);
            if (!isbeg)
            {
                userFriend.Begs.Add(CurrentUserId);

                //发邮件通知对方
                UserRole userRole = Storage.Load<UserRole>(CurrentUserId);
                //UserProfile targetUserProfile = Storage.Load<UserProfile>(TargetUserId);
                var msg = LangResource.GetLangResource(ResourceId.R_ApplyFriend, userRole.Id,
                                                                userRole.NickName);
                Utility.SendGmMailToTarget(userRole.Id, "", msg);
            }
        }
    }

    #endregion

    #region 对申请的操作(同意/拒绝) 3003
    /// <summary>
    /// 对申请的操作(同意/拒绝) 3003
    /// </summary> 
    [GameCode(OpCode = 3003)]
    public class IsAgreeFriendsSubmitRequest : GameHandler
    {
        /// <summary>
        /// 申请人ID
        /// </summary>
        public int SubmitUserId { get; set; }
        /// <summary>
        /// 同意为1，不同意为0
        /// </summary>
        public int isAgree { get; set; }

        public override void Process(GameContext context)
        {
            var userFriend = Storage.Load<UserFriend>(CurrentUserId, true);
            if (SubmitUserId == 0)
            {
                if (isAgree == 1)
                {
                    foreach (var id in userFriend.Begs)
                    {
                        var submitFriend = Storage.Load<UserFriend>(id, true);
                        userFriend.Friends.Remove(id);
                        userFriend.Friends.Add(id);
                        submitFriend.Friends.Add(CurrentUserId);
                        submitFriend.Friends.Remove(CurrentUserId);
                        submitFriend.Begs.Remove(CurrentUserId);
                        //发邮件通知对方
                        UserRole userRole = Storage.Load<UserRole>(CurrentUserId);
                        //UserProfile targetUserProfile = Storage.Load<UserProfile>(id);
                        var msg = LangResource.GetLangResource(ResourceId.R_AlreadyFriend, userRole.Id,
                                                                        userRole.NickName);
                        Utility.SendGmMailToTarget(userRole.Id, "", msg);
                    }
                }
                else
                {

                }
                userFriend.Begs.Clear();
            }
            else
            {
                userFriend.Begs.Remove(SubmitUserId);
                if (isAgree == 1)
                {
                    var submitFriend = Storage.Load<UserFriend>(SubmitUserId, true);
                    userFriend.Friends.Remove(SubmitUserId);
                    userFriend.Friends.Add(SubmitUserId);
                    submitFriend.Friends.Remove(CurrentUserId);
                    submitFriend.Friends.Add(CurrentUserId);
                    submitFriend.Begs.Remove(CurrentUserId);
                    //发邮件通知对方
                    UserRole userRole = Storage.Load<UserRole>(CurrentUserId);
                    //UserProfile targetUserProfile = Storage.Load<UserProfile>(SubmitUserId);
                    var msg = LangResource.GetLangResource(ResourceId.R_AlreadyFriend, userRole.Id,
                                                                    userRole.NickName);
                    Utility.SendGmMailToTarget(userRole.Id, "", msg);
                }
            }
        }

    }
    #endregion

    #region 搜索用户 3004
    public class SearchUserInfoResponse
    {
        public SearchUserInfoResponse()
        {
            Items = new List<FindUserInfo>();
        }
        /// <summary>
        /// 搜索数量
        /// </summary>
        [Tag(1)]
        public int SearchCount { get; set; }
        /// <summary>
        /// 搜索结果列表
        /// </summary>
        [Tag(2)]
        public List<FindUserInfo> Items { get; set; }

        public class FindUserInfo
        {
            public FindUserInfo()
            {
                userinfo = new BasicInfo();
            }
            /// <summary>
            /// 用户Id
            /// </summary>
            [Tag(1)]
            public BasicInfo userinfo { get; set; }
            /// <summary>
            /// 对应搜索结果列表，是否已经申请，申请过则为1，否则为0
            /// </summary>
            [Tag(2)]
            public int AleadyBeg { get; set; }
        }
    }
    /// <summary>
    /// 搜索用户 3004
    /// </summary>
    [GameCode(OpCode = 3004, ResponseType = typeof(SearchUserInfoResponse))]
    public class SearchUserInfoRequest : GameHandler
    {
        /// <summary>
        /// 搜索ID
        /// </summary>
        public int findid { get; set; }
        /// <summary>
        /// 搜索昵称
        /// </summary>
        [ParamCheck(Ignore = true)]
        public string findname { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }

        public override void Process(GameContext context)
        {
            if (findname == null)
            {
                findname = "";
            }
            SearchUserInfoResponse response = new SearchUserInfoResponse();
            var userFriend = Storage.Load<UserFriend>(CurrentUserId);
            var ids = RoleManager.SearchNickName(findname).Skip(Skip).Take(Take + 1);
            var userlist = Storage.LoadList<UserRole>(ids.ToArray());
            if (findid > 0)
            {
                var info = new SearchUserInfoResponse.FindUserInfo();
                var userrole = userlist.Find(o => o.Id == findid);
                FillBasicInfo.setBasicInfo(info.userinfo, userrole);
                response.Items.Add(info);
                response.SearchCount = 1;
                var uFriend = Storage.Load<UserFriend>(info.userinfo.UserId);
                bool isbeg = uFriend.Begs.Exists(o => o == CurrentUserId);
                bool isFri = uFriend.Friends.Exists(o => o == CurrentUserId);
                if (isbeg || isFri)
                {
                    info.AleadyBeg = 1;
                }
                else
                {
                    info.AleadyBeg = 0;
                }
            }
            else
            {
                int i = 0;
                foreach (var id in ids)
                {
                    ++i;
                    if (i > Take)
                    {
                        break;
                    }
                    if (id == CurrentUserId)
                    {
                        continue;
                    }
                    var info = new SearchUserInfoResponse.FindUserInfo();
                    response.Items.Add(info);
                    var userrole = userlist.Find(o => o.Id == id);
                    FillBasicInfo.setBasicInfo(info.userinfo, userrole);
                    var uFriend = Storage.Load<UserFriend>(info.userinfo.UserId);
                    bool isbeg = uFriend.Begs.Exists(o => o == CurrentUserId);
                    bool isFri = uFriend.Friends.Exists(o => o == CurrentUserId);
                    if (isbeg || isFri)
                    {
                        info.AleadyBeg = 1;
                    }
                    else
                    {
                        info.AleadyBeg = 0;
                    }
                }
                response.SearchCount = ids.Count();
                ResultObj = response;
            }
            ResultObj = response;
        }
    }
    #endregion

    #region 获取好友申请列表 3005
    public class GetFriendsSubmitResponse
    {
        public GetFriendsSubmitResponse()
        {
            Items = new List<BasicInfo>();
        }
        /// <summary>
        /// 申请好友数量
        /// </summary>
        [Tag(1)]
        public int FriendSubmitCount { get; set; }
        /// <summary>
        /// 申请好友列表
        /// </summary>
        [Tag(2)]
        public List<BasicInfo> Items { get; set; }

    }
    /// <summary>
    /// 获取好友申请列表 3005
    /// </summary>
    [GameCode(OpCode = 3005, ResponseType = typeof(GetFriendsSubmitResponse), NeedTransaction = false)]
    public class GetFriendsSubmitListRequest : GameHandler
    {
        public int Skip { get; set; }
        public int Take { get; set; }

        public override void Process(GameContext context)
        {
            GetFriendsSubmitResponse response = new GetFriendsSubmitResponse();
            var userFriend = Storage.Load<UserFriend>(CurrentUserId, true);
            var ids = userFriend.Begs.Skip(Skip).Take(Take);
            var userlist = Storage.LoadList<UserRole>(ids.ToArray());
            foreach (var id in ids)
            {
                var info = new BasicInfo();
                response.Items.Add(info);
                var userrole = userlist.Find(o => o.Id == id);
                FillBasicInfo.setBasicInfo(info, userrole);
            }
            response.FriendSubmitCount = userFriend.Friends.Count;
            ResultObj = response;
        }
    }
    #endregion
}
