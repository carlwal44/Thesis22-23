using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Linq;
using System.IO;

public class API_map : MonoBehaviour
{
    public double Latitude;
    public double Longitude;
    public int zoom = 17;
    public int size = 1000;
    public int scale = 2;
    
    public RawImage img;        // TODO - manually entering values - zoom in/out?
    public Button continueButton;
    public Button backButton;
    [HideInInspector]
    public RawImage mark;
    [HideInInspector]
    private UnityEngine.UI.Extensions.UILineRenderer LineRenderer;

    // OSM
    [HideInInspector]  
    public Dictionary<ulong, OsmNode> nodes;
    [HideInInspector]
    public Dictionary<Vector3, OsmBuilding> OSMBuildings;
    
    [HideInInspector]
    public List<buildingCoord> buildingCoordList;

    public double[] clickedPos;
    [HideInInspector]
    public Coordinate_GoogleMaps SW;
    [HideInInspector]
    public Coordinate_GoogleMaps NE;
    
    private bool correctBuilding; 
    private BuildingMaker BuildingMaker_Script;
    private buildingCoord closestBuilding;
    private int closestFacade; 

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mouse_position = Input.mousePosition;
            if(mouse_position.x < 1460 && mouse_position.x > 460 && mouse_position.y < 1040 && mouse_position.y > 40)
            {
                double latrange = Math.Abs(NE.Latitude - SW.Latitude);
                double lngrange = Math.Abs(NE.Longitude - SW.Longitude);
                double lat_click = SW.Latitude + latrange * (mouse_position.y - 40)/1000;
                double lng_click = SW.Longitude + lngrange * (mouse_position.x - 460)/1000;
                clickedPos = new double[] {lat_click, lng_click};

                Debug.Log("selected point: lat= " + lat_click + ", long = " + lng_click);       
                continueButton.enabled = true;
                findClosestBuilding();
                if(correctBuilding)
                {
                    mark.enabled = true;
                    var markPos = mark.GetComponent<RectTransform>();
                    markPos.localPosition = new Vector3(mouse_position.x-960, mouse_position.y-540, 0);
                }
                else
                {
                    closestBuilding.draw(latrange, lngrange, LineRenderer, SW.Latitude, SW.Longitude);
                    //var unitycoord = closestBuilding.convertToUnityCoords(100000.0, Latitude, Longitude);
                    //Debug.Log(closestBuilding.checkClockwise(unitycoord));
                }
            }
        }
    }

    void OnEnable()
    {
        displayMap(Latitude, Longitude, zoom, size, scale);
        continueButton.enabled = false;
        continueButton.onClick.AddListener(onContinue);
        backButton.enabled = false;
        backButton.onClick.AddListener(onBack);
        mark = GameObject.Find("/API Object/Canvas/GoogleMap/Mark").GetComponent<RawImage>();
        mark.enabled = false;
        correctBuilding = false; 
        LineRenderer = GameObject.Find("/API Object/Canvas/GoogleMap/OSM layout").GetComponent<UnityEngine.UI.Extensions.UILineRenderer>();
        /*Debug.Log("amount of nodes: " + nodes.Count);
        Debug.Log("amount of buildings: " + OSMBuildings.Count);
        Debug.Log("amount of ways: " + ways.Count);*/
        BuildingMaker_Script = GameObject.Find("/Creator").GetComponent<BuildingMaker>();
        buildingCoordList = new List<buildingCoord>();

        /*
        Code below was used to find the conversion factor between unity distances, and actual distances in meter between two points
        The actual distances were 2440 m, and FFFF respectively 
        */
        //Debug.Log("Longitude constant, Y dist = " + (MercatorProjection.latToY(50.94875) - MercatorProjection.latToY(50.92687)));
        //Debug.Log("Latitude constant, X dist = " + (MercatorProjection.lonToX(3.03942) - MercatorProjection.lonToX(3.00479)));
        
    }

    void displayMap(double lat, double lng, int zoom, int size, int scale)
    {
        //url = "https://maps.googleapis.com/maps/api/staticmap?center=50.93575,3.00524&zoom=19&size=640x640&scale=1&key=AIzaSyD-msy6dK3rSTIPTiX1AVfnkc9wJxooeQs";
        URLBuild_map urlbuilder = new URLBuild_map(lat, lng, zoom, size, scale);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), img));
        Debug.Log(urlbuilder.getURL());
        Coordinate_GoogleMaps Coordinates = new Coordinate_GoogleMaps(Longitude, Latitude);         // care - first parameter is longitude
        var result = GoogleMapsAPI.GetBounds(Coordinates, zoom, size, size);
        SW = result.SouthWest;
        NE = result.NorthEast;
    }

    IEnumerator DownloadImage(string url, RawImage img)
    {
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
            }
            else
            {
                img.texture = DownloadHandlerTexture.GetContent(request);
            }
        }
    }

    void onContinue()
    {
        if(correctBuilding == false)
        {
            Text instruct = GameObject.Find("/API Object/Canvas/GoogleMap/Instruct/Text").GetComponent<Text>();
            instruct.text = "Please select the middle of the facade";
            LineRenderer.color = new Color(0.5f,0.5f,0.5f);
            correctBuilding = true;
            backButton.enabled = true;   
        }
        else
        {
            Vector2[] unityCoordsNodes = closestBuilding.convertToUnityCoords(100000.0, Latitude, Longitude);
            if(closestBuilding.checkClockwise(unityCoordsNodes) == false) BuildingMaker_Script.nodes = closestBuilding.rearrangeOrder(unityCoordsNodes);
            else BuildingMaker_Script.nodes = unityCoordsNodes;
            findClosestBuilding();
            BuildingMaker_Script.enabled = true;
            BuildingMaker_Script.facade = closestFacade;
            API APIscript = GameObject.Find("/API Object").GetComponent<API>();
            APIscript.lat = clickedPos[0];
            APIscript.lon = clickedPos[1];
            APIscript.enabled = true;
            var APImap_UI = GameObject.Find("/API Object/Canvas/GoogleMap");
            APImap_UI.SetActive(false);
            var FOV = GameObject.Find("/API Object/Canvas/FOV");
            FOV.SetActive(true);
            this.enabled = false;
        }
    }

    void onBack()
    {
        Text instruct = GameObject.Find("/API Object/Canvas/GoogleMap/Instruct/Text").GetComponent<Text>();
        instruct.text = "Please click on the the building";
        LineRenderer.color = new Color(0f,0f,0f);
        correctBuilding = false;
        mark.enabled = false;
        backButton.enabled = false;  
    }

    void OnDisable()
    {
        //var APImap_UI = GameObject.Find("/API Object/Canvas/GoogleMap");
        //APImap_UI.SetActive(false);
    }

    void findClosestBuilding()
    {
        foreach(KeyValuePair<Vector3, OsmBuilding> kvp in OSMBuildings)
        {
            buildingCoord building = new buildingCoord(kvp.Value.ID);
            List<ulong> nodeIDs = kvp.Value.NodeIDs;
            foreach(ulong id in nodeIDs)
            {
                OsmNode node = nodes[id];
                building.coords.Add(node.getlatlon());
            }
            buildingCoordList.Add(building);
        }

        buildingCoord closestBuildingCopy = new buildingCoord(0);
        double closestDistance = 1.0;
        double Cur_distance;
        closestFacade = -1;

        foreach(buildingCoord building in buildingCoordList)
        {
            for(int i = 0; i<building.coords.Count-1; i++)
            {
                Cur_distance = building.dist(clickedPos,building.coords[i],building.coords[i+1]);
                if(Cur_distance < closestDistance)
                {
                    closestDistance = Cur_distance;
                    closestBuildingCopy = building;
                    closestFacade = i+1;
                }
            }

            Cur_distance = building.dist(clickedPos,building.coords[0],building.coords[building.coords.Count-1]);
            if(Cur_distance < closestDistance)
            {
                closestDistance = Cur_distance;
                closestBuildingCopy = building;
                closestFacade = building.coords.Count-1;
            }
        }
    
        closestBuilding = closestBuildingCopy;
        Debug.Log("closest building: id = " + closestBuilding.id + ", facade = " + closestFacade);
    }
}

