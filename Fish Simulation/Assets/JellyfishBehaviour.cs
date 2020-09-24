using UnityEngine;
using System.Collections;

public class JellyfishBehaviour : MonoBehaviour {

    public GameObject goalPrefab;

    public float minSpeed = 0.01f;
    public float maxSpeed = 1f;
    float currentSpeed = 0.2f;
    Vector3 target;
    public string currentAction;
    private Flock thisFlock;

    public bool avoid = false;
    Vector3 globalGoalPosition;

    public float defaultImpulseTime;
    public float rotationDuration;
    float xOffset;
    float zOffset;
    float dt;
    float dt2;
    int isRotating;

    public float speed;
    // Use this for initialization
    void Start () {
        currentSpeed = 0.01f;
        zOffset = 0;
        xOffset = 0;
        target = transform.position;
        currentAction = "Start";
        globalGoalPosition = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        dt -= Time.deltaTime;
        if (dt < 0)
            isRotating = 0;

        dt2 -= Time.deltaTime;
        if (dt2 <= 0 && !avoid)
        {
            if (Vector3.Distance(transform.position, globalGoalPosition) > 50.0f)
            {
                GoBackToSpawn();
            }
            else if (Random.Range(0, 100) > 60)
                AddIndividualGoal();
            else
            {
                dt2 = defaultImpulseTime + Random.Range(-0.4f, 0.5f);
                if (Random.Range(0, 100) > 40)
                {
                    GetComponentInChildren<Animation>().Play();
                    currentSpeed = maxSpeed - Random.Range(-0.1f, 0.2f) * maxSpeed;
                }
                else
                    UpdateCurrentGoal(target + transform.right * Random.Range(-3, 4) + transform.forward * Random.Range(-3, 4), 2);
                Debug.Log("Pulse");
            }
                
        }

        transform.eulerAngles = new Vector3(transform.eulerAngles.x + xOffset * Time.deltaTime * isRotating, 0,
                                            transform.eulerAngles.z + zOffset * Time.deltaTime * isRotating);
        if (currentSpeed <= 0)
            currentSpeed = minSpeed;
        else
        {
            transform.Translate(0, 0, Time.deltaTime * currentSpeed);
            currentSpeed = currentSpeed - (Time.deltaTime*0.5f);
        }

    }

    private void UpdateCurrentGoal(Vector3 target, int rotPriority)
    {
        Vector3 TargetRotation = Quaternion.LookRotation(target - transform.position).eulerAngles;
        var newX = TargetRotation.x;
        var newZ = TargetRotation.z;

        var oldX = transform.eulerAngles.x;
        var oldZ = transform.eulerAngles.z;
        
        xOffset = -(oldX - newX);
        zOffset = -(oldZ - newZ);

        if (xOffset < -180)
            xOffset = 360 + xOffset;
        if (xOffset > 180)
            xOffset = -(360 - xOffset);

        if (zOffset < -180)
            zOffset = 360 + zOffset;
        if (zOffset > 180)
            zOffset = -(360 - zOffset);

        goalPrefab.transform.position = target;

        dt = rotationDuration;
        xOffset = xOffset / rotationDuration;
        zOffset = zOffset / rotationDuration;
        isRotating = 1;

        currentSpeed = maxSpeed;
        GetComponentInChildren<Animation>().Play();
    }

    private void AddIndividualGoal()
    {
        currentAction = "IndividualGoal";

        target = transform.position + transform.up + Random.Range(-1.0f, 1.0f) * transform.right + Random.Range(-1.0f, 1.0f) * transform.forward;
        //Debug.Log("INDIVIDUALGOAL: " + target);

        UpdateCurrentGoal(target, 1);
        dt2 = defaultImpulseTime + Random.Range(-0.1f, 0.3f);
    }

    private void GoBackToSpawn()
    {
        currentAction = "GoingBackToSpawn";
        target = globalGoalPosition;

        UpdateCurrentGoal(target, 2);
        dt2 = defaultImpulseTime + Random.Range(-0.1f, 0.3f);
    }
}
