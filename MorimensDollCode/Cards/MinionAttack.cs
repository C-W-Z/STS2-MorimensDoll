using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Targeting;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using MorimensDoll.Minions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))] // TODO: MinionTargetTypes.AllMinions 要改成自訂的只有 DollMinion 的 TargetType
public sealed class MinionAttack() : AbstractMinionCard(1, CardType.Skill, CardRarity.Common, MinionTargetTypes.AllMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> pets = await CheckMinionExistAndSummon(choiceContext);

        foreach (var minion in pets)
            await DollMinionCmd.AttackRandomEnemy(choiceContext, minion, null);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
