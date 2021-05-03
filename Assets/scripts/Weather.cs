/// A estimated weather model based on https://tomforsyth1000.github.io/papers/cellular_automata_for_physical_modelling.html

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


[System.Serializable]
public class Weather
{
    public float precipitation;
    public float windSpeed;
    public Vector3 windDirection;
    public float cloudCover;
    public float airPressure;
    public bool isWindSource;

    public Weather(
        float Precipitation,
        float WindSpeed,
        Vector3 WindDirection,
        float CloudCover,
        float AirPressure,
        bool IsWindSource
        )
    {
        precipitation = Precipitation;
        windSpeed = WindSpeed;
        windDirection = WindDirection;
        cloudCover = CloudCover;
        airPressure = AirPressure;
        isWindSource = IsWindSource;
    }
    public Weather ShallowCopy()
    {
        return (Weather)this.MemberwiseClone();
    }
}

// Container for transform properties so we don't need to use Unity Transform
public static class WeatherHelpers
{
    const float AIR_FLOW_RATE = 0.005f;
    const float AIR_DENSITY = 1.22f;

    public static Weather UpdateWeather(Weather localWeather, Weather[] nWeathers, Vector3[] nVectors, ITransform localTransform)
    {
        var nWindDirections = nWeathers.Select(n => n.windDirection);
        var nWindSpeeds = nWeathers.Select(n => n.windSpeed);
        var nAirPressures = nWeathers.Select(n => n.airPressure);

        // OLD METHOD
        // var (newWindSpeed, newWindDirection) = localWeather.isWindSource
        //     ? (localWeather.windSpeed, localWeather.windDirection)
        //     : WeatherHelpers.UpdateWind(
        //         localTransform,
        //         nWindDirections.ToArray(),
        //         nWindSpeeds.ToArray(),
        //         nVectors);
        if(localWeather.isWindSource) {
            return localWeather;
        }
        Weather newWeather = localWeather.ShallowCopy();

        var (newWindSpeed, newWindDirection) = WeatherHelpers.UpdateWind(
                localWeather.airPressure,
                nAirPressures.ToArray(),
                localWeather.windDirection,
                localWeather.windSpeed,
                nWindDirections.ToArray(),
                nWindSpeeds.ToArray(),
                nVectors);

        newWeather.windSpeed = newWindSpeed;
        newWeather.windDirection = newWindDirection;

        newWeather.airPressure = UpdateAirPressure(
            localWeather.airPressure,
            nAirPressures.ToArray(),
            localWeather.windDirection,
            localWeather.windSpeed,
            nWindDirections.ToArray(),
            nWindSpeeds.ToArray(),
            nVectors
        );

        return newWeather;
    }

    /// Calculate the new air pressure for this cell based on the surrounding air pressure
    ///
    /// Returns a tuple where the first element is the new pressure and the
    ///
    /// This is calculate by assuming the pressure represents a hypobolic plane that
    /// air particles are moving over. Higher pressure cells are higher, lower pressure cells are lower
    /// we need to estimate the angle of the current cell.
    public static float UpdateAirPressure(
        float initialPressure,
        float[] nPressures,
        Vector3 initialWindDir,
        float initialWindMomentum,
        Vector3[] nWindDir,
        float[] nWindSpeeds,
        Vector3[] nVectors
    )
    {
        // First we find the imaginary 3d angle of the current cell on the "Air pressure plane"
        // Example: Local AP is 1 North plane has AP of 2 and South plane has 1

        // Forces involved:
        // Momentum from previous time step
        // Air pressure gradient /Gravity
        // Momentum of surrounding cells?


        // Find the new air pressure based on air moved from surrounding cells
        // Find the new momentum from surrounding cells
        // apply the force of air pressure difference.

        // Air pressure change

        // Get increased pressure from surrounding flows
        // The increase pressure is determinded by air flow * angleDiff / 90 if angleDiff < 90 else 0;
        // TODO: make sure wind contains magnitude
        // TODO Make sure nVector direction is curr cell to neighbour
        // IEnumerable<Vector3> netAngles = angles.Select(a => -Vector3.Normalize(a));
        // IEnumerable<Vector3> diffDirections = windDirections.Zip(netAngles, (wind, angle) => Vector3.Normalize(wind - angle)).ToArray();
        // IEnumerable<Vector3> magDirection = diffDirections.Zip(windSpeeds, (wind, speed) => wind * speed).ToArray();
        var angleDiff = nVectors.Zip(nWindDir, (angle, wind) => Vector3.Angle(-angle, wind));
        var angleCapped = angleDiff.Select(angleDiff => Math.Max(0, Math.Min(90,angleDiff)));
        var magIncreasedPressure = angleCapped.Select(angleDiff => (90 - angleDiff)/90);
        

        
        // TODO: Make sure speed is in volume per second.
        var increasedPressure = magIncreasedPressure
            .Zip(nWindSpeeds, (influence, speed) => influence * speed * AIR_FLOW_RATE)
            .Aggregate(0.0f, (acc, v) => acc + v);
        float newPressure = initialPressure + increasedPressure - (initialWindMomentum * AIR_FLOW_RATE * initialPressure);


        

        // foreach (var p in nPressures)
        // {
        //     float pressureDiff = newPressure - p;
        //     float flow = AIR_FLOW_RATE * pressureDiff;
        //     newPressure -= flow;
        // }
        return newPressure;
    }

