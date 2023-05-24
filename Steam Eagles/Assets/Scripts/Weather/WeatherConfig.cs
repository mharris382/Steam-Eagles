using System;
using Weather.Storms;

[Serializable]
public class WeatherConfig
{
    public float updateRate = 1f;
    public GlobalStormConfig stormConfig;
}