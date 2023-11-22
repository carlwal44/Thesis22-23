
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class OsmWayManager : BaseOsm

    // voorbeeld om code beter te begrijpen
   /* <way id="3343954" visible="true" version="34" changeset="110039633" timestamp="2021-08-21T19:19:23Z" user="Thibault Rommel" uid="5846458">
  <nd ref="16483790"/>
  <nd ref="455446790"/>
  <nd ref="5825880215"/>
  <nd ref="933182942"/>

   */
{

    public XmlNodeList ways { get; private set; }
    public Reader map { get; private set; }

    public OsmWayManager(XmlNodeList nodeList, Reader map)
    {
        ways = nodeList;
        this.map = map;
        getAllInformation();
    }

    public void getAllInformation() {
        foreach (XmlNode node in ways) {
            OsmWay w = new OsmWay(node, map);
            map.ways.Add(w.GetId(), w);
            XmlNodeList tags = node.SelectNodes("tag");
            foreach (XmlNode t in tags)
            {
                string key = GetAttribute<string>("k", t.Attributes);
                if (key == "building" || key == "building:part")
                {
                    /*string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "yes" || val == "house" || val == "apartments")
                    {
                        Debug.Log("building found, why not accepted?");
                        break;
                    }
                    else
                    {
                        OsmBuilding b = new OsmBuilding(node, map);
                        Vector3 centre = b.GetCentre();
                        if (!map.OSMBuildings.ContainsKey(centre))
                        {
                            map.OSMBuildings.Add(centre, b);
                        }
                    }*/

                    // I changed the above into this - C
                    OsmBuilding b = new OsmBuilding(node, map);
                    Vector3 centre = b.GetCentre();
                    if (!map.OSMBuildings.ContainsKey(centre))
                    {
                        map.OSMBuildings.Add(centre, b);
                    }

                }
                else if (key == "place")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "square")
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                }
                else if (key == "highway")
                {
                    Road r = new Road(node, map);
                    map.roads.Add(r);
                    AddLayer(r.GetTexture());
                }
                else if (key == "leisure")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "park" || val == "pitch" || val == "playground" || val == "sports_centre" || val == "fitness_station" || val == "garden")
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                }
                else if (key == "amenity")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "parking" || val == "bicycle_parking" || val == "parking_space" || val == "car_sharing" || val == "grave_yard")
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                    /*if (val == "charging_station" || val == "school" || val == "restaurant" || val == "shelter" || val == "cafe" || val == "fast_food" || val == "toilets" || val == "bar" || val == "pharmacy")
                    {
                        OsmBuilding b = new OsmBuilding(node, map);
                        Vector3 centre = b.GetCentre();
                        if (!map.OSMBuildings.ContainsKey(centre))
                        {
                            map.OSMBuildings.Add(centre, b);
                        } 
                    }*/
                }

                else if (key == "landuse")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "brownfield" || val == "railway" || val == "recreation_ground" || val == "urban_green" || val == "flowerbed" || val == "farmland" || val == "farmyard" || val == "grass" || val == "greenfield" || val == "meadow" || val == "cemetry" || val == "village_green" || val == "vineyard" || val == "orchard" || val == "landfill" || val == "plant_nursery" || val == "recreation_ground" || val == "allotments" || val == "basin")
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                    else if (val == "forest") {
                        Area f = new Area(node, map);
                        map.areas.Add(f);
                        //TODO: texture & normal toevoegen voor forest
                    }
                }
                else if (key == "barrier")
                {
                    Barrier b = new Barrier(node, map);
                    map.barriers.Add(b);
                }

                else if (key == "natural")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "tree_row" || val == "tree_trunk")
                    {
                        TreeRow a = new TreeRow(node, map);
                        map.treeRows.Add(a);
                    }
                    else if (val == "wood") {
                        Area f = new Area(node, map);
                        map.areas.Add(f);
                        //TODO: texture & normal toevoegen voor forest
                    }
                    else
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                }

                else if (key == "landcover")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "trees")
                    {
                        Area f = new Area(node, map);
                        map.areas.Add(f);
                        //TODO: texture & normal toevoegen voor forest
                    }
                }

                else if (key == "surface")
                {
                    //checken voor eerste en laatste node gelijk

                    List<ulong> NodeIDs = new List<ulong>();
                    XmlNodeList nds = node.SelectNodes("nd");
                    foreach (XmlNode n in nds)
                    {
                        ulong refNo = GetAttribute<ulong>("ref", n.Attributes);
                        NodeIDs.Add(refNo);
                    }
                    if (NodeIDs[0] == NodeIDs[NodeIDs.Count - 1])
                    {
                        Area a = new Area(node, map);
                        map.areas.Add(a);
                        AddLayer(a.GetTexture());
                    }
                    else
                    {
                        Road r = new Road(node, map);
                        map.roads.Add(r);
                        AddLayer(r.GetTexture());
                    }
                }
            }
        }
    }

    private void AddLayer(string texture)
    {
        /*Terrain terrain = map.terrainMaker.to.GetComponent<Terrain>();

        TerrainLayer[] oldLayers = terrain.terrainData.terrainLayers;

        TerrainLayer t0 = new TerrainLayer();
        t0.diffuseTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + texture);
        t0.normalMapTexture = Resources.Load<Texture2D>("art/texturesAndNormals/" + texture + "-normal");
        t0.tileOffset = Vector2.zero;
        t0.tileSize = Vector2.one;
        t0.name = texture;
        
        for (int i = 0; i < oldLayers.Length; i++)
        {
            if (oldLayers[i].name == t0.name)
            {
                return;
            }
        }

        TerrainLayer[] newLayers = new TerrainLayer[oldLayers.Length + 1];
        System.Array.Copy(oldLayers, 0, newLayers, 0, oldLayers.Length);
        newLayers[oldLayers.Length] = t0;

        terrain.terrainData.terrainLayers = newLayers;*/
    }
}
