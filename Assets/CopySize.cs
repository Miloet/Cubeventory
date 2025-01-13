using UnityEngine;

public class CopySize : MonoBehaviour
{
    public RectTransform copyFrom;
    private RectTransform self;

    public float offset;
    private void Start()
    {
        self = GetComponent<RectTransform>();
    }
    void Update()
    {
         self.sizeDelta = copyFrom.sizeDelta + new Vector2(offset, offset);
    }
}
