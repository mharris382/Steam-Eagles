public interface IStormViewLayer
{
    void SetActiveIntensity(int intensity);
    void SetVisible(bool visible);
    void StormStarting(float startDuration);
}