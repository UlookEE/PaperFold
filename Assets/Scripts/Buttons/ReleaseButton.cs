using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseButton : MonoBehaviour
{
    public void OnClick()
    {
        Paper.sphereCount = 0;
        FoldButton.start = false;
        FoldPaper.isCut = false;
    }
}
