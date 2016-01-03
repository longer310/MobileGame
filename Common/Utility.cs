using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using MobileGame.Core;
using MobileGame.Core.Dapper;
using MobileGame.Core.Logging;
using MobileGame.Core.ObjectMapper;
using MobileGame.Core.ObjectMapper.MappingConfiguration;
using MobileGame.tianzi.ConfigStruct;
using MobileGame.tianzi.Entity;
using MobileGame.tianzi.Repository;
using Newtonsoft.Json;
using ProtoBuf;


namespace MobileGame.tianzi.Common
{
    public class Utility
    {
        /// <summary>
        /// 记录文件log
        /// </summary>
        public static readonly ILog Logger = LogManager.GetLogger(typeof(GameApplication));

        #region 初始化NPC武将的属性
        public static UserHeroItem FillSysNpcHeroCfg(int heroId, int level, int starLevel, int armyLevel,
            int item1Id, int item1Level, int item1Star,
            int item2Id, int item2Level, int item2Star,
            int item3Id, int item3Level, int item3Star,
            int item4Id, int item4Level, int item4Star,
            int petId)
        {
            var userHeroItem = new UserHeroItem();
            userHeroItem.HeroId = heroId;
            userHeroItem.ClientLoadInit();
            userHeroItem.Level = level;
            userHeroItem.StarLevel = starLevel;
            userHeroItem.ArmyLevel = armyLevel;

            userHeroItem.RefreshHeroProperties();
            if (armyLevel > 0 && SysHeroCfg.Items.Any(o => o.Id == heroId && o.Recruit == 1))
                userHeroItem.RefreshArmyProperties();

            var itemIds = new List<int>() { item1Id, item2Id, item3Id, item4Id };
            var itemLevels = new List<int>() { item1Level, item2Level, item3Level, item4Level };
            var itemStars = new List<int>() { item1Star, item2Star, item3Star, item4Star };

            var index = 0;
            var eindex = (int)PropertiesType.Equip;
            foreach (var itemId in itemIds)
            {
                if (itemId > 0)
                {
                    var userEquipItem = new UserEquipItem();
                    userEquipItem.ItemId = itemIds[index];
                    userEquipItem.Level = itemLevels[index];
                    userEquipItem.Star = itemStars[index];

                    userEquipItem.RefreshProperties();

                    userHeroItem.Hps[eindex] += userEquipItem.Hp;
                    userHeroItem.Ads[eindex] += userEquipItem.Ad;
                    userHeroItem.Aps[eindex] += userEquipItem.Ap;
                    userHeroItem.AdArms[eindex] += userEquipItem.AdArm;
                    userHeroItem.ApArms[eindex] += userEquipItem.ApArm;
                }

                index++;
            }

            if (petId > 0)
            {
                var userPetItem = new UserPetItem();
                userPetItem.PetId = petId;
                userPetItem.RefreshProperties();

                var pindex = (int)PropertiesType.Pet;
                userHeroItem.Forces[pindex] += userPetItem.Force;
                userHeroItem.Intels[pindex] += userPetItem.Intel;
                userHeroItem.Commands[pindex] += userPetItem.Command;

                //骑宠影响 攻击速度 --越小越快
                userHeroItem.AttackSpeeds[pindex] -= userPetItem.AttackSpeed;
            }

            return userHeroItem;
        }
        #endregion

        #region 获得可以命名的昵称
        public static string SuffixNickName(string nickName)
        {
            int counter = 1;
            int rowcount = 0;
            string result = "";
            do
            {
                result = nickName + (counter == 1 ? "" : counter.ToString());
                var sql = string.Format("select count(1) from UserRole where NickName='{0}'", result);
                //rowcount = Convert.ToInt32(SqlHelper.ExecuteScalar(ParamHelper.GameServer, CommandType.Text, sql));
                rowcount = Convert.ToInt32(MySqlHelper.ExecuteScalar(ParamHelper.GameServer, CommandType.Text, sql));
                counter++;
            } while (rowcount != 0);
            return result;
        }
        #endregion

        #region 用户加经验
        /// <summary>
        /// 每个等级对应的用户Id集合
        /// </summary>
        public static readonly string LevelUserIdsKey = "LevelUserIds:{0}";

        public static readonly string UserRankKey = "UserRank";

        /// <summary>
        /// 获取物品的类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ExtractItemType GetIdExtractType(int id)
        {
            if (id > 9999999)
            {
                var a = Convert.ToInt32(id.ToString().Substring(0, 1));
                //6/7/8/9代表碎片 编号格式ABBBBBBB
                if (a >= 6)
                {
                    var aa = a * 10000000;
                    if (aa == (int)ToolType.PetChip) return ExtractItemType.PetChip;
                    if (aa == (int)ToolType.EquipChip) return ExtractItemType.EquipChip;
                    if (aa == (int)ToolType.HeroChip) return ExtractItemType.HeroChip;
                    if (aa == (int)ToolType.ConcubineChip) return ExtractItemType.ConcubineChip;
                }
                else
                {
                    return ExtractItemType.Tool;
                }
            }
            if (id > 9000000 && id < 9999999) return ExtractItemType.Hero;
            if (id > 5001000 && id < 5999999) return ExtractItemType.Concubine;
            if (id > 5000000 && id < 5001000) return ExtractItemType.Pet;
            return ExtractItemType.None;
        }

        /// <summary>
        /// 给用户加经验
        /// </summary>
        public static bool AddUserExp(UserRole userRole, int exp, int opCode)
        {
            if (exp <= 0) return false;
            userRole.Exp += exp;
            var maxLevel = SysUserUpgradeCfg.MaxLevel;
            if (userRole.Level > maxLevel) return false;

            var maxSysUserUpgradecfg = SysUserUpgradeCfg.Find(maxLevel);
            if (userRole.Level == maxLevel && userRole.Exp >= maxSysUserUpgradecfg.NextLvExp)
            {
                userRole.Exp = maxSysUserUpgradecfg.NextLvExp;
                return false;
            }

            var oldLevel = userRole.Level;
            SysUserUpgradeCfg cfg;
            do
            {
                cfg = SysUserUpgradeCfg.Find(userRole.Level);
                if (userRole.Exp >= cfg.NextLvExp)
                {
                    userRole.Exp -= cfg.NextLvExp;
                    userRole.Level++;
                    if (userRole.Level >= maxLevel) break;
                }
                else
                {
                    break;
                }
            } while (true);
            if (userRole.Level == maxLevel)
            {
                if (maxSysUserUpgradecfg != null && userRole.Exp > maxSysUserUpgradecfg.NextLvExp)
                    userRole.Exp = cfg.NextLvExp;
            }

            var hasLevelUp = false;

            if (oldLevel != userRole.Level)
            {//用户等级已经变了，从原来的等级列表删除，添加到新的等级列表中
                DataStorage.Current.Sets.Remove(string.Format(LevelUserIdsKey, oldLevel), userRole.Id.ToString(), true);
                DataStorage.Current.Sets.Add(string.Format(LevelUserIdsKey, userRole.Level), userRole.Id.ToString(), true);
                AddUserRank(userRole.Id);
                hasLevelUp = true;

                //体力上限改变
                var sysUserUpgradeCfg = SysUserUpgradeCfg.Find(userRole.Level);
                if (sysUserUpgradeCfg != null) userRole.SetMaxSp(sysUserUpgradeCfg.MaxSp);
            }

            //添加任务新达成数
            if (hasLevelUp)
            {
                AddMainLineTaskGoalData(userRole.Id, MainLineType.UserLevel, userRole.Level);
                var upgradeLevelAddSpModulus = ConfigHelper.UpgradeLevelAddSpModulus;
                //一次升多级的情况
                var addSp = 0;
                for (var i = oldLevel; i < userRole.Level; i++)
                {
                    if (i <= 10) addSp += 20;
                    else addSp += i * upgradeLevelAddSpModulus;
                }
                AddResource(userRole, ItemType.Sp, opCode, addSp);

                //等级变化 改变该值
                var userCity = DataStorage.Current.Load<UserCity>(userRole.Id);
                var userCityItems = userCity.CityItems.Where(o => o.OwnerType == OwnerType.Own).ToList();
                if (userCityItems.Count > 0)
                {
                    var serverMapCityItemIdList =
                        userCityItems.Where(o => o.ServerMapCityItemId > 0).Select(o => o.ServerMapCityItemId).ToList();

                    var serverMapCityItemList =
                        DataStorage.Current.LoadList<ServerMapCityItem>(serverMapCityItemIdList.ToArray(), true);
                    foreach (var serverMapCityItem in serverMapCityItemList)
                    {
                        serverMapCityItem.CityLevel = userRole.Level;
                    }
                }
            }

            return hasLevelUp;
        }
        /// <summary>
        /// 添加用户排行榜
        /// </summary>
        public static void AddUserRank(int userId)
        {
            var userRole = DataStorage.Current.Load<UserRole>(userId);
            double value = userRole.Level;
            DataStorage.Current.SortedSets.Add(UserRankKey, userId.ToString(), value, true);

        }
        #endregion

        #region 玩家等级操作
        /// <summary>
        /// 获取对应等级的所有用户Id列表
        /// 如果有传第二个参数 则先从缓存获取 保存一段时间
        /// </summary>
        public static List<int> GetLevelUserIdList(int level, int saveSecond = 0)
        {
            List<int> obj = null;
            if (saveSecond > 0)
            {
                var key = string.Format(LevelUserIdsKey, level);
                obj = (List<int>)GameCache.Get(key);
                if (obj == null)
                {
                    obj = DataStorage.Current.Sets.GetAll<string>(string.Format(LevelUserIdsKey, level)).Select(int.Parse).ToList();
                    GameCache.Put(key, obj, saveSecond);
                }
            }
            else
            {
                obj = DataStorage.Current.Sets.GetAll<string>(string.Format(LevelUserIdsKey, level)).Select(int.Parse).ToList();
            }
            return obj;
        }

        /// <summary>
        /// 获取高于等级的用户数量
        /// </summary>
        public static int GetUpLevelUserCount(int level)
        {
            int count = 0, curCount = 0;
            int j = 0;
            for (int i = level; i < 500; i++)
            {
                curCount = DataStorage.Current.Sets.GetAll<string>(string.Format(LevelUserIdsKey, i)).Count();
                if (curCount == 0) j++;
                else j = 0;
                count += curCount;
                if (j == 30) break;//连续30级没有找到一个用户 跳出
            }
            return count;
        }

        /// <summary>
        /// 添加到该等级的用户集合
        /// </summary>
        public static void AddToLevelUserIdList(int userId, int level)
        {
            DataStorage.Current.Sets.Add(string.Format(LevelUserIdsKey, level), userId.ToString(), true);
        }
        /// <summary>
        /// 获取全服玩家id
        /// </summary>
        /// <returns></returns>
        public static List<int> GetAllUserIdList()
        {
            return DataStorage.Current.Keys.Find("UserRole:*").Select(p => Convert.ToInt32(p.Substring(9))).ToList();
        }
        #endregion

        #region 获取碎片id对应的物品/武将/妃子id

