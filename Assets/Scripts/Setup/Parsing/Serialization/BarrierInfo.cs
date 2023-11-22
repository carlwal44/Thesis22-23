using System.Collections.Generic;
using UnityEngine;

public struct BarrierParameters
{

    public float width;
    public float height;
    public Material material;


    public BarrierParameters(float w, float h,string m)
    {
        width = w;
        height = h;
        material = Resources.Load<Material>("art/material/" + m);
    }
}

// wall, fence, kerb, hedge, retaining_wall
public static class BarrierInfo
{
    public static Dictionary<BarrierType, BarrierParameters> barrierInfo = new Dictionary<BarrierType, BarrierParameters> {
            { BarrierType.wall , new BarrierParameters( 0.2f, 2.7f,"brick") },
            { BarrierType.fence , new BarrierParameters(0.2f, 2.7f,"wood") },
            { BarrierType.kerb , new BarrierParameters( 0.3f, 0.2f, "sett") },
            { BarrierType.hedge , new BarrierParameters( 0.4f, 2, "hedge") },
            { BarrierType.retaining_wall , new BarrierParameters( 0.2f, 2.7f, "stone") }
    };

    public static BarrierParameters GetbarrierInfo(BarrierType type)
    {
        return barrierInfo[type];
    }
}
