using MudBlazor;

namespace UI.Components.Pages
{
    public partial class Index
    {
        MudDataGrid<Item> dataGrid;
        string searchString = null!;
        int counter = 5;

        private async Task<GridData<Item>> ServerReload(GridState<Item> state)
        {
            var t1 = state.Page;
            var t2 = state.PageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                data = data.Where(x => x.Name == searchString).ToList();
            }

            var items = new GridData<Item>
            {
                Items = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray(),
                TotalItems = data.Count
            };
            return items;
        }

        private Task OnSearch(string text)
        {
            searchString = text;
            return dataGrid.ReloadServerData();
        }

        void AddItem()
        {
            data.Add(new Item { Id = counter++, Name = "Олег" + counter, Date = DateTime.Parse("29/01/1977") });
            dataGrid.ReloadServerData();
        }

        List<Item> data = new List<Item>
        {
            new Item { Id = 1, Name = "Олег", Date = DateTime.Parse("29/01/1977") },
            new Item { Id = 2, Name = "Марина", Date = DateTime.Parse("01/07/1969") },
            new Item { Id = 3, Name = "Сергей", Date = DateTime.Parse("01/10/1969") },
            new Item { Id = 4, Name = "Татьяна", Date = DateTime.Parse("15/09/1970") },
        };
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}
