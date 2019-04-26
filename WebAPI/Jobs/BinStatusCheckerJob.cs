
using Flurl;
using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Integrations;
using WebAPI.Models;

namespace WebAPI.Jobs
{
    public class BinStatusCheckerJob : BackgroundService
    {
        private const string EndPoint = "https://api.thingspeak.com/channels";
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
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var serviceProvider = scope.ServiceProvider;
                    var context = serviceProvider.GetService<AuthenticationContext>();

                    var bins = context.Bins.ToList();

                    foreach (var bin in bins)
                    {
                        //Fetch data from thingspeak bin channel with channel id
                        var distance = EndPoint.AppendPathSegments(bin.ChannelId, "fields", 1)
                            .SetQueryParams(new { api_key = bin.ApiKey, results = 1 })
                            .GetJsonAsync<ResponseObj>().Result?.Feeds?.FirstOrDefault()?.Distance;

                        if (!distance.HasValue) continue;
                        
                        bin.Distance = distance.Value;
                        context.Bins.Update(bin);

                        //Create order in your database if bin is full
                        if (bin.Distance >= 75)
                        {
                            //Check if order already created to skip
                            var orderExist = context.Orders.FirstOrDefault(q => q.BinId == bin.Id && !q.Completed);
                            if (orderExist == null)
                            {
                                var newOrder = new Order
                                {
                                    BinId = bin.Id,
                                    DateRequested = DateTime.UtcNow
                                };
                                context.Orders.Add(newOrder);

                                //Send sms to borlamen on duty
                                var ip = System.Net.Dns.GetHostAddresses(Environment.MachineName)[1].ToString();
                                var url = $"{ip}/4200/home/routermaps";
                                var messenger = new Messenger();
                                var borlamen = context.Users.Where(q => q.OnDuty).ToList();
                                borlamen.ForEach(async q => await messenger.SendMessage(q.PhoneNumber, $"New Order Alert\n---------\nBin at ${bin.Location} is full.\n\n View Bin {url}."));
                            }

                        }

                        //Set pending to complete when bin goes empty
                        var pendingOrder = context.Orders.FirstOrDefault(q => q.BinId == bin.Id && !q.Completed);
                        if (pendingOrder != null && bin.Distance < 15)
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

    public class ResponseObj
    {
        public Channel Channel { get; set; }
        public List<Feed> Feeds { get; set; }
    }

    public class Channel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class Feed
    {
        [JsonProperty(PropertyName = "entry_id")]
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        [JsonProperty(PropertyName ="field1")]
        public int Distance { get; set; }
        
    }

}
