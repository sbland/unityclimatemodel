using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ITransform
{
    public Vector3 position;
    public Vector3 up;
    public Vector3 forward;

    public ITransform(
        Transform t
    )
    {
        position = t.position;
        up = t.up;
        forward = t.forward;
    }

    public ITransform(
        Vector3 Position,
        Vector3 Up,
        Vector3 Forward
    )
    {
        position = Position;
        up = Up;
        forward = Forward;
    }
}
