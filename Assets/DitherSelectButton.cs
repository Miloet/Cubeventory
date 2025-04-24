using UnityEngine;
using UnityEngine.UI;
using static DrawSurface;

public class DitherSelectButton : MonoBehaviour
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
        button.onClick.AddListener(SetDither);
    }

    public void SetDither()
    {
        drawSurface.dither =index; 
    }

}
