using System.Collections;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Parameters
    [SerializeField] private CharacterController characterController;
    
    [Header("General Settings")]
    public float speed = 10f;
    public float rotateSpeed = 2f;
    public bool isInOrbitMode = false;
    public bool allowMovement = true;
    public float verticalLimit = 80f;

    [HideInInspector] public VectorField vectorField;
    [HideInInspector] public Planet currentPlanet;
    private float _verticalAngle = 0f;
    private float _distance = 10f;
    
    private Vector3 _moveDirection;
    private bool _isRotating;
    private float _rotationX = 0f;
    
    [SerializeField] private HudManager hudManager;
    private Transform orbitPivot; 

    #endregion

    #region Unity Methods
    private void Update()
    {
        if (isInOrbitMode)
            HandleOrbitMode();
        else
            HandleFreeMode();

        UpdateHUD();
    }
    #endregion

    #region Free Mode
    private void HandleFreeMode()
    {
        HandleMovement();
        HandleRotation();
        HandleSelection();
    }

    private void HandleMovement()
    {
        _moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _moveDirection = transform.TransformDirection(_moveDirection);
        characterController.Move(_moveDirection * (Time.deltaTime * speed));
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
            _isRotating = true;
        if (Input.GetMouseButtonUp(1))
            _isRotating = false;

        if (_isRotating)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;

            transform.RotateAround(transform.position, Vector3.up, mouseX);
            _rotationX = Mathf.Clamp(_rotationX - mouseY, -verticalLimit, verticalLimit);
            transform.localRotation = Quaternion.Euler(_rotationX, transform.localRotation.eulerAngles.y, 0);
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("Planet"))
            {
                currentPlanet = hit.transform.GetComponent<Planet>();
                hudManager.vectorField = hit.transform.GetComponent<VectorField>();
                
                InitializeHUD(); 
                
                isInOrbitMode = true;
            }
        }
    }
    #endregion

    #region Orbit Mode
    private void HandleOrbitMode()
    {
        if (allowMovement)
            ApproachPlanet();
        else
            OrbitPlanet();

        if (Input.GetKeyDown(KeyCode.E))
            ExitOrbitMode();
    }

    private void AttachToOrbit()
    {
        if (orbitPivot == null)
        {
            orbitPivot = new GameObject("OrbitPivot").transform;
        }

        orbitPivot.position = currentPlanet.transform.position;
        orbitPivot.parent = currentPlanet.transform; 

        transform.parent = orbitPivot; 
        transform.localPosition = new Vector3(0, 0, -_distance); 
        transform.LookAt(currentPlanet.transform);
    }

    private void ApproachPlanet()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentPlanet.transform.position, speed * Time.deltaTime);
        transform.LookAt(currentPlanet.transform);

        if (Vector3.Distance(transform.position, currentPlanet.transform.position) < 3f * currentPlanet.scale)
        {
            AttachToOrbit();
            allowMovement = false;
        }
    }

    private void OrbitPlanet()
    {
        transform.LookAt(currentPlanet.transform);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput != 0)
            transform.RotateAround(currentPlanet.transform.position, Vector3.up, horizontalInput * 50 * Time.deltaTime);

        if (verticalInput != 0)
            AdjustVerticalOrbit(verticalInput);

        HandleZoom();
    }

    private void AdjustVerticalOrbit(float verticalInput)
    {
        float newAngle = _verticalAngle + verticalInput * 50 * Time.deltaTime;
        if (newAngle > -verticalLimit && newAngle < verticalLimit)
        {
            _verticalAngle = newAngle;
            transform.RotateAround(currentPlanet.transform.position, transform.right, verticalInput * 50 * Time.deltaTime);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            _distance = Mathf.Clamp(_distance - scroll * 20, 1, 30);
            transform.position = currentPlanet.transform.position - transform.forward * _distance;
        }
    }

    private void ExitOrbitMode()
    {
        hudManager.SetHudVisibility(false);
        isInOrbitMode = false;
        allowMovement = true;
        transform.parent = null; 
    }
    #endregion

    #region HUD Updates

    private void InitializeHUD()
    {
        hudManager.UpdateValues(); 
        hudManager.SetPlanetname(currentPlanet.name);
        hudManager.SetMass(currentPlanet.mass);
        hudManager.SetHudVisibility(true);
    }
    private void UpdateHUD()
    {
        if (currentPlanet != null)
        {
            hudManager.SetVelocity(currentPlanet.velocity);
            hudManager.SetPosition(currentPlanet.position);
        }
    }
    #endregion
}
