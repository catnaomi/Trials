using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CustomUtilities
{
    public static class DamageUtilities
    {

        public static bool HasType(this DamageType type, DamageType check)
        {
            return (type & check) != 0;
        }

        public static DamageType[] ToArray(this DamageType type)
        {
            return Enum.GetValues(typeof(DamageType)).Cast<DamageType>().Where(d => (d != 0 && ((d & type) != 0))).ToArray();
        }
    }
}