using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// I might also want have special rules on Source Reaplication
/*
    Stacking sources.
    Conditions reset.
    Removes States of same source
*/
// Glorified string essentially.
// AND!!! And, holds a SourceReapplication protocol

[Serializable]
public enum STATE_REAP_COND : int {
    STACK,
    RESET,
    TOGGLE,
}



// I just realized. This is all we really need. Then we have a function that takes in a StateSourceLogic.
// The same can be done with GroupLogic, just have the class be used as argument for a DetermineConditionRequirements.
// We should change the name from Logic however, since it is not a logic class, its a data class.
[Serializable]
public class StateSourceLogic : IEquatable<StateSourceLogic> {
    public STATE_REAP_COND reapplicationCond;
    // Source has a reference to the action? Not really needed

    /*
    CharStateSO _sourceSO;
    public CharStateSO sourceSO {
        get {
            return _sourceSO;
        }
    }
    */
    string _sourceName;
    public string sourceName {
        get {
            return _sourceName;
        }
        set {
            _sourceName = value;
        }
    }

    public bool Equals(StateSourceLogic rhs) {
        if(sourceName != rhs.sourceName) {
            return false;
        }

        return reapplicationCond == rhs.reapplicationCond;
    }
}



// What possible clear conditions do I want?
/*
    Never clears
        (Kinda nutty... truly permanent state groups leads into the realm of "character states needs to go into character ID"-topia)
    Clears on Source reaplication
        (Might just fall on source reaplication rules)
    Clears after timer 
        (Can specify length, if its based on charge)
    Clears after recieving action
        (Can specify, type, count, etc.)
    Clears on dmg threshhold 
        (Can specify, amount, type, etc.)
    Clears after sending action
        (Can specify, type, count, etc.)

    These are all very different and very distant from how they work.
    But they all the abstraction they either 'Hold' or have 'Failed'.
    They have to have some connection to Behaviour and Afflictions since a Condition can wish to count damage taken or time deltas.
    Time is fairly easy, damage is hard... since final damage taken has to go through CharPropertyCalculator...

    To do this properly, we might want to unify the 'path' were damage is recieved.

    But we might wanna do it based of impact... Aight we've got somewhat of a hacky solution to the 'where?'.
    We just need a 'how' now


*/
[Serializable]
public enum STATE_CLEAR_COND : int {
    NEVER,
    TIMER,
    RECIEVEACTION,
    SENDACTION,
    DMGTHRESHOLD,
}


// Holds many conditionals of when its child mods need to be 
// Eh, lets just do the dumb solution
// If counter goes to zero, conditional is broken
[Serializable]
public class StateGroup_Timer {
    STATE_CLEAR_COND _condType;
    public STATE_CLEAR_COND condType {
        get {
            return _condType;
        }
    }
    float _startCounter;
    public float startCounter {
        get {
            return _startCounter;
        }
    }
    public float counter;

    public StateGroup_Timer(float v) {
        _condType = STATE_CLEAR_COND.TIMER;
        counter = v;
        _startCounter = v;
    }

    public StateGroup_Timer(StateGroup_Timer rhs) {
        _condType = rhs._condType;
        counter = rhs.counter;
        _startCounter = counter;
    }

    // Returns false if counter has reached zero (condition is not met anymore)
    public void UpdateConditional(float v) {
        counter -= v;
    }

    // true if condition holds
    public bool Check() {
        return counter > 0;
    }
}

[Serializable]
public class StateGroup_RecieveActionCounter {
    STATE_CLEAR_COND _condType;
    public STATE_CLEAR_COND condType {
        get {
            return _condType;
        }
    }
    public ACT_TYPE typeCriteria;
    
    int _startCounter;
    public int startCounter {
        get {
            return _startCounter;
        }
    }

    public int counter;

    public StateGroup_RecieveActionCounter(int v, ACT_TYPE t) {
        _condType = STATE_CLEAR_COND.RECIEVEACTION;
        counter = v;
        typeCriteria = t;
        _startCounter = v;
    }

    public StateGroup_RecieveActionCounter(StateGroup_RecieveActionCounter rhs) {
        _condType = rhs._condType;
        counter = rhs.counter;
        typeCriteria = rhs.typeCriteria;
        _startCounter = counter;
    }

    // Returns false if counter has reached zero (condition is not met anymore)
    public void UpdateConditional(int v, ACT_TYPE t) {
        if(typeCriteria == t) {
            counter -= v;
        }
    }

    public bool Check() {
        return counter > 0;
    }
}

