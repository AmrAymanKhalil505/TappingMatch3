using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType variation;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;

    private ColorType _variation;

    public ColorType Color
    {
        get => _variation;
        set => SetColor(value);
    }

    public int NumColors => colorSprites.Length;

    private SpriteRenderer _sprite;
    private Dictionary<ColorType, Sprite> _colorSpriteDict;

    void Awake ()
    {
        //
        _sprite = transform.Find("piece").GetComponent<SpriteRenderer>();
        // instantiating and populating a Dictionary of all Color Types / Sprites (for fast lookup)
        _colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!_colorSpriteDict.ContainsKey (colorSprites[i].variation))
            {
                _colorSpriteDict.Add(colorSprites[i].variation, colorSprites[i].sprite);
            }
        }
        
    }

    public void SetColor(ColorType newColor)
    {
        _variation = newColor;
        
        if (_colorSpriteDict.ContainsKey(newColor))
        {
            _sprite.sprite = _colorSpriteDict[newColor];
        }
        
    }
	
}
