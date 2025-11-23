using UnityEngine;
using UnityEngine.InputSystem;   

public class KartController : MonoBehaviour
{
    //Driving
    private Vector2 moveInput;
    private Vector3 MoveForce;
    public float MoveSpeed = 50;
    public float MaxSpeed = 15;
    public float Drag = 0.98f;
    public float SteerAngle = 20;
    public float Traction = 1;
    
    
    //Drifting
    public bool isDrifting = false;                
    public float DriftSlideAmount = 0.35f;        
    public float BoostStrength = 20f;            
    public float MaxBoostCharge = 2f;             
    private float boostCharge = 0f;  
    public float DriftSteerMultiplier = 1.8f;    
    public float DriftSpeedMultiplier = 1.2f;     
    private float smoothSteering = 0f;
    [SerializeField] private float steerSmoothSpeed = 10f;
    
    //Animation
    public Animator Playeranim;

    void Start()
    {
        if (Playeranim == null) 
            Playeranim = GetComponentInChildren<Animator>(); 
    }
    public void OnMove(InputValue value)
    {
        
        moveInput = value.Get<Vector2>();
    }
    


    void Update()
    {
        bool driftPressed = false;
        if (Keyboard.current != null)
        {
            driftPressed = Keyboard.current.leftShiftKey.isPressed;
        }
        

        // Handle when drifting starts and ends
        if (driftPressed && moveInput.y > 0.1f)
        {
            if (!isDrifting)
            {
                isDrifting = true;
                boostCharge = 0f;   
            }
        }
        // release on this frame
        if (!driftPressed && isDrifting)
        {
            // apply boost
            float boostAmount = Mathf.Clamp(boostCharge, 0f, MaxBoostCharge);
            // obv in the forward direction
            MoveForce += transform.forward * BoostStrength * boostAmount;

            isDrifting = false;
            boostCharge = 0f;
        }

        // Moving
        //if drifting increase movespeed
        float currentMoveSpeed = isDrifting ? MoveSpeed * DriftSpeedMultiplier : MoveSpeed;
        MoveForce += transform.forward * currentMoveSpeed * moveInput.y * Time.deltaTime;

        transform.position += MoveForce * Time.deltaTime;

        //Steerring
        /*
         rotationAmount = steeringInput * velocityMagnitude * steerAngle * dt
         transform.Rotate(0, rotationAmount, 0)
        */
        // increase steer by multiplier
        float currentSteer = isDrifting ? SteerAngle * DriftSteerMultiplier : SteerAngle;
        transform.Rotate(Vector3.up, moveInput.x * MoveForce.magnitude * currentSteer * Time.deltaTime);
        
        // Drift
        if (isDrifting)
        {
            Vector3 slide = transform.right * moveInput.x * DriftSlideAmount;
            MoveForce += slide * Time.deltaTime;

            boostCharge += Time.deltaTime;   // ** charging mini-turbo **
        }

        // Drag
        MoveForce *= Drag;
        MoveForce = Vector3.ClampMagnitude(MoveForce, MaxSpeed);

        
        Debug.DrawRay(transform.position, MoveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.cyan);

        //Lerp
        
        float targetSteer = isDrifting ? moveInput.x : 0f;

        smoothSteering = Mathf.Lerp(smoothSteering, targetSteer, Time.deltaTime * steerSmoothSpeed);
        
        if (Playeranim != null)
        {
            Playeranim.SetBool("isSteering", Mathf.Abs(smoothSteering) > 0.05f);
            Playeranim.SetFloat("Steering", smoothSteering);
        }
        
    }
    
}
