using BasicUtilities;
using Ontology.GraphInduction.Model;
using System.Text;
using WebAppUtilities;

namespace Ontology.GraphInduction.Utils
{
	public class FormatUtil : JsonWs
	{
		public static bool IncludeValues = false;
		public static bool ShortForm = true;


		static public string FormatPrototype2(Prototype prototype)
		{
			return FormatPrototype(prototype, 0).ToString();
		}


		static public StringBuilder FormatPrototype(Prototype prototype, int iLevels = 0)
		{
			StringBuilder sb = new StringBuilder();
			if (null == prototype)
			{
				sb.Append("null");
				return sb;
			}

			if (ShortForm)
			{
				try
				{
					if (prototype.TypeOf("CSharp.Statement"))
					{
						if (IncludeValues)
							sb.Append("(").Append(prototype.Value).Append(") ");

						string strStatement = CSharp.Parsers.SimpleGenerator.Generate(Prototypes.FromPrototype(prototype) as CSharp.Statement);
						sb.Append(strStatement.Replace("\r\n", "\r\n" + new string('\t', iLevels)));

						return sb;
					}
					else if (prototype.TypeOf("CSharp.Expression"))
					{
						string strExpression = CSharp.Parsers.SimpleGenerator.Generate(Prototypes.FromPrototype(prototype) as CSharp.Expression);
						if (!StringUtil.IsEmpty(strExpression)) //For literals and non-serializable expression return the prototype
						{
							strExpression.Replace("\r\n", "\r\n" + new string('\t', iLevels));
							sb.Append(strExpression);
							return sb;
						}
					}

					//if (Prototypes.TypeOf(prototype, new Prototype("SQL.Statement")))
					//{
					//	sb.Append(SQL.Parsers.SimpleGenerator.Generate(Prototypes.FromPrototype(prototype) as SQL.Statement));
					//	return sb;
					//}
				}
				catch
				{

				}
			}

			//sb.Append(new string('\t', iLevels));
			sb.Append(prototype.PrototypeName).Append(" (").Append(prototype.PrototypeID).Append(")");
			iLevels++;
			foreach (var pair in prototype.Properties)
			{
				Prototype protoName = Prototypes.GetPrototype(pair.Key);


				sb.AppendLine();
				string strName = null;

				if (protoName.PrototypeName.StartsWith(prototype.PrototypeName))
				{
					if (StringUtil.InString(protoName.PrototypeName, ".Field."))
					{
						strName = "." + StringUtil.RightOfFirst(protoName.PrototypeName, prototype.PrototypeName + ".Field.");
					}
					else if (StringUtil.InString(protoName.PrototypeName, ".Property."))
					{
						strName = "." + StringUtil.RightOfFirst(protoName.PrototypeName, prototype.PrototypeName + ".Property.");
					}
					else
					{
						strName = protoName.PrototypeName;
					}
				}
				else
				{
					strName = protoName.PrototypeName;
				}

				sb.Append(new string('\t', iLevels));
				sb.Append(strName).Append(" = ");
				sb.Append(FormatPrototype(pair.Value, iLevels));
			}

			for (int i = 0; i < prototype.Children.Count; i++)
			{
				sb.AppendLine();
				sb.Append(new string('\t', iLevels));
				sb.Append("[").Append(i).Append("] = ");
				sb.Append(FormatPrototype(prototype.Children[i], iLevels));
			}


			iLevels--;

			return sb;

		}

