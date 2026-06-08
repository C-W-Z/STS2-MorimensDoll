using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Patching.Models;

namespace MorimensDoll.Patches.Minion;

public class DebugDoomPowerPatch : IPatchMethod
{
    public static string PatchId => "morimens_doll_debug_doom_power";
    public static string Description => "Debug Doom Power";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() => [new(typeof(DoomPower), nameof(DoomPower.BeforeSideTurnEnd))];

    public static void Prefix(DoomPower __instance, CombatSide side, IEnumerable<Creature> participants)
    {
        if (__instance == null || __instance.Owner == null)
            return;

        Entry.Logger.Debug($"DebugDoomPowerPatch: Doom owner: {__instance.Owner.Name}, participants");
        foreach (var p in participants)
            Entry.Logger.Debug(p.Name);

        // 判定如果是你的隨從觸發了這個 Power
        if (__instance.Owner.IsPet && __instance.Owner.CombatState != null)
        {
            var owner = __instance.Owner;
            var sideCreatures = owner.CombatState.GetCreaturesOnSide(side);
            var doomed = DoomPower.GetDoomedCreatures(sideCreatures);

            Entry.Logger.Debug($"================ [Doom Debug Start] ================");
            Entry.Logger.Debug($"[1] 當前結算 Side: {side} | 隨從自身 Side: {owner.Side}");
            Entry.Logger.Debug($"[2] Power擁有的隨從: {owner.Name} | HashCode: {owner.GetHashCode()} | HP: {owner.CurrentHp} | DoomAmount: {__instance.Amount}");
            Entry.Logger.Debug($"[3] GetCreaturesOnSide 回傳數量: {sideCreatures.Count}");

            foreach (var c in sideCreatures)
            {
                var p = c.GetPower<DoomPower>();
                Entry.Logger.Debug($"    -> 列表中的角色: {c.Name} | HashCode: {c.GetHashCode()} | 有無DoomPower: {p != null} | 該實例是否Doomed: {p?.IsOwnerDoomed()}");
            }

            Entry.Logger.Debug($"[4] 最終 GetDoomedCreatures 數量: {doomed.Count}");
            if (doomed.Count > 0)
            {
                Entry.Logger.Debug($"    -> 誰是 First: {doomed[0].Name} | HashCode: {doomed[0].GetHashCode()}");
                Entry.Logger.Debug($"    -> First == Owner 判定結果: {doomed[0] == owner}");
            }
            Entry.Logger.Debug($"================ [Doom Debug End] ================");
        }
    }
}
