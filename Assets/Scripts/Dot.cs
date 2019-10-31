using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dot class for collision determination.
/// </summary>
public class Dot : MonoBehaviour
{
    public static float deltaRadian = Mathf.PI / 36;    //Amount of rotation of next dot position.

    public Paper paper;                                 //Paper that includes this dot.
    public Vector3 pos;                                 //Current position of the dot in world position.

    /// <summary>
    /// Determines whether a dot crossed over a paper.
    /// </summary>
    /// <returns></returns>
    public bool IsCrossed()
    {
        return false;
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