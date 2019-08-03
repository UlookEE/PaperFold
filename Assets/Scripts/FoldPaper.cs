using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Papers
{
    public List<Paper> paperList;

    public Papers()
    {
        paperList = new List<Paper>();
    }
    public void makePaper(Vector3[] vertices)
    {
        paperList.Add(new Paper(vertices));
    }
    public class Paper
    {
        public GameObject paper;
        public Paper(Vector3[] vertices)
        {
            paper = new GameObject();
            paper.AddComponent<MeshFilter>();
            paper.AddComponent<MeshRenderer>();
            var mf = paper.GetComponent<MeshFilter>();
            var mr = paper.GetComponent<MeshRenderer>();
            var mesh = new Mesh();
            mf.mesh = mesh;
            mr.material.color = new Color(255, 255, 255);
            mesh.vertices = vertices;

            var tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1,
            };

            mesh.triangles = tris;

            var normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            var uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            mesh.uv = uv;
        }
    }

    
}
public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject mainPaper;

    void Start()
    {
        var vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(9, 0, 0),
            new Vector3(0, 5, 0),
            new Vector3(9, 5, 0),
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
