using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Minion;
using MorimensDoll.Anims;
using MorimensDoll.Minions;
using STS2RitsuLib.Scaffolding.Content;

namespace MorimensDoll.Minions;

public class DollMinion : ModMinionTemplate
{
    public override int MinInitialHp => 1; // 作为敌方方怪物生成时的血量，通常无需在意
    public override int MaxInitialHp => 1; // 作为敌方方怪物生成时的血量，通常无需在意

    public const decimal BASE_MAX_HP = 5m;
    public const decimal BASE_ATK = 1m;

    // 預設的基礎上限
    public const int BASE_FRONT_LIMIT = 2;

    private const string SceneRoot = $"{Entry.ResPath}/scenes/minions";

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"{SceneRoot}/Doll_minion.tscn"
    );

    // 召唤时执行的代码，通常用来设置血量、应用初始能力等，options 是在召唤随从时传入的参数
    // 注意使用 self 而非 this
    public override async Task OnSummon(Player owner, Creature self, MinionSummonOptions options)
    {

        if (options.MaxHp is decimal maxHp && maxHp > 0)
            await CreatureCmd.SetMaxHp(self, maxHp);
        else
            await CreatureCmd.SetMaxHp(self, BASE_MAX_HP);

        if (options.PrimaryStatAmount is decimal hp && hp > 0m)
            await CreatureCmd.SetCurrentHp(self, hp);
        else
            await CreatureCmd.SetCurrentHp(self, self.MaxHp);

        if (options.SecondaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), self, strength, owner.Creature, options.Source);
        else
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), self, BASE_ATK, owner.Creature, options.Source);
    }

    public override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Creature.Side || !participants.Contains(Creature))
            return;

        await DollMinionCmd.AttackRandomEnemy(choiceContext, this, null);
    }

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        return DollSpine.GetCreatureAnimator(controller);
    }
}
