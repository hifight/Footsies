using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
    public class UIEventAction : MonoBehaviour
    {
        public enum Action
        {
            LoadVsCPU,
            LoadVsPlayer,
            ExitGame,
            BGMToggle,
            SEToggle,
        }

        public Action action;

        private void Awake()
        {
            if(action == Action.BGMToggle)
            {
                var toggle = gameObject.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.isOn = SoundManager.Instance.isBGMOn;
                }
            }
        }

        public void InvokeAction()
        {
            switch(action)
            {
                case Action.LoadVsCPU:
                    LoadVsCPU();
                    break;
                case Action.LoadVsPlayer:
                    LoadVsPlayer();
                    break;
                case Action.ExitGame:
                    ExitGame();
                    break;
                case Action.BGMToggle:
                    toggleBGM();
                    break;
                case Action.SEToggle:
                    break;
            }
        }

        public void LoadVsCPU()
        {
            GameManager.Instance.LoadVsCPUScene();
        }

        public void LoadVsPlayer()
        {
            GameManager.Instance.LoadVsPlayerScene();
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void toggleBGM()
        {
            var isOn = SoundManager.Instance.toggleBGM();
            var toggle = gameObject.GetComponent<Toggle>();
            if(toggle != null)
            {
                toggle.isOn = isOn;
            }
        }
    }

}