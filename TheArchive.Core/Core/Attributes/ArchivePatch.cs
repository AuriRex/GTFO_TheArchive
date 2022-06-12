using System;

namespace TheArchive.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ArchivePatch : Attribute
    {
        public bool HasType
        {
            get
            {
                return Type != null;
            }
        }

        public Type Type { get; internal set; }

        public string MethodName { get; private set; }

        public Type[] ParameterTypes { get; internal set; }

        /// <summary>
        /// Describes what method to patch.
        /// <br/><br/>
        /// Type must be provided via a method marked with the <see cref="IsTypeProvider"/> Attribute inside of your type!
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameterTypes"></param>
        public ArchivePatch(string methodName, Type[] parameterTypes = null) : this(null, methodName, parameterTypes)
        {

        }

        /// <summary>
        /// Describes what method to patch.
        /// </summary>
        /// <param name="type">The type the method is on</param>
        /// <param name="methodName">The method name to patch</param>
        /// <param name="parameterTypes">Method parameters to distinguish between overloads</param>
        public ArchivePatch(Type type, string methodName, Type[] parameterTypes = null)
        {
            Type = type;
            MethodName = methodName;
            ParameterTypes = parameterTypes;
        }

    }
}
