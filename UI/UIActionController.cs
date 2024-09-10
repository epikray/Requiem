using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIActionController : MonoBehaviour
{
    public PCController controller;
    
    /* Prolly wont be needed */
    public RuntimeSet players;
    public RuntimeSet enemies;
    Behaviour behaviour;
    /* --- */

    [SerializeField]
    VisualTreeAsset ListEntryTemplate;

    VisualElement ButtonPanelRoot;
    UIActionViewController ActionViewController;
    UIActionButtonController ActionButtonController;

    BEHAVIOUR_CONTEXT prevBContext;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable() {
        behaviour = GameObject.Find("Player").GetComponent<Behaviour>();
        UIDocument BattleUI = GetComponent<UIDocument>();
        ButtonPanelRoot = BattleUI.rootVisualElement.Q<VisualElement>("Buttons");
        prevBContext = behaviour.context;

        ActionButtonController = new UIActionButtonController();
        
        ActionViewController = new UIActionViewController();
        //IReadOnlyList<CharActionSO> actions = behaviour.GetDataManager().cInstance.Attacks;        

        ActionViewController.InitializeActionList(
            BattleUI.rootVisualElement, 
            ListEntryTemplate, 
            behaviour.GetDataManager().aSelector); 
            // This turns out null at times. Race condition between OnEnable and Awake?
            // I though Awake would be called before OnEnable. Check docs.
        ActionViewController.Hide();

        ActionButtonController.InitializeActionButton(
            BattleUI.rootVisualElement, 
            behaviour.GetDataManager().aSelector);

        ActionButtonController.Hide();

        ActionButtonController.SetAttackAction(() => {
            if(behaviour.context != BEHAVIOUR_CONTEXT.BATTLE) return;
            behaviour.GetDataManager().aSelector.SetActionType(ACT_TYPE.ATTACK);
            ActionButtonController.Hide();
            
            ActionViewController.RefillActionList(behaviour.GetDataManager().cInstance.Attacks);
            ActionViewController.SelectFromList(0);
            ActionViewController.Show();
        });

        ActionButtonController.SetSkillAction(() => {
            if(behaviour.context != BEHAVIOUR_CONTEXT.BATTLE) return;
            behaviour.GetDataManager().aSelector.SetActionType(ACT_TYPE.SKILL);
            ActionButtonController.Hide();

            ActionViewController.RefillActionList(behaviour.GetDataManager().cInstance.Skills);
            ActionViewController.SelectFromList(0);
            ActionViewController.Show();
        });

        ActionButtonController.SetStanceAction(() => {
            if(behaviour.context != BEHAVIOUR_CONTEXT.BATTLE) return;
            behaviour.GetDataManager().aSelector.SetActionType(ACT_TYPE.STANCE);
            ActionButtonController.Hide();

            ActionViewController.RefillActionList(behaviour.GetDataManager().cInstance.Stances);
            ActionViewController.SelectFromList(0);
            ActionViewController.Show();
        });

        ActionButtonController.SetStrategyAction(() => {
            if(behaviour.context != BEHAVIOUR_CONTEXT.BATTLE) return;
            behaviour.GetDataManager().aSelector.SetActionType(ACT_TYPE.STRATEGY);
            ActionButtonController.Hide();

            ActionViewController.RefillActionList(behaviour.GetDataManager().cInstance.Strategies);
            ActionViewController.SelectFromList(0);
            ActionViewController.Show();
        });

    }

    // Update is called once per frame
    void Update()
    {
        if(contextChanged(behaviour.context, prevBContext)) {
            if(behaviour.context == BEHAVIOUR_CONTEXT.WORLD) {
                ActionButtonController.Hide();
                ActionViewController.Hide();
            } else if (behaviour.context == BEHAVIOUR_CONTEXT.BATTLE) {
                ActionButtonController.Show();
                ActionViewController.Hide();
            }
        }

        ActionButtonController.Control(behaviour.context, controller);

        ActionViewController.Control(behaviour.context);




        // TODO: Controller.cancelPressed;
        // I want something slightly more sophisticated.
        // If there is a confirmed (Submitted) selection, I want pressing Q to deselect while staying in the menu.
        // If there isnt a selection, Q does this.
        if(Input.GetKeyDown(KeyCode.Q)) {
            if(ActionViewController.HasChosen()) {
                // TODO: but still keep item focused
                // Cancel event doing more than I want
                ActionViewController.Deselect();
                ActionViewController.MoveSelectFromList(0);
            } else {
                ActionViewController.Hide();
                ActionButtonController.Show();
            }   
        }

        prevBContext = behaviour.context;
    }
    
    bool contextChanged(BEHAVIOUR_CONTEXT cur, BEHAVIOUR_CONTEXT prev) {
        int diff = cur - prev;
        if(diff != 0) {
            return true;
        }
        return false;
    }

}

