using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_facade : MonoBehaviour
{
    public GameObject facade; // Assign in inspector
    //public UnityEngine.UI.Extensions.UIPolygon UIPolygonPrefab;
    public Image UIpre;
    private List<WindowStruct> windows;
    private List<DoorStruct> doors;
    public float margin;

    // positioning elements
    private Vector3 left_under_corner;
    private Vector3 center;
    private float size;
    private float length;
    private float height;

    // Start is called before the first frame update
    void Start()
    {
        Building buildingScript = GameObject.Find("Gebouw").GetComponent<Building>();
        //buildingScript.readXml();
        buildingScript.readXML();

        windows = buildingScript.windows;
        doors = buildingScript.doors;

        getParams();
        scale();
        //showLayout();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(windows.Count);
    }

    public void showLayout()
    {
        Image UI_outline = Instantiate(UIpre);
        UI_outline.name = "Outline";
        //UI_outline.transform.parent = facade.transform;
        UI_outline.transform.SetParent(facade.transform);
        var recttr = UI_outline.GetComponent<RectTransform>();
        var pos = recttr.localPosition;
        recttr.localPosition = center;
        recttr.localScale = new Vector3(length,height,1);
        

        for(int i = 0; i < windows.Count; i++)
        {
            Image UI_window = Instantiate(UIpre);
            UI_window.name = "window - " + i.ToString();
            //UI_window.transform.parent = facade.transform;
            UI_window.transform.SetParent(facade.transform);
            UI_window.color = Color.white;

            WindowStruct window = windows[i];
            float scaleX = (float)window.width * length;
            float scaleY = (float)window.height * height;
            UI_window.GetComponent<RectTransform>().localScale = new Vector3(scaleX,scaleY,1);
            float posX = left_under_corner.x + (float)window.topleftX * length + scaleX/2;
            float posY = left_under_corner.y + (float)window.topleftY * height + scaleY/2;
            UI_window.GetComponent<RectTransform>().localPosition = new Vector3(posX,posY,0);

        }
    }

    void getParams()
    {
        GameObject background = facade.gameObject.transform.GetChild(0).gameObject;
        Rect rect = background.GetComponent<RectTransform>().rect;
        margin = 20;
        size = rect.width;
        center = background.GetComponent<RectTransform>().localPosition;

        BuildingMaker BuildingMaker_Script = GameObject.Find("/Creator").GetComponent<BuildingMaker>();
        int facadeId = BuildingMaker_Script.facade;
        //Debug.Log("UI: facadeID is " + facadeId + " and there are so many facades: " + BuildingMaker_Script.facades.Count);

        length = (float)BuildingMaker_Script.facades[facadeId-1].width;
        height = (float)BuildingMaker_Script.facades[facadeId-1].height;
    }

    void scale()
    {
        float longest = length>height ? length : height;
        float scalingfactor = longest/(size - 2*margin);
        length = length/scalingfactor;
        height = height/scalingfactor;
        left_under_corner = new Vector3(center.x - length/2,center.y - height/2, 0);
    }
}
