using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace ErrorFileWatcher
{
    static class Program
    {
        public const string SERVICE_NAME = "ErrorFileWatcher";
        public const string DISPLAY_NAME = "File Watcher Service (自動受入エラー検出)";

        static void Main(string[] args)
        {
            // Run as console mode when an argument is provided.
            if (1 <= args.Length)
            {
                RunAsConsoleMode(args[0]);
                return;
            }

            // Run as Windows Service
            ServiceBase.Run(new Service1()
            {
                CanShutdown = true,
                CanPauseAndContinue = false,
            });
        }

        /// <summary>
        /// Run as console mode when an argument is provided.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        static void RunAsConsoleMode(string arg)
        {
            string mode = arg.ToLower();
            var myAssembly = System.Reflection.Assembly.GetEntryAssembly();
            string path = myAssembly.Location;

            if (mode == "/i" || mode == "/u")
            {
                bool isInstallMode = (mode == "/i");
                var mes = (isInstallMode) ? "installed" : "uninstalled";
                if (IsServiceExists())
                {
                    Console.WriteLine("{0} has been already {1}.", DISPLAY_NAME, mes);
                    if (!isInstallMode)
                    {
                        StopService();
                        var param = (mode == "/i") ? new[] { path } : new[] { "/u", path };
                        ManagedInstallerClass.InstallHelper(param);
                        Console.WriteLine("{0} has been successfully {1}.", DISPLAY_NAME, mes);
                    }
                }
                else
                {
                    if (!isInstallMode) { StopService(); }
                    var param = (mode == "/i") ? new[] { path } : new[] { "/u", path };
                    ManagedInstallerClass.InstallHelper(param);
                    Console.WriteLine("{0} has been successfully {1}.", DISPLAY_NAME, mes);
                    if (isInstallMode) { StartService(); }
                }
            }
            else if (mode == "/start")
            {
                StartService();
            }
            else if (mode == "/stop")
            {
                StopService();
            }
            else
            {
                Console.WriteLine("Provided arguments are unrecognized.");
            }
        }

        /// <summary>
        /// Whether the service already exists in the computer or not.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static bool IsServiceExists(string name = SERVICE_NAME)
        {
            ServiceController[] services = ServiceController.GetServices();
            return services.Any(s => s.ServiceName == name);
        }

        /// <summary>
        /// Start the service.
        /// </summary>
        /// <returns></returns>
        static void StartService()
        {
            if (IsServiceExists())
            {
                FileWatcher fw = new FileWatcher();
                fw.Watch();
                Console.WriteLine("Starting {0}...", SERVICE_NAME);
                ServiceController sc = new ServiceController(SERVICE_NAME);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine("{0} has been already started.", DISPLAY_NAME);
                }
                else
                {
                    try
                    {
                        sc.Start();
                        Console.WriteLine("{0} has been started.", DISPLAY_NAME);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} could not be started.", DISPLAY_NAME);
                    }
                }
            }
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        /// <returns></returns>
        static void StopService()
        {
            if (IsServiceExists())
            {
                Console.WriteLine("Stopping {0}...", SERVICE_NAME);
                ServiceController sc = new ServiceController(SERVICE_NAME);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("{0} has been already stopped.", DISPLAY_NAME);
                }
                else
                {
                    try
                    {
                        sc.Stop();
                        Console.WriteLine("{0} has been stopped.", DISPLAY_NAME);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("{0} could not be stopped.", DISPLAY_NAME);
                    }
                }
            }
        }
    }

    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        /// <summary>
        /// プロジェクト インストーラー
        /// </summary>
        public ProjectInstaller()
        {
            var spi = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };
            var si = new ServiceInstaller
            {
                ServiceName = Program.SERVICE_NAME,
                DisplayName = Program.DISPLAY_NAME,
                Description = "EM サーバーの自動受入エラーフォルダを監視し メールにて通知するサービス。",
                StartType = ServiceStartMode.Automatic,
            };
            this.Installers.Add(spi);
            this.Installers.Add(si);
        }
    }

}
