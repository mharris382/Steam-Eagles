using System;
using GasSim;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIGasTank : MonoBehaviour
    {
        public GasTank gasTank;
        public UnityEvent<float> updateGasTankUI;
        public float maxFill = 0.5f;
        public UnityEngine.UI.Image fillImage;
        private void Awake()
        {
            if(gasTank == null){
                gasTank = GetComponentInParent<GasTank>();
            }
            Debug.Assert(gasTank!=null, "GasTank is null", this);
            gasTank.onAmountNormalizedChanged.AddListener(amt => updateGasTankUI?.Invoke(amt));
            if(fillImage)updateGasTankUI.AddListener(amt => fillImage.fillAmount = amt * maxFill);
        }

    }
}