using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public PCController controller;
    public RuntimeSet players;
    public RuntimeSet enemies;

    RuntimeSet behaviourSet;
    Behaviour behaviour;

    Behaviour playerBehaviour;
    int behaviourIndex = 0;

    UIDocument BattleUI;
    ProgressBar ActionCharge;
    ProgressBar ActionTimer;
    
    Label Name;
    Label DebugLabel;

    VisualElement Health;
    VisualElement Stamina;
    VisualElement Focus;
    VisualElement Will;
    
    VisualElement Strength;
    VisualElement Dexterity;
    VisualElement Intellect;
    VisualElement Spirit;
    
    VisualElement Rage;
    VisualElement Suave;
    VisualElement Wit;
    VisualElement Empathy;

    VisualElement Toughness;
    VisualElement Agility;
    VisualElement Resistance;
    VisualElement Stability;

    public VisualTreeAsset character_marker;
    public VisualTreeAsset taget_marker;

    BattleUICharMarker charMarker;
    BattleUICharMarker targetMarker;

    

    void OnEnable() {
        behaviourSet = players;
        behaviourIndex = 0;
        if(behaviourSet.Count != 0)
            behaviour = behaviourSet.At(behaviourIndex).GetComponent<Behaviour>();
        else
            behaviour = GameObject.Find("Player").GetComponent<PCBehaviour>();

        playerBehaviour = GameObject.Find("Player").GetComponent<PCBehaviour>();

        BattleUI = GetComponent<UIDocument>();

        if(BattleUI == null) {
            Debug.LogError("Missing UIDocument!");
        }
        
        VisualElement ChargeRoot = BattleUI.rootVisualElement.Q<VisualElement>("VTimers");
        ActionCharge = ChargeRoot.Q<ProgressBar>("ActionCharge");
        ActionTimer = ChargeRoot.Q<ProgressBar>("ActionTimer");
        if(ActionCharge == null) {
            Debug.LogError("Missing visual element 'ActionCharge'!");
        }
        if(ActionTimer == null) {
            Debug.LogError("Missing visual element 'ActionTimer'!");
        }
        VisualElement StatsRoot = BattleUI.rootVisualElement.Q<VisualElement>("VStats");

        Health = FindStatView(StatsRoot, "HealthElement");
        Stamina = FindStatView(StatsRoot, "StaminaElement");
        Focus = FindStatView(StatsRoot, "FocusElement");
        Will = FindStatView(StatsRoot, "WillElement");
        /*
        Strength = FindStatView(StatsRoot, "StrengthElement");
        Dexterity = FindStatView(StatsRoot, "DexterityElement");
        Intellect = FindStatView(StatsRoot, "IntellectElement");
        Spirit = FindStatView(StatsRoot, "SpiritElement");

        Rage = FindStatView(StatsRoot, "RageElement");
        Suave = FindStatView(StatsRoot, "SuaveElement");
        Wit = FindStatView(StatsRoot, "WitElement");
        Empathy = FindStatView(StatsRoot, "EmpathyElement");

        Toughness = FindStatView(StatsRoot, "ToughnessElement");
        Agility = FindStatView(StatsRoot, "AgilityElement");
        Resistance = FindStatView(StatsRoot, "ResistanceElement");
        Stability = FindStatView(StatsRoot, "StabilityElement");
        */
        DebugLabel = BattleUI.rootVisualElement.Q<Label>("DebugLabel");

        Name = BattleUI.rootVisualElement.Q<Label>("BName");
        if(DebugLabel == null) {
            Debug.LogError("Missing visual element 'DebugLabel'!");
        }
        if(Name == null) {
            Debug.LogError("Missing visual element 'BName'!");
        }
        

        
        //charMarker.objectToMark = GameObject.Find("Player");

        VisualElement marker = character_marker.Instantiate();
        BattleUI.rootVisualElement.Add(marker);
        charMarker = new BattleUICharMarker( marker, behaviour.gameObject);
        
        marker = taget_marker.Instantiate();
        BattleUI.rootVisualElement.Add(marker);
        targetMarker = new BattleUICharMarker( marker, null);

    }

    void Awake() {
        
    }

    void Start() {
        
    }

    // Update is called once per frame
    void Update()
    {   

        // TODO: Rework this to show behaviour targetted by player
        //       shrimply use GetTarget :)
        behaviour = playerBehaviour.GetDataManager().targeting.GetTarget();


        /*
        if(Input.GetKeyDown(KeyCode.C)) {
            behaviourIndex = 0;
            if(behaviourSet == players) {
                behaviourSet = enemies;
            } else {
                behaviourSet = players;
            }
            if(behaviourSet.Count != 0)
                behaviour = behaviourSet.At(behaviourIndex).GetComponent<Behaviour>();
            else
                behaviour = null;
        }
        if(Input.GetKeyDown(KeyCode.X)) {
            behaviourIndex++;
            if(behaviourIndex >= behaviourSet.Count) {
                behaviourIndex = 0;
            }

            if(behaviourSet.Count != 0)
                behaviour = behaviourSet.At(behaviourIndex).GetComponent<Behaviour>();
            else
                behaviour = null;
        }
        */

        if(behaviour == null) {
            behaviour = playerBehaviour;
        }
        charMarker.objectToMark = behaviour.gameObject;
        
        Behaviour target = playerBehaviour.GetDataManager().targeting.GetConfirmedTarget();
        if(target == null)
            targetMarker.objectToMark = null;
        else {
            targetMarker.objectToMark = target.gameObject;
        }   
        

        /*
            Study this case:
            https://forum.unity.com/threads/trigger-button-click-from-code.1124356/
            This too, Ive got the hunch that preventDefault() will allow us to ignore navigation events
            https://docs.unity3d.com/2021.3/Documentation/Manual/UIE-Events-Handling.html
        */
        Name.text = behaviour.name;
        float ACv = behaviour.GetDataManager().actionCharge.Cur; 
        float ACm = behaviour.GetDataManager().actionCharge.End;
        ACv = ACv/ACm*100f;
        ActionCharge.value = ACv;
        if(behaviour.GetDataManager().actionCharge.Locked) {
            ActionCharge.style.borderBottomWidth = 5;
            ActionCharge.style.borderTopWidth = 5;
            ActionCharge.style.borderLeftWidth = 5;
            ActionCharge.style.borderRightWidth = 5;
        } else {
            ActionCharge.style.borderBottomWidth = 0;
            ActionCharge.style.borderTopWidth = 0;
            ActionCharge.style.borderLeftWidth = 0;
            ActionCharge.style.borderRightWidth = 0;
        }
        /*
        float ATv = behaviour.GetDataManager().actionTimer.Cur;
        float ATm = behaviour.GetDataManager().actionTimer.Start;
        ATv = ATv/ATm*100f;
        ActionTimer.value = ATv;
        if(behaviour.GetDataManager().actionTimer.Locked) {
            ActionTimer.style.borderBottomWidth = 5;
            ActionTimer.style.borderTopWidth = 5;
            ActionTimer.style.borderLeftWidth = 5;
            ActionTimer.style.borderRightWidth = 5;
        } else {
            ActionTimer.style.borderBottomWidth = 0;
            ActionTimer.style.borderTopWidth = 0;
            ActionTimer.style.borderLeftWidth = 0;
            ActionTimer.style.borderRightWidth = 0;
        }
        */
        PropertyCalculator propCalc = behaviour.GetDataManager().cProperties;

        DrawStatView(Health, propCalc, STAT.HEALTH);
        DrawStatView(Stamina, propCalc, STAT.STAMINA);
        DrawStatView(Focus, propCalc, STAT.FOCUS);
        DrawStatView(Will, propCalc, STAT.WILL);
        
        /*
        DrawStatView(Strength, propCalc, STAT.STRENGTH);
        DrawStatView(Dexterity, propCalc, STAT.DEXTERITY);
        DrawStatView(Intellect, propCalc, STAT.INTELLECT);
        DrawStatView(Spirit, propCalc, STAT.SPIRIT);
        
        DrawStatView(Rage, propCalc, STAT.RAGE);
        DrawStatView(Suave, propCalc, STAT.GRACE);
        DrawStatView(Wit, propCalc, STAT.ATTENTION);
        DrawStatView(Empathy, propCalc, STAT.AWARNESS);
        
        DrawStatView(Toughness, propCalc, STAT.TOUGHNESS);
        DrawStatView(Agility, propCalc, STAT.AGILITY);
        DrawStatView(Resistance, propCalc, STAT.RESISTANCE);
        DrawStatView(Stability, propCalc, STAT.BALANCE);
        */

        string debugText = "Action Shit\n" +
        behaviour.GetActionInfo() + "\n" +
        "Action State? " + behaviour.GetDataManager().characterState;
        
        DebugLabel.text = debugText;

        charMarker.Update();
        targetMarker.Update();

        //Debug.Log("UI Base layout " + BattleUI.rootVisualElement.layout.width + " "  + BattleUI.rootVisualElement.layout.height);
    }

    void DrawStatView(VisualElement element, PropertyCalculator statCalc, STAT stat) {
        //Label name = element.Q<Label>("name");
        Label baseV = element.Q<Label>("base");
        Label max = element.Q<Label>("max");
        Label curV = element.Q<Label>("value");
        

        baseV.text = "100";
        // 0 1 2 3  8 9 10 11, a Pool
        if((int)stat < 4 || ((int)stat > 7 && (int)stat < 12)) {
            (float, float) val_Max = statCalc.GetPool(stat);
            max.text = val_Max.Item2.ToString();
            curV.text = val_Max.Item1.ToString();
        } else {
            float val = statCalc.GetStat(stat);
            curV.text = val.ToString();
        }
    }

    VisualElement FindStatView(VisualElement root, string name) {
        VisualElement VE = root.Q<VisualElement>(name);
        if(VE == null) {
            Debug.LogError("Could not find element " + name + " in " + root.name);
            return null;
        }
        
        return VE;
    }
}

public class UIStatController {

}

