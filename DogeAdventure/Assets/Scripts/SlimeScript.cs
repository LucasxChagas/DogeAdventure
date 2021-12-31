using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SlimeScript : MonoBehaviour
{
    private GameManager _gmGameManager;
    
    public ParticleSystem fxDeath;
    public GameObject slimeMesh;
    
    private Animator anim;
    private Rigidbody rb;
    
    public int health;
    public bool isDie;
    public float knockBackStrength;

    public enemyState state;

    private bool isPlayerVisible;
    private bool isWalk;
    private bool isAlert;
    private bool isAttacking;
    private NavMeshAgent agent;
    private Vector3 destination;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        
        _gmGameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        
        ChangeState(enemyState.IDLE);
    }

    private void Update()
    {
        StateManager();

        if (agent.desiredVelocity.magnitude > 0f)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }
        
        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isAlert", isAlert);
    }
    
    
    IEnumerator Died()
    {
        yield return new WaitForSeconds(1.2f);
        fxDeath.gameObject.SetActive(true);
        Destroy(slimeMesh);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    IEnumerator knockBackDie()
    {
        yield return new WaitForSeconds(0.1f);
        isDie = true;
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;

    }

    IEnumerator StillStayDelay()
    {
        yield return new WaitForSeconds(2f);
        StayStill(50); 
        
    }

    void GetHit(int amount)
    {
        if (isDie)
        {
            return;
        }
        
        health -= amount;
        if (health > 0)
        {
            ChangeState(enemyState.FURY);
            anim.SetTrigger("GetHit");
            StartCoroutine(AttackDelay());
        }
        else
        {
            ChangeState(enemyState.DIE);
            anim.SetTrigger("Die");
            StartCoroutine(Died());
            StartCoroutine(knockBackDie());
        }
        
    }

    void KnockBack(Transform knockBack)
    {
        if (isDie)
        {
            return;
        }       
        Vector3 direction = rb.transform.position - knockBack.position;
        rb.AddForce(direction * knockBackStrength, ForceMode.Impulse);
    
    }
    
    void Attack()
    {
        if (!isAttacking && isPlayerVisible)
        {
            isAttacking = true;
            anim.SetTrigger("Attack");
        }
    }
    
    public void AttackIsDone()
    {
        StartCoroutine(AttackDelay());
    }
    
    int Rand()
    {
        int rand = Random.Range(0, 100);
        return rand;
    }
    
     // ----------------------------------------------------------------------    
     // ----------------------------------------------------------------------
    void StateManager()
    {

        if (_gmGameManager.gameState == GameState.DIE && (state == enemyState.FOLLOW || state == enemyState.FURY || state == enemyState.ALERT))
        {
            ChangeState(enemyState.IDLE);
            _gmGameManager.RestartScene();
        }
        
        switch (state)
        {
            case enemyState.IDLE:
                destination = transform.position;
                agent.destination = destination;
                break;
            
            case enemyState.ALERT:
                LookAt();
                break;

            case enemyState.PATROL:
                
                break;
            
            case enemyState.FOLLOW:
                LookAt();
                destination = _gmGameManager.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                
                break;
            
            case enemyState.FURY:
                LookAt();
                destination = _gmGameManager.player.position;
                agent.destination = destination;
                
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                
                break;
            
            
        }
    }

    void ChangeState(enemyState newState)
    {
        StopAllCoroutines();
        isAlert = false;
        
        state = newState;
        print(state);
        
        switch (state)
        {
            case enemyState.IDLE:
                destination = transform.position;
                agent.stoppingDistance = 0;
                agent.destination = destination;
                
                StartCoroutine(IDLE());
                
                break;
            
            case enemyState.ALERT:
                destination = transform.position;
                agent.stoppingDistance = 0;
                agent.destination = destination;
                isAlert = true;
                StartCoroutine(ALERT());
                
                break;
            
            case enemyState.PATROL:
                destination = _gmGameManager.slimeWayPoints[Random.Range(0, _gmGameManager.slimeWayPoints.Length)]
                    .position;
                agent.stoppingDistance = 0;
                agent.destination = destination;
                
                StartCoroutine(PATROL());
                
                break;
            
            case enemyState.FURY:
                
                destination = _gmGameManager.player.position;
                agent.stoppingDistance = _gmGameManager.slimeDistanceToAttack;
                agent.destination = destination;

                break;
            
            case enemyState.FOLLOW:
                destination = _gmGameManager.player.position;
                agent.stoppingDistance = _gmGameManager.slimeDistanceToAttack;
                agent.destination = destination;
                break;
            
            case enemyState.DIE:
                destination = _gmGameManager.player.position;
                agent.destination = destination;
                
                break;
        }
    }

    IEnumerator IDLE()
    {
        yield return new WaitForSeconds(_gmGameManager.slimeIdleWaitTime);
        StayStill(50);
    }

    IEnumerator PATROL()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0);
        StayStill(50);
    }

    IEnumerator ALERT()
    {
        yield return new WaitForSeconds(_gmGameManager.slimeAlertTime);

        if (isPlayerVisible)
        {
            ChangeState(enemyState.FOLLOW);
        }
        else
        {
            StayStill(50);
        }
    }

    void StayStill(int yes)
    {
        if (Rand() < yes)
        {
            ChangeState(enemyState.IDLE);
        }
        else
        {
            ChangeState(enemyState.PATROL);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_gmGameManager.gameState != GameState.GAMEPLAY)
        {
            return;
        }
        
        
        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = true;
            
            if (state == enemyState.IDLE || state == enemyState.PATROL)
            {
                ChangeState(enemyState.ALERT);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerVisible = false;
            
            if (state == enemyState.FURY)
            {
                ChangeState(enemyState.FURY);
            }
            else
            {
                StartCoroutine(StillStayDelay());
            }
            
        }

    }

    void LookAt()
    {
 
        
        Vector3 lookDirection = (_gmGameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gmGameManager.slimeLookAtSpeed * Time.deltaTime);
    }

    
    
    
}
