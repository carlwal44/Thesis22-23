using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WindowColour
{
    Red, Grey, Brown
}

public class Window : ScriptableObject
{
    public float centerX { get; set; }
    public float centerY { get; set; }
    public float width { get; set; }
    public float length { get; set; }
    public WindowColour colour { get; set; }
}
