using Content.Shared.Containers.ItemSlots;
using Content.Shared.StoreDiscount.Components;
using Robust.Shared.GameObjects;

namespace Content.Shared.StoreDiscount.Systems;

/// <summary>
/// Replaces the starting item in a gun's magazine slot with a worn (partially filled) variant.
/// Runs during ComponentStartup so the change is in place before ItemSlotsSystem spawns items at MapInit.
/// </summary>
public sealed class WornMagazineSlotSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WornMagazineSlotComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<WornMagazineSlotComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<ItemSlotsComponent>(ent, out var slots))
            return;

        if (!slots.Slots.TryGetValue(ent.Comp.SlotId, out var slot))
            return;

        slot.StartingItem = ent.Comp.WornMagazineProto;
    }
}
