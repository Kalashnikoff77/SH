namespace UI.Components.Pages
{
    public partial class Index
    {
        IEnumerable<CItems> _selectedItems = new List<CItems>();
        IEnumerable<CItems> selectedItems { get; set; } = new HashSet<CItems>();

        List<CItems> items = new List<CItems>
        {
            new CItems { Id = 1, Name = "Oleg" },
            new CItems { Id = 2, Name = "Dima" }
        };

        void OnAdded()
        {
            items.Add(new CItems { Id = 3, Name = "Sasha" });
        }

        void OnDeleted()
        {
            ((HashSet<CItems>)selectedItems).Remove(items[0]);
            items.Remove(items[0]);
            StateHasChanged();
            //selectedItems = null;
        }

        void OnChanged()
        {
            items[0].Name = "Oleg_changed";
        }

    }

    class CItems
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public override string ToString()
        {
            return Name;
        }
    }

}
