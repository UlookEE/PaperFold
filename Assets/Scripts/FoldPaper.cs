using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Papers
{
    public List<Paper> paperList;

    public Papers()
    {
        Debug.Log("Papers()");
        paperList = new List<Paper>();
    }
    public void makePaper(Vector3[] vertices)
    {
        Debug.Log("makePaper()");
        paperList.Add(new Paper(vertices));
    }
    public class Paper
    {
        public GameObject paper;

        public Paper(Vector3[] vertices)
        {
            Debug.Log("Paper()");
            paper = new GameObject();
            paper.AddComponent<MeshFilter>();
            paper.AddComponent<MeshRenderer>();
            var mf = paper.GetComponent<MeshFilter>();
            var mr = paper.GetComponent<MeshRenderer>();
            var mesh = new Mesh();
            mf.mesh = mesh;
            mr.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            mesh.vertices = vertices;

            var vertices2D = new Vector2[vertices.Length];

            var allEqual = new bool[] { true, true, true };
            var first_element = new double[] { vertices[0].x, vertices[0].y, vertices[0].z };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 1; j < vertices.Length; j++)
                {
                    if (first_element[i] != vertices[j][i])
                    {
                        allEqual[i] = false;
                        break;
                    }
                }
            }
            for(int i=0; i<3; i++)
            {
                if (allEqual[i] || i == 2)
                {
                    for(int j=0; j<vertices2D.Length; j++)
                    {
                        vertices2D[j] = new Vector2(vertices[j][(i + 1) % 3], vertices[j][(i + 2) % 3]);
                        Debug.Log(vertices2D[j]);
                    }
                    break;
                }
            }

            Triangulator tr = new Triangulator(vertices2D);
            var tris = tr.Triangulate();
            mesh.triangles = tris;

            var normals = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = -Vector3.forward;
            }
            mesh.normals = normals;

            var uv = new Vector2[vertices.Length];
            for(int i=0; i<vertices.Length; i++)
            {
                uv[i] = new Vector2(0, 0);
            }
            mesh.uv = uv;

            //Debug.Log("start");
            foreach (var e in tris)
            {
                Debug.Log(e);
            }
        }
    }


}
public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject mainPaper;

    void Start()
    {
        Debug.Log("Start()");
        var vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,5),
            new Vector3(0,5,5),
            new Vector3(0,5,0),
        };
        
        Papers p = new Papers();
        p.makePaper(vertices);
        p.paperList[0].paper.transform.position = new Vector3(0, 0, 0);
        //mainPaper = GameObject.Find("MainPaper");
        //mainPaper.transform.Rotate(angle: 30.0f, axis: new Vector3(0));
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(mainPaper.transform.rotation);   
    }
}
