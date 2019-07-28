using UnityEngine;

public class StructureController : UnitController {

    public bool bCanAttack;
    public GameObject turret;

    // Projectile prefab
    public GameObject projectile;
    public float projectileInstantiationOffset;

    // Timers
    protected float _shootTimer;
    protected StructureProperties _unitData;
    protected GameObject _currentTarget;

    private void Awake()
    {
        // Set the available actions for this unit
        availableActions = new bool[Constants.NUMBER_OF_ACTIONS];
        availableActions[(int)Constants.UnitAction.Move] = false;
        availableActions[(int)Constants.UnitAction.Attack] = bCanAttack;
        availableActions[(int)Constants.UnitAction.Interact] = true;
        availableActions[(int)Constants.UnitAction.Deactivate] = true;
    }

    // Use this for initialization
    protected void Start()
    {
        base.Start();

        // Setup unit data accessor
        _unitData = (StructureProperties)_unitProperties;

        // Set initial health
        _health = _unitData.maxHealth;

        // Set to default action; deactivate
        _currentAction = (int)Constants.UnitAction.Deactivate;

        _shootTimer = -1;
    }

    // Update is called once per frame
    void Update()
    {
        // Only one of the following actions can be performed simultaneously
        if (_currentAction == (int)Constants.UnitAction.Attack)
        {
            ComputeAttack();
        }
        else if (_currentAction == (int)Constants.UnitAction.Interact)
        {
            ComputeInteraction();
        }
    }

    public override void SetTarget(GameObject target, Constants.TargetType type)
    {
        // Based on this unit's abilities, determine whether to ignore or set the target
        switch ((int)type)
        {
            case (int)Constants.TargetType.EnemyUnit:
                if (availableActions[(int)Constants.UnitAction.Attack] == true)
                {
                    Debug.Log(name + " setting current target set as " + target.name);
                    _currentTarget = target;
                }
                break;
            case (int)Constants.TargetType.MetalZone:
                if (availableActions[(int)Constants.UnitAction.Interact] == true)
                {
                    Debug.Log("Metal zone set as target: " + target.name);
                    _currentTarget = target;
                }
                break;
        }
    }

    public string GetCurrentAction()
    {
        switch (_currentAction)
        {
            case (int)Constants.UnitAction.Attack:
                return "attacking...";
            case (int)Constants.UnitAction.Interact:
                return "interacting...";
            default:
                return "idling...";
        }
    }

    #region unit actions

    protected void RotateTowardsTarget(Vector3 target)
    {
        Vector3 targetDir = target - turret.transform.position;
        Vector3 newDir = Vector3.RotateTowards(turret.transform.forward, targetDir,
            Time.deltaTime * _unitData.rotationSpeed, 0.0F);
        //Debug.DrawRay(transform.position, newDir, Color.red);
        turret.transform.rotation = Quaternion.LookRotation(newDir);
    }

    /// <summary>
    /// Set closest target, move into range, engage
    /// </summary>
    protected void ComputeAttack()
    {        
        if (_currentTarget != null)
            ComputeAttack(_currentTarget);

        // Find closest enemy target
        var units = GameObject.FindGameObjectsWithTag(Constants.NONPLAYER_UNIT);
        _currentTarget = null;
        float distance = Mathf.Pow(_unitData.firingRange, 2);

        foreach (GameObject unit in units)
        {
            Vector3 diff = unit.transform.position - transform.position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                _currentTarget = unit;
                distance = curDistance;
            }
        }
        if (_currentTarget == null)
            return;         // No targets are in range
        ComputeAttack(_currentTarget);
    }

    /// <summary>
    /// Move into range of set target, engage it
    /// </summary>
    protected void ComputeAttack(GameObject target)
    {
        float rangeToTarget = Vector3.Distance(transform.position, target.transform.position);
        _shootTimer -= Time.deltaTime;

        if (rangeToTarget < _unitData.firingRange)       
        {
            // Turn towards target
            RotateTowardsTarget(_currentTarget.transform.position);

            // Fire on target
            if (_shootTimer < 0 && 
                Vector3.Angle(turret.transform.forward, (target.transform.position - turret.transform.position)) < 3)
            {
                Projectile instance = Instantiate(projectile,
                    turret.transform.position + turret.transform.forward * projectileInstantiationOffset,
                    Quaternion.identity)
                    .GetComponent<Projectile>();

                instance.transform.parent = this.transform;
                instance.SetTarget(target);

                _shootTimer = (float)_unitData.reloadTime;
            }
        }
    }

    /// <summary>
    /// Process interaction with target object: mining zones, interacting with structures,
    /// following units, etc.
    /// </summary>
    protected void ComputeInteraction()
    {
        // TODO
    }

    #endregion unit actions

}
