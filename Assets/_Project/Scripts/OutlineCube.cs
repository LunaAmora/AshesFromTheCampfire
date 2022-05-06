using UnityEngine;

namespace Ashes
{
    [ExecuteAlways]
    public class OutlineCube : MonoBehaviour
    {
        public Color Color;
        public Color SecondColor;
        [Range(0, 1)]
        public float Scale;
        [Range(-0.5f, 0.5f)]
        public float OutlineSize;
        public bool Invert = false;

        public static float variance = 2f;
        private static float _tween = 0;
        private static LTDescr lTDescr;

        private MaterialPropertyBlock _propBlock;
        private Renderer _renderer;

        private Renderer Renderer
        {
            get
            {
                if(_renderer == null)
                {
                    _renderer = GetComponent<Renderer>(); 
                }

                return _renderer;
            }
        }

        private MaterialPropertyBlock PropertyBlock
        {
            get
            {
                if (_propBlock == null)
                {
                    _propBlock = new MaterialPropertyBlock();
                }
                return _propBlock;
            }
        }

        private float TweenValue
        {
            get
            {
                if (Application.isPlaying && lTDescr == null)
                {
                    lTDescr = LeanTween.value(_tween,  1 - _tween, 1F)
                        .setOnUpdate((a) => _tween = a)
                        .setEase(LeanTweenType.easeInOutElastic)
                        .setOnComplete(() => lTDescr = null);
                }
                return Invert? 1 - _tween : _tween;
            }
        }

        private Color TweenColor
        {
            get
            {
                // return !Invert? Color : SecondColor;
                return Color.Lerp(Color, SecondColor, TweenValue);
            }
        }

        public void Initialize(Color color, float scale, float outlineSize)
        {
            Color = color;
            Scale = scale;
            OutlineSize = outlineSize;
        }

        public void Update()
        {
            MaterialPropertyBlock propertyBlock = PropertyBlock;
            Renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Outline", TweenColor);
            propertyBlock.SetFloat("_Size", Scale * (1 + (TweenValue * variance)));
            propertyBlock.SetFloat("_OutlineSize", OutlineSize / (1 + (TweenValue  * variance)));
            Renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
