using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <see cref="https://github.com/Habrador/Unity-Rope-Tutorial/blob/master/Assets/Scripts/Rope%20realistic/RopeSection.cs"/>
/// </summary>
//A struct that will hold information about each rope section
public struct RopeSection
{
    public Vector3 pos;
    public Vector3 vel;

    //To write RopeSection.zero
    public static readonly RopeSection zero = new RopeSection(Vector3.zero);

    public RopeSection(Vector3 pos)
    {
        this.pos = pos;

        this.vel = Vector3.zero;
    }
}