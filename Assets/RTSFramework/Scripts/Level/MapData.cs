using UnityEngine;

/// <summary>
/// Keeps references of important game entities on the current level (map).
/// </summary>
public class MapData : Singleton<MapData> {

    public Transform[] SpawnAreas;
    public GameObject[] MineableZones;


    private void Awake()
    {
        // Find spawn zones and mineable zones on the map
        var spawns = GameObject.FindGameObjectsWithTag(Constants.SPAWN_AREA);
        SpawnAreas = new Transform[spawns.Length];
        int i = 0;
        foreach (GameObject spawn in spawns)
            SpawnAreas[i++] = spawn.transform;

        MineableZones = GameObject.FindGameObjectsWithTag(Constants.MINEABLE_ZONE);
    }

    /// <summary>
    /// Gets a random map destination for the NPCs to navigate to
    /// </summary>
    /// <returns>A descriptor with the position and type of a destination</returns>
    public MapDestination GetRandomDestination()
    {
        float rand = Random.Range(0, 2);
        Vector3 destPosition;
        MapDestination.DestinationType destType;

        if (rand < 1) { 
            destPosition = SpawnAreas[Random.Range(0, SpawnAreas.Length)].position;
            destType = MapDestination.DestinationType.SpawnZone;
        }
        else {
            destPosition = MineableZones[Random.Range(0, MineableZones.Length)].transform.position;
            destType = MapDestination.DestinationType.MineableZone;
        }


        return new MapDestination(destPosition, destType);
    }

}
