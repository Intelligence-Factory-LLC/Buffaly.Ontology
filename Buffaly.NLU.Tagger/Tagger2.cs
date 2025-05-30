using Buffaly.NLU.Tagger.RolloutController;
using Ontology;

namespace Buffaly.NLU.Tagger
{
	public class Tagger2 : Controller
	{
		static public Result Tag(BaseFunctionNode node)
		{
			Tagger2 tagger2 = new Tagger2();
			Result result = tagger2.ControlRoot(node);
			return result;
		}

		public Result Tag2(BaseFunctionNode node)
		{
			Tagger2 tagger2 = this;
			Result result = tagger2.ControlRoot(node);
			return result;
		}


		static public Prototype Tokenize(string strInput)
		{
			Prototype prototype = new Ontology.Collection();
			foreach (string strToken in Lexemes.Split(strInput))
			{
				prototype.Children.Add(NativeValuePrototype.GetOrCreateNativeValuePrototype(strToken).ShallowClone());
			}

			return prototype;
		}

	}
}
