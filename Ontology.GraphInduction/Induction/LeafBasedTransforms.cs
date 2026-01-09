using BasicUtilities;
using Buffaly.NLU;
using Ontology.BaseTypes;
using Ontology.GraphInduction.Model;
using Ontology.GraphInduction.Utils;
using Ontology.Simulation;
using ProtoScript;
using ProtoScript.Parsers;
using System.Text;

namespace Ontology.GraphInduction
{

	public class LeafBasedTransforms
	{
		public static void BuildTransform(Prototype protoSource, Prototype protoTarget, string strFile)
		{
			Prototype protoRoot = LeafBasedTransforms.GetRootedGraph(protoSource, protoTarget);

			//Use a leaf based comparison method to get the longest common paths within the rooted graph 
			List<Prototype> lstPaths = LeafBasedTransforms.GetEntityPathsByLeaf2(protoRoot);

			Logger.Log(lstPaths);

			//Generalize the graph, longest paths first, to remove any shorter common paths 
			foreach (Prototype protoPath in lstPaths)
			{
				Collection lstInstances = PrototypeGraphs.Find(protoRoot, x => PrototypeGraphs.AreEqual(x, protoPath));

				foreach (Prototype protoInstance in lstInstances.Children)
				{
					protoInstance.InsertTypeOf(Compare.Entity.Prototype);
				}
			}

			PrototypeLogging.Log(protoRoot);

			Prototype? protoSourceShadow = protoRoot.Properties["RootedTransform.Field.Source"];
			Prototype? protoTargetShadow = protoRoot.Properties["RootedTransform.Field.Target"];

			HCP hcpSource = HCPUtil.Create(new List<Prototype> { protoSource }, protoSourceShadow, Dimensions.CSharp_Code_Hidden);

			Logger.Log(hcpSource);

			HCP hcpTarget = HCPUtil.Create(new List<Prototype> { protoTarget }, protoTargetShadow, Dimensions.CSharp_Code_Hidden);

			Logger.Log(hcpTarget);

			Prototype hiddenSource = HCPs.GetHCPInstance(protoSource, hcpSource);
			Prototype hiddenTarget = HCPs.GetHCPInstance(protoTarget, hcpTarget);

			Logger.Log(hiddenSource);
			Logger.Log(hiddenTarget);

			//			LeafBasedTransformMaterializer.Materialize(strFile, hcpSource, hiddenSource, hcpTarget, hiddenTarget);
		}

		//public static List<Prototype> GetLeafSentintels(Prototype protoSource)
		//{
		//	//GetLeavesOnNormal allows us to run on an already augmented graph
		//	List<Prototype> lstLeaves = PrototypeGraphs.GetLeavesOnNormal(protoSource);
		//	List<Prototype> lstLeafSentintels = new List<Prototype>();

		//	foreach (Prototype protoLeaf in lstLeaves)
		//	{
		//		if (protoLeaf is NativeValuePrototype && (protoLeaf as NativeValuePrototype).NativeValue is string)
		//		{
		//			Prototype protoLexeme = TemporaryLexemes.GetLexemeOrNull((protoLeaf as NativeValuePrototype).NativeValue as string);
		//			if (null != protoLexeme)
		//			{
		//				Collection colRelated = TemporaryLexemes.GetRelatedPrototypes(protoLexeme);

		//				//Augment and return the original leaves
		//				colRelated.Children.ForEach(x => TypeOfs.Insert(protoLeaf, x));

		//				lstLeafSentintels.Add(protoLeaf);
		//			}
		//		}
		//	}

		//	return lstLeafSentintels;

		//}

		//public static Tuple<bool, Prototype> MatchesPathToRoot(Prototype protoLeaf, Prototype protoLeafSentinel)
		//{
		//	Prototype protoPossibleHiddens = protoLeafSentinel.Properties["LeafSentinel.Field.PossibleHiddens"];

		//	if (protoPossibleHiddens.Children.Count > 1)
		//		throw new Exception("Can't return more than one possibility ");

