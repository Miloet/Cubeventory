using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoSelection : MonoBehaviour, IPointerDownHandler
{
    void Start()
    {
        Selectable[] select = FindObjectsByType<Selectable>(FindObjectsSortMode.None);

        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;

        foreach (Selectable selectable in select)
        {
            selectable.navigation = nav;
        }

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        var button = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if(button != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
