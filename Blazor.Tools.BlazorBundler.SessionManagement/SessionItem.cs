﻿namespace Blazor.Tools.BlazorBundler.SessionManagement
{
    public class SessionItem
    {
        public string Key { get; set; } = default!;
        public object? Value { get; set; }
        public Type Type { get; set; } = default!;
        public bool Serialize { get; set; }
    }
}