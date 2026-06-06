using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MorimensDoll.Characters;
using MorimensDoll.Minion;
using MorimensDoll.Minions;
using MorimensDoll.Powers.Tmp;
using MorimensDoll.Targeting;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class MinionAttack() : AbstractMinionCard(1, CardType.Attack, CardRarity.Common, DollTargetTypes.AllDollMinions)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MinionAttackPower>(1m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<DollMinion> minions = await CheckMinionExistAndSummon(choiceContext);

        if (DynamicVars["MinionAttackPower"].BaseValue > 0)
        {
            await PowerCmd.Apply<MinionAttackPower>(choiceContext,
                minions.Select(m => m.Creature),
                DynamicVars["MinionAttackPower"].BaseValue,
                Owner.Creature, this);
        }

        foreach (var minion in minions)
            await DollMinionCmd.AttackRandomEnemy(choiceContext, minion, null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MinionAttackPower"].UpgradeValueBy(1m);
    }
}
