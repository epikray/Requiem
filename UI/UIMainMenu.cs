using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class UIMainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    UIDocument doc;
    void Awake() {
        doc = GetComponent<UIDocument>();
        VisualElement root = doc.rootVisualElement;

        root.Q<Button>("B_NewGame").clicked += NewGame;

        // TODO: Open List of saveFiles and Display them
        root.Q<Button>("B_LoadGame").clicked += LoadGame;

        root.Q<Button>("B_QuitGame").clicked += QuitGame;
    }   

    private void LoadGame() {
        SaveSystem.LoadSaveFile(0);
    }

    private void NewGame() {
        SaveSystem.CreateNewSaveFile("New Save");
    }

    private void QuitGame() {
        GameManager.QuitGame();
    }
    
}
