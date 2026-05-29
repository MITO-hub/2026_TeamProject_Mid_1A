using UnityEngine;
using UnityEngine.UI;

public class DogDexSlotView : MonoBehaviour
{
    [SerializeField] private DogDexEntry entry;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private GameObject lockedMark;
    [SerializeField] private Sprite lockedIcon;
    [SerializeField] private bool alwaysShowLockedView;

    public void SetEntry(DogDexEntry entry, bool isUnlocked)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        this.entry = entry;
        gameObject.SetActive(true);

        bool shouldShowUnlocked = isUnlocked && !alwaysShowLockedView;

        if (frameImage != null)
        {
            frameImage.color = shouldShowUnlocked ? new Color(0.92f, 0.92f, 0.88f) : new Color(0.62f, 0.62f, 0.60f);
        }

        if (iconImage != null)
        {
            iconImage.sprite = shouldShowUnlocked ? entry.Icon : lockedIcon;
            iconImage.color = shouldShowUnlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.45f);
            iconImage.enabled = iconImage.sprite != null;
        }

        if (nameText != null)
        {
            nameText.text = alwaysShowLockedView ? string.Empty : entry.Id.ToString();
        }

        if (lockedMark != null)
        {
            lockedMark.SetActive(!shouldShowUnlocked);
        }
    }

    public void SetAlwaysShowLockedView(bool value)
    {
        alwaysShowLockedView = value;
    }

    public void Refresh(DogDexManager dexManager)
    {
        if (entry == null || dexManager == null)
        {
            return;
        }

        SetEntry(entry, dexManager.IsUnlocked(entry.Id));
    }
}
