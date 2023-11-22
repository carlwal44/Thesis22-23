using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;

enum Stage
{
    Python,
    Rectify,
    MachineLearning,
    Done,
}

public class Progress : MonoBehaviour
{
    public GameObject loading;
    public GameObject generate;

    public GameObject pyth;
    public GameObject rect;
    public GameObject ML;
    public GameObject Finished;
    public GameObject UI_facade;

    private string resourcePath;
    private string xmlPath;
    private Stage curstage;

    private float totalSecs;

    // Start is called before the first frame update
    void Start()
    {
        pyth.SetActive(false);
        rect.SetActive(false);
        ML.SetActive(false);
        resourcePath = Application.dataPath + "/Resources/Progress/";
        xmlPath = Application.dataPath + "/Resources/result.xml";

        curstage = Stage.Python;
        totalSecs = 0;

        Button btn = generate.transform.GetChild(2).gameObject.GetComponent<Button>();
        btn.onClick.AddListener(startGeneration);
        loading.SetActive(true);

        var select = GameObject.Find("/API Object/Canvas/SelectPoints");
        select.SetActive(false);
        InvokeRepeating("checkNextStage", 0.1f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //InvokeRepeating("checkNextStage", 0.1f, 0.5f);
        //checkNextStage(curstage);
    }

    void checkNextStage()
    {
        switch(curstage)
        {
            case Stage.Python:
                if(File.Exists(resourcePath + "pythonStarted.txt"))
                {
                    pyth.SetActive(true);
                    curstage = Stage.Rectify;
                    rect.SetActive(true);
                }
                break;

            case Stage.Rectify:
                if(File.Exists(resourcePath + "rectify.txt"))
                {
                    curstage = Stage.MachineLearning;
                    var img = rect.transform.GetChild(1).gameObject;
                    img.SetActive(true);

                    float seconds = float.Parse(File.ReadAllText(resourcePath + "rectify.txt"));
                    totalSecs += seconds;

                    Debug.Log(totalSecs);
                    var sectextobj = rect.transform.GetChild(2).gameObject;
                    sectextobj.SetActive(true);
                    Text sectext = sectextobj.GetComponent<Text>();
                    sectext.text = seconds.ToString("n2") + "s";

                    ML.SetActive(true);
                }
                break;

            case Stage.MachineLearning:
                if(File.Exists(resourcePath + "Machine Learning.txt"))
                {
                    curstage = Stage.Done;
                    var img = ML.transform.GetChild(1).gameObject;
                    img.SetActive(true);
                    loading.SetActive(false);
                    
                    float seconds = float.Parse(File.ReadAllText(resourcePath + "Machine Learning.txt"));
                    totalSecs += seconds;
                    var sectextobj = ML.transform.GetChild(2).gameObject;
                    sectextobj.SetActive(true);
                    Text sectext = sectextobj.GetComponent<Text>();
                    sectext.text = seconds.ToString("n2") + "s";

                    Finished.SetActive(true);
                    sectextobj = Finished.transform.GetChild(1).gameObject;
                    sectext = sectextobj.GetComponent<Text>();
                    sectext.text = totalSecs.ToString("n2") + " seconds";
                    
                    UI_facade.SetActive(true);
                    UI_facade UI_facade_script = GameObject.Find("/API Object/Canvas").GetComponent<UI_facade>();
                    UI_facade_script.enabled = true;
                }
                break;
        }
    }

    void startGeneration()
    {
        BuildingMaker buildingmaker = GameObject.Find("/Creator").GetComponent<BuildingMaker>();
        buildingmaker.Generate();
        CanvasElements canvaselement= GameObject.Find("API Object/Canvas").GetComponent<CanvasElements>();
        canvaselement.Toggle();
        string[] files = Directory.GetFiles(resourcePath);
        Array.ForEach(files,File.Delete);
    }

    void OnDisable()
    {
        string[] files = Directory.GetFiles(resourcePath);
        Array.ForEach(files,File.Delete);

        try
        {
            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
                Debug.Log("File deleted successfully.");
            }
            else
            {
                Debug.Log("File does not exist.");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("An error occurred: " + ex.Message);
        }
    }
}
