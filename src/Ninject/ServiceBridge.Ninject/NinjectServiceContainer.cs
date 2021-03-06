﻿using System;
using System.Collections.Generic;
using Ninject;
using Ninject.Infrastructure;
using Ninject.Planning.Strategies;
using Ninject.Syntax;

namespace ServiceBridge.Ninject
{
    /// <summary>
    /// An implementation of <see cref="IServiceContainer"/> that wraps Ninject.
    /// </summary>
    public class NinjectServiceContainer : ServiceContainerBase
    {
        private readonly bool _externalKernel;
        private IKernel _kernel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NinjectServiceContainer" /> class for a container.
        /// </summary>
        /// <param name="kernel">
        ///     The <see cref="IKernel" /> to wrap with the <see cref="IServiceContainer" />
        ///     interface implementation.
        /// </param>
        public NinjectServiceContainer(IKernel kernel = null)
        {
            if (kernel == null)
            {
                var settings = new NinjectSettings {InjectAttribute = typeof(InjectionAttribute)};
                settings.ExtensionSearchPatterns =
                    new List<string>(settings.ExtensionSearchPatterns ?? new string[0])
                    {
                        "ServiceBridge.Ninject.*.dll"
                    }.ToArray();
                _kernel = new StandardKernel(settings);
            }
            else
            {
                _externalKernel = true;
            }
            _kernel.Components.RemoveAll<IPlanningStrategy>();
            _kernel.Components.Add<IPlanningStrategy, ConstructorStrategy>();
            _kernel.Components.Add<IPlanningStrategy, PropertyStrategy>();
            _kernel.Components.Add<IPlanningStrategy, MethodStrategy>();
            RegisterInstance(typeof(IServiceContainer), this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && !_externalKernel && _kernel != null)
            {
                _kernel.Dispose();
                _kernel = null;
            }
        }

        /// <summary>
        /// Resolves the requested service instance.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <param name="serviceName">Name of registered service you want. May be null.</param>
        /// <returns>
        ///     The requested service instance.
        /// </returns>
        protected override object DoGetInstance(Type serviceType, string serviceName)
        {
            if (_kernel == null)
            {
                throw new ObjectDisposedException("container");
            }
            return _kernel.Get(serviceType, serviceName);
        }

        /// <summary>
        /// Resolves all the requested service instances.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <returns>
        ///     Sequence of service instance objects.
        /// </returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (_kernel == null)
            {
                throw new ObjectDisposedException("container");
            }
            return _kernel.GetAll(serviceType);
        }

        /// <summary>
        /// Run an existing object through the container and perform injection on it.
        /// </summary>
        /// <param name="instance">The existing instance to be injected.</param>
        protected override void DoInjectInstance(object instance)
        {
            if (_kernel == null)
            {
                throw new ObjectDisposedException("container");
            }
            _kernel.Inject(instance);
        }

        /// <summary>
        /// Registers the type mapping.
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> that will be requested.</param>
        /// <param name="implementationType"><see cref="Type"/> that will actually be returned.</param>
        /// <param name="serviceName">Name to use for registration, null if a default registration.</param>
        /// <param name="lifetime">The lifetime strategy of the resolved instances.</param>
        protected override void DoRegister(Type serviceType, Type implementationType, string serviceName, ServiceLifetime lifetime)
        {
            if (_kernel == null) throw new ObjectDisposedException("container");
            var binding = _kernel.Bind(serviceType).To(implementationType);
            IBindingSyntax syntax = binding;
            if (serviceName != null) syntax = binding.Named(serviceName);
            OnRegistering(new NinjectServiceRegisterEventArgs(serviceType, implementationType, serviceName, lifetime, syntax));
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    binding.InScope(StandardScopeCallbacks.Transient);
                    break;
                case ServiceLifetime.Singleton:
                    binding.InScope(StandardScopeCallbacks.Singleton);
                    break;
                case ServiceLifetime.PerThread:
                    binding.InScope(ctx => System.Threading.Thread.CurrentThread);
                    break;
                case ServiceLifetime.PerRequest:
#if NetCore
                    binding.InScope(ctx => ctx.Kernel.Get<Microsoft.AspNetCore.Http.IHttpContextAccessor>()?.HttpContext);
#else
                    binding.InScope(ctx => System.Web.HttpContext.Current);
#endif
                    break;
            }
        }

        /// <summary>
        /// Registering the instance mapping.
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> that will be requested.</param>
        /// <param name="instance">The instance that will actually be returned.</param>
        /// <param name="serviceName">Name to use for registration, null if a default registration.</param>
        protected override void DoRegisterInstance(Type serviceType, object instance, string serviceName)
        {
            if (_kernel == null) throw new ObjectDisposedException("container");
            var binding = _kernel.Bind(serviceType).ToConstant(instance);
            if (serviceName != null) binding.Named(serviceName);
        }
    }
}
