using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoadSegment {

    public float length; // �������� [�P��10cm]
    public float radius; // ���񔼌a [�P��10cm]
    public float theta;  // ����p�x [�x�A���v��肪��]�B0��ݒ肷��ƋȐ����`�悳�ꂸ�����݂̂ƂȂ�B
    public float width = 1.6f; // ���H�̕��̔��� [�P��10cm] ET���{�R���̓��H��32cm���Č�����ꍇ��1.6�Ƃ���B
    static readonly float DIV_THETA = 5.0f; // �J�[�u�̕����p�x

    // ���H�Z�O�����g�̒��_����쐬����B
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

    // �Z�O�����g���X�g�B�����œ��H�̌`���ݒ肷��B
    public List<RoadSegment> segments;

    // ���_��
    private List<Vector3> vertices;
    // �O�p�`��
    private List<int> triangles;
    // �e�N�X�`���pUV��
    private List<Vector2> uvs;

    // �S�ẴZ�O�����g�̒��_����Ȃ��ŁA�O�p�`��A�e�N�X�`���pUV��𐶐�����B
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
                // ��]�s��
                var q = Quaternion.AngleAxis(theta, Vector3.up);

                if (vertices.Count > 1)
                {
                    // �ڑ��ʒu�̍��W�͏d������̂ō폜����B
                    vertices.RemoveRange(vertices.Count - 2, 2);
                }
                // �ڑ��ʒu�̍��W�ƌ������g���ĉ�]�E�ړ�������B
                foreach (var vt in seg_vertices)
                {
                    vertices.Add(q * vt + pos);
                }
                // ���̐ڑ��ʒu�̍��W�ƌ������v�Z����B
                pos = (vertices[vertices.Count - 2] + vertices[vertices.Count - 1])/2;
                theta += seg.theta;
            }
        }

        // ���_��̕��т���A�O�p�`��͉��L�̂悤�Ɍ���ł���B
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

        // UV��͓��H�e�N�X�`���̃p�^�[�����J��Ԃ��K�p���邽�߁A���L�̂悤�ɐݒ肷��B
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
            // Mesh�ɒ��_��A�O�p�`��AUV���ݒ肷��B
            var mesh = new Mesh();
            makeVerticesFromSegments();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); // �@���̍Čv�Z�����Ȃ��ƃ��C�e�B���O���o�O��B
            mesh.SetUVs(0, uvs);

            // MeshFilter��Mesh��ݒ肷��ƁAMeshRenderer�����������œ��H��`�悵�Ă����B
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
        // RoadSegment�ɕύX���������Ƃ��ɍĕ`�悷��B
        Start();
    }
}
