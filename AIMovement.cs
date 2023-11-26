using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;
using Unity.VisualScripting;

public class AIMovement {
    private NavMeshAgent agent;
    GameObject self;
    public int MaxSearchDistaceLeader = 10;
    public int MaxSearchDistace = 2;
    public int WanderMoveSpeed;
    public int SearchMoveSpeed;
    public int ChaseMoveSpeed;
    public int WanderAroundLeaderMoveSpeed;
    public int TurnSpeed;//Angular Speed on Nav Mesh Agent
    public int GoToPosStoppingDistance;
    public int DefaultStoppingDistance;


    public AIMovement(NavMeshAgent agent, GameObject self, AIMovementOptions movementOptions){
        this.agent = agent;
        this.self = self;
    
        MaxSearchDistace = movementOptions.MaxSearchDistace;
        MaxSearchDistaceLeader = movementOptions.MaxSearchDistaceLeader;
        WanderMoveSpeed = movementOptions.WanderMoveSpeed;
        WanderAroundLeaderMoveSpeed = movementOptions.WanderAroundLeaderMoveSpeed;
        SearchMoveSpeed = movementOptions.SearchMoveSpeed;
        ChaseMoveSpeed = movementOptions.ChaseMoveSpeed;
        TurnSpeed = movementOptions.TurnSpeed;
        GoToPosStoppingDistance = movementOptions.GoToPosStoppingDistance;
        DefaultStoppingDistance = movementOptions.DefaultStoppingDistance;
    }

    public void GoToPosition(Vector3 point){
        agent.stoppingDistance = GoToPosStoppingDistance;
        agent.speed = ChaseMoveSpeed;

        agent.SetDestination(point); //simple enough for now.
        agent.isStopped = false;
    }

    public void GoInvestigate(Vector3 point){
        agent.stoppingDistance = 0;
        agent.SetDestination(point);
    }

    public void SearchForPlayer(Vector3 lastKnownLocation){ // should be looking around the perimeter of when the player was seen last.
        agent.stoppingDistance = DefaultStoppingDistance;
        agent.speed = SearchMoveSpeed;

        if(agent.isStopped == true){
            Vector3 originalPosition = self.transform.position;
            Vector3 randomDirection = Random.onUnitSphere * MaxSearchDistace;
            Vector3 randomPoint = lastKnownLocation + originalPosition + randomDirection;

            agent.SetDestination(randomPoint);
            agent.isStopped = false;
            

        } else {
            float arrivalThreshold = 0.1f;
            if(agent.remainingDistance <= arrivalThreshold){
                agent.isStopped = true;    
            }
        }
    }

    public void Wander(){ //Make this into an IEnumerator that makes it wait at it's destination for a random amount of milliseconds.
        agent.stoppingDistance = DefaultStoppingDistance;
        agent.speed = WanderMoveSpeed;

        // float timeElapsed = 0f; // Initialize timeElapsed to 0
        // float duration = Random.Range(0.1f, 4.0f); // Set the duration for the lerp

        if(agent.isStopped == true){
            Vector3 originalPosition = self.transform.position;
            Vector3 randomDirection = Random.onUnitSphere * MaxSearchDistaceLeader;
            Vector3 randomPoint = originalPosition + randomDirection;

            agent.SetDestination(randomPoint);
            agent.isStopped = false;
            

        }else{
            float arrivalThreshold = 0.1f;
            if(agent.remainingDistance <= arrivalThreshold){
                agent.isStopped = true;    
                // yield return null;
            }
        }
    }

    public void WanderAroundLeader(AIMaster leader){
        agent.stoppingDistance = DefaultStoppingDistance;
        agent.speed = WanderAroundLeaderMoveSpeed;
        float minRadius = 2.0f;
        float maxRadius = 10.0f;

        // float timeElapsed = 0f; // Initialize timeElapsed to 0
        // float duration = Random.Range(0.1f, 4.0f); // Set the duration for the lerp

        if(agent.isStopped == true){
            Vector3 leaderPosition = leader.transform.position;
            // Generate a random distance within the specified range.
            float randomDistance = Random.Range(minRadius, maxRadius);

            // Generate a random angle in radians.
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // Calculate the random position based on the distance and angle.
            float x = randomDistance * Mathf.Cos(randomAngle);
            float z = randomDistance * Mathf.Sin(randomAngle);

            // Calculate the final position relative to the center of this GameObject.
            Vector3 randomPoint = leaderPosition + new Vector3(x, 0f, z);

            agent.SetDestination(randomPoint);
            agent.isStopped = false;
            

        }else{
            float arrivalThreshold = 0.1f;
            if(agent.remainingDistance <= arrivalThreshold){
                agent.isStopped = true;    
                // yield return null;
            }
        }
    }

    public void GoToLeader(AIMaster leader) {
        agent.SetDestination(leader.transform.position);
    }

    public void WanderAroundPoI(PoIController poi) {
        
        agent.isStopped = false;
        if (agent.remainingDistance < 1) {
            float radius = poi.GetComponent<CapsuleCollider>().radius;
            float thickness = 5.0f;
            Vector3 center = poi.transform.position;

            float randomAngle = Random.Range(0f, 2.0f * Mathf.PI); // Random angle
            float minRadius = radius + thickness; // Ensure distance from object
            float maxRadius = radius + 2 * thickness; // Define a larger circle
            float randomRadius = Mathf.Sqrt(Random.Range(minRadius * minRadius, maxRadius * maxRadius));

            // Convert polar coordinates to Cartesian coordinates
            float x = center.x + randomRadius * Mathf.Cos(randomAngle);
            float z = center.z + randomRadius * Mathf.Sin(randomAngle);
            float y = poi.transform.position.y; // Adjust y to terrain height

            agent.SetDestination(new Vector3(x, y, z));
        }
    }

    public void GoToConsumePoI(GameObject poiMemory) {
        agent.SetDestination(poiMemory.transform.position);
        // throw new System.NotImplementedException();
    }
}
