using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.IO;

public class API : MonoBehaviour
{
    // URL elements
    public double lat;
    public double lon;
    public double pitch;
    public int fov;
    public double heading;

    [HideInInspector]
    public string URL90;
    [HideInInspector]    
    public string URL120;

    // JSON & Pipeline
    JSON Jsonscript;
    PythonRunner PythonRunnerScript;
    public string pngPath;

    // UI elements
    [HideInInspector]
    public RawImage mapImage90;
    [HideInInspector]
    public RawImage mapImage120;
    [HideInInspector]
    public RawImage chosenImage;
    [HideInInspector]
    public Button but90;
    [HideInInspector]
    public Button but120;
    [HideInInspector]
    public Text pixelPos;
    [HideInInspector]
    public string chosenImgURL;

    public List<Vector2> clickedPoints;
    [HideInInspector]
    public List<Vector2> clickedPointsAdj;
    [HideInInspector]
    public UnityEngine.UI.Extensions.UILineRenderer LineRenderer;
    [HideInInspector]
    public Canvas canvas;
    private bool selecting;

    public string testurl;

    void Start()
    {
        
        mapImage90 = GameObject.Find("/API Object/Canvas/FOV/GoogleStreetView90").GetComponent<RawImage>();
        mapImage120 = GameObject.Find("/API Object/Canvas/FOV/GoogleStreetView120").GetComponent<RawImage>();

        // URL building
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(MetaDataImage(urlbuilder));

        // UI Elements
        Button btn = but90.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick90);
        btn.onClick.AddListener(startSelect);

