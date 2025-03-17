using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.SecDog;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SecDogSystem))]
public sealed partial class SecDogComponent: Component
{
    [DataField("latchtime"), ViewVariables(VVAccess.ReadWrite)]
    public float LatchTime = 1.5f;

    [DataField("lungedistance"), ViewVariables(VVAccess.ReadWrite)]
    public float LungeDistance = 50.0f;

    [DataField("alertsound")]
    public SoundSpecifier AlertSound;
}
