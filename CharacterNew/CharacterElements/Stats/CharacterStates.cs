using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class CharacterStates
{
    // Trust this, it makes a lot of sense after some thought
    // Source = Where, Group = StateGroupSO, Mod = AttributeState 
    // Dictionary< Source, 
    //    Dictionary< Group, 
    //        List< Mod > > > > 



    Dictionary<StateSourceLogic, Dictionary<StateGroupLogic, List<AttributeStatePayload>>> _attribDict;
    List<AttribStateGroupSO> _activeAttributeGroups;
    // TODO: Mod attribDict and behaviourDict to hold a StateSource class and StateGroup
    //Dictionary<string, Dictionary<string, List<AttributeStatePayload>>> attribDict; 
    //Dictionary<string, Dictionary<string, List<BehaviourStatePayload>>> behaviourDict;
    Dictionary<StateSourceLogic, Dictionary<StateGroupLogic, List<BehaviourStatePayload>>> _behaveDict;


    string triggerSource;
    TriggerStatePayload triggerState; // does it make sense to have multiple? not really

    int _groupCount;
    public int groupCount {
        get {
            return _groupCount;
        }
    }
    
    int _stateCount;
    public int stateCount {
        get {
            return _stateCount;
        }
    }

    public CharacterStates() {
        _attribDict = new Dictionary<StateSourceLogic, Dictionary<StateGroupLogic, List<AttributeStatePayload>>>();
        _activeAttributeGroups = new List<AttribStateGroupSO>();
        _behaveDict = new Dictionary<StateSourceLogic, Dictionary<StateGroupLogic, List<BehaviourStatePayload>>>();
        _groupCount = 0;
        _stateCount = 0;
    }


    // TODO: Im not a fan of many focused update methods. The conditionals are all something that Behaviour (or dataManager) can track.
    // But imma keep it like this for the moment.
    // TODO: Check behaveDict aswell
    public void UpdateTime(float dt) {   
        // If no groups are in the source we delete it.
        /*
        string log = "";
        foreach(var group in _activeAttributeGroups) {
            log += group.name + ", ";
        }
        Debug.Log(log + " are in the active attribute groups list");
        */
        UpdateTimeAttrib(dt);
        UpdateTimeBehave(dt);
    }

    void UpdateTimeAttrib(float dt) {
        int i_2 = 0;
        StateSourceLogic[] toRemove_2 = new StateSourceLogic[16];
        
        foreach(var source_groupDict_pair in _attribDict) {
            // If conditions are no longer met for a group, it should be removed
            int i = 0;
            StateGroupLogic[] toRemove = new StateGroupLogic[16];

            Debug.Log("Update Time Attrib, source_groupDict_Pair has source " + source_groupDict_pair.Key.sourceName);

            foreach(var group_attribList_pair in source_groupDict_pair.Value) {
                StateGroupLogic sgLogic = group_attribList_pair.Key;
                sgLogic.UpdateTimers(dt);   
                float temp = 0;
                if(sgLogic.timers.Count > 0) {
                    temp = sgLogic.timers[0].counter;
                }
                
                if(!sgLogic.Check()) {
                    toRemove[i] = sgLogic;
                    i++;
                    _activeAttributeGroups.Remove(sgLogic.groupSO as AttribStateGroupSO);
                    _groupCount--;
                }

                sgLogic.RemoveStaleTimers();
            }
            Debug.Log("i is " + i);
            
            for (int i_ = 0; i_ < i; i_++) {
                _stateCount -= source_groupDict_pair.Value[toRemove[i_]].Count;
                source_groupDict_pair.Value.Remove(toRemove[i_]);   
                
            }

            if(source_groupDict_pair.Value.Count == 0) {
                toRemove_2[i_2] = source_groupDict_pair.Key;
                i_2++;
            }

        }
        
        for(int i_ = 0; i_ < i_2; i_++) {
            _attribDict.Remove(toRemove_2[i_]);
            Debug.Log("Source " + toRemove_2[i_].sourceName + " was removed");
        }

    } 

    void UpdateTimeBehave(float dt) {
        int i_2 = 0;
        StateSourceLogic[] toRemove_2 = new StateSourceLogic[16];
        
        foreach(var source_groupDict_pair in _behaveDict) {
            // If conditions are no longer met for a group, it should be removed
            int i = 0;
            StateGroupLogic[] toRemove = new StateGroupLogic[16];

            Debug.Log("Update Time Behave, source_groupDict_Pair has source " + source_groupDict_pair.Key.sourceName);

            foreach(var group_behaveList_pair in source_groupDict_pair.Value) {
                StateGroupLogic sgLogic = group_behaveList_pair.Key;
                sgLogic.UpdateTimers(dt);   
                float temp = 0;
                if(sgLogic.timers.Count > 0) {
                    temp = sgLogic.timers[0].counter;
                }
                
                if(!sgLogic.Check()) {
                    toRemove[i] = sgLogic;
                    i++;
                    //_activeAttributeGroups.Remove(sgLogic.groupSO as AttribStateGroupSO);
                    _groupCount--;
                }

                sgLogic.RemoveStaleTimers();
            }
            Debug.Log("i is " + i);
            
            for (int i_ = 0; i_ < i; i_++) {
                _stateCount -= source_groupDict_pair.Value[toRemove[i_]].Count;
                source_groupDict_pair.Value.Remove(toRemove[i_]);   
                
            }

            if(source_groupDict_pair.Value.Count == 0) {
                toRemove_2[i_2] = source_groupDict_pair.Key;
                i_2++;
            }

        }
        
        for(int i_ = 0; i_ < i_2; i_++) {
            _behaveDict.Remove(toRemove_2[i_]);
            Debug.Log("Source " + toRemove_2[i_].sourceName + " was removed");
        }

    } 

    public void UpdateRecieveAction(ImpactResult ImpR) {
        UpdateRecieveActionAttrib(ImpR);
        UpdateRecieveActionBehave(ImpR);
    }

    void UpdateRecieveActionAttrib(ImpactResult ImpR) {
        int i_2 = 0;
        StateSourceLogic[] toRemove_2 = new StateSourceLogic[16];
        
        foreach(var source_groupDict_pair in _attribDict) {
            // If conditions are no longer met for a group, it should be removed
            int i = 0;
            StateGroupLogic[] toRemove = new StateGroupLogic[16];

            Debug.Log("Update  RecAction Counter Attrib, source_groupDict_Pair has source " + source_groupDict_pair.Key.sourceName);

            foreach(var group_attribList_pair in source_groupDict_pair.Value) {
                StateGroupLogic sgLogic = group_attribList_pair.Key;
                sgLogic.UpdateRecActions(1, ImpR.actType);   
                float temp = 0;
                if(sgLogic.recActionCounters.Count > 0) {
                    temp = sgLogic.recActionCounters[0].counter;
                }
                
                if(!sgLogic.Check()) {
                    toRemove[i] = sgLogic;
                    i++;
                    _activeAttributeGroups.Remove(sgLogic.groupSO as AttribStateGroupSO);
                    _groupCount--;
                }

                sgLogic.RemoveStaleRecActionCounters();
            }
            Debug.Log("i is " + i);
            
            for (int i_ = 0; i_ < i; i_++) {
                _stateCount -= source_groupDict_pair.Value[toRemove[i_]].Count;
                source_groupDict_pair.Value.Remove(toRemove[i_]);   
                
            }

            if(source_groupDict_pair.Value.Count == 0) {
                toRemove_2[i_2] = source_groupDict_pair.Key;
                i_2++;
            }

        }
        
        for(int i_ = 0; i_ < i_2; i_++) {
            _attribDict.Remove(toRemove_2[i_]);
            Debug.Log("Source " + toRemove_2[i_].sourceName + " was removed");
        }

    }

    void UpdateRecieveActionBehave(ImpactResult ImpR) {
        int i_2 = 0;
        StateSourceLogic[] toRemove_2 = new StateSourceLogic[16];
        
        foreach(var source_groupDict_pair in _behaveDict) {
            // If conditions are no longer met for a group, it should be removed
            int i = 0;
            StateGroupLogic[] toRemove = new StateGroupLogic[16];

            Debug.Log("Update RecAction Counter Behave, source_groupDict_Pair has source " + source_groupDict_pair.Key.sourceName);

            foreach(var group_behaveList_pair in source_groupDict_pair.Value) {
                StateGroupLogic sgLogic = group_behaveList_pair.Key;
                sgLogic.UpdateRecActions(1, ImpR.actType);
                float temp = 0;
                if(sgLogic.timers.Count > 0) {
                    temp = sgLogic.recActionCounters[0].counter;
                }
                
                if(!sgLogic.Check()) {
                    toRemove[i] = sgLogic;
                    i++;
                    //_activeAttributeGroups.Remove(sgLogic.groupSO as AttribStateGroupSO);
                    _groupCount--;
                }

                sgLogic.RemoveStaleRecActionCounters();
            }
            Debug.Log("i is " + i);
            
            for (int i_ = 0; i_ < i; i_++) {
                _stateCount -= source_groupDict_pair.Value[toRemove[i_]].Count;
                source_groupDict_pair.Value.Remove(toRemove[i_]);   
                
            }

            if(source_groupDict_pair.Value.Count == 0) {
                toRemove_2[i_2] = source_groupDict_pair.Key;
                i_2++;
            }

        }
        
        for(int i_ = 0; i_ < i_2; i_++) {
            _behaveDict.Remove(toRemove_2[i_]);
            Debug.Log("Source " + toRemove_2[i_].sourceName + " was removed");
        }
    }


    public void UpdateSendActions(CharActionSO actionSO) {

    }

    public List<AttributeStatePayload> GetAffectingStatesNew(ATTRIBUTE s) {
        List<AttributeStatePayload> res = new List<AttributeStatePayload>();
        
        foreach(var sourceDict in _attribDict) {
            foreach(var groupDict in sourceDict.Value) {
                res.AddRange(
                    groupDict.Value.FindAll((AttributeStatePayload state) => { 
                        if(state.attribToChange == s)
                            return true; 
                        return false;
                        }
                    )
                );
            }
        }
        

        return res;
    }
    public List<BehaviourStatePayload> GetAffectingBehaviourStates(CHAR_EVENT e) {
        List<BehaviourStatePayload> res = new List<BehaviourStatePayload>();
        
        foreach(var sourceDict in _behaveDict) {
            foreach(var groupDict in sourceDict.Value) {
                res.AddRange(
                    groupDict.Value.FindAll((BehaviourStatePayload state) => { 
                        if(state.eventToMod == e)
                            return true; 
                        return false;
                        }
                    )
                );
            }
        }

        return res;
    }

    // TODO: Test that all Reapplication Conditions work as expected
    public void AddStateSource(StateSourceLogic source, Dictionary<StateGroupLogic, List<AttributeStatePayload>> groupDict) {


        Debug.Assert(source != null);
        Debug.Log("Source reapplication protocol " + source.reapplicationCond);

        switch(source.reapplicationCond) {
            case STATE_REAP_COND.RESET : 
                AddSourceReset(source, groupDict);

            break;
            case STATE_REAP_COND.STACK :
                AddSourceStack(source, groupDict);

            break;

            case STATE_REAP_COND.TOGGLE :
                AddSourceToggle(source, groupDict);

            break;
            default : 

                Debug.LogError("StateSource " + source.sourceName + " has no reapplicationCond");
            break;
        }
        
    }

    public void AddStateSource(StateSourceLogic source, Dictionary<StateGroupLogic, List<BehaviourStatePayload>> groupDict) {


        Debug.Assert(source != null);
        Debug.Log("Source reapplication protocol " + source.reapplicationCond);

        switch(source.reapplicationCond) {
            case STATE_REAP_COND.RESET : 
                AddSourceReset(source, groupDict);

            break;
            case STATE_REAP_COND.STACK :
                AddSourceStack(source, groupDict);

            break;

            case STATE_REAP_COND.TOGGLE :
                AddSourceToggle(source, groupDict);

            break;
            default : 

                Debug.LogError("StateSource " + source.sourceName + " has no reapplicationCond");
            break;
        }
        
    }
    
    void AddSourceStack(StateSourceLogic source, Dictionary<StateGroupLogic, List<AttributeStatePayload>> groupDict) {
        
            if(_attribDict.TryAdd(source, new Dictionary<StateGroupLogic, List<AttributeStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _attribDict[source].TryAdd(mods.Key, new List<AttributeStatePayload>());
                    _attribDict[source][mods.Key].AddRange(mods.Value);

                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, adding attributes.");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);


                    _attribDict[source].TryAdd(mods.Key, new List<AttributeStatePayload>());
                    _attribDict[source][mods.Key].AddRange(mods.Value);
                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                    
                }
            }
    }

    void AddSourceStack(StateSourceLogic source, Dictionary<StateGroupLogic, List<BehaviourStatePayload>> groupDict) {
        
            if(_behaveDict.TryAdd(source, new Dictionary<StateGroupLogic, List<BehaviourStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _behaveDict[source].TryAdd(mods.Key, new List<BehaviourStatePayload>());
                    _behaveDict[source][mods.Key].AddRange(mods.Value);

                    //_activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, adding behaviour.");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);


                    _behaveDict[source].TryAdd(mods.Key, new List<BehaviourStatePayload>());
                    _behaveDict[source][mods.Key].AddRange(mods.Value);
                    //_activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                    
                }
            }
    }

    void AddSourceToggle(StateSourceLogic source, Dictionary<StateGroupLogic, List<AttributeStatePayload>> groupDict) {
        
            if(_attribDict.TryAdd(source, new Dictionary<StateGroupLogic, List<AttributeStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _attribDict[source].TryAdd(mods.Key, new List<AttributeStatePayload>());
                    _attribDict[source][mods.Key].AddRange(mods.Value);
                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, removing.");
                int temp = 0;
                foreach (var group in groupDict) {
                    temp += group.Value.Count;
                    _groupCount--;
                }
                _attribDict.Remove(source);
                foreach(var mods in groupDict) {
                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);
                }
                _stateCount -= temp;
            }
    }

    void AddSourceToggle(StateSourceLogic source, Dictionary<StateGroupLogic, List<BehaviourStatePayload>> groupDict) {
        
            if(_behaveDict.TryAdd(source, new Dictionary<StateGroupLogic, List<BehaviourStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _behaveDict[source].TryAdd(mods.Key, new List<BehaviourStatePayload>());
                    _behaveDict[source][mods.Key].AddRange(mods.Value);
                    //_activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, removing.");
                int temp = 0;
                foreach (var group in groupDict) {
                    temp += group.Value.Count;
                    _groupCount--;
                }
                _behaveDict.Remove(source);
                /*
                foreach(var mods in groupDict) {
                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);
                }
                */
                _stateCount -= temp;
            }
    }

    void AddSourceReset(StateSourceLogic source, Dictionary<StateGroupLogic, List<AttributeStatePayload>> groupDict) {
        
            if(_attribDict.TryAdd(source, new Dictionary<StateGroupLogic, List<AttributeStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _attribDict[source].TryAdd(mods.Key, new List<AttributeStatePayload>());
                    _attribDict[source][mods.Key].AddRange(mods.Value);
                    _activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, reseting attributes.");
                foreach(var modsSors in groupDict) {
                    Debug.Assert(modsSors.Key != null);
                    // TODO: Two GroupLogics with the same timer added at the different time will behave like different keys
                    // Makes RESET:ing very icky, since we can't find it from here. We want it ideally to work like above.
                    // Perhaps we need to make it

                    int i = 0;
                    StateGroupLogic[] toRemove = new StateGroupLogic[16];
                    foreach(var modsDest in _attribDict[source]) {

                        // No, you cant delete in a foreach loop, we talked about this
                        if(modsDest.Key.SoftEquals(modsSors.Key)) {
                            toRemove[i] = modsDest.Key;
                            i++;
                        }
                    }

                    for(int i_ = 0; i_ < i; i_++) {
                        int temp = _attribDict[source][toRemove[i_]].Count;
                        _attribDict[source].Remove(toRemove[i_]);
                        _activeAttributeGroups.Remove(toRemove[i_].groupSO as AttribStateGroupSO);
                        _stateCount -= temp;

                        _attribDict[source].Add(modsSors.Key, modsSors.Value);
                        _activeAttributeGroups.Add(modsSors.Key.groupSO as AttribStateGroupSO);
                        _stateCount += modsSors.Value.Count;
                    }
                    
                }
            }
    }

    
    void AddSourceReset(StateSourceLogic source, Dictionary<StateGroupLogic, List<BehaviourStatePayload>> groupDict) {
        
            if(_behaveDict.TryAdd(source, new Dictionary<StateGroupLogic, List<BehaviourStatePayload>>())) {
                Debug.Log("No source " + source.sourceName + " found, making one");
                foreach(var mods in groupDict) {
                    Debug.Assert(mods.Key != null);

                    _behaveDict[source].TryAdd(mods.Key, new List<BehaviourStatePayload>());
                    _behaveDict[source][mods.Key].AddRange(mods.Value);
                    //_activeAttributeGroups.Add(mods.Key.groupSO as AttribStateGroupSO);

                    _stateCount += mods.Value.Count;
                    _groupCount++;
                }
            } else {
                Debug.Log(source.sourceName + " already exists, reseting attributes.");
                foreach(var modsSors in groupDict) {
                    Debug.Assert(modsSors.Key != null);
                    // TODO: Two GroupLogics with the same timer added at the different time will behave like different keys
                    // Makes RESET:ing very icky, since we can't find it from here. We want it ideally to work like above.
                    // Perhaps we need to make it

                    int i = 0;
                    StateGroupLogic[] toRemove = new StateGroupLogic[16];
                    foreach(var modsDest in _behaveDict[source]) {

                        // No, you cant delete in a foreach loop, we talked about this
                        if(modsDest.Key.SoftEquals(modsSors.Key)) {
                            toRemove[i] = modsDest.Key;
                            i++;
                        }
                    }

                    for(int i_ = 0; i_ < i; i_++) {
                        int temp = _behaveDict[source][toRemove[i_]].Count;
                        _behaveDict[source].Remove(toRemove[i_]);
                        //_activeAttributeGroups.Remove(toRemove[i_].groupSO as AttribStateGroupSO);
                        _stateCount -= temp;

                        _behaveDict[source].Add(modsSors.Key, modsSors.Value);
                        //_activeAttributeGroups.Add(modsSors.Key.groupSO as AttribStateGroupSO);
                        _stateCount += modsSors.Value.Count;
                    }
                    
                }
            }
    }


    public void SetTriggerState(string source, TriggerStatePayload state) {
        if(source != triggerSource) {
            triggerSource = source;
            triggerState = state;
        } else {
            triggerSource = null;
            triggerState = null;
        }
        //Debug.Log("Changed trigger state! : " + triggerSource + ", on state " + triggerState);
    }

    public CHAR_STATE Trigger() {
        if(triggerState == null) return CHAR_STATE.READY;

        return triggerState.actToTriggerOn;
    }

    public bool DoActionTrigger(CHAR_STATE thisState, CHAR_STATE targetState) {
        if(triggerState == null)
            return thisState == CHAR_STATE.READY;

        if(triggerState.triggerOnSendAction)
            return thisState == CHAR_STATE.READY;

        if(triggerState.triggerOnEnemy)
            return targetState == triggerState.actToTriggerOn;

        return thisState == triggerState.actToTriggerOn;
    }

    public bool DoActionTriggerExpire(CHAR_STATE thisState, CHAR_STATE targetState) {
        if(triggerState == null) {
            if(thisState == CHAR_STATE.READY) {
                triggerState = null;
                return true;
            }
            return false;
        }

        if(triggerState.triggerOnSendAction){
            if(thisState == CHAR_STATE.READY) {
                //triggerState = null;
                return true;
            }
            return false;
        }

        if(triggerState.triggerOnEnemy) {
            if(targetState == triggerState.actToTriggerOn) {
                triggerState = null;
                return true;
            }
            return false;
        }

        if(thisState == triggerState.actToTriggerOn) {
            triggerState = null;
            return true;
        }
        return false;
    }

    public bool SendActionTrigger(CHAR_STATE thisState, CHAR_STATE targetState) {
        if(triggerState == null) 
            return thisState == CHAR_STATE.POSTACTION;

        if(!triggerState.triggerOnSendAction)
            return thisState == CHAR_STATE.POSTACTION;

        if(triggerState.triggerOnEnemy)
            return targetState == triggerState.actToTriggerOn;

        return thisState == triggerState.actToTriggerOn;
    }

    public bool SendActionTriggerExpire(CHAR_STATE thisState, CHAR_STATE targetState) {
        if(triggerState == null) {
            if(thisState == CHAR_STATE.POSTACTION) {
                //triggerState = null;
                return true;
            }
            return false;
        }

        if(!triggerState.triggerOnSendAction){
            if(thisState == CHAR_STATE.POSTACTION) {
                //triggerState = null;
                return true;
            }
            return false;
        }

        if(triggerState.triggerOnEnemy) {
            if(targetState == triggerState.actToTriggerOn) {
                triggerState = null;
                return true;
            }
            return false;
        }

        if(thisState == triggerState.actToTriggerOn) {
            triggerState = null;
            return true;
        }
        return false;
    }

    public List<AttribStateGroupSO> FindActiveAttributeGroups() {
        string names = "";
        foreach(var attribGroup in _activeAttributeGroups) {
            names += attribGroup.name + ", ";
        }
        Debug.Log(names + " are in the list of active attribute groups");


        return _activeAttributeGroups;
    }
}
