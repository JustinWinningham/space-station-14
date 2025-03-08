using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.CheckpointScanner;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedCheckpointScannerSystem))]
public sealed partial class CheckpointScannerComponent: Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool AllowCptnAccess;

    /// <summary>
    /// Whether or not the machine has power. We put it here
    /// so we can network and predict it.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public bool Powered;

    /// <summary>
    /// The sound played when somebody walks through the scanner and doesn't trigger the alarm.
    /// </summary>
    [DataField("clearsound"), AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? ClearSound;

    /// <summary>
    /// The sound played when somebody walks through the scanner and triggers the alarm.
    /// </summary>
    [DataField("alarmsound"), AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? AlarmSound;

    [DataField("pushforce"), AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public float PushForce = 100.0f;

    /// <summary>
    ///  If we want to destroy contraband. true by default, set to false if we just want to detect it
    /// </summary>
    [DataField]
    public bool Destroy = true;

    /// <summary>
    /// How badly we want to damage the player if they are carrying contraband illegally
    /// </summary>
    [DataField("damage"), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Shock", 10 },
            { "Heat", 5 },
        }
    };


    /// <summary>
    /// A counter of how many items have been disabled, destroyed, or otherwise prevented from passing through the scanner
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ItemsDisabled;

    /// <summary>
    /// Number of times people were curious. just for funnies
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TimesExamined;

    public EntityUid? Stream;
}
