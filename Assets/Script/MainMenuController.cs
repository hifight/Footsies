using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Footsies
{

    public class MainMenuController : StandaloneInputModule
    {
        private GameObject lastSelectedGameObject;
        
        void Update()
        {
            // Remember the last selected object, so that when user click empty space and gameobject is unselected
            // it will select the last object automatically.
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
            }
            else
            {
                lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            }
        }
        
        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            ProcessMouseEvent();
        }
    }

}