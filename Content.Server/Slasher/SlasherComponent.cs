using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Slasher;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SlasherComponent : Component
{
    public bool Weakened => WeakenedAccumulator > 0f;

    /// <summary>
    /// Tracking victims
    /// </summary>
    [DataField("victims")]
    public List<EntityUid> Victims = new();

    public int VictimsAllowed = 5;

    #region Visualizer
    [DataField("state")]
    public string State = "idle";
    [DataField("corporealState")]
    public string CorporealState = "active";
    #endregion

    /// <summary>
    /// The total amount of Essence the slasher has. Functions
    /// as health and is regenerated.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 Essence = 400;


    /// <summary>
    /// When we materialize, this is how long the slasher is weakened for - to prevent instant materialization and killing people out of nowhere
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("weakenedDuration")]
    public float WeakenedDuration = 10f;

    /// <summary>
    /// Have we just materialized and are in a temporary weakened state?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("weakenedAccumulator")]
    public float WeakenedAccumulator = 0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("bloodlustAccumulator")]
    public float BloodlustAccumulator = 0f;

    /// <summary>
    /// Maximum time the slasher can go without killing somebody before they die.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("bloodlustMaxAccumulator")]
    public float BloodlustMaxAccumulator = 600f;


    [DataField("materializeAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string MaterializeAction = "ActionMaterialize";

    [DataField("dematerializeAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string DematerializeAction = "ActionDematerialize";
}
