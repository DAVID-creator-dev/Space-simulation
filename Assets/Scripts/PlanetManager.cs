using System.Collections.Generic;
using UnityEngine;

public static class GravitationalConstants
{
    public const float G = 6.67430e-11f;
    public const float Au = 1.495978707e11f;
    public const float Av = 1000f;
    public const float Am = 5.972e24f;
}

public class PlanetManager : MonoBehaviour
{
    #region Parameters
    public bool simulationState = false;
    public List<Planet> planets = new List<Planet>();
    public float globalScale;
    public float timeStep = 3600f;
    #endregion

    #region UnityMethods
    private void Start()
    {
        foreach (Planet planet in planets) { 
            planet.scale = globalScale;
        }
    }
    #endregion

    #region Function
    Vector3 Acceleration(Vector3 pos, Vector3 target, float mass)
    {
        Vector3 direction = target - pos;
        float distance = direction.magnitude;

        if (distance == 0) return Vector3.zero;

        Vector3 acceleration = direction.normalized * (GravitationalConstants.G * mass / (distance * distance));

        return acceleration;
    }

    private Vector3 ComputeAcceleration(Planet planet)
    {
        Vector3 computeAcceleration = Vector3.zero;

        foreach (Planet otherPlanet in planets)
        {
            if (otherPlanet == planet)
                continue;

            Vector3 acceleration = Acceleration(planet.position, otherPlanet.position, otherPlanet.mass);

            computeAcceleration += acceleration;
        }

        return computeAcceleration;
    }

    void FixedUpdate()
    {
        if (!simulationState) return;

        foreach (Planet planet in planets)
        {
            Vector3 acceleration = ComputeAcceleration(planet);
            planet.SetAcceleration(acceleration);

            planet.UpdatePosition(timeStep);

            Vector3 newAcceleration = ComputeAcceleration(planet);
            planet.UpdateVelocity(newAcceleration, timeStep);
        }
    }
    #endregion
}
