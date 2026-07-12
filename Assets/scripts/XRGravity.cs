using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(CharacterController))]
public class XRGravity : MonoBehaviour
{
    public float gravity = -9.81f;
    private CharacterController cc;
    private float verticalVelocity;
    private TeleportationProvider teleportProvider;
    private bool isTeleporting;
    private float teleportCooldown;
    private const float TELEPORT_COOLDOWN = 0.2f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        teleportProvider = GetComponentInChildren<TeleportationProvider>();
        if (teleportProvider != null)
        {
            teleportProvider.locomotionStarted += OnTeleportBegin;
            teleportProvider.locomotionEnded += OnTeleportEnd;
        }
        else
        {
            Debug.LogWarning("XRGravity: no TeleportationProvider found");
        }
    }

    void OnTeleportBegin(LocomotionProvider provider)
    {
        isTeleporting = true;
        verticalVelocity = 0f;
    }

    void OnTeleportEnd(LocomotionProvider provider)
    {
        teleportCooldown = TELEPORT_COOLDOWN;
    }

    void Update()
    {
        if (isTeleporting) return;

        if (teleportCooldown > 0f)
        {
            teleportCooldown -= Time.deltaTime;
            if (teleportCooldown <= 0f)
                isTeleporting = false;
            return;
        }

        if (cc.isGrounded)
            verticalVelocity = -0.5f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        cc.Move(new Vector3(0, verticalVelocity * Time.deltaTime, 0));
    }

    void OnDestroy()
    {
        if (teleportProvider != null)
        {
            teleportProvider.locomotionStarted -= OnTeleportBegin;
            teleportProvider.locomotionEnded -= OnTeleportEnd;
        }
    }
}