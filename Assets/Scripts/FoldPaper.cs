using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paper : MonoBehaviour
{
    static int paperCount = 0;                                          // for gameobject name
    public List<Vector3> vertices;                                      // last vertice is CoG
    public static int sphereCount = 0;                                  // if vertex of foldline selected, spherecout++
    public static Paper usingPaper;                                     // one paper set of rotating papers
    public static Paper fixedPaper;                                     // one paper set of not rotating papers
    public static List<GameObject> paperList = new List<GameObject>();  // entire paper list

    /* Make One Paper */
    public void initPaper(List<Vector3> vertices, List<string> foldstring = null)
    {
        gameObject.layer = LayerMask.NameToLayer("PAPER");
        gameObject.name = "Paper Split " + paperCount++;

        var mf = gameObject.AddComponent<MeshFilter>();
        var mr = gameObject.AddComponent<MeshRenderer>();
        var mc = gameObject.AddComponent<MeshCollider>();

        var mesh = new Mesh();

        if (foldstring == null)
            attachedPaperList = new List<Paper>(); //접힌면 다 빈칸채우기

        Vector3 centermess = Vector3.zero;
        for (int i = 0; i < vertices.Count; i++)
        {
            centermess += vertices[i];
        }
        centermess /= vertices.Count;   // CoG vertex
        vertices.Add(centermess);
        this.vertices = vertices;

        Vector2[] uv = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            uv[i].x = vertices[i].x;
            uv[i].y = vertices[i].y;
        }
        int[] triangle = new int[(vertices.Count - 1) * 3]; // vertice.Count -1 : because of CoG

        for (int i = 0; i < (vertices.Count - 1) * 3; i += 3)
        {
            triangle[i] = i / 3;
            triangle[i + 1] = (i / 3 + 1) % (vertices.Count - 1);
            triangle[i + 2] = vertices.Count - 1;

        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangle;

        mf.mesh = mesh;
        mr.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        mr.material.shader = Shader.Find("Custom/Double-Sided-Shader"); // for view mesh both side
        mesh.uv = uv;

        mc.sharedMesh = mesh;
        mc.convex = true;
        mc.isTrigger = true;
    }
    /* make paper with gameobject */
    public static void makePaper(List<Vector3> vertices)
    {
        GameObject tmp = new GameObject();
        tmp.AddComponent<Paper>();
        tmp.GetComponent<Paper>().initPaper(vertices);
        paperList.Add(tmp);
    }

    /* set fixedPaper : fixedPaper is not rotate */
    public static void setFIxedPaper()
    {
        if (usingPaper == paperList[paperList.Count - 1].GetComponent<Paper>())
        {
            fixedPaper = paperList[paperList.Count - 2].GetComponent<Paper>();
        }
        else
        {
            fixedPaper = paperList[paperList.Count - 1].GetComponent<Paper>();
        }
    }

    /* return list of vertex converted position local to global */
    public List<Vector3> GetGlobalVertices()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (var v in vertices)
        {
            Vector3 vector = this.transform.TransformPoint(v);
            list.Add(vector);
        }
        return list;
    }

    public int zindex = 0;
    public List<Paper> attachedPaperList; /* for recursive folding */

    /* if Papers share edge, return true else false */
    public static bool PaperAttached(Paper p1, Paper p2)
    {
        for (int i = 0; i < p1.vertices.Count - 1; i++)
        {
            int next_index;
            if (i == p1.vertices.Count - 2)
            {
                next_index = 0;
            }
            else
            {
                next_index = i + 1;
            } /* do not use CoG */

            if (p2.vertices.Contains(p1.vertices[i]) && p2.vertices.Contains(p1.vertices[next_index]))
            {
                return true;
            }
        }
        return false;
    }
    public static HashSet<Paper> Tracking(Paper fix, Paper dynamic, HashSet<Paper> trackgroup)
    {
        trackgroup.Add(dynamic);
        for (int i = 0; i < dynamic.attachedPaperList.Count; i++)
        {
            if (dynamic.attachedPaperList[i] != fix && !trackgroup.Contains(dynamic.attachedPaperList[i]))
            {
                Debug.Log("attachedPaperList find : " + dynamic.attachedPaperList[i].gameObject.name);
                Tracking(fix, dynamic.attachedPaperList[i], trackgroup);
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
            return false; // 직선 상에서 분명 좌표를 안잡았음..
        }

        //오른쪽 페이퍼 만들기 시작
        List<Vector3> newvertices = new List<Vector3>();
        newvertices.Add(startpoint);
        for (int t = startindex + 1; t - 1 != endindex; t++)
        {
            if (t == vertices.Count - 1) t = 0;
            newvertices.Add(vertices[t]);
        }
        newvertices.Add(endpoint);

        makePaper(newvertices);
        //왼쪽 페이퍼 만들기 시작
        List<Vector3> new2vertices = new List<Vector3>();
        new2vertices.Add(endpoint);

        for (int t = endindex + 1; t - 1 != startindex; t++)
        {
            if (t == vertices.Count - 1) t = 0;
            new2vertices.Add(vertices[t]);
        }
        new2vertices.Add(startpoint);

        makePaper(new2vertices);

        //서로 연결
        var tmpPos = gameObject.transform.position;
        var tmpRot = gameObject.transform.rotation;
        paperList[paperList.Count - 2].transform.position = tmpPos;
        paperList[paperList.Count - 2].transform.rotation = tmpRot;
        paperList[paperList.Count - 1].transform.position = tmpPos;
        paperList[paperList.Count - 1].transform.rotation = tmpRot;
        paperList[paperList.Count - 2].GetComponent<Paper>().attachedPaperList.Add(paperList[paperList.Count - 1].GetComponent<Paper>());
        paperList[paperList.Count - 1].GetComponent<Paper>().attachedPaperList.Add(paperList[paperList.Count - 2].GetComponent<Paper>());

        for (int i = 0; i < attachedPaperList.Count; i++)
        {
            if (PaperAttached(paperList[paperList.Count - 2].GetComponent<Paper>(), attachedPaperList[i]))
            {
                paperList[paperList.Count - 2].GetComponent<Paper>().attachedPaperList.Add(attachedPaperList[i]);
            }

            if (PaperAttached(paperList[paperList.Count - 1].GetComponent<Paper>(), attachedPaperList[i]))
            {
                paperList[paperList.Count - 1].GetComponent<Paper>().attachedPaperList.Add(attachedPaperList[i]);
            }
        }
        for (int i = 0; i < paperList.Count - 2; i++)
        {
            if (paperList[i].GetComponent<Paper>().attachedPaperList.Contains(this))
            {
                if (PaperAttached(paperList[i].GetComponent<Paper>(), paperList[paperList.Count - 2].GetComponent<Paper>()))
                    paperList[i].GetComponent<Paper>().attachedPaperList.Add(paperList[paperList.Count - 2].GetComponent<Paper>());
                if (PaperAttached(paperList[i].GetComponent<Paper>(), paperList[paperList.Count - 1].GetComponent<Paper>()))
                    paperList[i].GetComponent<Paper>().attachedPaperList.Add(paperList[paperList.Count - 1].GetComponent<Paper>());
                paperList[i].GetComponent<Paper>().attachedPaperList.Remove(this);
            }
        }
        paperList.Remove(gameObject);
        Destroy(gameObject);
        //TODO : 연결관계 가져오기, 현재의 클래스 지우기

        return true;
    }


    /* if world pos in paper, return pos */
    public static Vector3 in_paper(Paper P, Vector3 pos)
    {
        var globalVertices = P.GetGlobalVertices();
        var equation = makeEquation.make_plane_equation(new List<Vector3>() { globalVertices[0], globalVertices[1], globalVertices[2] });
        pos = new Vector3(pos.x, pos.y, -(equation[0] * pos.x + equation[1] * pos.y + equation[3]) / equation[2]);
        var triangles = P.GetComponent<MeshFilter>().mesh.triangles;
        RaycastHit hit = new RaycastHit(); hit.point = pos;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (makeEquation.in_triangle(pos, new List<Vector3>() { globalVertices[triangles[i]], globalVertices[triangles[i + 1]], globalVertices[triangles[i + 2]] }))
            {
                return pos;
            }

        }
        return new Vector3(-100, -100, -100);
    }


    /* if pos is close to edge, return pos correct pos attatch to edge */
    public static Vector3 attatch_to_edge(Paper P, Vector3 pos)
    {
        var globalVertices = P.GetGlobalVertices();
        float minDistance = 100;
        int minIndex = -1;
        for (int i = 0; i < globalVertices.Count - 1; i++)
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
            if (angle >= 0 && angle <= 90)
            {
                float distance = v1.magnitude * v2.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

        }

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

            if (sphereCount == 0)
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

        if (sphereCount == 2)
            isDragging = true;

        cursorInitialPosition = Input.mousePosition;
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
        }
    }
}

