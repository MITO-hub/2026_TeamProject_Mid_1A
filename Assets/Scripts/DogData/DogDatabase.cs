using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DogDatabase", menuName = "Dog/Dog Database")]
public class DogDatabase : ScriptableObject
{
    public List<DogData> dogList = new List<DogData>();

    public DogData GetDogByID(int id)
    {
        foreach (DogData dog in dogList)
        {
            if (dog.ID == id)
                return dog;
        }

        return null;
    }
}
