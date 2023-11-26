using UnityEditor;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum AIState {
    Wandering,
    WanderToPointOfInterest,
    GoToConsume,
    Chasing,
    Searching
}

public class AIMovementOptions {
    public int WanderMoveSpeed {get; set;}
    public int ChaseMoveSpeed {get; set;}
    public int SearchMoveSpeed {get; set;}
    public int WanderAroundLeaderMoveSpeed {get; set;}
    public int MaxSearchDistace {get; set;}
    public int MaxSearchDistaceLeader {get; set;}
    public int TurnSpeed {get; set;}
    public int GoToPosStoppingDistance {get; set;}
    public int DefaultStoppingDistance {get; set;}

    public AIMovementOptions(int wanderMoveSpeed, int chaseMoveSpeed, int searchMoveSpeed, int wanderAroundLeaderMoveSpeed,
     int maxSearchDistace, int maxSearchDistaceLeader, int turnSpeed, int goToPosStoppingDistance, int defaultStoppingDistance) {

        WanderMoveSpeed = wanderMoveSpeed;
        ChaseMoveSpeed = chaseMoveSpeed;
        SearchMoveSpeed = searchMoveSpeed;
        WanderAroundLeaderMoveSpeed = wanderAroundLeaderMoveSpeed;
        MaxSearchDistace = maxSearchDistace;
        MaxSearchDistaceLeader = maxSearchDistaceLeader;
        TurnSpeed = turnSpeed;
        GoToPosStoppingDistance = goToPosStoppingDistance;
        DefaultStoppingDistance = defaultStoppingDistance;
    }
}

public class AIMaster : MonoBehaviour {

    public int AI_ID;
    public float health = 50f;
    public GameObject player;
    NavMeshAgent navAgent;
    AIMovement movement;
    AIPerception perception;
    public AIDecisions decisions;
    public AIGroup group;
    public AIInteract interact;
    public AIMemory memory;
    public AIDebug debug;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public float fov = 90f;
    public AIState aiState;
    public int remainingSearches = 5;
    public int enemyTier;
    float deltaTime;
    [Header("AI Movement Options")]

    public int WanderMoveSpeed = 4;
    public int ChaseMoveSpeed = 10;
    public int SearchMoveSpeed = 8;
    public int WanderAroundLeaderMoveSpeed = 6;
    public int MaxSearchDistaceLeader = 10;
    public int MaxSearchDistace = 2;
    public int TurnSpeed = 120;
    public int GoToPosStoppingDistance = 2;
    public int DefaultStoppingDistance = 0;

    [Header("Group Information")]
    public List<AIMaster> currentGroupList;
    public AIMaster currentLeader;
    public int groupID;

    [Header("Interact Information")]
    public float currentStomachLevel = 0f;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public float attackSpeed = 1f; //attacks per second

    #region GizmoDrawing
        private void OnDrawGizmos() {
            DrawFOV();
            if(group != null){
                DrawLineToLeader();
            }
        }
    
