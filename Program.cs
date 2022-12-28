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
            if (!e.IsLoading)
            {
                Console.WriteLine("\n\n\n\n\n\nLOADED...................................\n\n\n\n\n\n");
                Console.WriteLine("Currently at step " + _currentStep);
                Console.WriteLine("Currently at address " + _browser.Address);

                switch (_currentStep)
                {
                    case Steps.Login:
                        var loginScript = @"document.querySelector('#email').value = 'johngerson808@gmail.com';
                        document.querySelector('#password').value = 'test8008';
                        document.querySelector('.sds-button').click()";
                        _browser.EvaluateScriptAsync(loginScript).ContinueWith(u => {
                            if (u.Result.Success)
                            {
                                _currentStep = Steps.EnterFirstSearch;
                                Console.WriteLine("USER LOGGED IN!");
                            }
                            else
                            {
                                Console.WriteLine("USER FAILED LOGGING IN!");
                                _currentStep = Steps.Fail;
                                _browser.LoadUrl(_url);

                            }
                        });
                        break;
                    case Steps.EnterFirstSearch:
                        break;
                    case Steps.ScrapeFirstPageForModelS:
                        Console.WriteLine("SCRAPING FIRST PAGE FOR MODEL S!");
                        break;
                    case Steps.NextPage:
                        Console.WriteLine("SWITCHING FILTER!");
                        break;
                    case Steps.ScrapeLastPageForModelS:
                        Console.WriteLine("SCRAPING LAST PAGE FOR MODEL S!");
                        break;
                    case Steps.SelectModelX:
                        Console.WriteLine("SWITCHING FILTER!");
                        break;
                    case Steps.ScrapeFirstPageForModelX:
                        Console.WriteLine("SCRAPING FIRST PAGE FOR MODEL X!");
                        break;
                    case Steps.Finish:
                        Console.WriteLine("!!!WORK COMPLETE!!!");
                        break;
                    case Steps.Fail:
                        Console.WriteLine("FAILED");
                        break;

                }
            }
            else
            {
                Console.WriteLine("\n\n\n\n\n\nLOADING.................................!\n\n\n\n\n\n");
                Console.WriteLine("address " + _browser.Address);
            }
        }
