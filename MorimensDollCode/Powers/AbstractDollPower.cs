using STS2RitsuLib.Scaffolding.Content;

namespace MorimensDoll.Powers;

public abstract class AbstractDollPower : ModPowerTemplate
{
    // 原版大图通常 256x256，小图通常 64x64。
    public override PowerAssetProfile AssetProfile
    {
        get
        {
            // 1. 定義原本「應該」存在的路徑
            string targetPath = $"{Entry.ResPath}/images/powers/{GetType().Name}.png";

            // 2. 使用 Godot 的 API 檢查檔案是否存在
            if (!Godot.ResourceLoader.Exists(targetPath))
            {
                // 3. 如果找不到圖，用 MegaCrit 的日誌系統噴一個黃色警告，方便你開主控台看哪張卡沒圖
                Entry.Logger.Debug($"Missing power art for '{GetType().Name}'. Falling back to placeholder. (Expected path: {targetPath})");

                // 4. 自動導向你的 Missing 專用替代圖
                targetPath = $"{Entry.ResPath}/images/powers/missing.png";
            }

            // 使用 pragma 壓制 RITSU013 警告，避免檢查器誤判抽象類別
#pragma warning disable RITSU013
            return new PowerAssetProfile(IconPath: targetPath, BigIconPath: targetPath);
#pragma warning restore RITSU013
        }
    }
}
