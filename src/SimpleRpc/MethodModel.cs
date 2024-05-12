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

        private static string GetReturnType(Type returnType)
        {
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                if (returnType.IsGenericType)
                {
                    return GetTypeName(returnType.GetGenericArguments()[0]);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                if (returnType == typeof(void))
                {
                    return String.Empty;
                }
                else
                {
                    return GetTypeName(returnType);
                }
            }

        }

        public MethodModel(Type declaringType, string methodName, Type[] parameterTypes, Type returnType, Type[] genericArguments)
        {
            DeclaringType = GetTypeName(declaringType);

            MethodName = methodName;
            ParameterTypes = parameterTypes.Select(p => GetTypeName(p)).ToArray();
            ReturnType = GetReturnType(returnType);

            GenericArguments = genericArguments.Select(p => GetTypeName(p)).ToArray();
        }

        public MethodModel(MethodInfo method) :
            this(
                method.DeclaringType,
                method.Name,
                method.GetParameters().Select(t => t.ParameterType).ToArray(),
                method.ReturnType,
                method.GetGenericArguments())
        {
        }

        public string DeclaringType { get; init; }

        public string MethodName { get; init; }

        public string[] ParameterTypes { get; init; }

        public string[] GenericArguments { get; init; }

        public string ReturnType { get; init; }
    }

}
