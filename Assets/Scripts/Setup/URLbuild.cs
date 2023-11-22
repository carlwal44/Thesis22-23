using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class metaDataJSON
{
    public string name;
    public string date;
    public locationJSON location;
    public string pano;
    public string status;
}

[System.Serializable]
public struct locationJSON
{
    public string lat;
    public string lng;
}

public class URLbuild
{
    public string lat;
    public string lon;
    public string fov;
    public string heading;
    public string pitch;

    public string meta = "/metadata";
    public string URL_start = "https://maps.googleapis.com/maps/api/streetview";
    public string URL_lat = "?size=640x640&location=";
    public string URL_lon = @",%20";
    public string URL_heading = "&heading=";
    public string URL_pitch = "&pitch=";
    public string URL_key = "&***insert KEY***&fov=";

    public URLbuild(double lattitude, double longitude, int fieldofview, double headingval, double pitchval)
    {
        lat = lattitude.ToString();
        lon = longitude.ToString();
        fov = fieldofview.ToString();
        heading = headingval.ToString();
        pitch = pitchval.ToString();
    }

    public string getURL()
    {
        string url = URL_start + URL_lat + lat + URL_lon + lon + URL_heading + heading + URL_pitch + pitch + URL_key + fov;
        return url;
    }

    public string getURL_meta()
    {
        string url = URL_start + meta + URL_lat + lat + URL_lon + lon + URL_heading + heading + URL_pitch + pitch + URL_key + fov;
        return url;
    }
}

public class URLBuild_map
{
    public string centerlat;
    public string centerlng;
    public string zoom;
    public string size;
    public string scale;

    public string URL_start = "https://maps.googleapis.com/maps/api/staticmap?center=";
    public string key = "&***insert KEY***";

    public URLBuild_map(double lat, double lng, int zoomval, int sizeval, int scaleval)
    {
        centerlat = lat.ToString();
        centerlng = lng.ToString();
        zoom = zoomval.ToString();
        size = sizeval.ToString();
        scale = scaleval.ToString();
    } 

    public string getURL()
    {
        string url = URL_start + centerlat + "," + centerlng + "&zoom=" + zoom + "&size=" + size + "x" + size + "&scale=" + scale + key;
        return url;
    }
}