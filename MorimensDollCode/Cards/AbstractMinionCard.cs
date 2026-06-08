using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MorimensDoll.CardTags;
using MorimensDoll.Minions;

namespace MorimensDoll.Cards;

public abstract class AbstractMinionCard(int baseCost, CardType type, CardRarity rarity, TargetType target) : AbstractDollCard(baseCost, type, rarity, target)
{
    protected override HashSet<CardTag> CanonicalTags => [DollCardTag.MinionCmd];

    protected async Task<List<DollMinion>> CheckMinionExistAndSummon(PlayerChoiceContext choiceContext)
    {
        ArgumentNullException.ThrowIfNull(Owner);
        List<DollMinion> minions = DollMinionCmd.GetAllDollMinions(Owner);
        if (minions.Count == 0)
            await DollMinionCmd.Summon(choiceContext, Owner, this);
        return minions;
    }
}
