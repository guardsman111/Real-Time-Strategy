using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class AmmoPiece : MonoBehaviour
{
    [Header("Ammo Stats")]
    [SerializeField] protected int range;
    [SerializeField] protected int speed;
    [SerializeField] protected int manouverability;
    [SerializeField] private int damage;
    [SerializeField] private float actualPenetration;
    [SerializeField] private int maxPenetration;
    [SerializeField] private bool kineticPenetrator;
    [SerializeField] private bool tracking;

    public int Damage { get => damage; }
    public float Penetration { get => actualPenetration; }

    private UnitObject shooter;
    private UnitWeapon weapon;
    protected UnitObject target;

    [SerializeField] private GameObject impactEffect;

    protected bool spent, willHit;
    protected float maxTimer, startTimer;

    public virtual void Fired(int nRange, int nSpeed, Vector3 nFiredPoint, UnitObject nShooter, UnitWeapon nWeapon, UnitObject nTarget)
    {
        range = nRange;
        speed = nSpeed;

        shooter = nShooter;
        weapon = nWeapon;
        target = nTarget;

        startTimer = Time.realtimeSinceStartup;

        maxTimer += range / speed;
    }

    public void Update()
    {
        if (spent == false)
        {
            if(tracking == true)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, manouverability * Time.deltaTime);
            }

            if ((Time.realtimeSinceStartup - startTimer) > maxTimer)
            {
                Invoke("KillAmmo",0.0f);
            }

            transform.position += (transform.forward * speed * Time.deltaTime);

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                GetComponent<MeshRenderer>().enabled = false;
                speed = 0;
                spent = true;

                actualPenetration = maxPenetration;

                if (kineticPenetrator == true)
                {
                    float distanceOfShot = Vector3.Distance(transform.position, shooter.transform.position);

                    actualPenetration = maxPenetration * (1 - Mathf.Clamp((distanceOfShot / range), 0, 0.5f));

                    //Debug.Log("penetration = " + actualPenetration + " distance of shot = " + distanceOfShot);
                }

                if (hit.collider.tag == "Collider")
                {
                    hit.collider.GetComponent<UnitCollider>().Unit.RegisterHit(this, hit);
                }
                else if (hit.collider.tag == "Unit")
                {
                    hit.collider.GetComponent<UnitObject>().RegisterHit(this, hit);
                }
                SpawnImpact(hit);
                KillAmmo();
            }
        }
    }

    public UnitObject GetTarget()
    {
        return target;
    }

    public void SetTarget(UnitObject newTarget)
    {
        target = newTarget;
    }

    public void SetWillHit(bool newWillHit)
    {
        willHit = newWillHit;
    }

    private void KillAmmo()
    {
        if(shooter.LimitAmmoUse == true)
        {
            weapon.RemoveShot();
        }

        if(weapon.LimitedAmmo == true)
        {
            weapon.RemoveShot();
        }

        Destroy(this.gameObject); // Maybe make this go physics and slowly lower speed til it hits the ground?
    }

    private void SpawnImpact(RaycastHit hitInfo)
    {
        GameObject impact = Instantiate(impactEffect, hitInfo.point, this.transform.rotation);
    }
}
