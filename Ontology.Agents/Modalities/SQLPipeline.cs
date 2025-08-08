using BasicUtilities;
using Buffaly.NLU;
using Buffaly.SemanticDB.Data;
using Ontology.Agents.RAG_SQL;
using Ontology.Agents.DevelopmentAgent;
using Ontology.Simulation;
using ProtoScript.Interpretter;
using System.Text;

namespace Ontology.Agents
{
	public class SQLPipeline
	{
		public static bool ErrorOnNoCandidateActions = false;
		public static string ApplicationName = null;


                public static async Task UnderstandDirective(DirectiveRequest request)
                {
                        await PipelineBase.UnderstandDirective(request);
                }

		public static async Task<ImplementDirectiveResult> ImplementDirective(DirectiveRequest request)
		{
			ProtoScriptTagger tagger = request.Tagger;

			ImplementDirectiveResult result = await ImplementDirectiveViaStoredProcedure(request);

			//If an action was able to fulfill the request, use it. Otherwise fall through to the default 
			if (null != result)
			{
				return result;
			}

			throw new Exception("No valid action was identified to fulfill this directive");
		}

		private static async Task<ImplementDirectiveResult> ImplementDirectiveViaStoredProcedure(DirectiveRequest request)
		{
			ProtoScriptTagger tagger = request.Tagger;
			
			StringBuilder sb = new StringBuilder();
			StringBuilder sbNames = new StringBuilder();

			List<Prototype> colActions = request.CandidateActions;

			foreach (Prototype protoValue in colActions)
			{
				Prototype protoAction = tagger.Interpretter.NewInstance(protoValue);

				string strStoredProc = new StringWrapper(protoAction.Properties["StoredProcedure.Field.Name"]).GetStringValue();
				string strDefinition = new StringWrapper(protoAction.Properties["StoredProcedure.Field.Definition"]).GetStringValue();
				if (StringUtil.IsEmpty(strDefinition))
				{
					strDefinition = SQLActions.GetStoredProcedureText(strStoredProc, request.Session);
				}

				sbNames.AppendLine(strStoredProc);
				sb.Append(strDefinition);
			}

			FastPrototypeSet lstTables = new FastPrototypeSet();
			foreach (Prototype protoEntity in request.DirectiveEntities)
			{
				if (Prototypes.TypeOf(protoEntity, "Table"))
				{
					Prototype protoTableBase = protoEntity.GetBaseType();
					Prototype protoTable = tagger.Interpretter.NewInstance(protoTableBase);

					lstTables.Add(protoTableBase);

					Prototype protoRelationships = protoTable.Properties["Table.Field.Relationships"];
					if (null != protoRelationships)
					{
						foreach (Prototype protoRelated in protoRelationships.Children)
						{
							lstTables.Add(protoRelated.GetBaseType());
						}
					}
				}
			}


			JsonObject jsonValues = new JsonObject();

			jsonValues["StoredProcedureNames"] = sbNames.ToString();
			jsonValues["StoredProcedureDefinitions"] = sb.ToString();
			jsonValues["TableDefinitions"] = GetTableDefinitions(lstTables, tagger).ToString();
			jsonValues["AugmentedEntities"] = request.AugmentedEntities;
			jsonValues["ExampleDirectives"] = string.Join("\r\n\r\n", request.ExampleDirectives);

			ImplementDirectiveResult result = new ImplementDirectiveResult();

			if (request.CandidateActions.Count > 0)
			{
				var tuple = await SQLActions.GetSimpleGeneratedCall4(request.Directive, jsonValues);

				Logs.DebugLog.WriteEvent("Select Action Prompt", tuple.Item2);
				Logs.DebugLog.WriteEvent("Select Action Result", tuple.Item1);

				JsonObject jsonResult = new JsonObject(tuple.Item1);

				JsonArray jsonTables = jsonResult.GetJsonArrayOrDefault("ReferencedTables");
				FastPrototypeSet lstNewTables = GetNewlyReferencedTable(jsonTables, lstTables, tagger);


				result.Result = jsonResult;
				result.Result["ResultType"] = "StoredProcedure";
				result.Prompt = tuple.Item2;

				if (jsonResult.GetBooleanOrFalse("IsStoredProcedureAvailable") == false
					|| jsonResult.GetBooleanOrFalse("IsPartiallyAvailable")
					|| lstNewTables.Count > 0)
				{
					lstTables.AddRange(lstNewTables);

					jsonValues["PartialExplanation"] = jsonResult.GetStringOrNull("PartialExplanation");
					jsonValues["SelectedStoredProcedure"] = jsonResult.GetStringOrNull("GeneratedCall");

					JsonObject jsonResult2 = await CreateNewSQL(request, tagger, lstTables, jsonValues);
					foreach (string strKey in jsonResult2.Keys)
					{
						result.Result[strKey] = jsonResult2[strKey];
					}

					result.Result["ResultType"] = "SQL";
				}
			}
			else if (lstTables.Count > 0)
			{
				JsonObject jsonResult = await CreateNewSQL(request, tagger, lstTables, jsonValues);
				result.Result = jsonResult;
				result.Result["ResultType"] = "SQL";
			}


			return result;
		}

