using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MinionLib.Minion;
using STS2RitsuLib.Patching.Models;

namespace Morimens.Patches.Minion;

/// <summary>
/// 0.107.0 版本，官方在 CreatureCmd.KillWithoutCheckingWinCondition() 中移除死亡單位時，
/// 僅針對 Enemy 進行了 CombatManager 與 CombatState 的移除判定，
/// 導致友方 Minions 死後依然殘留在 CombatManager 與 CombatState 中。
/// 本 Patch 在該死亡 Task 結束後，補上友方隨從的清理邏輯。
/// </summary>
public class MinionKillPatch : IPatchMethod
{
    public static string PatchId => "MORIMENS_minion_kill";
    public static string Description => "Remove minions on kill";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() => [new(typeof(CreatureCmd), nameof(CreatureCmd.KillWithoutCheckingWinCondition))];

    // 注意：因為原方法是 async Task，我們必須用 ref Task __result 來包裹並延伸它的非同步生命週期
    public static void Postfix(ref Task __result, Creature creature)
    {
        // 為了不破壞原本的非同步執行順序，我們將原本的 Task 傳入包裝函數中
        __result = AwaitAndCleanupAsync(__result, creature);
    }

    private static async Task AwaitAndCleanupAsync(Task originalTask, Creature creature)
    {
        // 1. 先靜靜等待官方原本的死亡流程全部跑完（包含動畫、Power移除等）
        await originalTask;

        // 2. 檢查這隻死去的生物，是不是被官方遺忘的「玩家方隨從」
        if (creature != null && creature.Side == CombatSide.Player && creature.IsPet &&
            creature.Monster is MinionModel && creature.CombatState != null)
        {
            ICombatState combatState = creature.CombatState;

            // 模仿官方的檢查：判斷該生物死後是否應該從戰鬥中移除
            bool shouldRemoveFromCombat = combatState != null && Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, creature);

            if (shouldRemoveFromCombat)
            {
                // 幫官方擦屁股：手動將友方隨從從戰鬥管理器與狀態中徹底蒸發
                CombatManager.Instance?.RemoveCreature(creature);
                combatState?.RemoveCreature(creature);

                Entry.Logger.Debug($"[MinionFix] 偵測到友方隨從 {creature.Name} 死亡，已手動將其從 CombatManager 與 CombatState 中安全移除。");
            }
        }
    }
}
