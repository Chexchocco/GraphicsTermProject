using System.Drawing;
using UnityEngine;

public class Mirror : MonoBehaviour
{
    public GameObject Cat;
    private bool ModeOn=false;

    [Header("Mirror Settings")]
    public float MirrorSize;
    public float MirrorSpeed;
    public float MouseSpeed = 300.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.GetComponentInChildren<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        MirrorMode();
        MirrorMove();
    }

    void MirrorMode()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            GameManager.IsMirrorMode = true;
            GameManager.ScreenMove = false;
            GameManager.CatMove = false;

            if (!ModeOn)
            {
                ModeOn = true;
                gameObject.GetComponentInChildren<Renderer>().enabled = true;
                Vector3 spawnPos = Cat.transform.position + Cat.transform.forward * 10;
                transform.position = spawnPos;
                transform.rotation = Quaternion.LookRotation(Cat.transform.forward);
            }
        }
        else
        {
            GameManager.IsMirrorMode = false;
            GameManager.ScreenMove = true;
            GameManager.CatMove = true;
            gameObject.GetComponentInChildren<Renderer>().enabled = false;
            ModeOn = false;
        }

    }

    void MirrorMove()
    {
        if (GameManager.IsMirrorMode)
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSpeed * Time.deltaTime;

            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0;
            camRight.y = 0;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveDir += camForward;
            if (Input.GetKey(KeyCode.S)) moveDir -= camForward;
            if (Input.GetKey(KeyCode.A)) moveDir -= camRight;
            if (Input.GetKey(KeyCode.D)) moveDir += camRight;

            transform.position += moveDir * MirrorSpeed * Time.deltaTime;

            //if (Input.GetKey(KeyCode.W))
            //{
            //    transform.Translate(Vector3.forward * MirrorSpeed * Time.deltaTime);
            //}
            //if (Input.GetKey(KeyCode.S))
            //{
            //    transform.Translate(Vector3.back * MirrorSpeed * Time.deltaTime);
            //}
            //if (Input.GetKey(KeyCode.A))
            //{
            //    transform.Translate(Vector3.left * MirrorSpeed * Time.deltaTime);
            //}
            //if (Input.GetKey(KeyCode.D))
            //{
            //    transform.Translate(Vector3.right * MirrorSpeed * Time.deltaTime);
            //}
            if (Input.GetMouseButton(1))
            {
                transform.Rotate(Vector3.up * -mouseX);
                transform.Rotate(Vector3.right * mouseY);
            }
        }
    }
}
