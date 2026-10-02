using UnityEngine;
using Unity.Netcode; // Necesario para IsOwner

public class Camara_Effects : NetworkBehaviour
{
    public Player controller; // Referencia al script del Player
    public Transform cameraTransform;

    [Header("Head Bob Settings")]
    public float walkBobSpeed = 3f;
    public float walkBobAmount = 0.02f;
    public float runBobSpeed = 8f;
    public float runBobAmount = 0.05f;
    public float sprintBobSpeed = 14f;
    public float sprintBobAmount = 0.1f;

    private float defaultYPos;
    private float timer;
    float bobSpeed;
    float bobAmount;

    public override void OnNetworkSpawn()
    {
        // Al aparecer en la red, si no somos el dueño, desactivamos este script 
        // para ahorrar cálculos innecesarios.
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
    }

    void Start()
    {
        // Guardamos la posición inicial local de la cámara
        if (cameraTransform != null)
        {
            defaultYPos = cameraTransform.localPosition.y;
        }
        else
        {
            // Fallback por si no se asignó en el inspector
            cameraTransform = transform;
            defaultYPos = transform.localPosition.y;
        }
    }

    void Update()
    {
        // DOBLE SEGURIDAD: Solo el dueño procesa efectos visuales de su cámara
        if (!IsOwner) return;

        // Verificamos que controller no sea nulo para evitar errores si el Player muere/desaparece
        if (controller == null) return;

        if (controller.move.magnitude > 0.1f && controller.isGrounded)
        {
            if (controller.currentSpeed == controller.runSpeed)
            {
                bobSpeed = runBobSpeed;
                bobAmount = runBobAmount;
            }
            else if (controller.currentSpeed == controller.sprintSpeed)
            {
                bobSpeed = sprintBobSpeed;
                bobAmount = sprintBobAmount;
            }
            else if (controller.currentSpeed == controller.walkSpeed)
            {
                bobSpeed = walkBobSpeed;
                bobAmount = walkBobAmount;
            }
            // Si currentSpeed es 0 o diferente, usamos valores por defecto para evitar errores
            else
            {
                bobSpeed = walkBobSpeed;
                bobAmount = walkBobAmount;
            }

            timer += Time.deltaTime * bobSpeed;
            float newY = defaultYPos + Mathf.Sin(timer) * bobAmount;

            Vector3 localPos = cameraTransform.localPosition;
            cameraTransform.localPosition = new Vector3(localPos.x, newY, localPos.z);
        }
        else
        {
            // Vuelve suave a la posición base
            Vector3 localPos = cameraTransform.localPosition;
            if (Mathf.Abs(localPos.y - defaultYPos) > 0.001f)
            {
                localPos.y = Mathf.Lerp(localPos.y, defaultYPos, Time.deltaTime * 5f);
                cameraTransform.localPosition = localPos;
            }
            else
            {
                // Forzar posición exacta para evitar micro-movimientos
                localPos.y = defaultYPos;
                cameraTransform.localPosition = localPos;
                timer = 0; // Resetear timer para que el ciclo empiece limpio la próxima vez (opcional)
            }
        }
    }

    public void OnJump()
    {
        if (!IsOwner) return; // Solo efecto local
        cameraTransform.localPosition += new Vector3(0, -0.1f, 0);
        // Debug.Log("Jump effect triggered");
    }

    public void OnLand()
    {
        if (!IsOwner) return; // Solo efecto local
        cameraTransform.localPosition += new Vector3(0, 0.15f, 0);
        // Debug.Log("Land effect triggered");
    }
}