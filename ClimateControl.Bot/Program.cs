using ClimateControl.AzureDb;
using ClimateControl.Data.Entities;
using ClimateControl.Web.Helpers;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


namespace ClimateControl.Bot
{
    class Program
    {
        private static string botKey = ConfigurationManager.AppSettings["TelegramBotKey"];
        private static readonly TelegramBotClient Bot =
            new TelegramBotClient(botKey);
        private static AzureDbRepository repository = new AzureDbRepository();

        static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Console.WriteLine("Starting bot...");
            var me = Bot.GetMeAsync().Result;
            Console.WriteLine($"Successfully started: {me.Username}");
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();

        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.TextMessage) return;
            Console.WriteLine($"Received message: {message.Text}\nFrom: {message.From.Username}");
            if (message.Text.StartsWith("/get_climate"))
            {
                Sensor latestSensorData;
                latestSensorData = repository.GetSensorData()
                    .OrderByDescending(d => d.Timestamp)
                    .Take(1)
                    .SingleOrDefault();                    
                if (latestSensorData != null)
                {
                    var reply =
                        $"Temperature: {latestSensorData.Temperature:0.0}C\n" +
                        $"Humidity: {latestSensorData.Humidity:0.00}%\n" +
                        $"CO2: {latestSensorData.CO2}\n" +
                        $"Updated: {TimeZoneConverter.Convert(latestSensorData.Timestamp):dd/MM/yyyy HH:mm:ss}";
                    Console.WriteLine($"Sending response: {reply}");
                    await Bot.SendTextMessageAsync(message.Chat.Id, reply);
                }
                else
                {
                    var usage = @"Usage:
/get_climate   - Get the latest sensor data
";
                    await Bot.SendTextMessageAsync(message.Chat.Id, usage,
                        replyMarkup: new ReplyKeyboardMarkup());
                }
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine($"Error received: {receiveErrorEventArgs.ApiRequestException.Message}");
            Debugger.Break();
        }

        private static void BotOnChosenInlineResultReceived(object sender,
            ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine(
                $"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static async void BotOnCallbackQueryReceived(object sender,
            CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
