using BasicUtilities;
using Buffaly.AIProviderAPI;
using System.Text;

namespace Ontology.Agents
{
	/// <summary>
	/// Session-scoped state that records the conversational history.
	/// Now stores <see cref="Message"/> objects directly; the old
	/// MessagesRow wrapper has been removed.
	/// </summary>
	public class SessionObject : ProtoScript.Extensions.SessionObject
	{
		public List<Prototype> Entities = new List<Prototype>();
		public List<Prototype> UserState = new List<Prototype>();

		public List<Message> Messages { get; } = new List<Message>();

		/*───────────────────────── helpers ─────────────────────────*/
		public void AddUserMessage(string content)
		{
			Messages.Add(new Message { Content = content, Role = MessageRole.User });
		}

		public void AddAgentMessage(string content)
		{
			Messages.Add(new Message { Content = content, Role = MessageRole.Assistant });
		}

		public void AddSystemMessage(string content)
		{
			Messages.Add(new Message { Content = content, Role = MessageRole.System });
		}

		public bool HasSystemMessage()
		{
			return Messages.Any(m => m.Role == MessageRole.System);
		}

		/*───────────────────────── serialisation ───────────────────*/
		public string GetConversationHistoryAsTextBlock()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Message m in Messages)
			{
				if (m.Role == MessageRole.User)
				{
					sb.AppendLine("User: " + m.Content);
				}
				else if (m.Role == MessageRole.Assistant)
				{
					sb.AppendLine("Agent: " + m.Content);
				}
			}
			return sb.ToString();
		}

		public void PopulateConversationHistoryFromJSON(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return;
			}

			JsonObject root = new JsonObject(json);
			JsonArray arr = root["Messages"].ToJsonArray();

			Messages.Clear();
			foreach (JsonObject o in arr)
			{
				string roleTxt = o.GetStringOrNull("Role") ?? "User";
				if (!Enum.TryParse(roleTxt, out MessageRole role))
				{
					role = MessageRole.User;
				}

				Messages.Add(new Message
				{
					Content = o.GetStringOrNull("Content"),
					Role = role
				});
			}
		}

		internal string GetConversationHistoryAsJSON()
		{
			return JsonUtil.ToStringExt(this.Messages).ToString();
		}
	}
}