using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate : MonoBehaviour
{

    GameObject makeSphere(Vector3 pos, Color color)
    {
        /* Make one sphere with hit and Color */
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<SphereCollider>());
        sphere.name = "Sphere ";
        sphere.transform.position = pos;
        sphere.transform.localScale -= new Vector3(0.85f, 0.85f, 0.85f);
        sphere.GetComponent<MeshRenderer>().material.color = color;
        /* End Make one sphere with hit and Color */
        return sphere;
    }

    // Start is called before the first frame update
    Vector3 d1 = new Vector3(1, 2, 3);
    Vector3 d2 = new Vector3(3, 6, 9);
    Vector3 d3 = new Vector3(2, 5, 7);
    GameObject g;
    void Start()
    {
        transform.position = new Vector3(4, 8, 12);
        for (float i = 1.0f; i < 3.0f; i = i + 0.05f)
        {
            makeSphere(new Vector3(i, i * 2, i * 3), Color.red);
        }

        g = makeSphere(d3, Color.blue);

        
    }

    // Update is called once per frame
    void Update()
    {
        g.transform.RotateAround(d1, d2 - d1, 60*Time.deltaTime);
    }
}
