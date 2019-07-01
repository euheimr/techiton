using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Unit cargo hold inventory, properties such as health, speed, damage, range, etc.
/// Loaded from a file.
/// </summary>
public abstract class UnitProperties : MonoBehaviour
{

    public string propertiesFile;
    protected string[] unitParameters;

    [HideInInspector]
    public int maxHealth;
    [HideInInspector]
    public string type;
    [HideInInspector]
    public int cost;

    void Awake ()
	{
        // Read properties file
	    List<string> parameterList = new List<string>();

	    foreach (string line in File.ReadAllLines("Assets/RTS Framework/Data/"+propertiesFile))
	    {
            // Ignore comments
            if (line[0] != '#') 
	            parameterList.Add(line);           
	    }

	    unitParameters = parameterList.ToArray();
	    InterpretFileData();
	}

    /// <summary>
    /// Interpret the loaded data in the 
    /// </summary>
    protected abstract void InterpretFileData();

}
