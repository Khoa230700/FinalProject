﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using static NewEnemyAI;

public class EnemiAI : MonoBehaviour
{
    public Transform player;
    NavMeshAgent agent;
    Animator enemyAnimation;


    public float maxHealth = 100f;
    public float currentHealth;
    public int attackDamage = 15;
    public float attackSpeed = 1.5f;
    private float nextAttackTime = 0f;

    public float attackCooldown = 1.5f;
    private float lastAttackTime;
    public float attackRange = 2f;
    public float chaseRange = 10f;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<Animator>();
    }

    void Update()
    {
        agent.destination = player.position;
        enemyAnimation.SetFloat("speed", agent.velocity.magnitude);

        float distance = Vector3.Distance(transform.position, player.position);

        //if (distance <= chaseRange)
        //{
        //agent.SetDestination(player.position);

        if (distance <= attackRange)
        {
            Attack();
        }
        //}
        //else
        //{
        //    agent.ResetPath(); 
        //}
    }
    

    
    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            // damage player()
            Debug.Log("Enemy attacks the player!");
        }
        enemyAnimation.SetTrigger("attack");
    }
}
