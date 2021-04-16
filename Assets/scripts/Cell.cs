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
    public bool debugme;
    public Weather weather;
    public WindZone windZone;
    // public float[] influences;

    // public string[] neighboursNames;
    // public float[] neighboursAngles;
    // public float[] neighboursWindDir;

    // Start is called before the first frame update

    public float get_cell_influence(float wind_direction, float wind_speed, float angle)
    {
        float reverse_wind_dir = (wind_direction + 180) % 360;
        float diff = System.Math.Abs(reverse_wind_dir - angle);
        diff = System.Math.Min(diff, 360 - diff);
        float influence = 0;
        if (diff < 90.0f)
        {
            influence = System.Math.Min(1, 0.001f * wind_speed * (90 - diff) / 90.0f);
        }
        // Debug.Log("diff " + diff + " influence " + influence + " wind_speed " + wind_speed);
        return influence;
    }

    public Weather influenceWeather(Weather localWeather, Weather targetWeather, float influence)
    {
        float precipitation = localWeather.precipitation + (targetWeather.precipitation - localWeather.precipitation) * influence;
        float wind_speed = localWeather.wind_speed + (targetWeather.wind_speed - localWeather.wind_speed) * influence;

        // target = -40
        // local = 1

        float loc_wind_dir = (localWeather.wind_direction + 360) % 360;
        float tar_wind_dir = (targetWeather.wind_direction + 360) % 360;

        float wind_dir_diff_base = tar_wind_dir - loc_wind_dir;
        float wind_dir_diff_abs = System.Math.Abs(wind_dir_diff_base);
        float wind_dir_diff = System.Math.Min(wind_dir_diff_abs, 360 - wind_dir_diff_abs);
        float wind_dir_diff_360 = (wind_dir_diff_base + 360) % 360;

        if (360 > wind_dir_diff_360 && wind_dir_diff_360 > 180)
        {
            wind_dir_diff = -wind_dir_diff;
        }

        float new_wind_direction = localWeather.wind_direction + wind_dir_diff * influence;

        new_wind_direction = (new_wind_direction + 360) % 360;
        if (debugme)
        {
            Debug.Log(wind_dir_diff);
            Debug.Log(new_wind_direction);
        }
        float cloud_cover = localWeather.cloud_cover; // + (targetWeather.cloud_cover - localWeather.cloud_cover) * influence;
        return new Weather(
            precipitation,
            wind_speed,
            new_wind_direction,
            // localWeather.wind_direction,
            cloud_cover
        );
    }

    public void update_cell(Cell[] neighbours)
    {

        int i = 0;
        foreach (Cell n in neighbours)
        {
            // Debug.Log(gameObject.name + " " + i);
            Vector3 targetDir = n.gameObject.transform.position - transform.position;
            // Debug.DrawLine(gameObject.transform.position, n.gameObject.transform.position, Color.red, 2.5f);
            // TODO: Check angle
            float angle = Vector3.SignedAngle(targetDir, Vector3.forward, Vector3.up);
            angle = (-angle + 360) % 360;
            // Debug.DrawRay(gameObject.transform.position, Vector3.forward, Color.green, 2.5f);
            float influence = get_cell_influence(n.weather.wind_direction, n.weather.wind_speed, angle);
            if (influence > 0)
            {
                Weather newWeather = influenceWeather(weather, n.weather, influence);
                weather = newWeather;
                Debug.DrawLine(gameObject.transform.position, n.gameObject.transform.position, Color.red, 0.5f);
            }


            // neighboursNames[i] = n.gameObject.name;
            // influences[i] = influence;
            // neighboursAngles[i] = angle;
            // neighboursWindDir[i] = n.weather.wind_direction;
            i += 1;
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
        // influences = new float[3];
        // neighboursNames = new string[3];
        // neighboursAngles = new float[3];
        // neighboursWindDir = new float[3];
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