		//	foreach (Prototype protoPossibleHidden in protoPossibleHiddens.Children)
		//	{
		//		//Test the path back to root
		//		Prototype protoPathToRoot = protoPossibleHidden.Properties["SentinelHiddens.Field.PathToRoot"];
		//		protoPathToRoot.SetParents();

		//		Prototype protoRootLeaf = PrototypeGraphs.GetLeaves(protoPathToRoot).Last();

		//		Prototype protoTargetLeaf = protoLeaf;

		//		while (protoRootLeaf.Parent != null)
		//		{
		//			protoRootLeaf = protoRootLeaf.Parent;
		//			protoTargetLeaf = protoTargetLeaf?.Parent;
		//		}

		//		if (null != protoTargetLeaf && TemporaryPrototypeCategorization.IsCategorized(protoTargetLeaf, protoRootLeaf))
		//		{
		//			return new Tuple<bool, Prototype>(true, protoTargetLeaf);
		//		}

		//	}

		//	return new Tuple<bool, Prototype>(false, null);
		//}

		//public static List<Prototype> GetMatchingPathsToRoot(Prototype protoLeaf, Prototype protoLeafSentinel)
		//{
		//	Prototype protoPossibleHiddens = protoLeafSentinel.Properties["LeafSentinel.Field.PossibleHiddens"];

		//	List<Prototype> lstMatching = new List<Prototype>();

		//	foreach (Prototype protoPossibleHidden in protoPossibleHiddens.Children)
		//	{
		//		//Test the path back to root
		//		Prototype protoPathToRoot = protoPossibleHidden.Properties["SentinelHiddens.Field.PathToRoot"];
		//		protoPathToRoot.SetParents();

		//		Prototype protoRootLeaf = PrototypeGraphs.GetLeaves(protoPathToRoot).Last();

		//		Prototype protoTargetLeaf = protoLeaf;

		//		while (protoRootLeaf.Parent != null)
		//		{
		//			protoRootLeaf = protoRootLeaf.Parent;
		//			protoTargetLeaf = protoTargetLeaf?.Parent;
		//		}

		//		if (null != protoTargetLeaf && TemporaryPrototypeCategorization.IsCategorized(protoTargetLeaf, protoRootLeaf))
		//		{
		//			TypeOfs.Insert(protoTargetLeaf, protoPossibleHidden.Properties["SentinelHiddens.Field.HCP"]);

		//			lstMatching.Add(protoTargetLeaf);
		//		}

		//	}

		//	return lstMatching;
		//}

		//public static Prototype TransformLeafPath(Prototype protoPathToRoot, NativeInterpretter interpretter)
		//{
		//	Prototype protoHCP = TypeOfs.Get(protoPathToRoot).Children.First();

		//	protoHCP = interpretter.RunMethodAsPrototype(protoHCP, "To_Hidden", protoPathToRoot);
		//	Logger.Log(protoHCP);

		//	Prototype protoHCP2 = interpretter.RunMethodAsPrototype(protoHCP, "Transform", new List<object>());
		//	Logger.Log(protoHCP2);

		//	Prototype protoUnpacked = interpretter.RunMethodAsPrototype(protoHCP2, "From_Hidden", new List<object>());

		//	Logger.Log(protoUnpacked);

		//	return protoUnpacked;
		//}


