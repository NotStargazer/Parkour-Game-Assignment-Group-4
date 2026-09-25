using UnityEngine;

public class TokenPickupAndSound : MonoBehaviour, ILevelObject
{
    [Header("Score Settings")]
    public int scoreValue = 5; 

    [Header("Feedback Assets")]
    public AudioClip sfxClip;  
    public GameObject vfxPrefab; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Connect to Midas' GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Score += scoreValue;
                Debug.Log($"Token collected! Added {scoreValue} points to GameManager. Total Score: {GameManager.Instance.Score}");
            }

            // Audio feedback
            if (sfxClip != null)
            {
                AudioSource.PlayClipAtPoint(sfxClip, transform.position);
            }

            // Visual feedback
            if (vfxPrefab != null)
            {
                Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            }

           
            // This hides the token and turns off its collision without deleting it from memory
            gameObject.SetActive(false);
        }
    }
}

