using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
public class HandGesture : MonoBehaviour
{
    [SerializeField] private Transform _polySpatialCameraTransform;

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
    private void Start()
    {
        GetHandSystem();
        _scaledThreshold = k_PinchThreshold / _polySpatialCameraTransform.localScale.x;
    }

    private void Update()
    {
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
        if (_activeRightPinch && _activeLeftPinch)
        {
            // 各手のピンチ位置を、インデックスとサム先端の平均位置として求める
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

            // 両手の間の距離を計算
            float currentPinchDistance = Vector3.Distance(rightPinchPosition, leftPinchPosition);

            // 初回フレームの場合は前回距離を初期化
            if (_previousPinchDistance == 0f)
            {
                _previousPinchDistance = currentPinchDistance;
            }

            // 一定の閾値以上の変化があれば、ズーム動作と判定
            if (Mathf.Abs(currentPinchDistance - _previousPinchDistance) > 0.001f)
            {
                if (currentPinchDistance < _previousPinchDistance)
                {
                    Debug.Log("Zoom In Detected");
                }
                else if (currentPinchDistance > _previousPinchDistance)
                {
                    Debug.Log("Zoom Out Detected");
                }
            }

            // 前回の距離を更新
            _previousPinchDistance = currentPinchDistance;

            // 両手の中間点を計算
            Vector3 midPoint = (rightPinchPosition + leftPinchPosition) * 0.5f;

            // カメラから中間点への方向ベクトル（正規化）
            Vector3 midPointDirection = (midPoint - _polySpatialCameraTransform.position).normalized;

            // 両手のピンチ位置を結ぶ方向（正規化）
            Vector3 handLine = (rightPinchPosition - leftPinchPosition).normalized;

            // 回転軸は、手のラインとカメラ方向の外積で求める
            Vector3 rotationAxis = Vector3.Cross(handLine, midPointDirection).normalized;

            Debug.Log("Rotation Axis: " + rotationAxis);
        }
        else
        {
            // どちらかの手がピンチ状態でなくなった場合、前回距離をリセット
            _previousPinchDistance = 0f;
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