public class buildingCoord
{
    public ulong id;
    public List<double[]> coords;

    public buildingCoord(ulong id)
    {
        this.id = id;
        coords = new List<double[]>();
    }

    public double dist(double[] point, double[] coord1, double[] coord2)
    {
        double[] middle = {(coord1[0] + coord2[0])/2, (coord1[1] + coord2[1])/2};
        double distance = Math.Sqrt(Math.Pow(middle[0] - point[0],2) + Math.Pow(middle[1] - point[1],2));
        return distance;
    }

    public Vector2[] convertToUnityCoords(double scaling, double latCenter, double lngCenter)
    {
        List<double[]> coordsCopy = new List<double[]>();
        double minlat = coords[0][0];
        double minlng = coords[0][1];
        Vector2[] array = new Vector2[coords.Count];

        foreach(double[] coord in coords)
        {
            coordsCopy.Add(new double[] {coord[0],coord[1]});
            if(coord[0] < minlat)
            {
                minlat = coord[0];
            }
            if(coord[1] < minlng)
            {
                minlng = coord[1];
            }
        }

        Debug.Log("Minimum lat,lng:" + minlat + ", " + minlng);

        for(int i = 0; i < coords.Count; i++)   // translate to (0,0) and scale
        {
            /*
            NOTE - the functions are called "latToY" and "lonToX" but these return instead the "X" and "Z" values in unity coordinates respectively
            */

            coordsCopy[i][1] = MercatorProjection.latToY(coords[i][0]) - MercatorProjection.latToY(latCenter);  
            coordsCopy[i][0] = MercatorProjection.lonToX(coords[i][1]) - MercatorProjection.lonToX(lngCenter);  //-> MATERIAL SCALING FACTOR!

            array[i] = new Vector2((float)coordsCopy[i][0],(float)coordsCopy[i][1]);

            /*
            Code below was used to find the conversion factor between unity distances, and actual distances in meter between two points
            The actual distances were 2440 m, and FFFF respectively 
            */
            Debug.Log("Longitude constant, Y dist = " + (MercatorProjection.latToY(50.94875) - MercatorProjection.latToY(50.92687)));
            Debug.Log("Latitude constant, X dist = " + (MercatorProjection.lonToX(3.03942) - MercatorProjection.lonToX(3.00479)));

        }

        return array;
    }

