using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParllaxObject : MonoBehaviour
{
    public Material material;

    [Range(-0.1f, 0.1f)] public float parallaxFactor = 1f;
    public float subjectZPosition = 0f;
    
    public Camera cam;
    public Transform[] subjects;
    
    private ParallaxSprite[] _parallaxSprites;
    private ParallaxSprite _parallaxSprite;
    private ParallaxSprite parallaxSprite => _parallaxSprite ??= new ParallaxSprite(transform, this); 
    

    private void OnEnable()
    {
        parallaxSprite.Enable();
    }

    private void Update()
    {
        parallaxSprite.Update();  
    }

    private void OnDisable()
    {
        parallaxSprite.Disable();
    }

    public class ParallaxSprite
    {
        private readonly Transform _transform;
        private readonly ParllaxObject _parallaxObject;
        private Vector2 _startPosition;
        private float _startZ;

        private Camera Cam => _parallaxObject.cam;
        
        private bool ShouldUseFarPlane => distanceFromSubject > 0;

        private float clippingPlane =>
            (Cam.transform.position.z + (ShouldUseFarPlane ? Cam.farClipPlane : Cam.nearClipPlane));
        
        private Vector2 travel => (Vector2)Cam.transform.position - _startPosition;
        private float distanceFromSubject => _transform.position.z - _parallaxObject.subjectZPosition;
        
        float parallaxFactor => Mathf.Abs(distanceFromSubject) / clippingPlane;
        
        public ParallaxSprite(Transform transform, ParllaxObject parallaxObject)
        {
            this._transform = transform;
            this._parallaxObject = parallaxObject;
            _startPosition = transform.position;
            _startZ = transform.position.z;
        }

        public void Enable()
        {
            _startPosition = _transform.position;
            _startZ = _transform.position.z;
        }

        public void Update()
        {
            Vector3 newPosition = _startPosition + travel * parallaxFactor;
            newPosition.z = _startZ;
            _transform.position = newPosition;
        }

        public void Disable()
        {
            _transform.position = new Vector3(_startPosition.x, _startPosition.y, _startZ);
        }
    }
}
