﻿namespace Blazor.Tools.BlazorBundler.Entities.SampleObjects.Models
{
    public class DataSource<T>
    {
        public string DataSourceName { get; set; } = default!;
        public List<T> DataSourceList { get; set; } = default!;
    }
}
