using UnityEngine;

public class PlayerInventory : MonoBehaviour
{

    public int totalScore = 0;


    public void AddPoints(int PointsToAdd)
    {
        totalScore += PointsToAdd;


        Debug.Log("Total Points" + totalScore);

    }
}
