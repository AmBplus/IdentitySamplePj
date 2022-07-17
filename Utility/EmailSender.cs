using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;

namespace Utilities;

public class EmailSender : IEmailSender
{
    public string Server { get; set; }
    public int Port { get; set; }
    public EmailSender(int port, string server)
    {
        Server = server;
        Port = port;
    }
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
      using SmtpClient client = new SmtpClient();
        //client.Connect(Server,Port,SecureSocketOptions.StartTls);
        client.Connect(Server, Port, SecureSocketOptions.SslOnConnect);
        var mimeMessage = CreateMimeMessage(email, subject, htmlMessage);
        client.Authenticate("senderemail79@gmail.com", "ovwimaywrptblrqy");
      // client.Authenticate("brook.tromp57@ethereal.email", "H8u9mggv2V6feUqPYA");
        try
        {
            client.Send(mimeMessage);
            
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
        return Task.CompletedTask;
    }

    public MimeMessage CreateMimeMessage(string email, string subject, string htmlMessage)
    {
        return   new MimeMessage()
        {

            From =
            {
                MailboxAddress.Parse("senderemail79@gmail.com")
            },
            To = { MailboxAddress.Parse(email) },
            Subject = subject,
            Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage,
            }
        };

    }
}