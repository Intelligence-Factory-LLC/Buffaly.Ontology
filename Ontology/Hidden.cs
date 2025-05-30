namespace Ontology
{
	public class Hidden
	{
	
		public class Base
		{
			public const string PrototypeName = nameof(Ontology) + "." + nameof(Hidden) + "." + nameof(Base);

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

		static public Prototype GetProperty(Prototype protoHidden, string strField)
		{
			return protoHidden.Properties[protoHidden.PrototypeName + ".Field." + strField];
		}


	}
}
