using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimActionChannelSO", menuName = "ScriptableObjects/Channels/AnimAction")]
public class AnimActionChannelSO : ScriptableObject
{
    
    //AnimActionChannel can be useful yet
    //We need a Event from CharacterData to for AnimActionPort to start an attack
    //How can we do that while keeping CharData mostly oblivious to AnimActionPort
    //CData SHOULD NOT BE RELIANT ON ANIMACTIONPORT, this is shit code
    //But i sadly dont have time to write good code :/
    //Easy solution is

    public delegate void onAnimAttack();

    public void AA_Attack_start() {
        
    }
    
    public void AA_Attack_hit() {

    }

    public void AA_Attack_stop() {
        
    }

    
    




}
