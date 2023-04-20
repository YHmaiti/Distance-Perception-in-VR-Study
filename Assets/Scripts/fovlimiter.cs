using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fovlimiter : MonoBehaviour
{
    public GameObject[] right = new GameObject[6];

    public GameObject[] left = new GameObject[6];
    public int currentFoV = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    public int SetFov(int f)
    {
        currentFoV = f;
        foreach (GameObject fov in right)
        {
            fov.SetActive(false);
        }

        foreach (GameObject fov in left)
        {
            fov.SetActive(false);
        }

        if (f > 0)
        {
            right[f - 1].SetActive(true);
            left[f - 1].SetActive(true);
        }

        return currentFoV;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetFov(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetFov(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetFov(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetFov(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetFov(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetFov(6);
        }
    }
}