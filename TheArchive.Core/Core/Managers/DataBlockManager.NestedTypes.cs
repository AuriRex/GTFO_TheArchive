using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TheArchive.Core.Managers;

public static partial class DataBlockManager
{
    private interface ITransformationData
    {
        public int Priority { get; }
        public Type DBType { get; }
        internal void Invoke(IList list);

        public MethodBase OriginMethod { get; }
        public Type DeclaringType { get; }
        public Assembly DeclaringAssembly { get; }
    }

    private class TransformationData<T> : ITransformationData
    {
        public int Priority { get; private set; } = 0;
        public Type DBType => typeof(T);

        public MethodBase OriginMethod { get; private set; }
        public Type DeclaringType => OriginMethod.DeclaringType;
        public Assembly DeclaringAssembly => DeclaringType.Assembly;

        private readonly MethodInfo _method;
        private readonly object _target;

        public TransformationData(Action<List<T>> transform, int priority = 0, MethodBase originMethod = null)
        {
            _method = transform.Method;
            _target = transform.Target;
            OriginMethod = originMethod;
            Priority = priority;
        }

        public void Invoke(IList list)
        {
            _method.Invoke(_target, new object[] { list });
        }
    }
}