// All control we need is show and hide. The problem with this ui shit on keyboard is just making sure focus is maintained
public class UIActionButtonController {
    VisualElement buttonPanel;
    ActionSelector actionSelectionLogic;


    Button AttackButton;
    Button SkillButton;
    Button StanceButton;
    Button StrategyButton;


    public void InitializeActionButton(VisualElement root, ActionSelector charActionSelector) {
        buttonPanel = root.Q<VisualElement>("Buttons");
        AttackButton = buttonPanel.Q<Button>("AttackButton");
        SkillButton = buttonPanel.Q<Button>("SkillButton");
        StanceButton = buttonPanel.Q<Button>("StanceButton");
        StrategyButton = buttonPanel.Q<Button>("StrategyButton");

        actionSelectionLogic = charActionSelector;
    }

    public void Control(BEHAVIOUR_CONTEXT context, PCController controller) {
        if(!buttonPanel.visible) return;

        if(context != BEHAVIOUR_CONTEXT.BATTLE) return;

        // Read from PCController
        
        if(Input.GetKeyDown(KeyCode.W)) {
            AttackButton.Focus();
        }
        if(Input.GetKeyDown(KeyCode.A)) {
            SkillButton.Focus();
        }
        if(Input.GetKeyDown(KeyCode.D)) {
            StanceButton.Focus();
        }
        if(Input.GetKeyDown(KeyCode.S)) {
            StrategyButton.Focus();
        }
    }

    public void SetAttackAction(Action buttonAction) {
        AttackButton.clicked += buttonAction;
    }
    public void SetSkillAction(Action buttonAction) {
        SkillButton.clicked += buttonAction;
    }
    public void SetStanceAction(Action buttonAction) {
        StanceButton.clicked += buttonAction;
    }
    public void SetStrategyAction(Action buttonAction) {
        StrategyButton.clicked += buttonAction;
    }

    public void Show() {
        buttonPanel.visible = true;
        buttonPanel.Focus();
    }

    public void Hide() {
        buttonPanel.visible = false;
        buttonPanel.Blur();
    }

}
public class UIActionViewController {
    
    VisualTreeAsset ListEntryTemplate;
    ActionSelector actionSelectionLogic;
    
    VisualElement actionView;
    ListView actionList;
    Label actionName;
    Label actionDescription;
    VisualElement actionPortrait;

    List<CharActionSO> allActions;
    List<CharActionSO> emptyList;

    int actionListIndex;

    public void InitializeActionList(VisualElement root, VisualTreeAsset listElementTemplate, ActionSelector charActionSelector) {
        actionListIndex = 0;
        emptyList = new List<CharActionSO>();
        ActionSelector actionSelectionLogic = charActionSelector;

        EnumerateAllActions(emptyList);

        ListEntryTemplate  = listElementTemplate;

        actionView = root.Q<VisualElement>("ActionView");
        actionList = root.Q<ListView>("ActionList");
        actionName = root.Q<Label>("Name");
        actionDescription = root.Q<Label>("Description");

        FillActionList(); // Idk how to show an empty list

        actionList.onSelectionChange += OnActionSelected;
        actionList.onItemsChosen += OnActionSubmit;
    }

