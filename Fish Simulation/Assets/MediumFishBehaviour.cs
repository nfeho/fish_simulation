using UnityEngine;
using System.Collections;

public class MediumFishBehaviour : MonoBehaviour {

    // FLOCK PROPERTIES
    private float cohesionFactor;
    private float separationFactor;
    private float alignmentFactor;
    public float minSpeed = 0.1f;
    public float maxSpeed = 100.0f;
    private Flock thisFlock;

    // DEBUGGING ATTRIBUTES
    public GameObject goalPrefab;
    public string currentAction;

    // BEHAVIOUR BOOLS
    public bool awayFromFlock = false;
    bool predatorNearby = false;
    public bool avoid = false;
    public bool flocking = false;

    public float defaultAnimationSpeed;

    // FLOCKING ATTRIBUTES
    float currentSpeed = 1.0f;
    float neighbourDistance = 3.0f;
    float rotationDuration;
    Vector3 target;
    int speedFactor;
    int rotationFactor;
    Vector3 globalGoalPosition;

    // FOR UPDATE
    public float defaultUpdateTime;
    float xOffset;
    float yOffset;
    float dt;
    float dt2;
    int isRotating;

    // FLOCKING OPTIMALIZATIONS
    int flockUpdate;
    int flockMaxSize = 10;
    ArrayList nearbyFish = new ArrayList();
    Collider nearbyPrey;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.tag.Equals("BigFish") || other.tag.Equals("Player")) && !avoid)
        {
            RunAway(other);
        }
        else if (other.tag.Equals("SmallFish"))
        {
            nearbyPrey = other;
        }
        else
        {

            //Debug.Log("Entered: " + other.ToString());
            if (Vector3.Distance(other.transform.position, transform.position - transform.forward) > Vector3.Distance(other.transform.position, transform.position + transform.forward))
            {
                int sideIndex = (Vector3.Distance(other.transform.position, transform.position + transform.right) >
                                    Vector3.Distance(other.transform.position, transform.position - transform.right)) ? 1 : -1;
                target = transform.position + transform.right * sideIndex;
                rotationFactor = 3;
                speedFactor = 2;
                if (other.name.Contains("Floor"))
                {
                    rotationFactor = 4;
                    speedFactor = 1;
                    target = target + new Vector3(0, 5.0f, 0);
                }
                if (predatorNearby)
                    speedFactor = 4;

            }
            else
            {
                target = transform.position + transform.forward + (transform.position - other.transform.position);
                speedFactor = 2;
                rotationFactor = 3;
                if (other.name.Contains("Floor"))
                {
                    rotationFactor = 4;
                    speedFactor = 1;
                    target = target + new Vector3(0, 2.0f, 0);
                }
                if (predatorNearby)
                    speedFactor = 4;
            }


            UpdateCurrentGoal(target, rotationFactor, speedFactor);
            dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.25f;
            avoid = true;
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("BigFish"))
            predatorNearby = false;
        else if (other.tag.Equals("SmallFish") && other == nearbyPrey && nearbyPrey != null)
            nearbyPrey = null;
        else
            avoid = false;

    }

    public void setParent(Flock parent)
    {
        thisFlock = parent;
        minSpeed = parent.minimalSpeed;
        maxSpeed = parent.maximalSpeed;
        cohesionFactor = parent.cohesion;
        separationFactor = parent.separation;
        alignmentFactor = parent.alignment;
        globalGoalPosition = thisFlock.goalPosition;
        currentSpeed = (minSpeed + maxSpeed) / 2.2f;
        flockMaxSize = parent.flockMaxSize;
        neighbourDistance = 14.0f + 9 * cohesionFactor;
    }

    // Use this for initialization
    void Start () {

        // GET SPECIES VALUES FROM FLOCK
        /*cohesionFactor = thisFlock.cohesion;
        separationFactor = thisFlock.separation;
        alignmentFactor = thisFlock.alignment;

        minSpeed = thisFlock.minimalSpeed;
        maxSpeed = thisFlock.maximalSpeed;*/

        transform.Rotate(Random.Range(-10, 10), 2 * Random.Range(-45, 45), 0);
        currentSpeed = 5.0f;
        yOffset = 0;
        xOffset = 0;
        globalGoalPosition = transform.position;

        target = transform.position;
        neighbourDistance = 14.0f + 9 * cohesionFactor;

        flockUpdate = 0;
        currentAction = "Start";
    }

    // Update is called once per frame
    void Update () {
        dt2 -= Time.deltaTime;
        if (dt2 <= 0 && !avoid)
        {
            if (thisFlock != null)
                globalGoalPosition = thisFlock.goalPosition;
            if (Vector3.Distance(transform.position, globalGoalPosition) > 60.0f)
            {
                TurnBackToFlock();
            }
            else
            {
                awayFromFlock = false;
                if (flocking)
                    FlockBehaviour();
                else
                {
                    var random = Random.Range(0, 100);
                    if (random < 80)
                        AddIndividualGoal();
                    else if (random < 82)
                        TurnAround();
                    else if (random < 95)
                        FlockBehaviour();
                    else
                        AttackSmallSizedFish();
                }
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


    private void TurnAround()
    {
        currentAction = "TurnAround";
        target = transform.position - (transform.forward);
        rotationFactor = 2;
        speedFactor = 2;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.4f;
    }

    private void AttackSmallSizedFish()
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

    private void AddIndividualGoal()
    {
        currentAction = "IndividualGoal";
        var random = Random.Range(0, 100);
        if (random < 50)
            target = transform.position + (Random.Range(0.8f, 1.5f) * transform.forward + Random.Range(-1.0f, 1.0f) * transform.right);
        else
            target = transform.position + (Random.Range(0.8f, 1.5f) * transform.forward + Random.Range(-1.0f, 1.0f) * transform.right + Random.Range(-0.3f, 0.3f) * transform.up);

        //Debug.Log("INDIVIDUALGOAL: " + target);
        rotationFactor = 2;
        speedFactor = 2;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f);
    }

    private void TurnBackToFlock()
    {
        currentAction = "TurningBackToFlock";
        awayFromFlock = true;
        target = globalGoalPosition;
        rotationFactor = 3;
        speedFactor = Random.Range(2, 4);

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.75f;
    }

    public void RunAway(Collider other)
    {

        var zAxis = 0;
        var xAxis = 0;
        var yAxis = 0;
        Vector3 thisPos = transform.position;
        Vector3 otherPos = other.transform.position;
        var maxScale = Mathf.Max(other.gameObject.transform.lossyScale.x, other.gameObject.transform.lossyScale.y, other.gameObject.transform.lossyScale.z);
        Vector3 front = otherPos + other.transform.forward * maxScale;
        Vector3 back = otherPos - other.transform.forward * maxScale;
        Vector3 right = otherPos + other.transform.right * maxScale;
        Vector3 left = otherPos - other.transform.right * maxScale;
        Vector3 up = otherPos + other.transform.up * maxScale;
        Vector3 down = otherPos - other.transform.up * maxScale;
        /*
        Debug.Log("front " + front);
        Debug.Log("back " + back);
        Debug.Log("right " + right);
        Debug.Log("left " + left);
        Debug.Log("up " + up);
        Debug.Log("down " + down);
        */
        zAxis = Vector3.Distance(thisPos, front) >= Vector3.Distance(thisPos, back) ? -1 : 1;
        yAxis = Vector3.Distance(thisPos, right) >= Vector3.Distance(thisPos, left) ? -1 : 1;
        xAxis = Vector3.Distance(thisPos, up) >= Vector3.Distance(thisPos, down) ? -1 : 1;
        /*
        Debug.Log("zaxis " + zAxis);
        Debug.Log("yaxis " + yAxis);
        Debug.Log("xasxi " + xAxis);
        */

        var zRatio = Mathf.Max(Vector3.Distance(thisPos, front) / (Vector3.Distance(thisPos, front) + Vector3.Distance(thisPos, back)),
                               Vector3.Distance(thisPos, back) / (Vector3.Distance(thisPos, front) + Vector3.Distance(thisPos, back)));
        var yRatio = Mathf.Max(Vector3.Distance(thisPos, right) / (Vector3.Distance(thisPos, right) + Vector3.Distance(thisPos, left)),
                               Vector3.Distance(thisPos, left) / (Vector3.Distance(thisPos, right) + Vector3.Distance(thisPos, left)));
        var xRatio = Mathf.Max(Vector3.Distance(thisPos, up) / (Vector3.Distance(thisPos, up) + Vector3.Distance(thisPos, down)),
                               Vector3.Distance(thisPos, down) / (Vector3.Distance(thisPos, up) + Vector3.Distance(thisPos, down)));
        Vector3 target = Vector3.zero;
        /*
        Debug.Log("zratio " + zRatio);
        Debug.Log("yratio " + yRatio);
        Debug.Log("xratio " + xRatio);
        */
        if (zAxis == -1)
        {
            target = (back + otherPos) +
                     (yAxis * (right - otherPos)) * (1 - yRatio + 0.4f) +
                     (xAxis * (up - otherPos)) * (1 - xRatio + 0.4f);
        }
        else
        {
            target = (-1 * zAxis * (front - otherPos)) * (1 - zRatio + 0.4f) +
                     (yAxis * (right - otherPos)) * 3 * (1 - yRatio + 0.4f) +
                     (xAxis * (up - otherPos)) * 2 * (1 - xRatio + 0.4f) + otherPos;
        }
        /*
        Debug.Log("TARGETZ: -1*1 * " + front + " - " + otherPos + " + " + otherPos + " * (1 - " + zRatio + " + 0.4f");
        Debug.Log("TARGETY: " + yAxis + " * " + right + " - " + otherPos + " + " + otherPos + " * 3 * (1 - " + yRatio + " + 0.4f");
        Debug.Log("TARGETY: " + xAxis + " * " + up + " - " + otherPos + " + " + otherPos + " * 3 * (1 - " + xRatio + " + 0.4f");
        */

        currentAction = "RunAway";
        predatorNearby = true;
        rotationFactor = 4;
        speedFactor = 4;

        //Debug.Log("Target:" + target.ToString());
        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 2.5f;

    }


    void FlockBehaviour()
    {
        if (thisFlock == null)
            return;
        currentAction = "Flocking";
        Vector3 averagePosition = Vector3.zero;
        Vector3 averageHeading = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        globalGoalPosition = thisFlock.goalPosition;
        float dist;
        int numberOfNerabyAlignmentFish = 0;

        if (flockUpdate % 3 == 0)
        {
            nearbyFish.Clear();
            GameObject[] otherFish;
            otherFish = thisFlock.flockFish;
            foreach (GameObject go in otherFish)
            {
                if (go != this.gameObject)
                {
                    dist = Vector3.Distance(this.transform.position, go.transform.position);
                    if (dist <= neighbourDistance && nearbyFish.Count <= flockMaxSize)
                    {
                        nearbyFish.Add(go);
                        averagePosition += go.transform.position;
                        if (3 * dist <= (1.5f + alignmentFactor) * neighbourDistance)
                        {
                            numberOfNerabyAlignmentFish++;
                            averageHeading += go.transform.forward;
                        }

                        if (dist < (separationFactor * 3.0f) + 6.0f)
                        {
                            vAvoid = vAvoid + (this.transform.position - go.transform.position);
                        }

                    }
                }
            }
            if (nearbyFish.Count > 0)
            {
                //Debug.Log("AP:" + averagePosition + " AH:" + averageHeading + " GP:" + globalGoalPosition + " CNT:" + nearbyFish.Count);
                averagePosition = averagePosition / nearbyFish.Count;
                if (numberOfNerabyAlignmentFish > 0)
                {
                    averageHeading = averageHeading / numberOfNerabyAlignmentFish;
                    target = ((averagePosition - this.transform.position) + (Vector3.Magnitude(averagePosition - this.transform.position) /
                              Vector3.Magnitude(averageHeading)) * (averageHeading) + this.transform.position) + vAvoid * (separationFactor / 2 + 1.0f);
                }
                else
                {
                    target = (averagePosition - this.transform.position) + globalGoalPosition + vAvoid * (separationFactor / 2 + 1.0f);
                }

                //target = (averagePosition + averageHeading + vAvoid + globalGoalPosition);
                rotationFactor = 1;
                speedFactor = 2;
            }
            else
            {
                dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.3f;
                return;
            }
            flocking = true;

        }
        else
        {
            foreach (GameObject go in nearbyFish.ToArray())
            {
                dist = Vector3.Distance(this.transform.position, go.transform.position);
                if (dist <= neighbourDistance)
                {
                    averagePosition += go.transform.position;
                    if (3 * dist <= (1.5f + alignmentFactor) * neighbourDistance)
                    {
                        numberOfNerabyAlignmentFish++;
                        averageHeading += go.transform.forward;
                    }

                    if (dist < (separationFactor * 3.0f) + 6.0f)
                    {
                        vAvoid = vAvoid + (this.transform.position - go.transform.position);
                    }

                }
                else
                {
                    nearbyFish.Remove(go);
                }
            }
            if (nearbyFish.Count > 0)
            {
                //Debug.Log("AP:" + averagePosition + " AH:" + averageHeading + " AV:" + vAvoid + " GP:" + globalGoalPosition + " CNT:" + nearbyFish.Count);
                averagePosition = averagePosition / nearbyFish.Count;
                if (numberOfNerabyAlignmentFish > 0)
                {
                    averageHeading = averageHeading / numberOfNerabyAlignmentFish;
                    target = ((averagePosition - this.transform.position) + (Vector3.Magnitude(averagePosition - this.transform.position) /
                              Vector3.Magnitude(averageHeading)) * (averageHeading) + this.transform.position) + vAvoid * (separationFactor / 2 + 1.0f);
                }
                else
                    target = (averagePosition - this.transform.position) + globalGoalPosition + vAvoid * (separationFactor / 2 + 1.0f);

                rotationFactor = 1;
                speedFactor = 2;
            }
            if (flockUpdate % 3 == 2)
                flocking = false;
        }
        flockUpdate++;

        UpdateCurrentGoal(target, rotationFactor, speedFactor);
        dt2 = defaultUpdateTime + Random.Range(-0.03f, 0.1f) + 0.3f;
    }
}
