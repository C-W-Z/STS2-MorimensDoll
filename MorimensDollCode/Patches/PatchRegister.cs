using MorimensDoll.Patches.Minion;
using STS2RitsuLib;
using STS2RitsuLib.Patching.Core;

namespace MorimensDoll.Patches;

public static class PatchRegister
{
    public static void Register()
    {
        ModPatcher minionLibPatcher = RitsuLibFramework.CreatePatcher(Entry.ModId, "minion-patches");
        minionLibPatcher.RegisterPatch<MinionGuardianPatch>();
        minionLibPatcher.RegisterPatch<MinionBeforeTurnEndPatch>();
        if (!minionLibPatcher.PatchAll())
            throw new InvalidOperationException("MorimensDoll critical minion-patches failed!");
    }
}
