using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

public class BuildingMaker : MonoBehaviour
{
    [Tooltip("enter X,Y points manually")]
    public Vector2[] nodes;
    public double height;
    public float roofHeight;
    [HideInInspector] 
    public float materialScalingFactor;
    public int facade;
    [HideInInspector]
    public List<FacadeStruct> facades;
    //public Color defaultWallColor;

    public Dictionary<string, DoorInformation> doorInformationDictionary = new Dictionary<string, DoorInformation>();
    public Dictionary<string, WindowInformation> windowInformationDictionary = new Dictionary<string, WindowInformation>();

    //public GameObject wideWindow;
    //public GameObject normalBigWindow;
    //public GameObject normalWindow0;
    //public GameObject normalWindow1;
    //public GameObject normalWindow2;
    //public GameObject normalWindow3;
    public GameObject door0;
    //public GameObject door1;
    //public GameObject door2;
    //public GameObject door3;

    public GameObject type1;
    public GameObject type2;
    public GameObject type3;
    public GameObject type4;
    public GameObject type5;


    void Start()
    {
        materialScalingFactor = 0.05f;

        GameObject gebouw = new GameObject("Gebouw");
        gebouw.AddComponent<Building>();
        GameObject roof = new GameObject("Roof");
        roof.transform.parent = gebouw.transform;
        GameObject ceiling = new GameObject("Ceiling");
        ceiling.transform.parent = gebouw.transform;

        nodes = RemoveRedundantVertices(nodes);
        CreateWalls(gebouw);
        if(nodes.Count() == 4) complexRoof_4p_1(roof, roofHeight, nodes, checkLongest(nodes));
        if(nodes.Count() == 6) complexRoof_6p_1(roof, roofHeight, nodes);
        createFlatRoof(ceiling);
        setMaterial(gebouw);
        setParams();

        // set camera facing building
        MainCamera maincamera = GameObject.Find("Main Camera").GetComponent<MainCamera>();
        maincamera.setCamera(nodes);
    }

    void Update()
    {
        if (Input.GetKeyDown("space")) Generate();
    }

    public void Generate()
    {
        GameObject gebouw = GameObject.Find("Gebouw");
        Building buildingScript = gebouw.GetComponent<Building>();

        List<WindowStruct> windowsToSet = buildingScript.windows;
        List<DoorStruct> doorsToSet = buildingScript.doors;
        setColor(gebouw, buildingScript.wallColor);
        if(nodes.Count() == 4){
            setColor(GameObject.Find("Gebouw/Roof/leftside"), buildingScript.wallColor);
            setColor(GameObject.Find("Gebouw/Roof/rightside"), buildingScript.wallColor);
        }
        if(nodes.Count() == 6){
            setColor(GameObject.Find("Gebouw/Roof/roof - left/roof - sides"), buildingScript.wallColor);
            setColor(GameObject.Find("Gebouw/Roof/roof - right/roof - sides"), buildingScript.wallColor);
        }

        placeWindows(gebouw, windowsToSet, facade, buildingScript.windowColor);
        placeDoors(gebouw, doorsToSet, 1);
    }

    void CreateWalls(GameObject gebouwInst)
    {
        MeshFilter meshFilter = gebouwInst.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int[] toAddTris = { 0, 1, 3, 0, 3, 2 };
        List<Vector3> normals = new List<Vector3>();

        for (int i = 1; i < nodes.Length; i++)
        {
            vertices.Add(new Vector3(nodes[i - 1].x, 0, nodes[i - 1].y));
            vertices.Add(new Vector3(nodes[i].x, 0, nodes[i].y));
            vertices.Add(new Vector3(nodes[i - 1].x, (float)height, nodes[i - 1].y));
            vertices.Add(new Vector3(nodes[i].x, (float)height, nodes[i].y));

            tris.AddRange(toAddTris);
            for (int j = 0; j < 6; j++)
            {
                toAddTris[j] = toAddTris[j] + 4;
            }

            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);
            normals.Add(-Vector3.forward);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
        }

        // Connect the first and last wall
        vertices.Add(new Vector3(nodes[nodes.Length-1].x, 0, nodes[nodes.Length-1].y));
        vertices.Add(new Vector3(nodes[0].x, 0, nodes[0].y));
        vertices.Add(new Vector3(nodes[nodes.Length-1].x, (float)height, nodes[nodes.Length-1].y));
        vertices.Add(new Vector3(nodes[0].x, (float)height, nodes[0].y));
        tris.AddRange(toAddTris);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = uvs.ToArray();

