using System.Linq;
using Content.Server.Storage.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Traitor.Uplink.SurplusBundle;

/// <summary>
/// Fills a <see cref="SecondHandSurplusBundleComponent"/> crate with random second-hand
/// items at map init. Draws from the full pool of second-hand listings (any listing with
/// a secondHandCategory set), regardless of what a specific player's uplink has available.
/// </summary>
public sealed class SecondHandSurplusBundleSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SecondHandSurplusBundleComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<SecondHandSurplusBundleComponent> ent, ref MapInitEvent args)
    {
        var coords = Transform(ent).Coordinates;

        foreach (var listing in GetRandomContent(ent.Comp))
        {
            if (listing.ProductEntity is not { } proto)
                continue;

            var spawned = Spawn(proto, coords);
            _entityStorage.Insert(spawned, ent);
        }
    }

    private List<ListingData> GetRandomContent(SecondHandSurplusBundleComponent comp)
    {
        var ret = new List<ListingData>();

        // All listings that belong to any secondHandCategory, sorted cheapest-first.
        // Free items (0 TC) are treated as 1 TC so the budget loop always terminates.
        var listings = _proto.EnumeratePrototypes<ListingPrototype>()
            .Where(l => l.SecondHandCategory != null && l.ProductEntity != null)
            .Cast<ListingData>()
            .OrderBy(l => EffectiveCost(l))
            .ToList();

        if (listings.Count == 0)
            return ret;

        var totalCost = FixedPoint2.Zero;
        var index = 0;

        while (totalCost < comp.TotalPrice)
        {
            var remainingBudget = comp.TotalPrice - totalCost;

            // Advance past items whose effective cost exceeds the remaining budget.
            while (EffectiveCost(listings[index]) > remainingBudget)
            {
                index++;
                if (index >= listings.Count)
                    return ret;
            }

            var randomIndex = _random.Next(index, listings.Count);
            var picked = listings[randomIndex];
            ret.Add(picked);
            totalCost += EffectiveCost(picked);
        }

        return ret;
    }

    /// <summary>
    /// Returns the effective cost of a listing for budget purposes.
    /// Free items count as 1 TC so the budget loop always makes progress.
    /// </summary>
    private static FixedPoint2 EffectiveCost(ListingData listing)
    {
        var raw = listing.Cost.Values.Sum();
        return raw > 0 ? raw : FixedPoint2.New(1);
    }
}
