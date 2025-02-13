using Content.Server.Objectives;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Content.Shared.Slasher;

namespace Content.Server.Slasher;

public sealed class SlasherSystem: EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private float MaterialCooldown = 10.0f;
    private float EtherealCooldown = 60.0f;

    private bool isEtheral = false;

    public override void Initialize()
    {
        base.Initialize();

         SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnInit);

         SubscribeLocalEvent<SlasherComponent, SlasherMaterializeActionEvent>(OnTryMaterialize);
         SubscribeLocalEvent<SlasherComponent, SlasherDematerializeActionEvent>(OnTryDematerialize);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.WeakenedAccumulator > 0f)
            {
                comp.WeakenedAccumulator -= frameTime;

                // No longer weakened.
                if (comp.WeakenedAccumulator < 0f)
                {
                    comp.WeakenedAccumulator = 0f;
                    _movement.RefreshMovementSpeedModifiers(uid);
                }
            }
        }
    }

    private void OnInit(EntityUid uid, SlasherComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, component.MaterializeAction);
        _actions.AddAction(uid, component.DematerializeAction);
        _popup.PopupEntity("You... Must... KILL!", uid, PopupType.Large);
    }

    private void OnTryMaterialize(EntityUid uid, SlasherComponent component, SlasherMaterializeActionEvent args)
    {
        if (!isEtheral)
        {
            return;
        }
        // The player is in ghost form and can legally materialize - add check for solid objects like in revanent
        isEtheral = false;
        _popup.PopupEntity("You emerge from the shadows", uid, PopupType.Large);
        // set transparency layer thin
            // .Sprite.LayerSetState(0, component.CorporealState);
    }

    private void OnTryDematerialize(EntityUid uid, SlasherComponent component, SlasherDematerializeActionEvent args)
    {
        if (isEtheral)
        {
            return;
        }
        isEtheral = true;
        _popup.PopupEntity("You fade back into the shadows", uid, PopupType.Large);
        // set transparency layer thing
    }
}
