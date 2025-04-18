
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class HtmlLogs
{
    private string _currLog = "";
    private StringBuilder _logs = new StringBuilder();

    public string Get()
    {
        return _logs.ToString();
    }

    public void Write(string v, string? color = null)
    {
        if (color == null) color = "gray";
        _currLog += $"<span style='color: {color}'>{v}</span>";
    }


    public void WriteLine(string v, string? color = null)
    {
        Open();
        Write(v);
        Close();
    }

    public void Open()
    {
        _currLog = "<div>";
        WriteDateTime();
    }

    private void WriteDateTime()
    {
        _currLog += $"<span>{DateTime.Now.ToString("HH:mm:ss.ffff") + "| "}</span>";
    }

    public void Close()
    {
        _currLog += "</div>";
        _logs.Insert(0, _currLog);
    }
}