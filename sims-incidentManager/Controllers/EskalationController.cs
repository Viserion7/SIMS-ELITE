using Microsoft.AspNetCore.Mvc;
using sims_incidentManager.DTOs;
using System.Net;
using System.Net.Mail;
using System.Text.Json.Nodes;

namespace sims_incidentManager.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EskalationController : ControllerBase
    {
        private readonly ILogger<EskalationController> logger;
        private readonly HttpClient httpClient;

        public EskalationController(ILogger<EskalationController> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
        }

        [HttpPost]
        [EndpointDescription("Escalate Incident and send real email to admins via SMTP")]
        public async Task<ActionResult> Escalate([FromBody] EscalateRequestDto dto)
        {
            // Identity
            var identityUrl = Environment.GetEnvironmentVariable("IDENTITY_URL") ?? "http://identity:8080";
            var identityResponse = await httpClient.GetAsync($"{identityUrl}/api/v1/User/toNotify");

            if (!identityResponse.IsSuccessStatusCode)
            {
                logger.LogError("Identity Service is not reachable");
                return Problem("Identity Service not reachable");
            }

            var emailServer = Environment.GetEnvironmentVariable("EMAIL_SERVER");
            var emailUser = Environment.GetEnvironmentVariable("EMAIL_USER");
            var emailSecret = Environment.GetEnvironmentVariable("EMAIL_SECRET");
            var emailPort = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out var p) ? p : 587;

            if (string.IsNullOrEmpty(emailServer) || string.IsNullOrEmpty(emailSecret) || string.IsNullOrEmpty(emailUser))
            {
                logger.LogError("EMAIL_SERVER, EMAIL_USER or EMAIL_SECRET not configured.");
                return Problem("Server misconfiguration: Mail environment variables not set.");
            }

            // identity htpt req
            var jsonString = await identityResponse.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(jsonString);
            if (jsonNode == null)
            {
                return Problem("Failed to parse identity response");
            }

            var recipients = new List<string>();
            if (jsonNode is JsonArray jsonArray) // list
            {
                foreach (var item in jsonArray)
                {
                    var email = item?["email"]?.ToString();
                    if (!string.IsNullOrEmpty(email))
                        recipients.Add(email);
                }
            }
            else if (jsonNode?["email"] != null) // not a list
            {
                recipients.Add(jsonNode["email"]!.ToString());
            }

            if (recipients.Count == 0)
            {
                logger.LogWarning("No admin email addresses found in Identity response.");
                return NotFound("No escalation recipients found.");
            }

            // smtp
            using var smtpClient = new SmtpClient(emailServer)
            {
                Port = emailPort,
                Credentials = new NetworkCredential(emailUser, emailSecret),
                EnableSsl = true
            };

            var subject = $"[SIMS ESCALATION] Incident {dto.IncidentId}";
            var body = $"SIMS Escalation Alert\n\nIncident ID: {dto.IncidentId}\nMessage:\n{dto.Message}";

            foreach (var recipient in recipients)
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailUser, "SIMS Incident Manager"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(recipient);

                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInformation("Email sent successfully to {Recipient}", recipient);
            }
            
            return Ok();
        }
    }
}