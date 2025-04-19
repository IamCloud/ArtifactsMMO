
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class HtmlLogs
{
    private const int MaxLogCount = 200;
    private string _currLog = "";
    private List<string> _logs = new List<string>();

    public string Get()
    {
        StringBuilder sb = new StringBuilder();
        foreach (string log in _logs)
        {
            sb.AppendLine(log);
        }
        return sb.ToString();
    }

    public void Write(string v, string? color = null)
    {
        if (color == null) color = "light-gray";
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

        if (_logs.Count > MaxLogCount)
        {
            _logs.RemoveAt(_logs.Count - 1);
        }

        _logs.Insert(0, _currLog);
    }
}