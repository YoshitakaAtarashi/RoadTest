using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegment {
    public float length;
    public float radius;
    public float theta;
    public float width = 160.0f;
    static readonly float DIV_THETA = 5.0f;

    public override string ToString()
    {
        return $"length({length}) , radius({radius}) , theta({theta})";
    }

    public List<Vector3> makeVertices()
    {
        var vertices=new List<Vector3>();

        vertices.Add(new Vector3(-width, 0, 0));
        vertices.Add(new Vector3(+width, 0, 0));

        // ����
        if (length > 0)
        {
            vertices.Add(new Vector3(-width, 0, length));
            vertices.Add(new Vector3(+width, 0, length));
        }

        // �Ȑ� (theta==0�̎��͕`�悵�Ȃ��B)
        if (radius < width)
        {
            // �ŏ����񔼌a: radius��width��菬�����ꍇ��width�ɍ��킹��B
            radius = width;
        }
        if (theta > 0)
        {
            // ���v���
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
            // �����v���
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

        return vertices;
    }

}

public class RoadScript : MonoBehaviour
{

    // �Z�O�����g���X�g�B���H�̌`���ݒ肷��B
    public List<RoadSegment> segments;

    private List<Vector3> points;
    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;

    // �S�ẴZ�O�����g���Ȃ��B
    void makeVerticesFromSegments()
    {
        if (segments != null)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            float theta = 0;
            vertices = new List<Vector3>();

            foreach (var seg in segments)
            {
                var v = seg.makeVertices();
                foreach (var vt in v)
                {
                    vertices.Add(Quaternion.AngleAxis(theta, Vector3.up) * vt + pos);
                }
                pos = (vertices[vertices.Count - 2] + vertices[vertices.Count - 1])/2;
                theta += seg.theta;
            }
        }

        triangles = new List<int>();
        for (int i = 0; i < (vertices.Count - 2); i += 2)
        {
            triangles.Add(i + 0);
            triangles.Add(i + 2);
            triangles.Add(i + 1);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
            triangles.Add(i + 3);
        }

        uvs = new List<Vector2>();
        for (int i = 0; i < vertices.Count; i += 2)
        {
            uvs.Add(new Vector2(0, i / 2));
            uvs.Add(new Vector2(1, i / 2));
        }
    }


    // Start is called before the first frame update
    void Start()
    {

       if (segments != null)
       {
            var mesh = new Mesh();
            var p = new Vector3(0, 0, 0);

            makeVerticesFromSegments();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.SetUVs(0, uvs);

            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
