using UnityEngine;

public class MeasuringGate : MonoBehaviour
{
    public Sprite inactiveSprite;  // Default state when placed
    public Sprite state0Sprite;    // Measurement result 0
    public Sprite state1Sprite;    // Measurement result 1
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResetState();
    }

    public void ReceiveMeasurement(int state)
    {
        if (spriteRenderer == null) return;

        if (state == 0 && state0Sprite != null)
        {
            spriteRenderer.sprite = state0Sprite;
        }
        else if (state == 1 && state1Sprite != null)
        {
            spriteRenderer.sprite = state1Sprite;
        }
    }

    public void ResetState()
    {
        // Reset to inactive state
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }
}