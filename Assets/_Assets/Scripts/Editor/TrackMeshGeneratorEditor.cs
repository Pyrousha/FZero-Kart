using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackMeshGenerator))]
public class TrackMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TrackMeshGenerator meshGenData = (TrackMeshGenerator)target;

        base.OnInspectorGUI();

        if(GUILayout.Button("Add Max Distance to Widths"))
        {
            meshGenData.CreateMaxWidthPoint();
        }

        if(GUILayout.Button("Generate Track"))
        {
            meshGenData.GenerateTrack();
        }        
    }
}
