using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    #region Parameters
    [Header("UI Elements")]
    [SerializeField] private GameObject hud;

    [Header("Sliders")]
    [SerializeField] private Slider sliderGridSize; 
    [SerializeField] private Slider sliderGridSpacing; 

    [Header("Text Information")]
    [SerializeField] private TextMeshProUGUI textPlanetName;
    [SerializeField] private TextMeshProUGUI textMass;
    [SerializeField] private TextMeshProUGUI textPosX;
    [SerializeField] private TextMeshProUGUI textPosZ;
    [SerializeField] private TextMeshProUGUI textVelocityX;
    [SerializeField] private TextMeshProUGUI textVelocityZ;

    [Header("Toggles")]
    [SerializeField] private Toggle toggleFieldLine;
    [SerializeField] private Toggle toggleIs2D;
    [SerializeField] private Toggle toggleRotational;
    [SerializeField] private Toggle toggleVectorField;
    [SerializeField] private Toggle toggleIgnoreSun;

    [Header("Reference")]
    public VectorField vectorField; 
    public PlanetManager planetManager;
    #endregion

    #region Toggle

    public void UpdateValues()
    {
        toggleRotational.isOn = vectorField.showRotational;
        toggleVectorField.isOn = vectorField.showField; 
        toggleIs2D.isOn = vectorField.is2D;
        toggleFieldLine.isOn = vectorField.showFieldLine;
        toggleIgnoreSun.isOn = vectorField.ignoreSun; 

        sliderGridSize.value = vectorField.gridSize;
        sliderGridSpacing.value = vectorField.gridSpacing;
    }

    public void StopSimulation()
    {
        planetManager.simulationState = false;
    }

    public void StartSimulation()
    {
        planetManager.simulationState = true;
    }
    public void IgnoreSun(bool ignore)
    {
        vectorField.ignoreSun = ignore;
    }
    
    public void SetVisibilityVectorField(bool visible)
    {
        vectorField.showField = visible;
    }

    public void SetVisibilityFieldLine(bool visible)
    {
        vectorField.showFieldLine = visible; 
    }

    public void SetVisibility2D(bool visible)
    {
        vectorField.is2D = visible;
    }

    public void SetVisibilityRotational(bool visible)
    {
        vectorField.showRotational = visible;
    }
    #endregion
    
    public void SetHudVisibility(bool isVisible)
    {
        hud.SetActive(isVisible);
    }

    #region UpdateValues
    public void SetGridSize(float gridSize)
    {
        vectorField.gridSize = (int)gridSize;
    }

    public void SetGridSpacing(float gridSpacing)
    {
        vectorField.gridSpacing = gridSpacing;
    }
    public void SetVelocity(Vector3 velocity)
    {
        textVelocityX.text = velocity.x.ToString(CultureInfo.InvariantCulture); 
        textVelocityZ.text = velocity.z.ToString(CultureInfo.InvariantCulture);
    }
    
    public void SetPosition(Vector3 position)
    {
        textPosX.text = position.x.ToString(CultureInfo.InvariantCulture);
        textPosZ.text = position.z.ToString(CultureInfo.InvariantCulture);
    }
    public void SetMass(float mass)
    {
        textMass.text = mass.ToString(CultureInfo.InvariantCulture);
    }

    public void SetPlanetname(string planetName)
    {
        textPlanetName.text = planetName;
    }
    #endregion
}
