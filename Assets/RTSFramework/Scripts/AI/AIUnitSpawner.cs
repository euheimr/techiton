using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// This class is tasked with spawning AI units on the current level.
/// </summary>
public class AIUnitSpawner : MonoBehaviour {
    
    public float MinSpawnInterval, MaxSpawnInterval;
    public GameObject UnitPrefab;
    public int MaxUnits;

    private Transform[] _spawnLocations;
    private int _spawnedUnits = 0;

    void Start()
    {
        _spawnLocations = MapData.Instance.SpawnAreas;
        StartCoroutine(SpawnUnit());
    }

    private IEnumerator SpawnUnit() {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(MinSpawnInterval, MaxSpawnInterval));

            if (_spawnedUnits >= MaxUnits)
                continue;

            // Get randomized spawn position
            Vector3 spawnPos = _spawnLocations[Random.Range(0, _spawnLocations.Length)].position;
            spawnPos += new Vector3(Random.value * 5, 100, Random.value*5);

            // Raycast down to see spawn height (to place accurately on terrain)
            RaycastHit hit;
            if (Physics.Raycast(spawnPos, Vector3.down, out hit, Mathf.Infinity, LayerMask.NameToLayer("GroundLayer")))
            {
                spawnPos = hit.point;
            }

            EventManager.OnUnitSpawned(EventArgs.Empty, Instantiate(UnitPrefab, spawnPos, Quaternion.identity, transform));

            _spawnedUnits++;
        }
    }
  
}
