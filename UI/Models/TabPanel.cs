namespace UI.Models
{
    public class TabPanel
    {
        public bool IsExpanded { get; set; }
        public bool IsDisabled { get; set; }
        public Dictionary<string, TabPanelItem> Items { get; set; } = null!;
    }


    public class TabPanelItem
    {
        public bool IsValid { get; set; }
    }
}
