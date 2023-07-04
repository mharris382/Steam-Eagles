using System.Collections;
using System.Collections.Generic;
using Dest.Math;
using UnityEngine;

public class FireStarter : MonoBehaviour
{
    public FireSpreader fireSpreader;
    public Transform startPoint;
    public SpriteRenderer testTarget;
    public KeyCode testKey = KeyCode.B;
    public bool directSet = true;
    public Vector2 min = new Vector2(-1, -1);
    public Vector2 max = new Vector2(1, 1);
    
    void Update(){
        if(Input.GetKeyDown(testKey))
        {
            var pos = startPoint.localPosition;
            
            if(!directSet)
            {
                fireSpreader.StartFire(pos, testTarget);
                
            }
            else
            {
                var box = AAB2.CreateFromTwoPoints(ref min, ref max);
                var randomX = Random.Range(min.x, max.x);
                var randomY = Random.Range(min.y, max.y);
                
                fireSpreader.StartFire(new Vector3(randomX, randomY), box);
            }
        }
    }
}
