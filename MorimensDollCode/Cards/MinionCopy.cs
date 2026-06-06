using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using MorimensDoll.Minions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))] // TODO: MinionTargetTypes.AnyMinion 要改成自訂的只有DollMinion 的 TargetType
public sealed class MinionCopy() : AbstractMinionCard(2, CardType.Skill, CardRarity.Uncommon, MinionTargetTypes.AnyMinion)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> minions = await CheckMinionExistAndSummon(choiceContext);
        if (minions.Count == 0)
            return;

        ArgumentNullException.ThrowIfNull(cardPlay.Target?.Monster);
        await DollMinionCmd.SummonCopy(choiceContext, Owner, (DollMinion)cardPlay.Target.Monster, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
