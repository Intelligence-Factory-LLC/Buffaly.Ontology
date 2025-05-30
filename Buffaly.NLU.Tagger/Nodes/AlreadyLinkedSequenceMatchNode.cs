using System.Text;
using Ontology;
using Buffaly.NLU.Tagger.RolloutController;

namespace Buffaly.NLU.Tagger.Nodes
{
	public class AlreadyLinkedSequenceMatchNode : BaseTaggingNode 
	{
		static public bool IsEnabled = false; 

		public int SourceIndex = 0;
		public int ReferenceIndex = 0;
		public AlreadyLinkedSequenceMatchNode(Prototype protoSource, Prototype protoReference, int iSourceIndex, int iReferenceIndex) : base(protoSource, protoReference)
		{
			ReferenceIndex = iReferenceIndex;
			SourceIndex = iSourceIndex;
		}

		public delegate Prototype OnUnderstandDelegate(Prototype protoSequence);
		public static OnUnderstandDelegate OnUnderstand = null;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("AlreadyLinkedSequenceMatchNode - ");
			for (int i = 0; i < Source.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.SourceIndex)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Source.Children[i]));

				if (i == this.SourceIndex)
					sb.Append("]");
			}

			sb.Append(" / ");

			for (int i = 0; i < Reference.Children.Count; i++)
			{
				if (i > 0)
					sb.Append(",");

				if (i == this.ReferenceIndex)
					sb.Append("[");

				sb.Append(PrototypeLogging.ToChildString(Reference.Children[i]));

				if (i == this.ReferenceIndex)
					sb.Append("]");
			}

			return sb.ToString();
		}
		public override ControlFlag Evaluate()
		{
			Prototype protoLinkedRoot = Source.Children[SourceIndex - 1].Clone();
			Prototype protoLinkedObject = GetLastLinked(protoLinkedRoot);

			Collection colProjectedSource = new Collection();
			colProjectedSource.Add(protoLinkedObject);
			colProjectedSource.Children.AddRange(Source.Children.GetRange(SourceIndex, Reference.Children.Count - 1));

			int iSourceStart = SourceIndex - ReferenceIndex;					
			
			for (int i = 0; i < Reference.Children.Count; i++)
			{
				Prototype protoSource = colProjectedSource.Children[i];
				Prototype protoReference = Reference.Children[i];
				if (!Prototypes.TypeOf(protoSource, protoReference))
				{
					Continue cont = new Continue();
					return cont;
				}
			}

			//Changed this from shallow clone to a Clone + Clear so it 
			//preserves any properties on the sequence
			Prototype protoSequence = Reference.Clone();
			if (Prototypes.TypeOf(Reference, Sequence.Prototype))
				protoSequence = Sequences.GetSequence(Reference.PrototypeID).Clone();

			protoSequence.Children.Clear();
			protoSequence.Children.AddRange(colProjectedSource.Children);

			if (null == OnUnderstand)
				throw new Exception("Unexpected");

			protoSequence = OnUnderstand(protoSequence);

			//Don't allow for methods that return a collection 
			if (null == protoSequence || Prototypes.TypeOf(protoSequence, Ontology.Collection.Prototype))
				return null;

			//Check that Understand method returns the linked object, otherwise this isn't
			//valid as the we don't know where to fit the new object (or the linking could be wrong)
			if (protoSequence != protoLinkedObject)
				return null;



			protoLinkedRoot.Children.AddRange(colProjectedSource.Children.GetRange(ReferenceIndex, colProjectedSource.Count - ReferenceIndex));

			Collection colResult = new Collection();
			if (SourceIndex >= 2)
				colResult.Children.AddRange(Source.Children.GetRange(0, SourceIndex - 1));

			colResult.Children.Add(protoLinkedRoot);

			int iAfterSequence = SourceIndex + Reference.Children.Count - ReferenceIndex;
			if (Source.Children.Count > iAfterSequence)
			{
				colResult.Children.AddRange(Source.Children.GetRange(iAfterSequence, Source.Children.Count - iAfterSequence));
			}

			SingleResult singleResult = new SingleResult(colResult);

			return singleResult;
		}

		static public Prototype GetLastLinked(Prototype protoSource)
		{
			if (protoSource.Children.Count == 0 || protoSource == protoSource.Children.Last())
				return protoSource;

			//N20220426-01 - Recursively getting is causing us to use a cloned node
			//return protoSource.Children.Last();
			return GetLastLinked(protoSource.Children.Last());
		}
	}
}
