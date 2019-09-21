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
                var posList = FoldPaper.spherePosLocal(Paper.usingPaper.gameObject);
                rotPos1 = posList[0];
                rotPos2 = posList[1];
                pos1 = posList[2];
                pos2 = posList[3];
                Paper.usingPaper.Folding(pos1, pos2);
                start = true;
            }
            else
            {
                Paper.usingPaper.gameObject.transform.RotateAround(rotPos2, rotPos2 - rotPos1, 3);
            }
        }
    }
}
