using UnityEngine;

public class TokenPickup : MonoBehaviour, ILevelObject

{
   
    public int scoreValue = 5; 

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            PlayerInventory playerInv = other.GetComponent<PlayerInventory>();

            
            if (playerInv != null)
            {
                playerInv.AddPoints(scoreValue);
            }

            
            Debug.Log("Token Collected!");
            Destroy(gameObject);
        }
    }
}
