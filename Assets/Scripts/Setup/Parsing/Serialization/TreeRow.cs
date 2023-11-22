using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

//TODO aanmaken
public class TreeRow : OsmWay
{
    public TreeRow(XmlNode node, Reader map) : base(node, map)
    {
    }

    public override void GetTags()
    {
    }

}

