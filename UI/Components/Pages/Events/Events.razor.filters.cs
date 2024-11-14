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

                request.FeaturesIds = FeaturesList
                    .Where(x => value.Contains(x.Name))
                    .Select(s => s.Id)
                    .Distinct();

                //request.FeaturesIds = value.Select(s => s.Id);
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

                //_filteredFeatures = linq
                //    .GroupBy(g => g.Id)
                //    .Select(s => new FeaturesForEventsViewDto { Id = s.Key, Name = s.First().Name, NumberOfEvents = s.Count() })
                //    .OrderBy(o => o.Name)
                //    .ToList();

                _filteredFeatures = linq
                    .Select(s => s.Name)
                    .Distinct()
                    .OrderBy(o => o)
                    .ToList();

                return _filteredFeatures;
            }
        }
        #endregion

        #region Фильтр организаторов
        List<AdminsForEventsViewDto> AdminsList = new List<AdminsForEventsViewDto>();

        IEnumerable<AdminsForEventsViewDto> _selectedAdmins = null!;
        IEnumerable<AdminsForEventsViewDto> selectedAdmins
        {
            get => _selectedAdmins;
            set
            {
                filteredFeatures.Clear();
                filteredRegions.Clear();
                _selectedAdmins = value;
                request.AdminsIds = value.Select(s => s.Id);
                dataGrid.ReloadServerData();
            }
        }

        List<AdminsForEventsViewDto> _filteredAdmins = new List<AdminsForEventsViewDto>();
        List<AdminsForEventsViewDto> filteredAdmins
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
                    .GroupBy(g => g.Id)
                    .Select(s => new AdminsForEventsViewDto { Id = s.Key, Name = s.First().Name, NumberOfEvents = s.Count() })
                    .OrderBy(o => o.Name)
                    .ToList();

                return _filteredAdmins;
            }
        }
        #endregion

        #region Фильтр регионов
        IEnumerable<RegionsForEventsViewDto> _selectedRegions = null!;
        IEnumerable<RegionsForEventsViewDto> selectedRegions
        {
            get => _selectedRegions;
            set
            {
                filteredAdmins.Clear();
                filteredFeatures.Clear();
                _selectedRegions = value;
                request.RegionsIds = value.Select(s => s.Id);
                dataGrid.ReloadServerData();
            }
        }
        List<RegionsForEventsViewDto> _filteredRegions = new List<RegionsForEventsViewDto>();
        List<RegionsForEventsViewDto> filteredRegions
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
                    .GroupBy(g => g.Id)
                    .Select(s => new RegionsForEventsViewDto { Id = s.Key, Name = s.First().Name, NumberOfEvents = s.Count(), Order = s.First().Order })
                    .OrderBy(o => o.Order)
                    .ToList();

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
