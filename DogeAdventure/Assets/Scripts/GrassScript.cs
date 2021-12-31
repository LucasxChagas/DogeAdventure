using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassScript : MonoBehaviour
{
    public ParticleSystem fxHit;
    [Header("Grass Atributes")] 
    [SerializeField]
    private int health;
    [SerializeField]
    private bool isCut;

    private Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
    }
    
    void GetHit(int amount)
    {
        if (!isCut)
        {
            health -= amount;
            if (health <= 0)
            {
                fxHit.Emit(20);
                transform.localScale = new Vector3(1f, 1f, 1f);
                col.enabled = false; 
                isCut = true;
            }
        }
    }
  
}
