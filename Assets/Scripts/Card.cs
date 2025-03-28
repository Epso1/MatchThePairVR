using System.Collections;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] public SpriteRenderer frontSpriteRenderer;
    [SerializeField] public SpriteRenderer backSpriteRenderer;
    public float rotationSpeed = 2f;
    private bool isRotating = false;
    private GameManager gameManager;
    public bool isFlipped = false; //Variable para controlar si la carta está volteada

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void PlayerFlipsCard()
    {
        if (gameManager.playerCanClick && !isFlipped) // Evita voltear cartas ya descubiertas
        {
            SetFlippedCard();
            FlipCard();
            gameManager.PlaySoundFX(gameManager.clickSound, 0.5f);
        }
    }

    public void SetFlippedCard()
    {
        gameManager.CardFlipped(this);
    }

    public void FlipCard()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateSmoothly());
        }
    }

    private IEnumerator RotateSmoothly()
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);
        float elapsedTime = 0;

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
        Debug.Log("RotateSmoothly() marca la carta como volteada o no");
        isFlipped = !isFlipped; // Marca la carta como volteada o no
    }

    public void ResetCard() 
    {
        if (isFlipped)
        {
            Debug.Log("ResetCard() ejecutando FlipCard()");
            FlipCard();
        }
    }
}
