using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileGame.Core;
using MobileGame.Core.ObjectMapper.EmitInvoker;
using MobileGame.tianzi.Common;
using MobileGame.tianzi.Entity;

namespace MobileGame.tianzi.ConfigStruct
{
    /// <summary>
    /// 道具配置
    /// 1、编号说明： ABBBCCCC
    /// A
    /// 1表示普通道具（没有使用的按钮，例如装备进阶材料）
    /// 2表示功能道具（后面有使用按钮，点击使用会弹出其他窗口，例如宝箱）
    /// BBB
    /// 100表示金钱
    /// 200表示经验球
    /// 2、编号说明： ABBBBBBB
    /// 7表示武将碎片
    /// 8表示妃子碎片
    /// 9表示装备碎片
    /// 碎片类的道具参数1表示合成道具ID参数2表示合成需要的数量
    /// </summary>
    public class SysToolCfg : TableCfg<SysToolCfg>
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 品质
        /// </summary>
        public ItemQuality Quality { get; set; }
        /// <summary>
        /// 抽到权重（0为不可被抽到）
        /// </summary>
        public int ExtractWeights { get; set; }
        /// <summary>
        /// 使用等级限制
        /// </summary>
        public int NeedLevel { get; set; }
        /// <summary>
        /// 根据不同的道具类型有不同的意义
        /// </summary>
        public int Param1 { get; set; }
        /// <summary>
        /// 根据不同的道具类型有不同的意义
        /// </summary>
        public int Param2 { get; set; }
        /// <summary>
        /// 根据不同的道具类型有不同的意义
        /// </summary>
        public int Param3 { get; set; }
        /// <summary>
        /// 根据不同的道具类型有不同的意义
        /// </summary>
        public int Param4 { get; set; }
        /// <summary>
        /// 根据不同的道具类型有不同的意义
        /// </summary>
        public int Param5 { get; set; }
        /// <summary>
        /// 售价类型
        /// </summary>
        public MoneyType MoneyType { get; set; }
        /// <summary>
        /// 售价
        /// </summary>
        public int BuyPrice { get; set; }
        /// <summary>
        /// 出售的价格
        /// </summary>
        public int SellPrice { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Introduce { get; set; }

        /// <summary>
        /// 获取道具类型
        /// </summary>
        /// <returns></returns>
        public ToolType GetToolType()
        {
            if (Id < 9999999) return ToolType.None;
            //编号格式ABBBCCCC
            var a = Convert.ToInt32(Id.ToString().Substring(0, 1));
            //6/7/8/9代表碎片 编号格式ABBBBBBB
            if (a >= 6)
            {
                var aa = a * 10000000;
                if (aa == (int)ToolType.PetChip) return ToolType.PetChip;
                if (aa == (int)ToolType.EquipChip) return ToolType.EquipChip;
                if (aa == (int)ToolType.HeroChip) return ToolType.HeroChip;
                if (aa == (int)ToolType.ConcubineChip) return ToolType.ConcubineChip;
            }
            var typeId = Convert.ToInt32(Id.ToString().Substring(0, 3));
            return (ToolType)typeId;
        }

        /// <summary>
        /// 判断道具是否是碎片
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsChip(ToolType type)
        {
            if (type == ToolType.EquipChip || type == ToolType.HeroChip ||
                type == ToolType.ConcubineChip || type == ToolType.PetChip)
                return true;
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 添加到玩家身上
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="opCode"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool AddToUser(int userId, int opCode, int num = 1)
        {
            var result = false;
            var type = GetToolType();
            var isChip = IsChip(type);
            if (isChip)
            {
                //添加碎片
                var userChip = DataStorage.Current.Load<UserChip>(userId, true);
                result = userChip.AddChip(Id, num, type, opCode);
            }
            else if (type == ToolType.SpecialTool)
            {
                var userRole = DataStorage.Current.Load<UserRole>(userId, true);
                if (Id == (int)SpecialToolId.Money)
                {
                    RoleManager.AddGiveMoney(userRole, num);
                }
                else if (Id == (int)SpecialToolId.Coin)
                {
                    Utility.AddResource(userRole, ItemType.Coin, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Charm)
                {
                    Utility.AddResource(userRole, ItemType.Charm, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Repute)
                {
                    Utility.AddResource(userRole, ItemType.Repute, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Wood)
                {
                    Utility.AddResource(userRole, ItemType.Wood, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Stone)
                {
                    Utility.AddResource(userRole, ItemType.Stone, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Iron)
                {
                    Utility.AddResource(userRole, ItemType.Iron, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Honor)
                {
                    Utility.AddResource(userRole, ItemType.Honor, opCode, num);
                }
            }
            else if (type == ToolType.UpgradeVipTool)
            {
                var userRole = DataStorage.Current.Load<UserRole>(userId, true);
                var maxVipLevel = SysVipCfg.Items.Max(o => o.VipLevel);
                userRole.VipLevel += num;
                if (userRole.VipLevel > maxVipLevel) userRole.VipLevel = maxVipLevel;
            }
            else
            {
                //道具都是可叠加的物品
                var userTool = DataStorage.Current.Load<UserTool>(userId, true);
                result = userTool.TryAdd(Id, num, opCode);
            }
            return result;
        }

        /// <summary>
        /// 添加到玩家身上
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="userChip"></param>
        /// <param name="userRole"></param>
        /// <param name="userTool"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public bool AddToUser(int opCode, UserChip userChip, UserRole userRole, UserTool userTool, int num = 1)
        {
            var result = false;
            var type = GetToolType();
            var isChip = IsChip(type);
            if (isChip)
            {
                //添加碎片
                //var userChip = DataStorage.Current.Load<UserChip>(userId, true);
                result = userChip.AddChip(Id, num, type, opCode);
            }
            else if (type == ToolType.SpecialTool)
            {
                //var userRole = DataStorage.Current.Load<UserRole>(userId, true);
                if (Id == (int)SpecialToolId.Money)
                {
                    RoleManager.AddGiveMoney(userRole, num);
                }
                else if (Id == (int)SpecialToolId.Coin)
                {
                    Utility.AddResource(userRole, ItemType.Coin, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Charm)
                {
                    Utility.AddResource(userRole, ItemType.Charm, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Repute)
                {
                    Utility.AddResource(userRole, ItemType.Repute, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Wood)
                {
                    Utility.AddResource(userRole, ItemType.Wood, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Stone)
                {
                    Utility.AddResource(userRole, ItemType.Stone, opCode, num);
                }
                else if (Id == (int)SpecialToolId.Iron)
                {
                    Utility.AddResource(userRole, ItemType.Iron, opCode, num);
                }
            }
            else
            {
                //道具都是可叠加的物品
                //var userTool = DataStorage.Current.Load<UserTool>(userId, true);
                result = userTool.TryAdd(Id, num, opCode);
            }
            return result;
        }

        /// <summary>
        /// 使用道具添加物品到玩家身上
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="userRole"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<ItemPair> UseToUser(int opCode, UserRole userRole, int num)
        {
            var resItems = new List<ItemPair>();
            var toolType = GetToolType();
            switch (toolType)
            {
                case ToolType.CoinTool:
                    {
                        //var moneyTypeList = new List<MoneyType>() { MoneyType.Coin, MoneyType.Iron, MoneyType.Stone, MoneyType.Wood };
                        //foreach (var moneyType in moneyTypeList)
                        //{
                        //    if (Utility.CheckResourceIsFull(userRole, moneyType))
                        //    {
                        //        var moneyName = Utility.GetMonenyTypeName(moneyType);
                        //        //资源达到上限了
                        //        SetError(ResourceId.R_0000_ResourceIsFull, moneyName);
                        //        return ;
                        //    }
                        //}

                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)Utility.GetSpecialToolId(MoneyType.Coin),
                            Num = Param1 * num
                        });
                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)Utility.GetSpecialToolId(MoneyType.Wood),
                            Num = Param2 * num
                        });
                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)Utility.GetSpecialToolId(MoneyType.Stone),
                            Num = Param3 * num
                        });
                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)Utility.GetSpecialToolId(MoneyType.Iron),
                            Num = Param4 * num
                        });
                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)SpecialToolId.Money,
                            Num = Param5 * num
                        });

                        Utility.AddResource(userRole, ItemType.Coin, opCode, Param1 * num);
                        Utility.AddResource(userRole, ItemType.Wood, opCode, Param2 * num);
                        Utility.AddResource(userRole, ItemType.Stone, opCode, Param3 * num);
                        Utility.AddResource(userRole, ItemType.Iron, opCode, Param4 * num);

                        RoleManager.AddGiveMoney(userRole, Param5);

                        break;
                    }
                case ToolType.SpTool:
                    {
                        resItems.Add(new ItemPair()
                        {
                            ItemId = (int)Utility.GetSpecialToolId(MoneyType.Sp),
                            Num = Param1 * num
                        });
                        Utility.AddResource(userRole, ItemType.Sp, opCode, Param1 * num);
                        break;
                    }
                case ToolType.UpgradeVipTool:
                {
                    var sysToolCfg = SysToolCfg.Items.FirstOrDefault(o => o.Id == Id);
                    if (sysToolCfg != null)
                    {
                        sysToolCfg.AddToUser(userRole.Id, opCode, num);
                    }
                    break;
                    }
            }
            return resItems;
        }
    }
}
