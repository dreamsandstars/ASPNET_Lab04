namespace AspNetWeek4.Mvc.Options;

public class AppSettings
{
    public string AppName { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public bool EnableSeedData { get; set; }
    public int LowSeatThreshold { get; set; } // Feature 1: Seat threshold config
}
