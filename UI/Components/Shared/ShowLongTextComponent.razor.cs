using Common.Extensions;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace UI.Components.Shared
{
    public partial class ShowLongTextComponent
    {
        [Parameter, EditorRequired] public string Text { get; set; } = null!;

        int maxTextLength = 260;
        MarkupString htmlText = new MarkupString();
        StringBuilder formattedText = new StringBuilder(260);
        bool isShortText = true;

        protected override void OnParametersSet() =>
            CheckText();

        void OnWrap()
        {
            isShortText = !isShortText;
            CheckText();
        }

        void CheckText()
        {
            if (!string.IsNullOrEmpty(Text))
            {
                if (Text.Length > maxTextLength && isShortText)
                    htmlText = new MarkupString(formattedText.Clear().Append(Text.Substring(0, maxTextLength)).Append("...").ToString());
                else
                    htmlText = Text.ReplaceNewLineWithBR();
            }
        }
    }
}
