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
    public abstract class HaJSElement
    {
        protected List<HaJSElement> children = new List<HaJSElement>();
        public HashSet<int> statusContext = new HashSet<int>();

        public HaJSElement()
        {
        }

        public abstract bool HasChildren { get; }
        public abstract bool Parallel { get; }
        public abstract bool ControlFlowBreaker { get; }

        public virtual List<HaJSElement> Children
        {
            get { return children; }
        }
    }

    public class SwitchElement : HaJSElement
    {
        private HaJSSwitchFeature type;

        public SwitchElement(HaJSSwitchFeature type)
        {
            this.type = type;
        }

        public HaJSSwitchFeature Type { get { return type; } }

        public override bool HasChildren
        {
            get { return true; }
        }

        public override bool Parallel
        {
            get { return true; }
        }

        public override bool ControlFlowBreaker
        {
            get { return false; }
        }
    }

    public class CaseElement : HaJSElement
    {
        private string value;
        private bool arbitrary;
        private bool defCase;

        public CaseElement(bool defCase, bool arbitrary, string value)
        {
            this.arbitrary = arbitrary;
            this.value = value;
            this.defCase = defCase;
        }

        public string Compile(HaJSCompiler compiler, HaJSSwitchFeature feature, bool first)
        {
            if (defCase)
                return "else";
            StringBuilder sb = new StringBuilder();
            sb.Append(first ? "if (" : "else if (");
            if (arbitrary)
            {
                sb.Append(value.Replace("$x", feature.GetLeft(compiler)));
            }
            else
            {
                string[] rvalues = value.Split(",".ToCharArray());
                sb.Append(rvalues.Select(x => feature.Compile(compiler, x)).Aggregate((x, y) => x + " || " + y));
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override bool HasChildren
        {
            get { return true; }
        }

        public override bool Parallel
        {
            get { return false; }
        }

        public override bool ControlFlowBreaker
        {
            get { return false; }
        }
    }

    public abstract class MessageBaseElement : HaJSElement
    {
        public string text;
        int status;

        public MessageBaseElement(HaJSCompiler compiler, string text)
        {
            status = compiler.RegisterMessage(this);
            this.text = text;
        }

        public int NextStatus
        {
            get { return status; }
        }

        public abstract string Compile(HaJSCompiler compiler);
        public virtual string PostCompile(HaJSCompiler compiler, HaJSElement parent) { return null; }

        protected string Stringify(string x)
        {
            return "\"" + x + "\"";
        }

        public override bool HasChildren
        {
            get { return false; }
        }

        public override bool Parallel
        {
            get { return false; }
        }

        public override bool ControlFlowBreaker
        {
            get { return true; }
        }
    }

    public class NextMessageElement : MessageBaseElement
    {
        public NextMessageElement(HaJSCompiler compiler, string text)
            : base(compiler, text)
        {
        }

        public override string Compile(HaJSCompiler compiler)
        {
            return compiler.GetFeature("dlg_Next").Compile(compiler, Stringify(text));
        }
    }

    public class PrevNextMessageElement : MessageBaseElement
    {
        public PrevNextMessageElement(HaJSCompiler compiler, string text)
            : base(compiler, text)
        {
        }

        public override string Compile(HaJSCompiler compiler)
        {
            return compiler.GetFeature("dlg_PrevNext").Compile(compiler, Stringify(text));
        }

        private int GetPreviousStatus(HaJSElement parent)
        {
            MessageBaseElement prev = null;
            foreach (HaJSElement child in parent.Children)
            {
                if (!(child is MessageBaseElement))
                    continue;
                if (child == this)
                {
                    return prev == null ? -1 : prev.statusContext.ElementAt(0);
                }
                else
                {
                    prev = (MessageBaseElement)child;
                }
            }
            throw new ArgumentException("Internal error - element is not a child of the parent supplied");
        }

        public override string PostCompile(HaJSCompiler compiler, HaJSElement parent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("if (mode == 0)");
            sb.AppendLine("{");
            sb.AppendLine(IndentedStringBuilder.Indent + "status = " + GetPreviousStatus(parent).ToString());
            sb.AppendLine(IndentedStringBuilder.Indent + "action(1,0,0);");
            sb.AppendLine(IndentedStringBuilder.Indent + "return;");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    public class OkMessageElement : MessageBaseElement
    {
        public OkMessageElement(HaJSCompiler compiler, string text)
            : base(compiler, text)
        {
        }

        public override string Compile(HaJSCompiler compiler)
        {
            return compiler.GetFeature("dlg_Ok").Compile(compiler, Stringify(text));
        }
    }

    public class YesNoMessageElement : MessageBaseElement
    {
        public string noText;

        public YesNoMessageElement(HaJSCompiler compiler, string text, string noText)
            : base(compiler, text)
        {
            this.noText = noText;
        }

        public override string Compile(HaJSCompiler compiler)
        {
            return compiler.GetFeature("dlg_YesNo").Compile(compiler, Stringify(text));
        }

        public override string PostCompile(HaJSCompiler compiler, HaJSElement parent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("if (mode == 0)");
            sb.AppendLine("{");
            sb.AppendLine(IndentedStringBuilder.Indent + compiler.GetFeature("dlg_Ok").Compile(compiler, Stringify(noText)));
            sb.AppendLine(IndentedStringBuilder.Indent + compiler.GetFeature("special_End").Compile(compiler));
            sb.AppendLine(IndentedStringBuilder.Indent + "return;");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    public class OptionsMessageElement : MessageBaseElement
    {
        public OptionsMessageElement(HaJSCompiler compiler, string text)
            : base(compiler, text)
        {
        }

        public override bool HasChildren
        {
            get
            {
                return true;
            }
        }

        public override bool Parallel
        {
            get
            {
                return true;
            }
        }

        public override string Compile(HaJSCompiler compiler)
        {
            string fullText = text + "#b";
            for (int i = 0; i < Children.Count; i++)
            {
                OptionElement oe = (OptionElement)Children[i];
                fullText += "\\r\\n#L" + i.ToString() + "#" + oe.Text + "#l";
            }
            fullText += "#k";
            return compiler.GetFeature("dlg_Options").Compile(compiler, Stringify(fullText));
        }

        // We do not implement PostCompile becuase this is a special case, and will be handled in the main compiler
    }

    public class OptionElement : HaJSElement
    {
        string text;

        public OptionElement(string text)
        {
            this.text = text;
        }

        public string Text { get { return text; } }

        public override bool HasChildren
        {
            get { return true; }
        }

        public override bool Parallel
        {
            get { return false; }
        }

        public override bool ControlFlowBreaker
        {
            get { return false; }
        }
    }

    public class CommandElement : HaJSElement
    {
        private HaJSFeature feature;
        private List<string> args;

        public CommandElement(HaJSFeature feature)
        {
            this.feature = feature;
            this.args = new List<string>();
        }

        public CommandElement(HaJSFeature feature, string arg1)
        {
            this.feature = feature;
            this.args = new List<string>() { arg1 };
        }

        public CommandElement(HaJSFeature feature, string arg1, string arg2)
        {
            this.feature = feature;
            this.args = new List<string>() { arg1, arg2 };
        }

        public CommandElement(HaJSFeature feature, string arg1, string arg2, string arg3)
        {
            this.feature = feature;
            this.args = new List<string>() { arg1, arg2, arg3 };
        }

        public CommandElement(HaJSFeature feature, List<string> args)
        {
            this.feature = feature;
            this.args = args;
        }

        public string Compile(HaJSCompiler compiler)
        {
            return feature.Compile(compiler, args.ElementAtOrDefault(0), args.ElementAtOrDefault(1), args.ElementAtOrDefault(2));
        }

        public override bool HasChildren
        {
            get { return false; }
        }

        public override bool Parallel
        {
            get { return false; }
        }

        public override bool ControlFlowBreaker
        {
            get { return false; }
        }
    }
}
