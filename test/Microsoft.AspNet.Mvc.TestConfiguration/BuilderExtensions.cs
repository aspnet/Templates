// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.TestConfiguration;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Builder
{
    public static class BuilderExtensions
    {
        public static IConfiguration GetTestConfiguration(this IApplicationBuilder app)
        {
            var configurationProvider = app.ApplicationServices.GetService<ITestConfigurationProvider>();
            var configuration = configurationProvider == null
                ? new ConfigurationBuilder.BUild()
                : configurationProvider.Configuration;

            return configuration;
        }

        public static IApplicationBuilder UseErrorReporter(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ErrorReporterMiddleware>();
        }
    }
}