using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegment {
    public float length;
    public float radius;
    public float theta;
    public float width = 1.0f;
    static readonly float DIV_THETA = 5.0f;

    public override string ToString()
    {
        return $"length({length}) , radius({radius}) , theta({theta})";
    }

    public void makeVertices(out List<Vector3> vertices, out List<int> triangles)
    {
        vertices=new List<Vector3>();

        vertices.Add(new Vector3(-width, 0, 0));
        vertices.Add(new Vector3(+width, 0, 0));

        if (length > 0)
        {
            vertices.Add(new Vector3(-width, 0, length));
            vertices.Add(new Vector3(+width, 0, length));
        }

        if (theta > 0)
        {
            // 時計回り
            for (int i = 0; i < (int)(theta / DIV_THETA); i++)
            {
                float t = (float)(DIV_THETA * i) * Mathf.Deg2Rad;
                var v_out = new Vector3(radius - (radius + width) * Mathf.Cos(t), 0, (radius + width) * Mathf.Sin(t) + length);
                vertices.Add(v_out);
                var v_in = new Vector3(radius - (radius - width) * Mathf.Cos(t), 0, (radius - width) * Mathf.Sin(t) + length);
                vertices.Add(v_in);
            }
            {
                float t = theta * Mathf.Deg2Rad;
                var v_out = new Vector3(radius - (radius + width) * Mathf.Cos(t), 0, (radius + width) * Mathf.Sin(t) + length);
                vertices.Add(v_out);
                var v_in = new Vector3(radius - (radius - width) * Mathf.Cos(t), 0, (radius - width) * Mathf.Sin(t) + length);
                vertices.Add(v_in);
            }
        }
        else if (theta < 0)
        {
            // 反時計回り
            for (int i = 0; i < (int)(-theta / DIV_THETA); i++)
            {
                float t = (float)(DIV_THETA * i) * Mathf.Deg2Rad;
                var v_in = new Vector3(-radius + (radius - width) * Mathf.Cos(t), 0, (radius - width) * Mathf.Sin(t) + length);
                vertices.Add(v_in);
                var v_out = new Vector3(-radius + (radius + width) * Mathf.Cos(t), 0, (radius + width) * Mathf.Sin(t) + length);
                vertices.Add(v_out);
            }
            {
                float t = -theta * Mathf.Deg2Rad;
                var v_in = new Vector3(-radius + (radius - width) * Mathf.Cos(t), 0, (radius - width) * Mathf.Sin(t) + length);
                vertices.Add(v_in);
                var v_out = new Vector3(-radius + (radius + width) * Mathf.Cos(t), 0, (radius + width) * Mathf.Sin(t) + length);
                vertices.Add(v_out);
            }

        }

        triangles = new List<int>();
        for (int i = 0; i < (vertices.Count-2); i+=2)
        {
            triangles.Add(i + 0);
            triangles.Add(i + 2);
            triangles.Add(i + 1);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i + 3);
        }

    }

    public Vector3 calcNextPosition()
    {
        var np = new Vector3(0, 0, length);
        if (theta > 0)
        {
            // 時計回り
            float t = theta * Mathf.Deg2Rad;
            np = np+ new Vector3(radius * (1 - Mathf.Cos(t)), 0, radius * Mathf.Sin(t));
        } else if (theta < 0)
        {
            // 反時計回り
            float t = -theta * Mathf.Deg2Rad;
            np = np + new Vector3(-radius * (1 - Mathf.Cos(t)), 0, radius * Mathf.Sin(t));
        }
        return np;
    }

}

public class RoadScript : MonoBehaviour
{

    public List<RoadSegment> segments;
    private List<Vector3> points;

    // Start is called before the first frame update
    void Start()
    {

       if (segments != null)
       {
            var mesh = new Mesh();

            points = new List<Vector3>();
            var p = new Vector3(0, 0, 0);
            points.Add(p);
            foreach (var seg in segments)
            {
                p = p + seg.calcNextPosition();
                points.Add(p);
                Debug.Log(seg);
                Debug.Log(p);

                List<Vector3> vertices;
                List<int> triangles;
                seg.makeVertices(out vertices, out triangles);
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();

                var uvs = new List<Vector2>();
                for (int i=0; i < vertices.Count; i+=2)
                {
                    uvs.Add(new Vector2(0, i/2));
                    uvs.Add(new Vector2(1, i/2));
                }
                mesh.SetUVs(0, uvs);

            }

            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }

    void test()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3> {
          new Vector3 (-1, 0, -1),
          new Vector3 (-1, 0, 1),
          new Vector3 (1, 0, 1),
          new Vector3 (1, 0, -1),
          new Vector3 (2, 0, -1),
          new Vector3 (2, 0, 1),
        };
        mesh.SetVertices(vertices);

        var triangles = new List<int> { 0, 1, 2, 2, 3, 0, 2, 3, 4, 4, 5, 2 };
        mesh.SetTriangles(triangles, 0);

        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
