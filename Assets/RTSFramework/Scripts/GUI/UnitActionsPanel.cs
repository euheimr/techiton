using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the available unit actions in the right panel of the UI, depending on which
/// units have been selected.
/// </summary>
public class UnitActionsPanel : Singleton<UnitActionsPanel> {
    [Tooltip("Prefabs of unit action buttons: Move, Attack, Interact, Deactivate")]
    public GameObject[] ButtonPrefab;

    private bool[] _currentlyDisplayedActions;
    private SelectUnits _selectUnitsScript;

	void Start () {
        _currentlyDisplayedActions = new bool[Constants.NUMBER_OF_ACTIONS];
        _selectUnitsScript = BuildUnits.Instance.GetComponent<SelectUnits>();
	}

    /// <summary>
    /// Invoked by SelectUnits.cs, this sets the menu items to all the actions
    /// available to currently selected units.
    /// </summary>
    /// <param name="selectedObjects">Selected player units</param>
    public void SetMenuItems(List<GameObject> selectedObjects)
    {
        if (selectedObjects.Count < 1)
            return;
        _currentlyDisplayedActions = new bool[Constants.NUMBER_OF_ACTIONS];

        // Set actions to the union of all selected units' actions
        for (int i = 0; i < Constants.NUMBER_OF_ACTIONS; i++)
        {
            if (_currentlyDisplayedActions[i] == true)
                continue;
            foreach (GameObject go in selectedObjects) { 
                if (go.GetComponent<UnitController>().availableActions[i]){
                    _currentlyDisplayedActions[i] = true;
                    break;
                }
            }
        }

        // Clear menu items
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Add prefab buttons to menu
        for (int i = 0; i < Constants.NUMBER_OF_ACTIONS; i++)
            if (_currentlyDisplayedActions[i])
            {
                GameObject button = Instantiate(ButtonPrefab[i]);
                button.transform.SetParent(this.transform);
                // Add listener callback
                switch(i){
                    case 1:
                        button.GetComponent<Button>().onClick.AddListener(delegate {
                            SetButtonColor(button);
                            _selectUnitsScript.BroadcastNewUnitState(Constants.UnitAction.Attack);
                        });
                        break;
                    case 2:
                        button.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            SetButtonColor(button);
                            _selectUnitsScript.BroadcastNewUnitState(Constants.UnitAction.Interact);
                        });
                        break;
                    case 3:
                        button.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            SetButtonColor(button);
                            _selectUnitsScript.BroadcastNewUnitState(Constants.UnitAction.Deactivate);
                        });
                        break;
                    default:
                        button.GetComponent<Button>().onClick.AddListener(delegate
                        {
                            SetButtonColor(button);
                            _selectUnitsScript.BroadcastNewUnitState(Constants.UnitAction.Move);
                        });
                        break;
                }
                
            }
    }

    /// <summary>
    /// Set the pressed button to blue color; set all others to white.
    /// </summary>
    /// <param name="childIndex">Index of the pressed action button</param>
    private void SetButtonColor(GameObject button)
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Image>().color = (child.gameObject == button) ? Color.blue : Color.white;
        }
    }
}
