namespace Interactions
{
    public class CharacterInteractionAgent : InteractionAgent
    {
        public override string GetEntityGUID() => tag;
        

        
        //TODO: store a list of unique interactions that this agent can perform
        //TODO: for each unique interaction, store a list of interactables that this agent can perform it on
    }
}