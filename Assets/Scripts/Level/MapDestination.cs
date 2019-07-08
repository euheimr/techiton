using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDestination {

    public Vector3 DestPos;

    public DestinationType DestType;

    public enum DestinationType
    {
        Structure,
        MetalZone,
        SpawnZone
    }

    public MapDestination(Vector3 pos, DestinationType type)
    {
        DestPos = pos;
        DestType = type;
    }

    public MapDestination()
    {
    }

}
