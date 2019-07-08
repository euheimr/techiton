using UnityEngine;

/// <summary>
/// Controls a projectile fired by a unit, and its impact with a target.
/// </summary>
public class Projectile : MonoBehaviour {

    public LayerMask UnitCollisionLayer;
    public int ProjectileDamage;
    public float Speed;
    public float ProjectileRange;

    private GameObject _target;
    private bool _bDestroyed = false;
    private Vector3 _startPosition;

    void Start()
    {
        _startPosition = transform.parent.position;
        transform.LookAt(_target.transform.position);
        this.transform.SetParent(GameObject.Find("Units").transform);
    }

    void Update () {
        if (!_bDestroyed)
        {            
            float distance = Vector3.Distance(transform.position, _startPosition);

            if (distance > ProjectileRange)
            {
                DestroyProjectile();
            }
            transform.position += transform.forward*Speed*Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Projectile should only hit the intended target
    /// </summary>
    public void SetTarget(GameObject target)
    {
        if (this._target == null)
            this._target = target;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (((1 << collider.gameObject.layer) & UnitCollisionLayer) != 0)
        {
            DestroyProjectile();
            UnitController unit = collider.gameObject.GetComponent<UnitController>();
            unit.TakeDamage(ProjectileDamage);
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
        _bDestroyed = true;
    }
}
