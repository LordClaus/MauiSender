using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSender.Models;

public class SettingsModel
{
    public int BaseIntervalSec { get; set; } = 45;
    public int PartIntervalSec { get; set; } = 45;
    public int JitterPct { get; set; } = 15;
    public string TemplateMode { get; set; } = "randomWeighted";
    public string BaseUrl { get; set; } = "https://goldenbride.com/";
}
