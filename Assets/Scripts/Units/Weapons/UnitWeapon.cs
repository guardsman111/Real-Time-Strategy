using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWeapon : MonoBehaviour
{
    [SerializeField] protected UnitObject unit;

    [SerializeField] private WeaponStats stats;
    public WeaponStats Stats { get => stats; }

    [Header("Weapon Pieces")]
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private Transform gunHome;
    [SerializeField] private Transform muzzel;
    [SerializeField] private ParticleSystem flash;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] protected AmmoPiece firedAmmo;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 previousTargetPosition;
    [SerializeField] private Transform predictedTargetPosition;
    protected UnitObject targetUnit;
    public UnitObject TargetUnit { get => targetUnit; }
    private bool isShooting, isFiring;

    [SerializeField] protected int ammoCount;
    [SerializeField] protected int ammoReadyRack;
    [SerializeField] protected int ammoHalfReadyRack;

    [SerializeField] private bool limitedAmmo = true;
    public bool LimitedAmmo { get => limitedAmmo; }
    [SerializeField] protected bool isLoading;
    private Coroutine stockRacks;

    protected virtual void Start()
    {
        ammoReadyRack = stats.readyRack;
        ammoHalfReadyRack = stats.halfReadyRack;
        ammoCount = stats.ammoTotal - (ammoReadyRack + ammoHalfReadyRack);
    }

    protected void Update()
    {
        AttemptShot();
    }

    public virtual void AttemptShot()
    {
        if (firedAmmo == null)
        {
            if (target != null)
            {
                if (Stats.isTurreted)
                {
                    RotateTurret();
                }

                RotateGun();

                Vector3 targetDirection = previousTargetPosition - transform.position;
                float angle1 = Vector3.Angle(targetDirection, transform.forward);

                if (angle1 <= 45)
                {
                    if (Vector3.Distance(transform.position, target.position) < Stats.range && isShooting == false && unit.isDead == false)
                    {
                        StartShooting();
                        return;
                    }
                    if (Vector3.Distance(transform.position, target.position) > Stats.range)
                    {
                        if (isFiring)
                        {
                            StopCoroutine(TryFire());
                        }
                        isShooting = false;
                    }
                    return;
                }

                if (isFiring)
                {
                    StopCoroutine(TryFire());
                }
                isShooting = false;
                return;
            }

            if (isFiring)
            {
                StopCoroutine(TryFire());
            }

            Quaternion targetRotation = Quaternion.LookRotation(unit.transform.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);
            gunHome.rotation = Quaternion.RotateTowards(gunHome.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
            gunHome.localEulerAngles = new Vector3(gunHome.localRotation.eulerAngles.x, 0, 0);
        }
    }

    private void RotateTurret()
    {
        Vector3 predictedPosition = CalculatePredictedPosition();
        predictedPosition.y = transform.position.y;

        if (predictedPosition == Vector3.zero)
        {
            previousTargetPosition = target.position;
            return;
        }

        Vector3 targetDirection = predictedPosition - transform.position;
        targetDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
        //transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);

        previousTargetPosition = target.position;
        if (predictedTargetPosition != null)
        {
            predictedTargetPosition.position = predictedPosition;
        }
    }

    private Vector3 CalculatePredictedPosition()
    {
        if(previousTargetPosition == null)
        {
            previousTargetPosition = target.position;
            return target.position;
        }

        Vector3 targetVelocity = (target.position - previousTargetPosition) / Time.deltaTime;
        previousTargetPosition = target.position;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float timeToReachTarget = distanceToTarget / stats.ammoSpeed;

        Vector3 futurePosition = target.position + targetVelocity * timeToReachTarget;

        return futurePosition;
    }

    private void RotateGun()
    {
        Vector3 direction = (target.position - new Vector3(0, 1, 0)) - transform.position;
        direction.y = 0; // Remove vertical component to ensure rotation only on the Y-axis.

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion currentRotation = gunHome.rotation;
        currentRotation.x = 0; // Remove any existing rotation on the X-axis.
        currentRotation.z = 0; // Remove any existing rotation on the Z-axis.

        transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, stats.rotationSpeed * Time.deltaTime);
    }

    public void ChangeTarget(UnitObject nTarget)
    {
        if (unit.isDead == false)
        {
            targetUnit = nTarget;

            if (targetUnit != null)
            {
                target = nTarget.transform;

                if(stockRacks != null)
                    StopCoroutine(stockRacks);

                return;
            }

            Debug.Log("target lost");
            target = null;
            stockRacks = StartCoroutine(StockRacks());
        }
    }

    public void StartShooting()
    {
        if (isShooting == false)
        {
            if (isFiring == false)
            {
                StartCoroutine(TryFire());
            }
            isShooting = true;
        }
    }

    protected IEnumerator TryFire()
    {
        isFiring = true;

        if(targetUnit == null || unit.isDead) 
        {
            yield break;
        }

        yield return new WaitForSeconds(stats.aimTime);

        if (isLoading == false)
        {
            if (isShooting == true)
            {
                GameObject shot = Instantiate(ammoPrefab, muzzel.position, muzzel.rotation);
                if (flash != null)
                {
                    flash.Play();
                }
                shot.transform.parent = null;
                AmmoPiece shotAmmo = shot.GetComponent<AmmoPiece>();
                shotAmmo.Fired(Stats.range, Stats.ammoSpeed, muzzel.position, unit, this, targetUnit);

                Load();

                if (limitedAmmo == true || unit.LimitAmmoUse == true)
                {
                    firedAmmo = shotAmmo;
                }
            }
        }

        isFiring = false;
    }

    protected virtual void Load()
    {
        if (isLoading == false)
        {
            isLoading = true;

            if (ammoReadyRack > 0)
            {
                StartCoroutine(Loaded(60f / (float)Stats.roundsPerMinute));
                ammoReadyRack -= 1;
                return;
            }

            if (ammoHalfReadyRack > 0)
            {
                StartCoroutine(Loaded(60f / ((float)Stats.roundsPerMinute / 2)));
                ammoHalfReadyRack -= 1;
                return;
            }

            if (ammoCount > 0)
            {
                if (Stats.readyRack == 0)
                {
                    StartCoroutine(Loaded(60f / (float)Stats.roundsPerMinute));
                    ammoCount -= 1;
                    return;
                }

                StartCoroutine(Loaded(60f / ((float)Stats.roundsPerMinute / 4)));
                ammoCount -= 1;
                return;
            }

            isLoading = false;
        }
    }

    protected IEnumerator Loaded(float delay)
    {
        yield return new WaitForSeconds(delay);

        if(isShooting == true)
        {
            isLoading = false;

            if ((limitedAmmo == true || unit.LimitAmmoUse == true) && firedAmmo != null)
            {
                yield break;
            }

            if (isFiring == false)
            {
                StartCoroutine(TryFire());
            }
        }
    }

    protected IEnumerator StockRacks()
    {
        //Debug.Log("Stocking Racks");
        while(ammoReadyRack < Stats.readyRack || ammoHalfReadyRack < Stats.halfReadyRack)
        {
            //Debug.Log("Checking Ammo");
            if (ammoCount > 0)
            {
                yield return new WaitForSeconds(60f / ((float)Stats.rackRoundsPerMinute / 2));

                if (ammoReadyRack != Stats.readyRack)
                {
                    ammoReadyRack += 1;
                    ammoCount -= 1;
                    continue;
                }

                if (ammoHalfReadyRack != Stats.halfReadyRack)
                {
                    ammoHalfReadyRack += 1;
                    ammoCount -= 1;
                    continue;
                }
            }

            if (ammoReadyRack != Stats.readyRack && ammoHalfReadyRack > 0)
            {
                yield return new WaitForSeconds(60f / (float)Stats.rackRoundsPerMinute);
                ammoReadyRack += 1;
                ammoHalfReadyRack -= 1;
            }
        }
        //Debug.Log("Stocked Racks to Full?");
    }

    private bool CheckAmmo()
    {
        if(stats.readyRack == 0 && stats.halfReadyRack == 0)
        {
            if(ammoCount > 0)
            {
                return true;
            }
        }

        if(ammoReadyRack > 0)
        {
            return true;
        }

        return false;
    }

    public virtual void RemoveShot()
    {
        firedAmmo = null;
        if (isShooting == true && isLoading == false && isFiring == false && ammoCount != 0)
        {
            StartCoroutine(TryFire());
        }
    }

    public virtual void Die()
    {
        if (isFiring)
        {
            StopCoroutine(TryFire());
        }

        isShooting = false;
        targetUnit = null;
        target = null;
        this.enabled = false;
    }
}
