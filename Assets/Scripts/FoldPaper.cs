using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        paperList.Add(new Paper(vertices));
    }
    public class Paper
    {
        public GameObject paper;
        static int paperCount = 0;
        public Paper(Vector3[] vertices)
        {
            /* Make One Paper */
            Debug.Log("Paper()");
            paper = new GameObject();
            paper.name = "Paper Split " + paperCount++;
            var mf = paper.AddComponent<MeshFilter>();
            var mr = paper.AddComponent<MeshRenderer>();
            var mc = paper.AddComponent<MeshCollider>();
            var mesh = new Mesh();
            mf.mesh = mesh;
            mr.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

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
            /* End Make mesh.triangle use 2D Triangulator and Orthodontist */

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
            mc.sharedMesh = mesh;
        }
        /* End Make One Paper */
        }


    }
public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject mainPaper;

    void makeSphere(RaycastHit hit, Color color, int sphereCount)
    {
        /* Make one sphere with hit and Color */
        Transform objectHit = hit.transform;
        Debug.Log(hit.point);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere " + sphereCount;
        sphere.transform.position = hit.point;
        sphere.transform.localScale -= new Vector3(0.75f, 0.75f, 0.75f);
        sphere.GetComponent<MeshRenderer>().material.color = color;
        /* End Make one sphere with hit and Color */
    }
    void Start()
    {
        int width = 9, height = 5;
        Papers p = new Papers(width, height);

        /* Set position of camera and light */
        transform.position = new Vector3((float)width / 2, (float)height / 2, -10);
        var mainLight = GameObject.Find("Main Light");
        mainLight.transform.position = new Vector3((float)width/2, (float)height /2, -10);
        /* End Set position of camera and light */
    }

    // Update is called once per frame
    static int sphereCount = 0;
    static float lastClick = -0.5f;
    Vector3 lastClickPos = new Vector3(-1, -1, -1);
    void Update()
    {
        /* Make Sphere on Click */
        if (Input.GetMouseButtonDown(0) )
        {
            RaycastHit hit;
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out hit))
            {
                /* Double Click */
                if (hit.collider.name.Contains("Paper") && Time.time < lastClick + 0.5f && sphereCount == 2 && Input.mousePosition == lastClickPos)
                {
                    Debug.Log("Double Click!");
                    makeSphere(hit, Color.blue, sphereCount++);
                }
                lastClick = Time.time;
                lastClickPos = Input.mousePosition;
                /* End Double Click*/

                /* Click */
                if (hit.collider.name.Contains("Paper") && sphereCount < 2)
                {
                    makeSphere(hit, Color.red, sphereCount++);
                }
                /* End Click*/
            }
        }
        /* End Make Sphere on Click */

        /* Destroy Sphere on right Click */
        else if (Input.GetMouseButton(1) && sphereCount != 0)
        {
            GameObject sphere = GameObject.Find("Sphere " + --sphereCount);
            Debug.Log("Delete " + sphere.name);
            Destroy(sphere);
        }
        /* End Destroy Sphere on right Click */
    }
}
