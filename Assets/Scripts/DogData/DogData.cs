using UnityEngine;

[CreateAssetMenu(fileName = "DogData", menuName = "Dog/Dog Data")]


public class DogData : ScriptableObject
{
    public int ID;
    public string dogName;
    public int level;
    public Sprite sprite;
    public int nextDogID;   // 다음 단계 강아지 ID
}