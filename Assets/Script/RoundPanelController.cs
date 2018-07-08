using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
    /// <summary>
    /// Set round sprite with number of win from each player
    /// </summary>
    public class RoundPanelController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _battleCoreGameObject;

        [SerializeField]
        private Sprite spriteEmpty;

        [SerializeField]
        private Sprite spriteWon;

        [SerializeField]
        private bool isPlayerOne;

        [SerializeField]
        private GameObject[] roundWonImageObjects;

        #region private field

        private BattleCore battleCore;
        private Image[] roundWonImages;

        private uint currentRoundWon = 0;

        #endregion
        
        void Awake ()
        {
            if (_battleCoreGameObject != null)
            {
                battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
            }

            roundWonImages = new Image[roundWonImageObjects.Length];
            for(int i = 0; i < roundWonImageObjects.Length; i++)
            {
                roundWonImages[i] = roundWonImageObjects[i].GetComponent<Image>();
            }

            currentRoundWon = 0;
            UpdateRoundWonImages();
        }
	
	    // Update is called once per frame
	    void Update ()
        {
		    if(currentRoundWon != getRoundWon())
            {
                currentRoundWon = getRoundWon();
                UpdateRoundWonImages();
            }

	    }

        private uint getRoundWon()
        {
            if (battleCore == null)
                return 0;

            if (isPlayerOne)
                return battleCore.fighter1RoundWon;
            else
                return battleCore.fighter2RoundWon;
        }

        private void UpdateRoundWonImages()
        {
            for(int i = 0; i < roundWonImages.Length; i++)
            {
                if(i <= (int)currentRoundWon - 1)
                {
                    roundWonImages[i].sprite = spriteWon;
                }
                else
                {
                    roundWonImages[i].sprite = spriteEmpty;
                }
            }
        }
    }

}
