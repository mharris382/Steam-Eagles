using DG.Tweening;

namespace Characters.Cameras
{
    public interface ISplitScreenTween
    {
        Tween ToSplitScreenTween(float duration, ref float atPosition);
    }
}