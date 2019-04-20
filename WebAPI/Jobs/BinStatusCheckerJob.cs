
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Models;
using WebAPI.Integrations;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Jobs
{
    public class BinStatusCheckerJob : BackgroundService
    {
        private const string EndPoint = "https://final-year-19.firebaseio.com/testbins.json";
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _accessor;

        public BinStatusCheckerJob(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor accessor)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _accessor = accessor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using(var scope = _serviceScopeFactory.CreateScope())
                {
                    var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    Debug.WriteLine("IP Address => " + ip);

                    var serviceProvider = scope.ServiceProvider;
                    var context = serviceProvider.GetService<AuthenticationContext>();

                    //Fetch data from firebase using HTTP Client
                    var res = await EndPoint.GetJsonAsync<Dictionary<string, BinDto>>();
                    var bins = res.Select(q => q.Value).ToList();
                    Debug.WriteLine(bins);


                    //Update bin status in your database
                    foreach (var bin in bins)
                    {
                        var dbBin = context.Bins.FirstOrDefault(q=>q.Name == bin.Name);
                        if (dbBin == null) continue;

                        dbBin.Distance = bin.Distance;
                        context.Bins.Update(dbBin);

                        //Create order in your database if bin is full
                        if(bin.Distance >= 75)
                        {
                            //Todo: Check if order already created to skip
                            var orderExist = context.Orders.FirstOrDefault(q => q.BinId == dbBin.Id && !q.Completed);
                            if(orderExist == null)
                            {
                                var newOrder = new Order
                                {
                                    BinId = dbBin.Id,
                                    DateRequested = DateTime.UtcNow
                                };
                                context.Orders.Add(newOrder);

                                //Send sms to borlamen on duty
                                //Todo: Get current IP Address

                                //var url = $"{ip}/4200/home/routermaps";
                                //var messenger = new Messenger();
                                //var borlamen = context.Users.Where(q => q.OnDuty).ToList();
                                //borlamen.ForEach(async q => await messenger.SendMessage(q.PhoneNumber, $"New Order Alert\n---------\nBin at ${dbBin.Location} is full.\n\n View Bin {url}."));
                            }

                        }

                        //Set pending to complete when bin goes empty
                        var pendingOrder = context.Orders.FirstOrDefault(q => q.BinId == dbBin.Id && bin.Distance < 15 && !q.Completed);
                        if(pendingOrder == null)
                        {
                            pendingOrder.Completed = true;
                            pendingOrder.DateCompleted = DateTime.UtcNow;
                            context.Orders.Update(pendingOrder);
                        }
                    }
                    context.SaveChanges();
                    await Task.Delay(1000 * 5 * 1, stoppingToken); //Delay for 1 minute
                }
               
            }
        }
    }

    public class BinDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Distance { get; set; }
    }
    
}
