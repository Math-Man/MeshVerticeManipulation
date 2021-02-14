using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private MeshRenderer mr;
    public float paralax = 1.0f;
    public float shuffleX = 0f;
    public float shuffleY = 0f;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
    }

    void FixedUpdate()
    {
        Material mat = mr.materials[0];
        Vector2 offset = mat.GetTextureOffset("_MainTex");

        float shiftRndX = 0f;
        float shiftRndY = 0f;
        if (shuffleX != 0 || shuffleX != 0)
        {
            shiftRndX = Mathf.Sin(Time.time) * shuffleX;
            shiftRndY = Mathf.Cos(Time.time) * shuffleX;
        }

        offset.x = (transform.position.x / transform.localScale.x / paralax) + shiftRndX;
        offset.y = (transform.position.y / transform.localScale.y / paralax) + shiftRndY;

        mat.mainTextureOffset = offset;
    }
}
