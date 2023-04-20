using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowPointer : MonoBehaviour
{
    public GameObject startLocation;
    public float distanceThreshold = 0.1f;
    public float angleThreshold = 10.0f;
    public GameObject camera;
    public GameObject cameraRight;
    public GameObject cameraRig;
    public GameObject locations;
    public GameObject[] targets = new GameObject[4];
    private Transform[] clutters;
    private GameObject currentTarget;
    private Transform currentClutter;
    public int currentTargetIndex;
    public int currentClutterIndex;
    public GameObject targetArrow;
    public GameObject targetArrowMatch;
    public GameObject world;
    private GameObject boundries;
    private RandomLocation boundriesRandom;

    private Vector3 pointingDirection;
    public Material blackSky;
    public Material whiteSky;

    private bool isNavigateMode;

    private bool isDark = false;
    private bool isWalkingMode = true;

    private int participantID = -1;

    // fill this with the path where you want to save the data
    private string basePath = @""; 
    private string participantPath = "";

    private int currentFoV;

    private String[] trials;

    public int currentTrial = -1;
//    private int totalTrial = 72;

    private Mode currentMode = Mode.returning;
    private bool isFinished;
    private Material defaultSkyBox;

    int trashCounter = 0;
    string[] allTrials;

    // add two variables to hold the controller left and controller right 
    private GameObject controllerLeft;
    private GameObject controllerRight;

    // declare a gameobject to store the canvas and remove it 
    private GameObject canvasObjectHolder;

    public enum Mode
    {
        looking,
        walking,
        returning
    }

    private void Awake()
    {
        _soundGenerator = camera.GetComponent<SoundGenerator>();
    }

    static Mode NextMode(Mode value)
    {
        int next = ((int) value + 1) % Enum.GetNames(typeof(Mode)).Length;
        return (Mode) next;
    }

    public static Mode PreviousMode(Mode value)
    {
        int prev = ((int) value - 1) % Enum.GetNames(typeof(Mode)).Length;
        return (Mode) prev;
    }

    // Start is called before the first frame update
    void Start()
    {
        boundries = GameObject.Find("Boundries");
        boundriesRandom = boundries.GetComponent<RandomLocation>();
        currentTarget = targets[0];
        defaultSkyBox = RenderSettings.skybox;
        //---------------------------------
        //currentMode = Mode.returning;
        //RenderSettings.skybox = blackSky;
        //world.SetActive(false);
        //locations.SetActive(false);
        //isNavigateMode = true;
        //--------------------------------

        // Get participant id
        string contents = File.ReadAllText(basePath + "participantid.txt");
        participantID = int.Parse(contents);
        participantID++;
        File.WriteAllText(basePath + "participantid.txt", participantID.ToString());

        // create this directory set up for every participant, I will throw here the data also 
        Directory.CreateDirectory(basePath + participantID);
        participantPath = basePath + participantID + "\\";

        // Get participant trials
        string trialsRaw = File.ReadAllText(basePath + "trials.txt");
        Debug.Log(trialsRaw);
        allTrials = trialsRaw.Split('\n');
        //trials = allTrials[participantID].Split(',');
        print(allTrials);
        print("CONDITIONS :");
        for(int i = 0; i < allTrials.Length; i++)
        {
            Debug.Log(allTrials[i]);
        }
        
        // Switch to "returning" mode 
         int childCount = world.transform.childCount;
         clutters = new Transform[childCount];
         for (int i = 0; i < childCount; ++i)
             clutters[i] = world.transform.GetChild(i);
        currentMode = Mode.returning;
        RenderSettings.skybox = blackSky;
        world.SetActive(false);
        locations.SetActive(false);
        isNavigateMode = true;
        currentFoV = GameObject.Find("fovLimiter").GetComponent<fovlimiter>().currentFoV;
        SetFov(0); // this can be changed

        // fetch for the controller game objects in the active scene
        controllerLeft = GameObject.Find("Controller (left)");
        if (controllerLeft == null)
        {
            Debug.Log("Controller (left) not found");
        }
        controllerRight = GameObject.Find("Controller (right)");
        if (controllerRight == null)
        {
            Debug.Log("Controller (right) not found");
        }

        canvasObjectHolder = GameObject.Find("Canvas");

        Debug.Log("we reached the end of the Start ---------------");

    }

    private ArrayList walkedPath;
    private ArrayList walkedHeadsetDirection;

    private int sampling;
    private SoundGenerator _soundGenerator;

    void FixedUpdate()
    {
        if (currentMode == Mode.walking)
        {
            if (sampling > 15)
            {
                walkedPath.Add(camera.transform.position);
                walkedHeadsetDirection.Add(camera.transform.forward);
                sampling = 0;
            }
            else
            {
                sampling += 1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // first line of change 
        //SetFov(0); // brute force this to have the largest one enabled

        // always turn off the canvas
        // canvasObjectHolder.SetActive(false);

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (isFinished)
            {
                RenderSettings.skybox = whiteSky;
                world.SetActive(false);
                locations.SetActive(false);
                return;
            }

            currentMode = NextMode(currentMode);
            Debug.Log(currentMode);
            if (currentMode == Mode.looking)
            {
                boundriesRandom.randomizeLocationAndRotation();
                _soundGenerator.playOk();
                RenderSettings.skybox = defaultSkyBox;
                isNavigateMode = false;
                NextTrial();
                world.SetActive(true);
                locations.SetActive(true);
                targetArrow.SetActive(false);
                targetArrowMatch.SetActive(false);
                transform.Find("arrow").gameObject.SetActive(false);
                world.SetActive(true);
            }
            else if (currentMode == Mode.walking)
            {
                _soundGenerator.playGo();
                walkedPath = new ArrayList();
                walkedHeadsetDirection = new ArrayList();
                isNavigateMode = false;
                RenderSettings.skybox = blackSky;
                world.SetActive(false);
                locations.SetActive(false);
            }
            else if (currentMode == Mode.returning)
            {
                _soundGenerator.playDone();
                isNavigateMode = true;
                currentFoV = GameObject.Find("fovLimiter").GetComponent<fovlimiter>().currentFoV;

//                GameObject.Find("TestObject").transform.position = projectedPosition;
                var currentTargetPosition = currentTarget.transform.position;
                float distance = GetProjectedDistance(
                    startLocation.transform.position,
                    currentTarget.transform.position,
                    camera.transform.position);

                // get the distance between the start position and the controller left and right
                float distanceControllerLeft = GetProjectedDistance(startLocation.transform.position, currentTarget.transform.position, controllerLeft.transform.position);
                float distanceControllerRight = GetProjectedDistance(startLocation.transform.position, currentTarget.transform.position, controllerRight.transform.position);
                //                Debug.Log(Vector3.Distance(projectedPosition, currentTargetPosition));
                AppendLocation(
                    SceneManager.GetActiveScene().name,
                    currentTarget.name,
                    currentTargetPosition,
                    camera.transform.position,
                    startLocation.transform.position,
                    currentClutterIndex,
                    currentTargetIndex,
                    distance
                );

                // add a function that will store and write the controller parcour/distance
                // to a file
                AppendControllerParcour(SceneManager.GetActiveScene().name, currentTarget.name, currentTargetPosition, controllerLeft.transform.position,
                    controllerRight.transform.position, startLocation.transform.position, currentClutterIndex, currentTargetIndex, distanceControllerLeft, distanceControllerRight);
                
                /*AppendWalkedPath();
                AppendWalkedHeadsetDirection();*/
                
                sampling = 0;
                // SetFov(0);
            }
        }


        if (isWalkingMode)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                SetTarget(0);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                SetTarget(1);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                SetTarget(2);
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                SetTarget(3);
            }
        }

        // redirect / reset the position of the user
        if (Input.GetKeyDown(KeyCode.R))
        {
            cameraRig.transform.position = new Vector3(
                startLocation.transform.position.x - (camera.transform.position.x - cameraRig.transform.position.x),
                cameraRig.transform.position.y,
                startLocation.transform.position.z - (camera.transform.position.z - cameraRig.transform.position.z));
        }


        if (isNavigateMode)
        {
            gameObject.transform.position = new Vector3(camera.transform.position.x, gameObject.transform.position.y,
                camera.transform.position.z);
            pointingDirection = startLocation.transform.position - gameObject.transform.position;
            gameObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0),
                new Vector3(pointingDirection.x, 0, pointingDirection.z));
            if (Vector3.Distance(toFloorPlane(startLocation.transform.position),
                    toFloorPlane(gameObject.transform.position)) < distanceThreshold)
            {
                targetArrow.SetActive(true);
                pointingDirection = quaternionToLookDirection(camera.transform.rotation);
                pointingDirection = new Vector3(pointingDirection.x, 0, pointingDirection.z);
                gameObject.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), pointingDirection);
                Debug.Log(pointingDirection);
                Debug.Log(quaternionToLookDirection(targetArrow.transform.rotation));
                Debug.Log(Vector3.Angle(pointingDirection,
                    quaternionToLookDirection(targetArrow.gameObject.transform.rotation)));
                if (Mathf.Abs(Vector3.Angle(pointingDirection,
                                  quaternionToLookDirection(targetArrow.gameObject.transform.rotation)) - 180) <
                    angleThreshold)
                {
                    transform.Find("arrow").gameObject.SetActive(false);
                    targetArrowMatch.SetActive(true);
                }
                else
                {
                    transform.Find("arrow").gameObject.SetActive(true);
                    targetArrowMatch.SetActive(false);
                }
            }
            else
            {
                targetArrow.SetActive(false);
                targetArrowMatch.SetActive(false);
                transform.Find("arrow").gameObject.SetActive(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {

            SetFov(6);
        }
    }

    public static float GetProjectedDistance(Vector3 origin, Vector3 target, Vector3 rawPosition)
    {
        Vector3 projectedPosition = NearestPointOnLine(origin,
            (origin - target).normalized, rawPosition);
        var distance = Vector3.Distance(projectedPosition, target);
        var targetFromOrigin = Vector3.Distance(origin, target);
        // Debug.Log("targetFromOrigin " + targetFromOrigin);
        var positionFromOrigin = Vector3.Distance(origin, projectedPosition);
        // Debug.Log("positionFromOrigin " + positionFromOrigin);

        if (positionFromOrigin < targetFromOrigin)
        {
            return -distance;
        }

        return distance;
    }

    private void NextTrial()
    {
        currentTrial += 1;

        String[] setOfConditions = allTrials[currentTrial].Split(',');

        // this needs to be adjusted as is needed to accomodate the order of conditions
        if (currentTrial < allTrials.Length)
        {
            //int clutter = allTrials[currentTrial][0] - '0';
            //int target = allTrials[currentTrial][1] - '0';
            /* This needs to be changed 
             * based on the conditions txt*/
            int fov = int.Parse(setOfConditions[2]);
            int clutter = int.Parse(setOfConditions[0]);
            int target = int.Parse(setOfConditions[1]);
            Debug.Log("ID: " + participantID +
                      " TRIAL: " + currentTrial + "/" + allTrials.Length +
                      " CLUTTER: " + clutter +
                      " TARGET: " + target +
                      " FOV: " + fov);
            SetFov(fov);
            SetTarget(target - 1);
            SetClutter(clutter);
          ;
        }
        else
        {
            RenderSettings.skybox = whiteSky;
            world.SetActive(false);
            locations.SetActive(false);
            isFinished = true;
        }
    }

    //private void PrevTrial()
    //{
    //    currentTrial -= 1;
    //    int fov = trials[currentTrial][0] - '0';
    //    int target = trials[currentTrial][1] - '0';
    //    SetFov(fov);
    //    SetTarget(target - 1);
    //}

    private void SetFov(int f)
    {
        currentFoV = GameObject.Find("fovLimiter").GetComponent<fovlimiter>().SetFov(f);
    }

    private void SetTarget(int t)
    {
        currentTarget = targets[t];
        currentTargetIndex = t + 1;
        for (var i = 0; i < targets.Length; i++)
        {
            targets[i].SetActive(i == t);
        }
    }

    private void SetClutter(int t)
    {
        //t-=1;
        currentClutter = clutters[t];
        currentClutterIndex = t;
        if(t != 0)
            for (var i = 0; i < clutters.Length; i++)
            {
                // clutters[i].gameObject.SetActive(true);
                if(i == 2)
                     clutters[2].gameObject.SetActive(true);
                else
                    clutters[i].gameObject.SetActive(false);

            }
        else
        {
            clutters[0].gameObject.SetActive(true);
            clutters[2].gameObject.SetActive(false);
            clutters[1].gameObject.SetActive(false);
        }
    }


    public static Vector3 toFloorPlane(Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }

    Vector3 quaternionToLookDirection(Quaternion input)
    {
        return input * Vector3.forward;
    }

    public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
    {
        lineDir.Normalize(); //this needs to be a unit vector
        var v = pnt - linePnt;
        var d = Vector3.Dot(v, lineDir);
        return linePnt + lineDir * d;
    }

    string CommaSeparate(params object[] args)
    {
        string output = "";
        foreach (object arg in args)
        {
            output += "\"" + arg.ToString() + "\",";
        }

        return output;
    }
 
    private void AppendLocation(
        string envName,
        string targetName,
        Vector3 targetLocation,
        Vector3 userLocation,
        Vector3 startLocation,
        int clutterLVL,
        int target, float distance)
    {
        string txt = CommaSeparate(DateTime.Now, envName, targetName, targetLocation, userLocation, startLocation, distance, clutterLVL, target);
        
        Debug.Log("Current Walked Parcour: " + txt);
        File.AppendAllText(participantPath + participantID.ToString() + ".csv",
            txt + Environment.NewLine);
    }

    // store the controller left and right parcours:
    private void AppendControllerParcour(string envName, string targetName, Vector3 targetLocation, Vector3 controllerLeftLocation, Vector3 controllerRightLocation,
        Vector3 startLocation, int clutterLVL, int target, float distanceControllerLeft, float distanceControllerRight)
    {
        string txt = CommaSeparate(DateTime.Now, envName, targetName, targetLocation, controllerLeftLocation, 
            controllerRightLocation, startLocation, distanceControllerLeft, distanceControllerRight, 
            clutterLVL, target
        );
        Debug.Log("Controller parcour: " + txt);
        File.AppendAllText(participantPath + "ControllerParcour_" + participantID.ToString() + ".csv",
            txt + Environment.NewLine);
    }
    
    private void AppendWalkedPath()
    {
        string txt = "";

        foreach (Vector3 p in walkedPath)
        {
            txt += (p.x - startLocation.transform.position.x) + "," +
                   (p.y - startLocation.transform.position.y) + "," +
                   (p.z - startLocation.transform.position.z) + "," +
                   Environment.NewLine;
        }

        File.WriteAllText(participantPath + "walkedPath_" + currentTrial + ".csv",
            txt);
    }

    private void AppendWalkedHeadsetDirection()
    {
        string txt = "";

        foreach (Vector3 p in walkedHeadsetDirection)
        {
            txt += p.x + "," + p.y + "," + p.z + "," + Environment.NewLine;
        }

        File.WriteAllText(participantPath + "walkedHeadsetDirection" + currentTrial + ".csv",
            txt);
    }
}