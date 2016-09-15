﻿using MassActivation;
using ServiceBridge.Autofac.Activation;
using ServiceBridge.Autofac.Interception;

namespace ServiceBridge.Samples.WebForm
{
    public class Startup
    {
        public Startup(IActivatingEnvironment environment)
        {
            environment.UseAutofac().EnableInterception();
        }
    }
}