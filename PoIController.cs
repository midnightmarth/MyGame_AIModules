using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoIController : MonoBehaviour {

    public int PoI_ID;
    public float durability = 150;
    public float minConsume = 1.0f;
    // public float maxConsume = 10f;

    void Start(){
        PoI_ID = Random.Range(0, 9999);
    }
    public float Consume(float consumeFloat){
        float randomNum = Random.Range(minConsume, consumeFloat);
        if(durability <= randomNum){
            Debug.Log("Durability is too low to return normal consume");
            randomNum = Random.Range(minConsume, durability);
        }
            durability -= randomNum;
            return randomNum;
    }
}
