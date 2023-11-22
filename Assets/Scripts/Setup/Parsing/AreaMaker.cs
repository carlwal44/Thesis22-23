using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaMaker : InfrastructureBehaviour
{
    [HideInInspector]
    public float offset = 0;
    [HideInInspector]
    Vector2 leftTopUV;
    [HideInInspector]
    Vector2 rightBottomUV;
    [HideInInspector]
    List<Vector2> uvs;

    public GameObject weed1;
    public GameObject weed2;
    public GameObject grass;
    public GameObject bush;
    public GameObject tree1;
    public GameObject tree2;
    public GameObject tree3;
    private GameObject[] vegetables;
    bool areaKeyPressed = false;


    IEnumerator Start()
    {
        while (!map.IsReady || !areaKeyPressed)
        {
            yield return null;
        }

        Debug.Log("start painting areas");

        vegetables = new GameObject[7];
        vegetables[0] = weed1;
        vegetables[1] = weed2;
        vegetables[2] = grass;
        vegetables[3] = bush;
        vegetables[4] = tree1;
        vegetables[5] = tree2;
        vegetables[6] = tree3;

        List<Area> temp = map.areas;
        temp.Sort(SortHierarchy);
        foreach (Area area in temp) 
        {
            TerrainPainter terrainPainter = map.terrainMaker.to.GetComponent<TerrainPainter>();
            List<VegetationStruct> bushes = terrainPainter.PaintArea(area);
            foreach (VegetationStruct v in bushes)
            {
                Instantiate(vegetables[v.index], v.position - map.bounds.Centre, Quaternion.Euler(0, 0, 0f));
            }
        }

        yield return null;
        Debug.Log("done painting areas");
    }

    private static int SortHierarchy(Area area1, Area area2)
    {
        if (area1.GetSurfaceArea() < area2.GetSurfaceArea()) return 1;
        if (area1.GetSurfaceArea() > area2.GetSurfaceArea()) return -1;
        return 0;
    }
    public void SetAreaKeyPressed()
    {
        areaKeyPressed = true;
    }
}