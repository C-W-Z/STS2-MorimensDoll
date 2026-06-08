using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Morimens.Anims;
using Morimens.Characters;
using Morimens.Minions;
using Morimens.Targeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionGrow() : AbstractMinionCard(1, CardType.Skill, CardRarity.Common, DollTargetTypes.AllDollMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(5m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> minions = await CheckMinionExistAndSummon(choiceContext);

        foreach (var minion in minions)
        {
            await CreatureCmd.TriggerAnim(minion.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
            await CreatureCmd.GainMaxHp(minion.Creature, DynamicVars.MaxHp.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(3m);
    }
}
