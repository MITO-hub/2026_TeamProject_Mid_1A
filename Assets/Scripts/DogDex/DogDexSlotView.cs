using UnityEngine;
using UnityEngine.UI;

public class DogDexSlotView : MonoBehaviour
{
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

        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = isUnlocked ? entry.Icon : lockedIcon;
            iconImage.color = isUnlocked ? Color.white : Color.gray;
            iconImage.enabled = iconImage.sprite != null;
        }

        if (nameText != null)
        {
            string label = isUnlocked ? entry.DisplayName : "???";
            nameText.text = $"{entry.Id}. {label}";
        }

        if (lockedMark != null)
        {
            lockedMark.SetActive(!isUnlocked);
        }
    }
}
