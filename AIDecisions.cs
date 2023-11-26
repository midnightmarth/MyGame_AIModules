using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PriorityEvents{
    AttackPlayer = 0 ,
    SearchForPlayer = 1,
    Eat = 2,
    FollowLeader = 3,
    Wander = 4
}

public class AIDecisions {

    AIMaster self;
    public PriorityQueue<PriorityEvents> priorityQueue = new();
    public PriorityEvents currentEvent;
    public AIDecisions(GameObject self){
        this.self = self.GetComponent<AIMaster>();
    }

    public void AddToPriorityQueue(PriorityEvents priorityEvent){
        switch(priorityEvent){
            case PriorityEvents.AttackPlayer:
                priorityQueue.Enqueue(priorityEvent, 0);
            break;

            case PriorityEvents.SearchForPlayer:
                priorityQueue.Enqueue(priorityEvent, 1);
            break;
            
            case PriorityEvents.Eat:
                priorityQueue.Enqueue(priorityEvent, 2);
            break;

            case PriorityEvents.FollowLeader:
                priorityQueue.Enqueue(priorityEvent, 3);
            break;

            case PriorityEvents.Wander:
                priorityQueue.Enqueue(priorityEvent, 4);
            break;

            default:
                Debug.Log("PriorityEvent Does Not Exist");
            break;
        }
    }

public void Decide() {
    if (priorityQueue.Count == 0) {
        Debug.Log("No events in the priority queue.");
        return;
    }

    PriorityEvents peek = priorityQueue.Peek();
        currentEvent = peek;
        
        switch (peek) {
            case PriorityEvents.AttackPlayer:
                self.aiState = AIState.Chasing;
                break;

            case PriorityEvents.SearchForPlayer:
                self.aiState = AIState.Searching;
                break;

            case PriorityEvents.Eat:
                self.aiState = AIState.GoToConsume;
                break;

            case PriorityEvents.FollowLeader:
                self.aiState = AIState.Wandering;
                break;

            case PriorityEvents.Wander:
                self.aiState = AIState.Wandering;
                break;

            default:
                Debug.Log("PriorityEvent Does Not Exist When Deciding :()");
                break;
        }

}

    public void FinishOrder(){
        priorityQueue.Dequeue();
    }
}

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();
    private HashSet<int> uniqueIdentifiers = new HashSet<int>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        if (!uniqueIdentifiers.Contains(priority))
        {
            elements.Add((item, priority));
            uniqueIdentifiers.Add(priority);
            elements.Sort((x, y) => x.priority.CompareTo(y.priority));
        }
        else
        {
            // Handle duplicate addition
            // For example, you can update the priority of the existing item
            for (int i = 0; i < elements.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(elements[i].item, item))
                {
                    elements[i] = (item, priority);
                    elements.Sort((x, y) => x.priority.CompareTo(y.priority));
                    return;
                }
            }
        }
    }

    public T Dequeue()
    {
        if (elements.Count == 0)
        {
            throw new InvalidOperationException("Priority queue is empty.");
        }

        T item = elements[0].item;

        uniqueIdentifiers.Remove((int)elements[0].priority);
        elements.RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        if (elements.Count == 0)
        {
            throw new InvalidOperationException("Priority queue is empty.");
        }

        return elements[0].item;
    }
}
