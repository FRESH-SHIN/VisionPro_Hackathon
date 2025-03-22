using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
public class HandGesture : MonoBehaviour
{
    [SerializeField] private Transform _polySpatialCameraTransform;
    [SerializeField] private string targetTag = "Movable"; // 操作対象のタグ
    private XRHandSubsystem _handSubsystem;
    private XRHandJoint _rightIndexTipJoint;
    private XRHandJoint _rightThumbTipJoint;
    private XRHandJoint _leftIndexTipJoint;
    private XRHandJoint _leftThumbTipJoint;

    private bool _activeRightPinch;
    private bool _activeLeftPinch;
    private float _scaledThreshold;

    private const float k_PinchThreshold = 0.02f;
    // 前フレームの両手間ピンチ距離を保持するための変数
    private float _previousPinchDistance = 0f;

    private Vector3 _previousMidPoint = Vector3.zero;
    private Vector3 _previousHandLine = Vector3.zero;
    public bool _gestureActive = false;

    // 対象オブジェクトのリスト
    GameObject[] _targetObjects;
    
    private void Start()
    {
        GetHandSystem();
        _scaledThreshold = k_PinchThreshold / _polySpatialCameraTransform.localScale.x;
    }

    private void Update()
    {
        if(_targetObjects == null)
        {
            _targetObjects = GameObject.FindGameObjectsWithTag(targetTag);
            Debug.Log("Target Objects counts:" + _targetObjects.Length.ToString());
        }

        if (!CheckHandSubsystem()) return;

        XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags = _handSubsystem.TryUpdateHands(XRHandSubsystem.UpdateType.Dynamic);
        
        if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose) != 0)
        {
            // assign joint values
            _rightIndexTipJoint = _handSubsystem.rightHand.GetJoint(XRHandJointID.IndexTip);
            _rightThumbTipJoint = _handSubsystem.rightHand.GetJoint(XRHandJointID.ThumbTip);
            
            DetectPinch(_rightIndexTipJoint, _rightThumbTipJoint, ref _activeRightPinch, true);
        }

        if ((updateSuccessFlags & XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose) != 0)
        {
            // assign joint values.
            _leftIndexTipJoint = _handSubsystem.leftHand.GetJoint(XRHandJointID.IndexTip);
            _leftThumbTipJoint = _handSubsystem.leftHand.GetJoint(XRHandJointID.ThumbTip);
            
            DetectPinch(_leftIndexTipJoint, _leftThumbTipJoint, ref _activeLeftPinch, false);
        }

        // 両手がピンチ状態の場合、ズーム操作および回転軸の計算を実施
