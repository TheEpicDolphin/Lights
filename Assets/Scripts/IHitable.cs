using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitable
{
    void AddKnockback(float strength, Vector2 dir);
}
