using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace SportsResultsNotifier;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json");

        IConfiguration configuration = builder.Build();
        var settings = new Settings();
        configuration.GetSection("MailSettings").Bind(settings);
        
        string url = "https://www.basketball-reference.com/boxscores/";
        var gameAmount = WebScraper.ScrapeGameAmmount(url);
        var date = WebScraper.ScrapeDate(url);
        var games = WebScraper.Scraper(url);
        string gameData = @$"
{gameAmount}
{date}
";
        int gameNum = 1;
        foreach (var game in games)
        {
            gameData += @$"
Game number:{gameNum}
{game.SportsTeam1}: {game.Team1Score}
{game.SportsTeam2}: {game.Team2Score}
            ";
            gameNum++;
        }
        SendEmail(gameData, settings);
    }

    public static void SendEmail(string gameData, Settings settings)
    {
        string smtpAddress = "smtp.gmail.com";
        int portNumber = 587;
        bool enableSSL = true;
        // enter your own testing emails and password in appsettings.json file
        string emailFromAddress = $"{settings.emailFromAddress}";
        string password = $"{settings.password}";
        string emailToAddress = $"{settings.emailToAddress}";
        string subject = "Sports Results NBA";
        using (MailMessage mail = new MailMessage())
        {
            mail.From = new MailAddress(emailFromAddress);
            mail.To.Add(emailToAddress);
            mail.Subject = subject;
            mail.Body = gameData;
            mail.IsBodyHtml = true;
            using (SmtpClient smtpClient = new SmtpClient(smtpAddress, portNumber))
            {
                smtpClient.Credentials = new NetworkCredential(emailFromAddress, password);
                smtpClient.EnableSsl = enableSSL;
                smtpClient.Send(mail);
            }
        }
        Console.WriteLine("Email sent successfully!");
    }
}
