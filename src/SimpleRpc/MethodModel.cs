using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public record MethodModel
    {
        public MethodModel() { }

        public MethodModel(Type declaringType, string methodName, Type[] parameterTypes, Type returnType, Type[] genericArguments)
        {
            DeclaringType = declaringType.AssemblyQualifiedName;
            MethodName = methodName;
            ParameterTypes = parameterTypes.Select(p => p.AssemblyQualifiedName).ToArray();

            if (typeof(Task).IsAssignableFrom(returnType))
            {
                IsAsync = true;
                if (returnType.IsGenericType)
                {
                    ReturnType = returnType.GetGenericArguments()[0].AssemblyQualifiedName;
                }
                else
                {
                    ReturnType = string.Empty;
                }
            }
            else
            {
                if (returnType == typeof(void))
                {
                    ReturnType = String.Empty;
                }
                else
                {
                    ReturnType = returnType.AssemblyQualifiedName;
                }
            }

            GenericArguments = genericArguments.Select(p => p.AssemblyQualifiedName).ToArray();
        }

        public MethodModel(MethodInfo method, Type[] genericArguments) :
            this(
                method.DeclaringType,
                method.Name,
                method.GetParameters().Select(t => t.ParameterType).ToArray(),
                method.ReturnType,
                genericArguments)
        {
        }

        public string DeclaringType { get; init; }

        public string MethodName { get; init; }

        public string[] ParameterTypes { get; init; }

        public string[] GenericArguments { get; init; }

        public string ReturnType { get; init; }

        public bool IsAsync { get; init; }
    }

}
