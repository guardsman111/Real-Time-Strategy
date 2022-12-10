using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVehicle : UnitObject
{
    public List<ParticleSystem> Fires;

    // Can explode if it's a vehicle
    public override void TakeDamage(int damage)
    {
        // Check if the unit explodes, if not, do normal health check
        if (damage / Stats.toughness >= 10)
        {
            Stats.currentHealth = 0;
            Explode();
            return;
        }

        base.TakeDamage(damage);
    }

    private void Explode()
    {
        if (isDead)
        {
            return;
        }

        Vector3 explosionForce;

        foreach (UnitWeapon weapon in weapons)
        {
            if (weapon.Stats.removable == true)
            {
                weapon.transform.parent = null;
                explosionForce = new Vector3(Random.Range(0, 50), 1 * Random.Range(300, 500), Random.Range(0, 50));
                weapon.gameObject.AddComponent<Rigidbody>().AddForceAtPosition(explosionForce, weapon.transform.position);
            }
        }

        foreach(ParticleSystem system in Fires)
        {
            system.gameObject.SetActive(true);
            system.Play();
        }

        Die();
    }
}
