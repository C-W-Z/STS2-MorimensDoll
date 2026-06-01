using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Commands;
using MinionLib.Minion;
using MorimensDoll.Characters;
using MorimensDoll.Minions;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.Cards;

[RegisterCard(typeof(DollCardPool))]
[RegisterCharacterStarterCard(typeof(Doll), 1)]
public sealed class SpareBody() : AbstractDollCard(0, CardType.Skill, CardRarity.Basic, TargetType.None)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await MinionCmd.AddMinion<DollMinion>(choiceContext, Owner, new MinionSummonOptions(
            MaxHp: 4m,                              // 血量
            PrimaryStatAmount: 1m,                  // 主要参数（具体内容在随从的 OnSummon 里定义），还有次要参数等可以按需传入
            Source: this,                           // 召唤来源（通常是这张牌）
            Position: MinionPosition.Front));       // 站位（见后文，默认是前排）
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
