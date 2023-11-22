using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JSON : MonoBehaviour
{
    public string path;

    // Start is called before the first frame update
    void Start()
    {
        path = Application.dataPath + @"\Python\Rectify\JSON.txt";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void writeToJson(Selection select)
    {
        File.WriteAllText(path, JsonUtility.ToJson(select,true));
    }

    public void prints(string top)
    {
        Debug.Log(top);
    }
}