using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Morimens.Anims;
using Morimens.Cards;
using Morimens.ExEnergy;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace Morimens.Characters;

[RegisterCharacter]
public sealed class Doll : Awaker<DollCardPool, DollRelicPool, DollPotionPool>
{
    public static readonly Color ThemeColor = new(0.42f, 0.65f, 0.72f);

    private const string SceneRoot = $"{Entry.ResPath}/scenes/characters";
    private const string ImageRoot = $"{Entry.ResPath}/images/characters";
    private const string CharacterScenePath = $"{SceneRoot}/Doll_character.tscn";
    private const string EnergyCounterScenePath = $"{SceneRoot}/Doll_energy_counter.tscn";
    private const string MerchantScenePath = $"{SceneRoot}/Doll_merchant.tscn";
    private const string RestSiteScenePath = $"{SceneRoot}/Doll_rest_site.tscn";
    private const string CharacterSelectBgScenePath = $"{SceneRoot}/Doll_character_select_bg.tscn";

    // 角色名称颜色。
    public override Color NameColor => ThemeColor;
    // 能量图标轮廓颜色。
    public override Color EnergyLabelOutlineColor => new(0.08f, 0.18f, 0.24f);
    // 地图绘制颜色。
    public override Color MapDrawingColor => ThemeColor;

    // 人物性别（男女中立）。
    public override CharacterGender Gender => CharacterGender.Neutral;

    // 初始血量和金币。
    public override int StartingHp => 58;
    public override int StartingGold => 99;

    // CharacterAssetProfile 按类别拆分。你只写需要替换的部分，其他字段会保留回退。
    // AssetProfile 只指定模板自带的静态占位资源；没有复制的音频、拖尾、转场等资源继续从占位角色回退。
    public override CharacterAssetProfile AssetProfile => new(
        Scenes: new CharacterSceneAssetSet(
            // 人物模型 tscn 路径。
            VisualsPath: CharacterScenePath,
            // 能量表盘 tscn 路径。
            EnergyCounterPath: EnergyCounterScenePath,
            // 商店人物场景。
            MerchantAnimPath: MerchantScenePath,
            // 篝火休息场景。
            RestSiteAnimPath: RestSiteScenePath),
        Ui: new CharacterUiAssetSet(
            // 人物头像路径。
            IconTexturePath: $"{ImageRoot}/Doll_character_icon.png",
            // 人物头像轮廓。
            IconOutlineTexturePath: $"{ImageRoot}/Doll_character_icon_outline.png",
            // 人物选择背景。
            CharacterSelectBgPath: CharacterSelectBgScenePath,
            // 人物选择图标。
            CharacterSelectIconPath: $"{ImageRoot}/Doll_character_select.png",
            // 人物选择图标-锁定状态。
            CharacterSelectLockedIconPath: $"{ImageRoot}/Doll_character_select_locked.png",
            // 地图上的角色标记图标、表情轮盘上的角色头像。
            MapMarkerPath: $"{ImageRoot}/Doll_map_marker.png"),
        Audio: new CharacterAudioAssetSet(
            AttackSfx: "event:/Morimens/sfx/Doll/Attack",
            CastSfx: "event:/Morimens/sfx/Doll/Cast",
            DeathSfx: "event:/Morimens/sfx/Doll/Death"
        ));

    // 某个字段没写时，RitsuLib 会从占位角色配置里补齐。
    public override string? PlaceholderCharacterId => "necrobinder";
    // 如果你的人物不需要时间线小故事，加上这句。
    public override bool RequiresEpochAndTimeline => false;
    // 攻击和施法动画延迟，以对齐动画。静态占位资源不需要延迟。
    public override float AttackAnimDelay => DollSpine.AttackAnimDelay;
    public override float CastAnimDelay => DollSpine.CastAnimDelay;

    // 让 RitsuLib 把普通 Godot 场景转换成游戏需要的 NCreatureVisuals。
    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        return RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            CharacterScenePath);
    }

    // 攻击建筑师的攻击特效列表。
    public override List<string> GetArchitectAttackVfx()
    {
        return
        [
            "vfx/vfx_attack_blunt",
            "vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash",
            "vfx/vfx_bloody_impact",
            "vfx/vfx_rock_shatter"
        ];
    }

    protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller)
    {
        return DollSpine.GetCreatureAnimator(controller);
    }

    public override string ExaltTitle => LocManager.Instance.GetTable("characters").GetRawText("MORIMENS_CHARACTER_DOLL.EXALT.title");

    public override string ExaltDescription => GetExaltDescriptionText();

    public override string SuperExaltTitle => LocManager.Instance.GetTable("characters").GetRawText("MORIMENS_CHARACTER_DOLL.SUPER_EXALT.title");

    public override string SuperExaltDescription => GetExaltDescriptionText();

    public override async Task Exalt(Player player)
    {
        ArgumentNullException.ThrowIfNull(CombatManager.Instance._state);
        await CreatureCmd.TriggerAnim(player.Creature, DollSpine.State.ExSkill, DollSpine.ExSkillAnimDelay);
        // 驅散友方易傷狀態，全體友方回10血，全體友方+20狂
        foreach (var ally in CombatManager.Instance._state.Allies)
            await PowerCmd.Remove<VulnerablePower>(ally);
        // foreach (var ally in CombatManager.Instance._state.Allies)
        //     await CreatureCmd.Heal(ally, _exaltSkill.DynamicVars.Damage.BaseValue);
        foreach (var ally in CombatManager.Instance._state.Players)
            if (!LocalContext.IsMe(ally))
                await SecondaryResourceCmd.Gain(ally, ExEnergyManager.AliemusId, 20, this);

        // 🔴 實戰關鍵：直接抓取這張衍生牌被 Power（如力量、增傷等）完美加成後的 PreviewValue 作為實際傷害！
        var card = GetExaltCard();

        await DamageCmd.Attack(card.DynamicVars.Damage.BaseValue)
            .FromCard(card)
            .TargetingRandomOpponents(CombatManager.Instance._state)
            .Execute(null);
    }

    public override async Task SuperExalt(Player player) { }


    /// <summary>
    /// 核心輔助方法：獲取一張綁定了當前戰鬥狀態、且完美跑完 Power 加成邏輯的虛擬卡牌
    /// </summary>
    private CardModel GetExaltCard(Creature? target = null)
    {
        var card = ModelDb.Get<DollExalt>().ToMutable();
        card.UpgradePreviewType = CardUpgradePreviewType.Combat; // Important!
        var combatState = CombatManager.Instance._state;

        if (combatState != null)
        {
            // 🔴 核心修正：必須把擁有者、戰鬥狀態、戰役狀態全部餵給卡牌
            card.Owner = LocalContext.GetMe(combatState);
            // card.CombatState = combatState;
            // card.RunState = combatState.RunState;

            // 🔴 呼叫原版管線：先清空預覽，再觸發 Global Hooks 計算出考慮 Buff 後的 PreviewValue
            card.DynamicVars.ClearPreview();
            card.UpdateDynamicVarPreview(CardPreviewMode.Normal, target, card.DynamicVars);
        }
        return card;
    }

    private string GetExaltDescriptionText()
    {
        var card = GetExaltCard();
        LocString description = new("characters", "MORIMENS_CHARACTER_DOLL.EXALT.description");

        // 🔴 完美複用現有的 AddTo 接口！直接把這張牌已經動態計算好的變數組注入 LocString
        card.DynamicVars.AddTo(description);

        description.Add("InCombat", CombatManager.Instance.IsInProgress);
        return description.GetFormattedText();
    }
}
