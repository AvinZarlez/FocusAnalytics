using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity.InputModule;

/// <summary>
/// 1. ReportableFocusEvent
/// </summary>
[RequireComponent(typeof(Collider))]
public class AnalyticsFocusTarget : MonoBehaviour, IFocusable
{
    private Color focusedColor = Color.red;

    private DateTimeOffset focusEnterTime;

    private DateTimeOffset focusExitTime;

    private Color originalColor;

    private Material material;

    public bool VisualizeGaze = false;

    public string Label;

    void Awake()
    {
        material = GetComponent<Renderer>().material;

        if (material != null)
        {
            originalColor = material.GetColor("_Color");
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnFocusEnter()
    {
        this.focusEnterTime = DateTimeOffset.UtcNow;

        if (this.VisualizeGaze && material != null)
        {
            material.SetColor("_Color", focusedColor);
        }
    }

    public void OnFocusExit()
    {
        this.focusExitTime = DateTimeOffset.UtcNow;

        Vector3 position = this.gameObject.transform.position;

        if (this.VisualizeGaze && material != null)
        {
            material.SetColor("_Color", originalColor);
        }

        ReportableFocusEvent report = new ReportableFocusEvent(HardwareIdentification.GetPackageSpecificToken(), this.Label, this.focusEnterTime, this.focusExitTime, position);

        AnalyticsFocusReporter.InsertReportableFocusEvent(report);
    }
}
