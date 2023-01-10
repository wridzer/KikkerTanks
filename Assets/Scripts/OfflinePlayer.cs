using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflinePlayer : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float health;
    [SerializeField] private float minPower;
    [SerializeField] private float maxPower;
    [SerializeField] private float lineAlphaModifier;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject launchPoint;

    private LineRenderer lineRenderer;

    private Vector2 aimPos;
    private float lineAlphaBegin = 0;
    private float lineAlphaEnd = 0;
    private bool alphaGoingUp = true;
    private float power;

    public float Health { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Health = health;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 10;
        lineRenderer.endWidth = 1000f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, launchPoint.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lineRenderer.enabled = true;
        }
        if (Input.GetMouseButton(0))
        {
            lineRenderer.SetPosition(1, aimPos);
            power = GetPower();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Shoot(power);
        }
    }

    private float GetPower()
    {
        if (alphaGoingUp)
        {
            if (lineAlphaBegin < 1)
            {
                lineAlphaBegin += lineAlphaModifier;
            }
            else
            {
                lineAlphaEnd += lineAlphaModifier;
                if (lineAlphaEnd > 1) { alphaGoingUp = false; }
            }
        }
        if (!alphaGoingUp)
        {
            if (lineAlphaEnd > 0)
            {
                lineAlphaEnd -= lineAlphaModifier;
            }
            else
            {
                lineAlphaBegin -= lineAlphaModifier;
                if (lineAlphaBegin < 0) { alphaGoingUp = true; }
            }
        }
        lineRenderer.startColor = new Color(255, 255, 0, lineAlphaBegin);
        lineRenderer.endColor = new Color(255, 0, 0, lineAlphaEnd);

        return (lineAlphaBegin + lineAlphaEnd) / 2 * (maxPower - minPower);
    }

    void Shoot(float _power)
    {
        lineRenderer.enabled = false;
        lineAlphaBegin = 0;
        lineAlphaEnd = 0;
        Vector2 newRot = (Input.mousePosition - launchPoint.transform.position).normalized;
        GameObject rocket = Instantiate(
            bulletPrefab,
            launchPoint.transform.position,
            Quaternion.identity,
            this.transform
            );
        rocket.GetComponent<Rigidbody2D>().AddForce(newRot * _power);
    }

    public void TakeDamage(float _Damage)
    {
        Health -= _Damage;
        Debug.Log(Health);
        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
