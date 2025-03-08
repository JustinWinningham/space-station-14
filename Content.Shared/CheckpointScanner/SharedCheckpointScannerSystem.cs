using System.Linq;
using System.Numerics;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Contraband;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Roles;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared.CheckpointScanner;





public sealed class SharedCheckpointScannerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedIdCardSystem _id = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CheckpointScannerComponent, ComponentShutdown>(
            OnShutdown); // Material reclaimer component should be our fixture component for detecting stuff
        SubscribeLocalEvent<CheckpointScannerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CheckpointScannerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CheckpointScannerComponent, StartCollideEvent>(OnCollide);
    }

    private void OnMapInit(EntityUid uid, CheckpointScannerComponent component, MapInitEvent args)
    {
        // stub
    }

    private void OnShutdown(EntityUid uid, CheckpointScannerComponent component, ComponentShutdown args)
    {
        _audio.Stop(component.Stream);
    }

    private void OnExamined(EntityUid uid, CheckpointScannerComponent component, ExaminedEvent args)
    {
        ++component.TimesExamined;
        args.PushMarkup(Loc.GetString("checkpoint-scanner-count-items",
            ("items", component.ItemsDisabled),
            ("timesExamined", component.TimesExamined)));
    }

    // private void OnEmagged(EntityUid uid, CheckpointScannerComponent component, ref GotEmaggedEvent args)
    // {
    //     // Kill the emagger... maybe.
    // }


    private void OnCollide(EntityUid uid, CheckpointScannerComponent component, ref StartCollideEvent args)
    {
        // Check if the scanner is powered
        if (!_powerReceiver.IsPowered(uid))
        {
            return;
        }

        var alarm = false;

        if (component.ClearSound == null || component.AlarmSound == null)
        {
            Logger.Warning(
                $"MaterialReclaimerComponent on entity {uid} has either no clear sound or alarm sound defined.");
            return;
        }

        var otherEntity = args.OtherEntity; // Colliding entity

        // The entity hitting the scanner isn't a human.
        if (!HasComp<HumanoidAppearanceComponent>(otherEntity))
            TryHandleEntityContra(uid, component, otherEntity, ref alarm);
        else // At this point we know that the entity hitting the scanner is a humanoid - so we can treat them like a player and scan their inventory - still have to test for rats n other things that A) dont have an inventory and B) dont have an ID slot
            TryHandleHumanContra(uid, component, otherEntity, ref alarm);

        if (alarm)
        {
            _audio.PlayPredicted(component.AlarmSound, uid, otherEntity);
            ElectrocuteEntity(otherEntity, component);
        }
        else
        {
            _audio.PlayPredicted(component.ClearSound, uid, otherEntity);
        }
    }

    private void TryHandleHumanContra(EntityUid uid,
        CheckpointScannerComponent component,
        EntityUid entity,
        ref bool alarm)
    {
        if (!HasComp<HumanoidAppearanceComponent>(entity))
            return;

        List<ProtoId<DepartmentPrototype>> departments = new();
        var jobId = "";
        if (_id.TryFindIdCard(entity, out var id))
        {
            departments = id.Comp.JobDepartments;
            if (id.Comp.LocalizedJobTitle is not null)
            {
                jobId = id.Comp.LocalizedJobTitle;
            }



            if (HasCaptainAccess(id) && component.AllowCptnAccess)
            {
                // it's the captain. they can have basically anything on evac. Not sure if this protects against title changes in an ID computer
                return;
            }
        }

        foreach (var ent in GetAllContainedEntities(entity))
        {
            if (TryComp<ContrabandComponent>(ent, out var contraComp))
            {
                var jobs = contraComp.AllowedJobs.Select(p => _proto.Index(p).LocalizedName).ToArray();

                // Take our previous list of valid departments, and check it against the piece of contra we are looking at - then set the alarm if we don't find any matches between the item and the IDs allowed departments
                if (!departments.Intersect(contraComp.AllowedDepartments).Any()
                    && !jobs.Contains(jobId))
                {
                    alarm = true;
                    break;
                }
            }
        }
    }

    private void TryHandleEntityContra(EntityUid scanner,
        CheckpointScannerComponent component,
        EntityUid entity,
        ref bool alarm)
    {
        // save some time - if the container itself is contra, dont bother making all the checks on its contents and reject it right away
        if (!HasComp<ContrabandComponent>(entity))
        {
            foreach (var ent in GetAllContainedEntities(entity))
            {
                if (!HasComp<ContrabandComponent>(ent))
                    continue;
                alarm = true; // set the flag to true, so we know to zappy zappy, quantity doesnt matter
                // # todo: Here is the place we want contraband to be removed / dropped / moved to scanner inventory / whatever
                break;
            }
        }
        else
        {
            alarm = true;
        }

        if (alarm)
        {
            // if it IS contra though, we want to not let it pass
            var direction = _transform.GetWorldPosition(entity) - _transform.GetWorldPosition(scanner);
            // If the object and the scanner are at the same position, we cant really push it AWAY, so just abort
            if (direction == Vector2.Zero)
                return;

            // yeet that shit away please, since we cant zap nonliving objects, alteratively, we can destroy it with gib
            _physics.ApplyLinearImpulse(entity, direction.Normalized() * component.PushForce);
        }

    }

    // This might have to go into the server side system to access uplinkComponent
    // private void ResetUplinkTelecrystals(EntityUid entity)
    // {
    //     foreach (var ent in GetAllContainedEntities(entity))
    //     {
    //         if (!TryComp<UplinkComponent>(ent, out var uplink))
    //             continue;
    //
    //         uplink.Telecrystals = 0;
    //         Dirty(ent, uplink);
    //     }
    // }


    private IEnumerable<EntityUid> GetAllContainedEntities(EntityUid entity)
    {
        // If the entity does not have the ability to have containers. we really don't care about it, so get out.
        if (!TryComp<ContainerManagerComponent>(entity, out var containerManager))
            yield break;

        foreach (var container in _container.GetAllContainers(entity, containerManager))
        {
            foreach (var ent in container.ContainedEntities)
            {
                yield return ent;

                foreach (var child in GetAllContainedEntities(ent)) // recusive search
                {
                    yield return child;
                }
            }
        }
    }

    private bool IsAllowedToCarry(EntityUid puid, EntityUid contraEntityUid)
    {
        _id.TryFindIdCard(puid, out var id);
        return false;
        // // Check inventory ID slot - raw ID equipped
        // if (_inventory.TryGetSlotEntity(entity, "id", out var uid) && HasRequiredAccess(uid))
        // {
        //     // stub
        // }
        // // Check inventory ID slot - PDA (search for internal ID)
    }

    // private bool HasRequiredAccess(EntityUid entity)
    // {
    //     if (!TryComp<AccessComponent>(entity, out var access))
    //         return false;
    //     var requiredAccess = new HashSet<string>{ "Command", "Security"};
    // }

    private void ElectrocuteEntity(EntityUid entity, CheckpointScannerComponent component)
    {
        _damageable.TryChangeDamage(entity, component.Damage);
        _statusEffects.TryAddStatusEffect(entity, "Electrocute", TimeSpan.FromSeconds(5), false);
        _stun.TryParalyze(entity, TimeSpan.FromSeconds(5), false);
    }

    private bool HasCaptainAccess(EntityUid id)
    {
        return TryComp<AccessComponent>(id, out var access) &&
               access.Tags.Contains("Captain");
    }
}
