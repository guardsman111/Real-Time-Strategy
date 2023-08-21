using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitVehicle : UnitObject
{
    public List<ParticleSystem> Fires;
    [SerializeField] private bool reverseSelected = false;
    [SerializeField] private bool isReversing = false;
    public bool cantReverse { get; protected set; } = false;

    protected override void Update()
    {
        base.Update();
        if(isReversing == true)
        {
            FaceAwayFromDestination();


            return;
        }
    }

    private void FaceAwayFromDestination()
    {
        Vector3 targetPos = pather.destination;
        targetPos.y = transform.position.y;
        Vector3 targetDirection = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(-targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.traverseSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);
    }

    private void CheckCompletedPath()
    {
        if(pather.reachedDestination == true)
        {
            Reverse(false);
        }
    }

    // Can explode if it's a vehicle
    public override void TakeDamage(int damage)
    {
        // Check if the unit explodes, if not, do normal health check
        if (damage >= Stats.currentHealth || damage >= Stats.toughness * 3)
        {
            int rand = Random.Range(0, 100);
            if (rand >= 25)
            {
                return;
            }
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

        foreach(ParticleSystem fire in Fires)
        {
            fire.gameObject.SetActive(true);
            fire.Play();
        }

        GameObject explosion = Instantiate(manager.ExplosionPrefab, transform.position, Quaternion.identity);
        ParticleSystem system = explosion.GetComponent<ParticleSystem>();
        foreach(ParticleSystem subSystem in system.GetComponentsInChildren<ParticleSystem>())
        {
            subSystem.Play();
        }

        Die();
    }

    public override void SetTargetLocation(Vector3 target)
    {
        base.SetTargetLocation(target);
        Reverse(reverseSelected);
        ReadyReverse(false);
    }

    public void ReadyReverse(bool isReady)
    {
        reverseSelected = isReady;
    }

    public void Reverse(bool active)
    {
        pather.enableRotation = !active;
        isReversing = active;
        if(active == true)
        {
            pather.maxSpeed = Stats.driveSpeed / 2;
            InvokeRepeating("CheckCompletePath", 0.2f, 0.2f);
            return;
        }

        pather.maxSpeed = Stats.driveSpeed;
    }
}
