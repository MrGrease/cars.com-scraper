using CefSharp.OffScreen;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CarScraper
{
    internal class Program
    {
        enum Steps
        {
            Login,
            EnterFirstSearch,
            ScrapeFirstPageForModelS,
            NextPage,
            ScrapeLastPageForModelS,
            SelectModelX,
            ScrapeFirstPageForModelX,
            Finish,
            Fail
        }
        private static ChromiumWebBrowser _browser;
        private static Steps _currentStep;
        private static string _url = "https://www.cars.com/signin/";
        static void Main(string[] args)
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;

            var settings = new CefSettings()
            {
            };
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            _browser = new ChromiumWebBrowser(_url);
            _browser.LoadingStateChanged += BrowserLoadingStateChanged;

            Console.ReadKey();
            Cef.Shutdown();
        }
        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
        }
}
