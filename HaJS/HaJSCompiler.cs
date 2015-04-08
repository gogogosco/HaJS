/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HaJS
{
    public class HaJSCompiler
    {
        private Dictionary<string, HaJSFeature> features = new Dictionary<string,HaJSFeature>();
        private Dictionary<string, string> resources = new Dictionary<string, string>();
        private HashSet<string> deps = new HashSet<string>();
        private List<MessageBaseElement> messages = new List<MessageBaseElement>();
        Dictionary<HashSet<int>, IndentedStringBuilder> sbl = new Dictionary<HashSet<int>, IndentedStringBuilder>();
        List<HashSet<int>> contexts = new List<HashSet<int>>();

        public HaJSCompiler(string xmlPath)
        {
            ParseConfig(GetMainElementFromFile(xmlPath));
        }

        public HaJSCompiler(XmlElement rootConfigElement)
        {
            ParseConfig(rootConfigElement);
        }

        public int RegisterMessage(MessageBaseElement message)
        {
            messages.Add(message);
            return messages.Count - 1;
        }

        private XmlElement GetMainElementFromFile(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            foreach (XmlNode node in doc.ChildNodes)
            {
                if (node is XmlElement)
                {
                    return (XmlElement)node;
                }
            }
            throw new Exception("File has no main XML Element (" + file + ")");
        }

        private void ParseConfig(XmlElement el)
        {
            foreach (XmlElement fe in el.GetElementsByTagName("featureDefinition"))
            {
                string dep = fe.HasAttribute("dependsOn") ? fe.GetAttribute("dependsOn") : null;
                if (fe.HasAttribute("js"))
                {
                    features.Add(fe.GetAttribute("name"), new HaJSFeature(fe.GetAttribute("js"), dep));
                }
                else
                {
                    features.Add(fe.GetAttribute("name"), new HaJSSwitchFeature(fe.GetAttribute("right"), dep, fe.GetAttribute("left")));
                }
            }
        }

        public HaJSFeature GetFeature(string name)
        {
            return features[name];
        }

        public bool HasFeature(string name)
        {
            return features.ContainsKey(name);
        }

        public void Compile(string inPath, string outPath)
        {
            File.WriteAllText(outPath, CompileInternal(GetMainElementFromFile(inPath)));
        }

        public string Compile(XmlElement rootElement)
        {
            return CompileInternal(rootElement);
        }

        public HashSet<string> Dependencies
        {
            get { return deps; }
        }

        private string ResolveResource(string name)
        {
            if (resources.ContainsKey(name))
                return resources[name];
            else
                return null;
        }

        private string TryResolveResource(string name)
        {
            if (resources.ContainsKey(name))
                return resources[name];
            else
                return name;
        }

        private string GetTextFromElement(XmlElement element)
        {
            return element.HasAttribute("text") ? element.GetAttribute("text") : ResolveResource(element.GetAttribute("rsrc"));
        }

        private string GetFirstTag(string x)
        {
            int idx = x.IndexOf(">") + 1;
            if (idx >= x.Length)
                return x;
            else
                return x.Remove(idx);
        }

        private void BuildElementRecursive(XmlElement element, ref List<HaJSElement> targetList)
        {
            HaJSElement result;
            List<HaJSElement> elements;
            switch (element.Name)
            {
                case "switch":
                    SwitchElement se = new SwitchElement((HaJSSwitchFeature)GetFeature("switch_" + element.GetAttribute("type")));
                    elements = se.Children;
                    foreach (XmlElement caseNode in element.ChildNodes)
                    {
                        BuildElementRecursive(caseNode, ref elements);
                    }
                    result = se;
                    break;
                case "case":
                    CaseElement ce;
                    if (element.HasAttribute("cond"))
                    {
                        ce = new CaseElement(false, true, element.GetAttribute("cond"));
                    }
                    else
                    {
                        ce = new CaseElement(false, false, element.GetAttribute("val"));
                    }
                    elements = ce.Children;
                    foreach (XmlElement subnode in element.ChildNodes)
                    {
                        BuildElementRecursive(subnode, ref elements);
                    }
                    result = ce;
                    break;
                case "default":
                    CaseElement def = new CaseElement(true, false, null);
                    elements = def.Children;
                    foreach (XmlElement subnode in element.ChildNodes)
                    {
                        BuildElementRecursive(subnode, ref elements);
                    }
                    result = def;
                    break;
                case "message":
                    string msgText = GetTextFromElement(element);
                    switch (element.GetAttribute("style"))
                    {
                        case "n":
                            result = new NextMessageElement(this, msgText);
                            break;
                        case "pn":
                            result = new PrevNextMessageElement(this, msgText);
                            break;
                        case "yn":
                            string no = TryResolveResource(element.GetAttribute("no"));
                            result = new YesNoMessageElement(this, msgText, no);
                            break;
                        case "ok":
                            result = new OkMessageElement(this, msgText);
                            break;
                        default:
                            throw new ArgumentException("Unknown message type \"" + element.GetAttribute("style") + "\" in the element \"" + element.OuterXml + "\"");
                    }
                    break;
                case "options":
                    OptionsMessageElement ome = new OptionsMessageElement(this, GetTextFromElement(element));
                    elements = ome.Children;
                    foreach (XmlElement subnode in element.ChildNodes)
                    {
                        BuildElementRecursive(subnode, ref elements);
                    }
                    result = ome;
                    break;
                case "option":
                    OptionElement oe = new OptionElement(GetTextFromElement(element));
                    elements = oe.Children;
                    foreach (XmlElement subnode in element.ChildNodes)
                    {
                        BuildElementRecursive(subnode, ref elements);
                    }
                    result = oe;
                    break;
                case "ifdef":
                case "ifndef":
                    string condition = element.GetAttribute("name");
                    if ((element.Name == "ifdef" && features.ContainsKey(condition)) || (element.Name == "ifndef" && !features.ContainsKey(condition)))
                    {
                        foreach (XmlElement subnode in element.ChildNodes)
                        {
                            BuildElementRecursive(subnode, ref targetList);
                        }
                    }
                    return;
                case "assert":
                    string type = element.GetAttribute("type");
                    string fail = TryResolveResource(element.GetAttribute("onFail"));
                    string cond = element.GetAttribute("cond");
                    SwitchElement fakeSwitch = new SwitchElement((HaJSSwitchFeature)GetFeature("switch_" + type));
                    CaseElement fakeCase = new CaseElement(false, true, cond);
                    CaseElement fakeDef = new CaseElement(true, false, null);
                    fakeSwitch.Children.Add(fakeCase);
                    fakeSwitch.Children.Add(fakeDef);
                    OkMessageElement fakeOk = new OkMessageElement(this, fail);
                    fakeDef.Children.Add(fakeOk);
                    targetList.Add(fakeSwitch);
                    targetList = fakeCase.Children;
                    return;
                default:
                    if (features.ContainsKey(element.Name))
                    {
                        result = new CommandElement(GetFeature(element.Name),
                            element.HasAttribute("x") ? element.GetAttribute("x") : null,
                            element.HasAttribute("y") ? element.GetAttribute("y") : null,
                            element.HasAttribute("z") ? element.GetAttribute("z") : null);
                        break;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown tag type \"" + element.Name + "\" in the element \"" + GetFirstTag(element.OuterXml) + "\"");
                    }
            }
            targetList.Add(result);
        }

        private bool CompareHashSets(HashSet<int> a, HashSet<int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            foreach (int x in a)
            {
                if (!b.Contains(x))
                {
                    return false;
                }
            }
            return true;
        }

        private IndentedStringBuilder GetStringBuilderByContext(HashSet<int> context)
        {
            foreach (HashSet<int> existingContext in contexts)
            {
                if (CompareHashSets(existingContext, context))
                {
                    return sbl[existingContext];
                }
            }
            contexts.Add(context);
            sbl[context] = new IndentedStringBuilder();
            return sbl[context];
        }

        private void WriteStatusChange(IndentedStringBuilder sb, int newStatus)
        {
            sb.AppendLine("status = " + newStatus.ToString() + ";");
        }

        private void CompileJSRecursive(HaJSElement root, HaJSElement parent)
        {
            IndentedStringBuilder sb = GetStringBuilderByContext(root.statusContext);
            if (root is SwitchElement)
            {
                SwitchElement se = (SwitchElement)root;
                for (int i = 0; i < se.Children.Count; i++)
                {
                    CaseElement ce = (CaseElement)se.Children[i];
                    sb.AppendLine(ce.Compile(this, se.Type, i == 0));
                    sb.Enter();
                    CompileJSRecursive(ce, root);
                    sb.Leave();
                }
            }
            else if (root.HasChildren && !root.Parallel)
            {
                root.Children.ForEach(x => CompileJSRecursive(x, root));
            }
            else if (root is CommandElement)
            {
                CommandElement ce = (CommandElement)root;
                sb.AppendLine(ce.Compile(this));
            }
            else if (root is MessageBaseElement)
            {
                MessageBaseElement mbe = (MessageBaseElement)root;
                if (mbe.ControlFlowBreaker)
                    WriteStatusChange(sb, mbe.NextStatus);
                sb.AppendLine(mbe.Compile(this));
                if (mbe is OkMessageElement)
                {
                    sb.AppendLine(GetFeature("special_End").Compile(this));
                }
                sb.AppendLine("return;");
                string postCompileData = mbe.PostCompile(this, parent);
                if (root is OptionsMessageElement)
                {
                    // Special case handled here because OptionMessages are complicated
                    OptionsMessageElement ome = (OptionsMessageElement)root;
                    sb = GetStringBuilderByContext(new HashSet<int>() { ome.NextStatus });
                    for (int i = 0; i < ome.Children.Count; i++)
                    {
                        OptionElement oe = (OptionElement)ome.Children[i];
                        sb.AppendLine( (i == 0 ? "if" : "else if") + " (selection == " + i.ToString() + ")");
                        sb.Enter();
                        CompileJSRecursive(oe, root);
                        sb.Leave();
                    }
                }
                else if (postCompileData != null)
                {
                    GetStringBuilderByContext(new HashSet<int>() { mbe.NextStatus }).Append(postCompileData);
                }
            }
        }

        private void AssignStatusContext(HaJSElement parent, HashSet<int> currContext)
        {
            // Set this element statusContext
            currContext.ToList().ForEach(x => parent.statusContext.Add(x));

            // If we are a context switcher, update context now (in cases like OptionsMessageElement children need to be cast at the final context)
            if (parent.ControlFlowBreaker)
            {
                currContext.Clear();
                currContext.Add(((MessageBaseElement)parent).NextStatus);
            }
            
            // Handle children (forking)
            if (parent.HasChildren)
            {
                if (parent.Parallel) // Parallel fork
                {
                    HashSet<int> resultContext = new HashSet<int>();
                    foreach (HaJSElement element in parent.Children)
                    {
                        HashSet<int> elementExitContext = new HashSet<int>(currContext);
                        AssignStatusContext(element, elementExitContext);
                        elementExitContext.ToList().ForEach(x => resultContext.Add(x));
                    }
                    currContext.Clear();
                    resultContext.ToList().ForEach(x => currContext.Add(x));
                }
                else // Serial fork
                {
                    parent.Children.ForEach(x => AssignStatusContext(x, currContext));
                }
            }
        }

        private string CompileInternal(XmlElement element)
        {
            deps.Clear();
            bool hasRsrc = element.GetElementsByTagName("resources").Count > 0;
            if (hasRsrc)
            {
                foreach (XmlElement rsrcElement in element.GetElementsByTagName("resources")[0])
                {
                    resources.Add(rsrcElement.GetAttribute("name"), rsrcElement.GetAttribute("text"));
                }
            }

            List<HaJSElement> mainList = new List<HaJSElement>();
            BuildElementRecursive((XmlElement)element.ChildNodes[hasRsrc ? 1 : 0], ref mainList);
            HaJSElement mainElement = mainList[0];
            HashSet<int> startingContext = new HashSet<int>();
            startingContext.Add(-1);
            AssignStatusContext(mainElement, startingContext);
            CompileJSRecursive(mainElement, null);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/* This script was automatically generated by HaJS on " + DateTime.Today.ToShortDateString() + " */");
            foreach (string dep in deps)
            {
                sb.AppendLine("importPackage(" + dep + ");");
            }
            sb.AppendLine("var status = 0;");
            sb.AppendLine("function start() {");
            sb.AppendLine("    status = -1;");
	        sb.AppendLine("    action(1, 0, 0);");
            sb.AppendLine("}");
            sb.AppendLine("function action(mode, type, selection) {");
            sb.AppendLine("    if (mode == -1) {");
            sb.AppendLine("        cm.dispose();");
            sb.AppendLine("    } else {");
            foreach (HashSet<int> context in contexts)
            {
                sb.AppendLine("        if (" + context.ToList().Select(x => "status == " + x.ToString()).Aggregate((x, y) => x + " || " + y) + ")");
                sb.AppendLine("        {");
                sb.Append(sbl[context].ToString());
                sb.AppendLine("        }");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
	        return sb.ToString();
        }
    }
}
