using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Setting")]

    public float movementSpeed = 5f;

    public float mouseSensitivity = 2f;

    public float upDownRange = 90;

    public float jumpSpeed = 5;

    public float downSpeed = 5;

    public Transform cameraControlTransform;



    [Header("Cache")]

    [SerializeField]

    private Transform cameraTransform;

    private Vector3 speed;

    private float footStepTime;

    private float footSideStepTime;

    private float forwardSpeed;

    private float sideSpeed;



    private float rotLeftRight;

    private float rotUpDown;

    private float verticalRotation = 0f;

    private float verticalVelocity = 0f;

    private int pressedCount = 0;

    public bool isGround;



    private CharacterController cc;

    private Rigidbody rb;



    void Start()
    {

        cc = GetComponent<CharacterController>();

        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

    }



    void LateUpdate()
    {

        FPRotate();

        FPMove();

    }



    //Player의 x축, z축 움직임을 담당

    void FPMove()

    {

        forwardSpeed = Input.GetAxis("Vertical") * movementSpeed;

        sideSpeed = Input.GetAxis("Horizontal") * movementSpeed;



        if (cc.isGrounded)

            verticalVelocity = Physics.gravity.y;



        else if (!cc.isGrounded)

            verticalVelocity += Physics.gravity.y * Time.deltaTime;



        speed = new Vector3(sideSpeed, 0, forwardSpeed);

        speed = transform.rotation * speed;



        transform.position += speed * Time.deltaTime;

    }



    //Player의 회전을 담당

    void FPRotate()

    {

        /*

        #if UNITY_EDITOR || UNITY_STANDALONE



        //좌우 회전

        rotLeftRight = Input.GetAxis ("Mouse X") * mouseSensitivity;

        transform.Rotate (0f, rotLeftRight, 0f);

        //상하 회전

        verticalRotation -= Input.GetAxis ("Mouse Y") * mouseSensitivity;

        verticalRotation = Mathf.Clamp (verticalRotation, -upDownRange, upDownRange);

        cameraTransform.localRotation = Quaternion.Euler (verticalRotation, 0f, 0f);

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);



        #elif UNITY_ANDROID          //안드로이드 빌드시 회전

        */

        Vector3 newRotation = transform.eulerAngles;

        newRotation.y = cameraTransform.eulerAngles.y;

        newRotation.z = 0f;

        newRotation.x = 0f;

        transform.eulerAngles = newRotation;



        //    #endif



    }



    void Fall()
    {

        transform.parent = null;

    }

}
