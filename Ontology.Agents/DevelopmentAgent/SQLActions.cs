using BasicUtilities;
using Buffaly.AIProviderAPI;
using Ontology.Simulation;
using RooTrax.Common.DB;
using System.Data;
using System.Text;

namespace Ontology.Agents.RAG_SQL
{
	public class SQLActions
	{
		static public async Task<Tuple<string, string>> GetSimpleGeneratedCall4(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are helping to generate a call to a stored procedure in response to a user's request. 

You will be provided with a user's request, a list of stored procedures, and a list of entities identified in the user's request.

You will also be provided with previous examples of requests and the correctly generated calls. They may
contain information to help with your decision.

The correct stored procedure may not be provided. Use all the 
information available to generate a call to the correct stored procedure. If the stored procedure is not available,
do not generate a call, and return IsStoredProcedureAvailable = false. If a stored procedure is available, but it 
does not fully address the user's request, generate a call to the stored procedure and return 
IsPartiallyAvailable = true. 
Respond in JSON with these fields: 

{
	'IsStoredProcedureAvailable' = true or false	//indicates if the stored procedure is available 
	'IsPartiallyAvailable' = true or false //indicates if the stored procedure is available, but does not fully address the user's request
	'PartialExplanation' = 'explanation' //if IsPartiallyAvailable is true, this is the explanation
	'SelectedStoredProcedure' = 'StoredProcedureName' //the name of the stored procedure that was selected
	'GeneratedCall' = 'call' //the generated call to the stored procedure,
	'Entities' = ['entity1', 'entity2'] //all entities from the user's request
	'UnknownEntities' = ['entity1', 'entity2'] //the entities that were unknown from the user's request,
	'ReferencedTables' = ['table1', 'table2'] //the tables that were referenced in the SQL code

}

The call should be valid MSSQL for 2019. The request may not actually be related to the most likely stored procedures.
In this case you should return IsStoredProcedureAvailable = false and an explanation. 

Do not guess. Do not make up stored procedures.

";

			string strUserInput = @"
# The user's request is 

{Directive}

# Example Requests

{ExampleDirectives}

# The most likely stored procedures are:

{StoredProcedureNames}

# In the user request, these entities were identified:

{AugmentedEntities}

# Stored Procedure Definitions:

{StoredProcedureDefinitions}

# Table Definitions:

{TableDefinitions}

";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}

		static public async Task<Tuple<string, string>> CreateNewSQL(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are helping to create SQL Code to fulfill a user's request. One or more stored procedure may have already been identified 
as fulfilling part of the request, but additional logic may be needed. It is your job to take the user's request and
stored procedure(s) and generate the necessary SQL code to fulfill the request. An explanation as to why the current 
stored procedure does not completely fulfill the request may be provided. 

You may also be presented with examples of similar requests and the SQL code that was generated to fulfill them. Pay 
particular attention to those examples as they may contain valuable information. Those examples may be used 
directly or a template for the SQL code you generate.

The result of your SQL code should be a single result, answering the user's request as closely as possible. You may need 
to gather additional information to pass to the stored procedure, or you may need to perform additional logic on the results. 

Retrieving the results of a stored procedure and operating upon it is difficult. Prefer to create new SQL, inspired by the stored 
procedure, rather than trying to operate on the results of the stored procedure. List all tables that are referenced in the SQL code.

Generate the result in the following JSON structure: 

{
	'GeneratedSQL' = 'sql code', //this is the SQL code to fulfill the request
	'ReferencedTables' = ['table1', 'table2'] //the tables that were referenced in the SQL code

}

The call should be valid MSSQL for 2019. 

";

			string strUserInput = @"
# The user's request is 

{Directive}

# The selected stored procedure call is: 

{SelectedStoredProcedure}

# However the stored procedures do not completely fulfill the request because

{PartialExplanation}

# Example Requests from Similar Requests:

{ExampleDirectives}

# Additional potentially relevant stored procedures are provided for reference:

{StoredProcedureNames}

# In the user request, these entities were identified:

{AugmentedEntities}

# Stored Procedure Definitions:

{StoredProcedureDefinitions}

# Table Definitions:

{TableDefinitions}

";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}


		static public async Task<Tuple<string, string>> CreateNewStoredProcedure(string strDirective, JsonObject jsonValues)
		{
			string strPrompt = @"

# Instructions:

You are helping to create a new stored procedure or generate a call to an existing stored procedure 
based on a user's request. 

The most similar and relevant stored procedures will be provided. However, the correct stored procedure may not be provided. 
If you can use one of the existing stored procedures, or a combination of stored procedures, use that. Otherwise 
create a new stored procedure. Do not create a new stored procedure that duplicates the functionality of an 
existing stored procedure. 

Respond in JSON with these fields: 

{
	'GeneratedStoredProcedure' = 'stored procedure definition' //this is the stored procedure that was generated
	'GeneratedCall' = 'call to the stored procedure with all parameters' //the generated call to the stored procedure including all parameters
	'Entities' = ['entity1', 'entity2'] //all entities from the user's request
	'UnknownEntities' = ['entity1', 'entity2'] //the entities that were unknown from the user's request
}

The call should be valid MSSQL for 2019. If the request is not related to the most likely stored procedures, then you should 
not generate a new stored procedure. 

";

			string strUserInput = @"
# The user's request is 

{Directive}

# The most likely stored procedures are:

{StoredProcedureNames}

# In the user request, these entities were identified:

{AugmentedEntities}

# Stored Procedure Definitions:

{StoredProcedureDefinitions}

";

			strUserInput = strUserInput.Replace("{Directive}", strDirective);
			strUserInput = strUserInput.Replace("{StoredProcedureNames}", jsonValues.GetStringOrNull("StoredProcedureNames"));
			strUserInput = strUserInput.Replace("{StoredProcedureDefinitions}", jsonValues.GetStringOrNull("StoredProcedureDefinitions"));

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}

		static public async Task<Tuple<string, string>> Continuation(string strDirective,
			string strError, string strGeneratedCall, string strInfoUsed,
			JsonObject jsonValues)
		{
			if (StringUtil.IsEmpty(strGeneratedCall))
				throw new Exception("GeneratedCall is required");

			jsonValues["Directive"] = strDirective;
			jsonValues["Error"] = strError;
			jsonValues["GeneratedCall"] = strGeneratedCall;
			jsonValues["InfoUsed"] = strInfoUsed;

			string strPrompt = @"

# Instructions:

You are helping to create a new stored procedure or generate a call to an existing stored procedure 
based on a user's request for use with MSSQL 2019.

You already generated some SQL, but it did not work. The SQL you generated and the error message will 
be provided along with the information you used to create the call. Attempt to fix the call to the stored
procedure. 

Do not guess. Do not make up column or table names. If there is not enough information provided, do not guess. 
Respond with a message that you need more information and identify the information you need.

Respond in JSON with these fields: 

{
	'GeneratedSQL' = 'sql code' //this is the SQL code to fulfill the request
	'IsInformationNeeded' = true //if you need more information to fulfill the request
	'InformationNeeded' = 'information needed' //if you need more information, specify what you need
}

The call should be valid MSSQL for 2019. If the request is not related to the most likely stored procedures, then you should 
not generate a new stored procedure. 

";

			string strUserInput = @"
# The user's request is 

{Directive}

# You generated this SQL:

{GeneratedCall}

# However MS SQL returned this error:

{Error}

# Here is the original information you used to generate the call:

{InfoUsed}

";

			foreach (string strKey in jsonValues.Keys)
			{
				strUserInput = strUserInput.Replace("{" + strKey + "}", jsonValues.GetStringOrDefault(strKey, string.Empty));
			}

			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			return new Tuple<string, string>(strResult, strUserInput);
		}





		public static string GetStoredProcedureText(string strName, SessionObject session)
		{
			string strConnectionString = session.Settings.GetStringOrNull("ConnectionString");
			if (StringUtil.IsEmpty(strConnectionString))
				throw new Exception("SessionObject.Settings.ConnectionString not set");

			strName = StringUtil.Remove(strName, "[");
			strName = StringUtil.Remove(strName, "]");
			strName = StringUtil.Remove(strName, "dbo.");

			string strQuery = $"select ROUTINE_DEFINITION from INFORMATION_SCHEMA.routines where routine_name = '{strName}'";

			DataSet ds = DBUtilities.DataSetFromQuery(strQuery, strConnectionString);
			return ds.Tables[0].Rows[0][0].ToString();
		}


		public static string GetTableDefinition(string strName, SessionObject session)
		{
			strName = StringUtil.Remove(strName, "[");
			strName = StringUtil.Remove(strName, "]");
			strName = StringUtil.Remove(strName, "dbo.");

			TemporaryLexeme rowLexeme = TemporaryLexemes.GetLexemeByLexeme(strName);
			Prototype ? protoTable = rowLexeme.LexemePrototypes.FirstOrDefault(x => Prototypes.TypeOf(x.Key, "Table")).Key;
			if (null == protoTable)
				return string.Empty;

			if (protoTable.IsInstance())
				protoTable = protoTable.GetBaseType();

			StringBuilder sb = new StringBuilder();
			Prototype protoEntity = session.Tagger.Interpretter.NewInstance(protoTable);
			{
				string strTableName = protoEntity.Properties.GetStringOrDefault("Table.Field.TableName");
				sb.AppendLine($"## Table: {strTableName}");
				sb.AppendLine(protoEntity.Properties.GetStringOrDefault("Table.Field.Definition"));

			}

			return sb.ToString();
		}



		public static async Task<string> GetDatabaseEntities(string strEntity, SessionObject session)
		{
			DirectiveRequest request = new DirectiveRequest(strEntity, session);

			request.AllowLLMEntitySelection = false;
			request.Modality = "DatabaseEntity";
			request.IsModalityFixed = true;
			request.RequestValues = new JsonObject();

			await PipelineBase.UnderstandEntities(request);


			StringBuilder sb = new StringBuilder();
			foreach (Prototype protoEntity in request.DirectiveEntities)
			{
				if (Prototypes.TypeOf(protoEntity, "Table"))
				{
					string strTableName = protoEntity.Properties.GetStringOrDefault("Table.Field.TableName");
					sb.AppendLine($"## Table: {strTableName}");
					sb.AppendLine(protoEntity.Properties.GetStringOrDefault("Table.Field.Definition"));
				}

				if (Prototypes.TypeOf(protoEntity, "StoredProcedure"))
				{
					string strStoredProcedureName = protoEntity.Properties.GetStringOrDefault("StoredProcedure.Field.Name");
					string strStoredProcedureDefinition = GetStoredProcedureText(strStoredProcedureName, session);
					if (string.IsNullOrEmpty(strStoredProcedureDefinition))
					{
						sb.AppendLine($"## Stored Procedure: {strStoredProcedureName} Not Found");
					}
					else
					{
						sb.AppendLine($"## Stored Procedure: {strStoredProcedureName}");
						sb.AppendLine(strStoredProcedureDefinition);
					}
				}
			}

			return sb.ToString();

		}

		public static void ConnectToDatabase(string strDatabase, SessionObject session)
		{
			string strConnectionString = ConnectionManager.GetConnectionString(strDatabase);
			if (StringUtil.IsEmpty(strConnectionString))
				throw new Exception("ConnectionString not found for " + strDatabase);

			session.Settings["ConnectionString"] = strConnectionString;
		}


		static public async Task<string> GenerateSQLCall(string strDirective, string strCode, string strDatabaseObjects, JsonObject jsonValues)
		{
			// strPrompt: Contains the instructions for generating final SQL code.
			// The prompt instructs the API to generate valid MSSQL code (SQL Server 2019) based on:
			// - The user's natural language directive.
			// - Any embedded SQL code.
			// - A set of database object definitions (e.g., tables, stored procedures).
			// The API should return a JSON response with:
			//   "sqlCode": the generated SQL,
			//   "missingDatabaseObjects": a list of any unresolved database objects,
			//   "status": "Success" if complete, or "Incomplete" if further objects are needed.
			string strPrompt = @"
# Instructions:

You are tasked with generating final SQL code based on a comprehensive input that includes:
1. The user's natural language directive specifying the desired functionality.
2. Any embedded SQL code that may provide partial instructions or examples.
3. A complete set of database object definitions (including tables, stored procedures, and other relevant objects).

Your objective is to produce valid MSSQL code for SQL Server 2019 that fully implements the requested functionality described in the user's directive.

When constructing your output, please adhere to the following guidelines:

1. Thoroughly analyze the user's directive and the embedded SQL code to understand the intended functionality.
2. Use the provided database object definitions to ensure that all references in your generated SQL are valid.
3. If all required database objects are available, generate complete SQL code that fulfills the directive.
4. If one or more required database objects are missing or unresolved, do not fabricate them. Instead, return a status indicating that the result is incomplete.
5. Include any warnings, assumptions, or diagnostic details that might be useful for further refinement.
6. Provide a detailed explanation of your decisions, including your interpretation of the directive, assumptions made, and any ambiguities encountered.
7. Return additional debug information to assist in diagnosing issues or guiding subsequent iterations if needed.

Respond in JSON with the following fields:

{
    ""GeneratedSQL"": ""string"",					// The complete generated SQL code if all required objects are available; if not, any partial SQL that could be generated.
	""IsInformationNeeded"" = true					//if you need more information to fulfill the request
	""Explanation"" = 'explanation'					//Detail any problems with the sql, such as missing objects, references, things you may need to successfully complete the SQL. Assume this will be
//used to provide any new objects back for further processing
    ""Status"": ""string"",                     // ""Success"" if all required objects are available and the SQL code is complete; ""Incomplete"" if additional objects are needed.
}

Ensure that:
- If a required database object is not available, set ""status"" to ""Incomplete"" and list the missing objects in ""missingDatabaseObjects"".
- Do not fabricate or assume missing database objects; only use the provided definitions.
- Provide a clear and thorough explanation in the ""Explanation"" field to aid in debugging or iterative improvement.
- Include any warnings or additional debug information to facilitate further refinement if necessary.

		";

			// strUserInput: The assembled input template that incorporates the user's directive,
			// any embedded SQL code, and the database object definitions.
			// Placeholders in curly braces are replaced with actual values.
			string strUserInput = @"
# User Directive:
{Directive}

# Embedded SQL Code:
{Code}

# Database Object Definitions:
{DatabaseObjects}
";

			// Replace the {Directive} and {Code} placeholders with the provided parameters.
			strUserInput = strUserInput.Replace("{Directive}", strDirective)
								   .Replace("{Code}", strCode);

			strUserInput = strUserInput.Replace("{DatabaseObjects}", strDatabaseObjects);

			if (null != jsonValues)
			{
				// Replace any additional placeholders in strUserInput using jsonValues.
				// Expected key: "DatabaseObjects" (or any others provided in jsonValues)
				foreach (string key in jsonValues.Keys)
				{
					strUserInput = strUserInput.Replace("{" + key + "}", jsonValues.GetStringOrDefault(key, string.Empty));
				}
			}

			// Call the OpenAI API with the assembled user input and the prompt.
			string strResult = await Completions.CompleteAsJSON(strUserInput, strPrompt);

			// Return a tuple containing:
			//   1. The result from the API (expected to be a JSON response with the generated SQL code and status).
			//   2. The user input string (for logging or debugging purposes).
			return strResult;
		}

	}



}