		private static async Task<JsonObject> CreateNewSQL(DirectiveRequest request, ProtoScriptTagger tagger,
			FastPrototypeSet lstTables, JsonObject jsonValues)
		{
			jsonValues["TableDefinitions"] = GetTableDefinitions(lstTables, tagger).ToString();

			var tuple2 = await SQLActions.CreateNewSQL(request.Directive, jsonValues);

			Logs.DebugLog.WriteEvent("Create SQL Prompt", tuple2.Item2);
			Logs.DebugLog.WriteEvent("Create SQL Result", tuple2.Item1);

			JsonObject jsonResult2 = new JsonObject(tuple2.Item1);

			//Check that the tables referenced where actually provided
			JsonArray jsonTables = jsonResult2.GetJsonArrayOrDefault("ReferencedTables");
			FastPrototypeSet lstNewTables = GetNewlyReferencedTable(jsonTables, lstTables, tagger);

			//Add the new tables to the prompt and rerun
			if (lstNewTables.Count > 0)
			{
				lstTables.AddRange(lstNewTables);

				jsonValues["TableDefinitions"] = GetTableDefinitions(lstTables, tagger).ToString();

				var tuple3 = await SQLActions.CreateNewSQL(request.Directive, jsonValues);

				Logs.DebugLog.WriteEvent("Create SQL Prompt", tuple3.Item2);
				Logs.DebugLog.WriteEvent("Create SQL Result", tuple3.Item1);


				jsonResult2 = new JsonObject(tuple3.Item1);
			}

			return jsonResult2;
		}

		private static StringBuilder GetTableDefinitions(FastPrototypeSet lstTables, ProtoScriptTagger tagger)
		{
			StringBuilder sbTables = new StringBuilder();

			foreach (Prototype table in lstTables)
			{
				Prototype protoTableInstance = tagger.Interpretter.NewInstance(table);
				if (null != protoTableInstance.Properties["Table.Field.Definition"])
					sbTables.Append(new StringWrapper(protoTableInstance.Properties["Table.Field.Definition"]).GetStringValue());

				string strTableName = new StringWrapper(protoTableInstance.Properties["Table.Field.TableName"]).GetStringValue();
				FragmentsRow rowFragmentTable = FragmentsRepository.GetFragmentByFragmentKey(strTableName);
				if (null != rowFragmentTable)
				{
                                        FragmentsRow? rowTableDescription = rowFragmentTable.Fragments.FirstOrDefault(x => x.FragmentTagTags.Any(y => y.TagName == "Table Description"));
					if (null != rowTableDescription)
					{
						sbTables.Append("\r\n\r\n").AppendLine("## Additional Information for " + strTableName);
						sbTables.AppendLine(rowTableDescription.Fragment);
					}
				}
			}

			return sbTables;
		}

		private static FastPrototypeSet GetNewlyReferencedTable(JsonArray jsonTables, FastPrototypeSet lstTables, ProtoScriptTagger tagger)
		{
			//Check that the tables referenced where actually provided
			FastPrototypeSet lstNewTables = new FastPrototypeSet();

			foreach (JsonValue jsonTable in jsonTables)
			{
				string strTable = jsonTable.ToString();
				Prototype ? protoEntity = Lexemes.GetLexemeByLexeme(strTable)?.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "Table")).Key;
				if (null != protoEntity)
				{
					Prototype protoBaseTable = protoEntity.GetBaseType();

					if (!lstTables.Contains(protoBaseTable))
						lstNewTables.Add(protoBaseTable);
				}

				//todo: what if it is not a table? 
			}

			return lstNewTables;
		}
	}
}
