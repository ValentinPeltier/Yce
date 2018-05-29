using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveController : MonoBehaviour {
    
    [SerializeField] private string saveFolderName;
    [SerializeField] private string saveFileName;
    [Space(10)]
    [SerializeField] private DataStruct defaultData;

    [System.Serializable] private struct DataStruct {
        public Vector2Int map, cell;
        public Vector3 orientation;
        public bool lineRenderState;
    }

    private string saveFolderPath, saveFilePath;
    private PlayerMovement playerMovement;

    // -------------

    /// <summary>
    /// Save all informations about player and world
    /// </summary>
	public void Save() {
        // If directory doesn't exist
        if(!Directory.Exists(saveFolderPath)) {
            // Create it
            Directory.CreateDirectory(saveFolderPath);
        }

        // Get data
        DataStruct data = new DataStruct {
            map = playerMovement.currentMap,
            cell = playerMovement.currentCell,
            orientation = playerMovement.transform.rotation.eulerAngles,
            lineRenderState = MapManager.GetMapManager(playerMovement.currentMap).GetLineRenderState()
        };

        // Parse to JSON
        string dataString = JsonUtility.ToJson(data);

        // Save data into save file
        File.WriteAllText(saveFilePath, dataString);
	}

    /// <summary>
    /// Load data game from save file
    /// </summary>
    public void Load() {
        // If data file exists
        if (File.Exists(saveFilePath)) {
            // Get data from save file
            string dataString = File.ReadAllText(saveFilePath);

            // Parse into DataStruct
            DataStruct data = JsonUtility.FromJson<DataStruct>(dataString);

            // Set data
            SetData(data);
        }
        else {
            // Set default data
            SetData(defaultData);
        }
    }

    // ----------------

    private void SetData(DataStruct data) {
        // Set player position and orientation and set camera position
        playerMovement.SetPosition(data.map, data.cell, data.orientation, false);

        // Set current map
        MapManager mapManager = MapManager.GetMapManager(playerMovement.currentMap);
        mapManager.SetMapState(true);
        mapManager.SetLineRenderState(data.lineRenderState);
    }

    /// <summary>
    /// On application quit, save data
    /// </summary>
    private void OnApplicationQuit() {
        Save();
    }

    // ----------------

    private void Awake() {
        if(saveFileName == "") {
            Debug.LogError("Error : No save file name");
        }

        saveFolderPath = Path.Combine(Application.dataPath, saveFolderName);
        saveFilePath = Path.Combine(saveFolderPath, saveFileName);
        
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }

    private void Start() {
        Load();
    }
}
