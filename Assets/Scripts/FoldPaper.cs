using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Paper
{
    public GameObject paper;
    public Vector3[] vertices;
    static int paperCount = 0;
    public Paper(Vector3[] vertices)
    {
        /* Make One Paper */
        Debug.Log("Paper()");
        paper = new GameObject();
        paper.layer = LayerMask.NameToLayer("PAPER");
        paper.name = "Paper Split " + paperCount++;
        var mf = paper.AddComponent<MeshFilter>();
        var mr = paper.AddComponent<MeshRenderer>();
        var mc = paper.AddComponent<MeshCollider>();
        
       
        var mesh = new Mesh();
        this.vertices = vertices;
       
        mf.mesh = mesh;
        mr.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        mr.material.shader = Shader.Find("Custom/Double-Sided-Shader");
        
        /* Make mesh.triangle use 2D Triangulator and Orthodontist */
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
        for (int i = 0; i < 3; i++)
        {
            if (allEqual[i] || i == 2)
            {
                for (int j = 0; j < vertices2D.Length; j++)
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
        /* End Make mesh.triangle use 2D Triangulator and Orthodontist */

        var normals = new Vector3[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            normals[i] = -Vector3.forward;
        }
        mesh.normals = normals;

        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2(0, 0);
        }
        mesh.uv = uv;
        mc.sharedMesh = mesh;

       

    }
    /* End Make One Paper */

    public int fold_v;
    public bool inEdge(Vector3 point)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            var minAngle = Mathf.Min(Vector3.Angle(vertices[i] - vertices[(i + 1) % vertices.Length], vertices[i] - point),
                                    Vector3.Angle(-vertices[i] + vertices[(i + 1) % vertices.Length], vertices[(i + 1) % vertices.Length] - point));
            if (minAngle < 0.5f)
            {
                Debug.Log("Edge");
                return true;
            }
            Debug.Log(Vector3.Angle(vertices[i] - vertices[(i + 1) % vertices.Length], vertices[i] - point));
        }
        return false;
    }

    public bool isVertex(Vector3 point)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            if ((point - vertices[i]).magnitude < 0.05f)
            {
                Debug.Log(point + " : corner");
                fold_v = i;
                return true;
            }
        }
        return false;
    }

    public void separatePaper()
    {

    }
}
public class Papers
{
    public List<Paper> paperList;

    public Papers(int width, int height)
    {
        Debug.Log("Papers()");
        paperList = new List<Paper>();

        /* init first paper */
        var vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,height,0),
            new Vector3(width,height,0),
            new Vector3(width,0,0),
        };

        makePaper(vertices);
        paperList[0].paper.transform.position = new Vector3(0, 0, 0);
        /* End init first paper */


    }
    public void makePaper(Vector3[] vertices)
    {
        Debug.Log("makePaper()");
        Paper paper = new Paper(vertices);
        paperList.Add(paper);
        
    }

    public Paper findPaper(string name)
    {
        foreach (var p in paperList)
        {
            if (name == p.paper.name)
                return p;
        }
        return null;
    }



}
public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject mainPaper;
    Papers p;
    void makeSphere(RaycastHit hit, Color color, int sphereCount)
    {
        /* Make one sphere with hit and Color */
        Transform objectHit = hit.transform;
        Debug.Log(hit.point);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.name = "Sphere " + sphereCount;
        sphere.transform.position = hit.point;
        sphere.transform.localScale -= new Vector3(0.85f, 0.85f, 0.85f);
        sphere.GetComponent<MeshRenderer>().material.color = color;
        /* End Make one sphere with hit and Color */
    }
    List<Vector3> clickedPos;
    void Start()
    {
        int width = 9, height = 5;
        p = new Papers(width, height);
        clickedPos = new List<Vector3>();

        /* Set position of camera and light */
        transform.position = new Vector3((float)width / 2, (float)height / 2, -5);
        var mainLight = GameObject.Find("Main Light");
        mainLight.transform.position = new Vector3((float)width / 2, (float)height / 2, -10);
        /* End Set position of camera and light */

        var colPaper = p.paperList[0];
        p.makePaper(new Vector3[]
                    {
                        new Vector3(0,0,0),
                        new Vector3(0,5.0f,0),
                        new Vector3(3f,5.0f,0),
                        new Vector3(3f,0,0),
                    });

        p.makePaper(new Vector3[]
        {
                        new Vector3(3f,0,0),
                        new Vector3(3f,5.0f,0),
                        new Vector3(9.0f,5.0f,0),
                        new Vector3(9.0f,0,0),
        });

        p.paperList.Remove(colPaper);
        Destroy(colPaper.paper);

        var A = new Vector3(3.0f, 5.0f, 0);
        var B = new Vector3(3.0f, 0f, 0);
        p.paperList[1].paper.transform.RotateAround(A, B - A, 180);


    }

    // Update is called once per frame
    static int sphereCount = 0;
    static float lastClick = -0.5f;
    private int paperLayer;
    Vector3 lastClickPos = new Vector3(-1, -1, -1);

    void Update()
    {
        /* Make Sphere on Click */
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            paperLayer = LayerMask.NameToLayer("PAPER");
            if (Physics.Raycast(ray, out hit, 100.0f, 1 << paperLayer))
            {
                /* Double Click */
                Paper colPaper = p.findPaper(hit.collider.name);
                if (hit.collider.name.Contains("Paper") && Time.time < lastClick + 0.5f && sphereCount == 2 && Input.mousePosition == lastClickPos && colPaper.isVertex(hit.point))
                {
                    Debug.Log("Double Click!");
                    makeSphere(hit, Color.blue, sphereCount++);
                    clickedPos.Add(hit.point);

                }
                lastClick = Time.time;
                lastClickPos = Input.mousePosition;
                /* End Double Click*/

                /* Click */
                if (hit.collider.name.Contains("Paper") && sphereCount < 2 && colPaper.inEdge(hit.point) && !colPaper.isVertex(hit.point))
                {
                    makeSphere(hit, Color.red, sphereCount++);
                    clickedPos.Add(hit.point);
                    Debug.Log(clickedPos.Count);
                }


                /* End Click*/
            }
        }
        /* End Make Sphere on Click */

        /* Destroy Sphere on right Click */
        else if (Input.GetMouseButtonDown(1) && sphereCount != 0)
        {
            RaycastHit hit;
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, 1 << paperLayer))
            {
                GameObject sphere = GameObject.Find("Sphere " + --sphereCount);
                Debug.Log("Delete " + sphere.name);
                clickedPos.Clear();
                Destroy(sphere);
            }
        }
        /* End Destroy Sphere on right Click */
        
    }
}


// is_in : to all triangles