using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMissile : AmmoPiece
{
    [SerializeField] private int loseLockChance;

    [SerializeField] private float lockedLossTime;

    private void Start()
    {
    }

    public override void Fired(int nRange, int nSpeed, Vector3 nFiredPoint, UnitObject nShooter, UnitWeapon nWeapon, UnitObject nTarget)
    {
        base.Fired(nRange, nSpeed, nFiredPoint, nShooter, nWeapon, nTarget);

        willHit = CheckLoseLock();

        if (willHit == false)
        {
            startTimer = Time.realtimeSinceStartup;

            maxTimer = Vector3.Distance(nFiredPoint, nTarget.transform.position) / speed;

            lockedLossTime = Random.Range(0, maxTimer);

            Debug.Log($"Locked loss time is {lockedLossTime} seconds after launch, with max range of {maxTimer}");
        }
    }

    private new void Update()
    {
        if (willHit == true)
        {
            base.Update();
            return;
        }

        if(Time.realtimeSinceStartup - startTimer < lockedLossTime)
        {
            base.Update();
            return;
        }

        if (Time.realtimeSinceStartup - startTimer > maxTimer)
        {
            Invoke("KillAmmo", 0.0f);
        }

        float randomX = Random.Range(0, 100);
        float randomY = Random.Range(0, 100);
        float randomZ = Random.Range(0, 100);

        Vector3 direction = (new Vector3(randomX, randomY, randomZ) - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manouverability * Time.deltaTime);

        transform.position += (transform.forward * speed * Time.deltaTime);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            Invoke("KillAmmo", 0.25f);
            GetComponent<MeshRenderer>().enabled = false;
            speed = 0;
            spent = true;

            if (hit.collider.tag == "Collider")
            {
                hit.collider.GetComponent<UnitCollider>().Unit.RegisterHit(this, hit);
            }
        }
    }

    private bool CheckLoseLock()
    {
        float random = Random.Range(0, 100);

        if(random < loseLockChance)
        {
            return false;
        }

        return true;
    }
}