        void DrawFOV(){
            float halfFOV = fov / 2f;
            Quaternion leftRayRotation = Quaternion.Euler(0, -halfFOV, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, halfFOV, 0);
            Vector3 forwardVector = transform.forward * 100;
    
            Vector3 leftRayDirection = leftRayRotation * forwardVector;
            Vector3 rightRayDirection = rightRayRotation * forwardVector;
    
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, leftRayDirection);
            Gizmos.DrawRay(transform.position, rightRayDirection);
        }
    
        void DrawLineToLeader(){
            Gizmos.color = Color.blue;
            
            if(group.isLeader){
                foreach(AIMaster ling in group.currentGroupList){
                    if(ling.AI_ID != AI_ID){
                        foreach(AIMaster groupMate in group.currentGroupList){
                            Vector3 directionToGroupmate = (groupMate.transform.position - transform.position).normalized;
                            float rayLength = Vector3.Distance(transform.position, groupMate.transform.position);
                            Gizmos.DrawRay(transform.position, directionToGroupmate * rayLength);
                        }
                    }
                }
            }
        }
    
    
    #endregion

    void Start() {

        AIMovementOptions movementOptions = new(WanderMoveSpeed, ChaseMoveSpeed, SearchMoveSpeed, WanderAroundLeaderMoveSpeed, MaxSearchDistace, MaxSearchDistaceLeader, TurnSpeed, GoToPosStoppingDistance, DefaultStoppingDistance); 

        AI_ID = Random.Range(1, 999);
        aiState = AIState.Wandering;
        navAgent = gameObject.GetComponent<NavMeshAgent>();
        enemyTier = Random.Range(1,25);
        movement = new AIMovement(navAgent, this.gameObject, movementOptions);
        perception = new AIPerception(this, player, fov, targetMask, obstacleMask);
        memory = new AIMemory(this.gameObject);
        decisions = new AIDecisions(this.gameObject);
        group = new AIGroup(this);
        interact = new AIInteract(this, attackRange, attackDamage, attackSpeed);
        debug = new AIDebug(this);
        currentGroupList = new();
        currentLeader = this;
        decisions.AddToPriorityQueue(PriorityEvents.Wander);
        StartCoroutine ("FindTargetsWithDelay", .1f);

        debug.InitGenerateStateDebug();

    }

    public IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
            if(health == 0){
                break;
            }

			perception.FindVisibleTargets();
			yield return new WaitForSeconds (delay);
		}
	}

    void Update() {
        if(health == 0){
            navAgent.enabled = false;
            return;
        }
        if(decisions.priorityQueue.Count == 0){
            decisions.AddToPriorityQueue(PriorityEvents.Wander);
        }
        UpdateCurrentValues();
        deltaTime = Time.deltaTime;
        currentStomachLevel = interact.currentStomachContents;
        
        interact.DigestContentsInStomach(deltaTime);
        decisions.Decide();

        PerformAIStateBehavior();
        debug.UpdateAIStateText(aiState);
    }

    void UpdateCurrentValues() {
        currentGroupList = group.currentGroupList;
        groupID = group.groupID;
        currentLeader = group.leader;
    }

    void PerformAIStateBehavior() {
        switch (aiState) {
            case AIState.Wandering:
                HandleWanderingBehavior();
                break;

            case AIState.Chasing:
                HandleAttackingPlayer(player.transform.position);
                break;

            case AIState.WanderToPointOfInterest:
                HandleWanderToPointOfInterestBehavior();
                break;

            case AIState.GoToConsume:
                HandleGoToConsumeBehavior();
                break;

            case AIState.Searching:
                HandleSearchingBehavior();
                break;

            default:
                Debug.Log("No AI State");
                break;
        }
    }

    void HandleAttackingPlayer(Vector3 playerPosition){
        if(interact.isAttacking){
            return;
        }
        if(Vector3.Distance(transform.position, playerPosition) > attackRange){
            movement.GoToPosition(playerPosition);
        } else {
            Debug.Log("Attacking Player");
            navAgent.isStopped = true;
            StartCoroutine(interact.AttackPlayer(player.GetComponent<PlayerController>()));
        }
    }

    void HandleWanderingBehavior() {
        if (group.isLeader) {
            movement.Wander();
        } else {
            HandleNonLeaderWanderingBehavior();
        }
    }

    void HandleNonLeaderWanderingBehavior() {
        if (Vector3.Distance(group.leader.transform.position, gameObject.transform.position) >= 15) {
            movement.GoToLeader(group.leader);
        } else {
            movement.WanderAroundLeader(group.leader);
        }
    }

    void HandleWanderToPointOfInterestBehavior() {
        PoIController peek = memory.poiMemory.Peek();
        movement.WanderAroundPoI(peek);
        if (interact.currentStomachContents < interact.consumeLimit) {
            // aiState = AIState.GoToConsume;
            decisions.AddToPriorityQueue(PriorityEvents.Eat);
        }
    }

    void HandleGoToConsumeBehavior() {
        PoIController mound = memory.poiMemory.Peek();
        if (mound.durability < interact.maxConsumePerEat) {
            Debug.Log("Mound is too low for consumption");
            aiState = AIState.Wandering;
            decisions.FinishOrder();
            return;
        }

        if (Vector3.Distance(transform.position, mound.transform.position) > 5) {
            movement.GoToConsumePoI(mound.gameObject);
        } else {
            PerformConsumeBehavior(mound);
        }
    }

    void PerformConsumeBehavior(PoIController mound) {
        navAgent.isStopped = true;
        interact.Consume(mound);

        if (interact.currentStomachContents >= interact.consumeLimit - interact.maxConsumePerEat) {
            navAgent.isStopped = false;
            decisions.FinishOrder();
            decisions.AddToPriorityQueue(PriorityEvents.Wander);
        }
    }

    void HandleSearchingBehavior() {
        if (remainingSearches > 0 && navAgent.remainingDistance <= 2) {
            navAgent.isStopped = true;
            remainingSearches--;
            movement.SearchForPlayer(memory.lastKnownPlayerLocation);
        } else if (remainingSearches == 0) {
            ResetSearchBehavior();
        }
    }

    void ResetSearchBehavior() {
        memory.ClearPlayerLocation();
        decisions.FinishOrder();
        decisions.AddToPriorityQueue(PriorityEvents.Wander);
        remainingSearches = 5;
    }
    public void TakeDamage(float damageNumber){
        if(health <= 0){
            return;
        }
        
        health -= damageNumber;
        if(health <= 0){
            Debug.Log($"AI {gameObject.name} has died");
            //handle dying functionality
            navAgent.enabled = false;
        }
    }
}