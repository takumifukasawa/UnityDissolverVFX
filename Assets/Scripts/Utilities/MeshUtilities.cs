using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class MeshUtilities
    {
        static public Mesh CreatePlane()
        {
            // 0 -------- 1
            // |        / |
            // |       /  |
            // |      /   |
            // |     /    |
            // |    /     |
            // |   /      |
            // |  /       |
            // | /        |
            // 2 -------- 3
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f)
            };
            Vector2[] uv = new Vector2[]
            {
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(0, 0),
            };
            int[] triangles = new int[]{
                0, 2, 1,
                1, 2, 3
            };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            return mesh;
        }
    }
}
