
using UnityEngine;
using UnityEngine.UI;

public class ToolSelectButton : MonoBehaviour
{
    public DrawSurface drawSurface;
    public int index;
    private void OnValidate()
    {
        var button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SetTool);
    }

    public void SetTool()
    {
        drawSurface.SelectTool(index);
    }
}
