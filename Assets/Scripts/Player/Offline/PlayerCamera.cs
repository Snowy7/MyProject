using System.Linq;
using Cinemachine;
using SnInput;
using UnityEngine;

namespace Player.Offline
{
    public class PlayerCamera : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private CinemachineVirtualCamera cinemachine;
        
        [Title("Settings")]
        [SerializeField] private bool invertY;
        [SerializeField] private float height = 1.5f;
        [SerializeField] private float distance = 5f;
        [SerializeField] private float sensitivity = 2f;
        [SerializeField, Range(0, 90f)] private float clampAngle;
        //[SerializeField] private float runFovMultiplier = 1;
        [SerializeField] private Vector3 shoulderOffset;
        
        private Vector2 lookInput;
        
        private static readonly int MAX_ROT_CACHE = 3;
        private float[] rotArrayHor = new float[MAX_ROT_CACHE];
        private float[] rotArrayVert = new float[MAX_ROT_CACHE];
        private int rotCacheIndex;
        
        float xRot;
        float yRot;
        
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            if (!player)
            {
                Debug.LogError("No player reference found!");
                enabled = false;
            }
            
            if (!cameraPivot)
            {
                Debug.LogError("No camera pivot reference found!");
                enabled = false;
            }
            
            if (!cinemachine)
            {
                Debug.LogError("No cinemachine reference found!");
                enabled = false;
            }
            
            cinemachine.Follow = cameraPivot;
            cinemachine.LookAt = player;
            
            // set distance and vertical arm length
            var body = cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            body.CameraDistance = distance;
            body.VerticalArmLength = height;
            body.ShoulderOffset = shoulderOffset;
        }

        private void LateUpdate()
        {
            HandleInput();
            CameraRotation();
        }
        
        private void CameraRotation()
        {
            // rotate the camera pivot
            xRot += (lookInput.y * sensitivity) * Time.deltaTime * (invertY ? -1 : 1);
            xRot = Mathf.Clamp(xRot, -clampAngle, clampAngle);
            yRot += (lookInput.x * sensitivity) * Time.deltaTime;
            
            cameraPivot.localRotation = Quaternion.Euler(xRot, yRot, 0);
        }
        
        private void HandleInput()
        {
            // Smoothing the input using the average frame solution.
            float x = GetAverageHorizontal(InputManager.Instance.LookInput.x);
            float y = GetAverageVertical(InputManager.Instance.LookInput.y);
            IncreaseRotCacheIndex();

            lookInput = new Vector2(x, y);
        }
        
        private float GetAverageHorizontal(float h)
        {
            rotArrayHor[rotCacheIndex] = h;
            return rotArrayHor.Average();
        }
        
        private float GetAverageVertical(float v)
        {
            rotArrayVert[rotCacheIndex] = v;
            return rotArrayVert.Average();
        }

        private void IncreaseRotCacheIndex()
        {
            rotCacheIndex++;
            rotCacheIndex %= MAX_ROT_CACHE;
        }
    }
}