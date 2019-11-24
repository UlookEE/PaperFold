using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dot class for collision determination.
/// </summary>
public class Dot : MonoBehaviour
{
    public Vector3 pos;                                 //Current position of the dot in world position.

    /// <summary>
    /// Determines whether a dot crossed over a paper.
    /// </summary>
    /// <returns></returns>
    public bool IsCrossed(Paper rotPaper, Paper fixedPaper, float value)
    {
        List<float> equation;
        Vector3 normalPaper;
        float distance;
        //Calculates prev value of the plane equation.
        equation = makeEquation.make_plane_equation(fixedPaper.GetGlobalVertices());    //This is world coordinates.
        normalPaper = new Vector3(equation[0], equation[1], equation[2]);
        //Debug.Log(normalPaper.x + " " + normalPaper.y + " " + normalPaper.z);

        distance = makeEquation.DotToPlaneDistance(equation, pos);

        float beforeFloatRAW = distance;
        float beforeFloat = beforeFloatRAW / Mathf.Abs(beforeFloatRAW);
        int before = Mathf.RoundToInt(beforeFloat);

        //Instantiates temporary object.
        GameObject obj = new GameObject();
        //  obj.transform.SetParent(rotPaper.transform);
        obj.transform.position = pos;
        obj.transform.RotateAround(FoldPaper.rotPos2, FoldPaper.rotPos2 - FoldPaper.rotPos1, value < 5 ? 7.5f : value * 1.5f);

        //Calculates next value of the plane equation.
        distance = makeEquation.DotToPlaneDistance(equation, obj.transform.position);

        Vector3 afterPos = obj.transform.position;

        float afterFloatRAW = distance;
        float afterFloat = afterFloatRAW / Mathf.Abs(afterFloatRAW);
        int after = Mathf.RoundToInt(afterFloat);

        //Make a line equation, from before dot to after dot.
        float t = -(Vector3.Dot(normalPaper, obj.transform.position) + equation[3]) / Mathf.Pow(normalPaper.magnitude, 2);
        Vector3 pointAtPaper_Global = normalPaper * t + pos;
        obj.transform.position = pointAtPaper_Global;

        Destroy(obj);

        if (Paper.in_paper(fixedPaper, pointAtPaper_Global) == new Vector3(-100, -100, -100))
        {
            return false;
        }

        //Debug.Log("CROSSED! BEFORE: " + Mathf.Asin(beforeFloatRAW) * 180 / Mathf.PI + " AFTER: " + Mathf.Asin(afterFloatRAW) * 180 / Mathf.PI);

        ////This is for debug.
        ////Draws before-after line.
        //if (before * after == -1)
        //{
        //    //Debug.Log("CROSSED! BEFORE: " + Mathf.Asin(beforeFloatRAW) * 180 / Mathf.PI + " AFTER: " + Mathf.Asin(afterFloatRAW) * 180 / Mathf.PI);

        //    GameObject tempbef = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    tempbef.transform.position = pos;
        //    tempbef.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        //    GameObject line = new GameObject();
        //    LineRenderer ren = line.AddComponent<LineRenderer>();
        //    ren.SetPositions(new Vector3[] {
        //        pos, afterPos
        //    });
        //    ren.startWidth = 0.001f;
        //    ren.endWidth = 0.001f;
        //}

        //Result would be true if before and after are different.
        //Which means, after it moved, the dot crossed over the plane.

        return before * after == -1;
    }
}