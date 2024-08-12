namespace UI.Models
{
    public class TabPanel
    {
        public Dictionary<string, TabPanelItem> Items { get; set; } = null!;
    }


    public class TabPanelItem
    {
        public bool IsValid { get; set; }
    }
}
