﻿using CefSharp.OffScreen;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CefSharp.MinimalExample.OffScreen
{
    public class CefCommand
    { 
        ChromiumWebBrowser browser;

        public int Do()
        {

            try
            {
                string[] args = { }; // fake args

                const string testUrl = "https://www.bing.com/";

                Console.WriteLine("This example application will load {0}, take a screenshot, and save it to your desktop.", testUrl);
                Console.WriteLine("You may see Chromium debugging output, please wait...");
                Console.WriteLine();

#if NETCOREAPP
                //We are using our current exe as the BrowserSubProcess
                //Multiple instances will be spawned to handle all the 
                //Chromium proceses, render, gpu, network, plugin, etc.
                var subProcessExe = new CefSharp.BrowserSubprocess.BrowserSubprocessExecutable();
                var result = subProcessExe.Main(args);
                if (result > 0)
                {
                    return result;
                }
#endif

                var settings = new CefSettings()
                {
                    //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                    CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
                };

#if NETCOREAPP
                //We use our Applications exe as the BrowserSubProcess, multiple copies
                //will be spawned
                //TODO: The OffScreen implementation is crashing on Exit (WPF/WinForms are working fine).
                //So for now this is commented out and the old .Net CefSharp.BrowserSubProcess.exe
                //is used.
                //var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                //settings.BrowserSubprocessPath = exePath;
#endif

                //Perform dependency check to make sure all relevant resources are in our output directory.
                Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

                // Create the offscreen Chromium browser.
                browser = new ChromiumWebBrowser(testUrl);

                // An event that is fired when the first page is finished loading.
                // This returns to us from another thread.
                browser.LoadingStateChanged += BrowserLoadingStateChanged;

                // We have to wait for something, otherwise the process will exit too soon.
                Console.ReadKey();

                // Clean up Chromium objects.  You need to call this in your application otherwise
                // you will get a crash when closing.
                Cef.Shutdown();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return 0;
        }

        private void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            // Check to see if loading is complete - this event is called twice, one when loading starts
            // second time when it's finished
            // (rather than an iframe within the main frame).
            if (!e.IsLoading)
            {
                // Remove the load event handler, because we only want one snapshot of the initial page.
                browser.LoadingStateChanged -= BrowserLoadingStateChanged;

                var scriptTask = browser.EvaluateScriptAsync("document.querySelector('[name=q]').value = 'CefSharp Was Here!'");

                scriptTask.ContinueWith(t =>
                {
                    //Give the browser a little time to render
                    Thread.Sleep(500);
                    // Wait for the screenshot to be taken.
                    var task = browser.ScreenshotAsync();
                    task.ContinueWith(x =>
                    {
                        // Make a file to save it to (e.g. C:\Users\jan\Desktop\CefSharp screenshot.png)
                        var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CefSharp screenshot.png");

                        Console.WriteLine();
                        Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

                        // Save the Bitmap to the path.
                        // The image type is auto-detected via the ".png" extension.
                        task.Result.Save(screenshotPath);

                        // We no longer need the Bitmap.
                        // Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
                        task.Result.Dispose();

                        Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

                        // Tell Windows to launch the saved image.
                        Process.Start(new ProcessStartInfo(screenshotPath)
                        {
                            // UseShellExecute is false by default on .NET Core.
                            UseShellExecute = true
                        });

                        Console.WriteLine("Image viewer launched.  Press any key to exit.");
                    }, TaskScheduler.Default);
                });
            }
        }
    }
}