using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.ExEnergy;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;

namespace Morimens.Cards;

public sealed class MorimensOverExaltDoll : AbstractExaltCard
{
    public override CardPoolModel Pool => ModelDb.Get<DollCardPool>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(10m),
        ModCardVars.Int("Aliemus", 20),
        ModCardVars.Int("Turn", 3),
    ];

    public override async Task Execute()
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.ExSkill, DollSpine.ExSkillAnimDelay);
        // 驅散友方易傷狀態，全體友方回10血，全體友方+20狂
        foreach (var ally in CombatState.Allies)
            await PowerCmd.Remove<VulnerablePower>(ally);
        foreach (var ally in CombatState.Allies)
            await CreatureCmd.Heal(ally, DynamicVars.Heal.BaseValue);
        foreach (var ally in CombatState.Players)
            if (!LocalContext.IsMe(ally) && ally.Character is IAwaker)
                await SecondaryResourceCmd.Gain(ally, ExEnergyManager.AliemusId, DynamicVars["Aliemus"].IntValue, this);
    }
}
