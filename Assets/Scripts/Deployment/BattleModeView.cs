using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleModeView : MonoBehaviour
{
    public PageView DeploymentMenu;
    public Button DeploymentButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleDeploymentMenu()
    {
        if(DeploymentMenu.gameObject.activeSelf == true)
        {
            DeploymentMenu.SetPageState(false);
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        DeploymentMenu.SetPageState(true);
    }

    public void SetDeploymentMenuState(bool state)
    {
        DeploymentMenu.SetPageState(state);
    }
}
