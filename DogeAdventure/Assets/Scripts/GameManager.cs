using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum enemyState
{
    IDLE, ALERT, PATROL, FOLLOW, FURY, DIE
}

public enum GameState
{
    GAMEPLAY, DIE
}

public class GameManager : MonoBehaviour
{
    public GameState gameState;
    
    [Header("Slime IA")] 
    public Transform[] slimeWayPoints;
    public float slimeDistanceToAttack;
    public float slimeIdleWaitTime = 5f;
    public float slimeAlertTime = 1f;
    public float slimeLookAtSpeed = 1f;
    
    [Header("Player")] 
    public Transform player;

    [Header("Rain Manager")] 
    public PostProcessVolume post2;
    public ParticleSystem rainParticle;
    private ParticleSystem.EmissionModule rainModule;
    [Range(0, 1000)]
    public int rainRateOverTime;
    public int rainIncrement;
    public float rainIncrementDelay;

    private void Start()
    {
        rainModule = rainParticle.emission;
    }

    public void OnOffRain(bool isRain)
    {
        StopCoroutine(RainManager(isRain));
        StartCoroutine(RainManager(isRain));
        
        StopCoroutine(Post2Manager(isRain));
        StartCoroutine(Post2Manager(isRain));
    }

    IEnumerator RainManager(bool isRain)
    {
        switch (isRain)
        {
            case true:
                for (float r = rainModule.rateOverTime.constant; r < rainRateOverTime; r += rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }

                rainModule.rateOverTime = rainRateOverTime;
                
                break;
            
            case false:
                for (float r = rainModule.rateOverTime.constant; r > 0; r -= rainIncrement)
                {
                    rainModule.rateOverTime = r;
                    yield return new WaitForSeconds(rainIncrementDelay);
                }

                rainModule.rateOverTime = 0;

                break;
        }
    }

    IEnumerator Post2Manager(bool isRain)
    {
        switch (isRain)
        {
            case true:
                for (float w = post2.weight; w < 1; w += 1 * Time.deltaTime)
                {
                    post2.weight = w;
                    yield return new WaitForEndOfFrame();
                }

                post2.weight = 1;
                
                break;
            
            case false:
                for (float w = post2.weight; w > 0; w -= 1 * Time.deltaTime)
                {
                    post2.weight = w;
                    yield return new WaitForEndOfFrame();
                }
                
                post2.weight = 0;

                break;
        }
    }
    
    public void ChangeGameState(GameState newState)
    {
        gameState = newState;
    }

    public void RestartScene()
    {
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(0);
    }
    
}
