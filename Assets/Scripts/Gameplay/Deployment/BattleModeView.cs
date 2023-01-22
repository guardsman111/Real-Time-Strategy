using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleModeView : MonoBehaviour
{
    public PageView DeploymentMenu;
    public Button DeploymentButton;

    private List<GameObject> selection = new List<GameObject>();

    [SerializeField] private DeploymentPoint mouseFollower;

    public void ToggleDeploymentMenu()
    {
        if (DeploymentMenu.gameObject.activeSelf == true)
        {
            DeploymentMenu.SetPageState(false);
            DestroyGhosts();
            EventSystem.current.SetSelectedGameObject(null);

            return;
        }

        DeploymentMenu.SetPageState(true);

        mouseFollower.ToggleActive(true);
    }

    public void SetDeploymentMenuState(bool state)
    {
        DeploymentMenu.SetPageState(state);
        mouseFollower.ToggleActive(state);

        if (state == false)
        {
            DestroyGhosts();
            return;
        }

    }

    public void SpawnGhost(GameObject prefabGhost)
    {
        GameObject newGhost;

        if (selection.Count > 0)
        {
            if (selection[0].name.Contains(prefabGhost.name) == false)
            {
                DestroyGhosts();
                newGhost = Instantiate(prefabGhost, mouseFollower.transform);
                newGhost.transform.localPosition = new Vector3(5 * selection.Count, 0, 0);
                selection.Add(newGhost);
                return;
            }

            if (selection.Count < 4)
            {
                newGhost = Instantiate(prefabGhost, mouseFollower.transform);
                newGhost.transform.localPosition = new Vector3(5 * selection.Count, 0, 0);
                selection.Add(newGhost);
                return;
            }

            return;
        }

        newGhost = Instantiate(prefabGhost, mouseFollower.transform);
        newGhost.transform.localPosition = new Vector3(5 * selection.Count, 0, 0);
        selection.Add(newGhost);
    }

    private void DestroyGhosts()
    {
        foreach (GameObject ghost in selection)
        {
            Destroy(ghost);
        }

        selection.Clear();
    }
}