    public void draw(double latrange, double lngrange, UnityEngine.UI.Extensions.UILineRenderer linerenderer, double SWlat, double SWlng)
    {
        List<Vector2> clickedPoints = new List<Vector2>();

        for(int i = 0; i<coords.Count; i++)
        {   
            double y = 1000 * (coords[i][0] - SWlat)/latrange - 500;
            double x = 1000 * (coords[i][1] - SWlng)/lngrange - 500;
            Vector2 pos = new Vector2((float)x, (float)y);
            clickedPoints.Add(pos);
        }

        linerenderer.Points = clickedPoints.ToArray();
    }

    public bool checkClockwise(Vector2[] nodes)
    {
        float degreesSum = 0;
        Vector2[] nodesCopy = new Vector2[nodes.Count()+2];
		Array.Copy(nodes, nodesCopy, nodes.Count());
        nodesCopy[nodesCopy.Count()-2] = nodes[0];
        nodesCopy[nodesCopy.Count()-1] = nodes[1];

        for (int i = 0; i<nodes.Count(); i++)
        {
            float angle = Vector2.SignedAngle(nodesCopy[i+2]-nodesCopy[i+1], nodesCopy[i+1]-nodesCopy[i]);
            degreesSum += angle;
            //Debug.Log("before, index " + i + " was: " + nodesCopy[i] + " and angle is: " + angle);
        }
        
        Debug.Log("clockwise if bigger than 0: "+degreesSum);
        if(degreesSum > 0) return true;
        return false;
    }

    public Vector2[] rearrangeOrder(Vector2[] nodes)
    {
        Vector2[] nodesCopy = new Vector2[nodes.Count()];
        for(int i = 0; i<nodes.Count(); i++)
        {
            //Debug.Log("for index: " + i + ", new nodes " + nodesCopy[i] + " became "+ nodes[nodes.Count()-1-i]);
            nodesCopy[i] = nodes[nodes.Count()-1-i];
        }
        return nodesCopy;
    }
}