using Blazor.Tools.BlazorBundler.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.Tools.BlazorBundler.Entities
{
    public abstract class ATableGridData : ComponentBase
    {
        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _tableID = string.Empty;
        public string TableID
        {
            get { return _tableID; }
            set { _tableID = value; }
        }
        
        private int _totalRows;
        public int TotalRows
        {
            get { return _totalRows; }
            set { _totalRows = value; }
        }
        
        private int _totalCols;
        public int TotalCols
        {
            get { return _totalCols; }
            set { _totalCols = value; }
        }

        private List<TableColumnDefinition> _columnDefinitions = new List<TableColumnDefinition>();
        public List<TableColumnDefinition> ColumnDefinitions
        {
            get { return _columnDefinitions; }
            set { _columnDefinitions = value; }
        }

        private IEnumerable<IModelExtendedProperties> _items = default!;
        public IEnumerable<IModelExtendedProperties> Items
        {
            get { return GetItems(); }
            set { _items = value; }
        }

        private Dictionary<string, object> _dataSources = default!;
        public Dictionary<string, object> DataSources
        {
            get { return _dataSources; }
            set { _dataSources = value; }
        }
        
        private string _context = string.Empty;
        public string Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public ATableGridData() { }
        public virtual void OnItemsChanged(IEnumerable<IModelExtendedProperties> updatedItems)
        {
            _items = updatedItems.ToList();
        }
        private IEnumerable<IModelExtendedProperties> GetItems()
        {
            if (_items == null && _dataSources == null)
            {
                throw new Exception("Items list is null and no data sources were set for TableGrid data.");
            }

            var rawItems = _dataSources.First().Value;

            if (rawItems is IEnumerable<IModelExtendedProperties> items)
            {
                _totalRows = items.Count();
                _totalCols = _columnDefinitions.Count;

                return items;
            }
            else
            {
                throw new InvalidCastException("The provided object is not a valid collection of IViewModel<IBaseModel>.");
            }
        }

    }
}
    
 