using UnityEngine;
using UnityEngine.UI;

public class BoneScatterController : MonoBehaviour
{
    [SerializeField] private RectTransform spawnRoot;
    [SerializeField] private Sprite boneSprite;
    [SerializeField] private Text boneCountText;
    [SerializeField] private Button generateButton;
    [SerializeField] private int maxBones = 5;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-430f, -170f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(470f, 175f);

    private int boneCount;

    private void Awake()
    {
        RefreshState();
    }

    public void SpawnBones()
    {
        if (boneCount >= maxBones)
        {
            RefreshState();
            return;
        }

        if (spawnRoot == null || boneSprite == null)
        {
            Debug.Log("Bone scatter needs a spawn root and sprite.");
            return;
        }

        CreateBone();
        boneCount++;
        RefreshState();
    }

    private void CreateBone()
    {
        GameObject boneObject = new GameObject("GeneratedBone");
        boneObject.transform.SetParent(spawnRoot, false);

        Image image = boneObject.AddComponent<Image>();
        image.sprite = boneSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        RectTransform rect = boneObject.GetComponent<RectTransform>();
        float size = Random.Range(42f, 70f);
        rect.sizeDelta = new Vector2(size * 1.65f, size);
        rect.anchoredPosition = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y));
        rect.localEulerAngles = new Vector3(0f, 0f, Random.Range(-26f, 26f));
    }

    private void RefreshState()
    {
        if (boneCountText != null)
        {
            boneCountText.text = boneCount + "/" + maxBones;
            boneCountText.gameObject.SetActive(true);
        }

        if (generateButton != null)
        {
            generateButton.interactable = boneCount < maxBones;
        }
    }
}
