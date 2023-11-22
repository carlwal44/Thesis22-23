using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class OsmRelationManager : BaseOsm


{

    public XmlNodeList ways { get; private set; }
    public Reader map { get; private set; }

    public OsmRelationManager(XmlNodeList nodeList, Reader map)
    {
        ways = nodeList;
        this.map = map;
        getAllInformation();
    }

    public void getAllInformation()
    {
        foreach (XmlNode node in ways)
        {
            bool isMulti = false;
            bool HasSurface = false;
            XmlNodeList tags = node.SelectNodes("tag");
            foreach (XmlNode t in tags)
            {
                string key = GetAttribute<string>("k", t.Attributes);
                if (key == "type")
                {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "multipolygon") isMulti = true;
                }
                else if (key == "surface" || key == "parking")
                {
                    HasSurface = true;
                }
                else if (key == "place") {
                    string val = GetAttribute<string>("v", t.Attributes);
                    if (val == "square") HasSurface = true;
                }
                else if (key == "area")
                {
                    string val = GetAttribute<string>("v", t.Attributes); 
                    if (val == "yes") HasSurface = true;
                }

                if (isMulti && HasSurface) 
                {
                    OsmRelation relation = new OsmRelation(node, map);
                    map.relations.Add(relation);
                    break;
                }
            }
        }
    }
}