[Serializable]
public class StateGroup_SendActionCounter {
    STATE_CLEAR_COND _condType;
    public STATE_CLEAR_COND condType {
        get {
            return _condType;
        }
    }
    public ACT_TYPE typeCriteria;
    float _startCounter;
    public float startCounter {
        get {
            return _startCounter;
        }
    }
    public int counter;

    public StateGroup_SendActionCounter(int v, ACT_TYPE t) {
        _condType = STATE_CLEAR_COND.SENDACTION;
        counter = v;
        typeCriteria = t;
        _startCounter = v;
    }

    // Returns false if counter has reached zero (condition is not met anymore)
    public void UpdateConditional(int v, ACT_TYPE t) {
        if(typeCriteria == t) {
            counter -= v;
        }
    }

    public bool Check() {
        return counter <= 0;
    }
}

[Serializable]
public class StateGroup_DMGCounter {
    STATE_CLEAR_COND _condType;
    public STATE_CLEAR_COND condType {
        get {
            return _condType;
        }
    }
    public ACT_DMG typeCriteria;
    float _startCounter;
    public float startCounter {
        get {
            return _startCounter;
        }
    }
    public float counter;

    public StateGroup_DMGCounter(float v, ACT_DMG t) {
        _condType = STATE_CLEAR_COND.DMGTHRESHOLD;
        counter = v;
        typeCriteria = t;
        _startCounter = v;
    }

    // Returns false if counter has reached zero (condition is not met anymore)
    public void UpdateConditional(float v, ACT_DMG t) {
        if(typeCriteria == t) {
            counter -= v;
        }
    }

    public bool Check() {
        return counter < 0;
    }
}


// Many Conditionals in one group...
// List of them? Seperate lists, yeah.
[Serializable]
public class StateGroupLogic : IEquatable<StateGroupLogic> {

    // TODO: Add group name to help with comparisons
    string _groupName = "";
    public string groupName {
        get {
            return _groupName;
        }
    }

    CharStateSO _groupSO;
    public CharStateSO groupSO {
        get {
            return _groupSO;
        }
    }

    // More than one timer doesnt make sense... at this point
    public List<StateGroup_Timer> timers;
    public List<StateGroup_RecieveActionCounter> recActionCounters;
    public List<StateGroup_SendActionCounter> sendActionCounters;
    public List<StateGroup_DMGCounter> dmgCounters;
    
    public StateGroupLogic() {
        timers = new List<StateGroup_Timer>();
        recActionCounters = new List<StateGroup_RecieveActionCounter>();
        sendActionCounters = new List<StateGroup_SendActionCounter>();
        dmgCounters = new List<StateGroup_DMGCounter>();
    }

    public StateGroupLogic(StateGroupLogic rhs) {
        _groupSO = rhs._groupSO;
        _groupName = rhs._groupName;

        timers = rhs.timers.ConvertAll(timer => new StateGroup_Timer(timer)); // Apparently ive done it wrong before, this is what is needed when deep copying a list
        recActionCounters = rhs.recActionCounters.ConvertAll(recActionCounter => new StateGroup_RecieveActionCounter(recActionCounter));
        sendActionCounters = new List<StateGroup_SendActionCounter>(rhs.sendActionCounters);
        dmgCounters = new List<StateGroup_DMGCounter>(rhs.dmgCounters);
    }

    public void InjectGroupSO(CharStateSO s) {
        if(_groupSO == null) {
            _groupSO = s;
            _groupName = s.name;
        }
            
    } 

    // if false a conditional has past and implies deletion
    // false implies a condition has expired
    public bool Check() {
        string log = "SGLogic Check(), result is true\n";

        bool result = true;
        if(!CheckTimers(ref log)) {
            result = false;
            log += "!CheckTimers is true, result false\n";
        }
        if(!CheckDMGCounters()) {
            result = false;
            log += "!CheckDMGCounters is true, result false\n";
        }
        if(!CheckRecActions()) {
            result = false;
            log += "!CheckRecActions is true, result false\n";
        }
        if(!CheckSendActions()) {
            result = false;
            log += "!CheckSendActions is true, result false\n";
        }
        Debug.Log(log);
        return result;
    }

    // TODO: Add CheckConditionals Method. If one is false, requirements are not meant and Group should be removed.
    // If any is false => remove
    /*
        Cleans any element that reaches count zero.
    */
    public void UpdateTimers(float dt) {
        if(timers.Count == 0) {
            return;
        }

        for (int i = 0; i < timers.Count; i++) {
            var timer = timers[i];
            timer.UpdateConditional(dt);
        }
        
    }

    public void RemoveStaleTimers() {
        timers.RemoveAll((StateGroup_Timer timer) => {
            return !timer.Check();
        });
    }
    // if false, a timer condition has failed
    public bool CheckTimers(ref string log) {
        for (int i = 0; i < timers.Count; i++) {
            
            var timer = timers[i];
            if(!timer.Check()) {
                log += "!timer " + i + "check is true, returning false";
                return false;
            }
        }
        return true;
    }