		public static Prototype GetRootedGraph(Prototype protoSource, Prototype protoTarget)
		{
			ProtoScriptTagger tagger = new ProtoScriptTagger();
			tagger.InterpretCode(@"
reference Ontology Ontology; 
import Ontology Ontology.Prototype Prototype;


prototype RootedTransform
{
	Prototype Source;
	Prototype Target; 

	function Create(Prototype protoSource, Prototype protoTarget) : RootedTransform
	{
		return new RootedTransform() { Source = protoSource.Clone(), Target = protoTarget.Clone() };
	}
}
");

			//Put everything into a single rooted graph so we are operating on one graph
			Prototype protoRoot = tagger.Interpretter.RunMethodAsPrototype("RootedTransform", "Create", new List<object> { protoSource, protoTarget })!;

			return protoRoot;
		}

		public static List<Prototype> GetPathToRoot(Prototype prototype)
		{
			List<Prototype> lstPath = new List<Prototype>();

			while (null != prototype)
			{
				lstPath.Add(prototype);
				prototype = prototype.Parent;
			}

			return lstPath;
		}

		private Prototype GetPathToRoot2(Prototype prototype)
		{
			Prototype protoPath = Compare.Entity.Prototype.ShallowClone();

			while (null != prototype.Parent)
			{
				Prototype protoParent = prototype.Parent;

				Prototype protoTemp = protoPath;
				protoPath = protoParent.ShallowClone();

				bool bFound = false;

				foreach (var pair in protoParent.NormalProperties)
				{
					if (pair.Value == prototype)
					{
						protoPath.Properties[pair.Key] = protoTemp;
						bFound = true;
						break;
					}
				}

				if (!bFound)
				{
					foreach (Prototype protoChild in protoParent.Children)
					{
						if (protoChild == prototype)
						{
							protoPath.Children.Add(protoTemp);
						}
						else
						{
							protoPath.Children.Add(Compare.Ignore.Prototype.ShallowClone());
						}
					}
				}

				prototype = protoParent;
			}

			return protoPath;
		}
	

		public static List<Prototype> GetEntityPathsByLeaf2(Prototype prototype)
		{
			//This version allows for mulitple different versions of a path 
			prototype.SetParents();

			//Set all the values to 0 on the graph. We will use those to count occurrences
			PrototypeGraphs.DepthFirstOnNormal(prototype, x =>
			{
				x.Value = 0;
				return x;
			});

			List<Prototype> lstLeaves = PrototypeGraphs.GetLeaves(prototype).Where(x => Prototypes.TypeOf(x, System_String.Prototype) || Prototypes.TypeOf(x, Compare.Entity.Prototype)).ToList();
			List<Prototype> lstPaths = new List<Prototype>();

			//Do a deep comparison

			foreach (Prototype protoLeaf in lstLeaves)
			{
				//Get every path from root to a String or Entity. We will test each to find the largest portion 
				//that matches other locations in the graph 

				List<Prototype> lstPath = GetPathToRoot(protoLeaf);

				foreach (Prototype protoParent in lstPath)
				{
					if (protoParent.ShallowEqual(Compare.Entity.Prototype))
						continue;

					if (lstPaths.Any(x => x == protoParent))
					{
						break; //we've already been up this chain
							   //TODO: this checks for chains we've been up successfully 
							   //but we could check for chains we've been up unsuccessfully as well
					}

					//Find all other locations on the graph that match the current path
					Collection colResults = PrototypeGraphs.Find(prototype, x =>
						x != protoParent &&
						//Note: We've already tested children elements. But this will not just categorize along the 
						//direct path. If there are non-related children here, it will include those in the categorization 
						//ParentNode
						//		ChildNodeThatIsPartOfPath
						//		ChildNodeThatIsNotPartOfPath		(also included)
						TemporaryPrototypeCategorization.IsCategorized(x, protoParent)
					);

					if (colResults.Children.Count == 0)
					{
						break; //Don't keep testing this parent chain, nothing more will match
					}

					colResults.Children.ForEach(x => protoParent.Value++);

					//Only keep the longest path
					lstPaths.RemoveAll(x => x.Parent == protoParent);
					lstPaths.Add(protoParent);
				}

			}


			return lstPaths;

		}
	}

	//	public class LeafBasedTransformMaterializer
	//	{
	//		public static void Materialize(string strFile, HCP hcpSource, Prototype hiddenSource, HCP hcpTarget, Prototype hiddenTarget)
	//		{
	//			ProtoScript.File file = new ProtoScript.File();

	//			if (System.IO.File.Exists(strFile))
	//				file = ProtoScript.Parsers.Files.Parse(strFile);

