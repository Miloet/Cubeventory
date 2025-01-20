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


    public static ItemDescription instance;

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
        if(Item.lastPickedUp != null)
        {
            var trans = Item.lastPickedUp.visual.transform;
            var size = (RectTransform)Item.lastPickedUp.transform;
            transform.position = trans.position + new Vector3(0, vertOffset + 
                rect.sizeDelta.y/2f + followRotation.sizeDelta.y + size.sizeDelta.y/2f);
            transform.rotation = trans.rotation;
            followRotation.rotation = trans.rotation;
        }
        else gameObject.SetActive(false);
    }

    public void Switch()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
}
