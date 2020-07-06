using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM
{
    interface ISavedJsonData
    {
        public void Save();
        public static ISavedJsonData Load() { return null; }
    }
}
