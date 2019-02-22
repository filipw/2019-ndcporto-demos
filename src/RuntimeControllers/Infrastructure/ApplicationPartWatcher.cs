using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RuntimeControllers
{
    public class ApplicationPartWatcher : IHostedService
    {
        private ConcurrentDictionary<string, (AssemblyLoadContext context, List<Assembly> assemblies)> _loadedAssemblies = new ConcurrentDictionary<string, (AssemblyLoadContext, List<Assembly>)>();
        private readonly OnDemandActionDescriptorChangeProvider _onDemandActionDescriptorChangeProvider;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly ILogger<ApplicationPartWatcher> _logger;
        private FileSystemWatcher _watcher;

        public ApplicationPartWatcher(IActionDescriptorChangeProvider onDemandActionDescriptorChangeProvider, 
                ApplicationPartManager applicationPartManager, ILoggerFactory loggerFactory)
        {
            _onDemandActionDescriptorChangeProvider = (OnDemandActionDescriptorChangeProvider)onDemandActionDescriptorChangeProvider;
            _applicationPartManager = applicationPartManager;
            _logger = loggerFactory.CreateLogger<ApplicationPartWatcher>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var configFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");

            if (!Directory.Exists(configFolderPath))
                Directory.CreateDirectory(configFolderPath);

            _watcher = new FileSystemWatcher()
            {
                Path = configFolderPath,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName
            };

            _watcher.Changed += (s, e) => _logger.LogInformation("Changed: " + e.FullPath);
            _watcher.Renamed += (s, e) => _logger.LogInformation("Renamed: " + e.OldFullPath + " to " + e.FullPath);
            _watcher.Created += (s, e) =>
            {
                _logger.LogInformation("Created: " + e.FullPath);
                //hack to let the file complete the creation...
                Thread.Sleep(1000);

                var loadContext = new PluginAssemblyLoadContext();
                var loadedFolderAssemblies = new List<Assembly>();
                foreach (var file in Directory.EnumerateFiles(e.FullPath, "*.dll"))
                {
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var assembly = loadContext.LoadFromStream(fs);
                        loadedFolderAssemblies.Add(assembly);

                        var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                        foreach (var part in partFactory.GetApplicationParts(assembly))
                        {
                            _applicationPartManager.ApplicationParts.Add(part);
                        }

                    }
                }

                if (loadedFolderAssemblies.Any())
                {
                    _loadedAssemblies[e.FullPath] = (loadContext, loadedFolderAssemblies);
                    _onDemandActionDescriptorChangeProvider.TokenSource.Cancel();
                }
            };
            _watcher.Deleted += (s, e) =>
            {
                _logger.LogInformation("Deleted: " + e.FullPath);

                if (_loadedAssemblies.TryGetValue(e.FullPath, out var loadedData))
                {
                    foreach (var assembly in loadedData.assemblies)
                    {
                        var existingAssemblyPart = _applicationPartManager.ApplicationParts.FirstOrDefault(x => x.Name == assembly.GetName().Name);
                        if (existingAssemblyPart != null)
                        {
                            _applicationPartManager.ApplicationParts.Remove(existingAssemblyPart);
                        }
                    }

                    _onDemandActionDescriptorChangeProvider.TokenSource.Cancel();
#if NETCOREAPP3_0
                    loadedData.context.Unload();
#endif
                }

            };
            _watcher.EnableRaisingEvents = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher.Dispose();
            return Task.CompletedTask;
        }
    }

    public class PluginAssemblyLoadContext : AssemblyLoadContext
    {
#if NETCOREAPP3_0
        public PluginAssemblyLoadContext() : base(isCollectible: true)
        { }
#endif

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
