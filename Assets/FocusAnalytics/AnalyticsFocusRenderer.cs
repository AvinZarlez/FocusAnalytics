using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Microsoft.WindowsAzure.MobileServices;


[CustomEditor(typeof(AnalyticsFocusRenderer))]
public class AnalyticsFocusRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        if (GUILayout.Button("Render Table"))
        {
            AnalyticsFocusRenderer.instance.GenerateFocusRenderObjects();
        }
    }
}

public class AnalyticsFocusRenderer : MonoBehaviour
{
    [HideInInspector]
    static public AnalyticsFocusRenderer instance = null;

    public GameObject analyticsFocusObject;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogError("ERROR: Another AnalyticsFocusReporter exists alraedy in the scene");
            Destroy(gameObject);
        }
    }

    public async void GenerateFocusRenderObjects()
    {
        try
        {
            IMobileServiceTable<ReportableFocusEvent> tbl = AnalyticsFocusReporter.GetTable();

            List<ReportableFocusEvent> list = await tbl.ToListAsync();
            foreach (ReportableFocusEvent item in list)
            {
                Debug.Log($"{item.Id} - {item.Label} - {item.Position}");
                String[] pos = item.Position.Split(',');
                Assert.IsTrue(pos.Length >= 3);
                GameObject obj = (GameObject)Instantiate(analyticsFocusObject, transform);
                Debug.Log("" + float.Parse(pos[0]) + " " + float.Parse(pos[1]) + " " + float.Parse(pos[2]));
                obj.transform.position = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}
