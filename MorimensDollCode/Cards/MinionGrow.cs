using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using MorimensDoll.Anims;
using MorimensDoll.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionGrow() : AbstractMinionCard(1, CardType.Skill, CardRarity.Common, MinionTargetTypes.AllMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(4m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<Creature> pets = await CheckMinionExistAndSummon(choiceContext);

        foreach (var minion in pets)
        {
            await CreatureCmd.TriggerAnim(minion, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
            await CreatureCmd.GainMaxHp(minion, DynamicVars.MaxHp.BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(2m);
    }
}
