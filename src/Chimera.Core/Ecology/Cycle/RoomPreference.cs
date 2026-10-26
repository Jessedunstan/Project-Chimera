namespace Chimera.Core.Ecology.Cycle;

/// <summary>
/// Discrete spatial preference weights extracted from Rain World's room property tags.
/// </summary>
public enum RoomPreference : byte
{
    Forbidden = 0,
    Avoid = 1,
    Neutral = 2,
    Like = 3,
    Stay = 4
}

public static class RoomPreferenceExtensions
{
    public static float ToWeight(this RoomPreference preference) => preference switch
    {
        RoomPreference.Forbidden => 0.0f,
        RoomPreference.Avoid => 0.1f,
        RoomPreference.Neutral => 1.0f,
        RoomPreference.Like => 3.0f,
        RoomPreference.Stay => 10.0f,
        _ => 1.0f
    };
}
