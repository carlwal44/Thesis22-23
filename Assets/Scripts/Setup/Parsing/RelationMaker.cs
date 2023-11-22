using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class RelationMaker : InfrastructureBehaviour
{
    IEnumerator Start()
    {
        while (!map.IsReady)
        {
            yield return null;
        }

        foreach (OsmRelation r in map.relations)
        {
            if (!r.AreAllNodesPresent()) continue;
            List<ulong> outer = r.GetOuterBoundaries();
            if (outer.Count > 2)
            {
                CreatePlane(r, outer);
            }

            List<ulong> inner = r.GetInnerBoundaries();
            if (inner.Count > 2)
            {
                createComplexPlane(r, inner, outer);
            }
        }
    }

    private void CreatePlane(OsmRelation r, List<ulong> nodes)
    {


        Vector3 localOrigin = r.GetCentre();
        GameObject go = new GameObject();
        go.name = "relation";
        go.transform.position = localOrigin - map.bounds.Centre;

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = r.GetMaterial();

        Vector2[] vertices2D = new Vector2[nodes.Count - 1];

        for (int i = 0; i < vertices2D.Length; i++)
        {
            vertices2D[i] = new Vector2(map.nodes[nodes[i]].X, map.nodes[nodes[i]].Y) - new Vector2(localOrigin.x, localOrigin.z);
        }

        Triangulator tr = new Triangulator(vertices2D, false);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
        }

        //setUVBoundaries(vertices);

        mf.mesh.vertices = vertices;
        mf.mesh.triangles = indices;
        mf.mesh.RecalculateNormals();
        mf.mesh.RecalculateBounds();

        Bounds bounds = mf.mesh.bounds;
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
        }
        mf.mesh.uv = uvs;

        mr.material.mainTextureScale = new Vector2(bounds.size.x / 6, bounds.size.z / 6);
    }

   
    private void createComplexPlane(OsmRelation r, List<ulong> inner, List<ulong> outer)
    {
        int iFirst = 0;
        int iSecond = 0;
        float dist = 0;
        // iSecond is index van punt op inner boundary zo ver mogelijk van punt op index iFirst
        for (int i = 1; i <= inner.Count - 1; i++)
        {
            float tDist = Vector2.Distance(new Vector2(map.nodes[inner[0]].X, map.nodes[inner[0]].Y), new Vector2(map.nodes[inner[i]].X, map.nodes[inner[i]].Y));
            if (tDist > dist)
            {
                dist = tDist;
                iSecond = i;
            }
        }

        int oFirst = 0;
        int oSecond = 0;

        float distClosestFirst = 1000;
        float distClosestSecond = 1000;
        float tempDist;
        // find indices of nearest poiints on outer boundary for two inner points
        for (int i = 0; i <= outer.Count - 1; i++)
        {
            tempDist = Vector2.Distance(new Vector2(map.nodes[inner[iFirst]].X, map.nodes[inner[iFirst]].Y), new Vector2(map.nodes[outer[i]].X, map.nodes[outer[i]].Y));
            if (tempDist < distClosestFirst)
            {
                distClosestFirst = tempDist;
                oFirst = i;
            }
            tempDist = Vector2.Distance(new Vector2(map.nodes[inner[iSecond]].X, map.nodes[inner[iSecond]].Y), new Vector2(map.nodes[outer[i]].X, map.nodes[outer[i]].Y));
            if (tempDist < distClosestSecond)
            {
                distClosestSecond = tempDist;
                oSecond = i;
            }
        }

        if (oFirst == oSecond) oSecond = oFirst + 1;

        // voeg alle punten voor oFirst nog eens toe
        for (int i = 0; i < oFirst; i++)
        {
            outer.Add(outer[i]);
        }
        // en verwijder die 
        for (int i = oFirst - 1; i >= 0; i--)
        {
            outer.RemoveAt(i);
        }

        if (oSecond > oFirst) oSecond = oSecond - oFirst;
        else oSecond = outer.Count - oFirst + oSecond;
        oFirst = 0;

        List<ulong> leftHalfNodes = new List<ulong>();
        List<ulong> rightHalfNodes = new List<ulong>();
        // adding nodes from left shape
        for (int i = oFirst; i <= oSecond; i++)
        {
            rightHalfNodes.Add(outer[i]);
        }
        for (int i = iSecond; i <= inner.Count - 1; i++)
        {
            rightHalfNodes.Add(inner[i]);
        }
        rightHalfNodes.Add(inner[iFirst]);
        rightHalfNodes.Add(outer[oFirst]);
        CreatePlane(r, rightHalfNodes);

        // adding nodes from right shape
        leftHalfNodes.Add(outer[0]);
        for (int i = outer.Count - 1; i >= oSecond; i--)
        {
            leftHalfNodes.Add(outer[i]);
        }

        for (int i = iSecond; i >= iFirst; i--)
        {
            leftHalfNodes.Add(inner[i]);
        }
        leftHalfNodes.Add(outer[0]);
        CreatePlane(r, leftHalfNodes);
    }
}