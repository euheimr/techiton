using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the UI health bar and action text of a unit
/// </summary>
public class DetailedInfo : MonoBehaviour {
    private RectTransform rectTransform;
    private Slider healthSlider;
    private Text actionText;
    private GameObject owner;

    public Transform target;

    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        healthSlider = GetComponent<Slider>();
        actionText = GetComponentInChildren<Text>();
    }

    public void Initialize (GameObject owner) {
        this.owner = owner;       

        // Set owner object destruction listener:
        EventManager.UnitDestroyed +=
               (sender, e) =>
               {
                   if (owner.Equals((GameObject)sender))
                       Destroy(this.gameObject);
               };
    }

    void Update () {
        float x = Camera.main.WorldToScreenPoint(target.position).x - Screen.width / 2;
        float y = Camera.main.WorldToScreenPoint(target.position).y - Screen.height / 2;
        
        rectTransform.localPosition = new Vector3(x, y+15f, 0f);
    }

    /// <summary>
    /// Set the maximum value of the health slider
    /// </summary>
    /// <param name="value">Maximum value of the healthbar</param>
    public void SetMaxValue(float value)
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        healthSlider.maxValue = value;
        healthSlider.value = value;
    }

    /// <summary>
    /// Updates the slider with the current health value
    /// </summary>
    /// <param name="value">Current health amount</param>
    public void UpdateSlider(float value)
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        healthSlider.value = value;
    }

    /// <summary>
    /// Sets the unit action text above the health bar
    /// </summary>
    /// <param name="action"></param>
    public void SetText(string action)
    {
        if(actionText == null)
            actionText = GetComponentInChildren<Text>();
        actionText.text = action;
    }


}
