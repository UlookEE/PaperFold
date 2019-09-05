using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldButton : MonoBehaviour
{
    public static bool start = false;
    public static Vector3 pos1;
    public static Vector3 pos2;
    public void OnClick()
    {
        Debug.Log("Hello");
        if (Paper.sphereCount == 2)
        {
            
            if (start == false)
            {
                pos1 = Paper.usingPaper.gameObject.transform.Find("Sphere 0").transform.position;
                pos2 = Paper.usingPaper.gameObject.transform.Find("Sphere 1").transform.position;
                Paper.usingPaper.Folding(pos1, pos2);
                start = true;
            }
            else
            {
                Debug.Log(pos1 + " " + pos2);
                Paper.usingPaper.gameObject.transform.RotateAround(pos2, pos2 - pos1, 3);
            }
        }
        //버튼이 클릭되었을 때의 스크립트를 여기에 작성하시면 됩니다.
    }
}
