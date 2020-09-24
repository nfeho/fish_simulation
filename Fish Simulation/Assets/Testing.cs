using UnityEngine;
using System.Collections;

public class Testing : MonoBehaviour
{
    public float speed = 9.0f;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        transform.Translate(0, 0, Time.deltaTime * speed);
    }
}