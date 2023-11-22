using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Globalization;
using System.Linq;
using UnityEditor;


public class Building : MonoBehaviour
{
    public List<WindowStruct> windows;
    public List<DoorStruct> doors;
    //private string XMLfile;
    public Color wallColor;
    public Color windowColor;
    private TextAsset xml;

    // Start is called before the first frame update
    void Start()
    {
        //XMLfile = "result";
        //readXml();
    }

    public void readXML()
    {
        StartCoroutine(loadXML());
        Debug.Log("coroutine started!!");
    }
    public IEnumerator loadXML()
    {
        windows = new List<WindowStruct>();
        doors = new List<DoorStruct>();
        var txtAsset = Resources.Load<TextAsset>("result");
        while(txtAsset == null)
        {
            Debug.Log("file not found yet");
            yield return new WaitForSeconds(0.1f);
            txtAsset = Resources.Load<TextAsset>("result");
        }
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);
        XmlNode root = doc.DocumentElement;
        
        bool maxEtageReached = false;
        string img = "image/etage";
        int curEtage = 0;

        // Reading in the RGB-values

        float R = float.Parse(root["image"]["facade_color"]["R"].InnerText)/255f;
        float G = float.Parse(root["image"]["facade_color"]["G"].InnerText)/255f;
        float B = float.Parse(root["image"]["facade_color"]["B"].InnerText)/255f;

        wallColor = new Color(R,G,B);
        Debug.Log("wallcolour is " + R + "," + G + "," + B);

        R = float.Parse(root["image"]["window_color"]["R"].InnerText)/255f;
        G = float.Parse(root["image"]["window_color"]["G"].InnerText)/255f;
        B = float.Parse(root["image"]["window_color"]["B"].InnerText)/255f;

        windowColor = new Color(R,G,B);

        // Looping through etages

        while(maxEtageReached == false)
        {
            XmlNodeList xmlWindows = root.SelectNodes(img + curEtage.ToString() + "/window");
            if(xmlWindows.Count == 0)
            {
                maxEtageReached = true;
                break;
            }

            foreach (XmlNode node in xmlWindows)
            {
                WindowStruct window = new WindowStruct();
                window.id = System.Convert.ToInt32(node["window_number"].InnerText);
                window.topleftX = System.Convert.ToDouble(node["position"]["left_under_corner"]["width"].InnerText);
                window.topleftY = System.Convert.ToDouble(node["position"]["left_under_corner"]["height"].InnerText);
                window.height = System.Convert.ToDouble(node["size"]["height"].InnerText);
                window.width = System.Convert.ToDouble(node["size"]["width"].InnerText);

                // TODO - PYTHON FILES -> put window type in XML file 
                //window.type = "wideWindow";
                window.type = node["type"].InnerText;
                Debug.Log("okay, window type is: " + window.type);
                windows.Add(window);
            }

            curEtage++;
        }

        UI_facade facadelayout = GameObject.Find("API Object/Canvas").GetComponent<UI_facade>();
        facadelayout.showLayout();
    }

    /*public void readXml()
    {
        Debug.Log("XML file is being read");
        windows = new List<WindowStruct>();
        doors = new List<DoorStruct>();
        var txtAsset = Resources.Load<TextAsset>("result");

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);
        XmlNode root = doc.DocumentElement;
        
        bool maxEtageReached = false;
        string img = "image/etage";
        int curEtage = 0;

        float R = 255f/float.Parse(root["image"]["facade_color"]["R"].InnerText);
        float G = 255f/float.Parse(root["image"]["facade_color"]["G"].InnerText);
        float B = 255f/float.Parse(root["image"]["facade_color"]["B"].InnerText);

        wallColor = new Color(R,G,B);

        while(maxEtageReached == false)
        {
            XmlNodeList xmlWindows = root.SelectNodes(img + curEtage.ToString() + "/window");
            if(xmlWindows.Count == 0)
            {
                maxEtageReached = true;
                break;
            }

            foreach (XmlNode node in xmlWindows)
            {
                WindowStruct window = new WindowStruct();
                window.id = System.Convert.ToInt32(node["window_number"].InnerText);
                window.topleftX = System.Convert.ToDouble(node["position"]["left_under_corner"]["width"].InnerText);
                window.topleftY = System.Convert.ToDouble(node["position"]["left_under_corner"]["height"].InnerText);
                window.height = System.Convert.ToDouble(node["size"]["height"].InnerText);
                window.width = System.Convert.ToDouble(node["size"]["width"].InnerText);

                // TODO - PYTHON FILES -> put window type in XML file 
                window.type = "wideWindow";

                windows.Add(window);
            }

            curEtage++;
        }
    }*/
}

