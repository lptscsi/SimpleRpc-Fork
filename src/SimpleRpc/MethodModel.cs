using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public record MethodModel
    {
        public MethodModel() { }

        private static string GetTypeName(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name }";
        }

        public MethodModel(Type declaringType, string methodName, Type[] parameterTypes, Type returnType, Type[] genericArguments)
        {
            DeclaringType = GetTypeName(declaringType);

            MethodName = methodName;
            ParameterTypes = parameterTypes.Select(p => GetTypeName(p)).ToArray();

            if (typeof(Task).IsAssignableFrom(returnType))
            {
                IsAsync = true;
                if (returnType.IsGenericType)
                {
                    ReturnType = GetTypeName(returnType.GetGenericArguments()[0]);
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
                    ReturnType = GetTypeName(returnType);
                }
            }

            GenericArguments = genericArguments.Select(p => GetTypeName(p)).ToArray();
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
