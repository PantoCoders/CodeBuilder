using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Panto.Framework.MVC
{
	internal sealed class ActionDescription : BaseDescription
	{
		public ControllerDescription PageController { get; set; } //为PageAction保留
		public MethodInfo MethodInfo { get; private set; }
		public ActionAttribute Attr { get; private set; }
		public ParameterInfo[] Parameters { get; private set; }
		public bool HasReturn { get; private set; }

		public ActionDescription(MethodInfo m, ActionAttribute atrr)
			: base(m)
		{
			this.MethodInfo = m;
			this.Attr = atrr;
			this.Parameters = m.GetParameters();
			this.HasReturn = m.ReturnType != ReflectionHelper.VoidType;
		}
	}


	internal sealed class RegexActionDescription
	{
		public Regex Regex { get; set; }

		public ActionDescription ActionDescription { get; set; }
	}

	internal sealed class MatchActionDescription
	{
		public Match Match { get; set; }

		public ActionDescription ActionDescription { get; set; }
	}


}
