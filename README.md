# Morimens

語言 / Languages：中文 | [English](README.en.md)

## 支援遊戲版本

Slay the Spire 2 beta-branch v0.107.0

## 依賴模組

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) ^0.4.13

- [MinionLib](https://github.com/FuYnAloft/MinionLib) ^0.5.1

## 如何安装

將Morimens.dll, Morimens.pck, Morimens.json連同依賴模組的檔案一起放入殺戮尖塔2資料夾底下的mods資料夾。

建議資料夾結構：

```text
.../steamapps/common/Slay the Spire 2/mods/
├───MinionLib/
│       MinionLib.dll
│       MinionLib.json
│       MinionLib.pck
├───Morimens/
│       Morimens.dll
│       Morimens.json
│       Morimens.pck
└───STS2-RitsuLib/
    │   compat-target.txt
    │   mod_manifest.json
    │   STS2-RitsuLib.dll
    │   STS2-RitsuLib.pdb
    │   STS2-RitsuLib.xml
    └───viewer/
```

## 模組内容

| 项 | 值 |
|---|---|
| 角色 | 1個：朵爾 |
| 卡牌 | 27張 |
| 遺物 | 0 |
| 藥水 | 0 |

## 開發依賴

- [RitsuLibModTemplate](https://github.com/alkaid616/RitsuLibModTemplate)：模組初始模板。

- [STS2_FModProject_Minimal](https://github.com/BAKAOLC/STS2_FModProject_Minimal)：音頻fmod專案初始模板。

## 學習資源

- [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib)：Slay the Spire 2 Mod 的共享框架库，本模板基于它提供内容注册、角色脚手架和 Godot 资源接入能力。
- [RitsuLib 文档地址](https://github.com/GlitchedReme/SlayTheSpire2ModdingTutorials/tree/master/RitsuLib)：按文件阅读教程和示例。
- [Slay the Spire 2 Modding Tutorials 网页版](https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/index.html)：完整教程站点。
- RitsuLibModTemplate Wiki（以 Rider 为主线）：[中文首页](https://github.com/alkaid616/RitsuLibModTemplate/wiki) | [English Home](https://github.com/alkaid616/RitsuLibModTemplate/wiki/Home-EN)。

## 構建

| 命令 | 行为 |
|---|---|
| `dotnet build .\Morimens.csproj` | 完整构建：编译 + `CopyMod` + `ExportPCK` |
| `... /p:RunPckExport=false` | 跳过 PCK 导出（不需要 `GodotExe`） |
| `... /p:CopyModOnBuild=false` | 跳过复制到游戏 mods 目录（产物只留在 `bin/`） |
| `... /p:RunPckExport=false /p:CopyModOnBuild=false` | 仅验证 C# 编译 |

完整构建会在 `Build` 之后运行两个 MSBuild target：

- **`CopyMod`**：复制 dll 和 manifest 到游戏的 `mods/Morimens` 目录。
- **`ExportPCK`**：调用 `GodotExe` 导出 pck 到同一个 Mod 目录。

> `RitsuLibDeployDir` 只控制 RitsuLib 框架自身的部署位置；当前 Mod 的 dll、manifest 和 pck 由 `ModOutputDir` 控制（默认 `$(Sts2Dir)/mods/$(MSBuildProjectName)`）。

## 資料夾結構

```text
Morimens/
├── MorimensCode/   # C# 原始碼
├── Morimens/       # Godot 資源：影像、文本、音頻和場景
├── Morimens.csproj
├── Morimens.json   # Mod manifest
├── project.godot
└── local.props.template
```

`res://Morimens/...` 是 Godot/PCK 内的资源路径，对应仓库里的 `Morimens/` 资源目录。
