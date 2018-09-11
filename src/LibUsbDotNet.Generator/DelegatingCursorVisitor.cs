using Core.Clang;
using System;

namespace LibUsbDotNet.Generator
{
    internal class DelegatingCursorVisitor : CursorVisitor
    {
        private readonly Func<Cursor, Cursor, ChildVisitResult> visitor;

        public DelegatingCursorVisitor(Func<Cursor, Cursor, ChildVisitResult> visitor)
        {
            this.visitor = visitor;
        }

        protected override ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            return visitor(cursor, parent);
        }
    }
}
