using UnityEngine;
using System.Collections.Generic;
using System;

public class Triangulator
{
	private List<Vector2> m_points = new List<Vector2>();
	bool reverse;

	public Triangulator(Vector2[] points, bool reverse)
	{
		m_points = new List<Vector2>(points);
		this.reverse = reverse;
	}

	public int[] Triangulate()
	{
		List<int> indices = new List<int>();

		int n = m_points.Count;
		if (n < 3)
			return indices.ToArray();

		int[] V = new int[n];
		if (Area() > 0)
		{
			for (int v = 0; v < n; v++)
				V[v] = v;
		}
		else
		{
			for (int v = 0; v < n; v++)
				V[v] = (n - 1) - v;
		}

		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2;)
		{
			if ((count--) <= 0)
				return indices.ToArray();

			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;

			if (Snip(u, v, w, nv, V))
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}

		if(!reverse) indices.Reverse();
		return indices.ToArray();
	}

	private float Area()
	{
		int n = m_points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++)
		{
			Vector2 pval = m_points[p];
			Vector2 qval = m_points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}

	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		int p;
		Vector2 A = m_points[V[u]];
		Vector2 B = m_points[V[v]];
		Vector2 C = m_points[V[w]];
		float smallEpsilon = 0.1f;
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))) &
		Vector2.SqrMagnitude(A - B) > smallEpsilon &
		Vector2.SqrMagnitude(A - C) > smallEpsilon &
		Vector2.SqrMagnitude(B - C) > smallEpsilon)
		return false;
		for (p = 0; p < n; p++)
		{
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = m_points[V[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}

	private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}

	public Vector3[] getRoofRidge(Vector2[] nodes, float height, bool perp)
	{
		Vector3[] ridge = new Vector3[2];
		Vector2 half1 = Vector2.Lerp(nodes[0],nodes[1],0.5f);
		Vector2 half2 = Vector2.Lerp(nodes[2],nodes[3],0.5f);
		ridge[0] = new Vector3(half1.x,height,half1.y);
		ridge[1] = new Vector3(half2.x,height,half2.y);
		return ridge;
	}

	public int returnConvexPoint(Vector2[] nodes)
	{
		Vector2[] nodesCopy = new Vector2[8];
		Array.Copy(nodes, nodesCopy, 6);
		nodesCopy[6] = nodes[0];
		nodesCopy[7] = nodes[1];

		for (int i = 0; i < 6; i++)
		{
			Vector2 next = nodesCopy[i+1] - nodesCopy[i];
			Vector2 nextnext = nodesCopy[i+2] - nodesCopy[i];
			if(isConvex(next, nextnext) < 0)
			{
				if(i>1) return i-2;
				else return i+1;	// opgelet: als convex point node[0] is, wordt niet gezien, dus desnoods toevoegen
			}
		}
		return -1; 
	}

	public static float isConvex(Vector2 A, Vector2 B)
	{
		return -A.x * B.y + A.y * B.x;
	}

	public int getSplitConvex(Vector2[] nodes)
	{
		float bestdist = 1000;
		int bestindex = 0;
		for (int i = 0; i<3; i++)
		{
			float dist = Vector2.Distance(nodes[i],nodes[i+3]);
			if(dist < bestdist)
			{
				bestindex = i;
				bestdist = dist;
			}
		}
		return bestindex;
	}

	public Vector2[] getLeft(Vector2[] nodes, int index)
	{
		Vector2[] nodesRet = new Vector2[4];
		for (int i = 0; i<4; i++)
		{
			nodesRet[i] = nodes[index + i];
		}
		return nodesRet;
	}

	public Vector2[] getRight(Vector2[] nodes, int index)
	{
		Vector2[] nodesRet = new Vector2[4];
		Vector2[] nodesCopy = new Vector2[9];
		Array.Copy(nodes, nodesCopy, 6);
		nodesCopy[6] = nodes[0];
		nodesCopy[7] = nodes[1];
		nodesCopy[8] = nodes[2];

		for (int i = 0; i<4; i++)
		{
			nodesRet[i] = nodesCopy[index + i];
		}
		return nodesRet;
	}
}
