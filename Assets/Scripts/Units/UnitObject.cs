using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The basic unit class that all units will derive from
/// </summary>

public class UnitObject : MonoBehaviour
{
    [SerializeField] protected UnitManager manager;

    public int ID { get; protected set; }
    [SerializeField] protected bool isPlayer;
    public bool IsPlayer { get => isPlayer; }
    [SerializeField] protected GameObject model;
    [SerializeField] protected AIPath pather;
    public AIPath Pather => pather;
    [SerializeField] protected List<UnitWeapon> weapons;
    [SerializeField] private UnitStats stats;
    public UnitStats Stats { get => stats; }

    protected UnitObject closestEnemy;
    protected UnitObject target;
    protected List<UnitObject> enemiesSpotting = new List<UnitObject>();

    public List<UnitObject> EnemiesSpotting { get => enemiesSpotting; }
    [SerializeField] private bool limitAmmoUse;
    public bool LimitAmmoUse { get; }
    public bool isDead { get; protected set; }

    //Helpful links
    public UnitVehicle Vehicle { get; private set; }
    public UnitGroundVehicle GroundVehicle { get; private set; }

    [SerializeField] private LayerMask TerrainLayerMask;

    public Vector3 targetLocation;

    private void Start()
    {
        if(manager != null)
        {
            Initialize();
        }
    }

    public void Initialize(UnitManager newManager = null)
    {
        if (newManager != null)
        {
            manager = newManager;
        }

        ID = Random.Range(0, 100000);
        if (isPlayer)
        {
            while (manager.PlayerUnits.ContainsKey(ID))
            {
                ID = Random.Range(0, 100000);
            }

            gameObject.layer = 6;
        }

        if (!isPlayer)
        {
            while (manager.HostileUnits.ContainsKey(ID))
            {
                ID = Random.Range(0, 100000);
            }

            gameObject.layer = 7;
        }

        pather.maxSpeed = stats.driveSpeed;
        pather.maxAcceleration = stats.driveAcceleration;
        pather.rotationSpeed = stats.traverseSpeed;

        if (stats.currentHealth > 0)
        {
            stats.currentHealth = stats.health;
        }

        if (newManager == null)
        {
            manager.AddUnit(this, isPlayer);
        }

        if(targetLocation == null || targetLocation == Vector3.zero)
        {
            Debug.Log("Position set init");
            pather.destination = transform.position;
            targetLocation = transform.position;
        }

        InvokeRepeating("Scan", 1f, 1f);
    }

    protected virtual void Update()
    {
        AngleUnitToFloor();
        RotateToClosestEnemy();
    }

    // Scans for visible enemies and marks a visible enemy as it's target
    private void Scan()
    {
        if (isPlayer)
        {
            Spotting.FindVisibleEnemies(this, manager.HostileUnits);
        }
        else
        {
            Spotting.FindVisibleEnemies(this, manager.PlayerUnits);
        }

        if (CheckStillSpotted() == false)
        {
            manager.MakeUnitNotVisible(this, isPlayer);
            foreach (UnitObject unit in enemiesSpotting)
            {
                unit.SpottedEnemyNotSpottedAnymore(this);
            }
            enemiesSpotting.Clear();
        }

        if (target == null)
        {
            // gets the closest visible enemy as the default target
            UnitObject currentClosestEnemy = manager.CheckClosestVisibleEnemy(this, isPlayer);
            if (currentClosestEnemy == null)
            {
                ForgetTarget();
            }
            if (currentClosestEnemy != closestEnemy)
            {
                closestEnemy = currentClosestEnemy;
                foreach (UnitWeapon weapon in weapons)
                {
                    weapon.ChangeTarget(currentClosestEnemy);
                }
            }
        }
    }


    // Checks if this unit is already spotted by an enemy
    public bool CheckAlreadySpotted(UnitObject spotter)
    {
        if (enemiesSpotting.Contains(spotter))
        {
            return true;
        }

        return false;
    }

    // Check this unit is still being spotted by any enemies
    public bool CheckStillSpotted()
    {
        foreach (UnitObject unit in enemiesSpotting)
        {
            if (Spotting.CheckCanSeeSpotted(unit, this) == true)
            {
                return true;
            }
        }

        return false;
    }

