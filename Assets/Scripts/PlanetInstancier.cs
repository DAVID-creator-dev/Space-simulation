using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

public class PlanetInstancier : MonoBehaviour
{
    public GameObject planetInstancierPrefab;
    public TMP_InputField inputFieldMass;
    public TMP_InputField inputFieldScale;
    public TMP_InputField inputFieldPos;
    public TMP_InputField velX;
    public TMP_InputField velY;
    public TMP_InputField velZ;

    private Vector3 position;
    private Vector3 velocity;
    private float mass;
    private float scale;

    VectorField[] fields;
    public PlanetManager planetManager;

    void Start()
    {
        fields = FindObjectsOfType<VectorField>();

        inputFieldMass.onValueChanged.AddListener(OnInputMassChanged);
        inputFieldScale.onValueChanged.AddListener(OnInputScaleChanged);
        inputFieldPos.onValueChanged.AddListener(OnInputPosChanged);

        velX.onValueChanged.AddListener(delegate { UpdateVelocity(); });
        velY.onValueChanged.AddListener(delegate { UpdateVelocity(); });
        velZ.onValueChanged.AddListener(delegate { UpdateVelocity(); });
    }

    void UpdateVelocity()
    {
        float x = float.TryParse(velX.text, out float tempX) ? tempX : 0f;
        float y = float.TryParse(velY.text, out float tempY) ? tempY : 0f;
        float z = float.TryParse(velZ.text, out float tempZ) ? tempZ : 0f;

        velocity = new Vector3(x, y, z);
    }

    void OnInputPosChanged(string value)
    {
        if (float.TryParse(value, out float number))
        {
            position.x = number;
        }
    }
    void OnInputMassChanged(string value)
    {
        if (float.TryParse(value, out float number))
        {
            mass = number;
        }
    }
    
    void OnInputScaleChanged(string value)
    {
        if (float.TryParse(value, out float number))
        {
            scale = number;
        }
    }

    public void InstanciatePlanet()
    {
        GameObject planet = Instantiate(planetInstancierPrefab, position * planetManager.globalScale, Quaternion.identity);
        Planet planetComponent = planet.GetComponent<Planet>();
        planetComponent.mass = mass;
        planetComponent.transform.localScale = Vector3.one * scale;
        planetComponent.scale = scale;
        planetComponent.position = position; 
        planetComponent.velocity = velocity;
        VectorField field = planet.GetComponent<VectorField>();
        field._planetManager = planetManager;
        planetManager.planets.Add(planetComponent);
    }
}
