using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Spotlight
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public override string Name => "Spotlight";
        public override Guid Id => Guid.Parse("a8f3c7e2-1d4b-4a9e-8f2c-3d7b5e6a9c1f");
        public override string Description => "Discover your media with trailers on the Jellyfin homepage using dynamic spotlight";

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            RegisterFileTransformation();
        }

        public static Plugin? Instance { get; private set; }

        private void RegisterFileTransformation()
        {
            var payload = JsonSerializer.Serialize(new
            {
                id = "a8f3c7e2-1d4b-4a9e-8f2c-3d7b5e6a9c1f",
                fileNamePattern = "home-html\\..+\\.chunk\\.js$",
                callbackAssembly = GetType().Assembly.FullName,
                callbackClass = "Jellyfin.Plugin.Spotlight.Transformer",
                callbackMethod = "Transform"
            });

            Assembly? fileTransformationAssembly = null;
            foreach (var context in AssemblyLoadContext.All)
            {
                fileTransformationAssembly = context.Assemblies.FirstOrDefault(x =>
                    x.FullName?.Contains(".FileTransformation") ?? false);
                if (fileTransformationAssembly != null)
                    break;
            }

            if (fileTransformationAssembly != null)
            {
                Type? pluginInterfaceType = fileTransformationAssembly.GetType("Jellyfin.Plugin.FileTransformation.PluginInterface");
                if (pluginInterfaceType != null)
                {
                    pluginInterfaceType.GetMethod("RegisterTransformation")?.Invoke(null, new object?[] { payload });
                }
            }
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "spotlight",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.spotlight.html"
                },
                new PluginPageInfo
                {
                    Name = "spotlightcss",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.spotlight.css"
                },
                new PluginPageInfo
                {
                    Name = "spotlightjs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}
