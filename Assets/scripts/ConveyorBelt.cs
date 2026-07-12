using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    public float beltSpeed = 1f;
    public float detectionHeight = 0.05f;
    public float moveDuration = 3f; // how long the belt moves after the bell is rung
    public Bell bell;
    private Vector3 beltDirection = Vector3.right;
    [SerializeField]
    private List<Rigidbody> objectsOnBelt = new List<Rigidbody>();
    private Animator animator;
    private float currentMoveTime = 0f;


    void Start()
    {
        animator = GetComponent<Animator>();
        currentMoveTime = moveDuration;
        bell.OnBellRung += StartMoving;
    }

    void FixedUpdate()
    {
        if (currentMoveTime < moveDuration)
        {
            currentMoveTime += Time.fixedDeltaTime;
            animator.SetBool("isMoving", true);
            DetectObjectsOnBelt();
            MoveObjectsOnBelt();
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void StartMoving()
    {
        currentMoveTime = 0f;
        print("Conveyor belt started moving!");
    }

    private void DetectObjectsOnBelt()
    {
        objectsOnBelt.Clear();

        // Create a detection box slightly above the belt surface
        Vector3 beltSize = GetComponent<Collider>().bounds.size;
        Vector3 detectionCenter = transform.position;
        Vector3 detectionSize = new Vector3(beltSize.x, detectionHeight, beltSize.z);

        // Find all colliders in that box
        Collider[] colliders = Physics.OverlapBox(
            detectionCenter,
            detectionSize / 2,
            transform.rotation
        );

        foreach (Collider col in colliders)
        {
            // Skip the belt and unmovables
            if (col.gameObject.CompareTag("Unmovable")) continue;

            Rigidbody rb = col.GetComponentInParent<Rigidbody>();
            if (rb != null && !objectsOnBelt.Contains(rb))
            {
                objectsOnBelt.Add(rb);
            }
        }
    }

    private void MoveObjectsOnBelt()
    {
        if (beltDirection == Vector3.zero) return;

        Vector3 worldDirection = transform.TransformDirection(beltDirection.normalized);

        foreach (Rigidbody rb in objectsOnBelt)
        {
            if (rb == null || rb.isKinematic) continue;

            Vector3 targetVelocity = worldDirection * beltSpeed;
            Vector3 velocityChange = new Vector3(
                targetVelocity.x - rb.linearVelocity.x,
                0f,
                targetVelocity.z - rb.linearVelocity.z
            );

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    // Visualize the detection box in scene view
    private void OnDrawGizmos()
    {
        if (GetComponent<Collider>() == null) return;

        Gizmos.color = Color.cyan;
        Vector3 beltSize = GetComponent<Collider>().bounds.size;
        Vector3 detectionCenter = transform.position;
        Vector3 detectionSize = new Vector3(beltSize.x, detectionHeight, beltSize.z);
        Gizmos.DrawWireCube(detectionCenter, detectionSize);
    }
}