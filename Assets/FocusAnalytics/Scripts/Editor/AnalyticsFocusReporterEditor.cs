using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnalyticsFocusReporter))]
public class AnalyticsFocusReporterEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (Application.isPlaying)
		{
			if (GUILayout.Button("Run Report"))
			{
				((AnalyticsFocusReporter)target).ClearReport();
				((AnalyticsFocusReporter)target).RunReport();
			}

			if (GUILayout.Button("Clear Report"))
			{
				((AnalyticsFocusReporter)target).ClearReport();
			}
		}
	}
}
