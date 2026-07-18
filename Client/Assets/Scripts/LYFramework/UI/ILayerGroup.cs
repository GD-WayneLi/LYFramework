using System;

namespace LYFramework.UI
{
    public interface ILayerGroup
    {
        int Layer { get; }
        int StartDepth { get; }
    }
}