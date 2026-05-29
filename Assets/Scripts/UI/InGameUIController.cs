using UnityEngine;

public class InGameUIController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject dogDexPanel;
    [SerializeField] private DogDexView dogDexView;
    [SerializeField] private DogDexManager dogDexManager;

    private int nextUnlockDogId = 1;

    private void Awake()
    {
        HideSettings();
        HideDogDex();
    }

    public void StartGame()
    {
        HideSettings();
        HideDogDex();
        Debug.Log("Start button clicked.");
    }

    public void ContinueGame()
    {
        HideSettings();
        HideDogDex();
        Debug.Log("Continue button clicked.");
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ToggleDogDex()
    {
        if (dogDexPanel == null)
        {
            return;
        }

        bool shouldShow = !dogDexPanel.activeSelf;
        dogDexPanel.SetActive(shouldShow);

        if (shouldShow && dogDexView != null)
        {
            dogDexView.Refresh();
        }
    }

    public void HideDogDex()
    {
        if (dogDexPanel != null)
        {
            dogDexPanel.SetActive(false);
        }
    }

    public void GenerateDog()
    {
        if (dogDexManager == null)
        {
            dogDexManager = DogDexManager.Instance;
        }

        if (dogDexManager == null || dogDexManager.Database == null || dogDexManager.Database.Entries.Count == 0)
        {
            Debug.Log("Generate button clicked.");
            return;
        }

        int dogId = dogDexManager.Database.Entries[(nextUnlockDogId - 1) % dogDexManager.Database.Entries.Count].Id;
        nextUnlockDogId++;

        bool unlocked = dogDexManager.UnlockDog(dogId);
        Debug.Log(unlocked ? "Generated and unlocked dog: " + dogId : "Generated already unlocked dog: " + dogId);

        if (dogDexView != null)
        {
            dogDexView.Refresh();
        }
    }

    public void OpenShopPlaceholder()
    {
        Debug.Log("Shop button clicked. Shop flow is not wired yet.");
    }

    public void GenerateBonePlaceholder()
    {
        Debug.Log("Bone generator clicked.");
    }

    public void CloseTopPanel()
    {
        HideSettings();
        HideDogDex();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
