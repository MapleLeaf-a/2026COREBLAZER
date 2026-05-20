using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SimpleTutorialPlayer : MonoBehaviour
{
    [Header("ΩÃ≥Ã…Ë÷√")]
    public List<Sprite> tutorialImages;
    public GameObject tutorialPanel;
    public Image tutorialImage;
    public Button playButton;

    private int currentPage = 0;
    private bool isPlaying = false;

    void Start()
    {
        tutorialPanel.SetActive(false);
        playButton.onClick.AddListener(StartTutorial);
    }

    void Update()
    {
        if (isPlaying && Input.GetMouseButtonDown(0))
        {
            NextPage();
        }
    }

    public void StartTutorial()
    {
        if (tutorialImages.Count == 0) return;

        currentPage = 0;
        isPlaying = true;
        tutorialPanel.SetActive(true);
        UpdateImage();
    }

    private void NextPage()
    {
        if (currentPage + 1 < tutorialImages.Count)
        {
            currentPage++;
            UpdateImage();
        }
        else
        {
            EndTutorial();
        }
    }

    private void UpdateImage()
    {
        tutorialImage.sprite = tutorialImages[currentPage];
    }

    private void EndTutorial()
    {
        isPlaying = false;
        tutorialPanel.SetActive(false);
    }
}