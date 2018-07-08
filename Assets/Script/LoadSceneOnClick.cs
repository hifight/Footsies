using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{
    public class LoadSceneOnClick : MonoBehaviour
    {
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
    }

}