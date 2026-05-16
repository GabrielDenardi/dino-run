using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DinoController : MonoBehaviour
{
    [SerializeField] private float jumpForce = 9.2f;
    [SerializeField] private float moveSpeed = 5.5f;

    private Rigidbody body;
    private bool grounded;
    private bool dead;
    private float moveAxis;

    public bool IsGrounded => grounded;
    public bool IsDead => dead;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        grounded = ComputeGrounded();

        var manager = DinoGameManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (manager.IsGameOver)
        {
            moveAxis = 0f;
            if (!dead)
            {
                dead = true;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.constraints = RigidbodyConstraints.FreezeAll;
            }

            return;
        }

        if (dead)
        {
            dead = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (manager.IsPlaying && grounded && IsJumpPressed())
        {
            grounded = false;
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        moveAxis = GetHorizontalInput();
    }

    private void FixedUpdate()
    {
        var manager = DinoGameManager.Instance;
        if (manager == null || !manager.IsPlaying || dead || body == null)
        {
            return;
        }

        var position = body.position;
        position.x += moveAxis * moveSpeed * Time.fixedDeltaTime;
        position.x = Mathf.Clamp(position.x, DinoBootstrapper.PlayerMinX, DinoBootstrapper.PlayerMaxX);
        body.MovePosition(position);
    }

    private static bool IsJumpPressed()
    {
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetKeyDown(KeyCode.UpArrow);
    }

    private static float GetHorizontalInput()
    {
        var move = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            move -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            move += 1f;
        }

        return Mathf.Clamp(move, -1f, 1f);
    }

    private bool ComputeGrounded()
    {
        var collider = GetComponent<Collider>();
        var bounds = collider.bounds;
        var origin = bounds.center;
        var distance = bounds.extents.y + 0.12f;
        return Physics.Raycast(origin, Vector3.down, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    private void HandleCollision(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.45f)
                {
                    grounded = true;
                    break;
                }
            }

            return;
        }

        if (collision.collider.CompareTag("Obstacle"))
        {
            var manager = DinoGameManager.Instance;
            if (manager != null)
            {
                manager.GameOver($"Obstacle collision with {collision.collider.name}");
            }
        }
    }
}
