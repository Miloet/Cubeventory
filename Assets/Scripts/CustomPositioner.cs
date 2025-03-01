using UnityEngine;

public class CustomPositioner : MonoBehaviour
{
    public RectTransform trans;

    public Vector2 offset;
    public Vector2 rectMult;

    public RectTransform copySize;

    private void OnValidate()
    {
        trans = GetComponent<RectTransform>();
        SetPosition();
    }

    public void SetPosition()
    {
        trans.localPosition = offset + trans.sizeDelta * rectMult;
        trans.sizeDelta = copySize.sizeDelta;
    }

    private void Update()
    {
        SetPosition();
    }
}
