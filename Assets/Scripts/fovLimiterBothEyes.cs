using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fovLimiterBothEyes : MonoBehaviour
{
    public GameObject[] limiters = new GameObject[6];

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }

            limiters[0].SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }

            limiters[1].SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }

            limiters[2].SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }


            limiters[3].SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }


            limiters[4].SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            foreach (GameObject fov in limiters)
            {
                fov.SetActive(false);
            }


            limiters[5].SetActive(true);
        }
    }
}