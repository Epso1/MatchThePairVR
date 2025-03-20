using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private DataManager instance { get; set; }
    public PlayerData playerData = new PlayerData();

    void Awake()
    {
        if (instance == null) 
        { 
            instance = this;
        }
        else { Destroy (this); }        
    }

    void Start()
    {
        playerData.AddOrUpdateLevelTime(1, 120.5f); // Nivel 1 completado en 120.5 segundos
        playerData.AddOrUpdateLevelTime(2, 95.3f);  // Nivel 2 completado en 95.3 segundos
        playerData.AddOrUpdateLevelTime(3, 150.0f); // Nivel 3 completado en 150.0 segundos

        foreach (var item in playerData.GetAllLevelTimes())
        {
            Debug.Log($"Stage {item.Key} - Time: {item.Value}");
        }
    }
}

[Serializable]
public class PlayerData
{
    public string playerName { get; private set; }
    private Dictionary<int, float> levelCompletionTimes = new Dictionary<int, float>();

    public PlayerData(){}

    public PlayerData(string playerName, Dictionary<int, float> levelCompletionTimes) 
    {
        this.playerName = playerName;
        this.levelCompletionTimes = levelCompletionTimes;
    }

    // Método para agregar o actualizar el tiempo de un nivel
    public void AddOrUpdateLevelTime(int sceneIndex, float completionTime)
    {
        if (levelCompletionTimes.ContainsKey(sceneIndex))
        {
            // Si el nivel ya existe, comprueba si el nuevo tiempo es menor o igual al almacenado
            if (completionTime <= levelCompletionTimes[sceneIndex])
            {
                // Actualiza el tiempo si es mejor o igual
                levelCompletionTimes[sceneIndex] = completionTime;
                Debug.Log($"Tiempo actualizado para el nivel {sceneIndex}: {completionTime} segundos.");
            }
            else
            {
                // Si el tiempo no es mejor, muestra un mensaje informativo
                Debug.Log($"El tiempo proporcionado ({completionTime} segundos) no es mejor que el tiempo actual ({levelCompletionTimes[sceneIndex]} segundos) para el nivel {sceneIndex}. No se ha actualizado.");
            }
        }
        else
        {
            // Si el nivel no existe, lo añade al diccionario
            levelCompletionTimes.Add(sceneIndex, completionTime);
            Debug.Log($"Nuevo tiempo registrado para el nivel {sceneIndex}: {completionTime} segundos.");
        }
    }

    // Método para obtener el tiempo de un nivel específico
    public float GetLevelTime(int sceneIndex)
    {
        if (levelCompletionTimes.ContainsKey(sceneIndex))
        {
            return levelCompletionTimes[sceneIndex];
        }
        else
        {
            // Si el nivel no existe, devuelve -1 como valor predeterminado
            return -1f;
        }
    }

    // Método para obtener todos los niveles completados
    public Dictionary<int, float> GetAllLevelTimes()
    {
        return levelCompletionTimes;
    }

    // Método para borrar todos los datos
    public void ClearAllData()
    {
        levelCompletionTimes.Clear();
    }

    // Método para establecer el nombre del jugador
    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
    }
}

[Serializable]
public class PlayerDataList
{
    List<PlayerData> playerDataList = new List<PlayerData>();
}

