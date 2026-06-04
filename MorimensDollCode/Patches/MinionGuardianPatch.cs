using MegaCrit.Sts2.Core.Entities.Creatures;
using MinionLib.Minion;
using MinionLib.Powers.Patches;
using MorimensDoll.Powers;
using STS2RitsuLib.Patching.Models;

namespace MorimensDoll.Patches;

public class MinionGuardianPatch : IPatchMethod
{
    // 1. 設定 RitsuLib 要求的唯一 ID 與功能說明
    public static string PatchId => "morimens_doll_minion_guardian_patch";

    public static string Description => "擴充 MinionLib 的 IsFrontGuardian 判定，使其支援自訂的 DollMinionGuardianPower";

    public static bool IsCritical => true;

    // 3. 指定原版（或前置 Mod）的目標方法
    // RitsuLib 的 ModPatchTarget 內部會自動處理私有方法 (Private) 的反射尋找
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(MinionGuardianOverkillPatch), "IsFrontGuardian")];

    // 4. 實作 Postfix 後置補丁 (注意：名稱必須精準叫 Postfix)
    public static void Postfix(Creature creature, ref bool __result)
    {
        // 如果原本的檢測已經是 true（代表它是原生的 MinionGuardianPower），就不管它
        if (__result) return;

        // 如果不是，檢查它有沒有掛載我們自訂的新 Power
        // 並且同樣要符合「它是隨從，且站在前排」的規則
        if (creature.HasPower<DollMinionGuardianPower>())
        {
            __result = true; // 強行改成 true，欺騙原模組的溢出演算法！
        }
    }
}
