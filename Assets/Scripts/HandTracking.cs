using UnityEngine;
using Unity.PolySpatial.InputDevices;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using UnityEngine.InputSystem.EnhancedTouch;
using Apple.PHASE;
using System;

public class HandTracking : MonoBehaviour
{
    GameObject m_SelectedObject;
    public float sensitivity = 1f; 
    // 最大期待 deltaAngle (ラジアン単位) 例: 0.2f (約11.5°)
    public float maxExpectedDeltaAngle = 0.2f; 

    // 前回の角度（度単位）を保持する
    float previousAngle;
    bool isFirstMovedFrame = true;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void Start()
    {
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count > 0)
        {
            SpatialPointerState primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouches[0]);

            if (primaryTouchData.Kind == SpatialPointerKind.DirectPinch)
            {
                if (activeTouches[0].phase == TouchPhase.Began)
                {
                    GameObject selected = primaryTouchData.targetObject != null ? primaryTouchData.targetObject : null;
                    Debug.Log("Touch began");
                    if (selected != null && selected.CompareTag("Movable"))
                    {
                        m_SelectedObject = selected;
                        isFirstMovedFrame = true; 
                    }
                }
            }

            if (activeTouches[0].phase == TouchPhase.Moved)
            {
                if (m_SelectedObject != null)
                {
                    // オブジェクトの位置と回転の更新
                    m_SelectedObject.transform.SetPositionAndRotation(primaryTouchData.interactionPosition, primaryTouchData.inputDeviceRotation);

                    // 毎フレーム、手とオブジェクト間の線（軸）を再計算
                    Vector3 handPos = primaryTouchData.interactionPosition;
                    Vector3 objPos = m_SelectedObject.transform.position;
                    Vector3 axis = (objPos - handPos).normalized;

                    // 軸に対して、まず world up を平面投影して参照方向とする
                    Vector3 reference = Vector3.ProjectOnPlane(Vector3.up, axis).normalized;
                    if (reference == Vector3.zero)
                    {
                        // 万一、world up と軸が平行なら world forward を使用
                        reference = Vector3.ProjectOnPlane(Vector3.forward, axis).normalized;
                    }

                    // オブジェクトの up ベクトルを軸に対して平面投影し、参照方向との角度（度単位）を取得
                    Vector3 currentProjectedUp = Vector3.ProjectOnPlane(m_SelectedObject.transform.up, axis).normalized;
                    float currentAngle = Vector3.SignedAngle(reference, currentProjectedUp, axis);

                    float deltaAngleDegrees = 0f;
                    if (isFirstMovedFrame)
                    {
                        // 初回フレームは前回値がないので delta は 0
                        deltaAngleDegrees = 0f;
                        previousAngle = currentAngle;
                        isFirstMovedFrame = false;
                    }
                    else
                    {
                        // Mathf.DeltaAngle を使うことで、-180～180°の範囲で最小の角度差を求める
                        deltaAngleDegrees = Mathf.DeltaAngle(previousAngle, currentAngle);
                        previousAngle = currentAngle;
                    }
                    
                    // 角度差をラジアンに変換
                    float deltaAngleRad = deltaAngleDegrees * Mathf.Deg2Rad;
                    // sensitivity を掛けた生の値を算出し、maxExpectedDeltaAngle を用いて正規化 [0,1]
                    float rawValue = deltaAngleRad * sensitivity;
                    // PHASESource の gain を更新
                    PHASESource phaseSource = m_SelectedObject.GetComponent<PHASESource>();
                    double gain = Mathf.Clamp01((float)(rawValue + phaseSource.GetGain()));
                    Debug.Log("Gain：" + gain);
                    if (phaseSource != null)
                    {
                        phaseSource.SetGain(gain);
                    }
                }
            }

            if (activeTouches[0].phase == TouchPhase.Ended || activeTouches[0].phase == TouchPhase.Canceled)
            {
                m_SelectedObject = null;
            }
        }
    }
}
