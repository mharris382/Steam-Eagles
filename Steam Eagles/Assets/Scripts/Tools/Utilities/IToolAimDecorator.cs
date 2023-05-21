using UnityEngine;

namespace Tools.BuildTool
{
    public interface IToolAimDecorator
    {
        Vector3 GetAimPosition(Vector3 aimPosition);
    }
}