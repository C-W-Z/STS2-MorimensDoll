using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Scaffolding.Content;

namespace Morimens.Cards;

public abstract class AbstractExaltCard() : ModCardTemplate(0, CardType.None, CardRarity.None, TargetType.None)
{
    public abstract Task Execute(Player player);
}
