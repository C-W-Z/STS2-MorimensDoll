using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MorimensDoll.Minion;
using MorimensDoll.Minions;

namespace MorimensDoll.Cards;

public abstract class AbstractMinionCard(int baseCost, CardType type, CardRarity rarity, TargetType target) : AbstractDollCard(baseCost, type, rarity, target)
{
    protected override HashSet<CardTag> CanonicalTags => [DollCardTag.MinionCmd];

    protected async Task<IEnumerable<DollMinion>> CheckMinionExistAndSummon(PlayerChoiceContext choiceContext)
    {
        ArgumentNullException.ThrowIfNull(Owner);
        IEnumerable<DollMinion> minions = DollMinionCmd.GetAllDollMinions(Owner);
        if (!minions.Any())
            await DollMinionCmd.Summon(choiceContext, Owner, this);
        return minions;
    }
}
