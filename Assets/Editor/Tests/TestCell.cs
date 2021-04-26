using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestCell
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestUpdateWind()
    {
        Vector3 position = new Vector3(0, 0, 10);
        Vector3 up = new Vector3(0, 1, 0);
        Vector3 forward = new Vector3(0, 0, 1);
        Vector3[] windDirections = new Vector3[3];
        windDirections[0] = new Vector3(0, 0, 1);
        windDirections[1] = new Vector3(1, 0, 0);
        windDirections[2] = new Vector3(0, 0, 0);

        Vector3[] angles = new Vector3[3];
        angles[0] = new Vector3(0, 0, 0) - position;
        angles[1] = new Vector3(10, 0, 10) - position;
        angles[2] = new Vector3(0, 0, 1) - position;

        float[] windSpeeds = new float[3] { 1, 0, 0 };
        var a = WeatherHelpers.UpdateWind(
            position,
            up, forward,
            windDirections, windSpeeds, angles);
        float expectedMagnitude = 1;
        Vector3 expectedDirection = new Vector3(0, 0, 1);
        Assert.AreEqual(expectedMagnitude, a.Item1);
        Assert.AreEqual(expectedDirection, a.Item2);

        windSpeeds[1] = 1;
        var b = WeatherHelpers.UpdateWind(
            position,
            up, forward,
            windDirections, windSpeeds, angles);
        expectedMagnitude = 2;
        expectedDirection = new Vector3(0, 0, 1);
        Assert.AreEqual(expectedMagnitude, a.Item1);
        Assert.AreEqual(expectedDirection, a.Item2);
    }

}
