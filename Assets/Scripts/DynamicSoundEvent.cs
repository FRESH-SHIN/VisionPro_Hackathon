using XNode;
using UnityEngine;
using Apple.PHASE;
using System.Reflection;

public class DynamicSoundEvent : MonoBehaviour
{
    // public AudioClip audioClip; 
    
    void Start()
    {
        // PHASESource phaseSource = this.gameObject.AddComponent<PHASESource>(); //phase source作成
        // PHASESoundEventNodeGraph graph = ScriptableObject.CreateInstance<PHASESoundEventNodeGraph>();
        // PHASESoundEventSamplerNode samplerNode = graph.AddNode<PHASESoundEventSamplerNode>();
        // ChangeAudioClip(samplerNode, audioClip);
        // PHASESpatialMixer phaseSpatialMixer = graph.AddNode<PHASESpatialMixer>();
        // NodePort samplerOutput = samplerNode.GetOutputPort("Mixer"); 
        // NodePort spatialInput = phaseSpatialMixer.GetInputPort("ParentNode"); 
        // // 포트 연결
        // if (samplerOutput != null && spatialInput != null)
        // {
        //     samplerOutput.Connect(spatialInput);
        //     Debug.Log("Successfully connected PHASESoundEventSamplerNode -> PHASESpatialMixer");　//ノードコネクト
        // }
        // else
        // {
        //     Debug.LogError("Failed to get ports for connection");
        // }
        // SetSoundEvent(phaseSource, graph);
        
        
        
    }

    public void Init(AudioClip audioClip)
    {
        PHASESource phaseSource = this.gameObject.AddComponent<PHASESource>(); //phase source作成
        PHASESoundEventNodeGraph graph = ScriptableObject.CreateInstance<PHASESoundEventNodeGraph>();
        graph.name = System.Guid.NewGuid().ToString(); //名前を！！！！変えないと！！！！動かないよ！！！！これバグ！！！！修正しろ！！！
        PHASESoundEventSamplerNode samplerNode = graph.AddNode<PHASESoundEventSamplerNode>();
        ChangeAudioClip(samplerNode, audioClip);
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