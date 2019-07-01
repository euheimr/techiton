using UnityEngine;

/// <summary>
/// Reads a superset of properties from file, shared by moving vehicles, and 
/// makes them available to other components such as the VehicleController.
/// </summary>
public class VehicleProperties : UnitProperties {

    // Basic params
    [HideInInspector]
    public float speed;
    [HideInInspector]
    public float rotationSpeed;
    [HideInInspector]
    public float firingRange;
    [HideInInspector]
    public int damage;
    
    // Actions
    [HideInInspector]
    public float miningInterval;
    [HideInInspector]
    public float reloadTime;   

    protected override void InterpretFileData()
    {
        foreach (string parameter_line in unitParameters)
        {
            string tag = parameter_line.Split(' ')[0];
            string value = parameter_line.Split(' ')[1];
            switch (tag)
            {
                case "health":
                    maxHealth = int.Parse(value);
                    break;
                case "name":
                    type = value;
                    break;
                case "cost":
                    cost = int.Parse(value);
                    break;
                case "speed":
                    speed = float.Parse(value);
                    break;
                case "rot_speed":                    
                    rotationSpeed = float.Parse(value);
                    break;
                case "range":
                    firingRange = float.Parse(value);
                    break;
                case "reload":
                    reloadTime = float.Parse(value);
                    break;
                case "mining_interval":
                    miningInterval = float.Parse(value);
                    break;
                default:
                    Debug.LogError("Unexpected input for unit " + gameObject.name+": "+tag+" "+value);
                    break;
            }
        }      
    }
}
