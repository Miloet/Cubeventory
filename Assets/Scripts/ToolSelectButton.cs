
using UnityEngine;
using UnityEngine.UI;

public class ToolSelectButton : MonoBehaviour
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
        button.onClick.AddListener(SetTool);

        drawSurface.updateColor.AddListener(UpdateColor);
    }

    public void SetTool()
    {
        drawSurface.SelectTool(index);
    }
    public void UpdateColor(Color newColor)
    {
        GetComponent<Image>().color = newColor;
    }
}
