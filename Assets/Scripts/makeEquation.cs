using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeEquation : MonoBehaviour
{
    public static List<float> make_plane_equation(List<Vector3> vertices)
    {
        float x1 = vertices[0].x; float y1 = vertices[0].y; float z1 = vertices[0].z;
        float x2 = vertices[1].x; float y2 = vertices[1].y; float z2 = vertices[1].z;
        float x3 = vertices[2].x; float y3 = vertices[2].y; float z3 = vertices[2].z;

        float A = y1 * (z2 - z3) + y2 * (z3 - z1) + y3 * (z1 - z2);
        float B = z1 * (x2 - x3) + z2 * (x3 - x1) + z3 * (x1 - x2);
        float C = x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2);
        float D = -(x1 * (y2 * z3 - y3 * z2) + x2 * (y3 * z1 - y1 * z3) + x3 * (y1 * z2 - y2 * z1));

        var retList = new List<float>();
        retList.Add(A);
        retList.Add(B);
        retList.Add(C);
        retList.Add(D);

        //Debug.Log("Plane Equation Result:  A : " + A + ", " + "B : " + B + ", " + "C : " + C + ", " + "D : " + D + ", ");
        return retList;
    }

    public static List<float> make_line_equation(Vector2[] vertices)
    {
        float x1 = vertices[0].x; float y1 = vertices[0].y;
        float x2 = vertices[1].x; float y2 = vertices[1].y;

        float A = y1 - y2;
        float B = x2 - x1;
        float C = x1 * y2 - x2 * y1;

        var retList = new List<float>();
        retList.Add(A);
        retList.Add(B);
        retList.Add(C);

        //Debug.Log("A : " + A + ", " + "B : " + B + ", " + "C : " + C + ", ");
        return retList;
    }
    public static bool in_plane(List<float> equation, Vector3 vertex)
    {
        float A = equation[0]; float B = equation[1]; float C = equation[2]; float D = equation[3];
        //Debug.Log("Is It On The Plane? :  " + ((A * vertex.x + B * vertex.y + C * vertex.z + D) == 0));
        return (A * vertex.x + B * vertex.y + C * vertex.z + D) == 0;
    }

    public static float DotToPlaneDistance(List<float> equation, Vector3 vertex)
    {
        float A = equation[0]; float B = equation[1]; float C = equation[2]; float D = equation[3];
        //Debug.Log("Is It On The Plane? :  " + ((A * vertex.x + B * vertex.y + C * vertex.z + D) == 0));
        return (A * vertex.x + B * vertex.y + C * vertex.z + D);
    }

    public static float in_line(List<float> equation, Vector2 vertex)
    {
        float A = equation[0]; float B = equation[1]; float C = equation[2];
        return A * vertex.x + B * vertex.y + C;
    }

    public static bool in_triangle2(List<float> equation, Vector3 dot, List<Vector3> vertices)
    {
        var vertices2D = new Vector2[3];
        var allEqual = new bool[] { true, true, true };
        var first_element = new float[] { vertices[0].x, vertices[0].y, vertices[0].z };
        for (int i = 0; i < 3; i++)
        {
            for (int j = 1; j < 3; j++)
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
                    //Debug.Log(vertices2D[j]);

                }
                break;
            }
        }

        var line1 = make_line_equation(new Vector2[] { vertices2D[0], vertices2D[1] });
        var line2 = make_line_equation(new Vector2[] { vertices2D[1], vertices2D[2] });
        var line3 = make_line_equation(new Vector2[] { vertices2D[2], vertices2D[0] });

       
        if (in_line(line1, vertices2D[2]) * in_line(line1, new Vector2(dot.x, dot.y)) >= 0 &&
            in_line(line2, vertices2D[0]) * in_line(line2, new Vector2(dot.x, dot.y)) >= 0 &&
            in_line(line3, vertices2D[1]) * in_line(line3, new Vector2(dot.x, dot.y)) >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void in_vertices(List<float> equation, Vector3[] vertices)
    {
        var vertices2D = new Vector2[3];
        var allEqual = new bool[] { true, true, true };
        var first_element = new float[] { vertices[0].x, vertices[0].y, vertices[0].z };
        for (int i = 0; i < 3; i++)
        {
            for (int j = 1; j < 3; j++)
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
                    //Debug.Log(vertices2D[j]);

                }
                break;
            }
        }

        var line1 = make_line_equation(new Vector2[] { vertices2D[0], vertices2D[1] });
        var line2 = make_line_equation(new Vector2[] { vertices2D[1], vertices2D[2] });
        var line3 = make_line_equation(new Vector2[] { vertices2D[2], vertices2D[0] });

        float max_x = vertices2D[0].x, min_x = vertices2D[0].x, max_y = vertices2D[0].y, min_y = vertices2D[0].y;
        for (int i = 1; i < 3; i++)
        {
            if (vertices2D[i].x > max_x)
                max_x = vertices2D[i].x;
            if (vertices2D[i].x < min_x)
                min_x = vertices2D[i].x;
            if (vertices2D[i].y > max_y)
                max_y = vertices2D[i].y;
            if (vertices2D[i].y < min_y)
                min_y = vertices2D[i].y;
        }
        int count = 0;
        for (float i = min_x; i < max_x; i += 0.5f)
        {
            for (float j = min_y; j < max_y; j += 0.5f)
            {
                if (in_line(line1, vertices2D[2]) * in_line(line1, new Vector2(i, j)) >= 0 &&
                    in_line(line2, vertices2D[0]) * in_line(line1, new Vector2(i, j)) >= 0 &&
                    in_line(line3, vertices2D[1]) * in_line(line1, new Vector2(i, j)) >= 0)
                {
                    Vector3 point;
                    if (allEqual[0])
                    {
                        point = new Vector3(0, i, j);
                        float x = calc_plane_equation(equation, point) / equation[0];
                        point.x = x;

                    }
                    else if (allEqual[1])
                    {
                        point = new Vector3(j, 0, i);
                        float y = calc_plane_equation(equation, point) / equation[1];
                        point.y = y;
                    }
                    else
                    {
                        point = new Vector3(i, j, 0);
                        float z = calc_plane_equation(equation, point) / equation[2];
                        point.z = z;
                    }
                    //List<float> newEquation = make_plane_equation(new Vector3[] { vertices[0], vertices[1], point });
                    count++;
                }
            }
        }
        //Debug.Log(count);
    }
    // Start is called before the first frame update

    public static float calc_plane_equation(List<float> equation, Vector3 vertex)
    {
        float A = equation[0]; float B = equation[1]; float C = equation[2]; float D = equation[3];
        return -(A * vertex.x + B * vertex.y + C * vertex.z + D);
        // ax + by + cz + d = 0
    }

    public static bool in_triangle(Vector3 dot, List<Vector3> vertex)
    {
        Vector3 d1 = vertex[1] - vertex[0];
        Vector3 d2 = vertex[2] - vertex[1];
        Vector3 d3 = vertex[0] - vertex[2];

        Vector3 p1 = dot - vertex[0];
        Vector3 p2 = dot - vertex[1];
        Vector3 p3 = dot - vertex[2];

        if (
            Vector3.Dot(Vector3.Cross(d1, p1), Vector3.Cross(p1, -d3)) >= 0 &&
            Vector3.Dot(Vector3.Cross(d2, p2), Vector3.Cross(p2, -d1)) >= 0 &&
            Vector3.Dot(Vector3.Cross(d3, p3), Vector3.Cross(p3, -d2)) >= 0
            )
        {
            return true;
        }
        else return false;
    }

    public static bool in_line_3d(Vector3 dot, List<Vector3> line)
    {
        Vector3 d = line[1] - line[0];
        Vector3 p = dot - line[0];
        return Vector3.Cross(d, p) == Vector3.zero;
    }

    void Start()
    {


    }

    void Update()
    {

    }
}