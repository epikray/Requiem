using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;


[Serializable]
public class SaveData {
    // Display name of the file
    public string name;
    // Last location of the player, could be an int if spawns only happen at fixed positions (restzones or gaurdzones)
    public int locationSpot;
    public SceneSaveData sceneData;
    public SaveData(string _name) {
        name = _name;
        locationSpot = 0;
        sceneData = GameManager.GetDefaultSceneSaveData(1);
    }
    public string ToJson() {
        return JsonUtility.ToJson(this);
    }

    public static SaveData FromJson(string json) {
        return JsonUtility.FromJson<SaveData>(json);
    }

}
[Serializable]
public class SceneSaveData {
    // index = gaurdzone to spawn at
    // entry = number of and name of npc to spawn
    public string name;

    public List<GuardZoneSaveData> gaurdZoneData;
    
    public SceneSaveData(string _name, List<GuardZoneSaveData> _gaurdZoneData) {
        name = _name;
        gaurdZoneData = _gaurdZoneData;
    }
    
    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}

[Serializable]
public class GuardZoneSaveData {
    // Contains an 1D array of MonsterCountSaveData
    public List<MonsterEntrySaveData> monsterData;

    public GuardZoneSaveData(List<MonsterEntrySaveData> _monsterData) {
        monsterData = _monsterData;
    }

}

[Serializable]
public class MonsterEntrySaveData {
    // Contains a monster name and amount
    public string name;
    public int count;

    public MonsterEntrySaveData(string _name, int _count) {
        name = _name;
        count = _count;
    }
}
