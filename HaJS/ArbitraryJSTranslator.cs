using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaJS
{
    public static class ArbitraryJSTranslator
    {
        public static string Translate(HaJSCompiler compiler, string x)
        {
            int i = x.IndexOf("$");
            while (i != -1)
            {
                int callOpen = x.IndexOf("(", i);
                if (callOpen != -1)
                {
                    int callClose = x.IndexOf(")", callOpen);
                    if (callClose != -1)
                    {
                        string feature = x.Substring(i + 1, callOpen - (i + 1));
                        string[] args = x.Substring(callOpen + 1, callClose - (callOpen + 1)).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (compiler.HasFeature(feature))
                        {
                            HaJSFeature f = compiler.GetFeature(feature);
                            string replacement = f.Compile(compiler, args.ElementAtOrDefault(0), args.ElementAtOrDefault(1), args.ElementAtOrDefault(2));
                            x = x.Remove(i, callClose + 1 - i); // Remove old string
                            x = x.Insert(i, replacement); // Insert our replacement
                            i += replacement.Length;
                        }
                    }
                }

                if (i + 1 >= x.Length)
                    break;
                i = x.IndexOf("$", i + 1);
            }
            return x;
        }
    }
}
