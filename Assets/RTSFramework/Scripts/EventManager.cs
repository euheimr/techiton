using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Class handling events such as unit destruction and spawning. This class
/// provides static methods for easy subscription of objects.
/// </summary>
public class EventManager : MonoBehaviour {

    // Event used to notify subscribers that a Unit has been destroyed
    public static event EventHandler UnitDestroyed, UnitSpawned;   

    // Invoked by: UnitController
    // Listened to by: VehicleDetailsHUD, StructureDetailsHUD, DetailedInfo, SelectUnits
    public static void OnUnitDestroyed(EventArgs e, GameObject unit)
    {
        UnitDestroyed(unit, e);        
    }

    // Invoked by: AIUnitSpawner, VehiclePlacement, StructurePlacement, SelectUnits
    // Listened to by: AIunitController, SelectUnits, VehicleDetailsHUD
    public static void OnUnitSpawned(EventArgs e, GameObject unit)
    {
        UnitSpawned(unit, e);
    }
   
}
