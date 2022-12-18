namespace Players
{
    public interface IPlayerSubsystem
    {
        void OnPlayerStateUpdated(PlayerState playerState);
    }
}