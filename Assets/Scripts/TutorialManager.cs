using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField]
    public GameObject[] tutorialObjects;

    public int currentTutorialSlide = 0;

    public void NextTutorialSlide(int currentNum) // Current num needed to make sure you cant skip any parts of the tutorial on accident
    {
        if (currentNum != currentTutorialSlide)
        {
            return;
        }
        tutorialObjects[currentTutorialSlide].SetActive(false);
        tutorialObjects[currentTutorialSlide].transform.parent.gameObject.SetActive(false);

        if (currentTutorialSlide + 1 >= tutorialObjects.Length) return;

        tutorialObjects[currentTutorialSlide + 1].SetActive(true);
        tutorialObjects[currentTutorialSlide + 1].transform.parent.gameObject.SetActive(true);
        currentTutorialSlide++;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (currentTutorialSlide < 3 || currentTutorialSlide == 6 || currentTutorialSlide == 8 || (currentTutorialSlide > 10 && currentTutorialSlide < 15)))
        {
            NextTutorialSlide(currentTutorialSlide);
        }
    }
}
