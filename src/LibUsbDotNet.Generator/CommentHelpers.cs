using Core.Clang.Documentation.Doxygen;
using System.Collections.Generic;
using System.Text;

namespace LibUsbDotNet.Generator
{
    internal static class CommentHelpers
    {
        public static string GetCommentInnerText(this Comment comment)
        {
            if (comment == null) return null;
            var builder = new StringBuilder();
            AppendCommentInnerText(comment, builder);
            return builder.ToString().Trim();
        }

        public static void AppendSpaceIfNotAtStartOfLine(this StringBuilder builder)
        {
            if (builder == null || builder.Length == 0) return;
            if (builder[^1] != '\n') builder.Append(' ');
        }

        public static void AppendCommentInnerText(this Comment comment, StringBuilder builder)
        {
            string text = null;
            switch (comment)
            {
                case null:
                    return;
                case TextComment textComment:
                    text = textComment.GetText();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        builder.AppendSpaceIfNotAtStartOfLine();
                        builder.AppendLine(text.Trim());
                    }
                    break;
                case VerbatimLineComment verbatimLineComment when verbatimLineComment.GetCommandName() == "ref":
                    text = verbatimLineComment.GetText();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        builder.AppendSpaceIfNotAtStartOfLine();
                        builder.Append(text.Trim());
                    }
                    break;
                default:
                    // Recurse
                    var childCount = comment.GetNumChildren();
                    for (uint i = 0; i < childCount; i++)
                    {
                        var child = comment.GetChild(i);
                        AppendCommentInnerText(child, builder);
                    }
                    break;
            }
        }

        public static IEnumerable<Comment> GetCommentChildren(this FullComment fullComment)
        {
            uint fullCommentChildren = fullComment?.GetNumChildren() ?? 0;
            for (uint i = 0;i < fullCommentChildren; i++)
            {
                yield return fullComment.GetChild(i);
            }
        }
    }
}
