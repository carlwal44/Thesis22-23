using System;
using UnityEngine;

public class MercatorBounds
{
    static double MERCATOR_RANGE = 256;
    Point pixelsOrigin;
    double pixelsPerLonDegree;
    double pixelsPerLonRadian;

    public MercatorBounds()
    {
        pixelsOrigin = new Point(MERCATOR_RANGE / 2, MERCATOR_RANGE / 2);
        pixelsPerLonDegree = MERCATOR_RANGE / 360;
        pixelsPerLonRadian = MERCATOR_RANGE / (2 * Math.PI);
    }

    double bound(double value, double opt_min, double opt_max)
    {
        if (opt_min != 0)
            value = Math.Max(value, opt_min);
        if (opt_max != 0)
            value = Math.Min(value, opt_max);
        return value;
    }

    double degreesToRadians(double deg)
    {
        return deg * (Math.PI / 180);
    }

    double radiansToDegrees(double rad)
    {
        return rad / (Math.PI / 180);
    }

    Point fromLatLonToPoint(Coordinate_GoogleMaps latLon)
    {
        Point point = new Point();
        Point origin = pixelsOrigin;

        point.X = origin.X + latLon.Longitude * pixelsPerLonDegree;

        double sinY = bound(Math.Sin(degreesToRadians(latLon.Latitude)), -0.9999, 0.9999);

        point.Y = origin.Y + 0.5 * Math.Log((1 + sinY) / (1 - sinY)) * -pixelsPerLonRadian;

        return point;
    }

    Coordinate_GoogleMaps fromPointToLatlon(Point point)
    {
        Point origin = pixelsOrigin;
        Coordinate_GoogleMaps latLon = new Coordinate_GoogleMaps(0,0);

        latLon.Latitude = (point.X - origin.X) / pixelsPerLonDegree;
        double latRadians = (point.Y - origin.Y) / -pixelsPerLonRadian;
        latLon.Longitude = radiansToDegrees(2 * Math.Atan(Math.Exp(latRadians)) - Math.PI / 2);

        return latLon;
    }

    public void GetCorners(Coordinate_GoogleMaps center, float zoom, float mapWidth, float mapHeight)
    {
        double scale = Math.Pow(2, zoom);
        Point centerPx = fromLatLonToPoint(center);
        Point SWPoint = new Point(centerPx.X - (mapWidth / 2) / scale, centerPx.Y + (mapHeight / 2) / scale);
        Coordinate_GoogleMaps SWLatLon = fromPointToLatlon(SWPoint);
        Debug.Log(SWLatLon.Latitude + " " + SWLatLon.Longitude + " " + SWPoint.X + " " + SWPoint.Y);
    }
}

public static class GoogleMapsAPI{

    static GoogleMapsAPI()
    {
        OriginX = TileSize / 2;
        OriginY = TileSize / 2;
        PixelsPerLonDegree = TileSize / 360.0;
        PixelsPerLonRadian = TileSize / (2 * Math.PI);
    }

    public static int TileSize = 256;
    public static double OriginX, OriginY;
    public static double PixelsPerLonDegree;
    public static double PixelsPerLonRadian;

    public static double DegreesToRadians(double deg)
    {
        return deg * Math.PI / 180.0;
    }

    public static double RadiansToDegrees(double rads)
    {
        return rads * 180.0 / Math.PI;
    }

    public static double Bound(double value, double min, double max)
    {
        value = Math.Min(value, max);
        return Math.Max(value, min);
    }

    //From Lat, Lon to World Coordinate X, Y. I'm being explicit in assigning to
    //X and Y properties.
    public static Coordinate_GoogleMaps Mercator(double latitude, double longitude)
    {
        double siny = Bound(Math.Sin(DegreesToRadians(latitude)), -.9999, .9999);

        Coordinate_GoogleMaps c = new Coordinate_GoogleMaps(0, 0);
        c.X = OriginX + longitude * PixelsPerLonDegree;
        c.Y = OriginY + .5 * Math.Log((1 + siny) / (1 - siny)) * -PixelsPerLonRadian;

        return c;
    }

    //From World Coordinate X, Y to Lat, Lon. I'm being explicit in assigning to
    //Latitude and Longitude properties.
    public static Coordinate_GoogleMaps InverseMercator(double x, double y)
    {
        Coordinate_GoogleMaps c = new Coordinate_GoogleMaps(0, 0);

        c.Longitude = (x - OriginX) / PixelsPerLonDegree;
        double latRadians = (y - OriginY) / -PixelsPerLonRadian;
        c.Latitude = RadiansToDegrees(Math.Atan(Math.Sinh(latRadians)));

        return c;
    }

    public static MapCoordinates GetBounds(Coordinate_GoogleMaps center, int zoom, int mapWidth, int mapHeight)
    {
        var scale = Math.Pow(2, zoom);

        var centerWorld = Mercator(center.Latitude, center.Longitude);
        var centerPixel = new Coordinate_GoogleMaps(0, 0);
        centerPixel.X = centerWorld.X * scale;
        centerPixel.Y = centerWorld.Y * scale;

        var NEPixel = new Coordinate_GoogleMaps(0, 0);
        NEPixel.X = centerPixel.X + mapWidth / 2.0;
        NEPixel.Y = centerPixel.Y - mapHeight / 2.0;

        var SWPixel = new Coordinate_GoogleMaps(0, 0);
        SWPixel.X = centerPixel.X - mapWidth / 2.0;
        SWPixel.Y = centerPixel.Y + mapHeight / 2.0;

        var NEWorld = new Coordinate_GoogleMaps(0, 0);
        NEWorld.X = NEPixel.X / scale;
        NEWorld.Y = NEPixel.Y / scale;

        var SWWorld = new Coordinate_GoogleMaps(0, 0);
        SWWorld.X = SWPixel.X / scale;
        SWWorld.Y = SWPixel.Y / scale;

        var NELatLon = InverseMercator(NEWorld.X, NEWorld.Y);
        var SWLatLon = InverseMercator(SWWorld.X, SWWorld.Y);

        return new MapCoordinates() { NorthEast = NELatLon, SouthWest = SWLatLon };
    }
}

public class MapCoordinates
{
    public Coordinate_GoogleMaps SouthWest { get; set; }
    public Coordinate_GoogleMaps NorthEast { get; set; }
}

public class Coordinate_GoogleMaps
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double Y { get { return Latitude; } set { Latitude = value; } }
    public double X { get { return Longitude; } set { Longitude = value; } }

    public Coordinate_GoogleMaps(double lng, double lat)
    {
        Latitude = lat;
        Longitude = lng;
    }

    public override string ToString()
    {
        return Math.Round(X, 6).ToString() + ", " + Math.Round(Y, 6).ToString();
    }
}

public struct Point{
    public double X  { get; set; }
    public double Y  { get; set; }

    public Point(double X, double Y)
    {
        this.X = X;
        this.Y = Y;
    }
}