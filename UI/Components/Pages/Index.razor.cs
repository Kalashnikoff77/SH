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
            var item = new CItems { Id = 3, Name = "Sasha" };
            items.Add(item);

            item.Id = 4;
            items.Add(item);
        }

        void OnDeleted()
        {
            items[0].IsDeleted = true;

            //((HashSet<CItems>)selectedItems).Remove(items[0]);
            //items.Remove(items[0]);
            //selectedItems = null;
        }

        void OnChanged()
        {
            items[0].IsDeleted = false;
            //items[0].Name = "Oleg_changed";
        }

    }

    class CItems
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsDeleted { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }

}
