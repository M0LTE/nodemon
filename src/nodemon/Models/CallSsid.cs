namespace nodemon.Models;

public record CallSsid
{
    private CallSsid(string call, int ssid)
    {
        Call = call;
        Ssid = ssid;
    }

    public string Call { get; }
    public int Ssid { get; }

    public override string ToString()
    {
        if (Ssid == 0)
        {
            return Call;
        }

        return Call + "-" + Ssid;
    }

    /// <summary>
    /// Parse a string like M0LTE or M0LTE-2 into a CallSsid object
    /// </summary>
    /// <param name="callSsid"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when the call/ssid is invalid</exception>
    public static CallSsid Parse(string callSsid)
    {
        var parts = callSsid.Split('-');

        if (parts.Length == 1)
        {
            parts = [parts[0], "0"];
        }
        else if (parts.Length != 2)
        {
            throw new ArgumentException("Invalid call ssid format");
        }

        var callPart = parts[0].Trim();
        if (callPart.Length > 6)
        {
            throw new ArgumentException("Call part is too long");
        }

        if (string.Empty == callPart)
        {
            throw new ArgumentException("Call part is empty");
        }

        if (!int.TryParse(parts[1], out var ssid) || ssid < 0 || ssid > 15)
        {
            throw new ArgumentException("Invalid ssid");
        }

        return new CallSsid(parts[0], int.Parse(parts[1]));
    }

    public static implicit operator CallSsid(string callSsid) => Parse(callSsid);
}
