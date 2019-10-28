using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public bool forceUp = false;
    public bool invertFoward = false;
    void Update()
    {
        Look();
    }

    private void Look()
    {
        if (!forceUp)
            transform.LookAt(Camera.main.transform);
        else
            transform.LookAt(Camera.main.transform, Vector3.up);

        if (invertFoward)
            transform.forward = -transform.forward;
    }

}
