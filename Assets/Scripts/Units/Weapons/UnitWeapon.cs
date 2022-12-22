using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWeapon : MonoBehaviour
{
    [SerializeField] private UnitObject unit;

    [SerializeField] private WeaponStats stats;
    public WeaponStats Stats { get => stats; }

    [Header("Weapon Pieces")]
    [SerializeField] private GameObject ammoPrefab;
    [SerializeField] private Transform gunHome;
    [SerializeField] private Transform muzzel;
    [SerializeField] private ParticleSystem flash;
    [SerializeField] protected AmmoPiece firedAmmo;

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 previousTargetPosition;
    [SerializeField] private Transform predictedTargetPosition;
    private UnitObject targetUnit;
    public UnitObject TargetUnit { get => targetUnit; }
    private bool isShooting;

    [SerializeField] protected int ammoCount;
    [SerializeField] protected int ammoReadyRack;
    [SerializeField] protected int ammoHalfReadyRack;

    [SerializeField] private bool limitedAmmo = true;
    public bool LimitedAmmo { get => limitedAmmo; }
    protected bool isLoading;

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

                Vector3 targetDirection = target.position - transform.position;
                float angle1 = Vector3.Angle(targetDirection, transform.forward);

                if (angle1 <= 5)
                {
                    if (Vector3.Distance(transform.position, target.position) < Stats.range && isShooting == false && unit.isDead == false)
                    {
                        StartShooting();
                        return;
                    }
                    if (Vector3.Distance(transform.position, target.position) > Stats.range)
                    {
                        CancelInvoke("Fire");
                        isShooting = false;
                    }
                    return;
                }

                CancelInvoke("Fire");
                isShooting = false;
                return;
            }

            CancelInvoke("Fire");

            Quaternion targetRotation = Quaternion.LookRotation(unit.transform.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);
            gunHome.rotation = Quaternion.RotateTowards(gunHome.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
            gunHome.localEulerAngles = new Vector3(gunHome.localRotation.eulerAngles.x, 0, 0);
        }
    }

    private void RotateTurret()
    {
        Vector3 targetPos = target.transform.position;
        targetPos.y = transform.position.y;
        Vector3 targetDirection = (targetPos - transform.position).normalized;
        Vector3 predictedPosition = CalculatePredictedPosition();
        predictedPosition.y = transform.position.y;

        if (predictedPosition == Vector3.zero)
        {
            previousTargetPosition = target.position;
            return;
        }
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);

        previousTargetPosition = target.position;
        if (predictedTargetPosition != null)
        {
            predictedTargetPosition.position = predictedPosition;
        }
    }

    private Vector3 CalculatePredictedPosition()
    {
        //Pbi (muzzel position) + Vb (bullet velocity) * t = Pti (targetPosition) + Vti (tareget velocity) * t + 0.5 * At (target acceleration) * (t * t)

        if(previousTargetPosition == null)
        {
            previousTargetPosition = target.position;
            return Vector3.zero;
        }

        Vector3 positionProjection = transform.forward * Stats.ammoSpeed * Time.deltaTime;

        float Vb = ((positionProjection - (muzzel.position)).magnitude * Time.deltaTime);
        float Vti = (target.position - previousTargetPosition).sqrMagnitude * Time.deltaTime;

        Vector3 targetDir = target.position - transform.position;
        float iSpeed2 = Vb * Vb;
        float tSpeed2 = Vti * Vti;
        float fDot1 = Vector3.Dot(targetDir, (target.position - previousTargetPosition));
        float targetDist2 = targetDir.sqrMagnitude;
        float d = (fDot1 * fDot1) - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < 0.1f)  // negative == no possible course because the interceptor isn't fast enough
            return Vector3.zero;
        float sqrt = Mathf.Sqrt(d);
        float S1 = (-fDot1 - sqrt) / targetDist2;
        float S2 = (-fDot1 + sqrt) / targetDist2;
        if (S1 < 0.0001f)
        {
            if (S2 < 0.0001f)
                return Vector3.zero;
            else
                return (S2) * targetDir + (target.position - previousTargetPosition);
        }
        else if (S2 < 0.0001f)
            return (S1) * targetDir + (target.position - previousTargetPosition);
        else if (S1 < S2)
            return (S2) * targetDir + (target.position - previousTargetPosition);
        else
            return (S1) * targetDir + (target.position - previousTargetPosition);
    }

    private void RotateGun()
    {
        Quaternion previousRotation = transform.rotation;
        Vector3 targetPos = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
        Vector3 targetDirection = (target.transform.position - gunHome.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        gunHome.rotation = Quaternion.RotateTowards(gunHome.rotation, targetRotation, Stats.rotationSpeed * Time.deltaTime);
        gunHome.localEulerAngles = new Vector3(gunHome.localRotation.eulerAngles.x, 0, 0);
    }

    public void ChangeTarget(UnitObject nTarget)
    {
        if (unit.isDead == false)
        {
            targetUnit = nTarget;

            if (targetUnit != null)
            {
                target = nTarget.transform;
                CancelInvoke("StockRacks");
                return;
            }

            target = null;
            InvokeRepeating("StockRacks", 60f / (float)Stats.rackRoundsPerMinute, 60f / (float)Stats.rackRoundsPerMinute);
        }
    }

    public void StartShooting()
    {
        if (isShooting == false)
        {
            Invoke("Fire", Stats.aimTime);
            isShooting = true;
        }
    }

    protected virtual void Fire()
    {
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
    }

    protected virtual void Load()
    {
        isLoading = true;

        if (ammoReadyRack > 0)
        {
            Invoke("Loaded", 60f / (float)Stats.roundsPerMinute);
            ammoReadyRack -= 1;
            return;
        }

        if(ammoHalfReadyRack > 0)
        {
            Invoke("Loaded", 60f / ((float)Stats.roundsPerMinute / 2));
            ammoReadyRack -= 1;
            return;
        }

        if (ammoCount > 0)
        {
            if (Stats.readyRack == 0)
            {
                Invoke("Loaded", 60f / (float)Stats.roundsPerMinute);
                ammoCount -= 1;
                return;
            }

            Invoke("Loaded", 60f / ((float)Stats.roundsPerMinute / 4));
            ammoCount -= 1;
            return;
        }

        Debug.Log("Out of ammo so cannot load");
    }

    private void Loaded()
    {
        isLoading = false;
        if(isShooting == true)
        {
            if ((limitedAmmo == true || unit.LimitAmmoUse == true) && firedAmmo != null)
            {
                return;
            }

            Invoke("Fire", Stats.aimTime);
        }
    }

    protected void StockRacks()
    {
        Debug.Log("Stocking Racks");

        if(ammoReadyRack == Stats.readyRack && ammoHalfReadyRack == Stats.halfReadyRack)
        {
            CancelInvoke("StockRacks");
            return;
        }

        if (ammoCount == 0)
        {
            if(ammoReadyRack != Stats.readyRack && ammoHalfReadyRack != 0)
            {
                ammoReadyRack += 1;
                ammoHalfReadyRack -= 1;
                return;
            }

            CancelInvoke("StockRacks");
            return;
        }

        if(ammoReadyRack != Stats.readyRack)
        {
            ammoReadyRack += 1;
            ammoCount -= 1;
            return;
        }

        if (ammoHalfReadyRack != Stats.halfReadyRack)
        {
            ammoHalfReadyRack += 1;
            ammoCount -= 1;
            return;
        }
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
        if (isShooting == true)
        {
            if(isLoading == false)
            {
                Invoke("Fire", Stats.aimTime);
            }
        }
    }

    public void Die()
    {
        CancelInvoke("Fire");
        isShooting = false;
        targetUnit = null;
        target = null;
        this.enabled = false;
    }
}
