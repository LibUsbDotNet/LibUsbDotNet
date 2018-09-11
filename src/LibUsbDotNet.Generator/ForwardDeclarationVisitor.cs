// <copyright file="ForwardDeclarationVisitor.cs" company="Quamotion">
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

namespace LibUsbDotNet.Generator
{
    using System;
    using Core.Clang;

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
