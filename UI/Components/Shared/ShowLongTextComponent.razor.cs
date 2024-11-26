using Common.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace UI.Components.Shared
{
    public partial class ShowLongTextComponent
    {
        [Parameter, EditorRequired] public string Text { get; set; } = null!;
        [Parameter] public bool IsShortText { get; set; } = false;
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }

        MarkupString htmlText = new MarkupString();

        int length = 260;

        protected override void OnParametersSet()
        {
            var formattedText = new StringBuilder(260);

            if (IsShortText)
            {
                if (Text.Length > length)
                    htmlText = new MarkupString(formattedText.Append(Text.Substring(0, length)).Append("...").ToString());
                else
                    htmlText = Text.ReplaceNewLineWithBR();
            }
            else
                htmlText = Text.ReplaceNewLineWithBR();
        }
    }
}
