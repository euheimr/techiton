using UnityEngine;

public class Constants : MonoBehaviour {
    // Camera
    public static int CAMERA_MAX_HEIGHT = 100;
    public static int CAMERA_MIN_HEIGHT = 30;
    public static float CAMERA_MOVEMENT_MARGIN = 0.1f; // percentage of screen
    public static float CAMERA_ZOOM_SPEED = 20;
    public static float CAMERA_MOVEMENT_SPEED = 2.5f;

    // Unit actions
    public enum UnitAction{
        Move = 0,
        Attack = 1,
        Interact = 2,
        Deactivate = 3
    }
    public static int NUMBER_OF_ACTIONS = 4;
    public static UnitAction IntegerToAction(int i)
    {
        switch (i)
        {
            
            case 1:
                return UnitAction.Attack;
            case 2:
                return UnitAction.Interact;
            case 3:
                return UnitAction.Deactivate;
            default:
                return UnitAction.Move;
        }
    }

    // Target types, for broadcast to selected units by SelectUnits.cs, clicked
    // on by right mouse button
    public enum TargetType
    {
        EnemyUnit = 0,
        MetalZone = 1,
        Structure = 2
    }

    // Tags
    public static string NONPLAYER_UNIT = "NonplayerUnit";
    public static string PLAYER_UNIT = "SelectableUnit";
    public static string SPAWN_AREA = "Spawn";
    public static string METAL_ZONE = "Metal";

    // Terrain
    public static int TERRAIN_HALF_SIZE = 500;

}
