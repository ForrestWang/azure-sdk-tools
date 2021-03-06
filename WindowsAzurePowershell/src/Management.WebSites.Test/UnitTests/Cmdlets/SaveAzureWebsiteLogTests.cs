﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Cmdlets
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Management.Services;
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Model;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services.WebEntities;

    [TestClass]
    public class SaveAzureWebsiteLogTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void SaveAzureWebsiteLogTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();

            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1", SiteProperties = new SiteProperties
                        {
                            Properties = new List<NameValuePair>
                            {
                                new NameValuePair { Name = "repositoryuri", Value = "http" },
                                new NameValuePair { Name = "PublishingUsername", Value = "user1" },
                                new NameValuePair { Name = "PublishingPassword", Value = "password1" }
                            }
                        }} 
                    });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
            };

            SimpleDeploymentServiceManagement deploymentChannel = new SimpleDeploymentServiceManagement
            {
                DownloadLogsThunk = ar => new MemoryStream(Encoding.UTF8.GetBytes("test"))
            };

            // Test
            SaveAzureWebsiteLogCommand getAzureWebsiteLogCommand = new SaveAzureWebsiteLogCommand(channel, deploymentChannel)
            {
                Name = "website1", 
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = "SaveAzureWebsiteLogTests_SaveAzureWebsiteLogTest" }
            };
            
            getAzureWebsiteLogCommand.DefaultCurrentPath = "";
            getAzureWebsiteLogCommand.ExecuteCommand();
            Assert.AreEqual("test", File.ReadAllText(SaveAzureWebsiteLogCommand.DefaultOutput));
        }
    }
}
