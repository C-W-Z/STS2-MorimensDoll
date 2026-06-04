using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MinionLib.Commands;
using MinionLib.Minion;
using MorimensDoll.Minions;

namespace MorimensDoll.Minion;

public static class DollMinionCmd
{
    public static async Task Summon(PlayerChoiceContext choiceContext, Player player, CardModel? source, decimal hp = DollMinion.MAX_HP, decimal atk = 1m)
    {
        await MinionCmd.AddMinion<DollMinion>(choiceContext, player, new MinionSummonOptions(
            MaxHp: hp,                  // 血量
            PrimaryStatAmount: atk,     // 力量
            Source: source,             // 召喚來源（牌）
            Position: MinionPosition.Front)); // 站位但不重要因為 DollMinionLayout 會自動調整
    }

    public static async Task SummonHalfHP(PlayerChoiceContext choiceContext, Player player, CardModel source)
    {
        await Summon(choiceContext, player, source, DollMinion.MAX_HP / 2, 1m);
    }
}
