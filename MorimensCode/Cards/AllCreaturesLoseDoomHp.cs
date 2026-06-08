using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MinionLib.Targeting;
using Morimens.Anims;
using Morimens.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Morimens.Cards;

[RegisterCard(typeof(DollCardPool))]
public sealed class AllCreaturesLoseDoomHp() : AbstractDollCard(2, CardType.Attack, CardRarity.Rare, MinionTargetTypes.AllCreatures)
{
    protected override HashSet<CardTag> CanonicalTags => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        await CreatureCmd.TriggerAnim(Owner.Creature, DollSpine.State.ExSkill, DollSpine.ExSkillAnimDelay);

        // ToList()幫怪物列表拍快照，防止遍歷時崩潰
        var rawTargets = CombatState.Allies.Concat(CombatState.Enemies).ToList();
        // 重要排序：讓玩家（Owner）排在最前面，然後是Minion，最後才是敵人
        var sortedTargets = rawTargets
            .Where(e => e != null && e.IsHittable)
            .OrderByDescending(e => e.IsPlayer) // 讓 IsPlayer == true 的排在最前面
            .ToList();
        // 放棄 Task.WhenAll，改用依序 await 確保 Patch 的數學計算正確
        foreach (var e in sortedTargets)
        {
            // 防禦性檢查，因為前面的受擊可能已經導致後面的怪暴斃了
            if (e == null || !e.IsHittable) continue;
            int amount = e.GetPowerAmount<DoomPower>();
            if (amount > 0)
                await CreatureCmd.Damage(choiceContext, e, amount, ValueProp.Unblockable, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
