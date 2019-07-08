using System;
using UnityEngine;

public abstract class UnitController : MonoBehaviour {

    // Is unit selected by player and ready to receive input
    [HideInInspector]
    public bool isSelected = false;

    [HideInInspector]
    public bool[] availableActions;
    protected int _currentAction;

    public GameObject explosionPrefab;
    protected UnitProperties _unitProperties;

    // Unit
    [HideInInspector]
    protected int _health;
    // Cargo
    protected int _cargoSize;
    protected int[] _cargoItemIDs;

    // Player owned unit if true, AI if false
    protected bool _isPlayerUnit;

    #region abstract members
    /// <summary>
    /// Set target for current unit, regardless of if the unit can attack it.
    /// </summary>
    /// <param name="target">The given target</param>
    /// <param name="type">The target type: enemy, structure, etc.</param>
    public abstract void SetTarget(GameObject target, Constants.TargetType type);
   
    #endregion abstract members

    // Event used to notify GUI elements that a Unit has been destroyed
    protected virtual void OnUnitDestroyed(EventArgs e)
    {
        EventManager.OnUnitDestroyed(e, this.gameObject);
    }

    protected void Awake()
    {
        _isPlayerUnit = (tag == "SelectableUnit");
    }

    protected void Start()
    {
        _unitProperties = GetComponent<UnitProperties>();
        if (_unitProperties == null)
            Debug.LogError("Unit properties component not present on unit!");           
    }

    /// <summary>
    /// Invoked by the unit actions menu, this changes the unit's current state to whatever
    /// the user has clicked in the menu
    /// </summary>
    /// <param name="action">The action selected for this unit</param>
    public void ChangeState(Constants.UnitAction action)
    {
        //Debug.Log("Unit " + this.name + " changing to mode " + action);
        _currentAction = (int)action;
    }

    /// <summary>
    /// Invoked by a projectile hitting a unit's collider, this reduces the unit's
    /// current health by the amount given as damage. If the unit is destroyed,
    /// instantiate an explosion, remove the gameobject and raise an event signalling
    /// its destruction.
    /// </summary>
    /// <param name="damage">How much damage the unit took</param>
    public void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            //Debug.Log("Unit " + name + " has been destroyed.");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            OnUnitDestroyed(EventArgs.Empty);
            Destroy(this.gameObject);
        }
    }

    public int GetHealth()
    {
        return _health;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        //Debug.Log(this.gameObject.name + " is now " + (isSelected ? "selected" : "deselected"));
        GetComponentInChildren<Projector>().enabled = isSelected;
    }
}
