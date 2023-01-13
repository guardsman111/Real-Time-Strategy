using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoMissile : AmmoPiece
{
    [SerializeField] private int loseLockChance;

    [SerializeField] private float lockedLossTime;

    float randomX, randomY, randomZ;

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

            float lossTime = Vector3.Distance(nFiredPoint, nTarget.transform.position) / speed;

            lockedLossTime = Random.Range(0, lossTime);

            Debug.Log($"Locked loss time is {lockedLossTime} seconds after launch, with max range of {maxTimer}");
        }

        maxTimer = range / speed;

        StartCoroutine(GenerateRandomDirection());
    }

    private new void Update()
    {
        if (willHit == true && target != null)
        {
            base.Update();
            return;
        }

        if ((Time.realtimeSinceStartup - startTimer) > maxTimer)
        {
            Invoke("KillAmmo", 0.0f);
        }

        if (Time.realtimeSinceStartup - startTimer < lockedLossTime)
        {
            base.Update();
            return;
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            Invoke("KillAmmo", 0.25f);
            GetComponent<MeshRenderer>().enabled = false;
            spent = true;

            if (hit.collider.tag == "Collider")
            {
                hit.collider.GetComponent<UnitCollider>().Unit.RegisterHit(this, hit);
            }
        }

        Vector3 direction = new Vector3(randomX, randomY, randomZ);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manouverability * Time.deltaTime);

        transform.position += (transform.forward * speed * Time.deltaTime);
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

    private IEnumerator GenerateRandomDirection()
    {
        while (spent == false)
        {
            yield return new WaitForSeconds(0.5f);

            randomX = Random.Range(-360, 360);
            randomY = Random.Range(-360, 360);
            randomZ = Random.Range(-360, 360);
        }
    }

    public override void KillAmmo()
    {
        StopCoroutine(GenerateRandomDirection());

        base.KillAmmo();
    }
}
