using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class UIStateListController : MonoBehaviour {


    //
    //UIStateListViewController StateListViewController;
    [SerializeField]
    VisualTreeAsset listEntryTemplate;

    UIStateListViewController stateViewController;

    CharacterStates characterStates;

    int lastGroupCount = 0;

    void Awake() {

    }

    void Start() {
        lastGroupCount = 0;

        characterStates = GameObject.FindGameObjectWithTag("Player").GetComponent<Behaviour>().GetDataManager().cStates;
        Debug.Assert(characterStates != null);
    }

    void OnEnable() {
        var UIDocument = GetComponent<UIDocument>();

        stateViewController = new UIStateListViewController();
        stateViewController.InitializeStateList(UIDocument.rootVisualElement, listEntryTemplate);
    }

    void Update() {
        // Check if CharacterStates has changed its group count.
        // TODO: Doesnt find Player
        int currentGroupCount = characterStates.groupCount;
        if(currentGroupCount != lastGroupCount) {
            lastGroupCount = currentGroupCount;

            // Populate the StateListView

            stateViewController.PopulateStateList(characterStates.FindActiveAttributeGroups());
        }

    }

}


public class UIStateListViewController {
    VisualTreeAsset _listEntryTemplate;
    VisualElement _statesView;
    ListView _stateList;
    Label _statesTitle;

    List<AttribStateGroupSO> allActiveStates;

    public void InitializeStateList(VisualElement root, VisualTreeAsset listElementTemplate) {
        allActiveStates = new List<AttribStateGroupSO>();
        EnumerateAllActiveStates(new List<AttribStateGroupSO>());

        _listEntryTemplate = listElementTemplate;
        _statesView = root.Q<VisualElement>("StatesView");
        _stateList = root.Q<ListView>("StatesList"); // cant find stateslist???
        _statesTitle = root.Q<Label>("StatesTitle");   

        FillStateList();
    }

    public void PopulateStateList(List<AttribStateGroupSO> groups) {
        // It works now
        /*
        Debug.Log("groups count " + groups.Count);
        string groupNames = "";
        foreach (AttribStateGroupSO group in groups) {
            groupNames += group.name + ", ";
        }
        Debug.Log("Want to display " + groupNames);
        */

        EnumerateAllActiveStates(groups);
        FillStateList();
    }   

    void EnumerateAllActiveStates(IReadOnlyList<AttribStateGroupSO> stateGroups) {
        allActiveStates.Clear();
        allActiveStates.AddRange(stateGroups);
    }

    void FillStateList() {
        /*
        if(allActiveStates.Count == 0) {
            _stateList.Clear();
            return;
        }
        */

        _stateList.makeItem = () => {
            var newListEntry = _listEntryTemplate.Instantiate();
            var newListEntryLogic = new UIStateEntry();

            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);

            return newListEntry;
        };

        _stateList.bindItem = (item, index) => {
            (item.userData as UIStateEntry).SetActionData(allActiveStates[index]);
        };

        _stateList.fixedItemHeight = 45;
        _stateList.itemsSource = allActiveStates;
    }

}

// Same can be used for behaviours aswell if we use charstateSO instead 
public class UIStateEntry {
    Label name;
    Label description;

    public void SetVisualElement (VisualElement element) {
        name = element.Q<Label>("Name");
        description = element.Q<Label>("Description");
    }

    public void SetActionData (AttribStateGroupSO aStateGroupSO) {
        name.text = aStateGroupSO.name;
        description.text = aStateGroupSO.description;
    }

}