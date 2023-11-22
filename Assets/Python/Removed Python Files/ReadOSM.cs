using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class ReadOSM : MonoBehaviour
{
    public string osmFile;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void readOSM()
    {
        var txtAsset = Resources.Load<TextAsset>(osmFile);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);
    }
}
