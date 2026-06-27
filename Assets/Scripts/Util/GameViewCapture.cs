using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameViewCapture : MonoBehaviour
{
    [Header("Output")]
    [Tooltip("Folder path relative to the project root, e.g. Assets/Screenshots")]
    public string outputFolder = "Assets/Screenshots";

    [Tooltip("Filename prefix. Timestamp is appended automatically.")]
    public string filePrefix = "screenshot";

    [Header("Resolution Multiplier")]
    [Tooltip("1 = native Game View resolution, 2 = 2x, etc.")]
    [Range(1, 4)]
    public int superSize = 1;

    public void Capture()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            Debug.Log($"[GameViewCapture] Created folder: {outputFolder}");
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"{filePrefix}_{timestamp}.png";
        string fullPath = Path.Combine(outputFolder, filename);

        ScreenCapture.CaptureScreenshot(fullPath, superSize);
        Debug.Log($"[GameViewCapture] Saved → {fullPath}");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameViewCapture))]
public class GameViewCaptureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw all the serialised fields as normal
        DrawDefaultInspector();

        GUILayout.Space(8);

        var capture = (GameViewCapture)target;

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);   // soft green button
        if (GUILayout.Button("📷  Capture Game View", GUILayout.Height(32)))
        {
            // Works in Edit Mode and Play Mode
            capture.Capture();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(4);
        EditorGUILayout.HelpBox(
            "Screenshots are saved to the Output Folder.\n" +
            "The file is refreshed in the Project window automatically.",
            MessageType.Info);
    }
}
#endif