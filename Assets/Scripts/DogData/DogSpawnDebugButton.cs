using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DogSpawnDebugButton : MonoBehaviour
{
    [SerializeField] private DogSpawner spawner;
    [SerializeField] private int dogId = 1;

    private void Awake()
    {
        EnsureEventSystem();

        if (spawner == null)
        {
            spawner = FindAnyObjectByType<DogSpawner>();
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(SpawnDog);
            button.onClick.AddListener(SpawnDog);
        }
    }

    public void SpawnDog()
    {
        if (spawner == null)
        {
            spawner = FindAnyObjectByType<DogSpawner>();
        }

        if (spawner == null)
        {
            Debug.LogError("DogSpawner not found.");
            return;
        }

        spawner.SpawnDog(dogId);
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
