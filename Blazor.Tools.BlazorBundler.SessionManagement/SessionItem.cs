/*====================================================================================================
    Class Name  : SessionItem
    Created By  : Solomio S. Sisante
    Created On  : July 30, 2024
    Purpose     : To handle session items.
  ====================================================================================================*/
using Blazor.Tools.BlazorBundler.Entities;
using Blazor.Tools.BlazorBundler.Interfaces;
using DocumentFormat.OpenXml.EMMA;
using System.Data;

namespace Blazor.Tools.BlazorBundler.SessionManagement
{
    public class SessionItem
    {
        public string Key { get; set; } = default!;
        public object? Value { get; set; }
        public Type Type { get; set; } = default!;
        public bool Serialize { get; set; }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to string type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator string?(SessionItem sessionItem)
        {
            return sessionItem.Value as string;
        }

        /// <summary>
        /// Implicit conversion from string to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(string value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(string),
                Serialize = false // or true depending on your needs
            };
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to int type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator int(SessionItem sessionItem)
        {
            return sessionItem.Value != null ? (int)sessionItem.Value : default;
        }

        /// <summary>
        /// Implicit conversion from int to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(int value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(int),
                Serialize = true
            };
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to bool type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator bool(SessionItem sessionItem)
        {
            return sessionItem.Value != null ? (bool)sessionItem.Value : default;
        }

        /// <summary>
        /// Implicit conversion from bool to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(bool value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(bool),
                Serialize = true
            };
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to DataRow[] type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator DataRow[]?(SessionItem sessionItem)
        {
            return sessionItem.Value as DataRow[];
        }

        /// <summary>
        /// Implicit conversion from DataRow[] to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(DataRow[] value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(DataRow[]),
                Serialize = true
            };
        }

        /// <summary>
        /// Set method for IEnumerable<IViewModel<TModel, IModelExtendedProperties>>
        /// </summary>
        /// <param name="value"></param>
        public void SetI<TModel>(IEnumerable<IViewModel<TModel, IModelExtendedProperties>> value)
            where TModel : IBase
        {
            Value = value.ToList(); // Store as List for internal compatibility
            Type = typeof(IEnumerable<IViewModel<TModel, IModelExtendedProperties>>);
            Serialize = true;
        }

        /// <summary>
        /// Get method for IEnumerable<IViewModel<TModel, IModelExtendedProperties>>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IViewModel<TModel, IModelExtendedProperties>> GetI<TModel>()
            where TModel : IBase
        {
            return Value as IEnumerable<IViewModel<TModel, IModelExtendedProperties>> ?? Enumerable.Empty<IViewModel<TModel, IModelExtendedProperties>>();
        }

        /// <summary>
        /// SetI sets the interface method for IEnumerable<DataRow>
        /// </summary>
        /// <param name="value"></param>
        public void SetI(IEnumerable<DataRow>? value)
        {
            Value = value; // Store as List for internal compatibility
            Type = typeof(IEnumerable<DataRow>);
            Serialize = true;
        }

        /// <summary>
        /// Get method for IEnumerable<DataRow>
        /// </summary>
        /// <returns>IEnumerable<DataRow>?</returns>
        public IEnumerable<DataRow>? GetI()
        {
            return Value as IEnumerable<DataRow>;
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to DataRow type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator DataRow?(SessionItem sessionItem)
        {
            return sessionItem.Value as DataRow;
        }

        /// <summary>
        /// Implicit conversion from DataRow to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(DataRow value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(DataRow),
                Serialize = true
            };
        }
        
        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to DataTable type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator DataTable?(SessionItem sessionItem)
        {
            return sessionItem.Value as DataTable;
        }

        /// <summary>
        /// Implicit conversion from DataTable to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(DataTable value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(DataTable),
                Serialize = true
            };
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to DataSet type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator DataSet?(SessionItem sessionItem)
        {
            return sessionItem.Value as DataSet;
        }

        /// <summary>
        /// Implicit conversion from DataSet to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(DataSet value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(DataSet),
                Serialize = true
            };
        }

        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to List<string> type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator List<string>?(SessionItem sessionItem)
        {
            return sessionItem.Value as List<string>;
        }

        /// <summary>
        /// Implicit conversion from List<string> to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(List<string> value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(List<string>),
                Serialize = true
            };
        }
        
        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to List<TargetTableColumn> type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator List<TargetTableColumn>?(SessionItem sessionItem)
        {
            return sessionItem.Value as List<TargetTableColumn>;
        }

        /// <summary>
        /// Implicit conversion from List<TargetTableColumn> to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(List<TargetTableColumn> value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(List<TargetTableColumn>),
                Serialize = true
            };
        }
        
        /// <summary>
        ///Implicit conversion operator for the Value of SessionItem to BBBrowserFile type 
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator BBBrowserFile?(SessionItem sessionItem)
        {
            return sessionItem.Value as BBBrowserFile;
        }

        /// <summary>
        /// Implicit conversion from BBBrowserFile to SessionItem
        /// </summary>
        /// <param name="sessionItem"></param>
        public static implicit operator SessionItem(BBBrowserFile value)
        {
            return new SessionItem
            {
                Value = value,
                Type = typeof(BBBrowserFile),
                Serialize = true
            };
        }

    }
}
