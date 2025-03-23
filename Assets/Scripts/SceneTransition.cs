using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    public ScrollRect scrollRect;
    // Tween の再生時間（秒）
    public float tweenDuration = 0.5f;

    private float currentPosition = 0;

    public float[] scrollPositions;

    public void SceneChange(string targetScene)
    {
        SceneManager.LoadScene(targetScene);
    }

    public void NextButton()
    {
        Debug.Log($"{currentPosition}");
        switch (currentPosition)
        {
            case 0:
                scrollRect.DOHorizontalNormalizedPos(scrollPositions[1], tweenDuration);
                currentPosition++;
                break;
            case 1:
                scrollRect.DOHorizontalNormalizedPos(scrollPositions[2], tweenDuration);
                currentPosition++;
                break;
        }
        Debug.Log($"{currentPosition}");
    }

    public void PrevButton()
    {
        switch (currentPosition)
        {
            case 1:
                scrollRect.DOHorizontalNormalizedPos(scrollPositions[0], tweenDuration);
                currentPosition--;
                break;
            case 2:
                scrollRect.DOHorizontalNormalizedPos(scrollPositions[1], tweenDuration);
                currentPosition--;
                break;
        }
        Debug.Log("Prev");
    }
}
