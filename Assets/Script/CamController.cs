using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class CamController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("Camera Settings")] 
        [SerializeField] private InputActionAsset input;
        [SerializeField] private float lookSensitivity = 2f;

        [Header("Actions Maps")] [SerializeField]
        private InputAction look;
    
        void Start()
        {
            look = input.FindActionMap("Player").FindAction("Look");
        }
    
        private void OnEnable()
        {
            look.Enable();
        }
    
        private void OnDisable()
        {
            look.Disable();
        }

        // Update is called once per frame
        void Update()
        {
            float upDown = look.ReadValue<Vector2>().x * lookSensitivity * Time.deltaTime;
            transform.Rotate(upDown,0f, 0f);
        }
    }
}
