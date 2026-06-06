using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MorimensDoll.Anims;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using MorimensDoll.Minions;
using MorimensDoll.Targeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionMerge() : AbstractMinionCard(2, CardType.Skill, CardRarity.Rare, DollTargetTypes.AllDollMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> minions = await CheckMinionExistAndSummon(choiceContext);
        if (minions.Count == 0)
            return;

        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.Skill2, DollSpine.Skill2AnimDelay);
        await DollMinionCmd.MergeAllDollMinions(choiceContext, Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(2m);
    }
}
