using System.Collections;
using UnityEngine;

public class Sonar : MonoBehaviour
{
    public LayerMask obstacleLayer;

    private EntityController entity;

    private float wallInFront;
    private float wallInLeftUp;
	private float wallInLeftDown;
	private float wallInRightUp;
    private float wallInRightDown;

    Coroutine scanning;

	void Awake()
	{
		entity = GetComponentInParent<EntityController>();
	}

    // Use this for initialization
    void Start()
    {
        scanning = StartCoroutine("FindTargetsWithDelay", 0.25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (enabled) {
            DetectObstaces();
        }
        else {
            if (scanning != null) {
                StopCoroutine(scanning);
            }
        }
    }

	public float[] GetData()
    {
		return new float[5]{wallInFront, wallInLeftUp, wallInLeftDown, wallInRightUp, wallInRightDown};
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (enabled) {
            DetectObstaces();
            yield return new WaitForSeconds(delay);
        }
    }

    private void DetectObstaces()
    {
        RaycastHit hit;
		Vector3 rightUpVector   = Quaternion.AngleAxis(45, transform.up) * Quaternion.AngleAxis(45, transform.right) * transform.forward;
		Vector3 rightDownVector = Quaternion.AngleAxis(45, transform.up) * Quaternion.AngleAxis(-45, transform.right) * transform.forward;

		Vector3 leftUpVector    = Quaternion.AngleAxis(-45, transform.up) * Quaternion.AngleAxis(45, transform.right)  * transform.forward;
		Vector3 leftDownVector  = Quaternion.AngleAxis(-45, transform.up) * Quaternion.AngleAxis(-45, transform.right) * transform.forward;

        if (Physics.Raycast(transform.position, transform.forward, out hit, entity.Genes.Ears.range, obstacleLayer)) {
            wallInFront = hit.distance;
        }
        else {
            wallInFront = 0;
        }

		if (Physics.Raycast(transform.position, leftUpVector, out hit, entity.Genes.Ears.range, obstacleLayer)) {
            wallInLeftUp = hit.distance;
        }
        else {
            wallInLeftUp = 0;
        }

		if (Physics.Raycast(transform.position, leftDownVector, out hit, entity.Genes.Ears.range, obstacleLayer)) {
			wallInLeftDown = hit.distance;
		}
		else {
			wallInLeftDown = 0;
		}

		if (Physics.Raycast(transform.position, rightUpVector, out hit, entity.Genes.Ears.range, obstacleLayer)) {
			wallInRightUp = hit.distance;
		}
		else {
			wallInRightUp = 0;
		}

		if (Physics.Raycast(transform.position, rightDownVector, out hit, entity.Genes.Ears.range, obstacleLayer)) {
            wallInRightDown = hit.distance;
        }
        else {
            wallInRightDown = 0;
        }
    }
}
