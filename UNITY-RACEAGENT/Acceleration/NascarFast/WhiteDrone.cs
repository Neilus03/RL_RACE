using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class DroneAgent : Agent
{
    private Rigidbody rb;
    private float moveSpeed = 300f;
    private float turnSpeed = 300.0f;
    
    private Vector3 startingPos;

    private int currentStep = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        currentStep = 0; // Reset the step count
        // Reset the drone's position and velocity
        Vector3 parentPosition = transform.parent.position;

        //set starting position
        transform.position = new Vector3(parentPosition.x -102.0f, 1.5f, parentPosition.z+219.0f); //for circular circuit
        //transform.position = new Vector3(parentPosition.x -73.7f, 1.5f, parentPosition.z+6.6f); //for rally circuit
           
       
        transform.localRotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var discreteActions = actionBuffers.DiscreteActions;
        float move = 0f;
        float turn = 0f;

        if (discreteActions[0] == 1) move = 1f;
        //if (discreteActions[0] == 2) move = -1f;//-1f; //this is backwards
        if (discreteActions[0] == 3) move = 0f;

        if (discreteActions[1] == 1) turn = -1f;
        if (discreteActions[1] == 2) turn = 1f;
        if (discreteActions[1] == 3) turn = 0f;

        // Apply movement
        //rb.AddForce(transform.forward * move * moveSpeed, ForceMode.VelocityChange);
        //alternative add force by acceleration
        rb.AddForce(transform.forward * move * moveSpeed, ForceMode.Acceleration);
        transform.Rotate(Vector3.up, turn *turnSpeed * Time.deltaTime);

        // Reward for moving forward
        if (move == 1f)
        {
            AddReward(0.05f);
        }
        else if (move == -1f)
        {
            AddReward(-0.5f);
        }

        // Small reward for maintaining stable height
        if (transform.localPosition.y >= 1.0f && transform.localPosition.y <= 2.0f)
        {
            AddReward(0.001f);
        }
        else
        {
            AddReward(-0.005f);
        }

        // Penalize for deviating from the track (if drone falls off the track)
        if (transform.localPosition.y < 1.0f || transform.localPosition.y > 2.0f)
        {
            SetReward(-0.5f);
            EndEpisode();
        }

        if (currentStep >= 5000)
        {
            EndEpisode();
        }
        currentStep++;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // No movement by default
        discreteActionsOut[1] = 0; // No turn by default

        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 3;

        if (Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 2;

        if (Input.GetKey(KeyCode.X)) discreteActionsOut[0] = 3;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("track"))
        {
            SetReward(-0.5f);
            EndEpisode();
        }
    }
}
