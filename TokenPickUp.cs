using UnityEngine;

public class TokenPickUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)

    {

        if (other.CompareTag("Player"))
        {
            Debug.Log("Token Collected");


            Destroy(gameObject);
        
        }
    }
}