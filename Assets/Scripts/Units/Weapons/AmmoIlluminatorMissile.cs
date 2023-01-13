using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoIlluminatorMissile : AmmoPiece
{
    public override void Fired(int nRange, int nSpeed, Vector3 nFiredPoint, UnitObject nShooter, UnitWeapon nWeapon, UnitObject nTarget)
    {
        base.Fired(nRange, nSpeed, nFiredPoint, nShooter, nWeapon, nTarget);

        maxTimer = range / speed;
        willHit = true;
    }

    private new void Update()
    {
        if (willHit == true)
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

        float randomX = Random.Range(0, 100);
        float randomY = Random.Range(0, 100);
        float randomZ = Random.Range(0, 100);

        Vector3 direction = (new Vector3(randomX, randomY, randomZ) - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manouverability * Time.deltaTime);

        transform.position += (transform.forward * speed * Time.deltaTime);
    }
}