		public static StringBuilder ToFriendlyString(HCPTree.Node node, int iLevels, int i = 1)
		{
			StringBuilder sb = new StringBuilder();

			if (null != node.Categorization)
			{
				string strCategorization = FormatUtil.FormatPrototype(node.Categorization, iLevels).ToString();
				if (strCategorization.Count(x => x == '\n') > 1)
				{
					sb.Append(new string('\t', iLevels)).AppendLine($"Categorization {i}:");
					sb.Append(new string('\t', ++iLevels));
					sb.Append(strCategorization).AppendLine();
					--iLevels;
				}
				else
					sb.Append(new string('\t', iLevels)).Append(i).Append(": ").AppendLine(strCategorization.Trim());
			}

			if (null != node.Unpacked)
			{
				sb.Append(new string('\t', iLevels));
				sb.AppendLine("Unpacked:");
				sb.Append(new string('\t', ++iLevels));
				sb.Append(FormatUtil.FormatPrototype(node.Unpacked, iLevels)).AppendLine();
				--iLevels;
			}

			if (null != node.Context)
			{
				sb.Append(new string('\t', iLevels));
				sb.AppendLine("Context:");
				sb.Append(new string('\t', ++iLevels));
				sb.Append(FormatUtil.FormatPrototype(node.Context, iLevels)).AppendLine();
				--iLevels;
			}

			if (null != node.Label)
			{
				sb.Append(new string('\t', iLevels));
				sb.AppendLine("Label:");
				sb.Append(new string('\t', ++iLevels));
				sb.Append(FormatUtil.FormatPrototype(node.Label, iLevels)).AppendLine();
				--iLevels;
			}


			if (node.Children.Count > 0)
			{
				sb.Append(new string('\t', iLevels)).AppendLine("Children:");

				iLevels++;
				int iSubI = 1;
				foreach (HCPTree.Node nodeChild in node.Children)
				{
					sb.Append(ToFriendlyString(nodeChild, iLevels, iSubI));
					iSubI++;
				}
				--iLevels;
			}

			return sb;
		}



		public static StringBuilder ToFriendlyString(HCP hcp, HCPFormatting formatting)
		{
			int iLevels = 0;
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("Hidden Shadow:");
			sb.Append(new string('\t', ++iLevels));
			sb.Append(FormatUtil.FormatPrototype(hcp.HiddenShadow, iLevels)).AppendLine();

			sb.AppendLine("Shadow:");
			sb.Append(new string('\t', iLevels));
			sb.Append(FormatUtil.FormatPrototype(hcp.Shadow, iLevels)).AppendLine();

			//sb.AppendLine("Expectations:");

			//foreach (var pair in hcp.Expectations)
			//{
			//	sb.Append(new string('\t', 1));
			//	sb.Append(Prototypes.GetPrototype(pair.Key).PrototypeName).AppendLine(":");
			//	sb.Append(ToFriendlyString(pair.Value, formatting, 2));
			//}

			return sb;

		}

		public class HCPFormatting
		{
			public bool IncludeCategorization = true;
			public bool IncludeUnpacked = true;
			public bool IncludeContext = true;
			public bool IncludeLeaves = true;
		}

		//public static StringBuilder ToFriendlyString(HCPTree.Node node, HCPFormatting formatting , int iLevels)
		//{
		//	StringBuilder sb = new StringBuilder();

		//	if (null != node.Categorization && formatting.IncludeCategorization)
		//	{
		//		sb.Append(new string('\t', iLevels)).AppendLine("Categorization:");

		//		int i = 0;
		//		iLevels++;
		//		sb.Append(new string('\t', iLevels));
		//		sb.Append(FormatUtil.FormatPrototype(node.Categorization, iLevels)).AppendLine();

		//		--iLevels;
		//	}

		//	if (null != node.Unpacked && formatting.IncludeUnpacked)
		//	{
		//		sb.Append(new string('\t', iLevels));
		//		sb.AppendLine("Unpacked:");
		//		sb.Append(new string('\t', ++iLevels));
		//		sb.Append(FormatUtil.FormatPrototype(node.Unpacked, iLevels)).AppendLine();
		//		--iLevels;
		//	}

		//	if (null != node.Context && formatting.IncludeContext)
		//	{
		//		sb.Append(new string('\t', iLevels));
		//		sb.AppendLine("Context:");
		//		sb.Append(new string('\t', ++iLevels));
		//		sb.Append(FormatUtil.FormatPrototype(node.Context, iLevels)).AppendLine();
		//		--iLevels;
		//	}


		//	if (node.Children.Count > 0)
		//	{
		//		sb.Append(new string('\t', iLevels)).AppendLine("Children:");

		//		iLevels++;
		//		foreach (HCPTree.Node nodeChild in node.Children)
		//		{
		//			if (formatting.IncludeLeaves || nodeChild.Children.Count > 0)
		//				sb.Append(ToFriendlyString(nodeChild, formatting, iLevels));
		//		}
		//		--iLevels;
		//	}

		//	return sb;
		//}
	}
}
