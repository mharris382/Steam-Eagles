using System.Linq;
using System.Text;
using Players.PCController;
using Players.PCController.ParallaxSystems;
using UniRx;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Zenject;


public class PCParallaxSystem : PCSystem, IInitializable, ITickable, ILateTickable
{
    public class Factory : PlaceholderFactory<PC, PCParallaxSystem>, ISystemFactory<PCParallaxSystem> { }

    private readonly ParallaxSprites _parallaxSprites;

    private Transform[] _parallaxCopies;

    private Transform[] _nonParallaxOriginals;

    private TransformAccessArray _copyAccessArray;

    private TransformAccessArray _originalAccessArray;

    private NativeArray<Vector3> _originalPositions;

    private Camera _pcCamera;

    private CompositeDisposable _cd;

    private int _playerNumber;

    private int _currentCount;

    private JobHandle _copyOriginalJobHandle;

    private JobHandle _parallaxCalculateJobHandle;
    private NativeArray<Vector3> _results;
    private NativeArray<float> _distancesToSubject;

    public PCParallaxSystem(PC pc, ParallaxSprites parallaxSprites) : base(pc)
    {
        _playerNumber = pc.PlayerNumber;
        _parallaxSprites = parallaxSprites;
        _pcCamera = pc.PCInstance.camera.GetComponent<Camera>();
        Debug.Log("Created PCParallaxSystem");
        UpdateNumberOfSprites(_parallaxSprites.Count);
    }

    public void Tick()
    {
        if (_currentCount <= 0)
        {
            Debug.Log("No Parallax Sprites found");
            return;
        }

        var position = _pc.PCInstance.camera.transform.position;
        position.z = _pc.PCInstance.character.transform.position.z;
        var copyOriginalPositionsJob = new UpdateOriginalJob() {
            originalPositions = _originalPositions,
            distancesToSubject = _distancesToSubject,
            subjectPosition = position
        };
        _copyOriginalJobHandle = copyOriginalPositionsJob.Schedule(_originalAccessArray);
        
        float farPlane = _pcCamera.farClipPlane;
        float nearPlane = _pcCamera.nearClipPlane;
        
        var parallaxJob = new ParallaxJob() {
            originalPositions = _originalPositions,
            distancesToSubject = _distancesToSubject,
            subjectPosition = position,
            nearPlane = nearPlane,
            farPlane = farPlane,
            results = _results
        };
        _parallaxCalculateJobHandle = parallaxJob.Schedule(_copyAccessArray, _copyOriginalJobHandle);
    }

    public void LateTick()
    {
        _copyOriginalJobHandle.Complete();
        _parallaxCalculateJobHandle.Complete();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _currentCount; i++)
        {
            sb.Append(", ");
            sb.Append('(');
            sb.Append(_results[i]);
            sb.Append(')');
        }
        Debug.Log(("Parallax results: " + sb.ToString()).TrimStart(','));
    }

    public void Initialize()
    {
        Debug.Log($"Initialized PCParallaxSystem for player {_playerNumber}");
        _cd = new CompositeDisposable();
        _pcCamera = this.Pc.PCInstance.camera.GetComponent<Camera>();
        _parallaxSprites.OnSpriteChanged.Subscribe(UpdateNumberOfSprites).AddTo(_cd);
        Debug.Assert(_pcCamera != null);
    }

    private void UpdateNumberOfSprites(int numberOfTransforms)
    {
        Debug.Log($"Number of parallax sprites changed to {numberOfTransforms} for player {_playerNumber}", _pc.PCInstance.character);
        
        
        _nonParallaxOriginals = _parallaxSprites.GetOriginals().ToArray();
        _parallaxCopies  = _parallaxSprites.GetCopies(_playerNumber).ToArray();

        Debug.Assert (_parallaxCopies.Length == _nonParallaxOriginals.Length) ;
        numberOfTransforms = _parallaxCopies.Length;
        _currentCount = numberOfTransforms;
        
        if(_originalPositions.IsCreated)
            _originalPositions.Dispose();
        if(_copyAccessArray.isCreated)
            _copyAccessArray.Dispose();
        if(_originalAccessArray.isCreated)
            _originalAccessArray.Dispose();
        if(_distancesToSubject.IsCreated)
            _distancesToSubject.Dispose();
        _results = new NativeArray<Vector3>(numberOfTransforms, Allocator.Persistent);
        _originalPositions = new NativeArray<Vector3>(numberOfTransforms, Allocator.Persistent);
        _copyAccessArray = new TransformAccessArray(_parallaxCopies);
        _originalAccessArray = new TransformAccessArray(_nonParallaxOriginals);
        _distancesToSubject = new NativeArray<float>(numberOfTransforms, Allocator.Persistent);
    }
}


public struct UpdateOriginalJob : IJobParallelForTransform
{
    public NativeArray<Vector3> originalPositions;
    public NativeArray<float> distancesToSubject;
    
    public Vector3 subjectPosition;

    public void Execute(int index, TransformAccess transform)
    {
        originalPositions[index] = transform.position;
        distancesToSubject[index] = transform.position.z - subjectPosition.z;
    }
}

public struct ParallaxJob : IJobParallelForTransform
{
    [ReadOnly]
    public NativeArray<Vector3> originalPositions;

    [ReadOnly]
    public NativeArray<float> distancesToSubject;
    public NativeArray<Vector3> results;
    public Vector3 subjectPosition;
    public float nearPlane;
    public float farPlane;
    public void Execute(int index, TransformAccess transform)
    {
        var objReferencePosition =originalPositions[index];
        var distanceFromSubject = distancesToSubject[index];
        var travelMax = (Vector2)(subjectPosition - objReferencePosition);
        
        var isObjectInBg = distanceFromSubject > 0;
        var clipPlane = subjectPosition.z + (isObjectInBg ? farPlane : nearPlane);
        float parallaxFactor = math.abs(distanceFromSubject) / clipPlane;
        
        var newPosition = objReferencePosition + (Vector3)(travelMax * parallaxFactor);
        newPosition.z = objReferencePosition.z;
        transform.position = newPosition;
        
        results[index] = new Vector3(0, 0, parallaxFactor);
    }
}