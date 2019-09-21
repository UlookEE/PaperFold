using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Paper : MonoBehaviour
{
    public static int sphereCount = 0;
    public static Paper usingPaper; 
    public static List<GameObject> paperList = new List<GameObject>();
    public static void makePaper(List<Vector3> vertices)
    {
        //Debug.Log("makePaper()");

        GameObject tmp = new GameObject();
        tmp.AddComponent<Paper>();
        tmp.GetComponent<Paper>().initPaper(vertices);

        paperList.Add(tmp);

    }


    static int paperCount = 0;

    public List<Vector3> vertices; // 마지막 vertices는 무게중심
    
    public List<Vector3> GetGlobalVertices()
    {
        List<Vector3> list = new List<Vector3>();
        foreach(var v in vertices)
        {
            Vector3 vector = this.transform.TransformPoint(v);
            list.Add(vector);
        }
        return list;
    }


    public int zindex = 0;
    public List<Paper> foldline; //선번호마다 연결된 다른 papername 만약 연결안되면 null값 
    //경고!!!!!! foldline 이 바뀌어서 string에서 Paper로 꼭 바꿔야합니다.

    //TODO : 겹치는 면에 대해서(예를들어 2면 이상을 같이 접을 경우의 분할을 생각해야합니다.)

    public List<Paper> Tracking(Paper fix, Paper dynamic)
    {//TODO : tracking의 검증 필요, 검증은 안해봤음 아직
        List<Paper> trackgroup = new List<Paper>();
        trackgroup.Add(dynamic);
        for (int i = 0; i < dynamic.foldline.Count; i++)
        {
            if (dynamic.foldline[i] == null || dynamic.foldline[i] == fix) //pass
            {
                List<Paper> tmppaperlist = Tracking(dynamic, dynamic.foldline[i]);
                for (int j = 0; j < tmppaperlist.Count; j++)
                {
                    trackgroup.Add(tmppaperlist[j]);
                }
            }
        }
        return trackgroup;
    }

    //TODO : 2겹이 겹칠때 startindex와 endindex를 포함하더라도 우리가 접는 부분은 둘다 겹치는 부분의 정보를 가져가야합니다.
    //TODO : 각도가 돌아가 있거나 포지션이 바뀌어 있으면 그것도 같이 적용해야합니다.
    public bool Folding(Vector2 startpoint, Vector2 endpoint) //이 Vector2는 Mesh 상의 vector입니다. 월드좌표계 사용시 변환 필요
    {
        int startindex = -1;
        int endindex = -1;

        //TODO : x1,x2,y1,y2가 기존 점과 동일할 경우에는 새로 점을 만드는것이 아니게 예외처리 해야합니다.
        for (int i = 0; i < vertices.Count - 1; i++)
        {
            if (Vector2.Distance(vertices[i], startpoint)
                + Vector2.Distance(vertices[(i + 1) % (vertices.Count - 1)], startpoint)
                - Vector2.Distance(vertices[i], vertices[(i + 1) % (vertices.Count - 1)]) <= 0.00001f)
            {
                startindex = i;
            }
            if (Vector2.Distance(vertices[i], endpoint)
                + Vector2.Distance(vertices[(i + 1) % (vertices.Count - 1)], endpoint)
                - Vector2.Distance(vertices[i], vertices[(i + 1) % (vertices.Count - 1)]) <= 0.00001f)
            {
                endindex = i;
            }

        }

        if (startindex == -1 || endindex == -1)
        {
            //Debug.LogError("start, endindex fault");
            return false; // 직선 상에서 분명 좌표를 안잡았음..
        }

        //오른쪽 페이퍼 만들기 시작
        List<Vector3> newvertices = new List<Vector3>();
        List<Paper> newfoldline = new List<Paper>();
        newvertices.Add(startpoint);
        newfoldline.Add(foldline[startindex]); //주의 : 바꿧습니다.
        for (int t = startindex + 1; t - 1 != endindex; t++)
        {
            if (t == vertices.Count - 1) t = 0;
            newvertices.Add(vertices[t]);
            newfoldline.Add(foldline[t]);
        }
        newvertices.Add(endpoint);
        newfoldline.RemoveAt(newfoldline.Count - 1);

        makePaper(newvertices);
        //왼쪽 페이퍼 만들기 시작
        List<Vector3> new2vertices = new List<Vector3>();
        List<Paper> new2foldline = new List<Paper>();
        new2vertices.Add(endpoint);
        new2foldline.Add(foldline[endindex]); //주의 : 바꿧습니다.

        for (int t = endindex + 1; t - 1 != startindex; t++)
        {
            if (t == vertices.Count - 1) t = 0;
            new2vertices.Add(vertices[t]);
            new2foldline.Add(foldline[t]);
        }
        new2vertices.Add(startpoint);
        new2foldline.RemoveAt(new2foldline.Count - 1);

        makePaper(new2vertices);

        //서로 연결
        var tmpPos = gameObject.transform.position;
        var tmpRot = gameObject.transform.rotation;
        paperList[paperList.Count - 2].transform.position = tmpPos;
        paperList[paperList.Count - 2].transform.rotation = tmpRot;
        paperList[paperList.Count - 1].transform.position = tmpPos;
        paperList[paperList.Count - 1].transform.rotation = tmpRot;
        paperList[paperList.Count - 2].GetComponent<Paper>().foldline.Add(paperList[paperList.Count - 1].GetComponent<Paper>());
        paperList[paperList.Count - 1].GetComponent<Paper>().foldline.Add(paperList[paperList.Count - 2].GetComponent<Paper>());

        paperList.Remove(gameObject);
        Destroy(gameObject);
        //TODO : 연결관계 가져오기, 현재의 클래스 지우기

        //paper 잡는 임시 코드입니다 ----
        //usingPaper = paperList[paperList.Count - 2].GetComponent<Paper>();
        return true;
    }

    public void initPaper(List<Vector3> vertices, List<string> foldstring = null)
    {
        /* Make One Paper */
        //Debug.Log("Paper()");

        gameObject.layer = LayerMask.NameToLayer("PAPER");
        gameObject.name = "Paper Split " + paperCount++;

        var mf = gameObject.AddComponent<MeshFilter>();
        var mr = gameObject.AddComponent<MeshRenderer>();
        var mc = gameObject.AddComponent<MeshCollider>();

        var mesh = new Mesh();

        if (foldstring == null)
            foldline = new List<Paper>(); //접힌면 다 빈칸채우기
        for (int i = 0; i < vertices.Count; i++)
        {
            foldline.Add(null);
        }

        Vector3 centermess = Vector3.zero;
        for (int i = 0; i < vertices.Count; i++)
        {
            centermess += vertices[i];
        }
        centermess /= vertices.Count;
        vertices.Add(centermess);

        this.vertices = vertices;


        /* Make mesh.triangle use 2D Triangulator and Orthodontist */

        Vector2[] uv = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            uv[i].x = vertices[i].x;
            uv[i].y = vertices[i].y;
        }
        int[] triangle = new int[(vertices.Count - 1) * 3]; //무게중심은 제

        for (int i = 0; i < (vertices.Count - 1) * 3; i += 3)
        {
            triangle[i] = i / 3;
            triangle[i + 1] = (i / 3 + 1) % (vertices.Count - 1);
            triangle[i + 2] = vertices.Count - 1;

        }
        //Debug.Log(triangle);


        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangle;

        mf.mesh = mesh;
        mr.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        mr.material.shader = Shader.Find("Custom/Double-Sided-Shader");
        mesh.uv = uv;

        mc.sharedMesh = mesh;
        mc.convex = true;
        mc.isTrigger = true;    



    }
    /* End Make One Paper */

    public static Vector3 in_paper(Paper P, Vector3 pos)
    {
        //Debug.Log("before " + pos.ToString());
        var globalVertices = P.GetGlobalVertices();
        var equation = makeEquation.make_plane_equation(new List<Vector3>(){globalVertices[0], globalVertices[1], globalVertices[2]});
        pos = new Vector3(pos.x, pos.y, -(equation[0] * pos.x + equation[1] * pos.y + equation[3]) / equation[2]);
        //Debug.Log("after "+ pos.ToString());
        var triangles = P.GetComponent<MeshFilter>().mesh.triangles;
        RaycastHit hit = new RaycastHit(); hit.point = pos;
        for (int i=0; i<triangles.Length; i += 3)
        {
            if(makeEquation.in_triangle(pos, new List<Vector3>() { globalVertices[triangles[i]], globalVertices[triangles[i + 1]], globalVertices[triangles[i + 2]] }))
            {
                return pos;
            }

        }
        return new Vector3(-100,-100,-100);
    }

    public static Vector3 attatch_to_edge(Paper P, Vector3 pos)
    {
        var globalVertices = P.GetGlobalVertices();
        float minDistance = 100;
        int minIndex = -1;
        for (int i=0; i<globalVertices.Count-1; i++)
        {
            Vector3 v1;
            Vector3 v2;
            /* v1 : line of pos and vertex
               v2 : one edge */
            if (i != globalVertices.Count - 2)
            {
                v1 = pos - globalVertices[i];
                v2 = globalVertices[i + 1] - globalVertices[i];
            }
            else
            {
                v1 = pos - globalVertices[i];
                v2 = globalVertices[0] - globalVertices[i];
            }

            float angle = Vector3.Angle(v1, v2);
            if (angle>=0 && angle <= 90)
            {
                float distance = v1.magnitude * v2.magnitude * Mathf.Sin(angle*Mathf.Deg2Rad);
                
                if(distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }
       
        }
        //Debug.Log("DIST: " + minDistance);

        if (minDistance < 0.1)
        {
            Vector3 v1;
            Vector3 v2;
            if (minIndex != globalVertices.Count - 2)
            {
                v1 = pos - globalVertices[minIndex];
                v2 = globalVertices[minIndex + 1] - globalVertices[minIndex];
            }
            else
            {
                v1 = pos - globalVertices[minIndex];
                v2 = globalVertices[0] - globalVertices[minIndex];
            }

            //Debug.Log("PROJECTION: " + (Vector3.Project(v1, v2) + globalVertices[minIndex]).ToString());
            if(sphereCount == 0)
                return Vector3.Project(v1, v2) + globalVertices[minIndex];
            else
            {
                var spherePos = Paper.usingPaper.transform.Find("Sphere 0").transform.position;
                var angle = Vector3.Angle(v2, Vector3.Project(v1, v2) + globalVertices[minIndex] - spherePos);
                if (angle < 0.001 || angle > 179.999)
                {
                    return new Vector3(-100, -100, -100);
                }
                return Vector3.Project(v1, v2) + globalVertices[minIndex];
            }

        }
        return new Vector3(-100, -100, -100);
    }

    //MinSeok
    public static bool isDragging = false;          //Checks whether it's being dragged.
    public static Vector3 cursorInitialPosition;    //Cursor's initial position in screen.
    public static Vector3 cursorDeltaPosition;      //Cursor's delta position in screen.

    /// <summary>
    /// When a foldpaper clicked.
    /// </summary>
    private void OnMouseDown()
    {
        StopCoroutine("CheckDrag");
        StartCoroutine("CheckDrag");
    }

    /// <summary>
    /// When a foldpaper released.
    /// </summary>
    private void OnMouseUp()
    {
        StopCoroutine("CheckDrag");
        isDragging = false;
    }

    /// <summary>
    /// Checks whether foldpaper is being dragged or not.
    /// </summary>
    IEnumerator CheckDrag()
    {
        for (int i = 0; i < 10; ++i)
        {
            yield return new WaitForSeconds(0.01f);
        }

        if(sphereCount ==2)
            isDragging = true;

        cursorInitialPosition = Input.mousePosition;
        Debug.Log("INITIAL");
        StopCoroutine("CheckDrag");
    }

    /// <summary>
    /// Drag function.
    /// </summary>
    private void OnMouseDrag()
    {
        if (isDragging)
        {
            cursorDeltaPosition = cursorInitialPosition - Input.mousePosition;
            //Debug.Log(Input.mousePosition);
        }
    }
}

