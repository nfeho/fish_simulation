using UnityEngine;
using System.Collections;

public class Flock : MonoBehaviour {

    public GameObject fishPrefab;
    public GameObject goalPrefab;

    private int typeOfFlock = 0;

    public int amountOfFish = 25;
    public float cubeSize = 8.0f;
    public GameObject[] flockFish;

    public int flockMaxSize = 100;
    public float cohesion = 1.0f; // Affecting the radius of flocking
    public float separation = 0.1f; // Affecting the strength of avoidance and spacing between fish
    public float alignment = 0.5f; // Affecting the radius of aligning fish rotations
    public float minimalSpeed = 0.3f;
    public float maximalSpeed = 8.0f;

    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 goalPosition = Vector3.zero;

	// Use this for initialization
	void Start () {
        spawnPosition = this.transform.position;
        flockFish = new GameObject[amountOfFish];
        if (fishPrefab.tag == "MediumFish")
            typeOfFlock = 1;
        else if (fishPrefab.tag == "BigFish")
            typeOfFlock = 2;
        for (int i = 0; i < amountOfFish; i++)
        {
            Vector3 pos = new Vector3(Random.Range(spawnPosition.x - cubeSize, spawnPosition.x + cubeSize),
                                      Random.Range(spawnPosition.y - cubeSize, spawnPosition.y + cubeSize),
                                      Random.Range(spawnPosition.z - cubeSize, spawnPosition.z + cubeSize));
            flockFish[i] = (GameObject)Instantiate(fishPrefab, pos, Quaternion.identity);
            switch(typeOfFlock)
            {
                case 0:
                    flockFish[i].GetComponent<SmallFishBehaviour>().setParent(this);
                    break;
                case 1:
                    flockFish[i].GetComponent<MediumFishBehaviour>().setParent(this);
                    break;
                case 2:
                    flockFish[i].GetComponent<BigFishBehaviour>().setParent(this);
                    break;
            }

        }
	}
	
	// Update is called once per frame
	void Update () {
	    if (Random.Range(0, 10000) < 25)
        {
            goalPosition = new Vector3(Random.Range(spawnPosition.x - cubeSize, spawnPosition.x + cubeSize),
                                       Random.Range(spawnPosition.y - cubeSize / 2, spawnPosition.y + cubeSize / 2),
                                       Random.Range(spawnPosition.z - cubeSize, spawnPosition.z + cubeSize));
            goalPrefab.transform.position = goalPosition;
        }
	}
}
