using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlller : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var position = transform.position;
        var rotation = transform.rotation;
        rotation.y = rotation.y + Input.GetAxis("Mouse X");
        transform.rotation = rotation;
        position = transform.position + new Vector3(, , 0);
    }
}
