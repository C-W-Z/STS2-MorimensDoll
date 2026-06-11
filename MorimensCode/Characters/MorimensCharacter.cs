using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Morimens.ExEnergy;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Scaffolding.Characters;

namespace Morimens.Characters;

public abstract class MorimensCharacter<TCardPool, TRelicPool, TPotionPool> : ModCharacterTemplate<TCardPool, TRelicPool, TPotionPool>, ISecondaryResourceHookListener
    where TCardPool : CardPoolModel
    where TRelicPool : RelicPoolModel
    where TPotionPool : PotionPoolModel
{
    public virtual int BaseAliemus => 100;

    public decimal ModifyMaxSecondaryResource(SecondaryResourceMaxContext context, decimal amount)
    {
        if (context.Definition.Id == ExEnergyManager.AliemusId)
            return amount + BaseAliemus - (ExEnergyManager.AliemusDefinition.BaseMaxAmount ?? 0);
        return amount;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Entry.Logger.Debug($"AfterDamageReceived: {target.Name}");
        // 获得 1 点狂氣
        // TODO: 会经过 Gain Hook 修正，要改掉
        if (target.Player != null)
            await SecondaryResourceCmd.Gain(target.Player, ExEnergyManager.AliemusId, 1, this);
    }
}
