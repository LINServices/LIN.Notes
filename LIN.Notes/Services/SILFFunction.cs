using SILF.Script.Elements.Functions;
using SILF.Script.Elements;
using SILF.Script.Interfaces;
using SILF.Script.Runtime;
using SILF.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIN.Notes.Services;

public class SILFFunction(Action<List<SILF.Script.Elements.ParameterValue>> action) : IFunction
{

    public Tipo? Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Parameter> Parameters { get; set; } = [];
    public Context Context { get; set; } = null!;

    readonly Action<List<SILF.Script.Elements.ParameterValue>> Action = action;

    public FuncContext Run(Instance instance, List<SILF.Script.Elements.ParameterValue> values, ObjectContext @object)
    {
        Action.Invoke(values);
        return new();
    }

}
