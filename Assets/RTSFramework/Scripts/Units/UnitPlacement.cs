using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the unit's placement when building a unit on the terrain.
/// </summary>
public class UnitPlacement : MonoBehaviour
{
    // Red and green materials to indicate allowed or forbidden structure placement
    public Material validMaterial;
    public Material invalidMaterial;
    [Tooltip("The layer with which collisions and intersections are checked when placing")]
    public LayerMask buildingCollisionLayer;
    [Tooltip("Sound played upon confrmation and building of a unit")]
    public AudioClip buySound;
    [Tooltip("Reference to the prefab which will be built upon confirmation")]
    public GameObject unitToBuild;

    protected bool _bColliding = false;
    protected Material _originalMaterial;
    protected AudioSource _placementAudioSource;

    void Awake()
    {
        _placementAudioSource = AddAudio(buySound, false, false, 1f);
    }

    protected void Start()
    {
        _originalMaterial = GetComponentInChildren<MeshRenderer>().material;

        ApplyMaterial(validMaterial);
    }

    void Update()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas) && !_bColliding)
        {
            ApplyMaterial(validMaterial);
        }
        else
        {
            ApplyMaterial(invalidMaterial);
        }
    }

    void OnTriggerStay(Collider collider)
    {
        //Debug.Log(collider.gameObject.layer + " " + (1 << collider.gameObject.layer) + " " + buildingCollisionLayer);

        if (((1 << collider.gameObject.layer) & buildingCollisionLayer) != 0)
        {
            ApplyMaterial(invalidMaterial);
            _bColliding = true;
        }
    }

    void OnTriggerExit(Collider collider)
    {
        //Debug.Log(collider.gameObject.layer + " " + (1 << collider.gameObject.layer) + " " + buildingCollisionLayer);

        // Check if placed object is colliding
        if (((1 << collider.gameObject.layer) & buildingCollisionLayer) != 0)
        {
            ApplyMaterial(validMaterial);
            _bColliding = false;
        }
    }

    protected void ApplyMaterial(Material material)
    {
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.material = material;
        }
    }

    public bool PlaceUnit()
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
        {
            return false;
        }

        GameObject.Instantiate(unitToBuild, transform.position, transform.rotation, transform.parent);

        ApplyMaterial(_originalMaterial);

        _placementAudioSource.Play();

        // Broadcast spawn event
        EventManager.OnUnitSpawned(EventArgs.Empty, unitToBuild);

        return true;
    }

    private AudioSource AddAudio(AudioClip clip, bool loop, bool playAwake, float vol)
    {
        AudioSource newAudio = gameObject.AddComponent<AudioSource>();
        newAudio.clip = clip;
        newAudio.loop = loop;
        newAudio.playOnAwake = playAwake;
        newAudio.volume = vol;
        return newAudio;
    }

}
