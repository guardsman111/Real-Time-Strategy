using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageView : MonoBehaviour
{
    public void SetPageState(bool state)
    {
        if (state != gameObject.activeSelf)
        {
            gameObject.SetActive(state);
        }
    }
}