public struct WindowStruct
{
    public WindowStruct(string id, string type, string height, string width, string topleftX, string topleftY)
    {
        this.id = System.Convert.ToInt32(id);
        this.type = type;
        this.height = System.Convert.ToDouble(height);
        this.width = System.Convert.ToDouble(width);
        this.topleftX = System.Convert.ToDouble(topleftX);
        this.topleftY = System.Convert.ToDouble(topleftY);
    }

    public int id { get; set; }
    public string type {get; set;}
    public double height { get; set; }
    public double width { get; set; }
    public double topleftX { get; set; }
    public double topleftY { get; set; }
}

public struct DoorStruct
{
    public DoorStruct(string id, string type, string height, string width, string topleftX, string topleftY)
    {
        this.id = System.Convert.ToInt32(id);
        this.type = type;
        this.height = System.Convert.ToDouble(height);
        this.width = System.Convert.ToDouble(width);
        this.topleftX = System.Convert.ToDouble(topleftX);
        this.topleftY = System.Convert.ToDouble(topleftY);
    }

    public int id { get; set; }
    public string type {get; set;}
    public double height { get; set; }
    public double width { get; set; }
    public double topleftX { get; set; }
    public double topleftY { get; set; }
}

public struct FacadeStruct
{
    public FacadeStruct(int id, Vector2 right, Vector2 left, double height)
    {
        this.id = id;
        this.right = right;
        this.left = left;
        this.width = Vector2.Distance(right, left);
        this.height = height;
        this.area = width * height;

    }
    public int id { get; set; }
    public Vector2 right { get; set; }
    public Vector2 left { get; set; }
    public double width { get; set; }
    public double height { get; set; }
    public double area { get; set; }
}

public struct WindowInformation
{
    public float width;
    public float height;
    public float depth;
    public float MinHorizontalGap;
    public float MaxHorizontalGap;
    public float horizontalCenterOfGravity;
    public float verticalCenterOfGravity;
    public GameObject model;

    public int xoff;
    public int yoff;

    public WindowInformation(float w, float h, float d, float g, float gm, float hz, float vz, int xoffset, int yoffset, GameObject model)
    {
        width = w;
        height = h;
        depth = d;
        MinHorizontalGap = g;
        MaxHorizontalGap = gm;
        horizontalCenterOfGravity = hz;
        verticalCenterOfGravity = vz;
        this.model = model;
        xoff = xoffset;
        yoff = yoffset;
    }
}

public struct DoorInformation
{
    public float width;
    public float height;
    public float minDepth;
    public float maxDepth;
    public float horizontalCenterOfGravity;
    public float verticalCenterOfGravity;
    public GameObject model;

    public int xoff;
    public int yoff;

    public DoorInformation(float w, float h, float mind, float maxd, float hz, float vz, int xoffset, int yoffset, GameObject model)
    {
        width = w;
        height = h;
        minDepth = mind;
        maxDepth = maxd;
        horizontalCenterOfGravity = hz;
        verticalCenterOfGravity = vz;
        this.model = model;
        xoff = xoffset;
        yoff = yoffset;
    }
}
