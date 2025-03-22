using UnityEngine;
using DG.Tweening;

public class RandomScale : MonoBehaviour
{
    [SerializeField] float tweenDuration = 1f;
    [SerializeField] float maxScale = 1.5f;
    [SerializeField] float minScale = 0.8f;

    void Start()
    {
        transform.localScale = Vector3.one * minScale;
        Scale();
    }

    void Scale()
    {
        transform.DOScale(Vector3.one * maxScale, tweenDuration)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
