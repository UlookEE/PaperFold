using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dot class for collision determination.
/// </summary>
public class Dot : MonoBehaviour
{
    public static float deltaRadian = Mathf.PI / 36;    //Amount of rotation of next dot position.

    public Vector3 pos;                                 //Current position of the dot in world position.

    /// <summary>
    /// Determines whether a dot crossed over a paper.
    /// </summary>
    /// <returns></returns>
    public bool IsCrossed(Paper rotPaper, Paper fixedPaper, float value)
    {
        //Calculates prev value of the plane equation.
        List<float> equation = makeEquation.make_plane_equation(fixedPaper.vertices);    //This is local coordinates.
        Vector3 normalPaper = Paper.usingPaper.transform.TransformVector(new Vector3(equation[0], equation[1], equation[2]));   //This is world coordinates.
        float beforeFloat = Vector3.Dot(normalPaper, pos);
        beforeFloat /= Mathf.Abs(beforeFloat);
        int before = Mathf.RoundToInt(beforeFloat);

        //Instantiates temporary object.
        GameObject obj = new GameObject();
        obj.transform.SetParent(rotPaper.transform);
        obj.transform.position = pos;
        obj.transform.RotateAround(FoldPaper.rotPos2, FoldPaper.rotPos2 - FoldPaper.rotPos1, value * 1f);

        //Calculates next value of the plane equation.
        float afterFloat = Vector3.Dot(normalPaper, obj.transform.position);
        afterFloat /= Mathf.Abs(afterFloat);
        int after = Mathf.RoundToInt(afterFloat);

        //Make a line equation, from before dot to after dot.
        float t = -(Vector3.Dot(normalPaper, obj.transform.position) + equation[3]) / Mathf.Pow(normalPaper.magnitude, 2);
        Vector3 pointAtPaper_Global = normalPaper * t + pos;
        obj.transform.position = pointAtPaper_Global;

        Destroy(obj);

        if(Paper.in_paper(fixedPaper, pointAtPaper_Global) == new Vector3(-100, -100, -100)) return false;

        //Result would be true if before and after are different.
        //Which means, after it moved, the dot crossed over the plane.
        return before * after == -1; 
    }

    // NOTE:: This is for calculation without monobehaviour.

    ///// <summary>
    ///// Determines whether a dot crossed over a paper.
    ///// </summary>
    ///// <returns></returns>
    //public bool IsCrossed()
    //{
    //    Vector3 origin = FoldPaper.rotPos1;                             //Origin of coordinate.
    //    Vector3 direction = FoldPaper.rotPos2 - FoldPaper.rotPos1;      //Direction vector of axis.
    //    Vector3 operand = pos - origin;                                 //Operand vector.

    //    //1. Calculate foot of perpendicular(FOP) from dot to direction vector. (수선의 발)

    //    //Get a cosine between direction vector and operand vector.
    //    float cosine = Vector3.Dot(direction, operand) / (direction.magnitude * operand.magnitude);

    //    //Get a magnitude of FOP.
    //    float magnitude = operand.magnitude * cosine;

    //    //Get a FOP.
    //    Vector3 fop = direction.normalized * magnitude + origin;

    //    //2. Calculate a temporary rotated dot.

    //    //Get a local position of next dot position.
    //    Vector3 nextPosLocal = new Vector3((pos - fop).magnitude * Mathf.Cos(deltaRadian), Mathf.Sin(deltaRadian));

    //    Vector3 nextPos;                                        //Prediction of next position.

    //}
}