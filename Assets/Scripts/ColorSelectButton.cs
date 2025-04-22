
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectButton : MonoBehaviour
{
    public DrawSurface drawSurface;
    public int index;
    private void OnValidate()
    {
        SetButton();
    }


    private void Start()
    {
        SetButton();
    }

    private void SetButton()
    {
        var button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SetColor);

        GetComponent<Image>().color = drawSurface.colors[index];
    }

    public void SetColor()
    {
        drawSurface.SelectColor(index);
    }
}
