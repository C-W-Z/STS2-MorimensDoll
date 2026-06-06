
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MorimensDoll.CardTags;

[RegisterOwnedCardTag(nameof(Heal))]
[RegisterOwnedCardTag(nameof(MinionCmd))]
public static class DollCardTag
{
    public static readonly CardTag Heal = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Heal)).GetModCardTag();
    public static readonly CardTag MinionCmd = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(MinionCmd)).GetModCardTag();
}
