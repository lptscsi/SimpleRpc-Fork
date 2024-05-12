using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public record MethodModelCache
    {
        private Lazy<MethodModel> _methodModel;
        private static Type UnWrapReturnType(Type returnType)
        {
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                if (returnType.IsGenericType)
                {
                    return returnType.GetGenericArguments()[0];
                }
                else
                {
                    return typeof(void);
                }
            }
            else
            {
                return returnType;
            }

        }

        public MethodModelCache(Type declaringType, string methodName, Type[] parameterTypes, Type returnType, Type[] genericArguments)
        {
            DeclaringType = declaringType;

            MethodName = methodName;
            ParameterTypes = parameterTypes.Select(p => p).ToArray();
            ReturnType = UnWrapReturnType(returnType);
            IsAsync = typeof(Task).IsAssignableFrom(returnType);
            GenericArguments = genericArguments.Select(p => p).ToArray();

            _methodModel = new Lazy<MethodModel>(() => { 
               return new MethodModel(DeclaringType, MethodName, ParameterTypes, returnType, genericArguments);
            }, true);
        }

        public MethodModelCache(MethodInfo method) :
            this(
                method.DeclaringType,
                method.Name,
                method.GetParameters().Select(t => t.ParameterType).ToArray(),
                method.ReturnType,
                method.GetGenericArguments())
        {
        }

        public Type DeclaringType { get; }

        public string MethodName { get; }

        public Type[] ParameterTypes { get; }

        public Type[] GenericArguments { get; }

        public Type ReturnType { get; }

        public bool IsAsync { get; }

        public MethodModel Model => _methodModel.Value;
    }

}
