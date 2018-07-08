using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
    [System.Serializable]
    public class AttackData
    {
        public int attackID;
        public string attackName;

        public int damageActionID;
        public int guardActionID;

        public int numberOfHit;

        public int vitalHealthDamage;
        public int guardHealthDamage;

        public int hitStunFrame;
        public int guardStunFrame;
        public int guardBreakStunFrame;
    }
}