using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Tools.BuildTool
{
    public enum CraftingStatus
    {
        DISCONNECT,
        BUILD,
        INVALID,
        NONE
    }
    [RequireComponent(typeof(SpriteRenderer))]
    public class ToolTileAimPreview : MonoBehaviour
    {
        public Color disconnectColor = Color.yellow;
        public Color buildColor = Color.green;
        public Color invalidColor = Color.red;

        private SpriteRenderer _sr;
        
        private ToolAimHandler toolAimHandler;
        private ToolContext context;
        public SpriteRenderer Sr => _sr ??= GetComponent<SpriteRenderer>();
        
      

        public void SetMode(CraftingStatus status)
        {
            
            switch (status)
            {
                case CraftingStatus.DISCONNECT:
                    Sr.color = disconnectColor;
                    break;
                case CraftingStatus.BUILD:
                    Sr.color = buildColor;
                    break;
                case CraftingStatus.INVALID:
                    Sr.color = invalidColor;
                    break;
                case CraftingStatus.NONE:
                    Sr.color = Color.clear;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }   
        }


        [Inject] void Install(ToolAimHandler toolAimHandler, ToolContext context)
        {
            this.toolAimHandler = toolAimHandler;
            this.context = context;
            context.Mode.StartWith(CraftingStatus.NONE).Subscribe(SetMode).AddTo(this);
        }

        private void Start()
        {
            toolAimHandler.ToolAimInfo.Select(t => t.AimPositionWs).Subscribe(t => transform.position = t).AddTo(this);
        }
    }
}