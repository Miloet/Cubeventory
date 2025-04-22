using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ChatDisplayImage : MonoBehaviour
{
    public Image image;
    private Texture2D texture;

    public void AssignTexture(Texture2D tex)
    {
        texture = tex;
        image.sprite = DrawSurface.ConvertToSprite(tex);
    }
    public void SaveImage()
    {
        string rootFolderName = "Cubeventory";
        string documentsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string startPath = Path.Combine(documentsFolder, rootFolderName);

        string path =
        StandaloneFileBrowser.SaveFilePanel("Save Image", startPath, "silly_drawing.png", "png");

        if (path != "")
        {
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

    }
}
