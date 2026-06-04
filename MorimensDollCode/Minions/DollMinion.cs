using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MinionLib.Minion;
using MinionLib.RitsuAdapters;
using MorimensDoll.Anims;
using MorimensDoll.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace MorimensDoll.Minions;

public class DollMinion : ModMinionTemplate
{
    public override int MinInitialHp => 1; // 作为敌方方怪物生成时的血量，通常无需在意
    public override int MaxInitialHp => 1; // 作为敌方方怪物生成时的血量，通常无需在意

    public const decimal MAX_HP = 4m;

    // 預設的基礎上限
    public const int BASE_FRONT_LIMIT = 2;

    private const string SceneRoot = $"{Entry.ResPath}/scenes/minions";

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"{SceneRoot}/Doll_minion.tscn"
    );

    // 召唤时执行的代码，通常用来设置血量、应用初始能力等，options 是在召唤随从时传入的参数
    // 注意使用 self 而非 this
    public override async Task OnSummon(PlayerChoiceContext choiceContext, Player owner, Creature self, MinionSummonOptions options)
    {
        await CreatureCmd.SetMaxHp(self, MAX_HP);
        if (options.MaxHp is decimal maxHp)
        {
            if (maxHp > MAX_HP)
                await CreatureCmd.SetMaxHp(self, maxHp);
            await CreatureCmd.SetCurrentHp(self, maxHp); // 设置血量
        }

        if (options.PrimaryStatAmount is decimal strength && strength > 0m)
            await PowerCmd.Apply<StrengthPower>(choiceContext, self, strength, owner.Creature, options.Source); // 根据传入的参数设置力量
    }

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        return DollSpine.GetCreatureAnimator(controller);
    }
}
