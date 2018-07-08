using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
    /// <summary>
    /// Set guard sprite numbers with guard health from each player
    /// </summary>
    public class GuardPanelController : MonoBehaviour
    {

        [SerializeField]
        private GameObject _battleCoreGameObject;
        
        [SerializeField]
        private bool isPlayerOne;

        [SerializeField]
        private GameObject[] guardImageObjects;

        #region private field

        private BattleCore battleCore;

        private int currentGuardHealth = 0;

        #endregion

        void Awake()
        {
            if (_battleCoreGameObject != null)
            {
                battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
            }
            
            currentGuardHealth = 0;
            UpdateGuardHealthImages();
        }

        // Update is called once per frame
        void Update()
        {
            if (currentGuardHealth != getGuardHealth())
            {
                currentGuardHealth = getGuardHealth();
                UpdateGuardHealthImages();
            }

        }

        private int getGuardHealth()
        {
            if (battleCore == null)
                return 0;

            if (isPlayerOne)
                return battleCore.fighter1.guardHealth;
            else
                return battleCore.fighter2.guardHealth;
        }

        private void UpdateGuardHealthImages()
        {
            for (int i = 0; i < guardImageObjects.Length; i++)
            {
                if (i <= (int)currentGuardHealth - 1)
                {
                    guardImageObjects[i].SetActive(true);
                }
                else
                {
                    guardImageObjects[i].SetActive(false);
                }
            }
        }
    }
}