// SPDX-FileCopyrightText: Copyright (c) Quamotion
// SPDX-License-Identifier: LGPL-2.0-or-later

using Core.Clang;

namespace LibUsbDotNet.Generator
{
    internal sealed class ForwardDeclarationVisitor : CursorVisitor
    {
        private readonly Cursor beginningCursor;
        private readonly bool skipSystemHeaderCheck;
        private bool beginningCursorReached;

        public ForwardDeclarationVisitor(Cursor beginningCursor, bool skipSystemHeaderCheck = false)
        {
            this.beginningCursor = beginningCursor;
            this.skipSystemHeaderCheck = skipSystemHeaderCheck;
        }

        public Cursor ForwardDeclarationCursor { get; private set; }

        protected override ChildVisitResult Visit(Cursor cursor, Cursor parent)
        {
            if (!this.skipSystemHeaderCheck && cursor.IsInSystemHeader())
            {
                return ChildVisitResult.Continue;
            }

            if (cursor.Equals(this.beginningCursor))
            {
                this.beginningCursorReached = true;
                return ChildVisitResult.Continue;
            }

            if (this.beginningCursorReached)
            {
                this.ForwardDeclarationCursor = cursor;
                return ChildVisitResult.Break;
            }

            return ChildVisitResult.Recurse;
        }
    }
}
