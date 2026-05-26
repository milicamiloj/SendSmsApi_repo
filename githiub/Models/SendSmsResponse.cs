using System.ComponentModel;

namespace SendSmsApi.Models;

public class SendSmsResponse
{
    [Description("True ako je SMS uspješno poslat")]
    public bool Success { get; init; }

    [Description("Identifikator zahteva koji je vratio SDP")]
    public string? RequestId { get; init; }

    [Description("Poruka greške ukoliko slanje nije uspjelo")]
    public string? ErrorMessage { get; init; }
}