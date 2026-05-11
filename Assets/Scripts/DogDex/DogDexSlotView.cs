using UnityEngine;
using UnityEngine.UI;

public class DogDexSlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private GameObject lockedMark;
    [SerializeField] private Sprite lockedIcon;

    public void SetEntry(DogDexEntry entry, bool isUnlocked)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = isUnlocked ? entry.Icon : lockedIcon;
            iconImage.color = isUnlocked ? Color.white : Color.gray;
            iconImage.enabled = iconImage.sprite != null;
        }

        if (nameText != null)
        {
            nameText.text = isUnlocked ? entry.DisplayName : "???";
        }

        if (levelText != null)
        {
            levelText.text = isUnlocked ? $"Lv. {entry.Level}" : "Locked";
        }

        if (lockedMark != null)
        {
            lockedMark.SetActive(!isUnlocked);
        }
    }
}
