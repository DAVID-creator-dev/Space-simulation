using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class VectorField : MonoBehaviour
{
    #region Parameters
    [Header("Mode Settings")]
    public bool ignoreSun = true;

    [Header("Target Settings")]
    public Planet targetPlanet;

    [Header("LineFields Configuration")]
    private Planet planet;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<GameObject> obj = new List<GameObject>();
    int lineIndex = 0;
    public float radius = 5f;
    int pointsPerFace = 5;
    List<Vector3> positions = new List<Vector3>();

    [Header("Grid Configuration")]
    public int gridSize = 5;
    private int previousGridSize;
    [Space]
    public float gridSpacing = 1f;
    private float previousGridSpacing;

    [Header("Display Options")]
    public bool showField = true;
    private bool previousShowField = false;
    [Space]
    public bool showRotational = false;
    public bool showFieldLine = false;
    [Space]
    public bool is2D = false;
    private bool previousIs2D = false;

    [Header("Visual Elements")]
    public GameObject arrowPrefab;
    public float arrowScaleFactor = 1f;

    [Header("Update Control")]
    private bool doOnce = true;

    [Header("Stored Data")]
    public List<Planet> _planets = new List<Planet>();
    private List<GameObject> _arrows = new List<GameObject>();
    public PlanetManager _planetManager;
    #endregion

    #region Unity Methods
    void Start()
    {
        showFieldLine = false;
        _planets = _planetManager.planets;
        planet = GetComponent<Planet>();
    }

    void Update()
    {
        HandleGridUpdates();

        if (showField)
            UpdateField();
        else
            DestroyField();

        if (showFieldLine)
            UpdateFieldLine();
        else
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line);
            }
            lines.Clear();

            foreach(GameObject o in obj)
            {
                Destroy(o);
            }
            obj.Clear();
        }
    }

    void HandleGridUpdates()
    {
        if (showField != previousShowField || is2D != previousIs2D)
        {
            doOnce = true;
            previousShowField = showField;
            previousIs2D = is2D;
        }

        if (doOnce || gridSize != previousGridSize || !Mathf.Approximately(gridSpacing, previousGridSpacing))
        {
            previousGridSize = gridSize;
            previousGridSpacing = gridSpacing;
            DestroyField();
            GenerateField();
            doOnce = false;
        }
    }

    #endregion

    #region Field Management

    public void GenerateField()
    {
        int yMax = is2D ? 0 : gridSize;

        for (int x = -gridSize; x <= gridSize; x++)
        {
            for (int y = -yMax; y <= yMax; y++)
            {
                for (int z = -gridSize; z <= gridSize; z++)
                {
                    Vector3 point = new Vector3(x, y, z) * gridSpacing + (targetPlanet.position / GravitationalConstants.Au) * targetPlanet.scale;

                    GameObject arrow = Instantiate(arrowPrefab, point, Quaternion.identity, transform);
                    _arrows.Add(arrow);
                }
            }
        }
    }

    public void DestroyField()
    {
        foreach (var arrow in _arrows)
            Destroy(arrow);

        _arrows.Clear();
    }

    void UpdateField()
    {
        if (_arrows.Count == 0)
            return;

        foreach (var arrow in _arrows)
        {
            Vector3 point = arrow.transform.position;

            Vector3 gravity = GetTotalGravityAtPoint(point);  

            if (showRotational)
                UpdateVectorField(arrow, ComputeRotational(point, gridSpacing));
            else
                UpdateRotational(arrow, gravity);
        }
    }

    #endregion

    #region Gravity Calculation

    public Vector3 GetTotalGravityAtPoint(Vector3 point)
    {
        Vector3 totalGravity = Vector3.zero;

        foreach (Planet planet in _planets)
        {
            if (ignoreSun && planet.name == "Soleil")
            {
                continue;
            }

            totalGravity += GetGravity(point, (planet.position / GravitationalConstants.Au) * planet.scale, planet.mass);    
        }

        return totalGravity;
    }

    Vector3 GetGravity(Vector3 position, Vector3 sourcePosition, float sourceMass)
    {
        Vector3 direction = sourcePosition - position;
        float r = direction.magnitude;

        return r == 0 ? Vector3.zero : direction.normalized * (GravitationalConstants.G * sourceMass / (r * r));
    }

    #endregion

    #region Rotational Calculation

    Vector3 ComputeRotational(Vector3 basePoint, float h)
    {
        Vector3 f0, fx, fy, fz;

        f0 = GetTotalGravityAtPoint(basePoint);
        fx = GetTotalGravityAtPoint(new Vector3(basePoint.x - gridSpacing, basePoint.y, basePoint.z));
        fy = GetTotalGravityAtPoint(new Vector3(basePoint.x, basePoint.y - gridSpacing, basePoint.z));
        fz = GetTotalGravityAtPoint(new Vector3(basePoint.x, basePoint.y, basePoint.z - gridSpacing));

        float dFz_dy = (f0.z - fy.z) / 2 * h;
        float dFy_dz = (f0.y - fz.y) / 2 * h;

        float dFx_dz = (f0.x - fz.x) / 2 * h;
        float dFz_dx = (f0.z - fx.z) / 2 * h;

        float dFy_dx = (f0.y - fx.y) / 2 * h;
        float dFx_dy = (f0.x - fy.x) / 2 * h;

        float rotX = dFz_dy - dFy_dz;
        float rotY = dFx_dz - dFz_dx;
        float rotZ = dFy_dx - dFx_dy;

        return new Vector3(rotX, rotY, rotZ);
    }

    #endregion

    #region Arrow & Line Management

    void UpdateVectorField(GameObject arrow, Vector3 gravityVector)
    {
        arrow.transform.localScale = Vector3.one * arrowScaleFactor;

        if (gravityVector.magnitude < 0.0001f) return;

        arrow.transform.rotation = Quaternion.LookRotation(gravityVector.normalized);
    }

    void UpdateRotational(GameObject arrow, Vector3 rotationalVector)
    {
        arrow.transform.localScale = Vector3.one * arrowScaleFactor;

        if (rotationalVector.magnitude < 0.0001f) return;
        
        if (rotationalVector.sqrMagnitude < 1e-6f) 
            rotationalVector = Vector3.forward; 
        
        arrow.transform.rotation = is2D ?
            Quaternion.Euler(0, Mathf.Atan2(rotationalVector.x, rotationalVector.z) * Mathf.Rad2Deg, 0) :
            Quaternion.LookRotation(rotationalVector.normalized);
    }

    #endregion

    #region LineFields

    void UpdateFieldLine()
    {
        positions.Clear();
        for (int i = 0; i < pointsPerFace; i++)
        {
            for (int j = 0; j < pointsPerFace; j++)
            {
                float step = radius * 2f / (pointsPerFace - 1);

                positions.Add(planet.transform.position + new Vector3(-radius, i * step - radius, j * step - radius));
                positions.Add(planet.transform.position + new Vector3(radius, i * step - radius, j * step - radius));
                positions.Add(planet.transform.position + new Vector3(i * step - radius, -radius, j * step - radius));
                positions.Add(planet.transform.position + new Vector3(i * step - radius, radius, j * step - radius));
                positions.Add(planet.transform.position + new Vector3(i * step - radius, j * step - radius, -radius));
                positions.Add(planet.transform.position + new Vector3(i * step - radius, j * step - radius, radius));
            }
        }

        foreach (Vector3 point in positions)
        {
            GenerateGravityLine(point);
        }
        lineIndex = 0;
    }
    void GenerateGravityLine(Vector3 point)
    {
        if (lines.Count >= 50 * positions.Count)
        {
            for (int i = 0; i < 50; ++i)
            {
                Vector3 direction = GetTotalGravityAtPoint(point);
                Vector3 nextPoint = point - direction.normalized;

                lines[lineIndex].SetPosition(0, point);
                lines[lineIndex].SetPosition(1, nextPoint);

                point = nextPoint;
                lineIndex++;
            }
        }
        else
        {
            for (int i = 0; i < 50; ++i)
            {
                Vector3 direction = GetTotalGravityAtPoint(point);
                Vector3 nextPoint = point - direction.normalized;

                GameObject lineObj = new GameObject("GravityLine");
                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.startWidth = 0.02f;
                line.endWidth = 0.01f;
                line.positionCount = 2;

                line.SetPosition(0, point);
                line.SetPosition(1, nextPoint);
                line.material = new Material(Shader.Find("Unlit/Color"));
                line.material.color = Color.red;

                point = nextPoint;
                lines.Add(line);
                obj.Add(lineObj);
            }
        }
    }

    #endregion
}