    public static (float, Vector3) UpdateWind(
        // ITransform t,
        // Vector3[] windDirections,
        // float[] windSpeeds,
        // Vector3[] angles
        float initialPressure,
        float[] nPressures,
        Vector3 initialWindDir,
        float initialWindMomentum,
        Vector3[] nWindDir,
        float[] nWindSpeeds,
        Vector3[] nVectors
    )
    {
        // TODO: Should take into account distance
        // IEnumerable<Vector3> magDirection = windDirections.Zip(windSpeeds, (dir, speed) => -Vector3.Normalize(dir) * speed);
        // var a = windDirections.Zip(windSpeeds, (dir, speed) => Vector3.Normalize(dir) * speed).ToArray();
        // foreach (var i in a)
        // {
        //     Debug.Log(i);
        // }

        

        // Get acceleration towards each direction
        // TODO: Implement this 
        var pDiff = nPressures.Select(nP => initialPressure - nP);
        var accels = pDiff.Select(pd => -pd/AIR_DENSITY);
        var accelDir = accels.Zip(nVectors, (accel,vec) => accel * -vec);
        // TODO: Check what happens when flow in all directions
        var averageAccel = accelDir.Aggregate((accelDir, dir) => accelDir + dir);
        var newWind =  AIR_FLOW_RATE * averageAccel * initialPressure;
        // var newWind = (initialWindDir * initialWindMomentum)  + AIR_FLOW_RATE * averageAccel * initialPressure;
        float magnitude = Vector3.Magnitude(newWind);
        Vector3 direction = Vector3.Normalize(newWind);
        return (magnitude, direction);

        // var flows = nPressures.Select(nP => AIR_FLOW_RATE * (initialPressure - nP));
        // var flowVectors = flows.Zip(nVectors, (flow, nVector) => flow * nVector);
        // Vector3 sumDirection = magDirection.Aggregate((acc, dir) => acc + dir);


        //  OLD METHOD
        // IEnumerable<Vector3> netAngles = angles.Select(a => Vector3.Normalize(a));
        // var netAngleDebug = angles.Select(a => Vector3.Normalize(a)).ToArray();
        // IEnumerable<Vector3> diffDirections = windDirections.Zip(netAngles, (wind, angle) => Vector3.Normalize(wind - angle)).ToArray();
        // IEnumerable<Vector3> magDirection = diffDirections.Zip(windSpeeds, (wind, speed) => wind * speed).ToArray();
        // Vector3 sumDirection = magDirection.Aggregate((acc, dir) => acc + dir);
        // Vector3 averageDirection = sumDirection; // / windDirections.Length;
        // Vector3 normalDirection = Vector3.Normalize(averageDirection);
        // Debug.DrawRay(t.position, normalDirection, Color.blue, 0.1f);
        // Vector3 normalAngle = Vector3.Cross(normalDirection, t.up);
        // Debug.DrawRay(t.position, normalAngle, Color.red, 0.1f);
        // Vector3 normalAngleB = Vector3.Cross(normalDirection, t.forward);
        // Vector3 normalAngleC = Vector3.Cross(normalDirection, normalAngle);
        // Debug.DrawRay(t.position, normalAngleB, Color.green, 0.1f);
        // Debug.DrawRay(t.position, normalAngleC, Color.yellow, 0.1f);
        // Quaternion q = Quaternion.FromToRotation(t.forward, normalDirection);
        // float magnitude = Vector3.Magnitude(averageDirection);

        // return (magnitude, normalDirection);
    }

}
