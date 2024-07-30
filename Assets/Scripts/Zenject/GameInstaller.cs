using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<AudioManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<InputManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<NodeManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ViewManager>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ConnectionLineDrawer>().FromComponentInHierarchy().AsSingle();
    }
}