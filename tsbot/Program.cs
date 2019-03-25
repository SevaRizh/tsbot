using System;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace tsbot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotStart();            
        }

        private static string GetWeather(string City)
        {
            string url = @"https://xml.meteoservice.ru/export/gismeteo/point/";
            string textWeather = string.Empty;
            string textWind = string.Empty;
            string textPhenom = string.Empty;
            string defaulStr = string.Format("Не понимаем команду.\n Используйте:\n /Tver\n /Moscow\n /StPetersburg\n /Saratov\n /Staritsa\n");

            switch (City)
            {
                case "/Tver":
                    url += @"87.xml";
                    break;
                case "/Moscow":
                    url += @"37.xml";
                    break;
                case "/StPetersburg":
                    url += @"69.xml";
                    break;
                case "/Saratov":
                    url += @"7451.xml";
                    break;
                case "/Staritsa":
                    url += @"7185.xml";
                    break;
                default:
                    return defaulStr;
            }
            
            using (var webClient = new WebClient())
            {
                string data = webClient.DownloadString(url);

                var weatherCollection = XDocument.Parse(data)
                                                 .Descendants("MMWEATHER")
                                                 .Descendants("REPORT")
                                                 .Descendants("TOWN")
                                                 .Descendants("FORECAST").ToArray();

                textWeather = string.Empty;

                foreach (var item in weatherCollection)
                {

                    switch(item.Element("PHENOMENA").Attribute("precipitation").Value)
                    {
                        case "3":
                            textPhenom = "смешанные";
                            break;
                        case "4":
                            textPhenom = "дождь";
                            break;
                        case "5":
                            textPhenom = "ливень";
                            break;
                        case "6":
                            textPhenom = "снег";
                            break;
                        case "7":
                            textPhenom = "снег";
                            break;
                        case "8":
                            textPhenom = "гроза";
                            break;
                        case "10":
                            textPhenom = "без осадков";
                            break;
                        default:
                            textPhenom = "нет данных";
                            break;
                    }
                        
                    switch(item.Element("WIND").Attribute("direction").Value)
                    {
                        case "0":
                            textWind = "\"С\"";
                            break;
                        case "1":
                            textWind = "\"СВ\"";
                            break;
                        case "2":
                            textWind = "\"В\"";
                            break;
                        case "3":
                            textWind = "\"ЮВ\"";
                            break;
                        case "4":
                            textWind = "\"Ю\"";
                            break;
                        case "5":
                            textWind = "\"ЮЗ\"";
                            break;
                        case "6":
                            textWind = "\"З\"";
                            break;
                        case "7":
                            textWind = "\"СЗ\"";
                            break;
                        default:
                            break;
                    }

                        textWeather += string.Format(
                            "Дата: {0}-{1}-{2} {3}:00 \nТемп: {4} - {5} °C Дав: {6} мм.рт.ст. \nВетер: {8} м/с {9} Осадки: {7}\n\n",
                            item.Attribute("day").Value,
                            item.Attribute("month").Value,
                            item.Attribute("year").Value,
                            item.Attribute("hour").Value,
                            item.Element("TEMPERATURE").Attribute("min").Value,
                            item.Element("TEMPERATURE").Attribute("max").Value,
                            item.Element("PRESSURE").Attribute("max").Value,
                            textPhenom,
                            item.Element("WIND").Attribute("max").Value,
                            textWind
                            );
                }
            }

            return textWeather;
        }

        private static void BotStart()
        {
            int update_id = 0;
            string messageFromId = "";
            string messageText = "";
            string firstName = "";
            string token = "654852160:AAG7y_ti4yoLcNC41JyYtDYkS09V1R7dVyY";

            string startUrl = $"https://api.telegram.org/bot{token}";

            while (true)
            {
                using (var webClient = new WebClient())
                {
                    string url = $"{startUrl}/getUpdates?offset={update_id + 1}";

                    try
                    {
                        string response = webClient.DownloadString(url);

                        var Messages = JObject.Parse(response)["result"].ToArray();

                        foreach (var currentMessage in Messages)
                        {
                            update_id = Convert.ToInt32(currentMessage["update_id"]);

                            firstName = currentMessage["message"]["from"]["first_name"].ToString();
                            messageFromId = currentMessage["message"]["from"]["id"].ToString();
                            messageText = currentMessage["message"]["text"].ToString();

                            Console.WriteLine($"{firstName} {messageFromId} {messageText}");

                            messageText = GetWeather(messageText);
                            url = $"{startUrl}/sendMessage?chat_id={messageFromId}&text={messageText}";
                            webClient.DownloadString(url);

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}
