using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fragment.NetSlum.Server.Configuration;
using Fragment.NetSlum.Networking.Services.Enterprise;
using Fragment.NetSlum.Networking.Services.DynaLink;
using Fragment.NetSlum.Networking.Compatibility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fragment.NetSlum.Server.Services
{
    /// <summary>
    /// Hosted service that listens on the server console for "selftest" commands
    /// and runs lightweight verification tests that don't require external game clients.
    /// </summary>
    public class SelfTestService : IHostedService
    {
        private readonly IServiceProvider _services;
        private CancellationTokenSource? _cts;

        public SelfTestService(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Task.Run(() => ConsoleLoop(_cts.Token));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            return Task.CompletedTask;
        }

        private async Task ConsoleLoop(CancellationToken ct)
        {
            var log = Log.ForContext<SelfTestService>();
            log.Information("SelfTestService started. Type 'selftest list' or 'selftest run <name>'");

            while (!ct.IsCancellationRequested)
            {
                string? line = null;
                try
                {
                    line = Console.ReadLine();
                }
                catch
                {
                }

                if (line == null)
                {
                    await Task.Delay(200, ct).ContinueWith(_ => { });
                    continue;
                }

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                if (parts[0].Equals("selftest", StringComparison.OrdinalIgnoreCase))
                {
                    if (parts.Length == 1 || parts[1].Equals("list", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Available tests: game-registry, dyna-host-registry, enterprise-router, adapter-decode, all");
                        continue;
                    }

                    if (parts[1].Equals("run", StringComparison.OrdinalIgnoreCase) && parts.Length >= 3)
                    {
                        var test = parts[2].ToLowerInvariant();
                        switch (test)
                        {
                            case "game-registry":
                                RunGameRegistryTest();
                                break;
                            case "dyna-host-registry":
                                await RunDynaHostRegistryTest();
                                break;
                            case "enterprise-router":
                                await RunEnterpriseRouterTest();
                                break;
                            case "adapter-decode":
                                RunAdapterDecodeTest();
                                break;
                            case "all":
                                RunGameRegistryTest();
                                await RunDynaHostRegistryTest();
                                await RunEnterpriseRouterTest();
                                RunAdapterDecodeTest();
                                break;
                            default:
                                Console.WriteLine($"Unknown test: {test}");
                                break;
                        }
                    }
                }
            }
        }

        private void RunGameRegistryTest()
        {
            using var scope = _services.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<GameRegistry>();
            Console.WriteLine("GameRegistry: Loaded games: " + string.Join(", ", registry.AllGames.Select(g => g.Name)));

            if (registry.TryGet("Shutokou Battle Online", out var sbol))
                Console.WriteLine($"Resolved SBOL -> mode={sbol.Mode}, key={(string.IsNullOrEmpty(sbol.CommonKey) ? "(none)" : "[redacted]")}");
            else if (registry.TryGet("SBOL", out var sbolAlias))
                Console.WriteLine($"Resolved SBOL alias -> mode={sbolAlias.Mode}");
            else
                Console.WriteLine("SBOL not found in registry");

            if (registry.TryGet(".hack//Fragment", out var frag))
                Console.WriteLine($"Resolved .hack//Fragment -> mode={frag.Mode}");
            else if (registry.TryGet("Fragment", out var fragAlias))
                Console.WriteLine($"Resolved Fragment alias -> mode={fragAlias.Mode}");
            else
                Console.WriteLine("Fragment not found in registry");
        }

        private async Task RunDynaHostRegistryTest()
        {
            using var scope = _services.CreateScope();
            var hostRegistry = scope.ServiceProvider.GetRequiredService<DynaLinkHostRegistry>();

            // Create a transient FragmentTcpSession so we can register/unregister it
            var server = scope.ServiceProvider.GetRequiredService<Fragment.NetSlum.TcpServer.ITcpServer>();
            var session = ActivatorUtilities.CreateInstance<Fragment.NetSlum.Networking.Sessions.FragmentTcpSession>(scope.ServiceProvider, server, scope);

            var id = Encoding.UTF8.GetBytes("test-host-1");
            hostRegistry.RegisterHost(session, id, "test-host-1");

            var foundByName = hostRegistry.FindByName("test-host-1");
            Console.WriteLine(foundByName == session ? "DynaLink: FindByName OK" : "DynaLink: FindByName FAIL");

            var foundById = hostRegistry.FindById(id);
            Console.WriteLine(foundById == session ? "DynaLink: FindById OK" : "DynaLink: FindById FAIL");

            hostRegistry.UnregisterHost(session);
            var after = hostRegistry.FindByName("test-host-1");
            Console.WriteLine(after == null ? "DynaLink: Unregister OK" : "DynaLink: Unregister FAIL");

            // Dispose session scope
            try { session.Disconnect(); } catch { }
            await Task.CompletedTask;
        }

        private async Task RunEnterpriseRouterTest()
        {
            using var scope = _services.CreateScope();
            var router = scope.ServiceProvider.GetRequiredService<EnterpriseRouter>();
            var server = scope.ServiceProvider.GetRequiredService<Fragment.NetSlum.TcpServer.ITcpServer>();
            var session = ActivatorUtilities.CreateInstance<Fragment.NetSlum.Networking.Sessions.FragmentTcpSession>(scope.ServiceProvider, server, scope);

            // populate AreaServerInfo
            session.AreaServerInfo = new Fragment.NetSlum.Networking.Models.AreaServerInformation
            {
                ServerId = Encoding.UTF8.GetBytes("router-test-1"),
                ServerName = "router-test-1",
                ActiveSince = DateTime.UtcNow
            };

            router.Register(session);

            var foundByName = router.FindByName("router-test-1");
            Console.WriteLine(foundByName == session ? "EnterpriseRouter: FindByName OK" : "EnterpriseRouter: FindByName FAIL");

            var foundById = router.FindByServerId(session.AreaServerInfo.ServerId.Span);
            Console.WriteLine(foundById == session ? "EnterpriseRouter: FindById OK" : "EnterpriseRouter: FindById FAIL");

            router.Unregister(session);
            var after = router.FindByName("router-test-1");
            Console.WriteLine(after == null ? "EnterpriseRouter: Unregister OK" : "EnterpriseRouter: Unregister FAIL");

            try { session.Disconnect(); } catch { }
            await Task.CompletedTask;
        }

        private void RunAdapterDecodeTest()
        {
            using var scope = _services.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<CompatibilityAdapterRegistry>();

            // Enable SBOL adapter for test
            Environment.SetEnvironmentVariable("MPS_SBOL_COMPAT", "1");

            // Construct a simple SBOL-style packet: [size: ushort BE][type: ushort BE][subtype: ushort BE][payload]
            var type = (ushort)0x6810; // some opcode
            var subType = (ushort)0x0000;
            var payload = Encoding.UTF8.GetBytes("hello");
            var contentLen = 4 + payload.Length; // type+subtype+payload
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)contentLen)));
            buf.AddRange(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)type)));
            buf.AddRange(BitConverter.GetBytes((ushort)System.Net.IPAddress.HostToNetworkOrder((short)subType)));
            buf.AddRange(payload);

            var messages = new List<Fragment.NetSlum.Networking.Objects.FragmentMessage>();
            var consumed = registry.TryDecode(buf.ToArray(), messages);
            Console.WriteLine(consumed > 0 && messages.Count > 0 ? "AdapterDecode: OK" : "AdapterDecode: FAIL");
        }
    }
}
