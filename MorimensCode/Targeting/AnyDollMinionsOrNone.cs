using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Targeting;
using Morimens.Minions;

namespace Morimens.Targeting;

public class AnyDollMinionOrNoneTargetType : CustomTargetType
{
    // 核心魔術：讓 IsSingleTarget 變成動態的
    public override bool IsSingleTarget
    {
        get
        {
            // 獲取當前戰鬥中的本地玩家實例
            Player? localPlayer = LocalContext.GetMe(CombatManager.Instance._state);

            if (localPlayer == null) return false;

            // 檢查玩家當前的戰鬥狀態中，是否持有任何你的 DollMinion
            // 如果有，回傳 true (啟用單體拉線)；如果沒有，回傳 false (變成無目標卡，直接釋放打出)
            return localPlayer.PlayerCombatState?.Pets?
                .Any(p => p.Monster is DollMinion && p.IsAlive) ?? false;
        }
    }

    // 當有隨從（IsSingleTarget = true）時，拉出的箭頭只有指向你的 DollMinion 才合法
    public override bool IsValidTarget(CardModel card, Creature target)
    {
        return target is { IsAlive: true, IsPet: true } &&
               target.PetOwner == card.Owner &&
               target.Monster is DollMinion;
    }

    // 用於滑鼠懸停在卡牌上時的黃框預覽高亮
    public override bool IsValidTarget(Creature target)
    {
        // 當沒有隨從時（IsSingleTarget = false），此處會自然回傳 false，不會錯誤高亮任何單位
        return target is { IsAlive: true, IsPet: true } && target.Monster is DollMinion;
    }
}
