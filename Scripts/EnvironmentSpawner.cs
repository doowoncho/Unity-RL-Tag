using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    public GameObject environmentPrefab;
    public int count = 8;
    public float spacing = 25f;

    //Parallel simulations
    void Awake()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(i * spacing, 0, 0);
            Instantiate(environmentPrefab, position, Quaternion.identity);
        }
    }
}