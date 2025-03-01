using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescription : MonoBehaviour
{
    RectTransform rect;
    public RectTransform followRotation;
    public float vertOffset = 15;

    public TextMeshProUGUI text_name;
    public TextMeshProUGUI text_description;
    public GameObject linkButton;
    string link;

    public float positionLerp;
    public float flatSpeed;

    public GameObject visual;

    public static ItemDescription instance;

    private bool on;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        instance = this;
    }

    public void OnItemChange(Item item)
    {
        text_name.text = item.name;
        text_description.text = item.description;
        link = item.link;
        linkButton.SetActive(link != "");
    }
    public void OpenItemLink()
    {
        Application.OpenURL(link);
    }

    void Update()
    {
        if(Item.lastHoverOver != null && on)
        {
            visual.SetActive(true);
            var trans = Item.lastHoverOver.visual.transform;
            var size = (RectTransform)Item.lastHoverOver.transform;

            var targetPos = trans.position + new Vector3(0, vertOffset +
                rect.sizeDelta.y / 2f + followRotation.sizeDelta.y + size.sizeDelta.y / 2f);

            float distance = Vector2.Distance(transform.position, targetPos) * positionLerp;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, (distance + flatSpeed) * Time.deltaTime);
            
            transform.rotation = trans.rotation;
            followRotation.rotation = trans.rotation;
        }
        else visual.SetActive(false);
    }

    public void Switch()
    {
        on = !on;
        Item.lastHoverOver = null;
    }
}
