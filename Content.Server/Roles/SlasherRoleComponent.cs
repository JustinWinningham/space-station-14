using Content.Server.Slasher;
using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are a Slasher.
/// </summary>
[RegisterComponent, Access(typeof(SlasherSystem))]
public sealed partial class SlasherRoleComponent : BaseMindRoleComponent
{
}
