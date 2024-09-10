using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameManager
{   
    // I should make this private, cuz I dont want anyone to write to it
    private static List<SceneSaveData> _sceneSaveDatasDefault = new List<SceneSaveData> {
        new SceneSaveData(
            "DebugWorld",
            new List<GuardZoneSaveData> {
                new GuardZoneSaveData(
                    new List<MonsterEntrySaveData> {
                        new MonsterEntrySaveData("NPC_Phantom", 1), 
                    }
                )
            }
        ),
        new SceneSaveData(
            "PrologueWorld",
            new List<GuardZoneSaveData> {
                new GuardZoneSaveData(
                    new List<MonsterEntrySaveData> {
                        new MonsterEntrySaveData("NPC_Ghost", 1), 
                    }
                ),
                new GuardZoneSaveData(
                    new List<MonsterEntrySaveData> {
                        new MonsterEntrySaveData("NPC_Wolf", 1), 
                    }
                )
            }
        )
    };  
    
    public static string[] scenes = {
        "DebugWorld",
        "PrologueWorld",  
    };

    private static SaveData cachedData = null;

    public static SceneSaveData GetDefaultSceneSaveData(string s) {
        switch(s) {
            case "DebugWorld":
                return _sceneSaveDatasDefault[0];
            case "PrologueWorld":
                return _sceneSaveDatasDefault[1];
            default:
                Debug.LogError("No matching SceneSaveData for " + s);
                break;
        }
        return null;
    }

    public static SceneSaveData GetDefaultSceneSaveData(int ind) {
        return _sceneSaveDatasDefault[ind];
    }
    
    //static Scene scene;
    public static void LoadGameFromSaveData(SaveData data) {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        if(data == null) {
            Debug.LogError("Trying to load null save data.");
            return;
        }
        cachedData = data;
        string sceneName = data.sceneData.name;
        var parameters = new LoadSceneParameters(LoadSceneMode.Single);

        // TODO: Bad name!!!!
        SceneManager.UnloadSceneAsync("Menu");

        // Are there any perks of making game scene load async? Non that I can think of.
        Scene scene = SceneManager.LoadScene(sceneName, parameters);

        // When Can I unload Menu scene?
        // SceneManager.SetActiveScene(scene);
        

        

        

        // when the world has finnished loading, spawn enemies and player.
        // that can be handeled by Director. Prefab instantiation can be done with InstantiatePrefab
    }

    static void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        SaveData data = cachedData;
        cachedData = null;

        InitWorld(data);
    }   

    static void InitWorld(SaveData data) {
        PlacePlayerAtRestZone(data.locationSpot);
    }

    static void PlacePlayerAtRestZone(int zoneNum) {
        GameObject[] restZones = GameObject.FindGameObjectsWithTag("RestZone");
        string log = "Are RestZones ordered? ";
        foreach(var zone in restZones) {
            log += zone.name + ", ";
        }
        Debug.Log(log);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        // Note: NavMeshAgent Overrides a lot of Transform functionality
        player.GetComponent<NavMeshAgent>().Warp(restZones[zoneNum].transform.position);
    }

    public static void QuitGame() {
        Application.Quit();
    }


}
