using System;
using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerGUIs
{
 
    public abstract class StatBar : MonoBehaviour
    {
       [Required] public RectTransform layoutParent;
       [Required] public BarImage barImage;
       public int statToBarRatio = 2;


       private BarImage[] _barImages;
       private int _head;
       private Subject<Unit> _onValueChanged = new(); 

       public abstract IReadOnlyReactiveProperty<int> GetMaxValue();
       public abstract IReadOnlyReactiveProperty<int> GetCurrentValue();
       
       protected abstract IObservable<Unit> Redraw();
       
       public IObservable<Unit> OnValueChanged => _onValueChanged;

       protected abstract bool IsReady();


       public Rect GetRect()
       {
           float minX = float.MaxValue; float maxX = float.MinValue;
            float minY = float.MaxValue; float maxY = float.MinValue;
            foreach (var image in _barImages)
            {
                var r = image.img.rectTransform.rect;
                var rminx = r.xMin + image.img.rectTransform.offsetMin.x;
                var rmaxx = r.xMax + image.img.rectTransform.offsetMax.x;
                var rminy = r.yMin + image.img.rectTransform.offsetMin.y;
                var rmaxy = r.yMax + image.img.rectTransform.offsetMax.y;
                if (rminx < minX) minX = rminx; 
                if(rmaxx > maxX) maxX = rmaxx;
                if (rminy < minY) minY = rminy;
                if (rmaxy > maxY) maxY = rmaxy;
            }
            return Rect.MinMaxRect(minX, minY, maxX, maxY);
       }

       private int Max => Mathf.RoundToInt(GetMaxValue().Value);
       public float T => GetCurrentValue().Value / (float)GetMaxValue().Value;
       private int Count => Max/statToBarRatio;
       
       private BarImage HeadImage => _barImages[_head];

       private IEnumerator Start()
       {
           while (!IsReady())
           {
               yield return null;
           }
           Debug.Assert(GetMaxValue() != null, "Max Value Stream is null", this);
           Debug.Assert(GetCurrentValue() != null, "Current Value Stream is null", this);
           var valueChangedStream =  GetMaxValue().Merge(GetCurrentValue()).Merge(Redraw().Select(_ => 0)).StartWith(0);
           valueChangedStream.Subscribe(_ => OnValuesChanged()).AddTo(this);
           valueChangedStream.AsUnitObservable().Subscribe(_onValueChanged).AddTo(this);
       }

       private void OnValuesChanged()
       {
           var t = T;
           var c = Count;
           if (_barImages == null || c != _barImages.Length) OnCountChanged(c);
           var tc = t * _barImages.Length;
           var head = Mathf.FloorToInt(tc);
           if(head == _barImages.Length)head--;
           var headFullness = (tc - head);
           if (_head != head) OnHeadChanged(head);
           HeadImage.Set(headFullness);
       }

       private void OnHeadChanged(int newHead)
       {
           for (int i = 0; i < newHead; i++)
           {
               _barImages[i].Set(1);
           }
           
           
           for (int i = newHead+1; i < _barImages.Length; i++)
           {
               _barImages[i].Set(0);
           }

           _head = newHead;
       }

       private void OnCountChanged(int newCount)
       {
           if (_barImages == null)
           {
               _barImages = new BarImage[newCount];
               for (int i = 0; i < newCount; i++)
               {
                   _barImages[i] = Instantiate(barImage, layoutParent);
               }
           }
           else if (newCount > _barImages.Length)
           {
               var tmp = _barImages;
               _barImages = new BarImage[newCount];
               for (int i = 0; i < tmp.Length; i++)
               {
                   _barImages[i] = tmp[i];
               }
               for (int i = tmp.Length; i < newCount; i++)
               {
                   _barImages[i] = Instantiate(barImage, layoutParent);
               }
           }
           else if (newCount < _barImages.Length)
           {
               for (int i = newCount; i < _barImages.Length; i++)
               {
                   _barImages[i].gameObject.SetActive(false);
               }

               for (int i = 0; i < newCount; i++)
               {
                   _barImages[i].gameObject.SetActive(true);
               }
           }
       }
    }
}