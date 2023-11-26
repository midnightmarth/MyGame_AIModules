using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIGroup {

    public List<AIMaster> currentGroupList;
    public int groupID;
    public AIMaster leader;
    public bool isLeader;
    public AIMaster self;
    public AIGroup(AIMaster self){
        this.self = self;
        currentGroupList = new(){self};
        leader = self;
        isLeader = true;
        groupID = Random.Range(1, 999999);

    }

    public void AddMember(AIMaster ally){
        if (!currentGroupList.Any(member => member.AI_ID == ally.AI_ID)) {
            Debug.Log($"OnAddMember function fired for {self.name}");
            currentGroupList.Add(ally);
            AppointLeader();
        }
    }

    public void RemoveMember(AIMaster memberToRemove){
        List<AIMaster> newCurrentGroup = new(currentGroupList);
        newCurrentGroup.RemoveAll(groupie => groupie.AI_ID == memberToRemove.AI_ID);
    }

    public void RemoveAllDuplicatesInGroup(){
        List<AIMaster> newCurrentList = currentGroupList.GroupBy(x => x.AI_ID)
                                            .Select(g => g.First())
                                            .ToList();
        currentGroupList = newCurrentList;
    } 

    public void AppointLeader(){
        AIMaster newLeader = null;
        foreach(AIMaster groupie in currentGroupList){
            if(newLeader == null){
                newLeader = groupie;
                continue;
            }

            if(newLeader.enemyTier < groupie.enemyTier){
                newLeader = groupie;
            }
        }

        foreach(AIMaster groupie in currentGroupList){
            if(groupie.AI_ID == newLeader.AI_ID){
                newLeader.group.isLeader = true;
                newLeader.group.leader = newLeader;
            }else{
                groupie.group.isLeader = false;
                groupie.group.leader = newLeader;
            }
        }
    }

    public bool CheckForGroup(AIMaster ally){
        AIGroup allyGroup = ally.group;
        
        bool result = ShouldJoinNewGroup(allyGroup.currentGroupList.Count, allyGroup.leader.enemyTier);
        if(currentGroupList.Count == 0 || result){
            return true;
        }
        return false;
    }

    public bool ShouldJoinNewGroup(int newGroupSize, int leaderTier) {
        int sizeDifference = newGroupSize - currentGroupList.Count;

        // Calculate likelihood based on size difference
        float likelihood = CalculateLikelihood(sizeDifference);

        bool result = Random.Range(0.0f, 1.0f) < likelihood || leaderTier > self.enemyTier;

        // Check if the random number is less than the calculated likelihood
        return result;
    }

    private float CalculateLikelihood(int sizeDifference) {
        // You can define your own formula for likelihood here.
        // For example, you can use an exponential function to give higher likelihood for larger differences.
        // Adjust the parameters of the function as needed.
        float scale = 0.2f;
        float likelihood = Mathf.Exp(scale * sizeDifference);
        return likelihood;
    }

}
