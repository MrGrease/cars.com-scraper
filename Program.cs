using CefSharp.OffScreen;
using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
        private static string _scrapeScript = @"
                         (function(){cars=[];
                         document.querySelectorAll('.vehicle-card').forEach((child,index)=>{
                         	console.log(index);
                         	car=
                         	{
                         		link:'',
                         		mileage:'',
                         		primaryprice:'',
                         		monthlypayment:'',
                         		dealername:''
                         	}
                         	
                         	car.link=child.querySelector('.vehicle-card-link').href;
                         	car.mileage=child.querySelector('.mileage').textContent;
                         	car.primaryprice=child.querySelector('.primary-price').textContent;
                         	if(child.querySelector('.js-estimated-monthly-payment-formatted-value-with-abr')){
                         	car.monthlypayment=child.querySelector('.js-estimated-monthly-payment-formatted-value-with-abr').textContent;
                         	}
                         	car.dealername=child.querySelector('.dealer-name').textContent;
                         	
                         	cars.push(car);
                         });
                         return cars;})()";
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
                        _browser.EvaluateScriptAsync(loginScript).ContinueWith(u =>
                        {
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
                        var searchScript = @"document.querySelector('#make-model-search-stocktype').selectedIndex=3
                        document.querySelector('#makes').selectedIndex=27
                        document.querySelector('#models').selectedIndex=2
                        document.querySelector('#make-model-max-price').selectedIndex=18
                        document.querySelector('#make-model-maximum-distance').selectedIndex=11
                        document.querySelector('#make-model-zip').value='06530'
                        document.querySelector('.sds-home-search__submit').children[0].click()";
                        _browser.EvaluateScriptAsync(searchScript).ContinueWith(u =>
                        {
                            _currentStep = Steps.ScrapeFirstPageForModelS;
                            Console.WriteLine("SEARCH ENTERED!");
                        });
                        break;
                    case Steps.ScrapeFirstPageForModelS:
                        Console.WriteLine("SCRAPING FIRST PAGE FOR MODEL S!");
                        _browser.EvaluateScriptAsync(_scrapeScript).ContinueWith(u =>
                        {
                            _currentStep = Steps.NextPage;
                            Console.WriteLine("SCRAPE COMPLETE!");
                            if (u.Result.Success && u.Result.Result != null)
                            {
                                Console.WriteLine("First scrape ready");
                                var filePath = "Results.txt";
                                var response = (List<dynamic>)u.Result.Result;
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                File.WriteAllText(filePath, json);
                            }
                            else
                            {
                                Console.WriteLine("SOMETHING WENT WRONG!");
                                _currentStep = Steps.Fail;
                            }
                        });
                        Console.WriteLine("SCRAPING FIRST PAGE FOR MODEL S!");
                        break;
                    case Steps.NextPage:
                        Console.WriteLine("SWITCHING FILTER!");
                        var switchScript = @"document.querySelector('#next_paginate').click();";
                        _browser.EvaluateScriptAsync(switchScript).ContinueWith(u => {
                            _currentStep = Steps.ScrapeLastPageForModelS;
                            Console.WriteLine("PAGE SWITCHED!");
                        });
                        break;
                    case Steps.ScrapeLastPageForModelS:
                        Console.WriteLine("SCRAPING LAST PAGE FOR MODEL S!");
                        _browser.EvaluateScriptAsync(_scrapeScript).ContinueWith(u => {
                            _currentStep = Steps.SelectModelX;
                            Console.WriteLine("SCRAPE COMPLETE!");
                            if (u.Result.Success && u.Result.Result != null)
                            {
                                Console.WriteLine("Second scrape ready");
                                var filePath = "Results.txt";
                                var response = (List<dynamic>)u.Result.Result;
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                File.AppendAllText(filePath, json);
                            }
                            else
                            {
                                Console.WriteLine("SOMETHING WENT WRONG!");
                                _currentStep = Steps.Fail;
                            }
                        });
                        break;
                    case Steps.SelectModelX:
                        Console.WriteLine("SWITCHING FILTER!");
                        var selectScript = @"document.querySelector('#model_tesla-model_S').checked=false;
                        document.querySelector('#model_tesla-model_X').checked=true;
                        document.querySelector('#keyword_search_submit').click();";
                        _browser.EvaluateScriptAsync(selectScript).ContinueWith(u => {
                            _currentStep = Steps.ScrapeFirstPageForModelX;
                            Console.WriteLine("SELECTION COMPLETE!");
                        });
                        break;
                    case Steps.ScrapeFirstPageForModelX:
                        Console.WriteLine("SCRAPING FIRST PAGE FOR MODEL X!");
                        _browser.EvaluateScriptAsync(_scrapeScript).ContinueWith(u => {
                            _currentStep = Steps.Finish;
                            Console.WriteLine("SCRAPE COMPLETE!");
                            if (u.Result.Success && u.Result.Result != null)
                            {
                                Console.WriteLine("final scrape ready");
                                var filePath = "Results.txt";
                                var response = (List<dynamic>)u.Result.Result;
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                File.AppendAllText(filePath, json);
                                string text = File.ReadAllText("Results.txt");
                                text = text.Replace("][", ",");
                                File.WriteAllText("Results.txt", text);

                            }
                            else
                            {
                                Console.WriteLine("SOMETHING WENT WRONG!");
                                _currentStep = Steps.Fail;
                            }
                        });
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
    }
}
