using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegment {

    public float length; // 直線距離 [単位10cm]
    public float radius; // 旋回半径 [単位10cm]
    public float theta;  // 旋回角度 [度、時計回りが正]。0を設定すると曲線が描画されず直線のみとなる。
    public float width = 1.6f; // 道路の幅の半分 [単位10cm] ETロボコンの道路幅32cmを再現する場合は1.6とする。
    static readonly float DIV_THETA = 5.0f; // カーブの分割角度

    // 道路セグメントの頂点列を作成する。
    public List<Vector3> makeVertices()
    {
        var vertices=new List<Vector3>();

        vertices.Add(new Vector3(-width, 0, 0));
        vertices.Add(new Vector3(+width, 0, 0));

        // 直線
        if (length > 0)
        {
            vertices.Add(new Vector3(-width, 0, length));
            vertices.Add(new Vector3(+width, 0, length));
        }

        // 曲線 (theta==0の時は描画しない。)
        if (radius < width)
        {
            // 最小旋回半径: radiusがwidthより小さい場合はwidthに合わせる。
            radius = width;
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

        return vertices;
    }

}

public class RoadScript : MonoBehaviour
{

    // セグメントリスト。ここで道路の形状を設定する。
    public List<RoadSegment> segments;

    // 頂点列
    private List<Vector3> vertices;
    // 三角形列
    private List<int> triangles;
    // テクスチャ用UV列
    private List<Vector2> uvs;

    // 全てのセグメントの頂点列をつないで、三角形列、テクスチャ用UV列を生成する。
    void makeVerticesFromSegments()
    {
        if (segments != null)
        {
            Vector3 pos = new Vector3(0, 0, 0);
            float theta = 0;
            vertices = new List<Vector3>();

            foreach (var seg in segments)
            {
                var seg_vertices = seg.makeVertices();
                // 回転行列
                var q = Quaternion.AngleAxis(theta, Vector3.up);

                if (vertices.Count > 1)
                {
                    // 接続位置の座標は重複するので削除する。
                    vertices.RemoveRange(vertices.Count - 2, 2);
                }
                // 接続位置の座標と向きを使って回転・移動させる。
                foreach (var vt in seg_vertices)
                {
                    vertices.Add(q * vt + pos);
                }
                // 次の接続位置の座標と向きを計算する。
                pos = (vertices[vertices.Count - 2] + vertices[vertices.Count - 1])/2;
                theta += seg.theta;
            }
        }

        // 頂点列の並びから、三角形列は下記のように決定できる。
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

        // UV列は道路テクスチャのパターンを繰り返し適用するため、下記のように設定する。
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
            // Meshに頂点列、三角形列、UV列を設定する。
            var mesh = new Mesh();
            makeVerticesFromSegments();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); // 法線の再計算をしないとライティングがバグる。
            mesh.SetUVs(0, uvs);

            // MeshFilterにMeshを設定すると、MeshRendererがいい感じで道路を描画してくれる。
            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {
        // RoadSegmentに変更があったときに再描画する。
        Start();
    }
}
