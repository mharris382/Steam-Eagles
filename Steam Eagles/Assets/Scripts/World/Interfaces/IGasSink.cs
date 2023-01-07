namespace GasSim
{
    public interface IGasSink : IGasIO
    {
        /// <summary>
        /// notifies potential listeners when gas is taken from the source.  Called as a sum from all cells
        /// called in <see cref="GasSimParticleSystem.DoSources"/> 
        /// </summary>
        /// <param name="amountTaken"></param>
        void GasAddedToSink(int amountAdded);
    }
}