public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject mainPaper;
    public static GameObject makeSphere(Vector3 pos, Color color, GameObject parent)
    {
        /* Make one sphere with hit and Color */
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.name = "Sphere " + Paper.sphereCount;
        sphere.transform.position = pos;
        sphere.transform.localScale -= new Vector3(0.95f, 0.95f, 0.95f);
        sphere.GetComponent<MeshRenderer>().material.color = color;

        sphere.transform.parent = parent.transform;
        return sphere;
        /* End Make one sphere with hit and Color */
    }

    /* return real position and local position of sphere */
    public static List<Vector3> spherePosLocal(GameObject gameObjectOfPaper)
    {
        List<Vector3> retList = new List<Vector3>();
        retList.Add(gameObjectOfPaper.transform.Find("Sphere 0").transform.position);
        retList.Add(gameObjectOfPaper.transform.Find("Sphere 1").transform.position);

        var tmpRot = gameObjectOfPaper.transform.rotation;
        gameObjectOfPaper.transform.rotation = Quaternion.Euler(0, 0, 0);
        retList.Add(gameObjectOfPaper.transform.Find("Sphere 0").localPosition);
        retList.Add(gameObjectOfPaper.transform.Find("Sphere 1").localPosition);
        gameObjectOfPaper.transform.rotation = tmpRot;

        return retList;
    }
    void Start()
    {
        int width = 1, height = 1;

        List<Vector3> vertices = new List<Vector3>
        {
            new Vector3(0,0,0),
            new Vector3(width,0,0),
            new Vector3(width,height,0),
            new Vector3(0,height,0),
        };

        Paper.makePaper(vertices);
        

    }

    // Update is called once per frame


    void find_pos()
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        var paperLayer = LayerMask.NameToLayer("PAPER");
        if (Physics.Raycast(ray, out hit, 100.0f, 1 << paperLayer))
        {
            if (hit.collider.name.Contains("Paper"))
            {
                GameObject hitGO = hit.collider.gameObject;
                Paper p = hit.collider.gameObject.GetComponent<Paper>();
                //Debug.Log("PAPER GRAVITY CENTER LOC: " + p.vertices[p.vertices.Count - 1]);
                Vector3 planePos = Paper.in_paper(p, hit.point);
                if (planePos != new Vector3(-100, -100, -100))
                {
                    Vector3 edgePos = Paper.attatch_to_edge(p, planePos);
                    Debug.Log("Edgepos" + edgePos);
                    if (edgePos != new Vector3(-100, -100, -100) && (Paper.usingPaper == p || Paper.sphereCount == 0))
                    {
                        makeSphere(edgePos, Color.blue, hitGO);
                 
                        Paper.sphereCount++;
                        Paper.usingPaper = p;
                    }
                    return;
                }
                else
                {
                    hitGO.GetComponent<MeshCollider>().enabled = false;
                    find_pos();
                    hitGO.GetComponent<MeshCollider>().enabled = true;
                }
            }
            else
            {
                return;
            }
            /* End Click*/
        }
    }
    //MinSeok
    /// <summary>
    /// When click, find which foldpaper user is clicking now.
    /// </summary>
    void find_pos_noSphere()
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        var paperLayer = LayerMask.NameToLayer("PAPER");
        if (Physics.Raycast(ray, out hit, 100.0f, 1 << paperLayer))
        {
            if (hit.collider.name.Contains("Paper"))
            {
                GameObject hitGO = hit.collider.gameObject;
                Paper p = hit.collider.gameObject.GetComponent<Paper>();
                //Debug.Log("PAPER GRAVITY CENTER LOC: " + p.vertices[p.vertices.Count - 1]);
                Vector3 planePos = Paper.in_paper(p, hit.point);
                if (planePos != new Vector3(-1, -1, -1))
                {

                    return;
                }
                else
                {
                    hitGO.GetComponent<MeshCollider>().enabled = false;
                    find_pos();
                    hitGO.GetComponent<MeshCollider>().enabled = true;
                }
            }
            else
            {
                return;
            }
            /* End Click*/
        }
    }

    /// <summary>
    /// Sets usingpaper to a paper hit by raycast.
    /// </summary>
    void find_paper()
    {
        RaycastHit hit;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

        var paperLayer = LayerMask.NameToLayer("PAPER");
        if (Physics.Raycast(ray, out hit, 100.0f, 1 << paperLayer))
        {
            if (hit.collider.name.Contains("Paper"))
            {
                GameObject hitGO = hit.collider.gameObject;
                Paper p = hit.collider.gameObject.GetComponent<Paper>();
                //Debug.Log("PAPER GRAVITY CENTER LOC: " + p.vertices[p.vertices.Count - 1]);
                Vector3 planePos = Paper.in_paper(p, hit.point);
                if (planePos != new Vector3(-100, -100, -100))
                {
                    Paper.usingPaper = p;
                    return;
                }
                else
                {
                    hitGO.GetComponent<MeshCollider>().enabled = false;
                    find_paper();
                    hitGO.GetComponent<MeshCollider>().enabled = true;
                }
            }
            else
            {
                return;
            }
            /* End Click*/
        }
    }

    public static bool isCut = false;
    public static Vector3 pos1;
    public static Vector3 pos2;
    public static Vector3 rotPos1;
    public static Vector3 rotPos2;

    void Update()
    {
        //민석
        //If click occurs,
        if (Input.GetMouseButtonDown(0))
        {
            //If there's  1 or less sphere, and is not cutting paper yet, try to get another sphere.
            if (Paper.sphereCount < 2 && !isCut)
            {
                find_pos();
            }
            //If found all,
            if (Paper.sphereCount >= 2)
            {
                //If not cut foldpaper yet, start cutting it.
                if (!isCut)
                {
                    Debug.Log("isCut");
                    var posList = spherePosLocal(Paper.usingPaper.gameObject);
                    rotPos1 = posList[0];
                    rotPos2 = posList[1];
                    pos1 = posList[2];
                    pos2 = posList[3];
                    Paper.usingPaper.Folding(pos1, pos2);
                    isCut = true;
                }
                //If cutting enabled, find paper and start dragging it.
                else
                {
                    Debug.Log("");
                    find_paper();
                }
            }
        }

        //If user is dragging, rotate it.
        if (Paper.isDragging)
        {
            
            Vector2 campos1 = Camera.allCameras[0].WorldToScreenPoint(pos1);
            Vector2 campos2 = Camera.allCameras[0].WorldToScreenPoint(pos2);
            float angle = Mathf.Acos(Vector2.Dot(Paper.cursorDeltaPosition, campos2 - campos1) / (((Vector2)Paper.cursorDeltaPosition).magnitude * (campos2 - campos1).magnitude));
            float direction = Mathf.Sign(Vector3.Cross(Paper.cursorDeltaPosition, campos2 - campos1).z);
            float cosine = direction * Mathf.Abs(Mathf.Sin(angle));
            float value = Paper.cursorDeltaPosition.magnitude * cosine / 66;
            //Debug.Log(direction);
            if (double.IsNaN(value))
                value = 0;
            Paper.usingPaper.gameObject.transform.RotateAround(rotPos2, rotPos2 - rotPos1, value);
        }
    }
}