	//			Collection linkagesSource = new Collection();

	//			foreach (var pair in hiddenSource.NormalProperties)
	//			{
	//				Prototype protoEntity = pair.Value;
	//				Prototype protoEntityShadow = EntityUtil.CreateEntityShadowFromValues(new List<Prototype>() { protoEntity }, Dimensions.CSharp_Code_Hidden);

	//				linkagesSource.Properties[pair.Key] = protoEntityShadow.ShallowClone();
	//			}

	//			Collection linkagesTarget = new Collection();

	//			foreach (var pair in hiddenTarget.NormalProperties)
	//			{
	//				Prototype protoEntity = pair.Value;
	//				Prototype protoEntityShadow = EntityUtil.CreateEntityShadowFromValues(new List<Prototype>() { protoEntity }, Dimensions.CSharp_Code_Hidden);

	//				linkagesTarget.Properties[pair.Key] = protoEntityShadow.ShallowClone();
	//			}

	//			PrototypeSet setEntities = new PrototypeSet();
	//			setEntities.AddRange(linkagesSource.NormalProperties.Select(x => x.Value));
	//			setEntities.AddRange(linkagesTarget.NormalProperties.Select(x => x.Value));


	//			PrototypeDefinition defSourceHCP = HCPToPrototypeDefinition(hcpSource, linkagesSource);
	//			PrototypeDefinition defTargetHCP = HCPToPrototypeDefinition(hcpTarget, linkagesTarget);
	//			List<PrototypeDefinition> lstEntityDefinitions = CreateEntityDefinitions(setEntities);

	//			lstEntityDefinitions.ForEach(x => file.InsertOrReplace(x));
	//			file.InsertOrReplace(defSourceHCP);
	//			file.InsertOrReplace(defTargetHCP);


	//			//Add the transforms from source to target
	//			PtsGenerator generator = new PtsGenerator();
	//			generator.Add("hcpSource", hcpSource);
	//			generator.Add("hcpTarget", hcpTarget);

	//			defSourceHCP.Inherits.Add(ProtoScript.Parsers.Types.Parse("Transformable"));
	//			defSourceHCP.Methods.Add(generator.CreateFunctionDefinition(@"

	//	function Transform() : HCP 
	//	{
	//		Collection entities = this.To_Entities();

	//		<%hcpTarget.HiddenShadow.PrototypeName%> hcp = new <%hcpTarget.HiddenShadow.PrototypeName%>();
	//		hcp.From_Entities(entities);	

	//		return hcp;	
	//	}

	//"));


	//			List<Prototype> lstLeafSentinelPaths = PrototypeGraphs.GetLeafPaths(hcpSource.Shadow);
	//			List<PrototypeDefinition> lstSentinelDefinitions = new List<PrototypeDefinition>();

	//			foreach (Prototype protoPath in lstLeafSentinelPaths)
	//			{
	//				Prototype protoLeafSentinel = PrototypeGraphs.GetValue(hcpSource.Shadow, protoPath);
	//				if (Prototypes.TypeOf(protoLeafSentinel, System_String.Prototype))
	//				{
	//					string str = NativeValuePrototypes.FromPrototype(protoLeafSentinel) as string;
	//					string strLeafName = str.Replace(".", "_");
	//					generator.Add("LeafName", strLeafName);
	//					generator.Add("LeafString", str);
	//					generator.Add("PathToRoot", PrototypeLogging.ToFriendlyShadowString2(protoPath));


	//					PrototypeDefinition defLeaf = file.PrototypeDefinitions.FirstOrDefault(x => StringUtil.EqualNoCase(x.PrototypeName.TypeName, strLeafName));
	//					if (null == defLeaf)
	//					{
	//						defLeaf = generator.CreatePrototype(@"
	//[Lexeme.Singular(""<%LeafString%>"")]
	//prototype <%LeafName%>: LeafSentinel
	//{ 
	//	function that() : void 
	//	{
	//		this.PossibleHiddens = [ 
	//		];
	//	}
	//}");
	//						lstSentinelDefinitions.Add(defLeaf);
	//					}

