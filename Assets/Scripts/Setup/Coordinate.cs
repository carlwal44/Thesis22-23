using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Coordinate
{    
    public static double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = ToRad(lon2-lon1);
        var dPhi = Math.Log(
            Math.Tan(ToRad(lat2)/2+Math.PI/4)/Math.Tan(ToRad(lat1)/2+Math.PI/4));
            if (Math.Abs(dLon) > Math.PI) 
                dLon = dLon > 0 ? -(2*Math.PI-dLon) : (2*Math.PI+dLon);
        return ToBearing(Math.Atan2(dLon, dPhi));
    }

    public static double ToBearing(double radians) 
    {  
    // convert radians to degrees (as bearing: 0...360)
    return (ToDegrees(radians) +360) % 360;
    }

    public static double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    public static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }
}
