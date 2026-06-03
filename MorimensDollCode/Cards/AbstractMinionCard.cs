using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MinionLib.Commands;
using MinionLib.Minion;
using MinionLib.Utilities;
using MorimensDoll.Minions;

namespace MorimensDoll.Cards;

public abstract class AbstractMinionCard(int baseCost, CardType type, CardRarity rarity, TargetType target) : AbstractDollCard(baseCost, type, rarity, target)
{
    protected override HashSet<CardTag> CanonicalTags => [DollCardTag.MinionCmd];

    protected async Task<List<Creature>> CheckMinionExistAndSummon(PlayerChoiceContext choiceContext)
    {
        List<Creature>? pets = PetsOrderAccessor.GetRawPetsList(Owner);
        if (pets == null || pets.Count == 0)
        {
            await MinionCmd.AddMinion<DollMinion>(choiceContext, Owner, new MinionSummonOptions(
                MaxHp: DollMinion.MAX_HP / 2,           // 血量
                PrimaryStatAmount: 1m,                  // 主要参数（具体内容在随从的 OnSummon 里定义），还有次要参数等可以按需传入
                Source: this,                           // 召唤来源（通常是这张牌）
                Position: MinionPosition.Front));       // 站位（见后文，默认是前排）
            return [];
        }
        return pets;
    }
}
