using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Allows the player to place units and structures on the terrain.
/// </summary>
public class BuildUnits : Singleton<BuildUnits>
{
    [Tooltip("The layer on which units are built (TerrainLayer)")]
    public LayerMask buildingLayer;
    [Tooltip("The Transform which parents all player units")]
    public GameObject unitHolder;

    [HideInInspector]
    public List<GameObject> units;

    private bool _isPlacingBuilding = false;
    private GameObject _indicator;
    private int _buildingsCounter = 0;

    void Start()
    {
        // Add all initially existing units
        units.AddRange(GameObject.FindGameObjectsWithTag(Constants.PLAYER_UNIT));

        GameObject[] buildButtons = BuildPanel.Instance.buttons;

        buildButtons[0].GetComponent<Button>().onClick.AddListener(() => StartPlacing(BuildPanel.Instance.prefabs[0]));
        buildButtons[1].GetComponent<Button>().onClick.AddListener(() => StartPlacing(BuildPanel.Instance.prefabs[1]));
        buildButtons[2].GetComponent<Button>().onClick.AddListener(() => StartPlacing(BuildPanel.Instance.prefabs[2]));
        buildButtons[3].GetComponent<Button>().onClick.AddListener(() => StartPlacing(BuildPanel.Instance.prefabs[3]));
        buildButtons[4].GetComponent<Button>().onClick.AddListener(() => StartPlacing(BuildPanel.Instance.prefabs[4]));
    }

    void Update()
    {
        if (_isPlacingBuilding)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            // Display building indicator (with green or red material)
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, buildingLayer))
            {
                _indicator.transform.position = hit.point;
            }

            if (!EventSystem.current.IsPointerOverGameObject()) // Suitable build location
            {
                if (Input.GetMouseButtonDown(0))    // LMB to confirm build
                {
                    EndPlacing(true);
                }

                if (Input.GetMouseButtonDown(1))    // RMB to cancel build
                {
                    EndPlacing();
                }
            }

        }
    }

    /// <summary>
    /// Returns whether the player is currently placing a building
    /// </summary>
    public bool IsConstructing()
    {
        return _isPlacingBuilding;
    }

    /// <summary>
    /// Invoked by UI build buttons
    /// </summary>
    /// <param name="buildingToBuild">Building prefab which was clicked</param>
    public void StartPlacing(GameObject buildingToBuild)
    {
        if (_isPlacingBuilding)
        {
            EndPlacing();
        }

        _isPlacingBuilding = true;
        _indicator = Instantiate(buildingToBuild, unitHolder.transform, false);
    }

    /// <summary>
    /// Called when buiding placing is aborted or a building is placed
    /// </summary>
    /// <param name="placeBuilding">Building placed or placement aborted</param>
    public void EndPlacing(bool placeBuilding = false)
    {
        if (!placeBuilding)
        {
            _isPlacingBuilding = false;
        }
        else
        {
            _isPlacingBuilding = !_indicator.GetComponentInChildren<UnitPlacement>().PlaceUnit();
            _buildingsCounter++;
        }
        Destroy(_indicator);
        _isPlacingBuilding = false;

    }
}
