using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFirearm
{
    void Shoot(Vector2 target);

    float GetRange();

    bool ReadyToFire();
}
