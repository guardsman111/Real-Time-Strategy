using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoIlluminatorMissile : AmmoPiece
{
    float randomX, randomY, randomZ;

    public override void Fired(int nRange, int nSpeed, Vector3 nFiredPoint, UnitObject nShooter, UnitWeapon nWeapon, UnitObject nTarget)
    {
        base.Fired(nRange, nSpeed, nFiredPoint, nShooter, nWeapon, nTarget);

        maxTimer = range / speed;
        willHit = true;

        StartCoroutine(GenerateRandomDirection());
    }

    private new void Update()
    {
        if (willHit == true)
        {
            base.Update();
            return;
        }

        if ((Time.realtimeSinceStartup - startTimer) > maxTimer)
        {
            Invoke("KillAmmo", 0.0f);
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

        Vector3 direction = (new Vector3(randomX, randomY, randomZ) - transform.position);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manouverability * Time.deltaTime);

        transform.position += (transform.forward * speed * Time.deltaTime);
    }

    private IEnumerator GenerateRandomDirection()
    {
        while (spent == false)
        {
            yield return new WaitForSeconds(0.2f);

            randomX = Random.Range(-360, 360);
            Debug.Log(randomX);
            randomY = Random.Range(-360, 360);
            Debug.Log(randomY);
            randomZ = Random.Range(-360, 360);
            Debug.Log(randomZ);
        }
    }
}
