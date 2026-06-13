using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.ExEnergy;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class DollExalt : AbstractExaltCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10m, ValueProp.Move),
        new HealVar(10m),
        ModCardVars.Int("Aliemus", 20)
    ];

    public override async Task Execute(Player player)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        await CreatureCmd.TriggerAnim(player.Creature, DollSpine.State.ExSkill, DollSpine.ExSkillAnimDelay);
        // 驅散友方易傷狀態，全體友方回10血，全體友方+20狂
        foreach (var ally in CombatState.Allies)
            await PowerCmd.Remove<VulnerablePower>(ally);
        foreach (var ally in CombatState.Allies)
            await CreatureCmd.Heal(ally, DynamicVars.Damage.BaseValue);
        foreach (var ally in CombatState.Players)
            if (!LocalContext.IsMe(ally))
                await SecondaryResourceCmd.Gain(ally, ExEnergyManager.AliemusId, DynamicVars["Aliemus"].IntValue, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)
            .Execute(null);
    }
}
