using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.
using QuickFix;
using System;
using System.IO;

namespace TradeClient
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("=============");
            Console.WriteLine("This is only an example program, meant to run against the Executor or SimpleAcceptor example programs.");
            Console.WriteLine();
            Console.WriteLine("                                                    ! ! !");
            Console.WriteLine("              DO NOT USE THIS ON A COMMERCIAL FIX INTERFACE!  It won't work and it's a bad idea!");
            Console.WriteLine("                                                    ! ! !");
            Console.WriteLine();
            Console.WriteLine("=============");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Read QuickFIX/n settings from appsettings.json
            var quickFixConfig = configuration.GetSection("QuickFixSettings").GetChildren();


            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);
                    foreach (var section in quickFixConfig)
                    {
                        writer.WriteLine("[" + section.Key + "]");
                        foreach (var kvp in section.GetChildren())
                        {
                            writer.WriteLine(kvp.Key + "=" + kvp.Value);
                        }
                    }

                    writer.Flush();
                    memoryStream.Position = 0;

                    // Create SessionSettings from the MemoryStream
                    var settings = new SessionSettings(memoryStream.ToString());

                    // Use the settings as needed for QuickFIX/n

                TradeClientApp application = new TradeClientApp();
                QuickFix.IMessageStoreFactory storeFactory = new QuickFix.FileStoreFactory(settings);
                QuickFix.ILogFactory logFactory = new QuickFix.ScreenLogFactory(settings);
                QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);

                // this is a developer-test kludge.  do not emulate.
                application.MyInitiator = initiator;

                initiator.Start();
                Console.Read();
                initiator.Stop();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            Environment.Exit(1);
        }
    }
}
