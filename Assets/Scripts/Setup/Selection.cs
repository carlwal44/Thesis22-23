using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selection : MonoBehaviour
{
    /*
    Helper class to pass on the clicked points of the facade cornerpoints
    */
    public string imgURL;
    public Vector2 point1;
    public Vector2 point2;
    public Vector2 point3;
    public Vector2 point4;

    public Selection(string img, List<Vector2> points)
    {
        imgURL = img;
        point1 = points[0];
        point2 = points[1];
        point3 = points[2];
        point4 = points[3];
    }
}
