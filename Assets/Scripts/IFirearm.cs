using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFirearm
{
    void Shoot();

    float GetRange();

    float GetFireRate();

    bool ReadyToFire();

    Transform GetBarrelExit();
}
