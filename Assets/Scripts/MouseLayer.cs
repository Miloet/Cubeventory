using UnityEngine;

public class MouseLayer : MonoBehaviour
{
    void Start()
    {
        _ = CheckLayer();
    }

    public async Awaitable CheckLayer()
    {
        while(true)
        {
            transform.SetAsLastSibling();

            await Awaitable.WaitForSecondsAsync(5);
        }
    }
    
}
