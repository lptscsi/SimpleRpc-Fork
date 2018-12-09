using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Abstractions.Server;
using SimpleRpc.Transports.Http.Client;
using System;
using System.Net.Http;

namespace SimpleRpc.Transports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleRpcClient(
            this IServiceCollection services,
            string ClientName,
            HttpClientTransportOptions options)
        {
            if (string.IsNullOrEmpty(ClientName))
            {
                throw new ArgumentNullException(nameof(ClientName));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            services.AddHttpClient(ClientName, (client) => {
                var url = new Uri(options.Url);
                client.BaseAddress = url;

                if (options.DefaultRequestHeaders != null)
                {
                    foreach (var header in options.DefaultRequestHeaders)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                client.DefaultRequestHeaders.Add(Constants.Other.ApplicationName, options.ApplicationName);
                client.DefaultRequestHeaders.ConnectionClose = false;
                client.DefaultRequestHeaders.Host = url.Host;
            });

            services.TryAddSingleton<IClientConfigurationManager, ClientConfigurationManager>();
            services.AddSingleton(typeof(ClientConfiguration), (sp)=>
            {
                var serializer = SerializationHelper.GetByName(options.Serializer);
                return new ClientConfiguration()
                {
                    Name = ClientName,
                    Transport = new HttpClientTransport(ClientName, serializer, sp.GetRequiredService<IHttpClientFactory>())
                };
            });

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy<T>(this IServiceCollection services, string clientName)
        {
            AddSimpleRpcProxy(services, typeof(T), clientName);

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy(this IServiceCollection services, Type interfaceToProxy, string clientName)
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (interfaceToProxy == null)
            {
                throw new ArgumentNullException(nameof(interfaceToProxy));
            }

            if (!interfaceToProxy.IsInterface)
            {
                throw new NotSupportedException("You can use AddSimpleRpcProxy only on interfaces");
            }

            services.TryAddSingleton(interfaceToProxy, sp => sp.GetService<IClientConfigurationManager>().Get(clientName).BuildProxy(interfaceToProxy));

            return services;
        }

        public static IServiceCollection AddSimpleRpcServer<T>(
            this IServiceCollection services,
            IServerTransportOptions<T> serverTransportOptions) where T : class, IServerTransport, new()
        {
            if (serverTransportOptions == null)
            {
                throw new ArgumentNullException(nameof(serverTransportOptions));
            }

            var serverTransport = new T();

            services.AddSingleton<IServerTransport>(serverTransport);
            serverTransport.ConfigureServices(services, serverTransportOptions);

            return services;
        }
    }
}
