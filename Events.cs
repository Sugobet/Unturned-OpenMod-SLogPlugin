using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.API.Eventing;
using OpenMod.Unturned.Building.Events;
using System.Threading.Tasks;
using OpenMod.Unturned.Players.Inventory.Events;
using OpenMod.Unturned.Vehicles.Events;
using OpenMod.Core.Eventing;
using SDG.Unturned;
using OpenMod.Unturned.Players;
using Steamworks;
using System.Runtime.Serialization;
using System.Numerics;
using UnityEngine;
using System.Runtime.Remoting.Contexts;
using Autofac.Core;
using OpenMod.Unturned.Vehicles;
using UnityEngine.UIElements;

namespace MyOpenModPlugin
{
    public class UnturnedBuildableDestroyingEventListener : IEventListener<UnturnedBuildableDestroyingEvent>
    {
        /*
         * 建筑、结构、路障损毁中，拥有所有者时 写入日志
         */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedBuildableDestroyingEvent @event)
        {
            string? ownr = @event.Buildable.Ownership.OwnerPlayerId;
            if (ownr == null) { return; }

            UnturnedPlayer? instigator = @event.Instigator;
            string pos = "";
            string playerName = "";
            string playerSteamID = "";
            if (instigator != null)
            {
                pos = instigator.Transform.Position.ToString();
                playerName = instigator.DamageSourceName;
                playerSteamID = instigator.SteamId.ToString();
            }

            CSteamID ownrSteamID = new CSteamID(ulong.Parse(ownr));
            SteamPlayer ownrPlayer = PlayerTool.getPlayer(ownrSteamID).channel.owner;
            string ownrName = ownrPlayer.playerID.characterName;
            string buildID = @event.Buildable.Asset.BuildableAssetId;
            string buildType = @event.Buildable.Asset.BuildableType;
            string nowTime = DateTime.Now.ToString("yyyy年MM月dd HH时mm分ss秒");
            string damageOrigin = @event.DamageOrigin.ToString();
            string damageAmount = @event.DamageAmount.ToString();
            await FileCTL.AppendAllTextAsync($"{nowTime} - 玩家：{playerName} SteamID：{playerSteamID} 在坐标：{pos} 摧毁了 玩家：{ownrName} SteamID:{ownr} 的 {buildType} 物品ID：{buildID} 伤害来源：{damageOrigin} 受攻击次数：{damageAmount}");
        }
    }

    public class UnturnedPlayerDroppedItemEventListener : IEventListener<UnturnedPlayerDroppedItemEvent>
    {
        /*
         * 玩家丢弃（掉落）物品，写入日志
        */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerDroppedItemEvent @event)
        {
            UnturnedPlayer player = @event.Player;
            string playerName = player.DamageSourceName;
            string playerSteamID = player.SteamId.ToString();
            string playerPos = player.Transform.Position.ToString();
            string itemID = @event.Item.id.ToString();
            long itemAmount = Convert.ToInt64(@event.Item.amount);
            string itemName = (@event.Item.GetAsset()).itemName;
            string nowTime = DateTime.Now.ToString("yyyy年MM月dd HH时mm分ss秒");
            await FileCTL.AppendAllTextAsync($"{nowTime} - 玩家：{playerName} SteamID：{playerSteamID} 在坐标：{playerPos} 丢落物品：{itemName} 物品ID：{itemID} 物品个数：{itemAmount}");
        }
    }

    public class UnturnedPlayerTakingItemEventListener : IEventListener<UnturnedPlayerTakingItemEvent>
    {
        /*
         * 玩家拾取掉落物品
         */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerTakingItemEvent @event)
        {
            if (!@event.ItemData.isDropped) { return; }

            UnturnedPlayer player = @event.Player;
            string playerName = player.DamageSourceName;
            string playerSteamID = player.SteamId.ToString();
            string itemPos = @event.ItemData.point.ToString();
            string itemInstanceID = @event.ItemData.instanceID.ToString();
            
            ItemAsset itemAsset = @event.ItemData.item.GetAsset();
            string itemName = itemAsset.itemName;
            string itemAmount = itemAsset.amount.ToString();
            string itemID = @event.ItemData.item.id.ToString();
            string nowTime = DateTime.Now.ToString();

            await FileCTL.AppendAllTextAsync($"{nowTime} - 玩家：{playerName} SteamID：{playerSteamID} 在坐标：{itemPos} 拾取了掉落物品：{itemName} 物品数量：{itemAmount} 物品ID：{itemID} 物品实例ID：{itemInstanceID}");
        }
    }

    public class UnturnedPlayerEnteredVehicleEventListener : IEventListener<UnturnedPlayerEnteredVehicleEvent>
    {
        /*
         * 玩家进入车辆
         */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerEnteredVehicleEvent @event)
        {
            UnturnedPlayer player = @event.Player;
            string playerName = player.DamageSourceName;
            string playerPos = player.Transform.Position.ToString();
            string playerSteamID = player.SteamId.ToString();

            UnturnedVehicle vehicle = @event.Vehicle;
            string vehicleType = vehicle.Asset.VehicleType;
            string vehicleName = vehicle.Asset.VehicleName;
            string vehicleID = vehicle.Asset.VehicleAssetId;
            string vehicleInstanceID = vehicle.VehicleInstanceId;
            string vehicleEng = vehicle.Vehicle.asset.engine.ToString();

            UnturnedPlayer? vehicleDriver = vehicle.Driver as UnturnedPlayer;
            string driverName = "";
            string driverSteamID = "";
            if (vehicleDriver != null) {
                driverName = vehicleDriver.DamageSourceName;
                driverSteamID = vehicleDriver.SteamId.ToString();
            }

            string nowTime = DateTime.Now.ToString();

            await FileCTL.AppendAllTextAsync($"{nowTime} - 玩家：{playerName} SteamID：{playerSteamID} 在坐标：{playerPos} 进入了车辆：{vehicleName} 车辆类型：{vehicleType} 车辆引擎：{vehicleEng} 车辆ID：{vehicleID} 车辆实例ID：{vehicleInstanceID} 司机：{driverName} 司机SteamID：{driverSteamID}");
        }
    }

    public class UnturnedPlayerExitedVehicleEventListener : IEventListener<UnturnedPlayerExitedVehicleEvent>
    {
        /*
         * 玩家进入车辆
         */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedPlayerExitedVehicleEvent @event)
        {
            UnturnedPlayer player = @event.Player;
            string playerName = player.DamageSourceName;
            string playerPos = player.Transform.Position.ToString();
            string playerSteamID = player.SteamId.ToString();

            UnturnedVehicle vehicle = @event.Vehicle;
            string vehicleType = vehicle.Asset.VehicleType;
            string vehicleName = vehicle.Asset.VehicleName;
            string vehicleID = vehicle.Asset.VehicleAssetId;
            string vehicleInstanceID = vehicle.VehicleInstanceId;
            string vehicleEng = vehicle.Vehicle.asset.engine.ToString();

            UnturnedPlayer? vehicleDriver = vehicle.Driver as UnturnedPlayer;
            string driverName = "";
            string driverSteamID = "";
            if (vehicleDriver != null)
            {
                driverName = vehicleDriver.DamageSourceName;
                driverSteamID = vehicleDriver.SteamId.ToString();
            }

            string nowTime = DateTime.Now.ToString();

            await FileCTL.AppendAllTextAsync($"{nowTime} - 玩家：{playerName} SteamID：{playerSteamID} 在坐标：{playerPos} 离开了车辆：{vehicleName} 车辆类型：{vehicleType} 车辆引擎：{vehicleEng} 车辆ID：{vehicleID} 车辆实例ID：{vehicleInstanceID} 司机：{driverName} 司机SteamID：{driverSteamID}");
        }
    }

    public class UnturnedVehicleDamagingEventListener : IEventListener<UnturnedVehicleDamagingEvent>
    {
        /*
         * 载具正在遭受攻击，且血量 <= 0 判为将要爆炸
         */
        [EventListener()]
        public async Task HandleEventAsync(object? sender, UnturnedVehicleDamagingEvent @event)
        {
            int currentHealth = @event.Vehicle.Vehicle.health - @event.PendingTotalDamage;
            if (currentHealth > 0) { return; }


        }
    }
}
