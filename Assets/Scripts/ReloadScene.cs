using Unity.PolySpatial;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.PolySpatial.VolumeCamera;

public class ReloadScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    VolumeCamera volumeCamera;
    void Start()
    {
        volumeCamera = GetComponent<VolumeCamera>();
        volumeCamera.WindowStateChanged.AddListener(VolumeCamera_WindowStateChanged);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void VolumeCamera_WindowStateChanged(object sender,  WindowState e)
    {
    
    }
}
