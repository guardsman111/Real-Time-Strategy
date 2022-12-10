using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private UnitManager manager;

    [SerializeField] private Dictionary<int, UnitObject> playerUnits;
    [SerializeField] private List<UnitObject> selectedUnits;

    [SerializeField] private LayerMask mask;

    public void Initialize(Dictionary<int, UnitObject> PlayerUnits, UnitManager Manager)
    {
        playerUnits = PlayerUnits;
        manager = Manager;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectedUnits.Clear();

            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out hit))
            {
                if(hit.collider.tag == "Collider")
                {
                    if (hit.collider.GetComponent<UnitCollider>().Unit.IsPlayer)
                    {
                        selectedUnits.Add(hit.collider.GetComponent<UnitCollider>().Unit);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, mask))
            {
                if (hit.collider.tag == "Collider")
                {
                    if (!hit.collider.GetComponent<UnitCollider>().Unit.IsPlayer)
                    {
                        foreach(UnitObject unit in selectedUnits)
                        {
                            unit.SetTargetEnemy(hit.collider.GetComponent<UnitCollider>().Unit);
                        }
                    }
                }
                if (hit.collider.tag == "Terrain")
                {
                    foreach (UnitObject unit in selectedUnits)
                    {
                        unit.SetTargetLocation(hit.point);
                    }
                }
            }
        }
    }
}
