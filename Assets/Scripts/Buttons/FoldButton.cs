using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldButton : MonoBehaviour
{
    public static bool start = false;
    public static Vector3 pos1;
    public static Vector3 pos2;
    public static Vector3 rotPos1;
    public static Vector3 rotPos2;
    public void OnClick()
    {
        if (Paper.sphereCount == 2)
        {
            
            if (start == false)
            {
                var gameObjectOfPaper = Paper.usingPaper.gameObject; 
                rotPos1 = gameObjectOfPaper.transform.Find("Sphere 0").transform.position;
                rotPos2 = gameObjectOfPaper.transform.Find("Sphere 1").transform.position;
                
                var tmpRot = gameObjectOfPaper.transform.rotation;
                gameObjectOfPaper.transform.rotation = Quaternion.Euler(0, 0, 0);
                pos1 = gameObjectOfPaper.transform.Find("Sphere 0").localPosition;
                pos2 = gameObjectOfPaper.transform.Find("Sphere 1").localPosition;
                gameObjectOfPaper.transform.rotation = tmpRot;
                Paper.usingPaper.Folding(pos1, pos2);
                start = true;
            }
            else
            {
                //Debug.Log(pos1 + " " + pos2);
                Paper.usingPaper.gameObject.transform.RotateAround(pos2, pos2 - pos1, 3);
            }
        }
    }
}
