using UnityEngine;

public class TagTriggerArea : TriggerAreaBase<GameObject>
{
    public string[] matchTags = new string[1]{ "Player" };


    protected override bool HasTarget(Rigidbody2D rbTarget, out GameObject value)
    {
        if (HasAnyMatchingTag(rbTarget.gameObject))
        {                                        
            value = rbTarget.gameObject;           
            return true;                         
        }                                        
        value = null;
        return false;
    }

    protected override bool HasTarget(Collider2D target, out GameObject value)
    {
        if (HasAnyMatchingTag(target.gameObject))
        {
            value = target.gameObject;
            return true;
        }

        value = null;
        return false;
    }


    bool HasAnyMatchingTag(GameObject go)
    {
        foreach (var matchTag in matchTags)
        {
            if (go.CompareTag(matchTag))
            {
                return true;
            }
        }

        return false;
    }
}