// 両手がピンチ状態の場合に拡大・縮小・回転処理を実施
        if (_activeRightPinch && _activeLeftPinch)
        {
            // 各手のピンチ位置（インデックスとサム先端の中間点）を算出
            Vector3 rightPinchPosition = Vector3.zero;
            Vector3 leftPinchPosition = Vector3.zero;

            if (_rightIndexTipJoint.TryGetPose(out Pose rightIndexPose) &&
                _rightThumbTipJoint.TryGetPose(out Pose rightThumbPose))
            {
                rightPinchPosition = (rightIndexPose.position + rightThumbPose.position) * 0.5f;
            }

            if (_leftIndexTipJoint.TryGetPose(out Pose leftIndexPose) &&
                _leftThumbTipJoint.TryGetPose(out Pose leftThumbPose))
            {
                leftPinchPosition = (leftIndexPose.position + leftThumbPose.position) * 0.5f;
            }

            // 両手の中間点をpivotとして使用
            Vector3 midPoint = (rightPinchPosition + leftPinchPosition) * 0.5f;

            // 現在の両手間距離と手同士を結ぶ方向ベクトルを算出
            float currentPinchDistance = Vector3.Distance(rightPinchPosition, leftPinchPosition);
            Vector3 currentHandLine = (rightPinchPosition - leftPinchPosition).normalized;

            // 両手のラインと中間点方向の外積から回転軸を求める
            Vector3 midPointDirection = (midPoint - _polySpatialCameraTransform.position).normalized;
            Vector3 rotationAxis = _polySpatialCameraTransform.up; 

            if (!_gestureActive)
            {
                // 初回フレームなら前回情報を初期化
                _previousPinchDistance = currentPinchDistance;
                _previousHandLine = currentHandLine;
                _gestureActive = true;
            }
            else
            {
                

                // スケール計算：前フレームと比較して距離の比率を倍率とする
                float scaleFactor = currentPinchDistance / _previousPinchDistance;

                // 回転角は前フレームの手の方向と現在の手の方向の差分から算出
                float rotationAngle = Vector3.SignedAngle(_previousHandLine, currentHandLine, rotationAxis);

                // プレイヤーを中心（pivot）として、各対象オブジェクトに対して拡大・縮小と回転を適用
                Vector3 pivot = _polySpatialCameraTransform.position;
                foreach (GameObject obj in _targetObjects)
                {
                    // midPointを中心に拡大・縮小させる
                    Vector3 directionFromPivot = obj.transform.position - midPoint;
                    Vector3 scaledDirection = directionFromPivot * scaleFactor;
                    obj.transform.position = midPoint + scaledDirection;
                    //obj.transform.localScale *= scaleFactor;

                    // // 回転処理：midpointを中心に回転させる
                    // Quaternion deltaRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
                    // obj.transform.RotateAround(_polySpatialCameraTransform.position, rotationAxis, rotationAngle);
                    obj.transform.RotateAround(pivot, rotationAxis, rotationAngle);
                }

                // 前フレーム情報を更新
                _previousPinchDistance = currentPinchDistance;
                _previousHandLine = currentHandLine;
            }
        }
        else
        {
            // どちらかの手がピンチ状態でなくなったらジェスチャー終了
            _gestureActive = false;
            _previousPinchDistance = 0f;
            _previousHandLine = Vector3.zero;
        }


    }

    private void GetHandSystem()
    {
        XRGeneralSettings xrGeneralSettings = XRGeneralSettings.Instance;
        if (xrGeneralSettings == null)
        {
            Debug.LogError("XR general settings not set.");
            return;
        }

        XRManagerSettings manager = xrGeneralSettings.Manager;
        if (manager == null)
        {
            Debug.LogError("XR Manager Settings not set.");
            return;
        }

        XRLoader loader = manager.activeLoader;
        if (loader == null)
        {
            Debug.LogError("XR Loader not set.");
            return;
        }

        _handSubsystem = loader.GetLoadedSubsystem<XRHandSubsystem>();
        if (!CheckHandSubsystem())
        {
            return;
        }

        _handSubsystem.Start();
    }

    private bool CheckHandSubsystem()
    {
        if (_handSubsystem == null)
        {
#if !UNITY_EDITOR
            Debug.LogError("Could not find Hand Subsystem.");
#endif
            enabled = false;
            return false;
        }

        return true;
    }

    private void DetectPinch(XRHandJoint index, XRHandJoint thumb, ref bool activeFlag, bool right)
    {

        if (index.trackingState == XRHandJointTrackingState.None ||
            thumb.trackingState == XRHandJointTrackingState.None)
        {
            Debug.LogWarning("Index or thumb tracking state is None.");
            return;
        }

        Vector3 indexPosition = Vector3.zero;
        Vector3 thumbPosition = Vector3.zero;

        if (index.TryGetPose(out Pose indexPose))
        {
            indexPosition = indexPose.position;
        }

        if (thumb.TryGetPose(out Pose thumbPose))
        {
            thumbPosition = thumbPose.position;
        }

        float pinchDistance = Vector3.Distance(indexPosition, thumbPosition);
        if (pinchDistance <= _scaledThreshold)
        {
            if (!activeFlag)
            {
                activeFlag = true;
            }
        }
        else
        {
            activeFlag = false;
        }
    }
    
}
