using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace Protega___Crash_Reporter
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {

            //REFINE!!!!
            StreamReader file = null;
            string sFilePath = AppDomain.CurrentDomain.BaseDirectory + "latest_protega_error.err";
            string sError = "";
                   
            try
            {
                using (FileStream stream = File.Open(sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            sError = sError + reader.ReadLine();
                        }
                    }
                }

            }
            catch (Exception de)
            {
                string m = de.Message;
                sError = "The CrashReporter was not able to load the error file!";
                StartErrorWindow(sError);
                return;
            }
      
            File.Delete(sFilePath);
            StartErrorWindow(sError);
          
            
        }

        void StartErrorWindow(string sError)
        {
            MainWindow mw = new MainWindow(sError);
            mw.Show();
            mw.Activate();
        }

    }
}