        meshFilter.mesh = mesh;

        facades = new List<FacadeStruct>();
        for (int j = 1; j < nodes.Length; j++)
            {
            facades.Add(new FacadeStruct(j, nodes[j - 1], nodes[j], height));
            }
        facades.Add(new FacadeStruct(facades.Count+1, nodes[nodes.Length-1], nodes[0], height));
    }

    void setMaterial(GameObject gebouwInst)
    {
        float totalLength = 0;
        for(int j = 1; j<nodes.Length; j++)
        {
            totalLength = totalLength + Vector2.Distance(nodes[j-1], nodes[j]);
        }
        totalLength = totalLength + Vector2.Distance(nodes[nodes.Length-1], nodes[0]);

        MeshRenderer meshRenderer = gebouwInst.AddComponent<MeshRenderer>();
        //Material mat = Resources.Load<Material>("Materials/Materials/brighter") as Material;
        Material mat = Resources.Load<Material>("Materials/Materials/white") as Material;
        if (mat == null) { print("Material Not Found"); };
        //mat.mainTextureScale = new Vector2(materialScalingFactor*totalLength, (float)height);
        mat.mainTextureScale = new Vector2(2f,2f);
        meshRenderer.sharedMaterial = mat;
        meshRenderer.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void setColor(GameObject gebouwInst, Color color)
    {
        MeshRenderer meshRenderer = gebouwInst.GetComponent<MeshRenderer>();
        meshRenderer.material.color = color;
    }

    void createFlatRoof(GameObject roofobj)
    {
        MeshFilter meshFilter = roofobj.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        Triangulator tr = new Triangulator(nodes, false);
        int[] toAddTris = tr.Triangulate();

        for (int i = 0; i < nodes.Length; i++)
        {
            vertices.Add(new Vector3(nodes[i].x,(float)height,nodes[i].y));
        }

        tris.AddRange(toAddTris);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        if(vertices.Count() == 3)
        {
            mesh.uv = Unwrapping.GeneratePerTriangleUV(mesh);
        }
        else if(vertices.Count() == 4)
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));  
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = uvs.ToArray();

        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = roofobj.AddComponent<MeshRenderer>();
        Material mat = Resources.Load<Material>("Materials/Materials/flatroof") as Material;
        if (mat == null) { print("Material Not Found"); };
        meshRenderer.sharedMaterial = mat;
        meshRenderer.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void complexRoof_4p_1(GameObject roofobj, float roofRidgeHeight, Vector2[] nodeList, bool perp)
    {
        //GameObject roofsides = new GameObject("roof - sides");
        //roofsides.transform.parent = roofobj.transform;
        GameObject roofact = new GameObject("roof");
        roofact.transform.parent = roofobj.transform;
        GameObject leftside = new GameObject("leftside");   //
        leftside.transform.parent = roofobj.transform;    //
        GameObject rightside = new GameObject("rightside");   //
        rightside.transform.parent = roofobj.transform;    //


        MeshFilter meshFilter = roofact.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();

        if(perp)
        {
            Vector2 last = nodeList[3];
            nodeList[3] = nodeList[2];
            nodeList[2] = nodeList[1];
            nodeList[1] = nodeList[0];
            nodeList[0] = last;
        }

        List<int> trisperp = new List<int>() { 0, 5, 3, 4, 5, 0, 1, 2, 4, 4, 2, 5 };
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        Triangulator tr = new Triangulator(nodeList, false);
        Vector3[] ridge = tr.getRoofRidge(nodeList,(float)height + roofRidgeHeight,true);

        vertices.Add(new Vector3(nodeList[0].x, (float)height, nodeList[0].y));
        vertices.Add(new Vector3(nodeList[1].x, (float)height, nodeList[1].y));
        vertices.Add(new Vector3(nodeList[2].x, (float)height, nodeList[2].y));
        vertices.Add(new Vector3(nodeList[3].x, (float)height, nodeList[3].y));
        vertices.Add(ridge[0]);
        vertices.Add(ridge[1]);

        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);
        normals.Add(-Vector3.forward);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));  
        uvs.Add(new Vector2(0, 0.5f));
        uvs.Add(new Vector2(1, 0.5f));  
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = trisperp.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = uvs.ToArray();

        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = roofact.AddComponent<MeshRenderer>();
        Material mat = Resources.Load<Material>("Materials/Materials/3529") as Material;
        if (mat == null) { print("Material Not Found"); };
        mat.mainTextureScale = new Vector2(5f,5f);
        meshRenderer.sharedMaterial = mat;
        meshRenderer.material.mainTexture.wrapMode = TextureWrapMode.Repeat;

        // Side 1 roof/facade (left)
        MeshFilter meshFilter1 = leftside.AddComponent<MeshFilter>();
        Mesh mesh1 = new Mesh();
        List<Vector3> vertices1 = new List<Vector3>();
        vertices1.Add(vertices[0]);
        vertices1.Add(vertices[1]);
        vertices1.Add(vertices[4]);
        List<int> tris1 = new List<int>() { 0, 1, 2};

        uvs = new List<Vector2>();
        uvs.Add(new Vector2(1, 0.6f));
        uvs.Add(new Vector2(0, 0.6f));
        uvs.Add(new Vector2(0.5f, 0.3f));

        mesh1.vertices = vertices1.ToArray();
        mesh1.triangles = tris1.ToArray();
        mesh1.RecalculateNormals();
        mesh1.uv = uvs.ToArray();
        meshFilter1.mesh = mesh1;
        MeshRenderer meshRenderer1 = leftside.AddComponent<MeshRenderer>();
        mat = Resources.Load<Material>("Materials/Materials/white") as Material;
        if (mat == null) { print("Material Not Found"); };
        meshRenderer1.sharedMaterial = mat;
        float roofTextureScale = 6f * roofHeight/(float)height;
        mat.mainTextureScale = new Vector2(2f,roofTextureScale);
        //meshRenderer1.material.mainTexture.wrapMode = TextureWrapMode.Repeat;

        // Side 2 roof/facade (right)
        MeshFilter meshFilter2 = rightside.AddComponent<MeshFilter>();
        Mesh mesh2 = new Mesh();
        List<Vector3> vertices2 = new List<Vector3>();
        vertices2.Add(vertices[2]);
        vertices2.Add(vertices[3]);
        vertices2.Add(vertices[5]);
        List<int> tris2 = new List<int>() { 0, 1, 2};

        mesh2.vertices = vertices2.ToArray();
        mesh2.triangles = tris2.ToArray();
        mesh2.RecalculateNormals();
        mesh2.uv = uvs.ToArray();
        meshFilter2.mesh = mesh2;
        MeshRenderer meshRenderer2 = rightside.AddComponent<MeshRenderer>();
        meshRenderer2.sharedMaterial = mat;
        meshRenderer2.material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    void complexRoof_6p_1(GameObject roofobj, float roofRidgeHeight, Vector2[] nodeList)
    {
        Triangulator tr = new Triangulator(nodeList, false);
        int convex = tr.returnConvexPoint(nodeList);
        Debug.Log("convex point: " + convex);
        if(convex < 0)      // shape is convex: draw roof rig along longest path
        {
            int bestindex = tr.getSplitConvex(nodeList);        // find point(s) to connect which achieve a long rigged roof
            Vector2[] left = tr.getLeft(nodeList, bestindex);       // roof 1
            Vector2[] right = tr.getRight(nodeList, bestindex+3);   // opposite point - roof 2

            GameObject roofleft = new GameObject("roof - left");
            roofleft.transform.parent = roofobj.transform;
            GameObject roofright = new GameObject("roof - right");
            roofright.transform.parent = roofobj.transform;
            complexRoof_4p_1(roofleft, roofRidgeHeight, left, true);
            complexRoof_4p_1(roofright, roofRidgeHeight, right, true);

        }
        else
        {
            Vector2[] left = tr.getLeft(nodeList, convex);       // roof 1
            Vector2[] right = tr.getRight(nodeList, convex+3); 

            GameObject roofleft = new GameObject("roof - left");
            roofleft.transform.parent = roofobj.transform;
            GameObject roofright = new GameObject("roof - right");
            roofright.transform.parent = roofobj.transform;
            complexRoof_4p_1(roofleft, roofRidgeHeight, left, true);
            complexRoof_4p_1(roofright, roofRidgeHeight, right, true);
        }
    }

    void placeWindows(GameObject gebouwInst, List<WindowStruct> windowList, int facadeId, Color windowCol)
    {
        int facadeIndex = getFacadeIndex(facadeId);
        Debug.Log("windowList counts: " + windowList.Count);
        Debug.Log("facadeindex is: " + facadeIndex);
        
        Material mat = Resources.Load<Material>("DoorsAndWindows/materials/Windows01") as Material;
        mat.color = windowCol;
        mat = Resources.Load<Material>("DoorsAndWindows/materials/Windows02") as Material;
        mat.color = windowCol;
        mat = Resources.Load<Material>("DoorsAndWindows/materials/Windows03") as Material;
        mat.color = windowCol;
        mat = Resources.Load<Material>("DoorsAndWindows/materials/Windows04") as Material;
        mat.color = windowCol;


        for (int i = 0; i < windowList.Count; i++) 
        {
            // get facade parameters
            Vector3 startPos = facades[facadeIndex].left;
            startPos.z = startPos.y;    // swap y and z values, incorrect dimensioning from vector2 to vector3
            startPos.y = 0;
            Vector3 rightwards = facades[facadeIndex].right - facades[facadeIndex].left;
            rightwards.Normalize();
            rightwards.z = rightwards.y;    // swap y and z values, incorrect dimensioning from vector2 to vector3
            rightwards.y = 0;
            Vector3 upwards = Vector3.up;
            Vector3 forwards = Vector3.Cross(upwards,rightwards);
        
            double widthScale = facades[facadeIndex].width;
            double heightScale = facades[facadeIndex].height;

            // get window model parameters
            string type = windowList[i].type;
            WindowInformation win = windowInformationDictionary[type];

            float modelWidth = win.width;
            float modelHeight = win.height;

            WindowStruct window = windowList[i];
            int id = window.id;
            string editorName = "facade - " + facadeIndex.ToString() +  " window - " + id.ToString();

            // Create
            GameObject newWindow = Instantiate(win.model); 
            newWindow.name = editorName;
            newWindow.transform.parent = gebouwInst.transform;

            // Scale
            var coll = newWindow.GetComponent<BoxCollider>();
            Vector3 scale = new Vector3(1,1,1);
            scale.y = (float)(heightScale * window.height)/coll.size.y;
            scale.z = (float)(widthScale * window.width)/coll.size.z;
            scale.x = win.depth;
            newWindow.transform.localScale = scale;

            // Rotation
            float angle = Mathf.Rad2Deg * Mathf.Atan(rightwards.x / rightwards.z);
            newWindow.transform.Rotate(0, angle, 0, Space.Self);
            if(forwards.x < 0)
            {
                newWindow.transform.Rotate(0,180, 0, Space.Self);
            }

            // Position
            newWindow.transform.position = startPos + ((float)windowList[i].topleftX * (float)widthScale + win.xoff * scale.z * coll.size.z) * rightwards;                                        
            newWindow.transform.position = newWindow.transform.position + ((float)windowList[i].topleftY * (float)heightScale + win.yoff * scale.y * coll.size.y) * upwards;
            newWindow.transform.position = newWindow.transform.position + win.MinHorizontalGap * scale.x * forwards;
            
            // Colour - todo

        }
    }

    void placeDoors(GameObject gebouwInst, List<DoorStruct> doorList, int facadeId)
    {
        int facadeIndex = getFacadeIndex(facadeId);

        for (int i = 0; i < doorList.Count; i++)
        {
            // get facade parameters
            Vector3 startPos = facades[facadeIndex].left;
            startPos.z = startPos.y;    // swap y and z values, incorrect dimensioning from vector2 to vector3
            startPos.y = 0;
            Vector3 rightwards = facades[facadeIndex].right - facades[facadeIndex].left;
            rightwards.Normalize();
            rightwards.z = rightwards.y;    // swap y and z values, incorrect dimensioning from vector2 to vector3
            rightwards.y = 0;
            Vector3 upwards = Vector3.up;
            Vector3 forwards = Vector3.Cross(upwards,rightwards);
        
            double widthScale = facades[facadeIndex].width;
            double heightScale = facades[facadeIndex].height;

            // get door model parameters
            string type = doorList[i].type;
            DoorInformation doorinf = doorInformationDictionary[type];

            float modelWidth = doorinf.width;
            float modelHeight = doorinf.height;

            DoorStruct door = doorList[i];
            int id = door.id;
            string editorName = "facade - " + facadeIndex.ToString() + " door - " + id.ToString();

            // Create
            GameObject newDoor = Instantiate(door0); // TODO: select correct door model via window.type
            newDoor.name = editorName;
            newDoor.transform.parent = gebouwInst.transform;

            // Scale
            var coll = newDoor.GetComponent<BoxCollider>();
            Vector3 scale = new Vector3(1,1,1);
            scale.y = (float)(heightScale * door.height)/coll.size.y;
            scale.z = (float)(widthScale * door.width)/coll.size.z;
            //scale.x = door.depth;
            newDoor.transform.localScale = scale;

            // Rotation
            float angle = Mathf.Rad2Deg * Mathf.Atan(rightwards.x / rightwards.z);
            newDoor.transform.Rotate(0, angle, 0, Space.Self);
            if(forwards.x < 0)
            {
                newDoor.transform.Rotate(0,180, 0, Space.Self);
            }

            // Position
            newDoor.transform.position = startPos + ((float)doorList[i].topleftX * (float)widthScale + doorinf.xoff * scale.z * coll.size.z) * rightwards;                                        
            newDoor.transform.position = newDoor.transform.position + ((float)doorList[i].topleftY * (float)heightScale + doorinf.yoff * scale.y * coll.size.y) * upwards;
            //newDoor.transform.position = newDoor.transform.position + doorinf.MinHorizontalGap * scale.x * forwards;
        }
    }
    
    int getFacadeIndex(int facadeId)
    {
        int indexF = -1;
        for (int j = 0; j < facades.Count; j++)
        {
            Debug.Log("facade id is: " + facades[j].id);
            if (facades[j].id == facadeId)
            {
                indexF = j;
            }
        }
        if (indexF == -1) { Debug.Log("facadeId " + facadeId + " didn't match"); };
        return indexF;
    }

    void setParams()
    {
        // window models - width, height, depth, mingap, maxgap, horizontal grav, vertical grav

        /*windowInformationDictionary.Add("wideWindow", new WindowInformation(3.5f, 1.8f, 0.35f,-0.15f,2.5f,1.75f,0.9f,1,1, wideWindow));
        windowInformationDictionary.Add("normalBigWindow", new WindowInformation(3f, 2.25f, 0.35f,-0.1f,2.5f, 1.5f,1.12f,1,0, normalBigWindow));
        windowInformationDictionary.Add("normalWindow0", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,0,1, normalWindow0));
        windowInformationDictionary.Add("normalWindow1", new WindowInformation(3f, 2.25f, 0.35f,-0.1f,2.5f, 1.5f,1.12f,1,0, normalWindow1));
        windowInformationDictionary.Add("normalWindow2", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,0,0, normalWindow2));
        windowInformationDictionary.Add("normalWindow3", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,0,0, normalWindow3));*/

        windowInformationDictionary.Add("class_1", new WindowInformation(3f, 2.25f, 0.35f,-0.1f,2.5f, 1.5f,1.12f,1,1, type1));
        windowInformationDictionary.Add("class_2", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,1,0, type2));
        windowInformationDictionary.Add("class_3", new WindowInformation(3f, 2.25f, 0.35f,-0.1f,2.5f, 1.5f,1.12f,1,1, type3));  //window 25 - 1,0
        windowInformationDictionary.Add("class_4", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,1,0, type4));
        windowInformationDictionary.Add("class_5", new WindowInformation(3f, 2.25f, 0.35f,0.1f,2.5f, 1.5f,1.12f,1,0, type5));  // maybe 0,1 lol

        // door models
        doorInformationDictionary.Add("door0", new DoorInformation(2f, 3f, 0,0, 1f, 0, 1,1,door0));
    }
    
    Vector2[] RemoveRedundantVertices(Vector2[] nodes)
	{
        Vector2[] nodesCopy = new Vector2[nodes.Count()+2];
		Array.Copy(nodes, nodesCopy, nodes.Count());
        nodesCopy[nodesCopy.Count()-2] = nodes[0];
        nodesCopy[nodesCopy.Count()-1] = nodes[1];

        List<int> indices = new List<int>();
        int toSubtractFacade = 0;
        for (int i = 0; i<nodes.Count(); i++)
        {
            float angle = Vector2.Angle(nodesCopy[i+2]-nodesCopy[i+1], nodesCopy[i+1]-nodesCopy[i]);
            if(angle < 10)
            {
                indices.Add(i+1);
                if(i+1 < facade) toSubtractFacade++;
            }
        }
        Debug.Log("subtracting " + toSubtractFacade + "from facade, now being: " + facade);
        facade -= toSubtractFacade;
        var list = new List<Vector2>();
        
        for (int j = 0; j<nodes.Count(); j++)
        {
            if(indices.Contains(j)==false)
            {
                list.Add(nodes[j]);
            }
        }

		return list.ToArray();
	}

    bool checkLongest(Vector2[] nodeList)
    {
        if(Vector2.Distance(nodeList[0],nodeList[1])>Vector2.Distance(nodeList[0],nodeList[3])) return true;
        else return false;
    }

}


