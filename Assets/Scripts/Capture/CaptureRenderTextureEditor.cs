using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CaptureSprite))]
public class CaptureSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CaptureSprite captureScript = (CaptureSprite)target;

        if (GUILayout.Button("Capture Sprite"))
        {
            Capture(captureScript);
        }
    }

    private void Capture(CaptureSprite script)
    {
        Camera cam = script.renderCamera;
        RenderTexture rt = script.renderTexture;

        if (cam == null || rt == null)
        {
            Debug.LogError("Camera or RenderTexture is missing!");
            return;
        }

        cam.targetTexture = rt;
        RenderTexture.active = rt;

        cam.Render();

        Texture2D image = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        image.Apply();

        // Create directory if it doesn't exist
        string folderPath = Application.dataPath + "/Sprites";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename
        string baseName = "CapturedSprite";
        string path;
        int index = 1;
        do
        {
            path = $"{folderPath}/{baseName}_{index}.png";
            index++;
        } while (File.Exists(path));

        // Save PNG
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Debug.Log("Sprite captured and saved to: " + path);

        // Refresh to show new file in Assets
        AssetDatabase.Refresh();


        // Thiết lập lại Texture Import Settings
        string assetPath = "Assets/Sprites/" + Path.GetFileName(path);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            Debug.Log("Texture imported and set as Sprite (Single).");
        }
        else
        {
            Debug.LogError("Cannot find TextureImporter for path: " + assetPath);
        }
    }
}
