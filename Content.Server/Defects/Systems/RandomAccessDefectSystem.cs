using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Defects.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Defects.Systems;

/// <summary>
/// When an access breaker with <see cref="RandomAccessDefectComponent"/> successfully breaks access,
/// there is a chance the reader ends up reprogrammed with random access levels instead of being cleared.
/// The charge is still consumed — the result is just unpredictable.
/// </summary>
public sealed class RandomAccessDefectSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Run after EmagSystem so the charge is already consumed and AccessLists already cleared.
        SubscribeLocalEvent<RandomAccessDefectComponent, AfterInteractEvent>(OnAfterInteract,
            after: new[] { typeof(EmagSystem) });
    }

    private void OnAfterInteract(Entity<RandomAccessDefectComponent> ent, ref AfterInteractEvent args)
    {
        // Only intercept successful emag interactions.
        if (!args.Handled || args.Target is not { } target)
            return;

        if (!_random.Prob(ent.Comp.RandomizeChance))
            return;

        // Check that an access reader was cleared on the target (AccessLists empty, original was not).
        if (!TryComp<AccessReaderComponent>(target, out var reader))
            return;

        if (reader.AccessLists.Count != 0)
            return;

        if (reader.AccessListsOriginal == null || reader.AccessListsOriginal.Count == 0)
            return;

        // Repopulate with a random set of access levels.
        var allLevels = _proto.EnumeratePrototypes<AccessLevelPrototype>()
            .Select(p => new ProtoId<AccessLevelPrototype>(p.ID))
            .ToList();

        var count = _random.Next(ent.Comp.MinAccessGroups, ent.Comp.MaxAccessGroups + 1);
        var picked = _random.GetItems(allLevels, Math.Min(count, allLevels.Count), allowDuplicates: false);

        reader.AccessLists.Add(new HashSet<ProtoId<AccessLevelPrototype>>(picked));
        Dirty(target, reader);

        _popup.PopupEntity(Loc.GetString("random-access-defect-triggered"), ent, args.User, PopupType.SmallCaution);
    }
}
