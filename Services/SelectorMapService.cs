using MauiSender.Models;

namespace MauiSender.Services
{
    public sealed class SelectorMapService
    {
        public static SelectorMapService Current { get; } = new SelectorMapService();

        public SelectorMap Map { get; private set; } = SelectorMap.LoadFromEmbedded();

        public void Replace(SelectorMap map) => Map = map ?? new SelectorMap();
    }
}
