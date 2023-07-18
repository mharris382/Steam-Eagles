using System;
using GasSim.SimCore.DataStructures;
using UniRx;

namespace CoreLib.MyEntities.Utilities
{
    public class MainThreadStructuralChangesBuffer 
    {
        public const int MAX_BUFFER_SIZE = 100;
        private PriorityQueue<EntityStructureChangeRequest> _buffer;
        private Subject<EntityStructureChangeRequest> _requestStream = new();
        public MainThreadStructuralChangesBuffer()
        {
            _buffer = new PriorityQueue<EntityStructureChangeRequest>(MAX_BUFFER_SIZE);
        }
        
    }


    struct EntityStructureChangeRequest : IDisposable
    {
        public readonly string entityGUID;
        private readonly Subject<bool> _callback;

        public EntityStructureChangeRequest(string entityGUID, EntityRequestType request, Action<bool> onCompleted = null)
        {
            this.entityGUID = entityGUID;
            _callback = new();
            Request = request;
            Result = EntityRequestState.WAITING;
        }

        public EntityRequestState Result { get; private set; }
        public EntityRequestType Request { get; }
        
        public void Complete(bool success)
        {
            if (Result != EntityRequestState.WAITING) return;
            Result = success ? EntityRequestState.SUCCESS : EntityRequestState.FAILED;
            _callback.OnNext(success);
            _callback.OnCompleted();
        }
        
        public void Dispose()
        {
            if (Result != EntityRequestState.WAITING) return;
            this.Result = EntityRequestState.CANCELLED;
            _callback.OnNext(false);
            _callback.OnCompleted();
        }
    }

    public enum EntityRequestType
    {
        CREATE,
        DESTROY
    }

    public enum EntityRequestState
    {
        WAITING, 
        CANCELLED,
        SUCCESS,
        FAILED
    }
}