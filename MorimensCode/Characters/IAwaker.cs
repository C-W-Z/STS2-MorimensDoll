using MegaCrit.Sts2.Core.Entities.Players;

namespace Morimens.Characters;

public interface IAwaker
{
    int BaseAliemus { get; }
    int BaseKeyflare { get; }
    string ExaltTitle { get; }
    string ExaltDescription { get; }
    Task Exalt();
    string OverExaltTitle { get; }
    string OverExaltDescription { get; }
    Task OverExalt();
}
