using System.Collections;
using UnityEngine;


public class Explosion : MonoBehaviour
{
    [SerializeField] private float explosionTime = 0.5f;
    private float size;

    private void OnEnable()
    {
        size = transform.localScale.x;
    }

    private void Update()
    {
        size = transform.localScale.x;
        size -= explosionTime * size * Time.deltaTime;
        transform.localScale = new Vector3(size, size, size);

        if (size <= 0.1)
        {
            Destroy(gameObject);
        }
    }
}