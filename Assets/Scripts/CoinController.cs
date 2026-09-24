using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public static CoinController instance; // Singleton D

    public void Awake()
    {
        instance = this;
    }

    public int currentCoins;
    public CoinPickup coin;

    private readonly List<CoinPickup> coinPool = new List<CoinPickup>(); // The pool of coin pickups, so kills don't Instantiate/Destroy constantly. GK

    public void AddCoins(int coinsToAdd) // Add coins. D
    {
        currentCoins += coinsToAdd; // Add the coins. D
        UIController.instance.UpdateCoins(); // Update the coins. D
        SFXManager.instance.PlaySFXPitched(2); // Play the sound effect. GK
    }
    public void DropCoin(Vector3 position, int value)
    {
        Vector3 spawnPosition = position + new Vector3(0.2f, 1f, 0f); // The position to drop the coin at. D
        CoinPickup newCoin; // The coin to spawn. GK

        if (coinPool.Count > 0) // Reuse a pooled coin if one is available. GK
        {
            newCoin = coinPool[coinPool.Count - 1]; // Take the last coin from the pool. GK
            coinPool.RemoveAt(coinPool.Count - 1); // Remove it from the pool. GK
            newCoin.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity); // Move it to the spawn position. GK
        }
        else // No pooled coin available, so instantiate a new one. GK
        {
            newCoin = Instantiate(coin, spawnPosition, Quaternion.identity);
        }

        newCoin.gameObject.SetActive(true); // Make sure it's active either way. GK
        newCoin.ResetPickup(value); // Reset its state for this spawn. GK
    }

    public void ReturnToPool(CoinPickup coinToReturn) // Function to return a picked-up coin to the pool instead of destroying it. GK
    {
        coinToReturn.gameObject.SetActive(false); // Deactivate it. GK
        coinPool.Add(coinToReturn); // Add it back to the pool for reuse. GK
    }
    public void SpendCoins(int coinsToSpend) // Spend coins. GK
    {
        currentCoins -= coinsToSpend; // Subtract the coins. GK
        UIController.instance.UpdateCoins(); // Update the coins. GK
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}