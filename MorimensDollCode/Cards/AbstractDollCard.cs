using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace MorimensDoll.Cards;

public abstract class AbstractDollCard(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true) : ModCardTemplate(baseCost, type, rarity, target, showInCardLibrary)
{
    public override CardAssetProfile AssetProfile
    {
        get
        {
            // 1. 定義這張卡片原本「應該」存在的路徑
            string targetPath = $"{Entry.ResPath}/images/cards/{GetType().Name}.png";

            // 2. 使用 Godot 的 API 檢查檔案是否存在
            if (!Godot.ResourceLoader.Exists(targetPath))
            {
                // 3. 如果找不到圖，用 MegaCrit 的日誌系統噴一個黃色警告，方便你開主控台看哪張卡沒圖
                Entry.Logger.Debug($"Missing card art for '{GetType().Name}'. Falling back to placeholder. (Expected path: {targetPath})");

                // 4. 自動導向你的 Missing 專用替代圖
                targetPath = $"{Entry.ResPath}/images/cards/missing.png";
            }

            // 使用 pragma 壓制 RITSU013 警告，避免檢查器誤判抽象類別
#pragma warning disable RITSU013
            return new CardAssetProfile(PortraitPath: targetPath);
#pragma warning restore RITSU013
        }
    }
}
