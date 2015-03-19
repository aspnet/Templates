// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.ConfigurationModel;
using Microsoft.AspNet.Mvc;

namespace Microsoft.Web.Templates.Tests
{
    public class TestConfigurationProvider : ITestConfigurationProvider
    {
        public TestConfigurationProvider()
        {
            Configuration = new Configuration();
            Configuration.Add(new MemoryConfigurationSource());
        }

        public Configuration Configuration { get; set; }
    }
}