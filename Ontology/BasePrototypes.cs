namespace Ontology.BaseTypes
{
	public class System_Int32
	{
		public const string PrototypeName = "System.Int32";

		public static int PrototypeID
		{
			get
			{
				return Prototype.PrototypeID;
			}
		}

		private static Prototype m_Prototype = null;
		public static Prototype Prototype
		{
			get
			{
				if (null == m_Prototype)
					m_Prototype = Prototypes.GetOrInsertPrototype(PrototypeName);

				return m_Prototype;
			}
		}
	}

	public class System_String
	{
		public const string PrototypeName = "System.String";

		public static int PrototypeID
		{
			get
			{
				return Prototype.PrototypeID;
			}
		}

		private static Prototype m_Prototype = null;
		public static Prototype Prototype
		{
			get
			{
				if (null == m_Prototype)
					m_Prototype = Prototypes.GetOrInsertPrototype(PrototypeName);

				return m_Prototype;
			}
		}
	}


	public class System_Double
	{
		public const string PrototypeName = "System.Double";

		public static int PrototypeID
		{
			get
			{
				return Prototype.PrototypeID;
			}
		}

		private static Prototype m_Prototype = null;
		public static Prototype Prototype
		{
			get
			{
				if (null == m_Prototype)
					m_Prototype = Prototypes.GetOrInsertPrototype(PrototypeName);

				return m_Prototype;
			}
		}
	}

	public class System_Boolean
	{
		public const string PrototypeName = "System.Boolean";

		public static int PrototypeID
		{
			get
			{
				return Prototype.PrototypeID;
			}
		}

		private static Prototype m_Prototype = null;
		public static Prototype Prototype
		{
			get
			{
				if (null == m_Prototype)
					m_Prototype = Prototypes.GetOrInsertPrototype(PrototypeName);

				return m_Prototype;
			}
		}
	}
}
