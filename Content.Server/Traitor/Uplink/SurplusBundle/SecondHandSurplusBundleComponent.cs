namespace Content.Server.Traitor.Uplink.SurplusBundle;

/// <summary>
/// Fills a crate with random second-hand uplink items up to a total TC budget.
/// Unlike <see cref="SurplusBundleComponent"/>, pulls from all listings with a
/// secondHandCategory regardless of what is available in any specific player store.
/// </summary>
[RegisterComponent]
public sealed partial class SecondHandSurplusBundleComponent : Component
{
    /// <summary>
    /// Total telecrystal budget to fill the crate with.
    /// Free (0 TC) items each count as 1 TC toward this budget.
    /// </summary>
    [DataField]
    public int TotalPrice = 20;
}
