using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure.Devices.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ClimateControl.Web.IoTHub
{
    public class MessageProcessor
    {
        private static string iotDeviceId = ConfigurationManager.AppSettings["IoTDeviceId"];
        private static string connectionString = ConfigurationManager.AppSettings["IoTHubConnectionString"];
        private static string iotHubD2cEndpoint = ConfigurationManager.AppSettings["IoTHubD2CEndpoint"];
        private static EventHubClient eventHubClient;
        private static EventHubReceiver eventHubReceiver;

        private void SetHubs()
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            int eventHubPartitionsCount = eventHubClient.GetRuntimeInformation().PartitionCount;
            string partition = EventHubPartitionKeyResolver.ResolveToPartition(iotDeviceId,eventHubPartitionsCount);
            eventHubReceiver = eventHubClient.GetConsumerGroup("$Default").CreateReceiver(partition, DateTime.Now.AddMinutes(-5));
        }

        private async void GetData()
        {
            string result = null;            
            EventData eventData = await eventHubReceiver.ReceiveAsync();
            if (eventData != null)
            {
                result = Encoding.UTF8.GetString(eventData.GetBytes());                
            }            
        }
    }
}