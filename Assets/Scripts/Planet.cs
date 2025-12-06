using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class Planet : MonoBehaviour
{
    #region Parameters
    [Header("Information")]
    public Vector3 position; 
    public Vector3 velocity;
    public float mass;

    [HideInInspector] public float scale = 1.0f;

    private Vector3 _acceleration;
    #endregion

    #region Function
    void Start()
    {
        position = position * GravitationalConstants.Au;
        velocity = velocity * GravitationalConstants.Av;
        mass = mass * GravitationalConstants.Am;
    }
    public void UpdatePosition(float deltaTime)
    {
        Vector3 newPosition = position + velocity * deltaTime + 0.5f * _acceleration * (deltaTime * deltaTime);
        
        position = newPosition;
        
        transform.position = (newPosition / GravitationalConstants.Au) * scale;
    }

    public void UpdateVelocity(Vector3 newAcceleration, float deltaTime)
    {
        velocity = velocity + (_acceleration + newAcceleration) * deltaTime / 2;
        
        _acceleration = newAcceleration;
    }

    public void SetAcceleration(Vector3 newAcceleration)
    {
        _acceleration = newAcceleration;
    }
    #endregion
}
