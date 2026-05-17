using UnityEngine;

public class Dog : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public DogData data;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetData(DogData dogData)
    {
        data = dogData;
        spriteRenderer.sprite = dogData.sprite;
    }
}