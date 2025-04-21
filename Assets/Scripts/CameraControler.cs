using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    private float zoom = 1;
    private Vector2 minMaxZoom = new Vector2(540f/3f, 540f);
    private Vector4 minMaxPosition = new Vector4(0, 1920f, -1080, 1080);
    private Vector2 mousePosition;
    private Vector3 originalPosition = Vector3.zero;

    public float zoomSpeed = 2f;
    //public float panSpeed = 1f;
    //public float panDistSpeed = 0.2f;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            originalPosition = transform.position;
        }


        if (Input.GetMouseButton(1))
        {
            float scroll = Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed;
            zoom -= scroll;
            bool hasChanged = (zoom < 0.95f && zoom > 0.05f);
            zoom = Mathf.Clamp01(zoom);
            cam.orthographicSize = Mathf.Lerp(minMaxZoom.x, minMaxZoom.y, zoom);



            if (hasChanged)
            {

            }


            Vector2 currentMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 diff = currentMousePosition - mousePosition;

            Vector3 target = transform.position - (Vector3)diff;
            float dist = Vector3.Distance(transform.position, target);
            target.z = -10;

            target = ClampPan(target);

            transform.position = target;
        }

    }

    Vector3 ClampPan(Vector3 input)
    {
         
        Vector2 size = new Vector2(cam.aspect*cam.orthographicSize, cam.orthographicSize);
        float x = Mathf.Clamp(input.x, minMaxPosition.x+ size.x, minMaxPosition.y- size.x);
        float y = Mathf.Clamp(input.y, minMaxPosition.z + size.y, minMaxPosition.w - size.y);

        return new Vector3(x, y, -10);
    }

}
