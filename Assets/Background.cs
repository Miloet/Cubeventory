using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
public class Background : MonoBehaviour
{
    private Material mat;

    public FlexibleColorPicker primaryColorPicker;
    public FlexibleColorPicker secondaryColorPicker;
    public Slider lerpSlider;

    private static Sprite sprite;
    private static float lerp;
    private static Color color1;
    private static Color color2;

    private void Awake()
    {
        mat = GetComponent<Image>().material;
        Load();
        Set();

        if (primaryColorPicker != null)
        {
            primaryColorPicker.color = color1;
            secondaryColorPicker.color = color2;
            lerpSlider.value = lerp;

            primaryColorPicker.gameObject.SetActive(true);
            secondaryColorPicker.gameObject.SetActive(true);
            lerpSlider.gameObject.SetActive(true);
        }
    }

    public void Set()
    {
        if (sprite != null) GetComponent<Image>().sprite = sprite;
        mat.SetColor("_Color1", color1);
        mat.SetColor("_Color2", color2);
        mat.SetFloat("_Lerp", lerp);
    }
    public void UpdateColors()
    {
        color1 = primaryColorPicker.color;
        color2 = secondaryColorPicker.color;
        lerp = lerpSlider.value;
        Save();
    }


    public void Load()
    {
        if (sprite == null)
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = "Cubeventory";
            string file = "Background.png";
            string fullPath = Path.Combine(documents, path, file);
            if (File.Exists(fullPath))
            {
                var fileData = File.ReadAllBytes(fullPath);
                var texture = new Texture2D(1920, 1080);
                texture.LoadImage(fileData);

                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
            else sprite = null;
        }

        color1 =
            new Color(
                PlayerPrefs.GetFloat("Color1_r", 1f),
                PlayerPrefs.GetFloat("Color1_g", 184f/255f),
                PlayerPrefs.GetFloat("Color1_b", 200f/255f)
                );
        color2 =
            new Color(
                PlayerPrefs.GetFloat("Color2_r", 238f/255f),
                PlayerPrefs.GetFloat("Color2_g", 177f/255f),
                PlayerPrefs.GetFloat("Color2_b", 1f)
                );

        lerp = PlayerPrefs.GetFloat("Lerp", 0.35f);
    }

    public void Save()
    {

        PlayerPrefs.SetFloat("Color1_r", color1.r);
        PlayerPrefs.SetFloat("Color1_g", color1.g);
        PlayerPrefs.SetFloat("Color1_b", color1.b);

        PlayerPrefs.SetFloat("Color2_r", color2.r);
        PlayerPrefs.SetFloat("Color2_g", color2.g);
        PlayerPrefs.SetFloat("Color2_b", color2.b);


        PlayerPrefs.SetFloat("Lerp", lerp);
    }
}
