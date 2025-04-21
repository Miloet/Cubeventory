using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.IO;
using UnityEngine.Rendering;

public class DrawSurface : MonoBehaviour
{
    private ToolSize tool;
    private int color = 2;
    public Color[] colors;
    const int textureSize = 64; 

    public DrawingMenu menu;
    private Texture2D drawingTexture;
    public List<Texture2D> oldTexture = new List<Texture2D>(8);

    RectTransform trans;
    Image image;
    Camera cam;
    Vector2Int lastPos = Vector2Int.one * -1;

    enum ToolSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
    }

    void Start()
    {
        ClearCanvas();
        trans = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        cam = Camera.main;
    }

    private void Update()
    {
        if(!menu.isOpen)
            { return; }
        if (Input.GetButtonDown("Undo") && oldTexture.Count != 0)
        {
            int index = oldTexture.Count - 1;
            drawingTexture = oldTexture[index];
            oldTexture.RemoveAt(index);
            UpdateTexture(drawingTexture);
        }


        if (!IsWithinBoarder())
        {
            lastPos = Vector2Int.one * -1;
            return;
        }

        if (Input.GetButtonDown("Click"))
        {
            var tex = GetTexture();
            tex.CopyPixels(drawingTexture);
            oldTexture.Add(tex);
            if(oldTexture.Count > 8)
                oldTexture.RemoveAt(0);
        }
        if (Input.GetButton("Click"))
        {
            Draw();
        }
        if(Input.GetButtonUp("Click")) lastPos = Vector2Int.one * -1;
    }

    #region Selecting
    public void SelectColor(int index)
    {
        color = index;
    }
    public void SelectTool(int index)
    {
        tool = (ToolSize)index;
    }
    public void ClearCanvas()
    {
        drawingTexture = GetTexture();
    }
    public static Texture2D GetTexture()
    {
        Texture2D tex = new Texture2D(textureSize, textureSize);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply(false);

        return tex;
    }

    #endregion

    #region Drawing

    public void Draw()
    {
        Vector2Int pos = GetPositionOnTexture();
        pos.x = Mathf.Clamp(pos.x, 0, textureSize);
        pos.y = Mathf.Clamp(pos.y, 0, textureSize);

        SetTexture(pos);
        lastPos = pos;
    }

    Vector2Int GetPositionOnTexture()
    {
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 adjusted = Remap(mouse, (Vector2)trans.position - (trans.sizeDelta/2f * trans.lossyScale), trans.sizeDelta.x * trans.lossyScale.x);

        Vector2Int round = new Vector2Int((int)adjusted.x, (int)adjusted.y);

        return round;
    }


    void SetTexture(Vector2Int point)
    {
        //Get all pixels between 
        List<Vector2Int> pixels = new List<Vector2Int>();
        if (lastPos != Vector2Int.one * -1)
        {
            Vector2 dir = ((Vector2)(point - lastPos)).normalized;
            float distance = Vector2.Distance(lastPos, point);

            for(float i = 0; i < distance; i++)
            {
                Vector2Int rounded = lastPos + new Vector2Int(Mathf.RoundToInt(dir.x * i), Mathf.RoundToInt(dir.y * i));
                pixels.Add(rounded);
            }


            pixels = GetSize(pixels);

            foreach(Vector2Int p in pixels)
            {
                drawingTexture.SetPixel(p.x, p.y, colors[color]);
            }

        }
        foreach (Vector2Int p in GetSize(point))
        {
            drawingTexture.SetPixel(p.x, p.y, colors[color]);
        }
        UpdateTexture(drawingTexture);
    }

    void UpdateTexture(Texture2D tex)
    {
        tex.Apply(false);
        image.sprite = ConvertToSprite(tex);
    }
    #endregion

    #region Math

    List<Vector2Int> GetSize(Vector2Int pixel)
    {
        List<Vector2Int> increase = new List<Vector2Int>() { pixel };
        switch (tool)
        {
            default:
            case ToolSize.Small:
                return increase;

            case ToolSize.Medium:

                increase.Add(pixel + new Vector2Int(1, 0));
                increase.Add(pixel + new Vector2Int(-1, 0));
                increase.Add(pixel + new Vector2Int(0, 1));
                increase.Add(pixel + new Vector2Int(0, -1));

                return increase;
            case ToolSize.Large:

                increase.Add(pixel + new Vector2Int(1, 0));
                increase.Add(pixel + new Vector2Int(-1, 0));
                increase.Add(pixel + new Vector2Int(0, 1));
                increase.Add(pixel + new Vector2Int(0, -1));
                increase.Add(pixel + new Vector2Int(1, 1));
                increase.Add(pixel + new Vector2Int(1, -1));
                increase.Add(pixel + new Vector2Int(-1, 1));
                increase.Add(pixel + new Vector2Int(-1, -1));

                return increase;
        }
    }
    List<Vector2Int> GetSize(List<Vector2Int> pixels)
    {
        List <Vector2Int> increase = new List<Vector2Int>(pixels);
        switch (tool)
        {
            default:
            case ToolSize.Small:
                return pixels;

            case ToolSize.Medium:

                foreach(Vector2Int p in pixels)
                {
                    increase.Add(p + new Vector2Int(1,0));
                    increase.Add(p + new Vector2Int(-1,0));
                    increase.Add(p + new Vector2Int(0,1));
                    increase.Add(p + new Vector2Int(0,-1));
                }
                increase = increase.Distinct().ToList();

                return increase;
            case ToolSize.Large:

                foreach (Vector2Int p in pixels)
                {
                    increase.Add(p + new Vector2Int(1, 0));
                    increase.Add(p + new Vector2Int(-1, 0));
                    increase.Add(p + new Vector2Int(0, 1));
                    increase.Add(p + new Vector2Int(0, -1));

                    increase.Add(p + new Vector2Int(1,1));
                    increase.Add(p + new Vector2Int(1,-1));
                    increase.Add(p + new Vector2Int(-1,1));
                    increase.Add(p + new Vector2Int(-1,-1));
                }
                increase = increase.Distinct().ToList();

                return increase;
        }
    }

    public static Vector2 Remap(Vector2 mouse, Vector2 bottemLeft, float canvasWidth)
    {
        Vector2 from = new Vector2(0,0);

        return (mouse - bottemLeft) / (canvasWidth / textureSize);
    }
    public static Sprite ConvertToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
    public bool IsWithinBoarder()
    {
        var rect = trans.rect;
        rect.width *= trans.lossyScale.x;
        rect.height *= trans.lossyScale.x;
        rect.position = (Vector2)trans.position - new Vector2(rect.width, rect.height)/2f;
        
        return rect.Contains(cam.ScreenToWorldPoint(Input.mousePosition));
    }


    #endregion


    #region Saving/Sending

    public void SaveImage()
    {
        string rootFolderName = "Cubeventory";
        string documentsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string startPath = Path.Combine(documentsFolder, rootFolderName);

        string path =
        StandaloneFileBrowser.SaveFilePanel("Save Image", startPath, "silly_drawing.png", "png");

        if(path != "")
        {
            byte[] bytes = drawingTexture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

    }
    public void SendImage()
    {
        ChatSystem.SendPublicImage(drawingTexture);
    }


    #endregion
}
