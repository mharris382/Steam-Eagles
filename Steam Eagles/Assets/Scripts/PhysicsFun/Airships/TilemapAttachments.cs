using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Airships
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapAttachments : MonoBehaviour
    {
        private Tilemap _tm;
        public Tilemap tm => _tm ? _tm : _tm = GetComponent<Tilemap>();


        private Dictionary<Vector3Int, ITilemapAttachment> _attachments =
            new Dictionary<Vector3Int, ITilemapAttachment>();

        private void Awake()
        {
            MessageBroker.Default.Receive<DisconnectActionInfo>()
                .Where(t => t.tilemap == tm && _attachments.ContainsKey(t.cellPosition))
                .Select(t => t.cellPosition)
                .Subscribe(DisconnectAttachmentAt)
                .AddTo(this);

        }

        public void DisconnectAttachmentAt(Vector3Int cell)
        {
            if (_attachments.ContainsKey(cell))
            {
                return;
            }
            _attachments[cell].Disconnect();
            _attachments.Remove(cell);
        }

        public bool CanAttach(Vector3Int pos)
        {
            return tm.GetTile(pos) != null;
        }

        public ITilemapAttachment GetAttachmentAt(Vector3Int pos)
        {
            if (_attachments.ContainsKey(pos))
            {
                return _attachments[pos];
            }
            return null;
        }
        public T GetAttachmentAt<T>(Vector3Int pos) where T : class, ITilemapAttachment
        {
            if (_attachments.ContainsKey(pos))
            {
                return _attachments[pos] as T;
            }
            return null;
        }
        public bool TryAttach(ITilemapAttachment attachment, Vector3Int cell)
        {
            if (attachment == null)
                return false;
            if (!CanAttach(cell))
            {
                return false;
            }
            if(_attachments.ContainsKey(cell))
            {
                if (_attachments[cell] == attachment)
                {
                    return true;
                }
                if (_attachments[cell] != null)
                {
                    return false;
                }
                _attachments.Remove(cell);
            }
            _attachments.Add(cell, attachment);
            attachment.AttachToTilemap(tm, cell);
            
            return true;
        }
        
        
    }
}