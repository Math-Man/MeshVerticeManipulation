using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class STHelper
{

    public static List<Collider2D> GetCollidersInRange(Vector3 position, float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);
        colliders = OrderCollidersByDistance(position, colliders);
        return colliders.ToList();
    }


    public static Collider2D[] OrderCollidersByDistance(Vector3 position, Collider2D[] colliders)
    {
        return colliders.OrderBy(col => Vector3.Distance(col.transform.position, position)).ToArray();
    }

    public static Vector3 GetMouseWorldPosition(Camera cameraMain)
    {
        Vector3 vec = GetMouseWorldPositionWithZ(cameraMain);
        vec.z = 0f;
        return vec;
    }

    public static GameObject GetObjectOnCursorPosition(Camera cameraMain) 
    {
        RaycastHit hit;
        var ray = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform.gameObject;
        }
        else return null;
    }

    public static GameObject GetObjectOnCursorPosition2D(Camera cameraMain) 
    {
        //RaycastHit2D hit2 = Physics2D.Raycast(Input.mousePosition, Camera.main.transform.position - Input.mousePosition, 0, 001, 1000);
        RaycastHit2D hit = Physics2D.Raycast(cameraMain.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
            return hit.transform.gameObject;
        }
        else return null;

    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera cam)
    {
        return GetWorldPositionWithZ(Input.mousePosition, cam);
    }

    public static Vector3 GetWorldPositionWithZ(Vector3 screenPosition, Camera camera)
    {
        return camera.ScreenToWorldPoint(screenPosition);
    }

    public static TextMesh createWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 355, Color color = default(Color), TextAnchor anchor = TextAnchor.MiddleCenter, TextAlignment alignment = TextAlignment.Center, int sortingOrder = 0)
    {
        GameObject gameObject = new GameObject("world_text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = anchor;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = 0.01f;
        textMesh.color = color;
        textMesh.alignment = alignment;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

        return textMesh;
    }


    public static GameObject CreateCircleHighlighter(GameObject objWithLineRenderer, Transform targetParent, float radius, Color color1, Color color2, float alphaMultiplier = 1.0f, int edgeCount = 50)
    {
        GameObject obj = GameObject.Instantiate(objWithLineRenderer, targetParent.transform.position, Quaternion.identity, targetParent);
        LineRenderer renderer = obj.GetComponent<LineRenderer>();
        renderer.positionCount = edgeCount;
        renderer.useWorldSpace = false;
        renderer.loop = true;
        renderer.material.SetFloat("_AlphaMultiplier", alphaMultiplier);

        float x, y, z;
        float angle = 20f;
        for (int i = 0; i < (edgeCount); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            renderer.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / edgeCount);
        }

        var colorKey = new GradientColorKey[2];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color2;
        colorKey[1].time = 1.0f;
        var alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;
        renderer.startColor = color1;
        renderer.endColor = color2;
        renderer.colorGradient.SetKeys(colorKey, alphaKey);

        return obj;
    }


    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }



}
