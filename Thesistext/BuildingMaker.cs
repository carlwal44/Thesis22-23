
for (int i = 1; i < nodes.Length; i++)
    {
        vertices.Add(new Vector3(nodes[i - 1].x, 0, nodes[i - 1].y));
        vertices.Add(new Vector3(nodes[i].x, 0, nodes[i].y));
        vertices.Add(new Vector3(nodes[i - 1].x, (float)height, nodes[i - 1].y));
        vertices.Add(new Vector3(nodes[i].x, (float)height, nodes[i].y));
    }

