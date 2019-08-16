using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        Look();
    }

    private void Look()
    {
        transform.LookAt(Camera.main.transform);
    }

}
