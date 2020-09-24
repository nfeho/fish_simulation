using UnityEngine;
using System.Collections;

public class BigFishBehaviour : MonoBehaviour {

    public GameObject goalPrefab;

    public float minSpeed = 4.0f;
    public float maxSpeed = 15.0f;
    float currentSpeed = 4.0f;
    Vector3 target;
    public string currentAction;

    public bool avoid = false;
    Vector3 globalGoalPosition;

    public float defaultUpdateTime;

    public float defaultAnimationSpeed;

    Collider nearbyPrey;

    float rotationDuration;
    float xOffset;
    float yOffset;
    float dt;
    float dt2;
    int isRotating;

    int speedFactor;
    int rotationFactor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("MediumFish"))
        {
            nearbyPrey = other;
        }
        else if (other.tag.Equals("SmallFish"))
        {
            // ignore
        }
        else
        {

            //Debug.Log("Entered: " + other.ToString());
            if (Vector3.Distance(other.transform.position, transform.position - transform.forward) > Vector3.Distance(other.transform.position, transform.position + transform.forward))
            {
                int sideIndex = (Vector3.Distance(other.transform.position, transform.position + transform.right) >
                                    Vector3.Distance(other.transform.position, transform.position - transform.right)) ? 1 : -1;
                target = transform.position + transform.right * sideIndex;
                speedFactor = 2;
                rotationFactor = 4;
                if (other.name.Contains("Floor"))
                {
                    target = target + new Vector3(0, 3.0f, 0);
                    speedFactor = 1;
                    rotationFactor = 4;
                }

            }
            else
            {
                target = transform.position + transform.forward + (transform.position - other.transform.position);
                speedFactor = 2;
                rotationFactor = 4;
                if (other.name.Contains("Floor"))
                {
                    target = target + new Vector3(0, 3.0f, 0);
                    speedFactor = 1;
                    rotationFactor = 4;
                }

            }


            UpdateCurrentGoal(target, rotationFactor, speedFactor);
            dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.25f;
            avoid = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("MediumFish") && other == nearbyPrey && nearbyPrey != null)
            nearbyPrey = null;
        else
            avoid = false;

    }

    public void setParent(Flock parent)
    {
        minSpeed = parent.minimalSpeed;
        maxSpeed = parent.maximalSpeed;
        currentSpeed = 2 * minSpeed;
    }

    // Use this for initialization
    void Start () {
        currentSpeed = 2 * minSpeed;
        yOffset = 0;
        xOffset = 0;
        target = transform.position;
        currentAction = "Start";
        globalGoalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        dt2 -= Time.deltaTime;
        if (dt2 <= 0 && !avoid)
        {
            if (Vector3.Distance(transform.position, globalGoalPosition) > 110.0f)
            {
                GoBackToSpawn();
            } else
            {
                var random = Random.Range(0, 100);
                if (random < 85)
                    AddIndividualGoal();
                else if (random < 90)
                    TurnAround();
                else if (random < 91)
                    GoBackToSpawn();
                else
                    AttackMediumSizedFish();
            }

        }

        dt -= Time.deltaTime;
        if (dt < 0)
            isRotating = 0;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x + xOffset * Time.deltaTime * isRotating,
                                            transform.eulerAngles.y + yOffset * Time.deltaTime * isRotating, 0);
        transform.Translate(0, 0, Time.deltaTime * currentSpeed);
    }


    private void UpdateCurrentGoal(Vector3 target, int rotPriority, int spdPriority)
    {
        //
        //Debug.Log("Target: " + target.ToString() + " | RotatePri: " + rotPriority + " | SpeedPri: " + spdPriority);
        Vector3 TargetRotation = Quaternion.LookRotation(target - transform.position).eulerAngles;
        var newX = TargetRotation.x;
        var newY = TargetRotation.y;

        var oldX = transform.eulerAngles.x;
        var oldY = transform.eulerAngles.y;

        xOffset = -(oldX - newX);
        yOffset = -(oldY - newY);

        if (xOffset < -180)
            xOffset = 360 + xOffset;
        if (xOffset > 180)
            xOffset = -(360 - xOffset);

        if (yOffset < -180)
            yOffset = 360 + yOffset;
        if (yOffset > 180)
            yOffset = -(360 - yOffset);

        //Debug.Log("Offset is: " + xOffset + " | " + yOffset);
        switch (rotPriority)
        {
            case 4:
                rotationDuration = Random.Range(0.25f, 0.5f);
                break;
            case 3:
                rotationDuration = Random.Range(0.5f, 0.8f);
                break;
            case 2:
                rotationDuration = Random.Range(0.9f, 1.3f);
                break;
            case 1:
                rotationDuration = Random.Range(1.5f, 2.0f);
                break;
            default:
                rotationDuration = 1.0f;
                break;
        }
        switch (spdPriority)
        {
            case 4:
                currentSpeed = Random.Range(0.9f * maxSpeed, maxSpeed);
                break;
            case 3:
                currentSpeed = Random.Range(0.55f * maxSpeed, 0.8f * maxSpeed);
                break;
            case 2:
                currentSpeed = Random.Range(minSpeed + (0.2f * (maxSpeed - minSpeed)), minSpeed + (0.45f * (maxSpeed - minSpeed)));
                break;
            case 1:
                currentSpeed = Random.Range(minSpeed, minSpeed + (0.08f * (maxSpeed - minSpeed)));
                break;
            default:
                currentSpeed = Random.Range(1.0f, 2.0f);
                break;
        }

        GetComponentInChildren<Animation>()["Movement"].speed = currentSpeed * defaultAnimationSpeed;
        dt = rotationDuration;
        xOffset = xOffset / rotationDuration;
        yOffset = yOffset / rotationDuration;
        isRotating = 1;

    }

    private void AttackMediumSizedFish()
    {
        currentAction = "Attack";
        if (nearbyPrey != null)
        {
            target = (transform.forward + (nearbyPrey.transform.position - transform.position)) + transform.position;
            rotationFactor = 3;
            speedFactor = Random.Range(3, 5);
            UpdateCurrentGoal(target, rotationFactor, speedFactor);
        }
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f);

    }

    private void TurnAround()
    {
        currentAction = "TurnAround";
        target = transform.position - (transform.forward);
        rotationFactor = 1;
        speedFactor = 2;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.3f;
    }

    private void GoBackToSpawn()
    {
        currentAction = "GoingBackToSpawn";
        target = globalGoalPosition;
        rotationFactor = 1;
        speedFactor = 2;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 5.0f;
    }

    private void AddIndividualGoal()
    {
        currentAction = "IndividualGoal";
        var random = Random.Range(0, 100);
        if (random < 75)
            target = transform.position + (Random.Range(0.8f, 1.5f) * transform.forward + Random.Range(-1.0f, 1.0f) * transform.right);
        else
            target = transform.position + (Random.Range(0.8f, 1.5f) * transform.forward + Random.Range(-1.0f, 1.0f) * transform.right + Random.Range(-0.3f, 0.3f) * transform.up);

        //Debug.Log("INDIVIDUALGOAL: " + target);
        rotationFactor = 2;
        speedFactor = 2;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f);
    }
}
