using System.Collections.Generic;
using Apple.PHASE;
using UnityEngine;
using UnityEngine.Animations;

public class SourceGenerator : MonoBehaviour
{
    // AudioClip のリスト
    public List<AudioClip> audioClips;
    // 配置するスピーカのPrefab(殻)
    public GameObject speakerShellPrefab;
    // 配置するスピーカのPrefab(核)
    public GameObject speakerCorePrefab;
    // カメラ（中心となるオブジェクト）
    public GameObject camera;
    // 配置する半径（任意の値に調整可能）
    public float radius = 1f;

    // Start is called before the first frame update
    void Start()
    {
        int count = audioClips.Count;
        if (count == 0) return;

        double gain = 1.0 / count;

        for (int i = 0; i < count; i++)
        {
            // 各スピーカを均等な角度で配置 (角度は度単位)
            float angle = i * (360f / count);
            float angleRad = angle * Mathf.Deg2Rad;

            // カメラの位置を中心に、指定した半径で計算
            Vector3 pos = camera.transform.position + new Vector3(Mathf.Cos(angleRad) * radius, 1f, Mathf.Sin(angleRad) * radius);
            // スピーカがカメラの方向を向くように回転を設定
            Quaternion rotation = Quaternion.LookRotation(camera.transform.position - pos);

            // スピーカの生成
            GameObject speakerShell = Instantiate(speakerShellPrefab, pos, rotation);
            speakerShell.transform.SetParent(transform, true);

            // スピーカの生成
            GameObject speakerCore = Instantiate(speakerCorePrefab, pos, rotation);
            speakerCore.transform.SetParent(transform, true);

            // speakerCore の PositionConstraint に speakerShell をソースとして追加
            PositionConstraint posConstraint = speakerCore.GetComponent<PositionConstraint>();
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = speakerShell.transform;
            source.weight = 1.0f;
            posConstraint.AddSource(source);
            posConstraint.constraintActive = true;


            // AudioClip に対応する DynamicSoundEvent の初期化
            DynamicSoundEvent dynamicSoundEvent = speakerShell.AddComponent<DynamicSoundEvent>();
            Debug.Log(audioClips[i].name);
            dynamicSoundEvent.Init(audioClips[i]);

            // gain 設定
            PHASESource phaseSource = speakerShell.GetComponent<PHASESource>();
            phaseSource.SetGain(gain);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 必要に応じて更新処理を追加
    }
}