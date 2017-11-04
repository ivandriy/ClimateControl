using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClimateControl.Web.Helpers;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
using ClimateControl.Web.Models;

namespace ClimateControl.Bot
{
    class Program
    {
        private static readonly TelegramBotClient Bot =
            new TelegramBotClient("458066998:AAGUGIVMC_e8cfMBwS2Pa3SNde0ahrB60OQ");

        private static ClimateControlEntities db = new ClimateControlEntities();

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
                SensorData latestSensorData;
                latestSensorData = (from s in db.SensorData
                        orderby s.timestamp descending
                        select s).Take(1)
                    .SingleOrDefault();
                if (latestSensorData != null)
                {
                    var reply =
                        $"Temperature: {latestSensorData.temperature:0.00}C\nHumidity: {latestSensorData.humidity:0.00}%\nCO2: {latestSensorData.co2}\nLast updated: {TimeZoneConverter.Convert(latestSensorData.timestamp)}";
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
