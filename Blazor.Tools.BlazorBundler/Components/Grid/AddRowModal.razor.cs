//using BlazorBootstrap;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Rendering;
//using System.Data;

//namespace Blazor.Tools.BlazorBundler.Components.Grid
//{
//    public partial class AddRowModal<TModel, TIModel, TModelVM> : ComponentBase
//    {
//        [Parameter] public bool ShowAddRowModal { get; set; }
//        [Parameter] public TIModel IModel { get; set; } = default!;
//        [Parameter] public TModelVM ModelVM { get; set; } = default!;
//        [Parameter] public EventCallback OnClose { get; set; }
//        [Parameter] public EventCallback OnSave { get; set; }
//        [Parameter] public List<string> HiddenColumnNames { get; set; } = default!;

//        protected override void OnParametersSet()
//        {
//            Console.WriteLine("AddRowModal OnParametersSet");
//            base.OnParametersSet();
//        }
//        private void UpdateNewRowData(DataColumn column, string value)
//        {
//            //NewRowData[column.ColumnName] = Convert.ChangeType(value, column.DataType);
//        }

//        protected override void BuildRenderTree(RenderTreeBuilder builder)
//        {
//            int sequence = 0;

//            // Modal container
//            builder.OpenElement(sequence++, "div");
//            builder.AddAttribute(sequence++, "class", $"data-table-grid-modal {(ShowAddRowModal ? "show" : "")}");

//            // Buttons
//            builder.OpenElement(sequence++, "div");
//            builder.AddAttribute(sequence++, "class", "data-table-grid-modal-close-button");

//            builder.OpenComponent<Icon>(sequence++);
//            builder.AddAttribute(sequence++, "Name", "IconName.CheckCircleFill");
//            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, async () => await OnSave.InvokeAsync()));
//            builder.AddAttribute(sequence++, "title", "Add");
//            builder.CloseComponent();

//            builder.OpenComponent<Icon>(sequence++);
//            builder.AddAttribute(sequence++, "Name", "IconName.XCircleFill");
//            builder.AddAttribute(sequence++, "title", "Close");
//            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create(this, async () => await OnClose.InvokeAsync()));
//            builder.CloseComponent();

//            builder.CloseElement(); // Close buttons div

//            // Modal content
//            builder.OpenElement(sequence++, "div");
//            builder.AddAttribute(sequence++, "class", "data-table-grid-modal-content");

//            var properties = IModel?.GetType().GetPropertyNames();
//            if (properties != null)
//            {
//                foreach (var property in properties)
//                {
//                    var isHidden = HiddenColumnNames?.Contains(property.Name) ?? false;
//                    if (!isHidden)
//                    {
//                        builder.OpenElement(sequence++, "div");
//                        builder.AddAttribute(sequence++, "class", "form-group");

//                        builder.OpenElement(sequence++, "label");
//                        builder.AddContent(sequence++, property.Name);
//                        builder.CloseElement();

//                        builder.OpenElement(sequence++, "input");
//                        builder.AddAttribute(sequence++, "value", NewRowData[column.ColumnName]?.ToString() ?? string.Empty);
//                        builder.AddAttribute(sequence++, "oninput", EventCallback.Factory.Create(this, (ChangeEventArgs e) => UpdateNewRowData(column, e?.Value?.ToString() ?? string.Empty)));
//                        builder.AddAttribute(sequence++, "class", "form-control");
//                        builder.CloseElement();

//                        builder.CloseElement(); // Close form-group div
//                    }
//                }
//            }

//            builder.CloseElement(); // Close modal content div
//            builder.CloseElement(); // Close modal container div
//        }
//    }
//}

