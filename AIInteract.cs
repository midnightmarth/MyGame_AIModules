using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIInteract {

    private AIMaster self;

    float attackRange;
    float attackDamage;
    float attackSpeed; //attacks per second
    public bool isAttacking { get; private set; }


    #region Consumption Variables
    public float consumeLimit = 50.0f;
    public float maxConsumePerEat = 10.0f;
    public float currentStomachContents = 0f;
    public float totalAmountConsumed = 0f;
    public float digestMultiplier = 1.0f;
    public float digestBase = 1.5f;
    public float remainingConsumeToRankUp = 500.0f;
    public float timeLastConsume = 0f;
    public bool isOnCooldown = false;
    public float cooldownDuration = 5.0f;
    public float timer = 0f;

    #endregion

    public AIInteract(AIMaster self, float attackRange, float attackDamage, float attackSpeed){
        this.self = self;
        this.attackRange = attackRange;
        this.attackDamage = attackDamage;
        this.attackSpeed = attackSpeed;
    }


    public IEnumerator AttackPlayer(PlayerController player){
        if(Vector3.Distance(self.transform.position, player.transform.position) > attackRange){
            Debug.Log("I am too far away from the player to attack");
            yield return null;
        }else{
            isAttacking = true;
            player.TakeDamage(attackDamage);
            yield return new WaitForSeconds(attackSpeed);
            isAttacking = false;
        }
    }

    public void Consume(PoIController mound){
        if(isOnCooldown){
            cooldownDuration -= Time.deltaTime;

            if(cooldownDuration <= 0){
                isOnCooldown = false;
            }
            return;
        }

        if(currentStomachContents <= consumeLimit - maxConsumePerEat && !isOnCooldown){
            float consumeAmount = mound.Consume(maxConsumePerEat);
            Debug.Log($"Eating {consumeAmount} from mound");
            currentStomachContents += consumeAmount;
            totalAmountConsumed += consumeAmount;
            isOnCooldown = true;
            cooldownDuration = 3.0f;
        }else{
            Debug.Log("Stomach too full to eat");
        }
    }

    public void DigestContentsInStomach(float deltaTime){
        if (currentStomachContents == 0) {
            return;
        }

        float decrease = digestBase * digestMultiplier * deltaTime; // Calculate decrease based on time passed
        currentStomachContents -= decrease;

        // Ensure the number doesn't go below 0
        if (currentStomachContents < 0) {
            currentStomachContents = 0;
        }
    }
    
}
