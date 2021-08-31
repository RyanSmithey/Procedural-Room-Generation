// Used to generate Texture Array asset
// Menu button is available in GameObject > Create Texture Array
// See CHANGEME in the file
using UnityEngine;
using UnityEditor;

public class TextureArrayGeneration : MonoBehaviour
{
    [MenuItem("GameObject/Create Texture Arrays and Sprites")]
    static void Create()
    {
        for (uint I = 0; I < 5; I++)
        {
            int slices = 11;

            string filePattern = "Materials/Ambient/{0:000}";

            if (I == 0) { filePattern = "Materials/Ambient/{0:000}"; }
            if (I == 1) { filePattern = "Materials/Height/{0:000}"; }
            if (I == 2) { filePattern = "Materials/Metalic/{0:000}"; }
            if (I == 3) { filePattern = "Materials/Normal/{0:000}"; }
            if (I == 4) { filePattern = "Materials/Occlusion/{0:000}"; }


            Texture2DArray textureArray = new Texture2DArray(1024, 1024, slices, TextureFormat.RGB24, false);

            for (int i = 1; i <= slices; i++)
            {
                string filename = string.Format(filePattern, i);
                Texture2D tex = Resources.Load<Texture2D>(filename);
                textureArray.SetPixels(tex.GetPixels(0), i - 1, 0);
            }
            textureArray.Apply();

            string path = "Assets/Resources/AmbientTexs.asset";

            if (I == 0) { path = "Assets/Resources/AmbientTexs.asset"; }
            if (I == 1) { path = "Assets/Resources/HeightTexs.asset"; }
            if (I == 2) { path = "Assets/Resources/MetalicTexs.asset"; }
            if (I == 3) { path = "Assets/Resources/NormalTexs.asset"; }
            if (I == 4) { path = "Assets/Resources/OcclusionTexs.asset"; }
            AssetDatabase.CreateAsset(textureArray, path);
        }
    }

    //Copy Code to find Texture data and convert to sprite on the spot
    [MenuItem("GameObject/Create Texture Sprites")]
    static void CreateSprites()
    {
        int slices = 11;

        string filePattern = "Materials/Ambient/{0:000}";

        string path = "Assets/Resources/Sprites/AmbientSprite{0:000}";
        Sprite sprite;
        for (int i = 1; i <= slices; i++)
        {
            string filename = string.Format(filePattern, i);
            string Path = string.Format(path, i);
            Texture2D tex = Resources.Load<Texture2D>(filename);
            sprite = Sprite.Create(tex, new Rect(0, 0, 100, 100), new Vector2(50, 50), 1f, 0);
            AssetDatabase.CreateAsset(sprite, Path);
        }
    }
}