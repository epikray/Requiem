using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public static class SaveSystem
{
    private static string saveFilePath = Application.persistentDataPath + "\\SaveFiles";
    private static SaveData[] saveFiles;

    public static void CacheFiles() {
        saveFiles = new SaveData[16];
    }

    // TODO: SaveDataDisplayInfoHandle DisplaySaveFile(int saveNum) and int FindSaveFileByName(string saveName);
    public static void LoadSaveFile(int saveNum) {
        
        var saveDataEntries = Directory.EnumerateFiles(saveFilePath, "SaveData_?.json").ToArray<string>();
        //Debug.Log(Directory.EnumerateFiles(saveFilePath).Count());
        SaveData saveData = null;

        Debug.Log("Loading Directory, " + saveDataEntries.Count());
        if(saveDataEntries[saveNum] != null && saveNum < saveDataEntries.Count()) {
            StreamReader sr = File.OpenText(saveDataEntries[saveNum]);
            string s = sr.ReadToEnd();

            //Debug.Log("File read test\n" + s);

            saveData = SaveData.FromJson(s); 

            //Debug.Log("Json into SaveData\n " + saveData.name + ", player spawn at " + saveData.locationSpot + 
            //"\n" + saveData.sceneData.ToJson());

            sr.Close();
        }

        Debug.Log("SceneSaveData test " + GameManager.GetDefaultSceneSaveData(1).ToJson());

        GameManager.LoadGameFromSaveData(saveData);
    }

    public static void CreateNewSaveFile(string _name) {
        SaveData saveData = CreateDefaultSave(_name);
        
        
        if(!Directory.Exists(saveFilePath)) {
            Directory.CreateDirectory(saveFilePath);
        }

        int saveDataEntryCount = Directory.EnumerateFiles(saveFilePath, "SaveData_").Count();

        string filePath = saveFilePath + "\\SaveData_" + saveDataEntryCount + ".json";

        File.WriteAllText(filePath, saveData.ToJson());
    }

    public static void WriteToSaveFile() {}

    // The problem here is that save information is dependent on the scene. Our monster list, however I end up constructing it, is dependent on what scene we are in.
    // Got it, I think, create SceneSaveData.
    // Declare an array of them in GameManager, one entry for every scene.
    // Scene save data contains what RestZone to spawn at, (positive int)
    // An array of arrays of ints, or UIDS (no, docs say instance IDs are not persistent when loading from file, like prefabs)
    //  first layer holds which gaurdzone to spawn monsters in, second what and how many.
    // gaurdzone[1] returns mobs[]
    static SaveData CreateDefaultSave(string _name) {
        return new SaveData(_name);
    }
}
