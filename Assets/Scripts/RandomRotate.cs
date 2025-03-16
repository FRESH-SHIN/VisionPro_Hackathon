using UnityEngine;
using DG.Tweening;

public class RandomRotate : MonoBehaviour
{
    void Start()
    {
        Rotate();
    }

    void Rotate()
    {
        transform.DORotate(new Vector3(Random.Range(0, 720), Random.Range(0, 720), Random.Range(0, 720)), 3f).OnComplete(() => Rotate());
    }
}