    public void UpdateDMGCounters(float v, ACT_DMG t) {
        if(dmgCounters.Count == 0) {
            return;
        }

        for (int i = 0; i < dmgCounters.Count; i++) {
            var dmgCounter = dmgCounters[i];
            dmgCounter.UpdateConditional(v, t);
        }
    }

    public void RemoveStaleDMGCounters() {
        dmgCounters.RemoveAll((StateGroup_DMGCounter counter) => {
            return !counter.Check();
        });
    }

    public bool CheckDMGCounters() {
        for (int i = 0; i < dmgCounters.Count; i++) {
            var dmgCounter = dmgCounters[i];
            if(!dmgCounter.Check()) {
                return false;
            }
        }
        return true;
    }

    public void UpdateRecActions(int c, ACT_TYPE t) {
        if(recActionCounters.Count == 0) {
            return;
        }

        for (int i = 0; i < recActionCounters.Count; i++) {
            var recActionCounter = recActionCounters[i];
            recActionCounter.UpdateConditional(c, t);
        }
    }

    public void RemoveStaleRecActionCounters() {
        recActionCounters.RemoveAll((StateGroup_RecieveActionCounter counter) => {
            return !counter.Check();
        });
    }
    public bool CheckRecActions() {
        for (int i = 0; i < recActionCounters.Count; i++) {
            var recActionCounter = recActionCounters[i];
            if(!recActionCounter.Check()) {
                return false;
            }
        }
        return true;
    }
    public void UpdateSendActions(int c, ACT_TYPE t) {
        if(sendActionCounters.Count == 0) {
            return;
        }

        for (int i = 0; i < sendActionCounters.Count; i++) {
            var sendActionCounter = sendActionCounters[i];
            sendActionCounter.UpdateConditional(c, t);
        }
    }
    public void RemoveStaleSendActionCounters() {
        sendActionCounters.RemoveAll((StateGroup_SendActionCounter counter) => {
            return !counter.Check();
        });
    }
    public bool CheckSendActions() {
        for (int i = 0; i < sendActionCounters.Count; i++) {
            var sendActionCounter = sendActionCounters[i];
            if(!sendActionCounter.Check()) {
                return false;
            }
        }
        return true;
    }   
    public bool HardEquals(StateGroupLogic rhs) {
        // Name and SO should be in essence an equivalent comparison
        if(_groupSO != rhs._groupSO) {
            return false;
        }

        int count = timers.Count;
        // Timers are equal?
        if(count != rhs.timers.Count) {
            return false;
        } else {
            for(int i = 0; i < count; i++) {
                var e_lhs = timers[i];
                var e_rhs = rhs.timers[i];

                if(e_lhs.counter != e_rhs.counter) {
                    return false;
                }
            }
        }
        
        // Dmg counters are equal?
        count = dmgCounters.Count;
        if(count != rhs.dmgCounters.Count) {
            return false;
        } else {
            for(int i = 0; i < count; i++) {
                var e_lhs = dmgCounters[i];
                var e_rhs = rhs.dmgCounters[i];

                if(e_lhs.typeCriteria != e_rhs.typeCriteria) {
                    return false;
                }
                if(e_lhs.counter != e_rhs.counter) {
                    return false;
                }
            }
        }

        // Send Action counters are equal?
        count = sendActionCounters.Count;
        if(count != rhs.sendActionCounters.Count) {
            return false;
        } else {
            for(int i = 0; i < count; i++) {
                var e_lhs = sendActionCounters[i];
                var e_rhs = rhs.sendActionCounters[i];

                if(e_lhs.typeCriteria != e_rhs.typeCriteria) {
                    return false;
                }
                if(e_lhs.counter != e_rhs.counter) {
                    return false;
                }
            }
        }

        // Recieve Action counters are equal?
        count = recActionCounters.Count;
        if(count != rhs.recActionCounters.Count) {
            return false;
        } else {
            for(int i = 0; i < count; i++) {
                var e_lhs = recActionCounters[i];
                var e_rhs = rhs.recActionCounters[i];

                if(e_lhs.typeCriteria != e_rhs.typeCriteria) {
                    return false;
                }
                if(e_lhs.counter != e_rhs.counter) {
                    return false;
                }
            }
        }

        return true;
    }

    public bool SoftEquals(StateGroupLogic rhs) {
        if(_groupName != rhs._groupName) {
            return false;
        }
        return true;
    }

    public bool Equals(StateGroupLogic rhs) {
        return HardEquals(rhs);
    }

}

