using UnityEngine;

public class Qubit : MonoBehaviour
{
    public Sprite activeSprite;    // Sprite for activated state
    public Sprite inactiveSprite;  // Sprite for deactivated state

    private SpriteRenderer spriteRenderer;
    public int state;

    private void Awake()
    {
        // This will search this GameObject and its children for a SpriteRenderer
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
        else
        {
            Debug.LogError("SpriteRenderer not found!");
        }
    }

    public void Activate()
    {
        state = Random.Range(0, 2);
        if (spriteRenderer != null)
        {
            if (activeSprite != null)
            {
                spriteRenderer.sprite = activeSprite;
                Debug.Log($"Sprite changed to activeSprite for qubit at {transform.position}");
            }
            else
            {
                Debug.LogError("activeSprite is not assigned in the inspector!");
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer not found!");
        }
        Debug.Log($"Qubit at {transform.position} activated with state {state}");
    }

    public void Deactivate()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
    }
}
