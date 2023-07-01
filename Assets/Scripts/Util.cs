using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Util
{
    public static bool HasFlag(this Enum flags, Enum flag)
    {
        ulong keysVal = Convert.ToUInt64(flags);
        ulong flagVal = Convert.ToUInt64(flag);

        return (keysVal & flagVal) == flagVal;
    }
}
