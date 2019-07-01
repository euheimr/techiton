using UnityEngine;
using UnityEngine.AI;

public class VehicleController : UnitController
{
    [Tooltip("Whether the vehicle can engage and shoot enemy units")]
    public bool bCanAttack;
    [Tooltip("Whether the vehicle can interact with a mineable zone")]
    public bool bCanMine;

    // Projectile prefab
    public GameObject Projectile;
    // Prefab for the projector which displays a confirmation of given orders
    public GameObject MoveProjectorPrefab;

    // Timers
    protected float _shootTimer;
    protected VehicleProperties _unitData;

    protected GameObject _currentTarget;
    protected NavMeshAgent _navMeshAgent;

    // Timers
    private float _miningTimer;

    private void Awake()
    {
        base.Awake();
        // Set the available actions for this unit
        availableActions = new bool[Constants.NUMBER_OF_ACTIONS];
        availableActions[(int)Constants.UnitAction.Move] = true;
        availableActions[(int)Constants.UnitAction.Attack] = bCanAttack;
        availableActions[(int)Constants.UnitAction.Interact] = bCanMine;
    }

    // Use this for initialization
    protected void Start()
    {
        base.Start();
        // Setup unit data accessor
        _unitData = (VehicleProperties) _unitProperties;

        // Set initial health
        _health = _unitData.maxHealth;

        // Set to default action; move
        _currentAction = (int)Constants.UnitAction.Move;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
            Debug.LogError("Nav mesh agent not set on vehicle!");
        _navMeshAgent.speed = _unitData.speed;
        _navMeshAgent.angularSpeed = _unitData.rotationSpeed;      

        _shootTimer = -1;

        _miningTimer = _unitData.miningInterval;
    }

    // Update is called once per frame
    void Update()
    {
        ComputeMovement();

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
                    //Debug.Log(name+" setting current target set as " + target.name);
                    _currentTarget = target;
                }
                break;
            case (int)Constants.TargetType.MineableZone:
                if (availableActions[(int)Constants.UnitAction.Interact] == true)
                {
                    //Debug.Log("Mineable zone set as target: " + target.name);
                    _currentTarget = target;
                }
                break;
        }
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        //Debug.Log("name=" + name + ", agent stopped? "+ navMeshAgent.isStopped);
        // See if we are near the destination and hit another unit
        if (_navMeshAgent.isOnNavMesh && !_navMeshAgent.isStopped && Vector3.Distance(transform.position, _navMeshAgent.destination) < 2.5)
        {
            //Debug.Log("name=" + name + ", stopping agent ");
            _navMeshAgent.isStopped = true;
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
            case (int)Constants.UnitAction.Deactivate:
                return "idling...";
            default:
                return "moving...";
        }
    }

    #region unit actions

    /// <summary>
    /// Set waypoints for unit with the right mouse button, compute the unit's rotation and movement.
    /// </summary>
    protected void ComputeMovement()
    {
        if (isSelected && Input.GetMouseButtonDown(1))
        {
            // Command given to unit
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                _navMeshAgent.SetDestination(hit.point);
                _navMeshAgent.isStopped = false;

                // Spawn a projector to indicate that the order was accepted
                if (MoveProjectorPrefab != null)
                    GameObject.Instantiate(MoveProjectorPrefab).transform.position = hit.point + Vector3.up * 50;
            }
        }      
    }

    protected void RotateTowardsTarget(Vector3 target)
    {
        Vector3 targetDir = target - transform.position;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, Time.deltaTime * 1.5f, 0.0F);
        //Debug.DrawRay(transform.position, newDir, Color.red);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    /// <summary>
    /// Set closest target, move into range, engage
    /// </summary>
    protected void ComputeAttack()
    {
        if (_currentTarget != null)
            ComputeAttack(_currentTarget);

        // Find closest enemy target
        GameObject[] units = null;
        if (_isPlayerUnit)
            units = GameObject.FindGameObjectsWithTag(Constants.NONPLAYER_UNIT);
        else
            units = GameObject.FindGameObjectsWithTag(Constants.PLAYER_UNIT);

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

        if (rangeToTarget > _unitData.firingRange)
        {
            _navMeshAgent.isStopped = false;
            // Move towards target
            _navMeshAgent.destination = target.transform.position;
            ComputeMovement();
        }
        else
        {
            // Stay on the border of the range, don't get too close
            _navMeshAgent.isStopped = true;
            // Turn towards target
            RotateTowardsTarget(_currentTarget.transform.position);

            // Fire on target
            if (_shootTimer < 0 && Vector3.Angle(transform.forward, (target.transform.position-transform.position))<3)
            {
                Projectile instance = Instantiate(Projectile,
                    transform.position + transform.forward * (GetComponentInChildren<Collider>().bounds.size.z/1.5f),
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
        if (_currentTarget == null)
            return;
        if (!LayerMask.LayerToName(_currentTarget.layer).Equals("MineableLayer"))
            return;

        // Move to mineable zone
        _navMeshAgent.SetDestination(_currentTarget.transform.position);

        if (Vector3.Distance(transform.position, _currentTarget.transform.position) < 10)
        {
            _miningTimer -= Time.deltaTime;
            if (_miningTimer < 0)
            {
                _miningTimer = _unitData.miningInterval;
            }
        }
    }

    #endregion unit actions

}
