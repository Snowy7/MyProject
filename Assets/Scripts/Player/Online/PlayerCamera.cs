using System.Linq;
using Cinemachine;
using Fusion;
using SnInput;
using UnityEngine;

namespace Player.Online
{
    public class PlayerCamera : NetworkBehaviour, IAfterSpawned
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
        [SerializeField, Tooltip("Use the pivot original position as the offset")]
        private bool usePivotOgPosAsOffset;
        
        private Vector2 lookInput;
        
        private static readonly int MAX_ROT_CACHE = 3;
        private float[] rotArrayHor = new float[MAX_ROT_CACHE];
        private float[] rotArrayVert = new float[MAX_ROT_CACHE];
        private int rotCacheIndex;
        
        private float xRot;
        private float yRot;
        
        private Vector3 pivotOgPos;

        private void Awake()
        {
            // Set the pivot original position as the offset
            if (usePivotOgPosAsOffset && cameraPivot)
            {
                pivotOgPos = cameraPivot.localPosition;
            }
        }

        public void AfterSpawned()
        {
            if (!HasStateAuthority) return;
            
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
            
            // Get the cinemachine cam
            if (!cinemachine)
            {
                cinemachine = FindAnyObjectByType<CinemachineVirtualCamera>();
            }
            
            
            if (!cinemachine)
            {
                Debug.LogError("No cinemachine reference found!");
                enabled = false;
            }
            
            cinemachine.Follow = cameraPivot;
            cinemachine.LookAt = cameraPivot;
            
            // set distance and vertical arm length
            var body = cinemachine.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            body.CameraDistance = distance;
            body.VerticalArmLength = height;
            body.ShoulderOffset = shoulderOffset;
        }

        private void LateUpdate()
        {
            if (!HasStateAuthority) return;
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
            cameraPivot.position = player.position + pivotOgPos;
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