using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Constant
{
    public class Player : ScriptableObject {
        public PartySO PlayerParty;
        public PartySO EnemyParty;

        public CharacterDataSO FirstCharacter;
        public CharacterDataSO SecondCharacter;
        public CharacterDataSO ThirdCharacter;
        public CharacterDataSO FourthCharacter;
    }
}
