using System;
using UnityEngine;

public class StructureProperties : UnitProperties {

    // Basic params
    [HideInInspector]
    public bool bUsesEnergy;
    [HideInInspector]
    public float rotationSpeed;
    [HideInInspector]
    public float firingRange;
    [HideInInspector]
    public int damage;
    [HideInInspector]
    public int energy;

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
                case "uses_energy":
                    bUsesEnergy = bool.Parse(value);
                    break;
                case "rot_speed":
                    rotationSpeed = (float)double.Parse(value);
                    break;
                case "range":
                    firingRange = (float)double.Parse(value);
                    break;
                case "reload":
                    reloadTime = (float)double.Parse(value);
                    break;
                case "energy":
                    energy = int.Parse(value);
                    break;
                default:
                    Debug.LogError("Unexpected input for unit " + gameObject.name + ": " + tag + " " + value);
                    break;
            }
        }     
    }
}
