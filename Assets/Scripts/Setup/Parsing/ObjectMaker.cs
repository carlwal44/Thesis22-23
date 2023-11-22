using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaker : MonoBehaviour
{
    protected Reader map;
    public GameObject bench;
    public GameObject tree;
    public GameObject street_lamp;
    public GameObject waste_basket;
    public GameObject bollard;
    public GameObject post_box;
    public GameObject bicycle_parking;
    public GameObject fountain;
    public GameObject vending_machine;
    public GameObject street_cabinet;
    public GameObject balloon;
    Dictionary<string, GameObject> objectDictionary;
    bool objectKeyPressed = false;

    void Awake()
    {
        // modellen zoeken voor degene die nog in het rood staan
        map = GetComponent<Reader>();
        objectDictionary = new Dictionary<string, GameObject>();
        objectDictionary.Add("bicycle_parking", bicycle_parking);
        objectDictionary.Add("bollard", bollard);
        objectDictionary.Add("bench", bench);
        objectDictionary.Add("waste_basket", waste_basket);
        objectDictionary.Add("tree", tree);
        objectDictionary.Add("street_lamp", street_lamp);
        objectDictionary.Add("post_box", post_box);
        objectDictionary.Add("fountain", fountain);
        objectDictionary.Add("vending_machine", vending_machine);
        objectDictionary.Add("street_cabinet", street_cabinet);
        objectDictionary.Add("balloon", balloon);

    }
    IEnumerator Start()
    {
        while (!map.IsReady || !objectKeyPressed)
        {
            yield return null;
        }


        Debug.Log("start object generation");

        foreach (OsmNode n in map.objects) {
            if(!objectDictionary.ContainsKey(n.getPrefabName())) Debug.Log(n.getPrefabName());
            GameObject go;
            if (n.getPrefabName() == "balloon")
            {
                go = Instantiate(objectDictionary[n.getPrefabName()], new Vector3(0, 35, 0), n.getRotation());
            }
            else
            {
                go = Instantiate(objectDictionary[n.getPrefabName()], n.getPosition(), n.getRotation());
            }
            go.transform.localScale = n.getScale();
            go.name = n.getPrefabName();
        }

        Debug.Log("object generation complete");
    }

    public void SetObjectKeyPressed()
    {
        objectKeyPressed = true;
    }

}