	//					ProtoScript.Expression expr = generator.CreateExpression(@"
	//			new SentinelHiddens() {
	//				PathToRoot = PrototypeShortFormParser.FromShortString(@""
	//<%PathToRoot%>			
	//				""),
	//				HCP = <%hcpSource.HiddenShadow.PrototypeName%>
	//			}
	//");
	//					ProtoScript.Statement statement = defLeaf.Methods[0].Statements[0];

	//					ProtoScript.ExpressionStatement expressionStatement = statement as ProtoScript.ExpressionStatement;

	//					string strGuid = StringUtil.RightOfLast(hcpSource.HiddenShadow.PrototypeName, ".");
	//					List<ProtoScript.Expression> expressions = expressionStatement.Expression.GetChildrenExpressions().ToList();
	//					if (!expressions.Any(x => x is ProtoScript.Identifier && ((ProtoScript.Identifier)x).Value == strGuid))
	//					{
	//						ProtoScript.ArrayLiteral arrayLiteral = (ProtoScript.ArrayLiteral)expressions.First(x => x is ProtoScript.ArrayLiteral);
	//						arrayLiteral.Values.Insert(0, expr);
	//					}

	//				}
	//			}

	//			lstSentinelDefinitions.ForEach(x => file.InsertOrReplace(x));

	//			string strResult = SimpleGenerator.Generate(file);

	//			FileUtil.WriteFile(strFile, strResult);
	//		}


	//		public static ProtoScript.PrototypeDefinition HCPToPrototypeDefinition(HCP hcp, Collection linkages)
	//		{
	//			PtsGenerator generator = new PtsGenerator();
	//			generator.Add("hcpSource", hcp);

	//			PrototypeDefinition defSourceHCP = generator.CreatePrototype(@"
	//prototype <%hcpSource.HiddenShadow.PrototypeName%> : HCP {

	//	Prototype Shadow = PrototypeShortFormParser.FromShortString(@""
	//<%PrototypeLogging.ToFriendlyShadowString2 (hcpSource.Shadow)%>
	//"");

	//	<%hcpSource.HiddenShadow.NormalProperties.each pair{%>
	//	Prototype <%_index%>;
	//	<%}%>

	//	Prototype Paths = [
	//	<%hcpSource.Paths.each pair{%>
	//		<%if (not (_first)) ,%> PrototypeShortFormParser.FromShortString(@""
	//<%PrototypeLogging.ToFriendlyShadowString2 (pair.Value)%>
	//"")	<%}%>
	//];

	//	function GetShadow() : Prototype 
	//	{
	//		return this.Shadow;
	//	}

	//	function To_Hidden(Prototype protoSource) : that
	//	{
	//		that hidden = new that();

	//	<%hcpSource.Paths.each pair{%>
	//		hidden.<%_index%> = PrototypeGraphs.GetValue(protoSource, this.Paths[<%_index%>]);
	//	<%}%>

	//		return hidden;
	//	}


	//	function From_Hidden() : Prototype
	//	{
	//		Prototype protoShadow = this.Shadow.Clone();

	//	<%hcpSource.Paths.each pair{%>
	//		PrototypeGraphs.SetValue(protoShadow, this.Paths[<%_index%>], this.<%_index%>, false, false);
	//	<%}%>

	//		return protoShadow;
	//	}
	//}
	//");

	//			string strShadowComment = FormatUtil.FormatPrototype2(hcp.Shadow);
	//			defSourceHCP.Fields.First().Comments = "//" + StringUtil.LeftOfLast(strShadowComment, "\r\n");

	//			{
	//				StringBuilder sb = new StringBuilder();

