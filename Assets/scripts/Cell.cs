using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weather
{
    public float precipitation;
    public float wind_speed;
    public float wind_direction;
    public float cloud_cover;

    public Weather(
        float _precipitation,
        float _wind_speed,
        float _wind_direction,
        float _cloud_cover
        )
    {
        precipitation = _precipitation;
        wind_speed = _wind_speed;
        wind_direction = _wind_direction;
        cloud_cover = _cloud_cover;
    }
}



public class Cell : MonoBehaviour
{
    public Weather weather;
    public WindZone windZone;
    // Start is called before the first frame update

    public float get_cell_influence(float wind_direction, float wind_speed, float angle)
    {
        // Debug.Log("dir " + wind_direction + " angle " + angle);
        float diff = 1 - System.Math.Abs(((wind_direction - angle) % 360) / 180);
        return 0.001f * wind_speed * diff;
    }

    public Weather influenceWeather(Weather localWeather, Weather targetWeather, float influence)
    {
        float precipitation = localWeather.precipitation + (targetWeather.precipitation - localWeather.precipitation) * influence;
        float wind_speed = localWeather.wind_speed + (targetWeather.wind_speed - localWeather.wind_speed) * influence;
        float wind_direction = localWeather.wind_direction; // + (targetWeather.wind_direction - localWeather.wind_direction) * influence;
        float cloud_cover = localWeather.cloud_cover; // + (targetWeather.cloud_cover - localWeather.cloud_cover) * influence;
        return new Weather(
            precipitation,
            wind_speed,
            wind_direction,
            cloud_cover
        );
    }

    public void update_cell(Cell[] neighbours)
    {
        foreach (Cell n in neighbours)
        {
            Vector3 targetDir = n.gameObject.transform.position - transform.position;
            // TODO: Check angle
            float angle = Vector3.SignedAngle(targetDir, Vector3.forward, Vector3.up);
            float influence = get_cell_influence(n.weather.wind_direction, n.weather.wind_speed, angle);
            Weather newWeather = influenceWeather(weather, n.weather, influence);
            weather = newWeather;
        }
    }
    void Start()
    {
        weather = new Weather(
            0.0f,
            0.0f,
            0.0f,
            0.0f
        );
    }

    // Update is called once per frame
    void Update()
    {
        windZone.transform.eulerAngles = new Vector3(
            windZone.transform.eulerAngles.x,
            weather.wind_direction,
            windZone.transform.eulerAngles.z
        );
    }
}
