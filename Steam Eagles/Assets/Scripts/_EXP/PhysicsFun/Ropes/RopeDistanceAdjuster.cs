using UnityEngine;

[ExecuteAlways]
public class RopeDistanceAdjuster : MonoBehaviour
{
    [SerializeField]
    private RopeFHB _rope;

    
    [SerializeField]
    private float _tightenSpeed = 5f;

    [SerializeField]
    private float _loosenSpeed = 25f;
    
    [SerializeField] private bool autoConfigureDistance = true;
    
    [SerializeField]
    private float _targetDistance;
    private float _currentDistance;


    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float _smoothTime = 0.1f;
    private float _currentSpeed;
    private void Awake()
    {
        float ropeDistance = (_targetDistance = Vector2.Distance(_rope.StartPoint.position, _rope.EndPoint.position));
        _rope.SetRopeDistance(ropeDistance);
        _currentDistance = _targetDistance;
    }

    private void Update()
    {
        if (autoConfigureDistance)
            _targetDistance = Vector2.Distance(_rope.StartPoint.position, _rope.EndPoint.position);

        if (!Application.isPlaying)
            return;
        
        float speed =_targetDistance > _currentDistance ? _tightenSpeed : _loosenSpeed;
        
        if(useSmoothing) _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _currentSpeed, _smoothTime, speed);
        else _currentDistance = Mathf.MoveTowards(_currentDistance, _targetDistance, speed * Time.deltaTime);
        
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, Time.deltaTime * _tightenSpeed);
        _rope.SetRopeDistance(_currentDistance);
    }
}