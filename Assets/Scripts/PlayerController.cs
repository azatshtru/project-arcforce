﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float forceMagnitude = 6f;
    [SerializeField]
    private float gravitationalAcceleration = 200f;

    public GameObject bulletPrefab;

    private Rigidbody rb;
    private Plane plane;

    private Vector3 movement;
    private bool what;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        plane = new Plane(Vector3.forward, Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        float enter = 0.0f;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                movement = (hit - transform.position).normalized * forceMagnitude;
                what = true;
            }
        }
#elif UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                movement = (hit - transform.position).normalized * forceMagnitude;
                what = true;
            }
        }
#elif UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            if (plane.Raycast(ray, out enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                movement = (hit - transform.position).normalized * forceMagnitude;
                what = true;
            }
        }
#endif

    }

    private void FixedUpdate()
    {
        if (what)
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(-movement, ForceMode.VelocityChange);

            Shoot(movement.normalized);

            what = false;
        }

        rb.AddForce(Vector3.down * gravitationalAcceleration, ForceMode.Force);
    }

    private void Shoot(Vector3 lookDir)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, transform.position, Quaternion.LookRotation(lookDir, Vector3.forward));
        bulletGO.GetComponent<Bullet>().SetOwner(gameObject);

        Client.Instance.SendRay(new Ray(transform.position, lookDir));
    }
}
