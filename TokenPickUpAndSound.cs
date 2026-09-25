using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int totalScore = 0;

    public void AddPoints(int pointsToAdd)
    {
        totalScore += pointsToAdd;
    
    }
}
