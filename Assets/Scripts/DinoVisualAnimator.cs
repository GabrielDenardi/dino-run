using System.Linq;
using UnityEngine;

public class DinoVisualAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private DinoController controller;
    private Rigidbody body;

    private Sprite[] idleFrames;
    private Sprite[] runFrames;
    private Sprite[] jumpFrames;
    private Sprite[] deadFrames;
    private Sprite fallbackSprite;

    private Sprite currentSprite;
    private Sprite[] activeFrames;
    private float frameTimer;
    private int frameIndex;
    private string activeStateName;
    private bool wasGrounded;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        controller = GetComponent<DinoController>();
        body = GetComponent<Rigidbody>();

        var frames = DinoSpriteLibrary.GetSequence("Art/External/FreeDinoSprite/png");
        idleFrames = frames.WhereFrames("Idle");
        runFrames = frames.WhereFrames("Run");
        jumpFrames = frames.WhereFrames("Jump");
        deadFrames = frames.WhereFrames("Dead");
        fallbackSprite = DinoSpriteLibrary.GetAnySprite("Art/External/FreeDinoSprite/png");

        if (spriteRenderer != null)
        {
            var startingFrames = GetPreferredFrames(true);
            currentSprite = startingFrames != null && startingFrames.Length > 0
                ? startingFrames[Mathf.Clamp(startingFrames.Length / 2, 0, startingFrames.Length - 1)]
                : fallbackSprite;
            spriteRenderer.sprite = currentSprite;
            spriteRenderer.enabled = true;
        }

        wasGrounded = controller == null || controller.IsGrounded;
    }

    private void LateUpdate()
    {
        var manager = DinoGameManager.Instance;
        if (spriteRenderer == null || manager == null)
        {
            return;
        }

        var frames = GetPreferredFrames(true);
        var stateName = GetCurrentStateName();
        var speed = GetPlaybackSpeed(manager);
        var grounded = controller == null || controller.IsGrounded;
        var justLanded = grounded && !wasGrounded;
        var justTookOff = !grounded && wasGrounded;

        if (justLanded || justTookOff)
        {
            activeFrames = null;
        }

        if (frames == null || frames.Length == 0)
        {
            if (fallbackSprite != null && currentSprite != fallbackSprite)
            {
                currentSprite = fallbackSprite;
                spriteRenderer.sprite = currentSprite;
            }

            return;
        }

        if (frames != activeFrames || stateName != activeStateName)
        {
            activeFrames = frames;
            activeStateName = stateName;
            frameIndex = 0;
            frameTimer = 0f;
            currentSprite = null;
        }

        var frameDuration = 1f / Mathf.Max(1f, speed);
        frameTimer += Time.deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            if (stateName == "Jump" || stateName == "Dead")
            {
                frameIndex = Mathf.Min(frameIndex + 1, frames.Length - 1);
            }
            else
            {
                frameIndex = (frameIndex + 1) % frames.Length;
            }
        }

        var frame = frames[frameIndex];

        if (frame != currentSprite)
        {
            currentSprite = frame;
            spriteRenderer.sprite = frame;
        }

        wasGrounded = grounded;
    }

    private string GetCurrentStateName()
    {
        if (controller != null && controller.IsDead)
        {
            return "Dead";
        }

        if (controller != null && !controller.IsGrounded)
        {
            return "Jump";
        }

        return "Run";
    }

    private Sprite[] GetPreferredFrames(bool allowIdleFallback)
    {
        if (controller != null && controller.IsDead)
        {
            return deadFrames != null && deadFrames.Length > 0
                ? deadFrames
                : (runFrames != null && runFrames.Length > 0 ? runFrames : fallbackSprite == null ? null : new[] { fallbackSprite });
        }

        if (controller != null && controller.IsGrounded)
        {
            return runFrames != null && runFrames.Length > 0
                ? runFrames
                : (allowIdleFallback ? idleFrames : null);
        }

        return jumpFrames != null && jumpFrames.Length > 0
            ? jumpFrames
            : (runFrames != null && runFrames.Length > 0
                ? runFrames
                : (allowIdleFallback ? idleFrames : fallbackSprite == null ? null : new[] { fallbackSprite }));
    }

    private float GetPlaybackSpeed(DinoGameManager manager)
    {
        if (manager.IsGameOver)
        {
            return 10f;
        }

        if (controller != null && controller.IsGrounded)
        {
            return Mathf.Lerp(12f, 18f, Mathf.InverseLerp(6f, 14f, manager.CurrentSpeed));
        }

        return 13f;
    }
}

internal static class DinoSpriteExtensions
{
    public static Sprite[] WhereFrames(this Sprite[] sprites, string prefix)
    {
        if (sprites == null)
        {
            return null;
        }

        return sprites.Where(sprite => sprite != null && sprite.name.StartsWith(prefix)).ToArray();
    }
}
