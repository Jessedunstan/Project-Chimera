namespace Chimera.Core.Ecology.FoodWeb;

/// <summary>
/// Ecological trophic hierarchy levels in Project Chimera.
/// </summary>
public enum TrophicLevel : byte
{
    /// <summary>Primary producers: vegetation, fungus, moss, and detritus layers.</summary>
    PrimaryProducer = 0,

    /// <summary>Primary consumers: small insects, batflies, small herbivores, and grazers.</summary>
    PrimaryConsumer = 1,

    /// <summary>Secondary consumers: mid-tier predators (e.g. green/pink lizards, small carnivores).</summary>
    SecondaryConsumer = 2,

    /// <summary>Apex predators: dominant carnivores (e.g. red lizards, vultures, leviathans).</summary>
    ApexPredator = 3,

    /// <summary>Scavengers and decomposers: opportunistic eaters, carrion scavengers, detritivores.</summary>
    Scavenger = 4
}
