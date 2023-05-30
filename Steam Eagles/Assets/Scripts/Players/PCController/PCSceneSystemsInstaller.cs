using Players.PCController.Interactions;
using Players.PCController.RoomCamera;
using Zenject;

public class PCSceneSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        PCParallaxSystemsInstaller.Install(Container);
        PCInteractionSystemsInstaller.Install(Container);
        PCRoomCameraInstaller.Install(Container);
    }
}