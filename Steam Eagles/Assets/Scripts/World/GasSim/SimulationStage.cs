public enum SimulationStage
{
    /// <summary>
    /// simulation is not running because there it has no gas
    /// </summary>
    IDLE,
    /// <summary>
    /// tilemap will be static during this stage, therefore it should be acceptable to have the tiles pass the data
    /// to the simulation directly
    /// </summary>
    UPDATE_PRESSURE,
    /// <summary>
    /// calculate velocity based on current pressure, velocity is stored in a render texture, pressure will be
    /// passed into the compute shader using a compute buffer.  At this time we should have a collection of
    /// pressure cells from the previous stages
    /// </summary>
    CALCULATE_VELOCITY,
    /// <summary>
    /// once the compute buffer returns data, it needs to be applied to the tilemap.  read the changes from the compute
    /// buffer and apply them to the tilemap
    /// </summary>
    APPLY_PRESSURE
}