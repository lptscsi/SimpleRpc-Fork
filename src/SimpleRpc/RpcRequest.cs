using Fasterflect;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleRpc
{
    public class RpcRequest
    {       
        public MethodModel Method { get; set; }

        public object[] Parameters { get; set; }

        public async Task<object> Invoke(IServiceProvider serviceProvider)
        {
            Type declaringType = Type.GetType(Method.DeclaringType);
            var resolvedType = serviceProvider.GetRequiredService(declaringType);

            Type[] genericArgs = Method.GenericArguments.Select(p => Type.GetType(p)).ToArray();
            Type[] paramTypes = Method.ParameterTypes.Select(p => Type.GetType(p)).ToArray();

            if (paramTypes.Length != Parameters.Length)
            {
                throw new InvalidOperationException("ParamTypes.Length != Parameters.Length");
            }

            object[] parameters = new object[Parameters.Length];

            for (int i = 0; i < Parameters.Length; i++)
            {
                object p = Parameters[i];
                Type type = paramTypes[i];
                if (p != null && p is JsonElement element)
                {
                   parameters[i] = element.Deserialize(type);
                }
            }

            try
            {
                var result = resolvedType.CallMethod(
                    genericArgs,
                    Method.MethodName,
                    paramTypes,
                    parameters);

                if (result is Task task)
                {
                    await task;

                    if (!string.IsNullOrEmpty(Method.ReturnType)) 
                    {
                        Type returnType = Type.GetType(Method.ReturnType);
                        object res = task.GetPropertyValue(nameof(Task<object>.Result));
                        return res;
                    }

                    return null;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
