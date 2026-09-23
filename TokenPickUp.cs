using UnityEngine;

public class TokenPickup : MonoBehaviour
{
    // Detta gör att ni kan ändra poängvärdet (5 eller 20) direkt i Unitys Inspector
    public int scoreValue = 5; 

    private void OnTriggerEnter(Collider other)
    {
        // 1. Kolla om det är spelaren som krockar
        if (other.CompareTag("Player"))
        {
            // 2. Leta efter poängväskan på spelaren
            PlayerInventory playerInv = other.GetComponent<PlayerInventory>();

            // 3. Om väskan finns, skicka över poängen
            if (playerInv != null)
            {
                playerInv.AddPoints(scoreValue);
            }

            // 4. Logga i konsolen och ta bort token från banan
            Debug.Log("Token Collected!");
            Destroy(gameObject);
        }
    }
}
