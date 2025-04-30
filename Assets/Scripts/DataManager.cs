using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DataManager : MonoBehaviour
{
    // Número total de slots de guardado
    private int totalSlots = 6;

    // Arreglo de botones asignados desde la interfaz
    [Header("UI")]
    [SerializeField] Button[] slotButtons;
    [SerializeField] TextMeshProUGUI[] slotButtonsTexts;
    [SerializeField] Text infoText; // Texto de mensaje general (por ejemplo, "Select a game or create a new one")
    [SerializeField] AudioClip successSound; // Sonido de éxito al crear una partida
    [SerializeField] GameObject explosionPrefab; // Prefab de explosión para efectos visuales

    [Header("GameManager")]
    [SerializeField] GameManager gameManager; // Referencia al GameManager para acceder a sus métodos

    // Variable para mantener la partida cargada
    public PlayerData currentPlayerData;

    // Key base para acceder a los datos en PlayerPrefs
    private string slotKeyBase = "Slot";

    private void Start()
    {
        // Actualizar el texto de la interfaz
        if (infoText != null)
            infoText.text = $"Please, select an EMPTY slot to create a game \n or select a slot with data to load.";

        // Configurar cada botón de slot
        for (int i = 0; i < totalSlots; i++)
        {
            int index = i;
            // Añadir listener al botón
            slotButtons[i].onClick.AddListener(() => OnSlotButtonClicked(index));
            // Actualizar el texto del botón según si hay datos o no
            UpdateSlotButtonText(index);
        }
    }

    // Actualiza el texto del botón según si hay partida guardada o está vacío
    private void UpdateSlotButtonText(int slotIndex)
    {
        string key = slotKeyBase + slotIndex;
        TextMeshProUGUI buttonText = slotButtonsTexts[slotIndex];
        if (PlayerPrefs.HasKey(key))
        {
            // Si hay datos guardados, se recupera el JSON y se muestra la fecha y hora
            string json = PlayerPrefs.GetString(key);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            buttonText.text = $"{data.creationDate} {data.creationTime}";
        }
        else
        {
            // Si no hay datos, se muestra "EMPTY"
            buttonText.text = "EMPTY";
        }
    }

    // Método llamado cuando se hace clic en un botón de slot
    public void OnSlotButtonClicked(int slotIndex)
    {
        string key = slotKeyBase + slotIndex;

        if (!PlayerPrefs.HasKey(key))
        {
            // No hay partida guardada en este slot, se crea una nueva
            currentPlayerData = new PlayerData();
            string json = JsonUtility.ToJson(currentPlayerData);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();

            infoText.text = "Game created successfully";  // Actualizar el texto de información            
            UpdateSlotButtonText(slotIndex); // Actualizar el botón para mostrar la fecha y hora de creación          
           
        }
        else
        {
            // Existe una partida, se carga la información en currentPlayerData
            string json = PlayerPrefs.GetString(key);
            currentPlayerData = JsonUtility.FromJson<PlayerData>(json); 
            
            infoText.text = $"Game loaded: {currentPlayerData.creationDate} {currentPlayerData.creationTime}"; // Actualizar el texto de información                                                                  
        }

        gameManager.PlaySoundFX(successSound, 1f); ; // Reproducir sonido de éxito
        GameObject explosion = Instantiate(explosionPrefab, infoText.transform.position, Quaternion.identity);  // Instanciar el prefab de explosión en la posición del texto       
        StartCoroutine(StartGameCoroutine()); // Iniciar el juego después de un breve retraso
    }

    // Método para actualizar y guardar la partida actual (por ejemplo, tras jugar un nivel)
    public void UpdateCurrentGameData(int currentLevel, float newTime)
    {
        if (currentPlayerData != null)
        {
            // Buscar si ya existe LevelData para el nivel actual
            LevelData existingLevelData = currentPlayerData.levelData.Find(ld => ld.level == currentLevel);

            if (existingLevelData == null)
            {
                // Si no existe, se agrega un nuevo registro para el nivel actual
                LevelData newLevelData = new LevelData();
                newLevelData.level = currentLevel;
                newLevelData.time = newTime;
                currentPlayerData.levelData.Add(newLevelData);
                Debug.Log($"Nuevo registro creado para el nivel {currentLevel} con tiempo: {newTime}");
            }
            else
            {
                // Si existe, se compara el tiempo conseguido nuevo con el registrado previamente
                float savedTime = existingLevelData.time;               
                if (newTime <= savedTime)
                {
                    existingLevelData.time = newTime;
                    Debug.Log($"Tiempo actualizado para el nivel {currentLevel}: {newTime} (anteriormente {savedTime})");
                }
                else
                {
                    Debug.Log($"No se actualiza el tiempo para el nivel {currentLevel} porque el tiempo conseguido ({newTime}) supera el registrado ({savedTime}).");
                }     
            }
            // Encontrar la key del slot en el que se cargó la partida actual para guardar la actualización
            string key = FindSlotKeyForCurrentGame();
            if (!string.IsNullOrEmpty(key))
            {
                string json = JsonUtility.ToJson(currentPlayerData);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                Debug.Log("Game data updated and saved.");
            }
        }
    }

    private IEnumerator StartGameCoroutine()
    {
        yield return new WaitForSeconds(3f);
        gameManager.StartGame();
    }

    // Encuentra la key del slot en el que se cargó la partida actual (según la fecha y hora de creación)
    // Nota: Este método es un ejemplo si quieres implementar la actualización de datos, pudiendo guardarse el índice de slot actual.
    private string FindSlotKeyForCurrentGame()
    {
        for (int i = 0; i < totalSlots; i++)
        {
            string key = slotKeyBase + i;
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                if (data.creationDate == currentPlayerData.creationDate &&
                    data.creationTime == currentPlayerData.creationTime)
                {
                    return key;
                }
            }
        }
        return "";
    }
}

[Serializable]
public class LevelData
{
    public int level;    // Nivel de juego
    public float time;  // Tiempo empleado para completar el nivel
}

[Serializable]
public class PlayerData
{
    public string creationDate; // Fecha de creación de la partida
    public string creationTime; // Hora de creación de la partida
    public List<LevelData> levelData; // Lista de datos de nivel

    // Constructor para inicializar una nueva partida
    public PlayerData()
    {
        creationDate = DateTime.Now.ToString("dd/MM/yyyy");
        creationTime = DateTime.Now.ToString("HH:mm:ss");
        levelData = new List<LevelData>(); // Puedes agregar elementos iniciales si es necesario
    }
}
