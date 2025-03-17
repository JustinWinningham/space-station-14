namespace Content.Shared.SecDog;


public sealed class SecDogSystem : EntitySystem
{

    public override void Initialize()
    {
        SubscribeLocalEvent<SecDogComponent, ComponentShutdown>(OnShutdown);
    }


    public void OnShutdown(EntityUid uid, SecDogComponent component, ComponentShutdown args)
    {

    }
}
