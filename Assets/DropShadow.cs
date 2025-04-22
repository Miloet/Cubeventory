using UnityEngine;
using UnityEngine.UI;

public class DropShadow : Shadow
{
    public Image target;
    public Color translate;
    public Color scale;
    protected void OnValidate()
    {
        effectColor = (target.color*scale) + translate;
    }

    protected override void Start()
    {
        effectColor = (target.color * scale) + translate;
    }
}
