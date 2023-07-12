using CoreLib.Interfaces;
namespace Characters
{
    public interface ICharacterClimber
    {
        bool CanStartClimbing { get; }
        bool IsClimbing { get; }
        IClimbable StartClimbing();
    }
}