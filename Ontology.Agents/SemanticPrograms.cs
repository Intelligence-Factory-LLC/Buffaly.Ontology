using BasicUtilities;
using Ontology.Agents.NodeBasedAgent;
using WebAppUtilities;

namespace Ontology.Agents
{
    /// <summary>
    /// JsonWs wrapper around <see cref="PlanningOrchestrator"/> for UI interaction.
    /// </summary>
    public class SemanticPrograms : JsonWs
    {
        private static readonly Dictionary<string, PlanningOrchestrator> _orchestrators = new();

        /// <summary>
        /// Creates or retrieves a session, then prepares a <see cref="PlanningOrchestrator"/>.
        /// </summary>
        /// <param name="sessionKey">Unique session identifier.</param>
        /// <param name="directive">Initial directive for the program.</param>
        /// <param name="programName">Name of the semantic program fragment.</param>
        public static void Setup(string sessionKey, string directive, string programName)
        {
            SessionObject session = DevAgent.Instance.GetOrCreateSession(sessionKey);

            PlanningOrchestrator orchestrator = new PlanningOrchestrator();
            orchestrator.SetupSemanticProgram(session, programName);
            orchestrator.AddInitialDirective(session, directive);

            _orchestrators[sessionKey] = orchestrator;
        }

        /// <summary>
        /// Executes a single iteration of the semantic program and returns the result.
        /// </summary>
        /// <param name="sessionKey">Unique session identifier used in <see cref="Setup"/>.</param>
        /// <returns>Json describing the current iteration result.</returns>
        public static async Task<JsonObject> Step(string sessionKey)
        {
            if (!_orchestrators.TryGetValue(sessionKey, out PlanningOrchestrator? orchestrator))
            {
                JsonObject err = new JsonObject();
                err["Error"] = $"No orchestrator for session {sessionKey}";
                return err;
            }

            SessionObject session = DevAgent.Instance.GetOrCreateSession(sessionKey);
            return await orchestrator.SingleAgentLoop(session);
        }
    }
}
