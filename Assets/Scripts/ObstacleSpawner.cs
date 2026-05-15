using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private float spawnX = DinoBootstrapper.SpawnX;
    [SerializeField] private float minDelay = 0.7f;
    [SerializeField] private float maxDelay = 1.55f;

    private float spawnTimer = 0.9f;

    private void Update()
    {
        var manager = DinoGameManager.Instance;
        if (manager == null || !manager.IsPlaying)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
        {
            return;
        }

        SpawnObstacle();
        spawnTimer = GetSpawnDelay(manager.CurrentSpeed);
    }

    private float GetSpawnDelay(float speed)
    {
        var normalizedSpeed = Mathf.InverseLerp(6f, 14f, speed);
        var delay = Mathf.Lerp(maxDelay, minDelay, normalizedSpeed);
        return delay + Random.Range(0f, 0.35f);
    }

    private void SpawnObstacle()
    {
        var obstacle = new GameObject("Obstacle");
        obstacle.tag = "Obstacle";

        var body = obstacle.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        var box = obstacle.AddComponent<BoxCollider>();

        var variant = Random.Range(0, 3);
        var cactusSprite = DinoSpriteLibrary.GetSingle("Art/External/pdphotodotorg_barell_cactus-varalpha");
        if (!DinoSpriteLibrary.TryGetOpaqueBounds(cactusSprite, out var opaqueBounds))
        {
            opaqueBounds = cactusSprite != null ? cactusSprite.bounds : new Bounds(Vector3.zero, Vector3.one);
        }

        var targetHeight = variant == 0 ? 0.8f : variant == 1 ? 1.0f : 1.22f;
        var visualScale = targetHeight / Mathf.Max(0.01f, opaqueBounds.size.y);
        var colliderWidth = Mathf.Max(0.24f, opaqueBounds.size.x * visualScale * (variant == 0 ? 0.72f : variant == 1 ? 0.68f : 0.64f));
        var colliderHeight = Mathf.Max(0.55f, opaqueBounds.size.y * visualScale * (variant == 0 ? 0.76f : variant == 1 ? 0.72f : 0.68f));

        obstacle.transform.position = new Vector3(spawnX, DinoBootstrapper.GroundTopY, 0f);
        box.size = new Vector3(colliderWidth, colliderHeight, 0.8f);
        box.center = new Vector3(0f, colliderHeight * 0.5f, 0f);

        BuildCactusVisuals(obstacle.transform, cactusSprite, opaqueBounds, visualScale);
        obstacle.AddComponent<ObstacleMover>();
    }

    private static void BuildCactusVisuals(Transform parent, Sprite cactusSprite, Bounds opaqueBounds, float visualScale)
    {
        var spriteRoot = new GameObject("Visual");
        spriteRoot.transform.SetParent(parent, false);
        spriteRoot.transform.localPosition = new Vector3(
            -opaqueBounds.center.x * visualScale,
            -opaqueBounds.min.y * visualScale,
            0f);
        spriteRoot.transform.localScale = new Vector3(visualScale, visualScale, 1f);

        var spriteRenderer = spriteRoot.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 4;
        spriteRenderer.sprite = cactusSprite;
    }
}
