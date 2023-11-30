using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AIPerception {

    public AIMaster self;
    [Range(0, 360)]
    public float fov = 90f;
    public int viewDistance = 100;
    GameObject player;

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    public List<Transform> visibleTargets = new List<Transform>();

    public AIPerception(AIMaster self, GameObject player, float fov, LayerMask targetMask, LayerMask obstacleMask) {
        this.self = self;
        this.fov = fov;
        this.player = player;
        this.targetMask = targetMask;
        this.obstacleMask = obstacleMask;
    }

    public void FindVisibleTargets() {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(self.transform.position, viewDistance, targetMask);

        foreach (var targetCollider in targetsInViewRadius) {
            if (IsValidTarget(targetCollider)) {
                Transform target = targetCollider.transform;
                Vector3 dirToTarget = (target.position - self.transform.position).normalized;

                if (IsTargetInFieldOfView(target, dirToTarget)) {
                    float dstToTarget = Vector3.Distance(self.transform.position, target.position);

                    if (!IsObstacleBlockingView(dirToTarget, dstToTarget)) {
                        HandleVisibleTarget(target.gameObject);
                    } else {
                        HandleObstacleInView();
                    }
                }
            }
        }
    }

    bool IsValidTarget(Collider targetCollider) {
        return targetCollider.gameObject.tag != "Untagged" ||
            self.AI_ID != targetCollider.gameObject.GetComponent<AIMaster>().AI_ID;
    }

    bool IsTargetInFieldOfView(Transform target, Vector3 dirToTarget) {
        return Vector3.Angle(self.transform.forward, dirToTarget) < fov / 2;
    }

    bool IsObstacleBlockingView(Vector3 dirToTarget, float dstToTarget) {
        return Physics.Raycast(self.transform.position, dirToTarget, dstToTarget, obstacleMask);
    }

    void HandleVisibleTarget(GameObject targetObject) {
        switch (targetObject.tag) {
            case "Player":
                HandlePlayerTarget(targetObject);
                break;

            case "Point of Interest":
                HandlePoITarget(targetObject);
                break;

            case "Enemy":
                HandleEnemyTarget(targetObject);
                break;

            default:
                HandleDefaultTarget();
                break;
        }
    }

    void HandlePlayerTarget(GameObject playerObject) {
        self.memory.UpdatePlayerLocation(playerObject.transform.position);
            self.decisions.AddToPriorityQueue(PriorityEvents.AttackPlayer);
    }

    void HandlePoITarget(GameObject poiObject) {
        PoIController mound = poiObject.GetComponent<PoIController>();
        if (mound.durability > self.interact.maxConsumePerEat) {
            self.memory.UpdatePoiMemory(mound);
            self.decisions.AddToPriorityQueue(PriorityEvents.Eat);
        } else {
            Debug.Log($"I see a mound but its durability, {mound.durability}, is too low for me to care");
        }
    }

    void HandleEnemyTarget(GameObject enemyObject) {
        AIMaster ally = enemyObject.GetComponent<AIMaster>();
        
        if (self.AI_ID == ally.AI_ID && (ally == null || ally.group == null)) {
            return;
        }

        if (ally.group != null && self.group != null && self.group.groupID != ally.group.groupID && self.group.CheckForGroup(ally)) {
            HandleGroupInteraction(ally);
        }
    }

    void HandleGroupInteraction(AIMaster ally) {
        self.group.RemoveMember(self);
        self.group.AppointLeader();

        self.group = ally.group;
        self.group.self = self;
        self.group.isLeader = false;
        self.group.AddMember(self);
        self.aiState = AIState.Wandering;
    }

    void HandleDefaultTarget() {
        AIMemory memoryModule = self.memory;
        if (memoryModule.lastKnownPlayerLocation != Vector3.zero) {
            self.aiState = AIState.Searching;
        }
    }

    void HandleObstacleInView() {
        if (self.aiState == AIState.Chasing) {
            self.decisions.FinishOrder();
            self.decisions.AddToPriorityQueue(PriorityEvents.SearchForPlayer);
        }
    }

}
