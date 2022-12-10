using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public static class Spotting 
{
    public static void FindVisibleEnemies(UnitObject spotter, Dictionary<int, UnitObject> units)
    {
        foreach (UnitObject unit in units.Values)
        {
            if (unit.CheckAlreadySpotted(spotter) == true)
            {
                continue;
            }

            float targetDistance = Vector3.Distance(unit.transform.position, spotter.transform.position);

            if (targetDistance < spotter.Stats.opticsRange)
            {
                RaycastHit[] hits;

                Vector3 direction = ((unit.transform.position + new Vector3(0, 0.5f, 0)) - (spotter.transform.position + new Vector3(0, 0.5f, 0))).normalized;

                //Debug.DrawRay((spotter.transform.position + new Vector3(0, 0.5f, 0)), direction * spotter.Stats.opticsRange, Color.green, 0.5f);

                hits = Physics.RaycastAll(spotter.transform.position, direction, spotter.Stats.opticsRange);

                hits = ArrangeHitsByDistance(hits);

                if (hits.Length > 0)
                {
                    foreach (RaycastHit hit in hits)
                    {
                        if (hit.collider.gameObject.layer == spotter.gameObject.layer)
                        {
                            continue;
                        }

                        if (hit.collider.tag == "Collider")
                        {
                            hit.collider.GetComponent<UnitCollider>().Unit.MakeVisible(spotter, spotter.IsPlayer);
                            break;
                        }
                        if (hit.collider.tag == "Unit")
                        {
                            hit.collider.GetComponent<UnitObject>().MakeVisible(spotter, spotter.IsPlayer);
                            break;
                        }
                        if(hit.collider.tag == "Terrain")
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public static bool CheckCanSeeSpotted(UnitObject spotter, UnitObject spottedUnit)
    {
        if (Vector3.Distance(spottedUnit.transform.position, spotter.transform.position) < spotter.Stats.opticsRange)
        {
            RaycastHit[] hits;

            Vector3 direction = (spottedUnit.transform.position - spotter.transform.position).normalized;

            //Debug.DrawRay((spotter.transform.position + new Vector3(0, 0.1f, 0)), direction * spotter.Stats.opticsRange, Color.green, 0.5f);

            hits = Physics.RaycastAll(spotter.transform.position, direction, spotter.Stats.opticsRange);

            hits = ArrangeHitsByDistance(hits);

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.layer == spotter.gameObject.layer)
                    {
                        continue;
                    }
                    if (hit.collider.tag == "Collider")
                    {
                        if (hit.collider.GetComponent<UnitCollider>().Unit == spottedUnit)
                        {
                            return true;
                        }
                    }
                    if (hit.collider.tag == "Unit")
                    {
                        if (hit.collider.GetComponent<UnitObject>() == spottedUnit)
                        {
                            return true;
                        }
                    }
                    if (hit.collider.tag == "Terrain")
                    {
                        break;
                    }
                }
            }
        }

        return false;
    }

    private static RaycastHit[] ArrangeHitsByDistance(RaycastHit[] hits)
    {
        RaycastHit[] newHits = hits.ToList<RaycastHit>().OrderBy(x => x.distance).ToArray();

        return newHits;
    }
}
