using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDS.CustomizedData
{
    [CreateAssetMenu(fileName = "Character",menuName = "ScriptableObjects/SDS/Character")]
    public class Character : ScriptableObject
    {
        public string CharacterName;

        public string Idle;
        public string Happy;
        public string Sad;
        public string Angry;
        public string Surprised;
        public string Puzzled;
    }
}