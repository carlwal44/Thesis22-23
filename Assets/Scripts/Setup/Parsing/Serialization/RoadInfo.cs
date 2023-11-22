using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct RoadParameters {

    public float width;
    public int lanes;
    public float offset;
    public string texture;
    public bool lit;
    public int hier;


    public RoadParameters(float w, int l, float o , string m, bool lit, int hier)
    {
        width = w;
        lanes = l;
        offset = o;
        this.lit = lit;
        texture = m;
        this.hier = hier;
    }
}
public static class RoadInfo
{
    public static Dictionary<RoadType, RoadParameters> roadInfo = new Dictionary<RoadType, RoadParameters> {
            { RoadType.unclassified , new RoadParameters( 3f, 1, 0.0f,"road",false,0) },
            { RoadType.residential , new RoadParameters( 2.8f, 2, 0.0f,"road",false,0) },
            { RoadType.service , new RoadParameters( 2.0f, 1, 0.0f,"parking",true,0) },
            { RoadType.track , new RoadParameters( 2.0f, 1, 0.1f,"track",false,2) },
            { RoadType.footway , new RoadParameters( 1.8f, 1, 0.1f, "footway",false,2) },
            { RoadType.path , new RoadParameters( 1.4f, 1, 0.1f, "footway",false,2) },
            { RoadType.tertiary , new RoadParameters( 2.5f, 2, 0.0f,"road",false,2) },
            { RoadType.crossing , new RoadParameters( 2.0f, 1, 0.1f,"crossing",false,2) },
            { RoadType.secondary , new RoadParameters( 3.0f, 2, 0.0f,"road",true,1) },
            { RoadType.primary , new RoadParameters( 3.5f, 2, 0.0f,"road",true,0) },
            { RoadType.cycleway , new RoadParameters( 1.5f, 1, 0.1f,"cycleway",true,1) },
            { RoadType.platform , new RoadParameters( 2.0f, 1, 0.3f,"platform",false,2) },
            { RoadType.living_street , new RoadParameters( 2.5f, 1, 0.0f, "road",false,0) },
            { RoadType.pedestrian , new RoadParameters( 1.8f, 1, 0.1f,"footway",true,2) },
            { RoadType.road , new RoadParameters( 2.8f, 2, 0.0f,"road",false,0) }
    };

    public static RoadParameters getRoadInfo(RoadType type)
    {
        return roadInfo[type];
    }
}
