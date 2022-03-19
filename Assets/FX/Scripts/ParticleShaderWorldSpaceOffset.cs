using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class ParticleShaderWorldSpaceOffset : MonoBehaviour
{
    Renderer _renderer;

    static readonly int WorldPos = Shader.PropertyToID("WorldPos");

    MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);
        Vector3 existingWorldPos = _propBlock.GetVector(WorldPos);
        if (existingWorldPos != transform.position)
        {
            _propBlock.SetVector(WorldPos, transform.position);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}