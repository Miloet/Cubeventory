using UnityEngine;

public class CustomPositioner : MonoBehaviour
{
    public RectTransform trans;

    public Vector2 offset;
    public Vector2 rectMult;

    private void OnValidate()
    {
        trans = GetComponent<RectTransform>();
        SetPosition();
    }

    public void SetPosition()
    {
        trans.localPosition = offset + trans.sizeDelta * rectMult;
    }

    private void Update()
    {
        SetPosition();
    }
}
