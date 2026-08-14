using System;

namespace LYUnity.UI
{
    public interface ILayerGroup
    {
        int Layer { get; }
        int StartDepth { get; }
    }
}