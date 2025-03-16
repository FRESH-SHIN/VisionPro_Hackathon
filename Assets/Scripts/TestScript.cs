using XNode;
using UnityEngine;
using Apple.PHASE;
using System.Reflection;
public class TestScript : MonoBehaviour
{
    public AudioClip newClip; 
    
    void Start()
    {
        PHASESource phaseSource = this.gameObject.AddComponent<PHASESource>(); //phase source作成
        PHASESoundEventNodeGraph graph = ScriptableObject.CreateInstance<PHASESoundEventNodeGraph>();
        PHASESoundEventSamplerNode samplerNode = graph.AddNode<PHASESoundEventSamplerNode>();
        PHASESpatialMixer phaseSpatialMixer = graph.AddNode<PHASESpatialMixer>();
        NodePort samplerOutput = samplerNode.GetOutputPort("Mixer"); 
        NodePort spatialInput = phaseSpatialMixer.GetInputPort("ParentNode"); 
        // 포트 연결
        if (samplerOutput != null && spatialInput != null)
        {
            samplerOutput.Connect(spatialInput);
            Debug.Log("Successfully connected PHASESoundEventSamplerNode -> PHASESpatialMixer");　//ノードコネクト
        }
        else
        {
            Debug.LogError("Failed to get ports for connection");
        }
        SetSoundEvent(phaseSource, graph);
        ChangeAudioClip(samplerNode, newClip);
        
        
    }
    public void ChangeAudioClip(PHASESoundEventSamplerNode samplerNode, AudioClip audioClip)
    {
        if (samplerNode == null || audioClip == null)
        {
            return;
        }

        // Reflectionでフィルドを無理やり変える
        FieldInfo audioClipField = typeof(PHASESoundEventSamplerNode).GetField("_audioClip", BindingFlags.NonPublic | BindingFlags.Instance);
        if (audioClipField != null)
        {
            audioClipField.SetValue(samplerNode, audioClip);
            Debug.Log("ChangeAudioClip: "+ audioClip.name);
        }
        else
        {
            Debug.LogError("cannot access AudioClip");
        }
    }

    public void SetSoundEvent(PHASESource phaseSource, PHASESoundEventNodeGraph graph)
    {
        //こいつも
        FieldInfo soundEventField = typeof(PHASESource).GetField("_soundEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (soundEventField != null)
        {
            soundEventField.SetValue(phaseSource, graph);
        }
        else
        {
            Debug.LogError("");
        }
        
    }
}