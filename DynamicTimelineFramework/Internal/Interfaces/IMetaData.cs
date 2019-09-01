using System.Collections.Generic;
using System.Reflection;
using DynamicTimelineFramework.Objects;

namespace DynamicTimelineFramework.Internal.Interfaces
{
    internal interface IMetaData<out T> where T : DTFObject
    {
        ObjectNode Graph { get; }
        Map<string, FieldInfo> LateralFields { get; }
                
        List<string> LateralKeys { get; }
    }
}