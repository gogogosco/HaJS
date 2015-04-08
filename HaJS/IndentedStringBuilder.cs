/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaJS
{
    public class IndentedStringBuilder
    {
        public const int DefaultIdent = 3;
        public const string Indent = "    ";

        private StringBuilder sb;
        private int indent;
        private string indentCache;

        public IndentedStringBuilder()
        {
            sb = new StringBuilder();
            indent = DefaultIdent;
            RecacheIndent();
        }

        public void Enter()
        {
            AppendLine("{");
            indent++;
            RecacheIndent();
        }

        public void Leave()
        {
            indent--;
            RecacheIndent();
            AppendLine("}");
        }

        private void RecacheIndent()
        {
            indentCache = "";
            for (int i = 0; i < indent; i++)
            {
                indentCache += Indent;
            }
        }

        public void AppendLine(string data)
        {
            sb.AppendLine(indentCache + data);
        }

        public void Append(string data)
        {
            data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(x => AppendLine(x));
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