    // Makes enemy units visible to the opposing side
    public void MakeVisible(UnitObject spottingUnit, bool isSentFromPlayer)
    {
        if (isSentFromPlayer != isPlayer)
        {
            manager.MakeUnitVisible(this, isPlayer);
            enemiesSpotting.Add(spottingUnit);
        }
    }
    
    public void EnableMesh()
    {
        model.SetActive(true);
    }

    public void DisableMesh()
    {
        if (isDead == false)
        {
            model.SetActive(false);
        }
    }

    // An enemy that we were spotting is no longer spotted - forget it as a target
    public void SpottedEnemyNotSpottedAnymore(UnitObject unit)
    {
        if (closestEnemy == unit)
        {
            foreach (UnitWeapon weapon in weapons)
            {
                if (weapon.TargetUnit == closestEnemy)
                {
                    weapon.ChangeTarget(null);
                }
            }
            closestEnemy = null;
        }
    }

    // If there are no more enemies visible we will forget the target we last had - If it's not visible we don't need to worry about it!
    public void ForgetTarget()
    {
        foreach (UnitWeapon weapon in weapons)
        {
            if (weapon.TargetUnit == closestEnemy && weapon.TargetUnit != null)
            {
                weapon.ChangeTarget(null);
            }
        }
        closestEnemy = null;
    }

    // Reports the target
    public void SpottingEnemyDestroyed(UnitObject unit)
    {
        EnemiesSpotting.Remove(unit);
        foreach (UnitWeapon weapon in weapons)
        {
            if (weapon.TargetUnit == unit)
            {
                weapon.ChangeTarget(null);
            }
        }
    }

    // Future use - when the target unit is no longer visible
    public void TargetLost()
    {
        target = null;
    }

    // Registers a hit from an ammo piece and checks for damage
    public void RegisterHit(AmmoPiece ammo, RaycastHit hitInfo)
    {
        float hitAngle = Vector3.Angle(hitInfo.normal, ammo.transform.forward);
        if (ArmourPen.RegisterHit(stats, ammo, hitInfo) == true)
        {
            TakeDamage(ammo.Damage);
        }
    }

    // Does damage
    public virtual void TakeDamage(int damage)
    {
        Debug.Log(damage / stats.toughness);
        if (damage / stats.toughness >= 10)
        {
            stats.currentHealth = 0;
            Die();
            return;
        }

        stats.currentHealth -= damage;

        if (stats.currentHealth <= 0)
        {
            Die();
        }

        return;
    }

    protected void Die()
    {
        isDead = true;
        manager.RemoveUnit(this, isPlayer);

        pather.canMove = false;

        foreach (UnitObject unit in EnemiesSpotting)
        {
            if (unit.EnemiesSpotting.Contains(this))
            {
                unit.SpottingEnemyDestroyed(this);
            }
        }

        foreach (UnitWeapon weapon in weapons)
        {
            weapon.Die();
        }
        enabled = false;
    }

    public void SetTargetEnemy(UnitObject unit)
    {
        target = unit;
        foreach(UnitWeapon weapon in weapons)
        {
            weapon.ChangeTarget(unit);
        }
    }

    public virtual void SetTargetLocation(Vector3 target)
    {
        Debug.Log("Position set");
        pather.destination = target;
        targetLocation = target;
    }

    public void AngleUnitToFloor()
    {
        RaycastHit hit;

        if (Physics.Raycast(model.transform.position + new Vector3(0,0.5f,0), -model.transform.up, out hit, 100, TerrainLayerMask))
        {
            Quaternion newRot = Quaternion.FromToRotation(model.transform.up, hit.normal) * model.transform.rotation;
            model.transform.rotation = Quaternion.RotateTowards(model.transform.rotation, newRot, 25 * Time.deltaTime);
        }
    }

    private void RotateToClosestEnemy()
    {
        if(pather.reachedEndOfPath == false)
        {
            return;
        }
        if(closestEnemy != null)
        {
            Vector3 targetPos = closestEnemy.transform.position;
            targetPos.y = transform.position.y;
            Vector3 targetDirection = (targetPos - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Stats.traverseSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, transform.localRotation.eulerAngles.y, 0);
        }
    }

    public void SetPlayer(bool value)
    {
        isPlayer = value;
    }
}
