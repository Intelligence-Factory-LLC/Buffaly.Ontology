namespace Ontology.Simulation
{
	public class Possibilities : Prototype
	{
		public const string PrototypeName = nameof(Ontology) + "." + nameof(Possibilities);

		public static int PrototypeID
		{
			get
			{
				return Prototype.PrototypeID;
			}
		}

		private static AsyncLocal<Prototype> m_Prototype = new AsyncLocal<Prototype>();
		public static Prototype Prototype
		{
			get
			{
				if (null == m_Prototype.Value)
					m_Prototype.Value = Prototypes.GetOrInsertPrototype(PrototypeName);

				return m_Prototype.Value;
			}
		}
	}
}
