using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs and transforms")]
    [SerializeField] Sprite[] cardSprites;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform cardParent;
    [SerializeField] GameObject explosionPrefab;

    [Header("Game settings")]
    [SerializeField] int initialPairs = 5;
    [SerializeField] int columns = 5; // N�mero de columnas en la cuadr�cula
    [SerializeField] float spacing = 0.5f; // Espaciado entre cartas
    [SerializeField] float initialPauseTime = 2f;
    [SerializeField] int initialShowTimeSeconds = 5;

    [Header("Audio")]
    [SerializeField] public AudioClip clickSound;
    [SerializeField] AudioClip matchSound;
    [SerializeField] AudioClip mismatchSound;
    [SerializeField] AudioClip secondSound;
    [SerializeField] AudioClip goVoice;

    [SerializeField] AudioClip introMusic;
    [SerializeField] AudioClip sceneMusic;
    [SerializeField] AudioClip victoryMusic;

    [SerializeField] AudioSource FXAudioSource;
    [SerializeField] AudioSource musicAudioSource;

    [Header("UI")]
    [SerializeField] GameObject UIStart;
    [SerializeField] GameObject UIVictory;
    [SerializeField] GameObject UIDefeated;
    [SerializeField] Text timeResultText;
    [SerializeField] GameObject UIGetReady;
    [SerializeField] GameObject UITimer;
    [SerializeField] Text timerText;
    [SerializeField] GameObject UICountdown;
    [SerializeField] Text countdownText;

    [Header("Private variables")]
    [HideInInspector] public bool playerCanClick = false;
    Sprite[] currentCards;
    List<Card> instantiatedCards = new List<Card>();
    Card firstFlippedCard = null;
    Card secondFlippedCard = null;
    int matchCount = 0;
    float elapsedTime = 0f;
    bool isTimeRunning = false;
    int gameLevel = 1;
    int maxLevel = 4;
    float levelMaxTime = 0f;

    void Start()
    {
        InitializeGameLevel();  // Inicializa el nivel
    }

    private void Update()
    {
        if (isTimeRunning) // Si el tiempo est� corriendo, actualiza el temporizador
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerText(elapsedTime);

            // Cuando el tiempo transcurrido sea mayor o igual a levelMaxTime, se ejecuta LevelFailed
            if (elapsedTime >= levelMaxTime)
            {
                StopTimer();
                LevelFailed();
            }
        }
    }

    private void InitializeGameLevel()
    {
        // Desactivar UI
        UIVictory.SetActive(false);
        UIGetReady.SetActive(false);
        UICountdown.SetActive(false);
        UITimer.SetActive(false);
        UIDefeated.SetActive(false);

        if (gameLevel == 1) // Reproducir la m�sica de introducci�n y activar UIStart si es el primer nivel de juego
        {
            UIStart.SetActive(true);
            PlayMusic(introMusic, 0.5f, true);
        }
        else // Incrementar la cantidad de parejas iniciales
        {
            initialPairs++;
        }

        // Establecer el n�mero de columnas
        columns = initialPairs;

        initialShowTimeSeconds = initialPairs; // Establecer el tiempo de previsualizaci�n de las cartas con el valor de la cantidad de parejas 
        levelMaxTime = initialPairs * 5; // Establecer el tiempo m�ximo para completar el nivel
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isTimeRunning = true;
    }

    public void StopTimer()
    {
        isTimeRunning = false;
    }

    // Se actualiza el timerText para mostrar el tiempo restante
    private void UpdateTimerText(float time)
    {
        float remainingTime = levelMaxTime - time;
        if (remainingTime < 0)
            remainingTime = 0;

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    private void UpdateTimeResultText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timeResultText.text = "YOUR REMAINING TIME: " + string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    void CreateDeck()
    {
        List<Sprite> selectedCards = new List<Sprite>();

        // Seleccionar initialCards �nicas
        for (int i = 0; i < initialPairs; i++)
        {
            selectedCards.Add(cardSprites[i]);
        }

        // Duplicar cada carta
        List<Sprite> deck = new List<Sprite>(selectedCards);
        deck.AddRange(selectedCards);

        // Mezclar el mazo
        System.Random rng = new System.Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]); // Cambiar orden (swap)
        }

        currentCards = deck.ToArray();  // Convertir a array
        InstantiateDeck(); // Instanciar el mazo
    }

    void InstantiateDeck()
    {
        int totalRows = Mathf.CeilToInt((float)currentCards.Length / columns);
        int totalCols = Mathf.Min(columns, currentCards.Length);

        float offsetX = (totalCols - 1) * spacing * 0.5f;
        float offsetY = (totalRows - 1) * spacing * 0.5f;

        for (int i = 0; i < currentCards.Length; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardParent);
            Card cardComponent = newCard.GetComponent<Card>();
            cardComponent.frontSpriteRenderer.sprite = currentCards[i];

            // Posicionamiento centrado
            int row = i / columns;
            int col = i % columns;
            newCard.transform.localPosition = new Vector3(col * spacing - offsetX, -row * spacing + offsetY, 0);

            instantiatedCards.Add(cardComponent);
        }
    }

    IEnumerator ShowCardsTemporarily()
    {
        StopMusic();
        UIGetReady.SetActive(true); // Activar la UIGetReady
        yield return new WaitForSeconds(initialPauseTime); // Peque�a pausa inicial

        // Mostrar todas las cartas (boca arriba)
        foreach (var card in instantiatedCards)
        {
            card.FlipCard(); // Girar para mostrar la parte frontal
        }

        yield return StartCoroutine(CountdownCoroutine()); // Esperar el tiempo de visualizaci�n

        // Volver a ponerlas boca abajo
        foreach (var card in instantiatedCards)
        {
            card.FlipCard(); // Girar nuevamente para esconder
        }
        UIGetReady.SetActive(false); // Desactivar la UIGetReady
        playerCanClick = true; // Activar playerCanClick
        UITimer.SetActive(true); // Mostrar el temporizador
        StartTimer(); // Iniciar el temporizador
        PlayMusic(sceneMusic, 0.5f, true);
    }

    public void StartGame()
    {
        instantiatedCards.Clear();
        currentCards = new Sprite[initialPairs * 2];
        CreateDeck();
        StartCoroutine(ShowCardsTemporarily());
        UIStart.SetActive(false);
        matchCount = 0;
    }

    public void CardFlipped(Card card)
    {
        if (firstFlippedCard == null)
        {
            firstFlippedCard = card;
        }
        else if (secondFlippedCard == null)
        {
            secondFlippedCard = card;
            playerCanClick = false;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstFlippedCard.frontSpriteRenderer.sprite.name == secondFlippedCard.frontSpriteRenderer.sprite.name && firstFlippedCard != secondFlippedCard)
        {
            PlaySoundFX(matchSound, 0.5f);
            Instantiate(explosionPrefab, firstFlippedCard.transform.position, Quaternion.identity);
            Destroy(firstFlippedCard.gameObject);
            Instantiate(explosionPrefab, secondFlippedCard.transform.position, Quaternion.identity);
            Destroy(secondFlippedCard.gameObject);
            matchCount++;

            if (matchCount == initialPairs)
            {
                StopMusic();
                PlayMusic(victoryMusic, 0.5f, false);
                UITimer.SetActive(false);
                StopTimer();
                UIVictory.SetActive(true);
                UpdateTimeResultText(levelMaxTime - elapsedTime);
            }
        }
        else
        {
            PlaySoundFX(mismatchSound, 0.8f);
            firstFlippedCard.ResetCard(); // Resetear el estado de la carta
            secondFlippedCard.ResetCard(); // Resetear el estado de la carta
        }

        firstFlippedCard = null;
        secondFlippedCard = null;
        playerCanClick = true;
    }

    public void StartNextGameLevel()
    {
        if (gameLevel < maxLevel)
        {
            gameLevel++; // Incrementar el �ndice del nivel de juego
            InitializeGameLevel();
            StartGame();
        }
        else { Debug.Log("There are no more game levels."); }
    }

    void PlayMusic(AudioClip musicClip, float volume, bool playLoop)
    {
        musicAudioSource.Stop();
        musicAudioSource.volume = volume;
        musicAudioSource.clip = musicClip;
        musicAudioSource.loop = playLoop;
        musicAudioSource.Play();
    }

    void StopMusic()
    {
        musicAudioSource.Stop();
    }

    public void PlaySoundFX(AudioClip soundFX, float volume)
    {
        FXAudioSource.volume = volume;
        FXAudioSource.PlayOneShot(soundFX);
    }

    public IEnumerator CountdownCoroutine()
    {
        UICountdown.SetActive(false);
        for (int count = (int)initialShowTimeSeconds; count > 0; count--)
        {
            countdownText.text = count.ToString();
            UICountdown.SetActive(true);
            PlaySoundFX(secondSound, 0.8f);
            yield return new WaitForSeconds(1);
            UICountdown.SetActive(false);
        }

        countdownText.text = "GO!";
        UICountdown.SetActive(true);
        PlaySoundFX(goVoice, 0.6f);
        yield return new WaitForSeconds(1);
        UICountdown.SetActive(false);
    }

    void LevelFailed()
    {
        UIDefeated.SetActive(true);
        UITimer.SetActive(false);
        StopMusic();
        instantiatedCards.Clear();
        foreach (var card in FindObjectsOfType<Card>())
        {
            Destroy(card.gameObject);
        }
    }

    public void RestartLevel()
    {
        InitializeGameLevel();
        StartGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
