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

    public void SetEntry(DogDexEntry entry, bool isUnlocked)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        this.entry = entry;
        gameObject.SetActive(true);

        if (frameImage != null)
        {
            frameImage.color = isUnlocked ? new Color(0.92f, 0.92f, 0.88f) : new Color(0.62f, 0.62f, 0.60f);
        }

        if (iconImage != null)
        {
            iconImage.sprite = isUnlocked ? entry.Icon : lockedIcon;
            iconImage.color = isUnlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.45f);
            iconImage.enabled = iconImage.sprite != null;
        }

        if (nameText != null)
        {
            nameText.text = entry.Id.ToString();
        }

        if (lockedMark != null)
        {
            lockedMark.SetActive(!isUnlocked);
        }
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
