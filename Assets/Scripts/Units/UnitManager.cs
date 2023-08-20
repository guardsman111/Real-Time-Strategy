using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private Dictionary<int,UnitObject> playerUnits = new Dictionary<int, UnitObject>();
    public Dictionary<int, UnitObject> PlayerUnits
    {
        get => playerUnits;
        private set {  value = playerUnits; }
    }

    [SerializeField] private Dictionary<int, UnitObject> hostileUnits = new Dictionary<int, UnitObject>();
    public Dictionary<int, UnitObject> HostileUnits
    {
        get => hostileUnits;
        private set { value = hostileUnits; }
    }

    [SerializeField] private Dictionary<int, UnitObject> playerVisibleUnits = new Dictionary<int, UnitObject>();
    public Dictionary<int, UnitObject> PlayerVisibleUnits
    {
        get => playerVisibleUnits;
        private set { value = playerVisibleUnits; }
    }

    [SerializeField] private Dictionary<int, UnitObject> hostileVisibleUnits = new Dictionary<int, UnitObject>();
    public Dictionary<int, UnitObject> HostileVisibleUnits
    {
        get => hostileVisibleUnits;
        private set { value = hostileVisibleUnits; }
    }

    private GameObject particleManagerPrefab;
    public GameObject ParticleManagerPrefab { get => particleManagerPrefab; }

    [SerializeField] private LayerMask hostileLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask terrainLayer;

    public void AddUnit(UnitObject unit, bool isPlayer)
    {
        if (isPlayer)
        {
            PlayerUnits.Add(unit.ID, unit);

            return;
        }

        HostileUnits.Add(unit.ID, unit);

        unit.DisableMesh();
    }

    public void RemoveUnit(UnitObject unit, bool isPlayer)
    {
        if (isPlayer)
        {
            PlayerUnits.Remove(unit.ID);
            PlayerVisibleUnits.Remove(unit.ID);

            return;
        }

        HostileUnits.Remove(unit.ID);
        HostileVisibleUnits.Remove(unit.ID);
    }

    public void MakeUnitVisible(UnitObject unit, bool isPlayer)
    {
        if (isPlayer)
        {
            if (PlayerVisibleUnits.ContainsKey(unit.ID) == false)
            {
                PlayerVisibleUnits.Add(unit.ID, unit);
            }

            return;
        }

        if (HostileVisibleUnits.ContainsKey(unit.ID) == false)
        {
            HostileVisibleUnits.Add(unit.ID, unit);
        }

        unit.EnableMesh();
    }

    public void MakeUnitNotVisible(UnitObject unit, bool isPlayer)
    {
        if (isPlayer)
        {
            PlayerVisibleUnits.Remove(unit.ID);

            foreach(UnitObject spotter in unit.EnemiesSpotting)
            {
                spotter.SpottedEnemyNotSpottedAnymore(unit);
            }

            return;
        }

        foreach (UnitObject spotter in unit.EnemiesSpotting)
        {
            spotter.SpottedEnemyNotSpottedAnymore(unit);
        }

        HostileVisibleUnits.Remove(unit.ID);

        unit.DisableMesh();
    }

    public UnitObject CheckClosestVisibleEnemy(UnitObject viewer, bool isPlayer)
    {
        int closestID = 0;

        Dictionary<int, UnitObject> enemyDict = null;

        // If it's a player's unit check the enemy's list
        if (isPlayer)
        {
            if (HostileVisibleUnits.Count != 0)
            {
                enemyDict = HostileVisibleUnits;
            }
        }
        else
        {
            // If it's an enemy unit check the player's list
            if (PlayerVisibleUnits.Count != 0)
            {
                enemyDict = PlayerVisibleUnits;
            }
        }

        if(enemyDict == null)
        {
            //Debug.Log("No visible units");
            return null;
        }

        closestID = FindClosestUnit(viewer, enemyDict);

        if (closestID == -1)
        {
            //Debug.LogError("Error finding visible enemy - returned no enemy");
            return null;
        }

        return enemyDict[closestID];
    }

    public int FindClosestUnit(UnitObject viewer, Dictionary<int, UnitObject> enemyDict)
    {
        int closestID = -1;

        float shortestDistance = 0;
        foreach (UnitObject enemy in enemyDict.Values)
        {
            RaycastHit[] hits;

            Vector3 direction = enemy.transform.position - viewer.transform.position;

            LayerMask mask = playerLayer;
            if (viewer.IsPlayer)
            {
                mask = hostileLayer;
            }

            hits = Physics.RaycastAll(viewer.transform.position, direction, viewer.Stats.opticsRange);
            float targetDistance = Vector3.Distance(enemy.transform.position, viewer.transform.position);

            if (hits.Length > 0)
            { 
                hits = ArrangeHitsByDistance(hits);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer == viewer.gameObject.layer)
                    {
                        continue;
                    }
                    if (hit.collider.tag == "Collider")
                    {
                        Debug.DrawLine(viewer.transform.position, hit.point, Color.red, 1.0f);
                        if (hit.collider.GetComponent<UnitCollider>().Unit == enemy)
                        {
                            float distance = Vector3.Distance(viewer.transform.position, enemy.transform.position);
                            if (distance < shortestDistance || shortestDistance == 0)
                            {
                                shortestDistance = distance;
                                closestID = enemy.ID;
                                break;
                            }
                        }
                    }
                    if (hit.collider.tag == "Unit")
                    {
                        Debug.DrawLine(viewer.transform.position, hit.point, Color.red, 1.0f);
                        if (hit.collider.GetComponent<UnitObject>() == enemy)
                        {
                            float distance = Vector3.Distance(viewer.transform.position, enemy.transform.position);
                            if (distance < shortestDistance || shortestDistance == 0)
                            {
                                shortestDistance = distance;
                                closestID = enemy.ID;
                                break;
                            }
                        }
                    }
                    if (hit.collider.tag == "Terrain")
                    {
                        Debug.DrawLine(viewer.transform.position, hit.point, Color.yellow, 1.0f);
                        break;
                    }
                }
            }

        }
        return closestID;
    }

    private RaycastHit[] ArrangeHitsByDistance(RaycastHit[] hits)
    {
        RaycastHit[] newHits = hits.ToList<RaycastHit>().OrderBy(x => x.distance).ToArray();

        return newHits;
    }

    internal void SpawnPlayerNewUnit(UnitObject unit)
    {
        unit.SetPlayer(true);

        AddUnit(unit, true);
    }
}
