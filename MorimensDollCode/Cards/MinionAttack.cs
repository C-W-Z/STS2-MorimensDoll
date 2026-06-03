using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Minion;
using MinionLib.Targeting;
using MorimensDoll.Characters;
using MorimensDoll.Minions.Actions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionAttack() : AbstractDollCard(1, CardType.Skill, CardRarity.Common, MinionTargetTypes.AnyMinion)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MinionAttackAction>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { Monster: MinionModel } target) return;

        await PowerCmd.Apply<MinionAttackAction>(choiceContext, target, DynamicVars["MinionAttackAction"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MinionAttackAction"].UpgradeValueBy(1m);
    }
}
