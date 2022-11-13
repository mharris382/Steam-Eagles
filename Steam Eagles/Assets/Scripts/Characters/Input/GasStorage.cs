using System;
using UniRx;
using UnityEngine;

namespace Characters
{
    public class GasStorage : MonoBehaviour
    {
        [SerializeField] private int _gasAmount;


        public Vector2 ReleasePoint => transform.position;

        private void Awake()
        {
            
        }

        private static int messageCount;
        public void ReleaseGas(int amount)
        {
            amount = Mathf.Min(_gasAmount, amount);
            var releaseGasMsg = new ReleaseGasMessage(messageCount++, amount, ReleasePoint);
            
            MessageBroker.Default.Receive<ReleaseGasMessage>().Take(1)
                .Delay(TimeSpan.FromMilliseconds(50))
                .Subscribe(releaseMsg => _gasAmount += releaseMsg.Consume());
            
            MessageBroker.Default.Publish(releaseGasMsg);
        }
    }
    
    public class ReleaseGasMessage
    {
        private int _amount;

        public ReleaseGasMessage(int messageID, int amt, Vector3 position)
        {
            _amount = amt;
            MessageID = messageID;
            Position = position;
        }

        public int Consume()
        {
            _amount = 0;
            return _amount;
        }
        public int MessageID { get; }
        public Vector3 Position { get; }

        public int Consume(int amount)
        {
            var consumeValue = Mathf.Min(amount, _amount);
            _amount -= consumeValue;
            return consumeValue;
        }
    }
}