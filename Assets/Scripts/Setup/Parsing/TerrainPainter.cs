using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VegetationStruct
{
    public Vector3 position;
    public int index;

    public VegetationStruct(Vector3 pos, int index)
    {
        position = pos;
        this.index = index;
    }
}


public class TerrainPainter : MonoBehaviour
{

    //deze functie zou gebruikt worden door terrainmaker om de basislaag van kleur aan ons terrain te geven.
    public void paint()
    {
        // Get the attached terrain component
        Terrain terrain = GetComponent<Terrain>();

        // Get a reference to the terrain data
        TerrainData terrainData = terrain.terrainData;
        terrainData.alphamapResolution = terrainData.baseMapResolution;
        Debug.Log("alphamap resolution: " + terrainData.alphamapResolution);

        //alle layers die al uit de verschillende areas en dergelijke halen, zou kunnen dat de basis texture die we voor heel het terrein willen gebruiken er al tussen zit
        TerrainLayer[] oldLayers = terrainData.terrainLayers;

        //add new textures
        TerrainLayer t0 = new TerrainLayer();
        t0.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/scrub");
        t0.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/scrub-normal");
        t0.tileOffset = Vector2.zero;
        t0.tileSize = Vector2.one;
        t0.name = "scrub";

        TerrainLayer t1 = new TerrainLayer();
        t1.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/grass");
        t1.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/grass-normal");
        t1.tileOffset = Vector2.zero;
        t1.tileSize = Vector2.one;
        t1.name = "grass";

        TerrainLayer t2 = new TerrainLayer();
        t2.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/flowers");
        t2.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/flowers-normal");
        t2.tileOffset = Vector2.zero;
        t2.tileSize = Vector2.one;
        t2.name = "sand";

        int present_dirt = -1;
        for (int i = 0; i < oldLayers.Length; i++)
        {
            //Debug.Log(oldLayers[i].name);
            if (oldLayers[i].name == t0.name)
            {
                present_dirt = i;
                //Debug.Log("scrub " + i);
            }
        }
        if (present_dirt == -1)
        {
            //Debug.Log("new texture: scrub");
            TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
            System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
            newLayers[oldLayers.Length] = t0;
            terrainData.terrainLayers = newLayers;
            present_dirt = oldLayers.Length;
            oldLayers = newLayers;
        }
        int present_grass = -1;
        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t1.name)
            {
                present_grass = i;
                //Debug.Log("grass: " + 1);
            }
        }
        if (present_grass == -1)
        {
            //Debug.Log("new texture: grass");
            TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
            System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
            newLayers[oldLayers.Length] = t1;
            terrainData.terrainLayers = newLayers;
            present_grass = oldLayers.Length;
            oldLayers = newLayers; 
        }
        int present_sand = -1;
        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t2.name)
            {
                present_sand = i;
                //Debug.Log("flower: " + i);
            }
        }

        //als het nog niet tussen de layers zit, ff toevoegen en dan gewoon ook alles op nul zetten buiten die layer, geen else nodig.
        if (present_sand == -1)
        {
            //Debug.Log("new texture: flower");
            TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
            System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
            newLayers[oldLayers.Length] = t2;
            terrainData.terrainLayers = newLayers;
            present_sand = oldLayers.Length;
            oldLayers = newLayers;
        }

        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float dirt_weight = Random.Range(0.0f, 1.0f);
                float grass_weight = Random.Range(0.0f, 1.0f);
                float sand_weight = Random.Range(0.0f, 1.0f);

                float total_weight = dirt_weight + grass_weight + sand_weight;

                for (int j = 0; j < terrainData.alphamapLayers; j++)
                {
                    //Debug.Log(j);
                    if (j == present_dirt)
                    {
                        splatmapData[(int)x, (int)y, j] = dirt_weight / total_weight;
                        //Debug.Log("dirt: " + (dirt_weight / total_weight));
                    }
                    else if (j == present_grass)

                    {
                        splatmapData[(int)x, (int)y, j] = grass_weight / total_weight;
                        //Debug.Log("grass: " + (grass_weight / total_weight));
                    }
                    else if (j == present_sand)

                    {
                        splatmapData[(int)x, (int)y, j] = sand_weight / total_weight;
                        //Debug.Log("sand: " + (sand_weight / total_weight));
                    }
                    else splatmapData[(int)x, (int)y, j] = 0f;
                }
            }
        }

        // Finally assign the new splatmap to the terrainData:
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    //deze functie ging ik dan initialiseren voor alles verschillende areas te tekenen. je zou dus het terrain object meegeven, de area die de nodeids bevat en dergelijke
    //en dan de texture die je wilt die erop komt te liggen. dit zou uiteindelijk door de GetMaterial() functie overschreven moeten worden zodat op basis van de naam automatisch de juiste
    //diffuse en normal map gebruikt wordt.
    public List<VegetationStruct> PaintArea(Area area)
    {
        // Get the attached terrain component
        Terrain terrain = GetComponent<Terrain>();

        // Get a reference to the terrain data
        TerrainData terrainData = terrain.terrainData;
        //Debug.Log(terrainData.alphamapResolution + " " + area.map.terrainMaker.xSize + " " + area.map.terrainMaker.zSize);

        //begin door te kijken of de texture nog niet toevallig in de verschillende terrain layers aanwezig is om die zo te gebruiken, anders gaan er voor elke area een layer worden toegevoegd (banaal)
        TerrainLayer[] oldLayers = terrainData.terrainLayers;

        //oude gewichten opslaan om renormaliseren en dergelijke te fixen (dit is heeeeeeeeeeeeel inefficient)
        float[,,] oldSplatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        //create new layer
        TerrainLayer t0 = new TerrainLayer();
        t0.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + area.GetTexture());
        t0.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + area.GetTexture() + "-normal");
        t0.tileOffset = Vector2.zero;
        t0.tileSize = Vector2.one;
        t0.name = area.GetTexture();


        //hier ben ik gestopt, deze code doet nog niks was gewoon mijn hersenspinsel van toen. wou hier kijken naar het beginnen opvullen van een area en dergelijke idk weet het niet meer.
        int present = -1;
        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t0.name)
            {
                present = i;
            }
        }
        Vector2[] points = new Vector2[area.NodeIDs.Count];

        // CALCULATE POINT PLACEMENT ON ALPHAMAP
        for (int i = 0; i < area.NodeIDs.Count; i++)
        {

            float x = (float)(MercatorProjection.lonToX(area.map.bounds.MaxLon) - area.map.nodes[area.NodeIDs[i]].X);
            float y = (float)(MercatorProjection.latToY(area.map.bounds.MaxLat) - area.map.nodes[area.NodeIDs[i]].Y);

            points[i] = calculatePoint(x, y, area.map, terrainData);
        }

        List<Vector2> polyPoints = new List<Vector2>();
        //dit is als het een nieuwe texture is, gewichten oude splamapdata gaat hernormalized moeten worden.
        if (present == -1)
        {
            Debug.Log("new texture (area):" + area.GetTexture());
            TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
            System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
            newLayers[oldLayers.Length] = t0;
            terrainData.terrainLayers = newLayers;
            present = oldLayers.Length;
            float[,,] newSplatmapData = GetSplatmapData(terrainData, oldSplatmapData);

            polyPoints = SetAlphamaps(points, terrainData, present, newSplatmapData);
        }
        //geen nieuwe texture, nu gaan de gewichten op de juiste plaatsen moeten verschoven worden richting de gewilde texture
        else
        {
            polyPoints = SetAlphamaps(points, terrainData, present, oldSplatmapData);
        }

        return GetVegetation(polyPoints, area, terrainData);



    }

    public void PaintRoad(Vector2[] points, Road road)
    {
        Terrain terrain = GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;
        TerrainLayer[] oldLayers = terrainData.terrainLayers;
        float[,,] oldSplatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        //create new layer
        TerrainLayer t0 = new TerrainLayer();
        t0.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + road.GetTexture());
        t0.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + road.GetTexture() + "-normal");
        t0.tileOffset = Vector2.zero;
        t0.tileSize = Vector2.one;
        t0.name = road.GetTexture();

        int present = -1;
        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t0.name)
            {
                present = i;
            }
        }
        Vector2[] convertedPoints = new Vector2[points.Length];

        for (int i = 0; i < points.Length; i++)
        {

            float x = (float)(MercatorProjection.lonToX(road.map.bounds.MaxLon) - points[i].x);
            float y = (float)(MercatorProjection.latToY(road.map.bounds.MaxLat) - points[i].y);

            convertedPoints[i] = calculatePoint(x, y, road.map, terrainData);
        }

        //dit is als het een nieuwe texture is, gewichten oude splamapdata gaat hernormalized moeten worden.
        if (present == -1)
        {
            Debug.Log("new texture (road): " + road.GetTexture());
            present = oldLayers.Length;
            TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
            System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
            newLayers[oldLayers.Length] = t0;

            terrainData.terrainLayers = newLayers;

            float[,,] newSplatmapData = GetSplatmapData(terrainData, oldSplatmapData); 

            SetAlphamaps(convertedPoints, terrainData, present, newSplatmapData);
        }
        //geen nieuwe texture, nu gaan de gewichten op de juiste plaatsen moeten verschoven worden richting de gewilde texture
        else
        {
            SetAlphamaps(convertedPoints, terrainData, present, oldSplatmapData);
        }
    }

    private List<Vector2> SetAlphamaps(Vector2[] convertedPoints, TerrainData terrainData, int present, float[,,] oldSplatmapData) {

        List<Vector2> polyPoints = new List<Vector2>();
        
        // hier over alle loopen checken of er in ligt
        Vector2[] bounds = calculateBounds(convertedPoints);
        for (int y = (int)bounds[0].y; y < (int)bounds[1].y; y++)
        {
            for (int x = (int)bounds[0].x; x < (int)bounds[1].x; x++)
            {

                if (polyCheck(new Vector2(x, y), convertedPoints))
                {
                    polyPoints.Add(new Vector2(x, y));
                    for (int j = 0; j < terrainData.alphamapLayers; j++)
                    {
                        if (j == present) oldSplatmapData[(int)x, (int)y, j] = 1f;
                        else oldSplatmapData[(int)x, (int)y, j] = 0f;
                    }
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, oldSplatmapData);

        return polyPoints;
    }

    private Vector2 calculatePoint(float x, float y, Reader map, TerrainData terrainData)    {

        x /= map.terrainMaker.scaleX;
        y /= map.terrainMaker.scaleZ;

        x /= map.terrainMaker.xSize;
        y /= map.terrainMaker.zSize;

        float temp = x;
        x = 1 - y;
        y = 1 - temp;

        if (x > 1) x = 0.995f;
        if (y > 1) y = 0.995f;

        if (x < 0) x = 0;
        if (y < 0) y = 0;

        x *= terrainData.alphamapResolution;
        y *= terrainData.alphamapResolution;

        return new Vector2(x, y);
    }

    private float[,,] GetSplatmapData(TerrainData terrainData, float[,,] oldSplatmapData)
    {
        float[,,] newSplatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.terrainLayers.Length];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int i = 0; i < terrainData.terrainLayers.Length - 1; i++)
                {
                    newSplatmapData[x, y, i] = oldSplatmapData[x, y, i];
                }
                newSplatmapData[x, y, terrainData.terrainLayers.Length - 1] = 0f;
            }
        }

        return newSplatmapData;
    }

    private Vector2[] calculateBounds(Vector2[] points)
    {
        Vector2 min = new Vector2(100000, 100000);
        Vector2 max = new Vector2(0, 0);
        foreach (Vector2 v in points)
        {
            if (v.x > max.x) max.x = v.x;
            if (v.x < min.x) min.x = v.x;
            if (v.y > max.y) max.y = v.y;
            if (v.y < min.y) min.y = v.y;
        }
        Vector2[] vectors = new Vector2[2];
        vectors[0] = min;
        vectors[1] = max;
        return vectors;
    }

    public bool polyCheck(Vector2 v, Vector2[] p)
    {

        int j = p.Length - 1;
        bool c = false;
        for (int i = 0; i < p.Length; j = i++)
            c ^= p[i].y > v.y ^ p[j].y > v.y && v.x < (p[j].x - p[i].x) * (v.y - p[i].y) / (p[j].y - p[i].y) + p[i].x;
        return c;
    }

    private List<VegetationStruct> GetVegetation(List<Vector2> points, Area area, TerrainData terrainData)
    {
        List<VegetationStruct> vegetation = new List<VegetationStruct>();

        bool IsGrass = (area.GetClassification() == "grass");
        bool IsGarden = (area.GetClassification() == "garden");
        bool IsScrub = (area.GetClassification() == "scrub");

        foreach (Vector2 p in points)
        {
            if (IsGrass)
            {
                int r = Random.Range(0, 70);
                if (r == 1) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 0));
                if (r == 2) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 1));
                if (r == 3) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 2));
            }
            if (IsGarden)
            {
                int r = Random.Range(0, 400);
                if (r < 20) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 2));
                if (r == 1) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 4));
            }
            if (IsScrub)
            {
                int r = Random.Range(0, 30);
                if (r == 3) vegetation.Add(new VegetationStruct(GetRealPosition(p.x, p.y, terrainData, area), 3));
            }
        }

        return vegetation;
    }
    private Vector3 GetRealPosition(float y, float x, TerrainData terrainData, Area area)
    {

        x /= terrainData.alphamapResolution;
        y /= terrainData.alphamapResolution;

        y = 1 - y;
        x = 1 - x;

        x *= area.map.terrainMaker.xSize;
        y *= area.map.terrainMaker.zSize;

        x *= area.map.terrainMaker.scaleX;
        y *= area.map.terrainMaker.scaleZ;

        float X = (float)(MercatorProjection.lonToX(area.map.bounds.MaxLon) - x);
        float Y = (float)(MercatorProjection.latToY(area.map.bounds.MaxLat) - y);
        float Z = area.map.terrainMaker.FindHeight(new Vector3(X, 0, Y));
        return new Vector3(X, Z, Y);
    }

}