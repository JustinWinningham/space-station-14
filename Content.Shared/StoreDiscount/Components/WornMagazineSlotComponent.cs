using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.StoreDiscount.Components;

/// <summary>
/// Replaces the starting item in a gun's magazine slot with a worn (partially filled) variant.
/// Applied at component startup, before ItemSlotsSystem spawns the starting item at map init.
/// </summary>
[RegisterComponent]
public sealed partial class WornMagazineSlotComponent : Component
{
    /// <summary>
    /// The worn magazine entity prototype to use as the starting item.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId WornMagazineProto;

    /// <summary>
    /// The slot ID to replace the starting item in. Defaults to the standard magazine slot.
    /// </summary>
    [DataField]
    public string SlotId = "gun_magazine";
}
