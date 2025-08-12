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
        rotation.x = rotation.x + Input.GetAxis("Mouse X");
        rotation.z = rotation.z - Input.GetAxis("Mouse Y");
        transform.rotation = rotation;
        position = transform.position + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }
}
