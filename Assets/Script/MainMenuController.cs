using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Footsies
{

    public class MainMenuController : MonoBehaviour
    {
        private GameObject lastSelectedGameObject;
        
        void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
            }
            else
            {
                lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

}