        public static int AddItemToUser(int uid, ItemPair itemPair, int opcode)
        {
            var id = itemPair.ItemId;
            var type = GetItemType(id);
            if (type == BagItemType.Tool)
            {
                var sysToolCfg = SysToolCfg.Find(id);
                if (sysToolCfg != null)
                {
                    sysToolCfg.AddToUser(uid, opcode, itemPair.Num);
                }
                else return (int)type;
            }
            else if (type == BagItemType.Equip)
            {
                var sysEquipCfg = SysEquipCfg.Items.FirstOrDefault(o => o.Id == itemPair.ItemId);
                if (sysEquipCfg != null)
                {
                    var userEquip = DataStorage.Current.Load<UserEquip>(uid, true);
                    userEquip.AddEquipToUser(sysEquipCfg.Id, opcode);
                }
                else return (int)type;
            }
            else if (type == BagItemType.Pet)
            {
                var sysPetCfg = SysPetCfg.Items.FirstOrDefault(o => o.Id == itemPair.ItemId);
                if (sysPetCfg != null)
                {
                    var userPet = DataStorage.Current.Load<UserPet>(uid, true);
                    userPet.AddPetToUser(sysPetCfg.Id, opcode);
                }
                else return (int)type;
            }
            else if (type == BagItemType.Concubine)
            {
                var sysConcubineCfg = SysConcubineCfg.Items.FirstOrDefault(o => o.Id == itemPair.ItemId);
                if (sysConcubineCfg != null)
                {
                    var userConcubine = DataStorage.Current.Load<UserConcubine>(uid, true);
                    userConcubine.AddConcubineToUser(sysConcubineCfg.Id, opcode);
                }
                else return (int)type;
            }
            else if (type == BagItemType.Hero)
            {
                var sysHeroCfg = SysHeroCfg.Items.FirstOrDefault(o => o.Id == itemPair.ItemId);
                if (sysHeroCfg != null)
                {
                    var userHero = DataStorage.Current.Load<UserHero>(uid, true);
                    userHero.AddHeroToUser(sysHeroCfg.Id, opcode);
                }
                else return (int)type;
            }
            return 0;
        }

        /// <summary>
        /// 获得物品的类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BagItemType GetItemType(int id)
        {
            int i = (int)(id / 1000000);
            if (id > 10000000)
            {
                return BagItemType.Tool;
            }
            else if (i == 7)//7001001
                return BagItemType.Equip;
            else if (i == 5 && id < 5001000)//5000001
                return BagItemType.Pet;
            else if (i == 5 && id > 5001000)//5001001
                return BagItemType.Concubine;
            else if (i == 9)//9001401
                return BagItemType.Hero;
            return BagItemType.Tool;
        }

        /// <summary>
        /// 获取碎片id对应的物品/武将/妃子id
        /// </summary>
        /// <param name="chipId"></param>
        /// <returns></returns>
        public static int GetIdByChipId(int chipId)
        {
            if (chipId < 9999999) return chipId;
            //编号格式ABBBCCCC
            var a = Convert.ToInt32(chipId.ToString().Substring(0, 1));
            //7/8/9代表碎片 编号格式ABBBBBBB
            if (a >= 7)
            {
                var aa = a * 10000000;
                if (aa == (int)ToolType.EquipChip || aa == (int)ToolType.HeroChip || aa == (int)ToolType.ConcubineChip)
                    return chipId - aa;
            }
            return chipId;
        }
        /// <summary>
        /// 获取物品/武将/妃子id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetChipIdById(int id, ToolType type)
        {
            return id + (int)type;
        }
        #endregion

        #region 购买物品【元宝消费】
        /// <summary>
        /// 购买物品【元宝消费】
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="needMoney"></param>
        /// <param name="specialStoreId"></param>
        public static void Concume(UserRole userRole, int needMoney, int specialStoreId)
        {
            int subMoney, subGiveMoney;
            RoleManager.Consume(userRole, specialStoreId, needMoney, 1,
                out subMoney,
                out subGiveMoney);
        }
        #endregion

        #region 获得武将装备&骑宠的信息
        /// <summary>
        /// 获得武将装备&骑宠的信息
        /// </summary>
        /// <param name="userHeroItem"></param>
        /// <param name="userEquip"></param>
        /// <param name="userPet"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public static List<ResEquipItem> GetEquipItems(UserHeroItem userHeroItem, UserEquip userEquip, UserPet userPet, int opCode)
        {
            var equipList = new List<ResEquipItem>();
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
                var eitem = new ResEquipItem();
                eitem.Type = iequipType;
                if (iequipType == EquipType.Weapons || iequipType == EquipType.Clothes ||
                    iequipType == EquipType.Jewelry || iequipType == EquipType.Magic)
                {
                    var id = userHeroItem.EquipIdList[(int)iequipType - 1];
                    if (id > 0)
                    {
                        var equip = userEquip.Items.FirstOrDefault(o => o.Id == id);
                        if (equip != null)
                        {
                            //存在装备，获取装备信息
                            var type = equip.EquipType;
                            if (type == iequipType)
                            {
                                eitem.Id = id;
                                eitem.EquipId = equip.ItemId;
                                eitem.Level = equip.Level;
                                eitem.Star = equip.Star;
                                eitem.Exp = equip.Exp;
                            }
                            else
                            {
                                var logItem = new GameLogItem();
                                logItem.F1 = id;
                                logItem.F2 = (int)type;
                                logItem.F3 = (int)iequipType;
                                logItem.F4 = equip.ItemId;
                                logItem.S1 = "根据存储在英雄身上的装备id找不到的装备类型不一致";
                                GameLogManager.CommonLog(opCode, userEquip.Id, 0, logItem);
                            }
                        }
                        else
                        {
                            var logItem = new GameLogItem();
                            logItem.F1 = id;
                            logItem.S1 = "根据存储在英雄身上的装备id找不到装备";
                            GameLogManager.CommonLog(opCode, userEquip.Id, 0, logItem);
                        }
                    }

                    if (eitem.Id == 0)
                    {
                        //不存在装备，寻找是否有符合的未装备的装备
                        var unEquipedList =
                            userEquip.Items.Where(o => o.EquipType == iequipType && o.HeroId == 0).ToList();
                        if (unEquipedList.Count > 0)
                        {
                            //1:有可穿戴的装备但等级没有达到
                            eitem.EquipId++;
                            var canEquipList = unEquipedList.Where(o => o.NeedLevel <= userHeroItem.Level).ToList();

                            //有可穿戴的装备
                            if (canEquipList.Count > 0)
                                eitem.EquipId++;
                        }
                    }
                }
                else if (iequipType == EquipType.Pet)
                {
                    //骑宠数据
                    eitem.Id = userHeroItem.PetId;
                    eitem.EquipId = userHeroItem.SysPetId;
                    if (eitem.Id > 0)
                    {
                        var petItem = userPet.Items.FirstOrDefault(o => o.Id == eitem.Id);
                        if (petItem == null)
                            throw new ApplicationException(string.Format("userPet:List:Id:{0} NOT FIND", eitem.Id));

                        eitem.Level = petItem.Level;
                    }
                    else
                    {
                        var unEquipedList =
                               userPet.Items.Where(o => o.HeroId == 0).ToList();
                        //1:有可穿戴的骑宠但等级没有达到
                        if (unEquipedList.Count > 0)
                        {
                            eitem.EquipId++;

                            var canEquipList = unEquipedList.Where(o => o.NeedLevel <= userHeroItem.Level).ToList();

                            //有可穿戴的骑宠
                            if (canEquipList.Count > 0)
                                eitem.EquipId++;
                        }
                    }
                }

                equipList.Add(eitem);
            }
            return equipList;
        }
        #endregion

        #region 竞技场排行
        /// <summary>
        /// Pk存储集合的Key
        /// </summary>
        public static readonly string PkSetKey = "PkSetKey";
        /// <summary>
        /// 正在被Pk类ID
        /// </summary>
        public static readonly int ServerPkBeAttackingId = 1;
        /// <summary>
        /// 竞技场排名数据
        /// </summary>
        [ProtoContract]
        public class PkRankKey
        {
            /// <summary>
            /// Pk对手类型
            /// </summary>
            [ProtoMember(1)]
            public int Type { get; set; }
            /// <summary>
            /// 用户id
            /// </summary>
            [ProtoMember(2)]
            public int UserId { get; set; }
            /// <summary>
            /// 遭受攻击截止时间戳
            /// </summary>
            //[ProtoMember(3)]
            //public int AttackEndTimestamp { get; set; }

            public override string ToString()
            {
                return string.Format("{{{0},{1}}}", Type, UserId);
            }
        }
        /// <summary>
        /// 获取排名并设置排名【只能玩家请求】
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static int GetAndSetPkRank(int userId, int rank)
        {
            var maxRank = DataStorage.Current.SortedSets.GetLength(PkSetKey);
            //竞技场第一次有人进入或者改变了NPC个数时，初始化NPC
            var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.Pk, true);
            var npcNumIndex = (int)ServerDataPkIntType.PkNpcNum;
            var curNpcNum = serverData.IntData[npcNumIndex];

            bool refreshRank = false;
            var npcNum = ConfigHelper.PkCfgData.NpcNum;
            if (maxRank == 0 || curNpcNum != npcNum)
            {
                //先清空
                DataStorage.Current.SortedSets.RemoveRange(PkSetKey, 1.0, 1000000);

                //已初始化的个数
                var initNum = 0;
                //可能第一次获得到的人 立即存入redis
                var inum = (int)(ConfigHelper.PkCfgData.RangePrecsList[0][3] * 1.0 *
                           npcNum / 100);
                inum = npcNum - inum;
                while (npcNum > initNum)
                {
                    initNum++;

                    var item = new PkRankKey { Type = (int)BattleTargetType.Npc, UserId = initNum };//, AttackEndTimestamp = 0
                    //第四个参数join【true表示延迟执行，false表示立即执行】
                    var join = !(initNum >= inum);
                    DataStorage.Current.SortedSets.Add(PkSetKey, item, initNum, join);

                    DataStorage.Current.MarkDeleted<ServerPkNpc>(initNum);
                }
                maxRank = initNum;

                //赋值
                serverData.IntData[npcNumIndex] = npcNum;

                refreshRank = true;
            }

            KeyValuePair<PkRankKey, double>[] roleList = DataStorage.Current.SortedSets.Range<PkRankKey>(PkSetKey, rank, rank, true,
                true, true, 0, 1);
            if (!refreshRank && rank > 0 && rank <= maxRank &&
                roleList[0].Key.Type == (int)BattleTargetType.User && roleList[0].Key.UserId == userId)
                return rank;

