using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectUnits : MonoBehaviour {
    // Reference to UnitActionsPanel needed to update menu items
    public UnitActionsPanel ActionsPanel;

    // Currently selected objects
    private List<GameObject> _selectedUnits;
    // Friendly (selectable) vehicles
    private GameObject[] _selectableUnits;
    // Selected (enemy) target
    private GameObject _selectedTarget;

    private BuildUnits _player;

    #region Selection Utility Rectangles
    static Texture2D _whiteTexture;
    private static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }


    private static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min( screenPosition1, screenPosition2 );
        var bottomRight = Vector3.Max( screenPosition1, screenPosition2 );
        // Create Rect
        return Rect.MinMaxRect( topLeft.x, topLeft.y, bottomRight.x, bottomRight.y );
    }

    private static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    private static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, rect.width, thickness ), color );
        // Left
        DrawScreenRect( new Rect( rect.xMin, rect.yMin, thickness, rect.height ), color );
        // Right
        DrawScreenRect( new Rect( rect.xMax - thickness, rect.yMin, thickness, rect.height ), color);
        // Bottom
        DrawScreenRect( new Rect( rect.xMin, rect.yMax - thickness, rect.width, thickness ), color );
    }

    private static Bounds GetViewportBounds( Camera camera, Vector3 screenPosition1, Vector3 screenPosition2 )
    {
        var v1 = Camera.main.ScreenToViewportPoint( screenPosition1 );
        var v2 = Camera.main.ScreenToViewportPoint( screenPosition2 );
        var min = Vector3.Min( v1, v2 );
        var max = Vector3.Max( v1, v2 );
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;
 
        var bounds = new Bounds();
        bounds.SetMinMax( min, max );
        return bounds;
    }

    bool isSelecting = false;
    Vector3 mousePosition1;

    private bool IsWithinSelectionBounds(GameObject gameObject)
    {
        var camera = Camera.main;
        var viewportBounds =
            GetViewportBounds(camera, mousePosition1, Input.mousePosition);

        return viewportBounds.Contains(
            camera.WorldToViewportPoint(gameObject.transform.position));
    }

    void OnGUI()
    {
        if (isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = GetScreenRect(mousePosition1, Input.mousePosition);
            // Draw transparent rectangle
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            // Draw rectangle border
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
    #endregion Selection Utility Rectangles

    void Start()
    {
        _selectedUnits = new List<GameObject>();
        ActionsPanel = UnitActionsPanel.Instance;
        _player = BuildUnits.Instance;

        // Setup listeners
        EventManager.UnitDestroyed += (sender, e) =>
        {
            GameObject unit = (GameObject)sender;
            // If a selected unit is destroyed, remove it.
            foreach (GameObject go in _selectedUnits)
            {
                if (unit.Equals(go))
                {
                    _selectedUnits.Remove(go);
                    break;
                }
            }
        };
    }

    void Update () { 
        RaycastHit hit;

        if (_player.IsConstructing())
            return;
        // If we press the left mouse button, save mouse location and begin selection
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            _selectableUnits = GameObject.FindGameObjectsWithTag(Constants.PLAYER_UNIT);
            //if (!EventSystem.current.IsPointerOverGameObject()) //UI clicked
            //    return;

            // If player clicked on a selectable unit
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                //Debug.Log("Unit clicked: "+hit.transform.gameObject.name);
                if (hit.transform.gameObject.tag == Constants.PLAYER_UNIT)
                {
                    ClearSelectedUnits();
                    hit.transform.gameObject.GetComponent<UnitController>().SetSelected(true);
                    _selectedUnits.Add(hit.transform.gameObject);
                    isSelecting = false;

                    // Notify UnitActionsPanel
                    ActionsPanel.SetMenuItems(_selectedUnits);
                    return;
                }
            }
            isSelecting = true;
            mousePosition1 = Input.mousePosition;
        }
        // If we let go of the left mouse button, end selection
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!isSelecting)
                return;
            isSelecting = false;
            // Find selectable gameObjects in rectangle
            foreach(GameObject go in _selectableUnits)
                if (IsWithinSelectionBounds(go))
                {
                    go.GetComponent<UnitController>().SetSelected(true);
                    _selectedUnits.Add(go);
                    ActionsPanel.SetMenuItems(_selectedUnits);
                }
                else
                {                  
                    go.GetComponent<UnitController>().SetSelected(false);
                    _selectedUnits.Remove(go);
                    ActionsPanel.SetMenuItems(_selectedUnits);
                }
        }

        // Right mouse button : Check if player clicked on a target and set current target
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        { 
            // If player clicked on a selectable unit
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if (hit.transform.gameObject.tag == Constants.NONPLAYER_UNIT)
                {                    
                    // Set target unit
                    _selectedTarget = hit.transform.gameObject;
                    BroadcastNewTarget(Constants.TargetType.EnemyUnit);
                    return;
                }
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("MineableLayer"))
                {
                    // Set destination interactable zone
                    _selectedTarget = hit.transform.gameObject;
                    BroadcastNewTarget(Constants.TargetType.MineableZone);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Mark all selected units as unselected
    /// </summary>
    void ClearSelectedUnits()
    {
        foreach (GameObject go in _selectedUnits)
        {
            if (go.GetComponent<UnitController>() == null)
                Debug.Log("go name "+go.name);
            go.GetComponent<UnitController>().SetSelected(false);
        }
        _selectedUnits = new List<GameObject>();

        // Notify UnitActionsPanel
        ActionsPanel.SetMenuItems(_selectedUnits);
    }

    /// <summary>
    /// Invoked by pressing the GUI buttons in the Unit Actions Panel, this broadcasts
    /// the new state to all selected units.
    /// </summary>
    /// <param name="newState">The next state to notify all units</param>
    public void BroadcastNewUnitState(Constants.UnitAction newState) 
    {
        foreach (GameObject go in _selectedUnits)
            go.GetComponent<UnitController>().ChangeState(newState);
    }

    /// <summary>
    /// Broadcast to all selected units that a new target has been selected. Units
    /// that cannot interact with the target will ignore it.
    /// </summary>
    public void BroadcastNewTarget(Constants.TargetType targetType)
    {
        foreach (GameObject go in _selectedUnits) {
            go.GetComponent<UnitController>().SetTarget(_selectedTarget, targetType);
        }
    }    
}
