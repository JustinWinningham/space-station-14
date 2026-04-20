using Robust.Shared.GameStates;

namespace Content.Shared.Defects.Components;

/// <summary>
/// On an access breaker (emag), gives a per-use chance that the targeted access reader
/// is reprogrammed with a random department's access instead of being broken open.
/// The charge is still consumed; the result is just unpredictable.
/// </summary>
[RegisterComponent]
public sealed partial class RandomAccessDefectComponent : DefectComponent
{
    public RandomAccessDefectComponent()
    {
        Prob = 0.25f;
        DefectLabel = "faulty authentication chip";
    }

    /// <summary>
    /// Per-use probability that the access reader gets randomized rather than cleared.
    /// </summary>
    [DataField]
    public float RandomizeChance = 0.5f;

    /// <summary>
    /// Minimum number of random access levels to assign to the reader.
    /// </summary>
    [DataField]
    public int MinAccessGroups = 1;

    /// <summary>
    /// Maximum number of random access levels to assign to the reader.
    /// </summary>
    [DataField]
    public int MaxAccessGroups = 2;
}
