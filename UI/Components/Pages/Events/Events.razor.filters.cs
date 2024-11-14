using Common.Dto.Views;

namespace UI.Components.Pages.Events
{
    public partial class Events
    {
        #region Фильтр услуг
        List<FeaturesForEventsViewDto> FeaturesList = new List<FeaturesForEventsViewDto>();

        IEnumerable<string> _selectedFeatures = null!;
        IEnumerable<string> selectedFeatures
        {
            get => _selectedFeatures;
            set
            {
                filteredAdmins.Clear();
                filteredRegions.Clear();
                _selectedFeatures = value;

                if (value != null)
                    request.FeaturesIds = FeaturesList
                        .Where(x => value.Contains(x.Name))
                        .Select(s => s.Id)
                        .Distinct();

                dataGrid.ReloadServerData();
            }
        }

        List<string> _filteredFeatures = new List<string>();
        List<string> filteredFeatures
        {
            get
            {
                if (_filteredFeatures.Count() > 0)
                    return _filteredFeatures;

                IEnumerable<FeaturesForEventsViewDto> linq = FeaturesList;

                if (isActualEvents)
                    linq = FeaturesList.Where(w => w.EndDate > DateTime.Now);
                else
                    linq = FeaturesList.Where(w => w.EndDate < DateTime.Now);

                if (request.RegionsIds?.Count() > 0)
                    linq = linq.Where(x => request.RegionsIds.Contains(x.RegionId));

                if (request.AdminsIds?.Count() > 0)
                    linq = linq.Where(x => request.AdminsIds.Contains(x.AdminId));

                _filteredFeatures = linq
                    .Select(s => s.Name)
                    .Distinct()
                    .OrderBy(o => o)
                    .ToList();

                if (selectedFeatures?.Count() > 0)
                {
                    if (!selectedFeatures.All(x => _filteredFeatures.Contains(x)))
                    {
                        selectedFeatures = null!;
                        StateHasChanged();
                    }
                }

                return _filteredFeatures;
            }
        }
        #endregion


        #region Фильтр организаторов
        List<AdminsForEventsViewDto> AdminsList = new List<AdminsForEventsViewDto>();

        IEnumerable<string> _selectedAdmins = null!;
        IEnumerable<string> selectedAdmins
        {
            get => _selectedAdmins;
            set
            {
                filteredFeatures.Clear();
                filteredRegions.Clear();
                _selectedAdmins = value;
                if (value != null)
                    request.AdminsIds = AdminsList
                        .Where(x => value.Contains(x.Name))
                        .Select(s => s.Id)
                        .Distinct();
                
                dataGrid.ReloadServerData();
            }
        }

        List<string> _filteredAdmins = new List<string>();
        List<string> filteredAdmins
        {
            get
            {
                if (_filteredAdmins.Count() > 0)
                    return _filteredAdmins;

                IEnumerable<AdminsForEventsViewDto> linq = AdminsList;

                if (isActualEvents)
                    linq = AdminsList.Where(w => w.EndDate > DateTime.Now);
                else
                    linq = AdminsList.Where(w => w.EndDate < DateTime.Now);

                if (request.RegionsIds?.Count() > 0)
                    linq = linq.Where(x => request.RegionsIds.Contains(x.RegionId));

                if (request.FeaturesIds?.Count() > 0)
                    linq = linq.Where(x => request.FeaturesIds.Contains(x.FeatureId));

                _filteredAdmins = linq
                    .Select(s => s.Name)
                    .Distinct()
                    .OrderBy(o => o)
                    .ToList();

                if (selectedAdmins?.Count() > 0)
                {
                    if (!selectedAdmins.All(x => _filteredAdmins.Contains(x)))
                    {
                        selectedAdmins = null!;
                        StateHasChanged();
                    }
                }

                return _filteredAdmins;
            }
        }
        #endregion


        #region Фильтр регионов
        List<RegionsForEventsViewDto> RegionsList = new List<RegionsForEventsViewDto>();

        IEnumerable<string> _selectedRegions = null!;
        IEnumerable<string> selectedRegions
        {
            get => _selectedRegions;
            set
            {
                filteredAdmins.Clear();
                filteredFeatures.Clear();
                _selectedRegions = value;

                if (value != null)
                    request.RegionsIds = RegionsList
                        .Where(x => value.Contains(x.Name))
                        .Select(s => s.Id)
                        .Distinct();

                dataGrid.ReloadServerData();
            }
        }
        List<string> _filteredRegions = new List<string>();
        List<string> filteredRegions
        {
            get
            {
                if (_filteredRegions.Count() > 0)
                    return _filteredRegions;

                IEnumerable<RegionsForEventsViewDto> linq = RegionsList;

                if (isActualEvents)
                    linq = RegionsList.Where(w => w.EndDate > DateTime.Now);
                else
                    linq = RegionsList.Where(w => w.EndDate < DateTime.Now);

                if (request.AdminsIds?.Count() > 0)
                    linq = linq.Where(x => request.AdminsIds.Contains(x.AdminId));

                if (request.FeaturesIds?.Count() > 0)
                    linq = linq.Where(x => request.FeaturesIds.Contains(x.FeatureId));

                _filteredRegions = linq
                    .OrderBy(o => o.Order)
                    .Select(s => s.Name)
                    .Distinct()
                    .ToList();

                if (selectedRegions?.Count() > 0)
                {
                    if (!selectedRegions.All(x => _filteredRegions.Contains(x)))
                    {
                        selectedRegions = null!;
                        StateHasChanged();
                    }
                }

                return _filteredRegions;
            }
        }
        #endregion


        #region Фильтр актуальных мероприятий
        string actualEventsLabel = "Актуальные мероприятия";
        bool isActualEvents
        {
            get => request.IsActualEvents;
            set
            {
                filteredFeatures.Clear();
                filteredAdmins.Clear();
                filteredRegions.Clear();
                request.IsActualEvents = value;
                actualEventsLabel = value ? "Актуальные мероприятия" : "Завершённые мероприятия";
                dataGrid.ReloadServerData();
            }
        }
        #endregion
    }
}
