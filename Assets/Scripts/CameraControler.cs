using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    private static float zoom = 1;
    private static Vector2 minMaxZoom = new Vector2(540f/3f, 540f);

    private static Vector2 position = new Vector2(0.5f, 0.5f);

    public float zoomSpeed = 2f;
    public float panSpeed = 1f;
    public float adjustmentSpeed = 1f;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {

            float scroll = Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed;
            zoom -= scroll;
            bool hasChanged = (zoom < 0.95f && zoom > 0.05f);
            zoom = Mathf.Clamp01(zoom);

            if(hasChanged)
            {
                Vector2 mouse = MouseBehaviour.Mouse01() - Vector2.one * 0.5f;
                position += mouse * scroll * adjustmentSpeed / GetZoomDifference();
                position = new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
            }

            cam.orthographicSize = Mathf.Lerp(minMaxZoom.x, minMaxZoom.y, zoom);
            position -= (Vector2)MouseBehaviour.NormalizeMousePosition(Input.mousePositionDelta*Time.deltaTime*panSpeed/Mathf.Pow(GetZoomDifference(),2));
            position = new Vector2(Mathf.Clamp01(position.x), Mathf.Clamp01(position.y));
        }

        if (zoom > 0.95f) position = Vector2.one * 0.5f;


        transform.position = new Vector3(
            Mathf.Lerp(cam.orthographicSize * cam.aspect, 1920f - cam.orthographicSize*cam.aspect, position.x),
            Mathf.Lerp(cam.orthographicSize, 1080f - cam.orthographicSize, position.y),
            -10);
    }

    public static float GetZoomDifference()
    {
        print(Mathf.Lerp(minMaxZoom.y / minMaxZoom.x, 1f, zoom));
        return Mathf.Lerp(minMaxZoom.y / minMaxZoom.x, 1f, zoom);
    }
}
