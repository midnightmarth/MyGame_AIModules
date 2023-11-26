using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMemory {

    public PriorityQueue<PoIController> poiMemory {get; private set;}
    public Vector3 lastKnownPlayerLocation {get; private set;}
    GameObject self;
    public AIMemory(GameObject self){
        this.self = self;
        poiMemory = new();
        lastKnownPlayerLocation = new();
    }

    public void UpdatePoiMemory(PoIController memory){
        this.poiMemory.Enqueue(memory, (int)Vector3.Distance(memory.transform.position, self.transform.position));
    }

    public void ClearPoiMemory(){
        this.poiMemory = null;
    }

    public void UpdatePlayerLocation(Vector3 location){
        this.lastKnownPlayerLocation = location;
    }

    public void ClearPlayerLocation(){
        this.lastKnownPlayerLocation = new Vector3();
    }
}
