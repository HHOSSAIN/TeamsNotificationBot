using TeamsNotificationBot.Models;
using AdaptiveCards.Templating;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamsFx.Conversation;
using Newtonsoft.Json;
using RestSharp;
using System.Collections;

namespace TeamsNotificationBot.Controllers
{
    [Route("api/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ConversationBot _conversation;
        private readonly string _adaptiveCardFilePath = Path.Combine(".", "Resources", "NotificationDefault.json");

        public NotificationController(ConversationBot conversation)
        {
            this._conversation = conversation;
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(CancellationToken cancellationToken = default)
        {
            // Read adaptive card template
            var cardTemplate = await System.IO.File.ReadAllTextAsync(_adaptiveCardFilePath, cancellationToken);

            /*test code start*/
            Console.WriteLine("testing notification");
            string strIPLocation = string.Empty;
            var client = new RestClient("https://ipapi.co/json");
            var request = new RestRequest()
            {
                Method = Method.Get
            };
            var response = client.Execute(request);
            var dictionary = JsonConvert.DeserializeObject<IDictionary>(response.Content);
            foreach (var item in dictionary.Keys)
            {
                strIPLocation += item.ToString() + ": " + dictionary[item] + "\r\n";
            }
            /*test code end*/

            var installations = await this._conversation.Notification.GetInstallationsAsync(cancellationToken);
            foreach (var installation in installations)
            {
                // Build and send adaptive card
                var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
                (
                    new NotificationDefaultModel
                    {
                        Title = "New Event Occurred!",
                        AppName = "Contoso App Notification tesstt",
                        Description = $"This is a sample http-triggered notification to {installation.Type}. tessttt text",
                        NotificationUrl = "https://www.adaptivecards.io/",
                        TestField = $"testing if it works {strIPLocation}" //referring to ${testfield} in notificationdefault.json
                    }
                );
                await installation.SendAdaptiveCard(JsonConvert.DeserializeObject(cardContent), cancellationToken);
            }

            return Ok();
        }
    }
}
