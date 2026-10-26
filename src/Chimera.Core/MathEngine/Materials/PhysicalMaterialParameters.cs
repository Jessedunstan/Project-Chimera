namespace Chimera.Core.MathEngine.Materials;

/// <summary>
/// Continuous physical properties representing a voxel material or constructed object.
/// Everything is parameterized via continuous floats. Zero heap allocations.
/// </summary>
public readonly struct PhysicalMaterialParameters
{
    public float Density { get; }                    // kg / m^3 (e.g. Aerogel ~10, Water 1000, Iron 7874, Gold 19320)
    public float Rigidity { get; }                   // normalized [0..1] resistance to elastic deflection
    public float Brittleness { get; }                // normalized [0..1] likelihood of sudden crack propagation vs bending
    public float Malleability { get; }               // normalized [0..1] capacity for plastic yield deformation without cracking
    public float ThermalConductivity { get; }        // normalized [0..1] rate of heat transfer (scaled to W/(m*K))
    public float SpecificHeatCapacity { get; }       // J / (kg * K) thermal mass
    public float MeltingPoint { get; }               // Kelvin (phase change Solid -> Liquid)
    public float BoilingPoint { get; }               // Kelvin (phase change Liquid -> Gas)
    public float FrictionCoefficient { get; }        // normalized [0..1] surface shear friction
    public float TensileYield { get; }               // MPa critical stress threshold before yielding

    public PhysicalMaterialParameters(
        float density,
        float rigidity,
        float brittleness,
        float malleability,
        float thermalConductivity,
        float specificHeatCapacity,
        float meltingPoint,
        float boilingPoint,
        float frictionCoefficient = 0.6f,
        float tensileYield = 250.0f)
    {
        Density = MathF.Max(0.01f, density);
        Rigidity = Math.Clamp(rigidity, 0.0f, 1.0f);
        Brittleness = Math.Clamp(brittleness, 0.0f, 1.0f);
        Malleability = Math.Clamp(malleability, 0.0f, 1.0f);
        ThermalConductivity = Math.Clamp(thermalConductivity, 0.0f, 1.0f);
        SpecificHeatCapacity = MathF.Max(1.0f, specificHeatCapacity);
        MeltingPoint = MathF.Max(0.0f, meltingPoint);
        BoilingPoint = MathF.Max(MeltingPoint, boilingPoint);
        FrictionCoefficient = Math.Clamp(frictionCoefficient, 0.0f, 1.0f);
        TensileYield = MathF.Max(0.001f, tensileYield);
    }

    /// <summary>
    /// Constructs parameters directly from a ContinuousParameterBank.
    /// </summary>
    public static PhysicalMaterialParameters FromParameterBank(ContinuousParameterBank bank)
    {
        return new PhysicalMaterialParameters(
            bank.GetSliderValue(SliderId.MaterialDensity),
            bank.GetSliderValue(SliderId.MaterialRigidity),
            bank.GetSliderValue(SliderId.MaterialBrittleness),
            bank.GetSliderValue(SliderId.MaterialMalleability),
            bank.GetSliderValue(SliderId.MaterialThermalConductivity),
            bank.GetSliderValue(SliderId.MaterialSpecificHeatCapacity),
            bank.GetSliderValue(SliderId.MaterialMeltingPoint),
            bank.GetSliderValue(SliderId.MaterialBoilingPoint),
            bank.GetSliderValue(SliderId.MaterialFrictionCoefficient),
            bank.GetSliderValue(SliderId.MaterialTensileYield)
        );
    }
}
