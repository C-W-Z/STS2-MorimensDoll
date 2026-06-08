using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Targeting;
using MinionLib.Targeting.Utilities;
using Morimens.Minions;

namespace Morimens.Targeting;

public static class DollTargetTypes
{
    private static bool IsMyDollMinion(CardModel card, Creature target)
    {
        // 確保目標存活、是寵物，且擁有者是打出這張牌的玩家 (聯機安全)
        return target is { IsAlive: true, IsPet: true } &&
               target.PetOwner == card.Owner &&
               target.Monster is DollMinion;
    }

    /// <summary>
    /// 我所有的 DollMinion (群體)
    /// </summary>
    public static readonly TargetType AllDollMinions = CustomTargetTypeManager.Register(
        new LambdaTargetType(
            false, // IsSingleTarget = false
            target => target is { IsAlive: true, IsPet: true },
            (card, target) => IsMyDollMinion(card, target) // 群體牌會對所有回傳 true 的目標生效
        ),
        "Morimens",
        nameof(AllDollMinions)
    );

    /// <summary>
    /// 只能指向我的一個 DollMinion (單體)
    /// </summary>
    public static readonly TargetType AnyDollMinion = CustomTargetTypeManager.Register(
        new LambdaTargetType(
            true, // IsSingleTarget = true
            target => target is { IsAlive: true, IsPet: true }, // 基本預覽判斷
            (card, target) => IsMyDollMinion(card, target) // 卡牌打出時的嚴格判斷
        ),
        "Morimens",
        nameof(AnyDollMinion)
    );

    /// <summary>
    /// 選擇我的一個 DollMinion (若沒有也可打出)
    /// </summary>
    public static readonly TargetType AnyDollMinionOrNone = CustomTargetTypeManager.Register(
        new AnyDollMinionOrNoneTargetType(),
        "Morimens",
        nameof(AnyDollMinionOrNone)
    );
}
