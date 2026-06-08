using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Combat;
using STS2RitsuLib.Patching.Models;

namespace MorimensDoll.Patches.Minion;

public class MinionTurnEndPatch : IPatchMethod
{
    // 1. 填寫 RitsuLib 要求的唯一 ID 與說明
    public static string PatchId => "morimens_doll_minion_before_turn_end";

    public static string Description => "將戰鬥中存活的Minions放入回合結束的參與者名單中，主要為了修復災厄不會在Minions身上觸發的問題。";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(Hook), nameof(Hook.BeforeTurnEnd)),
        new(typeof(Hook), nameof(Hook.AfterTurnEnd))
    ];

    // 3. 實作 Prefix 補丁 (維持 RitsuLib 的規範，名稱必須為 Prefix)
    public static void Prefix(ICombatState combatState, CombatSide side, ref IEnumerable<Creature> participants)
    {
        if (participants == null) return;

        Entry.Logger.Debug($"MinionBeforeTurnEndPatch: original participants");
        foreach (var p in participants)
            Entry.Logger.Debug(p.Name.ToString());

        var updatedParticipants = participants.ToList();
        bool updated = false;

        foreach (var creature in participants)
        {
            // 只有玩家的回合結束需要把隨從拉進來結算
            if (!creature.IsPlayer || creature.Pets == null) continue;

            Entry.Logger.Debug($"MinionBeforeTurnEndPatch: Find Player {creature.Name}, Pet Count {creature.Pets.Count}");

            // 走訪收集到的所有隨從
            foreach (var pet in creature.Pets)
            {
                if (pet == null || pet.Monster == null || pet.IsDead) continue;

                Entry.Logger.Debug($"MinionBeforeTurnEndPatch: Find Pet {pet.Monster?.GetType().Namespace}");

                // 利用命名空間排除原版和其他模組的召喚物，只保留我們的自訂召喚物
                if (pet.Monster?.GetType().Namespace?.StartsWith(Entry.ModId) == false) continue;

                if (!updatedParticipants.Contains(pet))
                {
                    updatedParticipants.Add(pet);
                    updated = true;
                }
            }
        }

        // 如果名單有變動，透過 ref 關鍵字把修改後的新名單覆蓋回去
        if (updated)
        {
            participants = updatedParticipants;
        }

        Entry.Logger.Debug($"MinionBeforeTurnEndPatch: new participants");
        foreach (var p in participants)
            Entry.Logger.Debug(p.Name.ToString());
    }
}
