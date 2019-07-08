using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DestinationType = MapDestination.DestinationType;

/// <summary>
/// Class responsible for giving orders to AI units and vehicles in the 
/// current sector, using a similar approach to the SelectUnits script which
/// relays player's orders to selected units.
/// </summary>
public class AICommander : MonoBehaviour {

    [Tooltip("NPC types: patrol and move between destinations")]
    public float PatrolProbability;

    // For optimization reasons unit destinations are not checked in every Update
    private float _checkInterval = 2f;
    // Units available to the AI commander
    private List<NPCUnit> _availableUnits;

    private class NPCUnit
    {
        public NPCUnit() { }

        public NPCUnit(GameObject unit) { this.Unit = unit; }

        public GameObject Unit;
        // Given destinations for each of the units
        public Vector3 Destination;
        // Given AI unit types (patrol or travel)
        public bool IsPatrol;
        public DestinationType DestType; 
    }

	void Start () { 
        _availableUnits = new List<NPCUnit>();

        foreach(GameObject unit in GameObject.FindGameObjectsWithTag(Constants.NONPLAYER_UNIT))
        {
            AddNewUnit(unit);
        }

        // Set all units to attack mode
        BroadcastNewUnitState(Constants.UnitAction.Attack);

        // Add an event listener for when a unit is destroyed
        EventManager.UnitDestroyed +=
            (sender, e) =>
            {
                GameObject unit = (GameObject)sender;
                if (unit.tag != Constants.NONPLAYER_UNIT)
                    return;

                for (int i=0; i<_availableUnits.Count; i++)
                {
                    if (_availableUnits[i].Unit.Equals(unit))
                    {
                        _availableUnits.RemoveAt(i);
                        break;
                    }
                }
            };

	    EventManager.UnitSpawned += 
            (spawnedUnit, e) =>
	        {
                GameObject unit = (GameObject)spawnedUnit;

                if (unit.tag == Constants.NONPLAYER_UNIT)
                {
                    //Randomize unit name
                    unit.name = "Vehicle "+(char)('A'+(int)Random.Range(0,26))+(int)(100*Random.Range(0.1f, 1f));
                    AddNewUnit(unit);
                }
	        };

        StartCoroutine(CheckUnitDestinations());
	}
	
	private IEnumerator CheckUnitDestinations() {
        while (true)
        {
            // Check if a unit needs a new destination
            NPCUnit npc;

            for (int i = 0; i < _availableUnits.Count; i++)
            {
                npc = _availableUnits[i];

                if (Vector3.Distance(npc.Unit.transform.position, npc.Destination) < 20
                    || !npc.Unit.GetComponent<NavMeshAgent>().hasPath)
                {
                    OnUnitReachedDestination(npc);
                }
            }
            yield return new WaitForSeconds(_checkInterval);
        }
	}

    private void OnUnitReachedDestination(NPCUnit npc)
    {
        npc.Destination = GenerateNewDestination(npc);
        npc.Unit.GetComponent<NavMeshAgent>().SetDestination(npc.Destination);
    }

    private void AddNewUnit(GameObject unit)
    {
        NPCUnit npc = new NPCUnit(unit)
        {
            IsPatrol = Random.Range(0f, 1f) < PatrolProbability ? true : false,
        };

        npc.Destination = GenerateNewDestination(npc);
        npc.Unit.GetComponent<NavMeshAgent>().SetDestination(npc.Destination);

        if (npc.IsPatrol)
            npc.DestType = DestinationType.SpawnZone;

        _availableUnits.Add(npc);
    }

    private Vector3 GenerateNewDestination(NPCUnit npc)
    {
        Vector3 dest;

        if (npc.IsPatrol)
        {
            int spawnAreasCount = MapData.Instance.SpawnAreas.Length;
            dest = MapData.Instance.SpawnAreas[Random.Range(0, spawnAreasCount)].position;
        }
        else
        {
            MapDestination md = MapData.Instance.GetRandomDestination();
            dest = md.DestPos;
            npc.DestType = md.DestType;
        }

        return dest;
    }

    /// <summary>
    /// Used to give an action to all AI units
    /// </summary>
    public void BroadcastNewUnitState(Constants.UnitAction newState)
    {
        foreach (NPCUnit npc in _availableUnits)
        {
            npc.Unit.GetComponent<UnitController>().ChangeState(newState);
        }
    }    
}
