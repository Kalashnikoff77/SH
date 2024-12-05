using Common.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace UI.Components.Shared
{
    public partial class ShowLongTextComponent
    {
        [Parameter, EditorRequired] public string Text { get; set; } = null!;
        [Parameter] public int MaxTextLength { get; set; } = 260;
        [Parameter] public EventCallback<int> MarkAsReadCallback { get; set; }
        [Parameter] public int? MarkAsReadId { get; set; }

        MarkupString htmlText = new MarkupString();
        StringBuilder formattedText = null!;
        bool isShortText = true;

        protected override void OnInitialized() => CheckText();

        async Task OnWrap()
        {
            isShortText = !isShortText;
            CheckText();
            if (MarkAsReadCallback.HasDelegate && MarkAsReadId.HasValue)
                await MarkAsReadCallback.InvokeAsync(MarkAsReadId.Value);
        }

        void CheckText()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                formattedText = new StringBuilder(MaxTextLength);
                if (Text.Length > MaxTextLength && isShortText)
                    htmlText = new MarkupString(formattedText.Clear().Append(Text.Substring(0, MaxTextLength)).Append("...").ToString());
                else
                    htmlText = Text.ReplaceNewLineWithBR();
            }
        }
    }
}
