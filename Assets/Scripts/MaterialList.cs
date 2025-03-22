using System.Collections.Generic;
using UnityEngine;

public class MaterialList : MonoBehaviour
{
    [SerializeField] public List<Material> materials;
    [SerializeField] public List<GameObject> objects;
    static public int index = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var obj in objects)
        {
            MeshRenderer mesh = obj.GetComponent<MeshRenderer>();
            mesh.materials = new Material[]{ materials[index]};
        }
        index = index + 1;
        index = index % materials.Count;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
