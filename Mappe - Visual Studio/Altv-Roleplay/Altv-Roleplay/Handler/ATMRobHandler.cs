using AltV.Net;
using AltV.Net.Async;
using Altv_Roleplay.Factories;
using Altv_Roleplay.Model;
using Altv_Roleplay.models;
using Altv_Roleplay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altv_Roleplay.Handler
{
    class ATMRobHandler : IScript
    {
        internal static async Task RobATM(ClassicPlayer player, Server_ATM robATM)
        {
            try
            {
                if (player == null || !player.Exists || player.CharacterId <= 0 || robATM == null) return;
                if (player.HasPlayerHandcuffs() || player.HasPlayerRopeCuffs()) { HUDHandler.SendNotification(player, 3, 5000, "Wie willst du das mit Handschellen/Fesseln machen?"); return; }
                if (player.IsPlayerUsingCrowbar())
                {
                    player.EmitLocked("Client:Inventory:StopAnimation");
                    player.SetPlayerUsingCrowbar(false);
                    HUDHandler.SendNotification(player, 2, 1500, "Du hast den Aufbruch abgebrochen.");
                    return;
                }
                else
                {
                    //Aufbrechen
                    if (DateTime.Now.Subtract(robATM.lastRobbed).TotalMinutes < 1)
                    {
                        HUDHandler.SendNotification(player, 3, 2500, "Dieser Automat wurde vor kurzem erst aufgebrochen.");
                        return;
                    }

                    if (!CharactersInventory.ExistCharacterItem(player.CharacterId, "Schweißgerät", "inventory"))
                    {
                        HUDHandler.SendNotification(player, 3, 2500, "Du hast kein Schweißgerät dabei um den ATM aufzubrechen.");
                        return;
                    }

                    if (ServerFactions.GetFactionDutyMemberCount(2) + ServerFactions.GetFactionDutyMemberCount(12) < 0)
                    {
                        HUDHandler.SendNotification(player, 3, 2500, "Es sind weniger als 4 Polizisten im Staat.");
                        return;
                    }
                    foreach (var p in Alt.GetAllPlayers().Where(x => x != null && x.Exists && x.GetCharacterMetaId() > 0).ToList())
                    {
                        if (!ServerFactions.IsCharacterInAnyFaction((int)p.GetCharacterMetaId()) || !ServerFactions.IsCharacterInFactionDuty((int)p.GetCharacterMetaId()) || ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 2 && ServerFactions.GetCharacterFactionId((int)p.GetCharacterMetaId()) != 12) continue;
                        HUDHandler.SendNotification(p, 1, 9500, "Ein stiller Alarm wurde ausgelöst.");
                    }

                    ServerFactions.AddNewFactionDispatchNoName("Stiller Alarm", 2, "Ein aktiver ATM Raub wurde gemeldet.", player.Position);
                    ServerFactions.AddNewFactionDispatchNoName("Stiller Alarm", 12, "Ein aktiver ATM Raub wurde gemeldet.", player.Position);

                    robATM.lastRobbed = DateTime.Now;
                    int duration = 50000;
                    int rndmMoney = new Random().Next(6250, 8000);
                    player.SetPlayerUsingCrowbar(true);
                    InventoryHandler.InventoryAnimation(player, "breakUp", duration);
                    HUDHandler.SendNotification(player, 1, duration, "Du brichst den ATM auf..");
                    await Task.Delay(duration);
                    if (player == null || !player.Exists) return;
                    if (!player.Position.IsInRange(new AltV.Net.Data.Position(robATM.posX, robATM.posY, robATM.posZ), 10f)) { HUDHandler.SendNotification(player, 3, 5000, "Aufbrechen abgebrochen, du bist zu weit entfernt."); player.SetPlayerUsingCrowbar(false); return; }
                    if (!player.IsPlayerUsingCrowbar()) return;
                    HUDHandler.SendNotification(player, 2, 2500, $"ATM aufgebrochen, du hast {rndmMoney}$ erbeutet - verschwinde..");
                    player.SetPlayerUsingCrowbar(false);
                    InventoryHandler.StopAnimation(player, "anim@amb@clubhouse@tutorial@bkr_tut_ig3@", "machinic_loop_mechandplayer");
                    CharactersInventory.AddCharacterItem(player.CharacterId, "Bargeld", rndmMoney, "inventory");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }

}
}
