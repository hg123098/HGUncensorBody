using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshSplitter : MonoBehaviour
{	
    public static Mesh splitByQuad(SkinnedMeshRenderer mesh_renderer, MeshFilter mesh_filter)
    {
        var plane = createPlane(mesh_filter);
        var mesh = mesh_renderer.sharedMesh;
        var matrix = mesh_renderer.transform.localToWorldMatrix;

        string mesh_name = mesh_renderer.gameObject.name;

        var tri_a = new List<List<int>>();
        var tri_b = new List<List<int>>();

        for (int j = 0; j < mesh.subMeshCount; j++)
        {
            var triangles = mesh.GetTriangles(j);
            tri_a.Add(new List<int>());
            tri_b.Add(new List<int>());

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var triangle = triangles.Skip(i).Take(3);
                bool side = false;

                foreach (int n in triangle)
                {
                    side = side || plane.GetSide(matrix.MultiplyPoint(mesh.vertices[n]));
                }

                if (side)
                {
                    tri_a[j].AddRange(triangle);
                }
                else
                {
                    tri_b[j].AddRange(triangle);
                }
	        }
        }
        return createNewMesh(mesh_renderer, tri_b.Select(n => n.ToArray()).ToArray(), mesh_name);
    }

    private static Plane createPlane(MeshFilter mesh_filter)
    {
        var matrix = mesh_filter.transform.localToWorldMatrix;
        var mesh = mesh_filter.sharedMesh;
        var vertices = mesh.triangles.Take(3).Select(n => matrix.MultiplyPoint(mesh.vertices[n])).ToArray();
        return new Plane(vertices[0], vertices[1], vertices[2]);
    }

    private static Mesh createNewMesh(SkinnedMeshRenderer original, int[][] triangles, string name)
    {
        var mesh = Instantiate(original.sharedMesh) as Mesh;

        mesh.subMeshCount = triangles.Length;
        for (int i = 0; i < triangles.Length; i++)
        {
            mesh.SetTriangles(triangles[i], i);
        }
        return mesh;
    }
}