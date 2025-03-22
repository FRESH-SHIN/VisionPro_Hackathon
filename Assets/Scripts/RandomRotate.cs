using UnityEngine;
using DG.Tweening;

public class RandomRotate : MonoBehaviour
{
    [SerializeField] float tweenDuration = 1f;
    [SerializeField] float rotationAngle = 180f;
    private Vector3 randomVector;
    public static Vector3 RandomVector3()
    {
        Vector3 v = Vector3.zero;
        v.x = Random.Range(-1.0f, 1.0f);
        v.y = Random.Range(-1.0f, 1.0f);
        v.z = Random.Range(-1.0f, 1.0f);
        return v.normalized;
    }
    void Start()
    {
        randomVector = RandomVector3();
        Rotate();
    }

    void Rotate()
    {
        transform.DORotate(Vector3.Cross(Vector3.one * rotationAngle, randomVector), tweenDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }
}
