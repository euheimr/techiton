using System;
using UnityEngine;

/// <summary>
/// Displays unit healthbar and status on the UI
/// </summary>
public class UnitDetailsHUD : MonoBehaviour {

    [Tooltip("Health bar and text prefab")]
    public DetailedInfo prefab;

    private DetailedInfo _detailedInfo;
    private UnitController _unit;

    private void Start()
    {
        _unit = GetComponent<UnitController>();

        _detailedInfo = Instantiate(prefab, GameObject.Find("DrawOnBottom").transform);
        _detailedInfo.Initialize(this.gameObject);
        _detailedInfo.target = transform;
        _detailedInfo.SetMaxValue(GetComponent<UnitProperties>().maxHealth);
        _detailedInfo.UpdateSlider(_unit.GetHealth());
        _detailedInfo.SetText(_unit.name);
        EventManager.UnitDestroyed += DelegateInstance;
        _detailedInfo.gameObject.SetActive(false);
    }

    private void DelegateInstance(object sender, EventArgs e)
    {
        if (_unit == null || _unit.gameObject.Equals((GameObject)sender))
            Destroy(_detailedInfo.gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
            _detailedInfo.gameObject.SetActive(true);
        if (Input.GetKeyUp(KeyCode.LeftAlt))
            _detailedInfo.gameObject.SetActive(false);

        if (_detailedInfo.gameObject.activeInHierarchy) { 
            _detailedInfo.SetMaxValue(GetComponent<UnitProperties>().maxHealth);
            _detailedInfo.UpdateSlider(_unit.GetHealth());
        }
    }

    private void OnDestroy()
    {
        EventManager.UnitDestroyed -= DelegateInstance;
    }
}
