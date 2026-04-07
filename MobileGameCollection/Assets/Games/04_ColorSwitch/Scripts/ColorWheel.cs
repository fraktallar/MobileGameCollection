using UnityEngine;
using System.Collections.Generic;

public class ColorWheel : MonoBehaviour
{
    public float OuterRadius = 2.0f;
    public float InnerRadius = 1.4f;
    public float RotateSpeed = 60f;
    public bool  Scored      = false;

    private int segments = 4;
    private List<GameObject> segObjs = new List<GameObject>();
    private float startAngle;

    void Start()
    {
        startAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, 0, startAngle);
        BuildWheel();
    }

    void Update()
    {
        if (ColorSwitchManager.Instance.IsEnded()) return;
        transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);

        // Kameranın çok altına düşünce sil
        float cullY = Camera.main.transform.position.y -
                      Camera.main.orthographicSize - 3f;
        if (transform.position.y < cullY)
            Destroy(gameObject);
    }

    void BuildWheel()
    {
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            Color col = ColorSwitchManager.GameColors[i];
            float angle = i * angleStep;
            CreateArc(col, angle, angleStep - 4f);
        }
    }

    void CreateArc(Color col, float startDeg, float spanDeg)
    {
        int steps = 24;
        Mesh mesh  = new Mesh();

        List<Vector3> verts  = new List<Vector3>();
        List<int>     tris   = new List<int>();
        List<Color>   colors = new List<Color>();

        float startRad = startDeg * Mathf.Deg2Rad;
        float endRad   = (startDeg + spanDeg) * Mathf.Deg2Rad;

        for (int i = 0; i <= steps; i++)
        {
            float t   = (float)i / steps;
            float ang = Mathf.Lerp(startRad, endRad, t);

            float cx = Mathf.Cos(ang);
            float cy = Mathf.Sin(ang);

            verts.Add(new Vector3(cx * InnerRadius, cy * InnerRadius, 0));
            verts.Add(new Vector3(cx * OuterRadius, cy * OuterRadius, 0));
            colors.Add(col); colors.Add(col);

            if (i < steps)
            {
                int idx = i * 2;
                tris.Add(idx);     tris.Add(idx+1); tris.Add(idx+2);
                tris.Add(idx+1);   tris.Add(idx+3); tris.Add(idx+2);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetColors(colors);

        GameObject seg = new GameObject("Seg_" + col);
        seg.transform.SetParent(transform, false);
        MeshFilter mf = seg.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        MeshRenderer mr = seg.AddComponent<MeshRenderer>();
        mr.material = CreateColorMat(col);
        mr.sortingOrder = 5;

        segObjs.Add(seg);
    }

    Material CreateColorMat(Color col)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = col;
        return mat;
    }

    // Topun pozisyonuna göre hangi renk diliminde olduğunu döndür
    public Color GetColorAtAngle(Vector2 ballPos)
    {
        Vector2 dir = ballPos - (Vector2)transform.position;
        // Çemberin kendi rotasyonunu çıkar
        float worldAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float localAngle = worldAngle - transform.eulerAngles.z;

        // 0-360 aralığına normalize et
        localAngle = (localAngle % 360f + 360f) % 360f;

        int segIdx = Mathf.FloorToInt(localAngle /
                     (360f / segments)) % segments;
        return ColorSwitchManager.GameColors[segIdx];
    }
}