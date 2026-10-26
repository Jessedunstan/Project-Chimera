namespace Chimera.Core.Contracts;

using Chimera.Core.MathEngine;

/// <summary>
/// Universal continuous slider interface using strongly typed <see cref="SliderId"/> (ushort)
/// to eliminate GC allocations, string hashing, and dictionary lookup overhead in hot simulation loops.
/// </summary>
public interface ISliderModifiable
{
    float GetSliderValue(SliderId sliderId);
    void SetSliderValue(SliderId sliderId, float value);
}
