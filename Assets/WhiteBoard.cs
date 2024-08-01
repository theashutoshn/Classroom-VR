using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteBoard : MonoBehaviour
{
    public Texture2D _texture;
    public Vector2 _textureSize = new Vector2(2048, 2048);
    private Material _material;
    private Renderer _renderer;
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _texture = new Texture2D((int)_textureSize.x, (int)_textureSize.y);
        _renderer.material.mainTexture = _texture;
    }

    // Update is called once per frame
    

    
}
