
using UnityEngine;

public class Bollard
{
    Vector3 pos;
    public Bollard(Vector3 pos)
    {
        this.pos = pos;
    }

    public Vector3 getPos()
    {
        return pos;
    }
}

