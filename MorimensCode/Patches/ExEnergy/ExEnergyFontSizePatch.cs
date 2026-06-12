
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using Morimens.ExEnergy;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Patching.Models;

namespace Morimens.Patches.ExEnergy;

public class ExEnergyFontSizePatch : IPatchMethod
{
    public static string PatchId => "MORIMENS_ex_energy_font_size";

    public static string Description => "固定secondary resources的font size";

    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() => [new(typeof(NSecondaryResourceCounter), nameof(NSecondaryResourceCounter._Ready))];

    public static void Postfix(NSecondaryResourceCounter __instance)
    {
        var definitionField = AccessTools.Field(typeof(NSecondaryResourceCounter), "_definition");
        if (definitionField?.GetValue(__instance) is SecondaryResourceDefinition definition &&
            (definition.Id == ExEnergyManager.AliemusId || definition.Id == ExEnergyManager.KeyflareId))
        {
            // 利用 Harmony 的 AccessTools 抓取私有的 _amountLabel 反射欄位
            var labelField = AccessTools.Field(typeof(NSecondaryResourceCounter), "_amountLabel");
            if (labelField?.GetValue(__instance) is MegaLabel amountLabel)
            {
                // 直接關閉自動縮放功能
                amountLabel.AutoSizeEnabled = false;
                // 讓最小字號等於最大字號，這樣它物理上就完全沒有縮小的空間了
                amountLabel.MinFontSize = amountLabel.MaxFontSize;
            }
        }
    }
}