        btn = but120.GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClick120);
        btn.onClick.AddListener(startSelect);

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Continue").GetComponent<Button>();
        btn.onClick.AddListener(startML);
        btn.interactable = false;

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Redo").GetComponent<Button>();
        btn.onClick.AddListener(redraw);

        btn = GameObject.Find("/API Object/Canvas/FOV/FOVinput/Continue").GetComponent<Button>();
        btn.onClick.AddListener(TaskOnClickChosen);
        btn.onClick.AddListener(startSelect);

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Arrows/left/Button").GetComponent<Button>();
        btn.onClick.AddListener(turnLeft);

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Arrows/right/Button").GetComponent<Button>();
        btn.onClick.AddListener(turnRight);

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Arrows/up/Button").GetComponent<Button>();
        btn.onClick.AddListener(lookUp);

        btn = GameObject.Find("/API Object/Canvas/SelectPoints/Arrows/down/Button").GetComponent<Button>();
        btn.onClick.AddListener(lookDown);

        // Point selection
        LineRenderer = GameObject.Find("/API Object/Canvas/SelectPoints/UI LineRenderer").GetComponent<UnityEngine.UI.Extensions.UILineRenderer>();
        clickedPoints = new List<Vector2>();
        clickedPointsAdj = new List<Vector2>();
        selecting = false;
        Jsonscript = GetComponent<JSON>();

        // Pipeline
        PythonRunnerScript = GameObject.Find("/PythonRunner").GetComponent<PythonRunner>();
        pngPath = Application.dataPath + @"\Python\Rectify\facade.png";

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouse_position = Input.mousePosition;
        int x = (int)Math.Floor(mouse_position.x) - 640;
        int y = (int)Math.Floor(mouse_position.y) - 220;

        pixelPos = GameObject.Find("Position").GetComponent<Text>();
        pixelPos.text = x.ToString() + "," + y.ToString();
        
        if(selecting) selectingRoutine(x,y);

    }

    void selectingRoutine(int x, int y)
    {
        if(Input.GetMouseButtonDown(0) && x < 640 && x > 0 && y < 640 && y > 0)
        {
            clickedPoints.Add(new Vector2(x, y));
            clickedPointsAdj.Add(new Vector2(x-320, y-320));
            LineRenderer.Points = clickedPointsAdj.ToArray();
            if(clickedPoints.Count == 4)
            {
                Image buttoncolor = GameObject.Find("/API Object/Canvas/SelectPoints/Continue").GetComponent<Image>();
                buttoncolor.color = Color.green;
                Button button = GameObject.Find("/API Object/Canvas/SelectPoints/Continue").GetComponent<Button>();
                button.interactable = true;
                clickedPointsAdj.Add(clickedPointsAdj[0]);
                LineRenderer.Points = clickedPointsAdj.ToArray();
                selecting = false;
            }
        }
        
    }
    void TaskOnClick90()
    {
        chosenImgURL = URL90;
        chosenImage = mapImage90;
        fov = 90;
        mapImage120.enabled = false;
        GameObject btn = GameObject.Find("/API Object/Canvas/FOV/Button120");
        btn.SetActive(false);
        btn = GameObject.Find("/API Object/Canvas/FOV/Button90");
        btn.SetActive(false);
        var img = GameObject.Find("GoogleStreetView90").GetComponent<RectTransform>();
        var pos = img.localPosition;
        img.localPosition = new Vector3(0, pos.y, pos.z);
    }

    void TaskOnClick120()
    {
        chosenImgURL = URL120;
        chosenImage = mapImage120;
        fov = 120;
        mapImage90.enabled = false;
        GameObject btn = GameObject.Find("/API Object/Canvas/FOV/Button90");
        btn.SetActive(false);
        btn = GameObject.Find("/API Object/Canvas/FOV/Button120");
        btn.SetActive(false);
        var img = GameObject.Find("GoogleStreetView120").GetComponent<RectTransform>();
        var pos = img.localPosition;
        img.localPosition = new Vector3(0, pos.y, pos.z);
    }

    void TaskOnClickChosen()
    {
        InputField input = GameObject.Find("/API Object/Canvas/FOV/FOVinput").GetComponent<InputField>();
        fov = int.Parse(input.text);
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), mapImage90));
        
        chosenImage = mapImage90;
        mapImage90.enabled = true;
        mapImage120.enabled = false;
        GameObject btn = GameObject.Find("/API Object/Canvas/FOV/Button120");
        btn.SetActive(false);
        btn = GameObject.Find("/API Object/Canvas/FOV/Button90");
        btn.SetActive(false);
        var img = GameObject.Find("GoogleStreetView90").GetComponent<RectTransform>();
        var pos = img.localPosition;
        img.localPosition = new Vector3(0, pos.y, pos.z);

        var texture2d = new Texture2D(mapImage90.texture.width, mapImage90.texture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(mapImage90.texture, texture2d);
        byte[] ToPng = texture2d.EncodeToPNG();
        File.WriteAllBytes(pngPath, ToPng);

    }

    void startSelect()
    {
        var selectpointsmessage = GameObject.Find("/API Object/Canvas/SelectPoints");
        selectpointsmessage.SetActive(true);

        var selectFOVmessage = GameObject.Find("/API Object/Canvas/FOV/SelectFOV");
        selectFOVmessage.SetActive(false);

        selecting = true;
    }

    void startML()
    {
        RawImage imgcolor = chosenImage.GetComponent<RawImage>();
        
        var texture2d = new Texture2D(chosenImage.texture.width, chosenImage.texture.height, TextureFormat.RGB24, false);
        Graphics.CopyTexture(chosenImage.texture, texture2d);
        byte[] ToPng = texture2d.EncodeToPNG();
        File.WriteAllBytes(pngPath, ToPng);
        imgcolor.color = Color.grey;
        
        JSONwrite();
        PythonRunnerScript.runML();
        GameObject progress = GameObject.Find("/API Object/Canvas/Progress");
        progress.SetActive(true);
        var FOVui = GameObject.Find("/API Object/Canvas/FOV");
        FOVui.SetActive(false);
    }

    void redraw()
    {
        Image buttoncolor = GameObject.Find("/API Object/Canvas/SelectPoints/Continue").GetComponent<Image>();
        buttoncolor.color = Color.white;
        Button button = GameObject.Find("/API Object/Canvas/SelectPoints/Continue").GetComponent<Button>();
        button.interactable = false;
        clickedPoints.Clear();
        clickedPointsAdj.Clear();
        LineRenderer.Points = new Vector2[4];
        selecting = true;
    }

    void getAPIresults()
    {
        
        if(testurl == "")
        {
            StartCoroutine(DownloadImage(URL90, mapImage90));
            StartCoroutine(DownloadImage(URL120, mapImage120));
        }
        else
        {
            StartCoroutine(DownloadImage(testurl, mapImage90));
        }
    }

    // Start is called before the first frame update
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

    IEnumerator MetaDataImage(URLbuild urlbuilder)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(urlbuilder.getURL_meta()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(request.error);
            }
            else
            {
                metaDataJSON data = JsonUtility.FromJson<metaDataJSON>(request.downloadHandler.text);
                if(data.status == "OK")
                {
                    heading = Coordinate.DegreeBearing(System.Convert.ToDouble(data.location.lat), System.Convert.ToDouble(data.location.lng), lat, lon);
                    urlbuilder.heading = heading.ToString();
                    urlbuilder.fov = "90";
                    URL90 = urlbuilder.getURL();
                    urlbuilder.fov = "120";
                    URL120 = urlbuilder.getURL();
                    getAPIresults();                    
                }
                else
                {
                    Debug.Log("error getting metadata");
                }
            }
        }
    }

    void turnLeft()
    {
        heading -= 10;
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), chosenImage));
    }

    void turnRight()
    {
        heading += 10;
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), chosenImage));
    }

    void lookUp()
    {
        pitch += 5;
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), chosenImage));
    }

    void lookDown()
    {
        pitch -= 5;
        URLbuild urlbuilder = new URLbuild(lat, lon, fov, heading, pitch);
        StartCoroutine(DownloadImage(urlbuilder.getURL(), chosenImage));
    }
    

    void JSONwrite()
    {
        Selection select = new Selection(chosenImgURL,clickedPoints);
        Jsonscript.writeToJson(select);
    }

}
