using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
    /// <summary>
    /// Fighter data. Contain status and motion, attack and action data
    /// </summary>
    [CreateAssetMenu]
    public class FighterData : ScriptableObject
    {
        public int startGuardHealth = 3;

        public float forwardMoveSpeed = 2.2f;
        public float backwardMoveSpeed = 1.8f;

        public int dashAllowFrame = 10;

        public int specialAttackHoldFrame = 60;

        public bool canCancelOnWhiff = false;
        
        [SerializeField]
        public Rect baseHurtBoxRect;

        [SerializeField]
        public Rect basePushBoxRect;

        [SerializeField]
        private ActionDataContainer actionDataContainer;

        [SerializeField]
        private AttackDataContainer attackDataContainer;

        [SerializeField]
        private MotionDataContainer motionDataContainer;

        public Dictionary<int, ActionData> actions { get { return _actions; } }
        private Dictionary<int, ActionData> _actions = new Dictionary<int, ActionData>();

        public Dictionary<int, AttackData> attackData { get { return _attackData; } }
        private Dictionary<int, AttackData> _attackData = new Dictionary<int, AttackData>();

        public Dictionary<int, MotionData> motionData { get { return _motionData; } }
        private Dictionary<int, MotionData> _motionData = new Dictionary<int, MotionData>();

        public void setupDictionary()
        {
            if(actionDataContainer == null)
            {
                Debug.LogError("ActionDataContainer is not set");
                return;
            }
            else if (attackDataContainer == null)
            {
                Debug.LogError("ActionDataContainer is not set");
                return;
            }

            _actions = new Dictionary<int, ActionData>();
            foreach (var action in actionDataContainer.actions)
            {
                _actions.Add(action.actionID, action);
            }

            _attackData = new Dictionary<int, AttackData>();
            foreach (var data in attackDataContainer.attackDataList)
            {
                _attackData.Add(data.attackID, data);
            }

            _motionData = new Dictionary<int, MotionData>();
            foreach (var data in motionDataContainer.motionDataList)
            {
                _motionData.Add(data.motionID, data);
            }
        }
    }
}