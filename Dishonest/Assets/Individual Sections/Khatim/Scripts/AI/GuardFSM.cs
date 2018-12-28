﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardFSM : MonoBehaviour
{
    [Header("Guard Ditance Check")]
    public float attackDistance;
    //public float closeDistance;
    public float chaseDistance;
    public float distanceToPlayer;
    [Header("Enemy FoV")]
    public float foV;
    public float verticalFov;
    [Header("Guard Speed")]
    public float guardWalking;
    public float guardRunning;

    [Header("Wander Variables")]
    public float wanderRadius;
    public float maxWanderTimer;
    [Header("Other")]
    public GameObject player;
    public Light flashlight;
    private int currCondition;
    private int wanderCondition = 1;
    private int chaseCondition = 2;
    private int attackCondition = 3;
    private NavMeshAgent guardAgent;
    private Transform target;
    private float wanderTimer;
    private Vector3 tarDir;
    private Animator anim;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float verticalAngle;
    void Start()
    {
        guardAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        wanderTimer = maxWanderTimer;
    }

    void Update()
    {
        //Distance check to Player
        Vector3 chaseLength = this.transform.TransformDirection(Vector3.forward) * chaseDistance;
        Debug.DrawRay(this.transform.position, chaseLength, Color.green);

        distanceToPlayer = Vector3.Distance(this.transform.position, player.transform.position);
        tarDir = player.transform.position - this.transform.position;
        angle = Vector3.Angle(this.tarDir, this.transform.forward);
        verticalAngle = Vector3.Angle(this.tarDir, -this.transform.up);

        if ((distanceToPlayer > chaseDistance || angle > foV) && currCondition != wanderCondition)
        {
            //Wander
            currCondition = 1;
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
        }

        if (angle < foV && distanceToPlayer < attackDistance && verticalAngle < verticalFov && currCondition != attackCondition)
        {
            //Attack Player
            currCondition = 3;
            anim.SetBool("isAttacking", true);
        }
        else if (angle < foV && distanceToPlayer < chaseDistance && verticalAngle < verticalFov && currCondition != chaseCondition)
        {
            //Chase Player
            currCondition = 2;
            anim.SetBool("isRunning", true);
            anim.SetBool("isAttacking", false);

        }

        if (!player.activeInHierarchy)
        {
            currCondition = 1;
            anim.SetBool("isRunning", false);
            anim.SetBool("isAttacking", false);
        }
    }

    void FixedUpdate()
    {
        switch (currCondition)
        {
            case 1: //Wander Condition
                wanderTimer += Time.deltaTime;

                if (wanderTimer >= maxWanderTimer)
                {
                    //isAttacking = false;
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    guardAgent.SetDestination(newPos);
                    guardAgent.speed = guardWalking;
                    wanderTimer = 0;
                }
                // Debug.LogWarning("Wandering");
                break;

            case 2: //Chase Condition
                guardAgent.SetDestination(player.transform.position);
                guardAgent.speed = guardRunning;
                // Debug.LogWarning("Chasing Player");
                break;

            case 3: //Attack Condition
                // Debug.LogWarning("Attacking Player");
                break;

            case 4: //Idle Condition
                Debug.LogWarning("Idle");
                break;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
