namespace Chimera.Core.Ecology.Behavior;

/// <summary>
/// Baseline relationship classifications extracted from Rain World's CreatureTemplate.Relationship.Type.
/// </summary>
public enum RelationshipType : byte
{
    Ignores = 0,
    Eats = 1,
    Attacks = 2,
    Afraid = 3,
    StayOutOfWay = 4,
    Antagonizes = 5,
    PlaysWith = 6,
    Pack = 7,
    SocialDependent = 8
}
