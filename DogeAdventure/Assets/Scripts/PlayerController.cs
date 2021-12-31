using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameManager _GameManager;
    private CharacterController controller;
    private Animator anim;

    [Header("Player")] 
    public float movementSpeed = 4f;
    private Vector3 direction;
    private bool isWalking;
    public int healthPoints;

    [Header("Attack")] 
    public ParticleSystem fxAttack;
    public ParticleSystem[] fxHit;
    private bool isAttacking;
    public Transform hitBox;
    [Range(0.2f, 1f)]
    public float hitRange = 0.5f;
    public LayerMask hitMask;
    private Collider[] hitInfo;
    public int amountDmg = 1;
    public Transform knockBack;
    
    private float horizontal;
    private float vertical;

    
    void Start()
    {
        _GameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (_GameManager.gameState != GameState.GAMEPLAY)
        {
            return;
        }
        
        Inputs();
        
        CharacterMovement();
    }

    void Inputs()
    {
        //Movement
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        direction = new Vector3(horizontal, 0, vertical).normalized;

        //Attack
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            CharacterAttack();
        }
        
    }

    void CharacterMovement()
    {
        if (direction.magnitude > 0f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
        
        controller.Move(direction * movementSpeed * Time.deltaTime);
        anim.SetBool("isWalking", isWalking);
        
    }

    void CharacterAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attacking"); 
        fxAttack.Emit(1);

        hitInfo = Physics.OverlapSphere(hitBox.position, hitRange, hitMask);

        foreach (Collider colliderInHit in hitInfo)
        {
            colliderInHit.gameObject.SendMessage("GetHit", amountDmg, SendMessageOptions.DontRequireReceiver);
            colliderInHit.gameObject.SendMessage("KnockBack",  knockBack, SendMessageOptions.DontRequireReceiver);
            foreach (ParticleSystem fxInHit in fxHit)
            {
                fxInHit.gameObject.SetActive(true);
                fxInHit.Emit(1);
            }
        }
    }

    public void AttackIsDone()
    {
        movementSpeed = 4f;
        isAttacking = false;
    }

    public void CantMove()
    {
        movementSpeed = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (hitBox)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(hitBox.position, hitRange);
        }
       
    }

    void GetHit(Transform playerKnockback)
    {
        healthPoints -= 1;
        
        
        Vector3 direction = controller.transform.position - playerKnockback.position;
        controller.Move(direction * 100 * Time.deltaTime);
        
        
        if (healthPoints > 0)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            _GameManager.ChangeGameState(GameState.DIE);
            anim.SetTrigger("Die");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "TakeDamage")
        {
            GetHit(other.gameObject.transform);
        }
    }
    
}
