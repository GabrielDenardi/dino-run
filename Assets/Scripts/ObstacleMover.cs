using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleMover : MonoBehaviour
{
    [SerializeField] private float destroyX = -14f;

    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void FixedUpdate()
    {
        var manager = DinoGameManager.Instance;
        if (manager != null && manager.IsPlaying)
        {
            var nextPosition = body.position + Vector3.left * manager.CurrentSpeed * Time.fixedDeltaTime;
            body.MovePosition(nextPosition);
        }

        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}
