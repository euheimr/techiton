using UnityEngine;

public class BuildPanel : Singleton<BuildPanel> {   

    [Tooltip("References to buttons of the build panel")]
    public GameObject[] buttons;
    [Tooltip("Build prefabs from Prefabs/BuildableUnits/")]
    public GameObject[] prefabs;
}
