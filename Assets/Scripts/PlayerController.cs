using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    CharacterController controller;

    public float speed = 3.0f;
    public float runSpeed = 2.0f;

    public float acceleration = 1.0f;
    public float drag = 0.90f;
    public float initvel = 0.1f;

    public Vector3 velocity;
    public Vector3 gravity;
    public AnimationCurve curve;


    public float sensitivity = 2.0f;
    Vector2 viewXY;
    Transform isoTarget;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        isoTarget = GameObject.Find("CameraTarget").transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = (transform.position - Camera.main.transform.position);
        forward.y = 0;
        forward.Normalize();

        Vector3 right = Vector3.Cross(forward, Vector3.up);
        right.Normalize();
        Vector3 wishdir = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            wishdir += forward;
        if (Input.GetKey(KeyCode.S))
            wishdir += -forward;
        if (Input.GetKey(KeyCode.A))
            wishdir += right;
        if (Input.GetKey(KeyCode.D))
            wishdir += -right;

        //if (wishdir.magnitude <= 0.05f)
        //{
        //    velocity -= (velocity.normalized * drag) * Time.deltaTime;
        //}

        //float dragX = Vector3.Dot(velocity.normalized, Vector3.right) < 0 ? 0 : (velocity * drag * Time.deltaTime).x;
        //float dragY = Vector3.Dot(velocity.normalized, Vector3.right) < 0 ? 0 : (velocity * drag * Time.deltaTime).y;
        //velocity.x -= dragX;
        //velocity.y -= dragY;
        //if (Vector3.Dot(velocity.normalized, forward) < 0)
        //    velocity.x = velocity.x * (1 - Time.deltaTime * drag);

        //if (Vector3.Dot(velocity.normalized, right) < 0)
        //    velocity.z = velocity.z * (1 - Time.deltaTime * drag);

        velocity = velocity * (1 - Time.deltaTime * drag);
        //else if (velocity.magnitude < 0.1f)
        //{
        //    velocity = wishdir.normalized * initvel;
        //}

        wishdir.Normalize();
        //float mult = 1.0f;
        //if (Vector3.Dot(wishdir.normalized, velocity.normalized) < -0.95f)
        //{
        //    mult = 4.0f;
        //}

        velocity += wishdir * acceleration /** mult*/ * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, speed);

        if (controller.isGrounded)
            gravity = Vector3.zero;
        else
            gravity += Vector3.down * 9.81f * Time.deltaTime;

        float runSpeedMult = 1.0f;

        if (Input.GetKey(KeyCode.LeftShift))
            runSpeedMult = runSpeed;

        controller.Move(gravity * Time.deltaTime + (velocity * runSpeedMult) * Time.deltaTime);
        PositionCamera();
        CameraRotation();
    }

    void PositionCamera()
    {
        isoTarget.position = transform.position + transform.up;

        Ray ray = new Ray(isoTarget.position, -isoTarget.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10.0f, LayerMask.GetMask("Default")))
        {
            Camera.main.transform.position = hit.point;
        }
        else
        {
            Camera.main.transform.localPosition = Vector3.forward * -22.0f;
        }
    }
    public void CameraRotation()
    {
        if (Input.GetMouseButton(1))
        {
            viewXY.x += Input.GetAxis("Mouse X") * sensitivity;
            viewXY.y -= Input.GetAxis("Mouse Y") * sensitivity;
        }
        
        isoTarget.rotation = Quaternion.Euler(viewXY.y, viewXY.x, 0);
    }
}
