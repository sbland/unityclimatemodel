using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Weather
{
    public float precipitation;
    public float windSpeed;
    public Vector3 windDirection;
    public float cloudCover;

    public Weather(
        float Precipitation,
        float WindSpeed,
        Vector3 WindDirection,
        float CloudCover
        )
    {
        precipitation = Precipitation;
        windSpeed = WindSpeed;
        windDirection = WindDirection;
        cloudCover = CloudCover;
    }
}

public static class WeatherHelpers
{
    public static (float, Vector3) UpdateWind(Vector3 position, Vector3 up, Vector3 forward, Vector3[] windDirections, float[] windSpeeds, Vector3[] angles)
    {
        // TODO: Should take into account distance
        // IEnumerable<Vector3> magDirection = windDirections.Zip(windSpeeds, (dir, speed) => -Vector3.Normalize(dir) * speed);
        // var a = windDirections.Zip(windSpeeds, (dir, speed) => Vector3.Normalize(dir) * speed).ToArray();
        // foreach (var i in a)
        // {
        //     Debug.Log(i);
        // }

        IEnumerable<Vector3> netAngles = angles.Select(a => Vector3.Normalize(a));
        var netAngleDebug = angles.Select(a => Vector3.Normalize(a)).ToArray();
        IEnumerable<Vector3> diffDirections = windDirections.Zip(netAngles, (wind, angle) => Vector3.Normalize(wind - angle)).ToArray();
        IEnumerable<Vector3> magDirection = diffDirections.Zip(windSpeeds, (wind, speed) => wind * speed).ToArray();
        Vector3 sumDirection = magDirection.Aggregate((acc, dir) => acc + dir);
        Vector3 averageDirection = sumDirection; // / windDirections.Length;
        Vector3 normalDirection = Vector3.Normalize(averageDirection);
        Debug.DrawRay(position, normalDirection, Color.blue, 0.1f);
        Vector3 normalAngle = Vector3.Cross(normalDirection, up);
        Debug.DrawRay(position, normalAngle, Color.red, 0.1f);
        Vector3 normalAngleB = Vector3.Cross(normalDirection, forward);
        Vector3 normalAngleC = Vector3.Cross(normalDirection, normalAngle);
        Debug.DrawRay(position, normalAngleB, Color.green, 0.1f);
        Debug.DrawRay(position, normalAngleC, Color.yellow, 0.1f);
        Quaternion q = Quaternion.FromToRotation(forward, normalDirection);
        float magnitude = Vector3.Magnitude(averageDirection);

        return (magnitude, normalDirection);
    }

}

public class Cell : MonoBehaviour
{
    public bool debugme;
    public Weather weather;
    public WindZone windZone;

    public bool isWindSource;
    /// Update the wind speed and direction of a cell based on neighbour wind speed and direction


    public void UpdateCell(Cell[] neighbours)
    {
        float[] nWindSpeeds = neighbours.Select(n => n.weather.windSpeed).ToArray();
        Vector3[] nWindDirections = neighbours.Select(n => n.weather.windDirection).ToArray();
        Vector3[] nAngles = neighbours.Select(n => n.gameObject.transform.position - transform.position).ToArray();

        var newWind = isWindSource
            ? (weather.windSpeed, weather.windDirection)
            : WeatherHelpers.UpdateWind(
                transform.position,
                transform.up,
                transform.forward,
                nWindDirections,
                nWindSpeeds,
                nAngles);
        weather.windSpeed = newWind.Item1;
        weather.windDirection = newWind.Item2;
    }
    void Start()
    {
        weather = new Weather(
            0.0f,
            0.0f,
            new Vector3(0, 0, 0),
            0.0f
        );
    }

    // Update is called once per frame
    void Update()
    {
        windZone.transform.LookAt(windZone.transform.position + weather.windDirection);
        if (isWindSource)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                weather.windDirection = Quaternion.AngleAxis(-2, Vector3.up) * weather.windDirection;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                weather.windDirection = Quaternion.AngleAxis(2, Vector3.up) * weather.windDirection;

                // weather.windDirection = Vector3.RotateTowards(
                //     weather.windDirection,
                //     new Vector3(0, -1, 0),
                //     0.1f,
                //     0.1f
                //     );
            }
        }
    }
}
