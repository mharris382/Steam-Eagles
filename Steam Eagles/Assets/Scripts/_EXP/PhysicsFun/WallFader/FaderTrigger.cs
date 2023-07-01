using System;
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
   [RequireComponent(typeof(WallFaderController))]
    public class FaderTrigger : MonoBehaviour
    {
        BoolReactiveProperty _tpEnteredTrigger = new BoolReactiveProperty(false);
        BoolReactiveProperty _bdEnteredTrigger = new BoolReactiveProperty(false);

        private WallFaderController _wallFaderController;
        //private WallFaderBase _wallFader;
        public bool remainFaded = false;
        [SerializeField]
        private int _cnt = 0;

        private int cnt
        {
            get => _cnt;
            set
            {
                _cnt = Mathf.Clamp(value,0, 2);
                if (_cnt > 0)
                {
                    _wallFaderController.FadeOut();
                }
                else
                {
                    _wallFaderController.FadeIn();
                }
            }
        }


        public bool HasTP
        {
            get => _tpEnteredTrigger.Value;
            set => _tpEnteredTrigger.Value = value;
        }
        
        public bool HasBD
        {
            get => _bdEnteredTrigger.Value;
            set => _bdEnteredTrigger.Value = value;
        }
        private void Awake()
        {
            this._wallFaderController = this.GetComponent<WallFaderController>();
            _bdEnteredTrigger = new BoolReactiveProperty(false);
            _tpEnteredTrigger = new BoolReactiveProperty(false);
            _bdEnteredTrigger.Where(t=> t).Select(t=> 1).Subscribe(t => cnt+=t).AddTo(this);
            _tpEnteredTrigger.Where(t=> t).Select(t=> 1).Subscribe(t => cnt+=t).AddTo(this);   
            _bdEnteredTrigger.Where(t=>!t).Select(t=> -1).Subscribe(t => cnt+=t).AddTo(this);
            _tpEnteredTrigger.Where(t=>!t).Select(t=> -1).Subscribe(t => cnt+=t).AddTo(this);   
        }
        
   

        public virtual void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Transporter"))
            {
                _tpEnteredTrigger.Value = true;
            }

            else if (col.CompareTag("Builder"))
            {
                _bdEnteredTrigger.Value = true;
            }
        }
        public virtual void OnTriggerExit2D(Collider2D col)
        {
            if (remainFaded) return;
            if (col.CompareTag("Transporter"))
            {
                _tpEnteredTrigger.Value = false;
            }

            else if (col.CompareTag("Builder"))
            {
                _bdEnteredTrigger.Value = false;
            }
        }
    }
}