            //有刷新排行 不存在排名 或者 现有排名不是当前玩家 或者 排名大于已记录的排名列表 都需要重新进行排名
            if (refreshRank || rank == 0 || roleList.Count() == 0 || roleList[0].Key.Type == (int)BattleTargetType.Npc ||
                (roleList[0].Key.Type == (int)BattleTargetType.User && roleList[0].Key.UserId != userId) ||
                rank > (int)maxRank)
            {
                rank = (int)maxRank + 1;
                var item = new PkRankKey() { Type = (int)BattleTargetType.User, UserId = userId };//, AttackEndTimestamp = 0 
                DataStorage.Current.SortedSets.Add(PkSetKey, item, rank, true);
            }
            return rank;
        }

        /// <summary>
        /// 判断对手是否排名发生改变
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="warType"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static bool JudgeRankChanged(int userId, WarType warType, int rank)
        {
            var result = false;
            var type = warType == WarType.PkNpc ? BattleTargetType.Npc : BattleTargetType.User;

            KeyValuePair<PkRankKey, double>[] roleList = DataStorage.Current.SortedSets.Range<PkRankKey>(PkSetKey, rank, rank, true,
                true, true, 0, 1);
            if (roleList.Count() == 0) result = true;//当前玩家排名已发生改变
            //else if (roleList[0].Key.Type == (int)type && roleList[0].Key.UserId == userId &&
            //    roleList[0].Key.AttackEndTimestamp > DateTime.Now.ToUnixTime()) result = true; //当前玩家正在遭受别的玩家进攻
            else if (roleList[0].Key.Type != (int)type || roleList[0].Key.UserId != userId) result = true;//当前玩家排名已发生改变

            //当前玩家正在遭受别的玩家进攻
            var serverPkBeAttacking = DataStorage.Current.Load<ServerPkBeAttacking>(ServerPkBeAttackingId, true);
            var serverPkBeAttackingItem =
                serverPkBeAttacking.Items.FirstOrDefault(o => o.Type == (int)type && o.UserId == userId);
            if (serverPkBeAttackingItem != null &&
                serverPkBeAttackingItem.AttackEndTimestamp > DateTime.Now.ToUnixTime())
                result = true;

            return result;
        }
        /// <summary>
        /// 设置玩家正遭受攻击
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="warType"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static int SetUserAttacking(int userId, WarType warType, int rank)
        {
            var result = 0;
            var type = warType == WarType.PkNpc ? BattleTargetType.Npc : BattleTargetType.User;

            KeyValuePair<PkRankKey, double>[] roleList = DataStorage.Current.SortedSets.Range<PkRankKey>(PkSetKey, rank, rank, true,
                true, true, 0, 1);
            if (roleList.Count() > 0 && roleList[0].Key.Type == (int)type && roleList[0].Key.UserId == userId)
            {
                //var role = roleList[0];
                //role.Key.AttackEndTimestamp = (int)DateTime.Now.AddSeconds(ConfigHelper.PkAttackLockTime).ToUnixTime();
                //DataStorage.Current.SortedSets.Add(PkSetKey, role.Key, rank, true);

                var serverPkBeAttacking = DataStorage.Current.Load<ServerPkBeAttacking>(ServerPkBeAttackingId, true);
                var serverPkBeAttackingItem =
                    serverPkBeAttacking.Items.FirstOrDefault(o => o.Type == (int)type && o.UserId == userId);
                if (serverPkBeAttackingItem != null)
                {
                    result = 1;
                    //serverPkBeAttackingItem.AttackEndTimestamp = (int)DateTime.Now.AddSeconds(ConfigHelper.PkAttackLockTime).ToUnixTime();
                }
                else
                {
                    serverPkBeAttacking.Items.Add(new PkBeAttackingItem()
                    {
                        Type = (int)type,
                        UserId = userId,
                        AttackEndTimestamp = (int)DateTime.Now.AddSeconds(ConfigHelper.PkAttackLockTime).ToUnixTime(),
                        Rank = rank,
                    });
                    result = 2;
                }
            }

            return result;
        }

        /// <summary>
        /// 设置排名
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rank"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static void SetPkRank(int userId, int rank, BattleTargetType targetType)
        {
            var item = new PkRankKey() { Type = (int)targetType, UserId = userId };//, AttackEndTimestamp = 0
            DataStorage.Current.SortedSets.Add(PkSetKey, item, rank, true);
        }

        #endregion

        #region 声望排行
        /// <summary>
        /// 声望存储集合的Key
        /// </summary>
        public static readonly string ReputeSetKey = "ReputeSetKey";

        /// <summary>
        /// 初始化全部玩家声望、魅力、战力排行
        /// </summary>
        public static void InitAllUserReputeCharmRank()
        {
            var allUserIdList = GetAllUserIdList();
            foreach (var i in allUserIdList)
            {
                UserRole userRole;
                UserFormation userFormation;
                UserHero userHero;
                DataStorage.Current.Load(out userRole, out userFormation, out userHero, i, true);

                SetReputeScore(userRole.Id, userRole.Repute);
                SetCharmScore(userRole.Id, userRole.Charm);
                var attMaxCombat = 0;
                foreach (var userHeroItem in userHero.Items)
                {
                    if (userFormation.StrongestFormation.Exists(o => o.HeroId == userHeroItem.HeroId))
                        attMaxCombat += userHeroItem.Combat;
                }
                SetCombatScore(userRole.Id, attMaxCombat);
                if (!userFormation.AttMaxCombat.Equals(attMaxCombat))
                    userFormation.AttMaxCombat = attMaxCombat;
            }
        }

        /// <summary>
        /// 设置声望排行
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="value"></param>
        public static void SetReputeScore(int userId, int value)
        {
            DataStorage.Current.SortedSets.Add(ReputeSetKey, userId, value, true);

            var length = DataStorage.Current.SortedSets.GetLength(ReputeSetKey);
            if (length > 1000)
            {
                //只记录前1000名玩家
                DataStorage.Current.SortedSets.RemoveRange(ReputeSetKey, 1001, length);
            }
        }
        #endregion

        #region 魅力排行
        /// <summary>
        /// 魅力存储集合的Key
        /// </summary>
        public static readonly string CharmSetKey = "CharmSetKey";
        /// <summary>
        /// 设置魅力排行
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="value"></param>
        public static void SetCharmScore(int userId, int value)
        {
            DataStorage.Current.SortedSets.Add(CharmSetKey, userId, value, true);

            var length = DataStorage.Current.SortedSets.GetLength(CharmSetKey);
            if (length > 1000)
            {
                //只记录前1000名玩家
                DataStorage.Current.SortedSets.RemoveRange(CharmSetKey, 1001, length);
            }
        }
        #endregion

        #region 总战力排行
        /// <summary>
        /// 总战力存储集合的Key
        /// </summary>
        public static readonly string CombatSetKey = "CombatSetKey";

        /// <summary>
        /// 设置最强战力排行
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="value"></param>
        public static void SetCombatScore(int userId, int value)
        {
            DataStorage.Current.SortedSets.Add(CombatSetKey, userId, value, true);

            var length = DataStorage.Current.SortedSets.GetLength(CombatSetKey);
            if (length > 1000)
            {
                //只记录前1000名玩家
                DataStorage.Current.SortedSets.RemoveRange(CombatSetKey, 1001, length);
            }
        }
        #endregion

        #region 判断资源是否足够
        /// <summary>
        /// 判断玩家是否正在遭受攻击
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool JudgeUserBeAttack(UserRole userRole, out string errorMsg)
        {
            errorMsg = "";
            if (userRole.BeAttackEndTime.ToTs() > 0)
            {
                if (userRole.BeAttackType == BeAttackType.Attacking)
                    errorMsg = ResourceId.R_0000_BeAttackingCanNotUseRes;
                else if (userRole.BeAttackType == BeAttackType.Result)
                    errorMsg = ResourceId.R_0000_WaitResultCanNotUseRes;
                else if (userRole.BeAttackType == BeAttackType.Rob)
                    errorMsg = ResourceId.R_0000_BeRobingCanNotUseRes;
                else
                    errorMsg = ResourceId.R_0000_BeAttackingCanNotUseRes;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断资源是否足够
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="coin"></param>
        /// <param name="wood"></param>
        /// <param name="stone"></param>
        /// <param name="iron"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static bool JudgeResourceEnough(UserRole userRole, int coin, int wood, int stone, int iron, out string errorMsg)
        {
            errorMsg = "";
            //消耗铜币
            if (userRole.Coin < coin)
            {
                //铜钱不足
                errorMsg = ResourceId.R_0000_CoinNotEnough;
                return false;
            }

            //消耗木材
            if (userRole.Wood < wood)
            {
                errorMsg = ResourceId.R_0000_WoodNotEnough;
                return false;
            }

            //消耗石头
            if (userRole.Stone < stone)
            {
                errorMsg = ResourceId.R_0000_StoneNotEnough;
                return false;
            }

            //消耗铁矿
            if (userRole.Iron < iron)
            {
                errorMsg = ResourceId.R_0000_IronNotEnough;
                return false;
            }
            return true;
        }
        #endregion

        #region 增加战斗技能光环效果
        public class HeroRingEffect
        {
            public int HeroId { get; set; }
            public SkillRingsType Type { get; set; }
            public int Effect { get; set; }
        }
        /// <summary>
        /// 添加战斗技能光环效果
        /// </summary>
        /// <param name="attHeroItems"></param>
        /// <param name="defHeroItems"></param>
        public static void AddBattleSkillRing(List<BattleHeroItem> attHeroItems, List<BattleHeroItem> defHeroItems = null)
        {
            var attAllEffects = new List<int>();
            for (int i = 0; i < (int)SkillRingsType.MaxIndex; i++) attAllEffects.Add(0);
            var defAllEffects = new List<int>();
            for (int i = 0; i < (int)SkillRingsType.MaxIndex; i++) defAllEffects.Add(0);

            var attSingleEffects = new List<HeroRingEffect>();
            var defSingleEffects = new List<HeroRingEffect>();

            #region 计算光环效果
            foreach (var battleHeroItem in attHeroItems)
            {
                var sysHeroCfg = SysHeroCfg.Find(battleHeroItem.HeroId);
                if (sysHeroCfg != null)
                {
                    int index = 0;
                    foreach (var skillId in sysHeroCfg.SkillIdList)
                    {
                        var skillLevel = battleHeroItem.SkillLevelList[index];
                        if (skillLevel > 0)
                        {
                            var sysSkillCfg = SysSkillCfg.Find(skillId);
                            if (sysSkillCfg != null)
                            {
                                var effect = sysSkillCfg.AttackValue + skillLevel * sysSkillCfg.AttackValueGrowup;
                                //光环效果
                                if (sysSkillCfg.AttackType == SkillAttackType.Ring)
                                {
                                    if (sysSkillCfg.Target == SkillTargetType.Own)
                                    {
                                        //自己
                                        var item = new HeroRingEffect()
                                        {
                                            HeroId = battleHeroItem.HeroId,
                                            Type = sysSkillCfg.RingsType
                                        };
                                        item.Effect = effect;
                                        attSingleEffects.Add(item);//一个一条记录，相同效果到时候求和
                                    }
                                    else if (sysSkillCfg.Target == SkillTargetType.FriendlyAll)
                                    {
                                        //己方全体
                                        attAllEffects[(int)sysSkillCfg.RingsType - 1] += effect;
                                    }
                                    else if (sysSkillCfg.Target == SkillTargetType.EnemyAll)
                                    {
                                        //敌方全体 减去
                                        defAllEffects[(int)sysSkillCfg.RingsType - 1] -= effect;
                                    }
                                }
                                else if (sysSkillCfg.AttackType == SkillAttackType.Armytype)
                                {
                                    var armyType = (ArmyType)sysSkillCfg.Target;
                                    foreach (var battleHeroItem2 in attHeroItems)
                                    {
                                        if (battleHeroItem2.ArmyType == armyType)
                                        {
                                            attSingleEffects.Add(new HeroRingEffect()
                                            {
                                                HeroId = battleHeroItem2.HeroId,
                                                Type = sysSkillCfg.RingsType,
                                                Effect = effect
                                            });
                                        }
                                    }
                                }
                                else if (sysSkillCfg.AttackType == SkillAttackType.Deftype)
                                {
                                    var deftype = (DefendType)sysSkillCfg.Target;
                                    foreach (var battleHeroItem2 in attHeroItems)
                                    {
                                        if (battleHeroItem2.DefendType == deftype)
                                        {
                                            attSingleEffects.Add(new HeroRingEffect()
                                            {
                                                HeroId = battleHeroItem2.HeroId,
                                                Type = sysSkillCfg.RingsType,
                                                Effect = effect
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        index++;
                    }
                }
            }

            defHeroItems = defHeroItems ?? new List<BattleHeroItem>();
            foreach (var battleHeroItem in defHeroItems)
            {
                var sysHeroCfg = SysHeroCfg.Find(battleHeroItem.HeroId);
                if (sysHeroCfg != null)
                {
                    int index = 0;
                    foreach (var skillId in sysHeroCfg.SkillIdList)
                    {
                        var skillLevel = battleHeroItem.SkillLevelList[index];
                        if (skillLevel > 0)
                        {
                            var sysSkillCfg = SysSkillCfg.Find(skillId);
                            if (sysSkillCfg != null)
                            {
                                var effect = sysSkillCfg.AttackValue + skillLevel * sysSkillCfg.AttackValueGrowup;
                                //光环效果
                                if (sysSkillCfg.AttackType == SkillAttackType.Ring)
                                {
                                    if (sysSkillCfg.Target == SkillTargetType.Own)
                                    {
                                        //自己
                                        var item = new HeroRingEffect()
                                        {
                                            HeroId = battleHeroItem.HeroId,
                                            Type = sysSkillCfg.RingsType
                                        };
                                        item.Effect = effect;
                                        defSingleEffects.Add(item);
                                    }
                                    else if (sysSkillCfg.Target == SkillTargetType.FriendlyAll)
                                    {
                                        //己方全体
                                        defAllEffects[(int)sysSkillCfg.RingsType - 1] += effect;
                                    }
                                    else if (sysSkillCfg.Target == SkillTargetType.EnemyAll)
                                    {
                                        //敌方全体 减去
                                        attAllEffects[(int)sysSkillCfg.RingsType - 1] -= effect;
                                    }
                                }
                                else if (sysSkillCfg.AttackType == SkillAttackType.Armytype)
                                {
                                    var armyType = (ArmyType)sysSkillCfg.Target;
                                    foreach (var battleHeroItem2 in defHeroItems)
                                    {
                                        if (battleHeroItem2.ArmyType == armyType)
                                        {
                                            defSingleEffects.Add(new HeroRingEffect()
                                            {
                                                HeroId = battleHeroItem2.HeroId,
                                                Type = sysSkillCfg.RingsType,
                                                Effect = effect
                                            });
                                        }
                                    }
                                }
                                else if (sysSkillCfg.AttackType == SkillAttackType.Deftype)
                                {
                                    var deftype = (DefendType)sysSkillCfg.Target;
                                    foreach (var battleHeroItem2 in defHeroItems)
                                    {
                                        if (battleHeroItem2.DefendType == deftype)
                                        {
                                            defSingleEffects.Add(new HeroRingEffect()
                                            {
                                                HeroId = battleHeroItem2.HeroId,
                                                Type = sysSkillCfg.RingsType,
                                                Effect = effect
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        index++;
                    }
                }
            }
            #endregion

            #region 给武将添加效果
            foreach (var battleHeroItem in attHeroItems)
            {
                int eindex = 0;
                var heroId = battleHeroItem.HeroId;
                foreach (var attAllEffect in attAllEffects)
                {
                    var ringType = (SkillRingsType)eindex + 1;
                    var singleEffect =
                        attSingleEffects.Where(o => o.HeroId == heroId && o.Type == ringType)
                            .Sum(o => o.Effect);
                    var totalEffect = attAllEffect + singleEffect;
                    switch (ringType)
                    {
                        case SkillRingsType.AdAura:
                            battleHeroItem.Ad += totalEffect;
                            if (battleHeroItem.Ad < 0) battleHeroItem.Ad = 0;
                            break;
                        case SkillRingsType.ApAura:
                            battleHeroItem.Ap += totalEffect;
                            if (battleHeroItem.Ap < 0) battleHeroItem.Ap = 0;
                            break;
                        case SkillRingsType.AdArmAura:
                            battleHeroItem.AdArm += totalEffect;
                            if (battleHeroItem.AdArm < 0) battleHeroItem.AdArm = 0;
                            break;
                        case SkillRingsType.ApArmAura:
                            battleHeroItem.ApArm += totalEffect;
                            if (battleHeroItem.ApArm < 0) battleHeroItem.ApArm = 0;
                            break;
                        case SkillRingsType.ForceAura:
                            battleHeroItem.Force += totalEffect;
                            if (battleHeroItem.Force < 0) battleHeroItem.Force = 0;
                            break;
                        case SkillRingsType.IntelAura:
                            battleHeroItem.Intel += totalEffect;
                            if (battleHeroItem.Intel < 0) battleHeroItem.Intel = 0;
                            break;
                        case SkillRingsType.CommandAura:
                            battleHeroItem.Command += totalEffect;
                            if (battleHeroItem.Command < 0) battleHeroItem.Command = 0;
                            break;
                        case SkillRingsType.EnergyAura:
                            battleHeroItem.InitEnergy += totalEffect;
                            if (battleHeroItem.InitEnergy < 0) battleHeroItem.InitEnergy = 0;
                            break;
                        case SkillRingsType.AdCrit:
                            battleHeroItem.AdCrit += totalEffect;
                            if (battleHeroItem.AdCrit < 0) battleHeroItem.AdCrit = 0;
                            break;
                        case SkillRingsType.ApCrit:
                            battleHeroItem.ApCrit += totalEffect;
                            if (battleHeroItem.ApCrit < 0) battleHeroItem.ApCrit = 0;
                            break;
                        case SkillRingsType.Blood:
                            battleHeroItem.Blood += totalEffect;
                            if (battleHeroItem.Blood < 0) battleHeroItem.Blood = 0;
                            break;
                        case SkillRingsType.Block:
                            battleHeroItem.Block += totalEffect;
                            if (battleHeroItem.Block < 0) battleHeroItem.Block = 0;
                            break;
                        case SkillRingsType.DownDef://护甲&法抗
                            battleHeroItem.AdArm += totalEffect;
                            battleHeroItem.ApArm += totalEffect;
                            if (battleHeroItem.AdArm < 0) battleHeroItem.AdArm = 0;
                            if (battleHeroItem.ApArm < 0) battleHeroItem.ApArm = 0;
                            break;
                        case SkillRingsType.UpHpRecover://生命回复
                            battleHeroItem.HpRecovery += totalEffect;
                            if (battleHeroItem.HpRecovery < 0) battleHeroItem.HpRecovery = 0;
                            break;
                        default: break;
                    }
                    eindex++;
                }
            }
            foreach (var battleHeroItem in defHeroItems)
            {
                int eindex = 0;
                var heroId = battleHeroItem.HeroId;
                foreach (var defAllEffect in defAllEffects)
                {
                    var ringType = (SkillRingsType)eindex + 1;
                    var singleEffect =
                        defSingleEffects.Where(o => o.HeroId == heroId && o.Type == ringType)
                            .Sum(o => o.Effect);
                    var totalEffect = defAllEffect + singleEffect;
                    switch (ringType)
                    {
                        case SkillRingsType.AdAura:
                            battleHeroItem.Ad += totalEffect;
                            if (battleHeroItem.Ad < 0) battleHeroItem.Ad = 0;
                            break;
                        case SkillRingsType.ApAura:
                            battleHeroItem.Ap += totalEffect;
                            if (battleHeroItem.Ap < 0) battleHeroItem.Ap = 0;
                            break;
                        case SkillRingsType.AdArmAura:
                            battleHeroItem.AdArm += totalEffect;
                            if (battleHeroItem.AdArm < 0) battleHeroItem.AdArm = 0;
                            break;
                        case SkillRingsType.ApArmAura:
                            battleHeroItem.ApArm += totalEffect;
                            if (battleHeroItem.ApArm < 0) battleHeroItem.ApArm = 0;
                            break;
                        case SkillRingsType.ForceAura:
                            battleHeroItem.Force += totalEffect;
                            if (battleHeroItem.Force < 0) battleHeroItem.Force = 0;
                            break;
                        case SkillRingsType.IntelAura:
                            battleHeroItem.Intel += totalEffect;
                            if (battleHeroItem.Intel < 0) battleHeroItem.Intel = 0;
                            break;
                        case SkillRingsType.CommandAura:
                            battleHeroItem.Command += totalEffect;
                            if (battleHeroItem.Command < 0) battleHeroItem.Command = 0;
                            break;
                        case SkillRingsType.EnergyAura:
                            battleHeroItem.InitEnergy += totalEffect;
                            if (battleHeroItem.InitEnergy < 0) battleHeroItem.InitEnergy = 0;
                            break;
                        case SkillRingsType.AdCrit:
                            battleHeroItem.AdCrit += totalEffect;
                            if (battleHeroItem.AdCrit < 0) battleHeroItem.AdCrit = 0;
                            break;
                        case SkillRingsType.ApCrit:
                            battleHeroItem.ApCrit += totalEffect;
                            if (battleHeroItem.ApCrit < 0) battleHeroItem.ApCrit = 0;
                            break;
                        case SkillRingsType.Blood:
                            battleHeroItem.Blood += totalEffect;
                            if (battleHeroItem.Blood < 0) battleHeroItem.Blood = 0;
                            break;
                        case SkillRingsType.Block:
                            battleHeroItem.Block += totalEffect;
                            if (battleHeroItem.Block < 0) battleHeroItem.Block = 0;
                            break;
                        case SkillRingsType.DownDef://护甲&法抗
                            battleHeroItem.AdArm += totalEffect;
                            battleHeroItem.ApArm += totalEffect;
                            if (battleHeroItem.AdArm < 0) battleHeroItem.AdArm = 0;
                            if (battleHeroItem.ApArm < 0) battleHeroItem.ApArm = 0;
                            break;
                        case SkillRingsType.UpHpRecover://生命回复
                            battleHeroItem.HpRecovery += totalEffect;
                            if (battleHeroItem.HpRecovery < 0) battleHeroItem.HpRecovery = 0;
                            break;
                        default: break;
                    }
                    eindex++;
                }
            }
            #endregion
        }
        #endregion

        #region 资源变动
        /// <summary>
        /// 增加资源
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="type"></param>
        /// <param name="opCode"></param>
        /// <param name="num"></param>
        /// <param name="userId"></param>
        public static int AddResource(UserRole userRole, ItemType type, int opCode, int num, int userId = 0)
        {
            if (num <= 0) return 0;
            if (userId > 0) userRole = DataStorage.Current.Load<UserRole>(userId, true);

            var titleLevel = userRole.TitleLevel;
            var realNum = userRole.AddResource(type, opCode, num);

            //添加任务新达成
            if (type == ItemType.Repute)
            {
                AddMainLineTaskGoalData(userRole.Id, MainLineType.ReputeReach, userRole.MaxRepute);

                BigMapCommon.CheckAndSetTitleLevel(userRole);
                //爵位有变化 则下发信息
                if (titleLevel != userRole.TitleLevel)
                {
                    GameContext.Current.Session.Notify();

                    var sysTitleCfg = SysTitleCfg.Find(o => o.Id == userRole.TitleLevel);
                    if (sysTitleCfg == null)
                        throw new ApplicationException(string.Format("SysTitleCfg Not Find Id={0}", userRole.TitleLevel));
                    var color = GetQualityColor(sysTitleCfg.Quality);
                    var msg = LangResource.GetLangResource(ResourceId.R_TitleChange, userRole.Id,
                                                         userRole.NickName, color, userRole.TitleName);
                    if (!string.IsNullOrEmpty(msg)) GameApplication.Instance.Broadcast(msg);
                }
                //加入排行
                SetReputeScore(userRole.Id, userRole.Repute);
            }
            else if (type == ItemType.Charm)
            {
                AddMainLineTaskGoalData(userRole.Id, MainLineType.CharmReach, userRole.Charm);
                //加入排行
                SetCharmScore(userRole.Id, userRole.Charm);
            }

            if (ItemType.Coin == type || ItemType.Wood == type || ItemType.Stone == type || ItemType.Iron == type)
            {
                //计算各个建筑存储量的变化
                ReassignBuildingResource(userRole, type, 1);
            }

            return realNum;
        }
        /// <summary>
        /// 消费资源
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="type"></param>
        /// <param name="opCode"></param>
        /// <param name="num"></param>
        /// <param name="userId"></param>
        /// <param name="reassign"></param>
        public static void ConsumeResource(UserRole userRole, ItemType type, int opCode, int num, int userId = 0, int reassign = 1)
        {
            if (num <= 0) return;
            if (userId > 0) userRole = DataStorage.Current.Load<UserRole>(userId, true);
            userRole.ConsumeResource(type, opCode, num);

            if (type == ItemType.Repute)
            {
                AddMainLineTaskGoalData(userRole.Id, MainLineType.ReputeReach, userRole.MaxRepute);
            }

            if (reassign == 1)
            {
                //计算各个建筑存储量的变化
                ReassignBuildingResource(userRole, type);
            }
            if (type == ItemType.Sp && reassign == 1)
            {
                //添加主公经验
                AddUserExp(userRole, num, opCode);
            }
        }
        /// <summary>
        /// 分配资源到建筑
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <param name="isAdd">是否是增加</param>
        public static void ReassignBuildingResource(UserRole userRole, ItemType type, int userId = 0, int isAdd = 0)
        {
            if (userId > 0) userRole = DataStorage.Current.Load<UserRole>(userId, true);
            var moneyType = MoneyType.None;
            var value = 0;
            var initValue = 0;
            var buildingCfgData = ConfigHelper.BuildingCfgData;
            switch (type)
            {
                case ItemType.Coin:
                    moneyType = MoneyType.Coin;
                    value = userRole.Coin;
                    initValue = buildingCfgData.InitCoinCapacity; break;
                case ItemType.Wood:
                    moneyType = MoneyType.Wood;
                    value = userRole.Wood;
                    initValue = buildingCfgData.InitWoodCapacity; break;
                case ItemType.Stone:
                    moneyType = MoneyType.Stone;
                    value = userRole.Stone;
                    initValue = buildingCfgData.InitStoneCapacity; break;
                case ItemType.Iron:
                    moneyType = MoneyType.Iron;
                    value = userRole.Iron;
                    initValue = buildingCfgData.InitIronCapacity; break;
                default: return;
            }

            //没有超出初始容量 无需分配到宫殿 这部分资源相当于保护起来了。
            if (isAdd == 1 && initValue >= value) return;
            //多出的部分进行建筑分配
            value -= initValue;

            if (moneyType != MoneyType.None)
            {
                var userBuilding = DataStorage.Current.Load<UserBuilding>(userRole.Id, true);
                userBuilding.ReassignResource(moneyType, value);
            }
        }

        /// <summary>
        /// 检查资源是否达到上限
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="moneyType"></param>
        /// <returns></returns>
        public static bool CheckResourceIsFull(UserRole userRole, MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Coin:
                    if (userRole.Coin < userRole.MaxCoin) return false;
                    break;
                case MoneyType.Stone:
                    if (userRole.Stone < userRole.MaxStone) return false;
                    break;
                case MoneyType.Wood:
                    if (userRole.Wood < userRole.MaxWood) return false;
                    break;
                case MoneyType.Iron:
                    if (userRole.Iron < userRole.MaxIron) return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// 获得资源数量
        /// </summary>
        /// <param name="userRole"></param>
        /// <param name="moneyType"></param>
        /// <returns></returns>
        public static int GetResourceValue(UserRole userRole, MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Coin:
                    return userRole.Coin;
                case MoneyType.Stone:
                    return userRole.Stone;
                case MoneyType.Wood:
                    return userRole.Wood;
                case MoneyType.Iron:
                    return userRole.Iron;
            }
            return userRole.Coin;
        }

        /// <summary>
        /// 获取资源名称【客户端显示的名称】
        /// </summary>
        /// <param name="moneyType"></param>
        /// <returns></returns>
        public static string GetMonenyTypeName(MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Coin:
                    return LangResource.GetLangResource(ResourceId.R_0000_CoinName);
                    break;
                case MoneyType.Stone:
                    return LangResource.GetLangResource(ResourceId.R_0000_StoneName);
                    break;
                case MoneyType.Wood:
                    return LangResource.GetLangResource(ResourceId.R_0000_WoodName);
                    break;
                case MoneyType.Iron:
                    return LangResource.GetLangResource(ResourceId.R_0000_IronName);
                    break;
            }
            return LangResource.GetLangResource(ResourceId.R_0000_CoinName);
        }

        /// <summary>
        /// 获取资源名称【客户端显示的名称】
        /// </summary>
        /// <param name="specialToolId"></param>
        /// <returns></returns>
        public static string GetSpecialToolIdName(SpecialToolId specialToolId)
        {
            switch (specialToolId)
            {
                case SpecialToolId.Coin:
                    return LangResource.GetLangResource(ResourceId.R_0000_CoinName);
                    break;
                case SpecialToolId.Stone:
                    return LangResource.GetLangResource(ResourceId.R_0000_StoneName);
                    break;
                case SpecialToolId.Wood:
                    return LangResource.GetLangResource(ResourceId.R_0000_WoodName);
                    break;
                case SpecialToolId.Iron:
                    return LangResource.GetLangResource(ResourceId.R_0000_IronName);
                    break;
            }
            return LangResource.GetLangResource(ResourceId.R_0000_CoinName);
        }
        #endregion

        #region 添加Log表
        /// <summary>
        /// 添加LOG表
        /// </summary>
        /// <param name="dateStr"></param>
        public static void AddLogTable(string dateStr)
        {
            //1、添加GameLog表
            var sql = @"CREATE TABLE `gamelog{Date}` (
                          `Id` bigint(20) NOT NULL AUTO_INCREMENT,
                          `LogType` int(11) NOT NULL DEFAULT '0',
                          `UserIdA` bigint(20) NOT NULL DEFAULT '0',
                          `UserIdB` bigint(20) NOT NULL DEFAULT '0',
                          `LogTime` datetime NOT NULL DEFAULT '2015-01-01',
                          `F1` int(11) NOT NULL DEFAULT '0',
                          `F2` int(11) NOT NULL DEFAULT '0',
                          `F3` int(11) NOT NULL DEFAULT '0',
                          `F4` int(11) NOT NULL DEFAULT '0',
                          `F5` int(11) NOT NULL DEFAULT '0',
                          `F6` int(11) NOT NULL DEFAULT '0',
                          `F7` int(11) NOT NULL DEFAULT '0',
                          `F8` int(11) NOT NULL DEFAULT '0',
                          `F9` int(11) NOT NULL DEFAULT '0',
                          `F10` int(11) NOT NULL DEFAULT '0',
                          `F11` int(11) NOT NULL DEFAULT '0',
                          `F12` int(11) NOT NULL DEFAULT '0',
                          `F13` int(11) NOT NULL DEFAULT '0',
                          `F14` int(11) NOT NULL DEFAULT '0',
                          `F15` int(11) NOT NULL DEFAULT '0',
                          `S1` longtext,
                          `S2` longtext,
                          `S3` longtext,
                          `S4` longtext,
                          `S5` longtext,
                          `S6` longtext,
                          `S7` longtext,
                          `S8` longtext,
                          `S9` longtext,
                          `S10` longtext,
                          `MoreLog` longtext,
                          PRIMARY KEY (`Id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

            //2、添加LoginLog表
            sql += @"CREATE TABLE `loginlog{Date}` (
                          `Id` int(11) NOT NULL AUTO_INCREMENT,
                          `UserId` int(11) NOT NULL,
                          `LoginTime` datetime NOT NULL DEFAULT '2015-01-01',
                          `Sid` varchar(50) DEFAULT NULL,
                          `V1` int(11) NOT NULL DEFAULT '0',
                          `V2` int(11) NOT NULL DEFAULT '0',
                          `IP` varchar(50) DEFAULT NULL,
                          `MobileType` int(11) NOT NULL DEFAULT '0',
                          `ChannelId` varchar(50) DEFAULT NULL,
                          `UniqueId` varchar(128) DEFAULT NULL,
                          `LoginStatus` int(11) DEFAULT NULL DEFAULT '0',
                          `Country` varchar(50) DEFAULT NULL,
                          `Local` varchar(50) DEFAULT NULL,
                          PRIMARY KEY (`Id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

            //3、添加MsgLog表
            sql += @"CREATE TABLE `msglog{Date}` (
                        `Id` int(11) NOT NULL AUTO_INCREMENT,
                        `SenderId` int(11) NOT NULL DEFAULT '0',
                        `ReceiverType` int(11) NOT NULL DEFAULT '0',
                        `ReceiverId` int(11) NOT NULL DEFAULT '0',
                        `Msg` text,
                        `Title` varchar(50) DEFAULT NULL,
                        `SendTime` datetime NOT NULL DEFAULT '2015-01-01',
                        PRIMARY KEY (`Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

            //4、添加ItemLog表
            sql += @"CREATE TABLE `itemlog{Date}` (
                      `Id` int(11) NOT NULL AUTO_INCREMENT,
                      `UserId` int(11) NOT NULL DEFAULT '0',
                      `Num` int(11) NOT NULL DEFAULT '0',
                      `StartNum` int(11) NOT NULL DEFAULT '0',
                      `EndNum` int(11) NOT NULL DEFAULT '0',
                      `OpCode` int(11) NOT NULL DEFAULT '0',
                      `ItemType` int(11) NOT NULL DEFAULT '0',
                      `LogTime` datetime NOT NULL DEFAULT '2015-01-01',
                      `ItemId` int(11) DEFAULT NULL DEFAULT '0',
                        PRIMARY KEY (`Id`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

            sql = sql.Replace("{Date}", dateStr);

            using (var conn = Util.ObtainConn(ParamHelper.GameLog))
            {
                conn.Execute(sql);
            }
        }
        #endregion

        #region 删除战报
        /// <summary>
        /// 删除战报
        /// </summary>
        /// <param name="battleId"></param>
        public static void DelBattle(int battleId)
        {
            var storage = DataStorage.Current;
            var battle = storage.Load<AllBattle>(battleId);
            if (battle.IsNew) return;
            var aHeroList = battle.BattleAttackerHeroItemIdList;
            var dHeroList = battle.BattleDefenderHeroItemIdList;
            aHeroList = aHeroList ?? new List<int>();
            dHeroList = dHeroList ?? new List<int>();
            aHeroList.AddRange(dHeroList);
            foreach (var i in aHeroList)
            {
                storage.MarkDeleted<BattleHeroItem>(i);
            }
            battle.RoundIdList = battle.RoundIdList ?? new List<int>();
            foreach (var i in battle.RoundIdList)
            {
                storage.MarkDeleted<BattleRound>(i);
            }
            storage.MarkDeleted<AllBattle>(battleId);
        }
        #endregion

        #region 大地图
        /// <summary>
        /// 获取全服地图城池id【同一个CityId的编为同一个Id储存，类似用户】
        /// </summary>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public static int GetServerMapCityId(int cityId)
        {
            var m = ConfigHelper.ClearBigMapNum * 1000;
            return (int)ServerDataIdType.BigMapCity * m + cityId;
        }
        /// <summary>
        /// 获取cityid
        /// </summary>
        /// <param name="serverMapCityId"></param>
        /// <returns></returns>
        public static int GetCityId(int serverMapCityId)
        {
            var m = ConfigHelper.ClearBigMapNum * 1000;
            return serverMapCityId - (int)ServerDataIdType.BigMapCity * m;
        }

        /// <summary>
        /// 获取领地玩家占领ID【相近等级的——暂时这个规则】
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="exceptUserIdList"></param>
        /// <returns></returns>
        public static int GetOwnerUserId(int userId, List<int> exceptUserIdList)
        {
            //TODO:规则需完善
            var userRole = DataStorage.Current.Load<UserRole>(userId);
            var level = userRole.Level;
            exceptUserIdList.Add(userId);

            //取和自己等级相同和前后十级的玩家
            var findLevelList = new List<int>();
            findLevelList.Add(level);
            for (var i = 1; i < 10; i++)
            {
                if (level + i <= 99) findLevelList.Add(level + i);
                if (level - i > 0) findLevelList.Add(level - i);
            }

            var totalHit = 0;
            foreach (var i in findLevelList)
            {
                var userIdList = Utility.GetLevelUserIdList(i);
                userIdList = userIdList.Except(exceptUserIdList).ToList();
                while (userIdList.Count > 0)
                {
                    totalHit++;

                    var r = Util.GetRandom(0, userIdList.Count);
                    var targetUserId = userIdList[r];
                    var targetUserRole = DataStorage.Current.Load<UserRole>(targetUserId);
                    if (targetUserRole.ProtectEndTime.AddHours(-1) > DateTime.Now)
                        userIdList.Add(targetUserId);
                    else return targetUserId;

                    //最多找15个玩家，还没找到则直接不找了，给npc。
                    if (totalHit >= 15) return 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// 获取领地玩家占领ID【相近等级的——暂时这个规则】
        /// </summary>
        /// <param name="cityLevel"></param>
        /// <returns></returns>
        public static int GetOwnerNpcId(int cityLevel)//, List<int> exceptNpcIdList
        {
            //TODO:规则需完善
            //var userRole = DataStorage.Current.Load<UserRole>(userId);
            var level = cityLevel;

            //取和自己等级相同和前后十级的玩家
            //var findLevelList = new List<int>();
            //findLevelList.Add(level);
            //for (var i = 1; i < 10; i++)
            //{
            //    if (level + i <= 99) findLevelList.Add(level + i);
            //    if (level - i > 0) findLevelList.Add(level - i);
            //}

            //foreach (var i in findLevelList)
            //{
            //    var npcIdList =
            //        SysNpcFormationCfg.Items.Where(o => o.NpcId > 2000 && o.NpcId < 2999 && o.Level == i)
            //            .Select(o => o.NpcId)
            //            .Distinct()
            //            .ToList();
            //    npcIdList = npcIdList.Except(exceptNpcIdList).ToList();
            //    if (npcIdList.Count > 0)
            //    {
            //        var r = Util.GetRandom(0, npcIdList.Count);
            //        return npcIdList[r];
            //    }
            //}

            var range = 5;
            var startLevel = level - range > 0 ? level - range : 1;
            var endLevel = level + range > 99 ? 99 : level + range;
            level = Util.GetRandom(startLevel, endLevel + 1);

            var npcIdList =
                SysBigMapHeroCfg.Items.Where(o => o.NpcId > 2000 && o.NpcId < 2999 && o.Level == level)
                    .Select(o => o.NpcId)
                    .Distinct()
                    .ToList();
            if (npcIdList.Count > 0)
            {
                var r = Util.GetRandom(0, npcIdList.Count);
                return npcIdList[r];
            }

            return 0;
        }
        #endregion

        #region 添加战斗回合
        /// <summary>
        /// 添加战斗回合
        /// </summary>
        /// <param name="roundsStr">客户端上传的回合字符串数据</param>
        /// <returns>回合id列表</returns>
        public static List<ListIntItem> AddBattleRound(string roundsStr)
        {
            var idListList = new List<ListIntItem>();
            var idList = new ListIntItem();
            if (string.IsNullOrEmpty(roundsStr)) return idListList;

            var roundList = roundsStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var lastRound = 0;
            foreach (var roundStr in roundList)
            {
                int battleRoundId = Util.GetSequence(typeof(BattleRound), 0);
                var item = DataStorage.Current.Load<BattleRound>(battleRoundId, true);

                var index = 0;
                var sindex = 0;
                var eindex = roundStr.IndexOf(',');//分隔符
                var aindex = roundStr.IndexOf('[');//数组或者类的开头
                var aeindex = roundStr.IndexOf(']', sindex);//数组或者类的结尾
                while (eindex != -1)
                {
                    var str = roundStr.Substring(sindex, eindex - sindex);
                    if (index == 0) item.Round = Convert.ToInt32(str);              //回合数
                    else if (index == 1) item.AttHeroId = str;                      //攻击方id
                    else if (index == 2) item.AttackType = Convert.ToInt32(str);    //攻击类型【普通攻击、大招、技能】
                    else if (index == 3) item.DrinkBlood = Convert.ToInt32(str);    //吸血
                    else if (index == 4)                                            //攻击路径
                    {
                        //没有经过路径
                        if (aindex < 0 || aindex > eindex) item.PathList = new List<int>();
                        else
                        {
                            eindex = aeindex + 1;//]符号后一位为该字符的最后一位
                            str = roundStr.Substring(sindex, eindex - sindex);
                            item.PathList = JsonConvert.DeserializeObject<List<int>>(str);
                        }
                    }
                    else if (index == 5 || index == 6)                              //影响的效果
                    {
                        eindex = aeindex + 1;
                        str = roundStr.Substring(sindex + 1, eindex - sindex - 2);
                        var effectStrList = str.Split(new char[] { ',' }).ToList();
                        while (effectStrList.Count < 4) effectStrList.Add("0");

                        if (index == 5)
                        {
                            item.TarEffectItems = item.TarEffectItems ?? new List<HeroEffectItem>();
                            //攻击方攻击产生的影响列表
                            item.TarEffectItems.Add(new HeroEffectItem()
                            {
                                Type = Convert.ToInt32(effectStrList[0]),
                                HeroId = effectStrList[1],
                                Blood = Convert.ToInt32(effectStrList[2]),
                                BackBlood = Convert.ToInt32(effectStrList[3]),
                            });
                        }
                        else
                        {
                            item.AEffectItems[item.AEffectItems.Count - 1].FjEffectItems =
                                item.AEffectItems[item.AEffectItems.Count - 1].FjEffectItems ??
                                new List<HeroEffectItem>();
                            //反击影响的列表
                            item.AEffectItems[item.AEffectItems.Count - 1].FjEffectItems.Add(new HeroEffectItem()
                            {
                                Type = Convert.ToInt32(effectStrList[0]),
                                HeroId = effectStrList[1],
                                Blood = Convert.ToInt32(effectStrList[2]),
                                BackBlood = Convert.ToInt32(effectStrList[3]),
                            });
                        }
                    }
                    else
                    {
                        break;
                    }

                    if (roundStr.Count() <= eindex) break;
                    sindex = eindex + 1;
                    eindex = roundStr.IndexOf(',', sindex);
                    aindex = roundStr.IndexOf('[', sindex);
                    aeindex = roundStr.IndexOf(']', sindex);

                    if (index < 5)
                    {
                        index++;
                    }
                    else if (roundStr.Count() > sindex)
                    {
                        var firstChar = roundStr.Substring(sindex, 1);
                        if (firstChar != "[")
                        {
                            //反击！！
                            index = 6;

                            var fjHeroId = roundStr.Substring(sindex, eindex - sindex);
                            item.AEffectItems.Add(new HeroAEffectItem() { FjHeroId = fjHeroId });

                            sindex = eindex + 1;
                            eindex = roundStr.IndexOf(',', sindex);
                            aindex = roundStr.IndexOf('[', sindex);
                            aeindex = roundStr.IndexOf(']', sindex);
                        }
                    }
                }
                if (item.Round > lastRound)
                    idList.IdItems.Add(battleRoundId);
                else
                {
                    idListList.Add(idList);
                    idList = new ListIntItem();
                    idList.IdItems.Add(battleRoundId);
                }
                lastRound = item.Round;
            }
            idListList.Add(idList);
            return idListList;
        }
        #endregion

        #region 设置/添加任务达成数
        /// <summary>
        /// 设置主线任务达成数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="curValue"></param>
        public static void AddMainLineTaskGoalData(int userId, MainLineType type, int curValue)
        {
            var userTask = DataStorage.Current.Load<UserTask>(userId, true);

            var id = userTask.GetTaskId(TaskSort.MainLine, (int)type);
            var sysTaskCfg = SysTaskCfg.Find(id);
            if (MainLineType.OccupiedCity == type)
            {
                if (!userTask.OccupiedCityIdList.Contains(curValue))
                {
                    userTask.OccupiedCityIdList.Add(curValue);
                    if (sysTaskCfg != null && sysTaskCfg.GoalValue == curValue)
                    {
                        //占领的城池即为任务目标城池
                        userTask.ChangeMainLineNewMsg();
                    }
                }

            }
            else
            {
                var index = (int)type - 1;
                var oldValue = userTask.MainLineValueList[index];
                if (curValue > oldValue && oldValue != -1)
                {
                    //有增长
                    userTask.MainLineValueList[index] = curValue;

                    if (sysTaskCfg != null && curValue >= sysTaskCfg.GoalValue)
                    {
                        userTask.ChangeMainLineNewMsg();
                    }
                }
            }
        }
        /// <summary>
        /// 添加每日任务达成数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="addValue">增加的数量</param>
        public static void AddDailyTaskGoalData(int userId, DailyType type, int addValue = 1)
        {
            var userTask = DataStorage.Current.Load<UserTask>(userId, true);

            while (userTask.DailyValueList.Count < (int)DailyType.Max)
            {
                userTask.DailyValueList.Add(0);
            }
            var index = (int)type - 1;
            if (userTask.DailyValueList[index] != -1)
            {
                userTask.DailyValueList[index] += addValue;

                var id = userTask.GetTaskId(TaskSort.Daily, (int)type);
                var sysTaskCfg = SysTaskCfg.Find(id);
                if (sysTaskCfg != null && userTask.DailyValueList[index] >= sysTaskCfg.GoalValue)
                {
                    userTask.ChangeDailyNewMsg();
                }
            }
        }
        #endregion

        #region 大地图寻访
        /// <summary>
        /// 大地图全局ID
        /// </summary>
        public static int ServerMapCityActivityKey = 10000;
        /// <summary>
        /// 获取大地图活动数据
        /// </summary>
        /// <returns></returns>
        public static ServerMapCityActivity GetServerMapCityActivity(int update = 0)
        {
            var serverMapCityActivity = DataStorage.Current.Load<ServerMapCityActivity>(ServerMapCityActivityKey, true);
            var count = 0;
            for (var i = serverMapCityActivity.Items.Count() - 1; i >= 0; i--)
            {
                var visitorItems = serverMapCityActivity.Items.ToList()[i].ActivityVisitorItems;
                for (var j = visitorItems.Count - 1; j >= 0; j--)
                {
                    var item = visitorItems[j];
                    if (item.LeaveTimes <= DateTime.Now)
                    {
                        visitorItems.Remove(item);
                    }
                }
                count += visitorItems.Count;
            }
            if (update == 0 && count == 0)
            {
                RefreshMapCityActivity();
            }

            return serverMapCityActivity;
        }
        /// <summary>
        /// 获取大地图活动数据
        /// </summary>
        /// <returns></returns>
        public static List<ActivityVisitorTtem> GetServerMapCityActivityByCityId(int cityId)
        {
            var serverMapCityActivity = DataStorage.Current.Load<ServerMapCityActivity>(ServerMapCityActivityKey, true);

            var serverMapCityActivityItem = serverMapCityActivity.Items.FirstOrDefault(o => o.CityId == cityId);
            if (serverMapCityActivityItem == null) return new List<ActivityVisitorTtem>();
            return serverMapCityActivityItem.ActivityVisitorItems;
        }

        /// <summary>
        /// 刷新大地图活动
        /// </summary>
        public static void RefreshMapCityActivity()
        {
            var activitys = SysBigMapActivity.Activitys.Where(o => o.Turn == (int)DateTime.Today.DayOfWeek);
            var serverMapCityActivity = GetServerMapCityActivity(1);

            //过滤已经过期的活动 （5分钟前过滤也可以）
            for (var i = serverMapCityActivity.Items.Count() - 1; i >= 0; i--)
            {
                var visitorItems = serverMapCityActivity.Items.ToList()[i].ActivityVisitorItems;
                for (var j = visitorItems.Count - 1; j >= 0; j--)
                {
                    var item = visitorItems[j];
                    if (item.LeaveTimes < DateTime.Now || item.LeaveTimes < DateTime.Now.AddSeconds(-300))
                    {
                        visitorItems.Remove(item);
                    }
                }
            }

            //遍历所有的活动 然后添加
            foreach (var sysBigMapActivity in activitys)
            {
                bool needAdd = false;
                var serverMapCityActivityItem = serverMapCityActivity.Items.FirstOrDefault(o => o.CityId == sysBigMapActivity.CityId);
                if (serverMapCityActivityItem != null)
                {
                    var visitorItems = serverMapCityActivityItem.ActivityVisitorItems;
                    if (!visitorItems.Exists(o => o.VisitorId == sysBigMapActivity.VisitorId))
                    {
                        //添加活动
                        needAdd = true;
                    }
                }
                else
                {
                    serverMapCityActivityItem = KVEntity.CreateNew<ServerMapCityActivityItem>();
                    serverMapCityActivityItem.CityId = sysBigMapActivity.CityId;

                    serverMapCityActivity.Items.Add(serverMapCityActivityItem);

                    needAdd = true;
                }
                if (needAdd)
                {
                    var leaveTime = DateTime.Now.AddHours(sysBigMapActivity.StayTime);
                    serverMapCityActivityItem.ActivityVisitorItems.Add(new ActivityVisitorTtem()
                    {
                        VisitorId = sysBigMapActivity.VisitorId,
                        VisitTimes = sysBigMapActivity.MaxVisitTimes,
                        LeaveTimes = leaveTime,
                        VisitedIdList = new List<int>(),
                    });
                }
            }
        }

        /// <summary>
        /// 获取城池里的到访列表（自身+全局活动）
        /// </summary>
        /// <param name="cityItem"></param>
        /// <param name="cityId"></param>
        /// <returns></returns>
        public static List<VisitorItem> GetVisitorItems(UserCityItem cityItem, int cityId)
        {
            var list = new List<VisitorItem>();
            //全局城池活动信息
            var serverMapCityActivity = Utility.GetServerMapCityActivity();

            //全局城池内到访的活动
            var serverMapCityActivityItem =
                serverMapCityActivity.Items.FirstOrDefault(o => o.CityId == cityId);
            if (serverMapCityActivityItem != null && serverMapCityActivityItem.ActivityVisitorItems.Any())
            {
                foreach (var visitorItem in serverMapCityActivityItem.ActivityVisitorItems)
                {
                    visitorItem.VisitedIdList = visitorItem.VisitedIdList ?? new List<int>();
                    list.Add(new VisitorItem()
                    {
                        LeaveTimes = visitorItem.LeaveTimes,
                        VisitorId = visitorItem.VisitorId,
                        VisitTimes = visitorItem.VisitTimes,
                        CanVisitor = visitorItem.VisitedIdList.Contains(cityItem.Pid) ? 0 : 1,
                    });
                }
                //list.AddRange(serverMapCityActivityItem.ActivityVisitorItems.ToArray());
            }
            //添加自身的
            //foreach (var visitorItem in cityItem.VisitorItems)
            //{
            //    list.Add(new VisitorItem()
            //    {
            //        LeaveTimes = visitorItem.LeaveTimes,
            //        VisitorId = visitorItem.VisitorId,
            //        VisitTimes = visitorItem.VisitTimes,
            //        CanVisitor = visitorItem.VisitTimes > 0 ? 1 : 0,
            //    });
            //}
            //list.AddRange(cityItem.VisitorItems.ToArray());

            return list;
        }

        /// <summary>
        /// 获得品质的颜色
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static string GetQualityColor(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Green:
                    return "86,214,0";
                case ItemQuality.Blue:
                    return "30,210,255";
                case ItemQuality.Purple:
                    return "239,0,255";
                case ItemQuality.Orange:
                    return "255,168,0";
            }
            return "255,255,255";
        }

        /// <summary>
        /// 获得到访的字符串 按照品质标记为相应颜色
        /// </summary>
        /// <param name="visitorItems"></param>
        /// <returns></returns>
        public static string GetVisitorMsg(List<VisitorItem> visitorItems)
        {
            var str = new StringBuilder(string.Empty);
            bool isFirst = true;
            foreach (var visitorItem in visitorItems)
            {
                ItemQuality quality = ItemQuality.White;
                string name = "武将或者妃子";

                var sysHeroCfg = SysHeroCfg.Items.Find(o => o.Id == visitorItem.VisitorId);
                if (sysHeroCfg == null)
                {
                    var sysConcubineCfg = SysConcubineCfg.Items.Find(o => o.Id == visitorItem.VisitorId);
                    if (sysConcubineCfg != null)
                    {
                        quality = sysConcubineCfg.Quality;
                        name = sysConcubineCfg.Name;
                    }
                }
                else
                {
                    quality = sysHeroCfg.Quality;
                    name = sysHeroCfg.Name;
                }
                if (!isFirst)
                    str.AppendFormat("<label>,</label>");
                if (quality != ItemQuality.White)
                    str.AppendFormat("<label color='{0}'>{1}</label>", GetQualityColor(quality), name);
                else
                {
                    str.AppendFormat("<label>{0}</label>", name);
                }
                isFirst = false;
            }

            return str.ToString();
        }


        /// <summary>
        /// 检查大地图 是否清空重来
        /// </summary>
        /// <returns></returns>
        public static void CheckAndSetBigMapNum()
        {
            //大地图第一次有人进入或者刷新大地图数据数大于存储的值时，初始化数据
            var serverData = DataStorage.Current.Load<ServerData>((int)ServerDataIdType.BigMapCity, true);
            var clearBigMapNumIndex = (int)ServerDataBigMapIntType.ClearBigMapNum;
            var curClearBigMapNum = serverData.IntData[clearBigMapNumIndex];

            var clearBigMapNum = ConfigHelper.ClearBigMapNum;
            if (curClearBigMapNum != clearBigMapNum)
            {
                //先清空
                var serverMapCityIdList =
                    DataStorage.Current.Keys.Find("ServerMapCity:*")
                        .Select(p => Convert.ToInt32(p.Substring(14)))
                        .ToList();
                var serverMapCityList = DataStorage.Current.LoadList<ServerMapCity>(serverMapCityIdList.ToArray(), true);
                foreach (var i in serverMapCityIdList)
                {
                    var serverMapCity = serverMapCityList.FirstOrDefault(o => o.Id == i);
                    if (serverMapCity != null)
                    {
                        var serverMapCityItemList =
                            DataStorage.Current.LoadList<ServerMapCityItem>(
                                serverMapCity.ItemIdList.ToArray(), true);
                        foreach (var serverMapCityItem in serverMapCityItemList)
                        {
                            DataStorage.Current.MarkDeleted(serverMapCityItem);
                        }
                    }

                    DataStorage.Current.MarkDeleted(serverMapCity);
                }

                //赋值
                serverData.IntData[clearBigMapNumIndex] = clearBigMapNum;
            }
        }
        #endregion

        #region VIP变化处理各个次数
        /// <summary>
        /// VIP变化处理各个次数
        /// </summary>
        /// <param name="oldVipLevel"></param>
        /// <param name="newVipLevel"></param>
        /// <param name="userId"></param>
        /// <param name="chargeNum"></param>
        public static int VipChangeNum(int oldVipLevel, int newVipLevel, int userId, int chargeNum)
        {
            //if (oldVipLevel >= newVipLevel) return oldVipLevel;

            if (newVipLevel > oldVipLevel)
            {
                var userCity = DataStorage.Current.Load<UserCity>(userId, true);

                userCity.InitVipNum(newVipLevel);
            }
            else
            {
                newVipLevel = oldVipLevel;
            }

            if (chargeNum > 0)
            {
                var userTask = DataStorage.Current.Load<UserTask>(userId, true);
                userTask.TodayRecharge += chargeNum;
                userTask.ChangeTodayRechargeNewMsg();
            }
            return newVipLevel;
        }

        #endregion

        #region 清理用户垃圾数据
        /// <summary>
        /// 清理用户垃圾数据
        /// </summary>
        /// <param name="userId"></param>
        public static void ClearRdata(int userId)
        {
            var uid = userId;
            var userConcubine = DataStorage.Current.Load<UserConcubine>(uid, true);
            for (var i = userConcubine.Items.Count - 1; i >= 0; i--)
            {
                var userConcubineItem = userConcubine.Items[i];
                var sysConcubineCfg = SysConcubineCfg.Find(userConcubineItem.ConcubineId);
                if (sysConcubineCfg == null)
                    userConcubine.Items.Remove(userConcubineItem);
            }
            var userBuilding = DataStorage.Current.Load<UserBuilding>(uid, true);
            foreach (var userBuildingItem in userBuilding.Items)
            {
                for (var i = userBuildingItem.ConcubineIdList.Count - 1; i >= 0; i--)
                {
                    var id = userBuildingItem.ConcubineIdList[i];
                    var item = userConcubine.Items.Find(o => o.Id == id);
                    if (item == null)
                        userBuildingItem.ConcubineIdList.Remove(id);
                }
            }
            var userHero = DataStorage.Current.Load<UserHero>(uid, true);
            for (var i = userHero.Items.Count - 1; i >= 0; i--)
            {
                var userHeroItem = userHero.Items[i];
                var sysHeroCfg = SysHeroCfg.Find(userHeroItem.HeroId);
                if (sysHeroCfg == null)
                    userHero.Items.Remove(userHeroItem);
            }

            var userChip = DataStorage.Current.Load<UserChip>(uid, true);
            for (var i = userChip.ChipItems.Count - 1; i >= 0; i--)
            {
                var chipItem = userChip.ChipItems[i];
                var sysToolCfg = SysToolCfg.Find(chipItem.ItemId);
                if (sysToolCfg == null) userChip.ChipItems.Remove(chipItem);
            }

            var userTool = DataStorage.Current.Load<UserTool>(uid, true);
            for (var i = userTool.Items.Count - 1; i >= 0; i--)
            {
                var toolItem = userTool.Items[i];
                var sysToolCfg = SysToolCfg.Find(toolItem.ItemId);
                if (sysToolCfg == null) userTool.Items.Remove(toolItem);
            }

            GMMail gmMail = DataStorage.Current.Load<GMMail>(uid, true);
            for (var i = gmMail.GMIdList.Count - 1; i >= 0; i--)
            {
                var gmId = gmMail.GMIdList[i];
                var attach = (List<ItemPair>)gmMail.GetAttach(gmId);
                foreach (var itemPair in attach)
                {
                    var sysToolCfg = SysToolCfg.Find(itemPair.ItemId);
                    if (sysToolCfg == null)
                    {
                        gmMail.Delete(gmId);
                    }
                }
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 获取特殊道具ID
        /// </summary>
        /// <param name="moneyType"></param>
        /// <returns></returns>
        public static SpecialToolId GetSpecialToolId(MoneyType moneyType)
        {
            if (MoneyType.Money == moneyType) return SpecialToolId.Money;
            if (MoneyType.Coin == moneyType) return SpecialToolId.Coin;
            if (MoneyType.Wood == moneyType) return SpecialToolId.Wood;
            if (MoneyType.Stone == moneyType) return SpecialToolId.Stone;
            if (MoneyType.Iron == moneyType) return SpecialToolId.Iron;
            if (MoneyType.Sp == moneyType) return SpecialToolId.Sp;
            return SpecialToolId.Coin;
        }
        /// <summary>
        /// 时间戳转为C#格式时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime); return dtStart.Add(toNow);
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name=”time”></param>
        /// <returns></returns>
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }


        /// <summary>
        /// 获取字符串长度 区分中文英文
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int GetLength(string text)
        {

            int len = 0;

            for (int i = 0; i < text.Length; i++)
            {

                byte[] byteLen = Encoding.Default.GetBytes(text.Substring(i, 1));

                if (byteLen.Length > 1)
                    len += 2; //如果长度大于1，是中文，占两个字节，+2

                else
                    len += 1;  //如果长度等于1，是英文，占一个字节，+1

            }
            return len;
        }

        /// <summary>
        /// 获取固定个数的列表
        /// </summary>
        /// <param name="list"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<int> GetNumList(List<int> list, int num)
        {
            List<int> templist = new List<int>();
            foreach (var i in list) templist.Add(i);
            List<int> resultlist = new List<int>();
            int index = 0;
            foreach (var i in templist)
            {
                index = Util.GetRandom(0, list.Count);
                resultlist.Add(list[index]);
                if (resultlist.Count >= num) break;

                list.RemoveAt(index);
            }

            return resultlist;
        }

        /// <summary>
        /// 获取随机列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputList"></param>
        /// <returns></returns>
        public static List<T> GetRandomList<T>(List<T> inputList)
        {
            //Copy to a array
            T[] copyArray = new T[inputList.Count];
            inputList.CopyTo(copyArray);

            //Add range
            List<T> copyList = new List<T>();
            copyList.AddRange(copyArray);

            //Set outputList and random
            List<T> outputList = new List<T>();
            Random rd = new Random(DateTime.Now.Millisecond);

            while (copyList.Count > 0)
            {
                //Select an index and item
                int rdIndex = rd.Next(0, copyList.Count - 1);
                T remove = copyList[rdIndex];

                //remove it from copyList and add it to output
                copyList.Remove(remove);
                outputList.Add(remove);
            }
            return outputList;
        }


        /// <summary>
        /// 获取需要使用的元宝数(取上限)
        /// </summary>
        /// <param name="totalTime">总时间</param>
        /// <param name="everyIngotRemoveTime">每元宝可清除的时间</param>
        /// <returns></returns>
        public static int GetNeedNum(int totalTime, int everyIngotRemoveTime)
        {
            double i = (double)totalTime / everyIngotRemoveTime;
            int j = totalTime / everyIngotRemoveTime;

            if (i < 1) return 1;
            if (i > j) return j + 1;
            return j;
        }


        /// <summary>
        /// 评星活动
        /// </summary>
        /// <param name="Userid"></param>
        /// <param name="IngotCount"></param>
        public static void InsertEvaluateLog(int Userid, int IngotCount)
        {
            var sql =
               string.Format(
                   "insert into EvaluateLog (GetTime,UserId,IngotCount) values('{0}',{1},{2})",
                   DateTime.Now, Userid, IngotCount);
            //SqlHelper.ExecuteNonQuery(ParamHelper.GameServer, CommandType.Text, sql);
            MySqlHelper.ExecuteNonQuery(ParamHelper.GameServer, CommandType.Text, sql);
        }

        /// <summary>
        /// 判断上一个时间和当前时间是否跨越了所给的星期数
        /// </summary>
        /// <param name="oldTime">上次保存的时间点</param>
        /// <param name="week">重置的星期数</param>
        /// <returns></returns>
        public static bool IfBetweenDayContainDay(DateTime oldTime, DayOfWeek week)
        {
            bool needReset = false;
            if (week == DateTime.Now.DayOfWeek)
            {//当天为重置的星期数
                needReset = true;
            }
            else if (Convert.ToDateTime(oldTime.AddDays(7).ToShortDateString()) <=
                Convert.ToDateTime(DateTime.Now.ToShortDateString()))
            {//前后间隔等于或超过一周
                needReset = true;
            }
            else if (DateTime.Now.DayOfWeek < oldTime.DayOfWeek)
            {//前后间隔跨越了重置星期数（没超过一周 此情况只有在初始数据那周会出现）
                switch (week)
                {
                    case DayOfWeek.Sunday:
                    case DayOfWeek.Saturday:
                        needReset = true; break;
                    default:
                        if (DateTime.Now.DayOfWeek > week)
                            needReset = true;
                        break;
                }
            }
            return needReset;
        }

        /// <summary>
        /// 判断字符串是否是整数
        /// </summary>
        /// <param name="inString"></param>
        /// <returns></returns>
        public static bool IsInt(string inString)
        {
            Regex regex = new Regex("^[0-9]*[1-9][0-9]*$");
            return regex.IsMatch(inString.Trim());
        }

        /// <summary>
        /// 解一元二次方程 求得具体点应该带的士兵数量
        /// </summary>
        /// <param name="a">二次系数</param>
        /// <param name="b">一次系数</param>
        /// <param name="c">常量</param>
        /// <param name="isBiger">士兵伤害是否大于应该的伤害</param>
        /// <returns></returns>
        public static int GetDueSoldierNum(double a, double b, double c, bool isBiger)
        {
            double d, e, f, g, h, i, j, k;
            d = b * b;
            e = 4 * a * c;
            f = d - e;
            g = (int)(Math.Sqrt(f));
            i = -b + g;
            j = -b - g;
            h = i / (2 * a);
            k = j / (2 * a);
            //只有一个根
            if (f == 0) return (int)(h < 0 ? 0 : h);
            //有两个根
            else if (f > 0)
            {
                if (h > 0 && k < 0) return (int)h;
                if (k > 0 && h < 0) return (int)k;
                if (h > 0 && k > 0)
                {
                    if (isBiger)
                    {//取大值
                        if (h >= k) return (int)h;
                        else return (int)k;
                    }
                    else
                    {//取小值
                        if (h >= k) return (int)k;
                        else return (int)h;
                    }
                }
            }
            //无根
            else
            {
                return 0;
            }
            return 0;
        }

        /// <summary>
        /// 获取一个数 在列表中的位置
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIndexFromList(List<int> list, int value)
        {
            int i = 0;
            for (; i < list.Count; i++)
            {
                if (list[i] > value)
                    break;
            }
            i = i > (list.Count - 1) ? (list.Count - 1) : i;
            return i;
        }

        /// <summary>
        /// 发送系统邮件
        /// </summary>
        /// <param name="userId">目标用户主公信息</param>
        /// <param name="title">邮件头</param>
        /// <param name="msg">邮件内容</param>
        /// <param name="attack">附件的格式</param>
        public static void SendGmMailToTarget(int userId, string title, string msg, object attack = null, string sendTimeStr = null)
        {
            var gmmail = DataStorage.Current.Load<GMMail>(userId, true);
            gmmail.Send(title, msg, attack, sendTimeStr);
        }

        /// <summary>
        /// 在权重列表中的获取位置
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int GetIndexFromWeightsList(List<int> list)
        {
            var totalWeights = list.Sum(o => o);
            var randNum = Util.GetRandom(1, totalWeights + 1);
            var num = 0;
            int index = 0;
            foreach (var i in list)
            {
                //根据总权重 随机出哪个类型
                num += i;
                if (randNum <= num)
                {
                    break;
                }
                index++;
            }
            return index;
        }
        #endregion

        #region 修复竞技场多发次数

        public class ItemLogError
        {
            public int UserId { get; set; }
            public int Num { get; set; }
        }

        /// <summary>
        /// 清理用户垃圾数据
        /// </summary>
        /// <param name="userId"></param>
        public static void ModifyErrorData(int userId)
        {
            var userRole = DataStorage.Current.Load<UserRole>(userId, true);
            var logItem = new GameLogItem();
            try
            {
                if (userRole.ModifyErrorNum == 10) return;

                //删除还没有领取的错误邮件
                var gmmail = DataStorage.Current.Load<GMMail>(userId, true);
                var mailList = gmmail.GetMailList(0, 200);

                logItem.F1 = mailList.Count;
                logItem.S1 = JsonConvert.SerializeObject(mailList);
                logItem.F2 = 0;
                MailItem tmpmailItem = null;
                //foreach (var mailItem in mailList)
                var lastTime = new DateTime(2015, 12, 16, 21, 0, 0);
                for (int i = mailList.Count - 1; i >= 0; i--)
                {
                    var mailItem = mailList[i];
                    if (mailItem.SendTime >= lastTime)
                    {
                        if (mailItem.Title.Equals("竞技场每日排名奖励") && mailItem.Attach != null)
                        {
                            if (tmpmailItem == null)
                            {
                                tmpmailItem = mailItem;
                            }
                            logItem.F2 += 1;
                            gmmail.Delete(mailItem.MsgId);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (tmpmailItem != null)
                {
                    logItem.S2 = JsonConvert.SerializeObject(tmpmailItem);
                    SendGmMailToTarget(userId, "竞技场每日排名奖励", tmpmailItem.Msg, tmpmailItem.Attach,
                        tmpmailItem.SendTime.ToString());
                }
                else
                {
                    var userPk = DataStorage.Current.Load<UserPk>(userId);
                    var rank = userPk.Rank;

                    var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.StarRank <= rank && o.EndRank >= rank);
                    if (sysPkCfg != null)
                    {
                        var msg = LangResource.GetLangResource(ResourceId.R_DayRankMsg,
                                                       21, rank);

                        var attach = new List<ItemPair>();

                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Money, Num = sysPkCfg.Money });
                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Coin, Num = sysPkCfg.Coin });
                        attach.Add(new ItemPair() { ItemId = (int)SpecialToolId.Honor, Num = sysPkCfg.Honor });
                        attach.Add(new ItemPair() { ItemId = sysPkCfg.ToolId, Num = sysPkCfg.ToolNum });

                        logItem.S2 = JsonConvert.SerializeObject(attach);
                        Utility.SendGmMailToTarget(userId, "竞技场每日排名奖励", msg, attach, lastTime.ToString());
                    }
                }

                var sql =
                    string.Format(
                        "select UserId,Num from ItemLog201512 where itemtype=8 and userid={0} and opcode=1303 and LogTime>'2015-12-16 21:00' order by id desc;",
                        userId);
                logItem.S3 = sql;

                using (var conn = Util.ObtainConn(ParamHelper.GameLog))
                {
                    logItem.S4 = ParamHelper.GameLog;
                    var result = conn.Query<ItemLogError>(sql);
                    if (result == null)
                    {
                        userRole.ModifyErrorNum = 10;
                        GameLogManager.CommonLog((int)SpecialLogType.ModifyPkRewardErrorLog, userId, 0, logItem);
                        return;
                    }
                    var resultList = result.ToList();
                    var totalcount = resultList.Count;
                    if (totalcount > 1)
                    {
                        var num = resultList[0].Num;
                        logItem.S5 = JsonConvert.SerializeObject(resultList);
                        logItem.F3 = totalcount;
                        logItem.F4 = num;

                        var sysPkCfg = SysPkCfg.Items.FirstOrDefault(o => o.Honor == num);
                        if (sysPkCfg != null)
                        {
                            logItem.S6 = JsonConvert.SerializeObject(sysPkCfg);
                            int subMoney, subGiveMoney;
                            var needMoney = sysPkCfg.Money * totalcount;
                            logItem.F5 = needMoney;
                            RoleManager.Consume(userRole, (int)SpecialLogType.ModifyPkRewardErrorLog, needMoney, 1, out subMoney, out subGiveMoney);

                            var honor = sysPkCfg.Honor * totalcount;
                            userRole.ConsumeResource(ItemType.Honor, (int)SpecialLogType.ModifyPkRewardErrorLog, honor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.InfoFormat("ModifyErrorData:Message:{0}", ex.Message);
            }
            userRole.ModifyErrorNum = 10;
            GameLogManager.CommonLog((int)SpecialLogType.ModifyPkRewardErrorLog, userId, 0, logItem);
        }

        #endregion
    }
}