    public void Control(BEHAVIOUR_CONTEXT context) {
        if(!actionView.visible) return;

        if(context != BEHAVIOUR_CONTEXT.BATTLE) return;

        if(actionList.selectedIndex > -1)
            actionListIndex = actionList.selectedIndex;

        Show();

        if(Input.GetKeyDown(KeyCode.B)) {
            MoveSelectFromList(-1);
        }
        if(Input.GetKeyDown(KeyCode.N)) {
            MoveSelectFromList(1);
        }
    }

    public void RefillActionList(IReadOnlyList<CharActionSO> actions) {
        allActions.Clear();
        allActions.AddRange(actions);
        // Do i need to clear the listview?
        FillActionList();
        actionList.Rebuild();
    }

    public void SelectFromList(int i) {
        int upper = actionList.itemsSource.Count - 1;
        if (upper < 0) {
            upper = 0;
        }

        actionListIndex = Math.Clamp(i, 0, upper);
        //if(actionListIndex == -1) actionListIndex = 0;
        actionList.ScrollToItem(actionListIndex);
        actionList.SetSelection(actionListIndex);
    }

    public void MoveSelectFromList(int i) {
        //Debug.Log("pre MoveSelectFromList actionListIndex " + actionListIndex);
        SelectFromList(actionListIndex + i);
        //Debug.Log("post MoveSelectFromList actionListIndex " + actionListIndex);
    }

    public void Show() {
        actionView.visible = true;
        actionList.Focus();
    }

    public void Hide() {
        actionView.visible = false;
        actionList.Blur();
    }

    public bool HasChosen() {
        return actionSelectionLogic.HasSelection();
    }

    public void Deselect() {
        
        actionSelectionLogic.CancelSelection();
        
    }

    void EnumerateAllActions(IReadOnlyList<CharActionSO> actions) {
        allActions = new List<CharActionSO>();
        allActions.AddRange(actions);
    }

    void FillActionList() {

        actionList.makeItem = () => {
            // idk, some lambda nonsense

            var newListEntry = ListEntryTemplate.Instantiate();
            var newListEntryLogic = new UIActionEntry();

            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);

            return newListEntry;
        };

        actionList.bindItem = (item, index) => {
            (item.userData as UIActionEntry).SetActionData(allActions[index]);
        };

        actionList.fixedItemHeight = 45;

        // Set the actual item's source list/array
        actionList.itemsSource = allActions;
    }

    void OnActionSelected(IEnumerable<object> selectedItems) {

        var selectedAction = actionList.selectedItem as CharActionSO;
        //Debug.Log("OnActionSelected| ActionSelector assigned: " + actionSelectionLogic.ToString()); //Why is the ref lost?
        //Debug.Log("UIActionButtonController/OnActionSelected: selectedindex : " + actionList.selectedIndex);
        //Debug.Log("UIActionButtonController/OnActionSelected: actionType : " + actionSelectionLogic.actionType);
        if(actionSelectionLogic == null) {
            //Debug.Log("UIActionViewController ActionSelector reference was null. Getting a new one.");
            actionSelectionLogic = GameObject.Find("Player").GetComponent<Behaviour>().GetDataManager().aSelector;
        }
        actionSelectionLogic.SelectAt(actionList.selectedIndex);

        //Debug.Log("What is actionListIndex doing? " + actionListIndex);
        
        if(selectedAction == null) {
            // clear top label
            actionName.text = "";
            actionDescription.text = "";
            ///actionPortrait.style.backgroundImage = null;

            return;
        }

        // Fill in character details
        actionName.text = selectedAction.name;
        actionDescription.text = selectedAction.description;
        //actionPortrait.style.backgroundImage = new StyleBackground(selectedAction.PortraitImage);
    }

    void OnActionSubmit(IEnumerable<object> selectedItems) {
        actionSelectionLogic.SubmitAction();
        //Debug.Log(actionSelectionLogic.GetSubmittedAction(false).name + " I CHOOSE YOU!");
    }
}

public class UIActionEntry {
    Label name;
    Label description;

    public void SetVisualElement (VisualElement element) {
        name = element.Q<Label>("Name");
        description = element.Q<Label>("Description");
    }

    public void SetActionData (CharActionSO actionSO) {
        name.text = actionSO.name;
        description.text = actionSO.description;
    }

}