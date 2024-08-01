using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEngine;

public class Marker : MonoBehaviour
{
    public Transform markerTip;
    private float tipHeight = 0.02f;
    private WhiteBoard _whiteBoard;
    private Vector2 _touchPos;
    private int penSize = 5;
    private bool _touchLastFrame;
    private Color[] _color;
    private Vector2 _lastTouchPos;
    private Quaternion _lastTouchRot;
    private Renderer _renderer;
    private void Start()
    {
        _renderer = markerTip.GetComponent<Renderer>();
        _color = Enumerable.Repeat(_renderer.material.color, penSize * penSize).ToArray();
    }
    private void Update()
    {
        Write();
    }

    private void Write()
    {
        RaycastHit hit;
        if(Physics.Raycast(markerTip.transform.position, markerTip.transform.forward, out hit, tipHeight))
        {
            
            if(hit.transform.CompareTag("WhiteBoard"))
            {
                Debug.Log("Write");
                if(_whiteBoard == null)
                {
                    _whiteBoard = hit.transform.GetComponent<WhiteBoard>();
                }

                _touchPos = new Vector2(hit.textureCoord.x, hit.textureCoord.y);

                var x = (int)(_touchPos.x * _whiteBoard._textureSize.x - (penSize / 2));
                var y = (int)(_touchPos.y * _whiteBoard._textureSize.y - (penSize / 2));

                if(y < 0 || y > _whiteBoard._textureSize.y || x < 0 || x > _whiteBoard._textureSize.x)
                {
                    return;
                }

                if(_touchLastFrame)
                {
                    _whiteBoard._texture.SetPixels(x, y, penSize, penSize, _color);

                    for (float f = 0.01f; f < 1.00f; f += 0.07f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteBoard._texture.SetPixels(lerpX, lerpY, penSize, penSize, _color);
                    }

                    transform.rotation = _lastTouchRot;
                    _whiteBoard._texture.Apply();
                }

                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchLastFrame = true;
                return;
            }
        }
        _whiteBoard = null;
        _touchLastFrame = false;
    }




}