public class FoldPaper : MonoBehaviour
{
    // Start is called before the first frame update
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

    /* raycast to paper ignore convex */
    void setSphere()
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
                Vector3 planePos = Paper.in_paper(p, hit.point);
                if (planePos != new Vector3(-100, -100, -100))
                {
                    Vector3 edgePos = Paper.attatch_to_edge(p, planePos);
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
                    setSphere();
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
    /// 
    /* raycast to paper ignore convex */
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
                Vector3 planePos = Paper.in_paper(p, hit.point);
                if (planePos != new Vector3(-100, -100, -100))
                {

                    Paper.usingPaper = p;
                    if (Paper.paperList[Paper.paperList.Count - 1].name == p.gameObject.name)
                    {
                        Paper.fixedPaper = Paper.paperList[Paper.paperList.Count - 2].GetComponent<Paper>();
                    }
                    else
                    {
                        Paper.fixedPaper = Paper.paperList[Paper.paperList.Count - 1].GetComponent<Paper>();
                    }
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
    /* set rotPositions and rotPaper */
    public void setRot()
    {
        //If not cut foldpaper yet, start cutting it.
        if (!isCut)
        {
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
            find_paper();
            Paper.setFIxedPaper();
            if (Paper.usingPaper != null)
            {
                rotPapers = Paper.Tracking(Paper.fixedPaper, Paper.usingPaper, new HashSet<Paper>());
                string rotPaperstr = "";
                foreach (var p in rotPapers)
                {
                    rotPaperstr += p.gameObject.name + " ";
                }
                Debug.Log("rotpapers : " + rotPaperstr);

                fixedPapers = Paper.Tracking(Paper.usingPaper, Paper.fixedPaper, new HashSet<Paper>());
                string fixPaperstr = "";
                foreach (var p in fixedPapers)
                {
                    fixPaperstr += p.gameObject.name + " ";
                }
                Debug.Log("fixedpapers : " + fixPaperstr);
            }
        }
    }
    /* rotate paper using screen drag */
    public void rotatePaper()
    {
        Vector2 campos1 = Camera.allCameras[0].WorldToScreenPoint(pos1);
        Vector2 campos2 = Camera.allCameras[0].WorldToScreenPoint(pos2);
        float angle = Mathf.Acos(Vector2.Dot(Paper.cursorDeltaPosition, campos2 - campos1) / (((Vector2)Paper.cursorDeltaPosition).magnitude * (campos2 - campos1).magnitude));
        float direction = Mathf.Sign(Vector3.Cross(Paper.cursorDeltaPosition, campos2 - campos1).z);
        float cosine = direction * Mathf.Abs(Mathf.Sin(angle));
        float value = Paper.cursorDeltaPosition.magnitude * cosine / 66;
        if (double.IsNaN(value))
            value = 0;

        //Cross check.
        bool isCrossedL = false;
        bool isCrossedR = false;

        foreach (var p in rotPapers)
        {
            foreach (var q in fixedPapers)
            {
                isCrossedL = CrossCheck(p, q, value);
                if (isCrossedL)
                {
                    break;
                }
            }

        }

        foreach (var p in fixedPapers)
        {
            foreach (var q in rotPapers)
            {
                isCrossedR = CrossCheck(p, q, value);
                if (isCrossedR)
                {
                    break;
                }
            }

        }

        if (!isCrossedL && !isCrossedR)
        {
            foreach (var p in rotPapers)
            {
                p.transform.RotateAround(rotPos2, rotPos2 - rotPos1, value);
            }
        }
    }
    /* check whether two papers are crossed */
    public bool CrossCheck(Paper rotPaper, Paper fixedPaper, float value)
    {

        //Instantiates object.
        GameObject obj = new GameObject();
        obj.transform.SetParent(rotPaper.transform);
        Dot dot = obj.AddComponent<Dot>();

        int count = 0;
        int executed = 0;
        while (count < 100)
        {
            //exception handling: if executed number exceeds 10000, throws an error.
            executed++;
            if (executed > 10000)
            {
                throw new System.Exception("Executed over 10,000 times at count " + count + ". Preventing infinite loop...");
            }

            //Sets position of the object.
            switch (count)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    {
                        Vector3 pos = rotPaper.vertices[count];
                        Vector3 small = pos - rotPaper.vertices[rotPaper.vertices.Count - 1];
                        small *= 1.01f;
                        obj.transform.localPosition = small + rotPaper.vertices[rotPaper.vertices.Count - 1];

                        //GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //temp.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                        //temp.transform.position = obj.transform.position;
                    }
                    break;
            }

            switch (count % 10)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    {
                        Vector3 pos = Vector3.Lerp(rotPaper.vertices[count % 4], rotPaper.vertices[(count + 1) % 4], Random.Range(0f, 1f));
                        Vector3 small = pos - rotPaper.vertices[rotPaper.vertices.Count - 1];
                        small *= 1.01f;
                        obj.transform.localPosition = small + rotPaper.vertices[rotPaper.vertices.Count - 1];

                        //if (Random.Range(0, 4) == 0)
                        //{
                        //    GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //    temp.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                        //    temp.transform.position = obj.transform.position;
                        //}
                    }
                    break;
            }

            {
                obj.transform.localPosition = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0f);
                //If the position of the object is not located to the paper, continue.
                if (Paper.in_paper(rotPaper, obj.transform.position) == new Vector3(-100, -100, -100))
                {
                    continue;
                }
            }

            //Else, Assigns world position to dot instance.
            dot.pos = obj.transform.position;

            count++;

            //Check whether it's crossed.
            if (dot.IsCrossed(rotPaper, fixedPaper, value))
            {
                //Paper.isDragging = false;
                Destroy(obj);
                return true;
            }
        }

        Destroy(obj);
        return false;
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

    public static bool isCut = false;
    public static Vector3 pos1;                 //local positions
    public static Vector3 pos2;
    public static Vector3 rotPos1;              //real positions
    public static Vector3 rotPos2;
    public static HashSet<Paper> rotPapers;     //rotating papers
    public static HashSet<Paper> fixedPapers;   //fixed papers;
    void Update()
    {
        //민석
        //If click occurs,

        if (Input.GetMouseButtonDown(0))
        {
            //If there's  1 or less sphere, and is not cutting paper yet, try to get another sphere.
            if (Paper.sphereCount < 2)
            {
                setSphere();
            }
            //If found all,
            if (Paper.sphereCount == 2)
            {
                setRot();
            }
        }

        //If user is dragging, rotate it.
        if (Paper.isDragging)
        {
            rotatePaper();
        }
    }
}