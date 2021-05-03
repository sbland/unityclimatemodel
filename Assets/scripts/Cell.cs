using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool debugme;
    public (int, int, int) id;
    public int x;
    public int y;
    public int z;

    public Weather weather;
    public WindZone windZone;

    /// Update the wind speed and direction of a cell based on neighbour wind speed and direction


    public void UpdateCell(Cell[] neighbours)
    {
        Weather[] nWeathers = neighbours.Select(n => n.weather).ToArray();
        Vector3[] nAngles = neighbours.Select(n => n.gameObject.transform.position - transform.position).ToArray();

        weather = WeatherHelpers.UpdateWeather(weather, nWeathers, nAngles, new ITransform(transform));
    }
    void Start()
    {
        // weather = new Weather(
        //     0.0f,
        //     0.0f,
        //     new Vector3(0, 0, 0),
        //     0.0f
        // );
    }

    void UpdateColor() {
        var renderer = transform.GetComponent<Renderer>();
        var strength = weather.airPressure / 100.0f;
        renderer.material.SetColor("_Color", new Color(strength, 0.5f, 0.5f,1f));
    }

    // Update is called once per frame
    void Update()
    {
        UpdateColor();
        windZone.transform.LookAt(windZone.transform.position + weather.windDirection);
        if (weather.isWindSource)
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
