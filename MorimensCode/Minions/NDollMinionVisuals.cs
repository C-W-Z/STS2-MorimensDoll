using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Morimens.Minions;

// 必須加上 partial 關鍵字，且必須繼承自 NCreatureVisuals
public partial class NDollMinionVisuals : NCreatureVisuals
{
    // 這裡目前不需要寫任何程式碼，空著即可
    // 它的存在是為了讓 Godot 實例化時，.NET 性能認出它是 NCreatureVisuals 型別
}
