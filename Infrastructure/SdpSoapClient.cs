using System.Security;
using System.Text;
using System.Xml.Linq;

namespace SendSmsApi.Infrastructure;

public class SdpSoapClient
{
    private readonly HttpClient _httpClient;

    public SdpSoapClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendSmsAsync(
        string[] addresses,
        string senderName,
        string message,
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        string soapEnvelope = BuildSoapEnvelope(addresses, senderName, message, userName, password);

        using var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "\"\"");

        using var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        string responseXml = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseSendSmsResult(responseXml);
    }

    private static string BuildSoapEnvelope(
        string[] addresses,
        string senderName,
        string message,
        string userName,
        string password)
    {
        string addressElements = string.Join(
            Environment.NewLine,
            addresses.Select(a => $"       <addresses>{SecurityElement.Escape(a)}</addresses>"));

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <Envelope xmlns="http://schemas.xmlsoap.org/soap/envelope/">
              <Header>
                <wsse:Security
                  xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"
                  xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"
                  xmlns:p4="http://schemas.xmlsoap.org/soap/envelope/"
                  p4:mustUnderstand="1">
                  <wsse:UsernameToken>
                    <wsse:Username>{SecurityElement.Escape(userName)}</wsse:Username>
                    <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">{SecurityElement.Escape(password)}</wsse:Password>
                  </wsse:UsernameToken>
                </wsse:Security>
              </Header>
              <Body>
                <sendSms xmlns="http://www.csapi.org/schema/parlayx/sms/send/v2_1/local">
            {addressElements}
                  <senderName>{SecurityElement.Escape(senderName)}</senderName>
                  <message>{SecurityElement.Escape(message)}</message>
                </sendSms>
              </Body>
            </Envelope>
            """;
    }

    private static string ParseSendSmsResult(string responseXml)
    {
        var doc = XDocument.Parse(responseXml);
        XNamespace ns = "http://www.csapi.org/schema/parlayx/sms/send/v2_1/local";
        var result = doc.Descendants(ns + "result").FirstOrDefault()
                     ?? doc.Descendants("result").FirstOrDefault();
        return result?.Value ?? responseXml;
    }
}