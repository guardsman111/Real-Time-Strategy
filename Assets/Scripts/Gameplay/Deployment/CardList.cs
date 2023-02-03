using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardList : PageView
{
    public void TogglePageState()
    {
        if (gameObject.activeSelf == true)
        {
            SetPageState(false);
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        SetPageState(true);
    }
}