	//				int i = 0;
	//				foreach (var linkage in linkages.NormalProperties)
	//				{
	//					//CSharp.Code.Hidden.BABD521386CD0B58289E8951BF965D87 entity1 = new CSharp.Code.Hidden.BABD521386CD0B58289E8951BF965D87();
	//					//entity1.0 = this.0;
	//					//entities.Add(entity1);
	//					string strSourceProp = StringUtil.RightOfLast(new Prototype(linkage.Key).PrototypeName, ".");
	//					sb.Append($"{linkage.Value.PrototypeName} entity{i} = new {linkage.Value.PrototypeName}();\r\n");
	//					sb.Append($"entity{i}.0 = this.{strSourceProp};\r\n");
	//					sb.Append($"entities.Add(entity{i});\r\n\r\n");


	//					ProtoScript.FieldDefinition fieldDef = defSourceHCP.Fields.First(x => x.FieldName == i.ToString());
	//					fieldDef.Annotations.Add(AnnotationExpressions.Parse($"[EntityLinkage({linkage.Value.PrototypeName})]"));

	//					i++;
	//				}

	//				defSourceHCP.Methods.Add(generator.CreateFunctionDefinition(@"
	//	function To_Entities() : Collection 
	//	{
	//		Collection entities = new Collection();
	//		" + sb.ToString() + @"
	//		return entities;
	//	}
	//"));
	//			}

	//			{
	//				PrototypeSet setSourceEntities = new PrototypeSet(linkages.NormalProperties.Select(x => x.Value));
	//				StringBuilder sb2 = new StringBuilder();
	//				foreach (Prototype entity in setSourceEntities)
	//				{
	//					generator.Add("entity", entity.PrototypeName);

	//					//function From(CSharp.Code.Hidden.BABD521386CD0B58289E8951BF965D87 entity) : void
	//					//{
	//					//	this.0 = entity.0;
	//					//	this.1 = entity.0;
	//					//}

	//					StringBuilder sb = new StringBuilder();
	//					foreach (var linkage in linkages.NormalProperties.Where(x => x.Value == entity))
	//					{
	//						string strSourceProp = StringUtil.RightOfLast(new Prototype(linkage.Key).PrototypeName, ".");
	//						sb.Append($"this.{strSourceProp} = entity.0;\r\n");
	//					}

	//					defSourceHCP.Methods.Add(generator.CreateFunctionDefinition(@"
	//	function From<<%entity%>>(<%entity%> entity) : void
	//	{
	//		" + sb.ToString() + @"
	//	}
	//"));

	//					//function From_Entities(Collection entities) : void
	//					//{
	//					//	foreach (Prototype entity in entities)
	//					//	{
	//					//		if (entity typeof CSharp.Code.Hidden.BABD521386CD0B58289E8951BF965D87)
	//					//							this.From(entity as CSharp.Code.Hidden.BABD521386CD0B58289E8951BF965D87);
	//					//	}
	//					//}

	//					sb2.Append(generator.Evaluate(@"
	//if (entity typeof <%entity%>)
	//	this.From<<%entity%>>(entity as <%entity%>);"
	//));
	//				}

	//				defSourceHCP.Methods.Add(generator.CreateFunctionDefinition(@"
	//	function From_Entities(Collection entities) : void
	//	{
	//		foreach (Prototype entity in entities)
	//		{
	//			" + sb2.ToString() + @"
	//		}
	//	}
	//"));
	//			}


	//			return defSourceHCP;
	//		}

	//		public static List<PrototypeDefinition> CreateEntityDefinitions(PrototypeSet setEntities)
	//		{
	//			List<PrototypeDefinition> lstDefinitions = new List<PrototypeDefinition>();
	//			PtsGenerator generator = new PtsGenerator();

	//			foreach (Prototype entity in setEntities)
	//			{
	//				generator.Add("entity", entity.PrototypeName);

	//				lstDefinitions.Add(generator.CreatePrototype(@"
	//prototype <%entity%> : Entity 
	//{ 
	//	Prototype 0;
	//}"
	//				));
	//			}

	//			return lstDefinitions;
	//		}


	//	}

}
