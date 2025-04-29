using System;
using UnityEngine;

public class DrawingMenu : MonoBehaviour
{
    [NonSerialized] public bool isOpen = false;
    [NonSerialized] public bool isFullscreen = false;
    RectTransform parent;
    RectTransform trans;
    public GameObject expandButton;
    public Animator fullscreenButton;
    public RuntimeAnimatorController fullscreenController;
    public RuntimeAnimatorController smallscreenController;

    private Vector2 closedPosition;
    private Vector2 openPosition;

    private Vector3 parentOriginalPos;

    public Canvas canvas;
    private void Start()
    {
        openPosition =      new Vector2(0, 45);
        closedPosition =    new Vector2(0, 370);

        

        trans = GetComponent<RectTransform>();
        parent = (RectTransform) trans.parent;
        parentOriginalPos = parent.anchoredPosition;
    }

    private void Update()
    {

        if(isFullscreen)
        {
            isOpen = true;
            return;
        }

        Vector2 target = isOpen ? openPosition : closedPosition;
        if(trans.anchoredPosition != target)
        {
            float flat = 300;
            float per = 3f;

            float dist = Vector3.Distance(target, trans.anchoredPosition);

            trans.anchoredPosition = Vector3.MoveTowards(trans.anchoredPosition, target, Time.deltaTime * (dist * per + flat));
        }
    }

    public void SwitchOpen()
    {
        SetOpen(!isOpen);
    }
    public void SetOpen(bool open)
    {
        isOpen = open;
        expandButton.transform.rotation = isOpen ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
    }

    public void SwitchFullscreen()
    {
        SetFullscreen(!isFullscreen);
    }
    public void SetFullscreen(bool fullscreen)
    {
        isFullscreen = fullscreen;

        fullscreenButton.runtimeAnimatorController = !isFullscreen ? fullscreenController : smallscreenController;
        if (isFullscreen)
        {
            isOpen = true;
            parent.anchoredPosition = new Vector3(-800, -125);
            parent.localScale = Vector3.one * 2.5f;
            //canvas.renderMode = RenderMode.WorldSpace;
        }
        else
        {
            parent.anchoredPosition = parentOriginalPos;
            parent.localScale = Vector3.one;
            //canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }
}
