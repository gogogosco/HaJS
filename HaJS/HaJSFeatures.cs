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
    public class HaJSFeature
    {
        protected string val;
        protected string dep;

        public HaJSFeature(string jsValue, string dependency)
        {
            this.val = jsValue;
            this.dep = dependency;
        }

        public virtual string Compile(HaJSCompiler compiler, string x = null, string y = null, string z = null)
        {
            if (dep != null)
            {
                compiler.Dependencies.Add(dep);
            }
            string result = val;
            if (val.Contains("$."))
            {
                result = result.Replace("$.", compiler.manager + ".");
            }
            if (val.Contains("$x"))
            {
                result = result.Replace("$x", x);
            }
            if (val.Contains("$y"))
            {
                result = result.Replace("$y", y);
            }
            if (val.Contains("$z"))
            {
                result = result.Replace("$z", z);
            }
            return result;
        }
        public bool HasDependency { get { return dep != null; } }
        public string Dependency { get { return dep; } }
    }

    public class HaJSSwitchFeature : HaJSFeature
    {
        protected string left;

        public HaJSSwitchFeature(string rvalue, string dep, string lvalue)
            : base(rvalue, dep)
        {
            this.left = lvalue;
        }

        public override string Compile(HaJSCompiler compiler, string x = null, string y = null, string z = null)
        {
            string rval = base.Compile(compiler, x, y, z);
            string lval = left;
            if (left.Contains("$."))
            {
                lval = lval.Replace("$.", compiler.manager + ".");
            }
            return lval + " == " + rval;
        }

        public string GetLeft(HaJSCompiler compiler)
        {
            if (dep != null)
            {
                compiler.Dependencies.Add(dep);
            }
            string lval = left;
            if (left.Contains("$."))
            {
                lval = lval.Replace("$.", compiler.manager + ".");
            }
            return lval;
        }
    }
}
