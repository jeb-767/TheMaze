using UnityEngine;

namespace PaulosTools
{
    public class RotatorTool : MonoBehaviour
    {
        [SerializeField] private bool rotateOn = true;
        [SerializeField] private Transform rotatingTransform;
        [SerializeField] private Vector3 rotationAxis = new Vector3(0,1,0);
        [SerializeField] private Space rotationSpace = Space.World;
        [SerializeField] private float rotationSpeed = 1f;

        private void Start()
        {
            if (!rotatingTransform)
                rotatingTransform = transform;
        }

        private void Update()
        {
            if (rotateOn)
                rotatingTransform.Rotate(rotationAxis, Time.deltaTime * rotationSpeed, rotationSpace);
        }
    }
}
