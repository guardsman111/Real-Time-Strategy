using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleModeView : MonoBehaviour
{
    [SerializeField] private UnitManager manager;

    public PageView DeploymentMenu;
    public Button DeploymentButton;

    private List<GameObject> selection = new List<GameObject>();

    [SerializeField] private DeploymentPoint mouseFollower;

    [SerializeField] private List<Transform> spawnLocations;

    private void Start()
    {
        manager = FindObjectOfType<UnitManager>();
    }

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

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            if(selection.Count > 0)
            {
                if(EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 1000))
                {
                    if(hit.collider.tag == "Terrain")
                    {
                        SpawnNewUnits(hit.point);
                        DestroyGhosts();
                    }
                }
            }
        }
    }

    private void SpawnNewUnits(Vector3 point)
    {
        GameObject unitPrefab = selection[0].GetComponent<GhostUnit>().UnitPrefab;
        Transform spawnPosition = null;

        if (spawnLocations.Count > 0)
        {
            float distance = -1;
            foreach(Transform vec in spawnLocations)
            {
                float distance2 = Vector3.Distance(point, vec.position);
                if (distance == -1)
                {
                    spawnPosition = vec;
                    distance = distance2;
                    continue;
                }

                if(distance2 < distance)
                {
                    spawnPosition = vec;
                    distance = distance2;
                }
            }
        }


        for(int i = 0; i < selection.Count; i++)
        {
            GameObject newGO = Instantiate(unitPrefab, spawnPosition.position, spawnPosition.rotation, null);
            UnitObject unit = newGO.GetComponent<UnitObject>();
            newGO.transform.position += -newGO.transform.forward * (10 * i);
            unit.Initialize(manager);
            unit.SetTargetLocation(point);

            manager.SpawnPlayerNewUnit(unit);
        }
    }
}
