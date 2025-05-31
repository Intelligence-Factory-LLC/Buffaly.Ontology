
using BasicUtilities;
using BasicUtilities.Collections;
using Ontology.BaseTypes;

namespace Ontology
{
	public class NativeValuePrototype : Prototype
	{
		public Object NativeValue;
		private bool m_bObjectInstance = false;

		private NativeValuePrototype()
		{
		}



		//public NativeValuePrototype(Prototype prototype, Object obj) 
		//{			
		//	this.NativeValue = obj;
		//	this.PrototypeName = prototype.PrototypeName + "[" + obj.GetHashCode() + "]";
		//	this.PrototypeID = TemporaryPrototypes.GetOrCreateTemporaryPrototype(PrototypeName, prototype).PrototypeID;
		//	this.InsertTypeOf(prototype);
		//}

		//This method is for wrapping a prototype in a NativeValuePrototype (so it can be returned as one) but doesn't create a NativeValue
		//public NativeValuePrototype(Prototype prototype)
		//{
		//	this.PrototypeID = prototype.PrototypeID;
		//	this.PrototypeName = prototype.PrototypeName;
		//	this.Properties = prototype.Properties;
		//	this.Children = prototype.Children;
		//	this.Value = prototype.Value;

		//}


		static public NativeValuePrototype GetOrCreateNativeValuePrototype(string strValue)
		{
			string strPrototypeName = System_String.Prototype.PrototypeName + "[" + strValue + "]";
			Prototype? prototype = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null == prototype)
			{
				NativeValuePrototype nv = new NativeValuePrototype();
				nv.NativeValue = strValue;
				nv.PrototypeName = strPrototypeName;
				nv.PrototypeID = TemporaryPrototypes.GetOrInsertPrototype(nv).PrototypeID;
				nv.InsertTypeOf(System_String.Prototype);
				return nv;
			}
			
			if (prototype is not NativeValuePrototype nv2)
			{
				throw new Exception("Prototype with name '" + strPrototypeName + "' is not a NativeValuePrototype, but " + prototype.GetType().Name);
			}

			return nv2;
		}

		static public NativeValuePrototype GetOrCreateNativeValuePrototype(int iValue)
		{
			string strPrototypeName = System_Int32.Prototype.PrototypeName + "[" + iValue + "]";
			Prototype? prototype = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null == prototype)
			{
				NativeValuePrototype nv = new NativeValuePrototype();
				nv.NativeValue = iValue;
				nv.PrototypeName = strPrototypeName;
				nv.PrototypeID = TemporaryPrototypes.GetOrInsertPrototype(nv).PrototypeID;
				nv.InsertTypeOf(System_Int32.Prototype);

				return nv;
			}
			if (prototype is not NativeValuePrototype nv2)
			{
				throw new Exception("Prototype with name '" + strPrototypeName + "' is not a NativeValuePrototype, but " + prototype.GetType().Name);
			}
			return nv2;
		}

		static public NativeValuePrototype GetOrCreateNativeValuePrototype(bool bValue)
		{
			string strPrototypeName = System_Boolean.Prototype.PrototypeName + "[" + bValue + "]";
			Prototype? prototype = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null == prototype)
			{
				NativeValuePrototype nv = new NativeValuePrototype();
				nv.NativeValue = bValue;
				nv.PrototypeName = strPrototypeName;
				nv.PrototypeID = TemporaryPrototypes.GetOrInsertPrototype(nv).PrototypeID;
				nv.InsertTypeOf(System_Boolean.Prototype);
				return nv;
			}
			if (prototype is not NativeValuePrototype nv2)
			{
				throw new Exception("Prototype with name '" + strPrototypeName + "' is not a NativeValuePrototype, but " + prototype.GetType().Name);
			}
			return nv2;
		}

		static public NativeValuePrototype GetOrCreateNativeValuePrototype(double dValue)
		{
			string strPrototypeName = System_Double.Prototype.PrototypeName + "[" + dValue + "]";
			Prototype? prototype = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strPrototypeName);
			if (null == prototype)
			{
				NativeValuePrototype nv = new NativeValuePrototype();
				nv.NativeValue = dValue;
				nv.PrototypeName = strPrototypeName;
				nv.PrototypeID = TemporaryPrototypes.GetOrInsertPrototype(nv).PrototypeID;
				nv.InsertTypeOf(System_Double.Prototype);
				return nv;
			}
			if (prototype is not NativeValuePrototype nv2)
			{
				throw new Exception("Prototype with name '" + strPrototypeName + "' is not a NativeValuePrototype, but " + prototype.GetType().Name);
			}
			return nv2;
		}


		//public NativeValuePrototype(Object obj, bool bInstance = true)
		//{
		//	Prototype protoType = ExtractTypeHierarchy(obj.GetType());
		//	if (bInstance)
		//	{
		//		this.PrototypeName = protoType.PrototypeName + "[" + obj.GetHashCode() + "]";
		//		this.PrototypeID = TemporaryPrototypes.GetOrCreateTemporaryPrototype(PrototypeName).PrototypeID;
		//		this.NativeValue = obj;
		//		this.InsertTypeOf(protoType);
		//		this.m_bObjectInstance = true;
		//	}
		//	else
		//	{
		//		this.NativeValue = obj;
		//		this.PrototypeName = protoType.PrototypeName;
		//		this.PrototypeID = protoType.PrototypeID;

		//		foreach (int protoTypeOf in protoType.GetTypeOfs())
		//		{
		//			this.InsertTypeOf(protoTypeOf);
		//		}
		//	}
		//}

		private static Prototype ExtractTypeHierarchy(System.Type type)
		{
			string strTypeName = type.FullName;
			Prototype? prototype = TemporaryPrototypes.GetTemporaryPrototypeOrNull(strTypeName);
			if (null == prototype)
			{
				prototype = TemporaryPrototypes.GetOrCreateTemporaryPrototype(strTypeName);
				if (type != typeof(object))
				{
					Prototype protoBase = ExtractTypeHierarchy(type.BaseType);
					prototype.InsertTypeOf(protoBase.PrototypeID);
				}
			}
			return prototype;
		}

		public override bool ShallowEquivalent(Prototype rhs)
		{
			//Rule: primitive types compare like AreEqual (System.Int[1] does not equal System.Int[2])
			//Object types need to ignore the instance and descend
			if (this.m_bObjectInstance)
			{
				Prototype lhs = this.IsInstance() ? this.GetBaseType() : this;
				rhs = rhs.IsInstance() ? rhs.GetBaseType() : rhs;

				return lhs.ShallowEqual(rhs);
			}

			return this.ShallowEqual(rhs);
		}

		public override Prototype Clone()
		{
			return this.ShallowClone();
		}

		public override Prototype ShallowClone()
		{
			NativeValuePrototype nv = new NativeValuePrototype();

			PopulateClone(nv);
			nv.NativeValue = this.NativeValue;

			return nv;
		}

		public override JsonObject ToJsonObject(bool bRemoveNulls = false)
		{
			JsonObject jsonPrototype = new JsonObject();
			jsonPrototype[nameof(PrototypeName)] = this.PrototypeName;

			//N20210109-03 - NativeValue can be cast back to base type
			if (null != this.NativeValue)
			{
				if (this.PrototypeID == System_Int32.PrototypeID)
				{
					jsonPrototype[nameof(NativeValue)] = (int)this.NativeValue;
				}
				else if (this.PrototypeID == System_Boolean.PrototypeID)
				{
					jsonPrototype[nameof(NativeValue)] = (bool)this.NativeValue;
				}
				else if (this.PrototypeID == System_Double.PrototypeID)
				{
					jsonPrototype[nameof(NativeValue)] = (double)this.NativeValue;
				}
				else if (this.PrototypeID == System_String.PrototypeID)
				{
					jsonPrototype[nameof(NativeValue)] = (string)this.NativeValue;
				}
			}

			foreach (var pair in this.Properties)
			{
				if (!bRemoveNulls || pair.Value != null)
					jsonPrototype[pair.Key.ToString()] = pair.Value?.ToJsonObject();
			}

			if (this.Children.Count > 0)
			{
				JsonArray jsonChildren = new JsonArray();

				foreach (Prototype child in this.Children)
				{
					jsonChildren.Add(child == null ? null : child.ToJsonObject());
				}

				jsonPrototype[nameof(Children)] = jsonChildren;
			}

			return jsonPrototype;
		}

		public override string ToJSON(bool bRemoveNulls = false)
		{
			return ToJsonObject(bRemoveNulls).ToJSON();
		}


		public override JsonObject ToFriendlyJsonObject()
		{
			JsonObject jsonPrototype = new JsonObject();
			jsonPrototype[nameof(PrototypeName)] = this.PrototypeName;

			//N20210109-03 - NativeValue can be cast back to base type
			if (null != this.NativeValue)
			{
				if (this.NativeValue is int)
				{
					jsonPrototype[nameof(NativeValue)] = (int)this.NativeValue;
				}
				else if (this.NativeValue is bool)
				{
					jsonPrototype[nameof(NativeValue)] = (bool)this.NativeValue;
				}
				else if (this.NativeValue is double)
				{
					jsonPrototype[nameof(NativeValue)] = (double)this.NativeValue;
				}
				else if (this.NativeValue is string)
				{
					jsonPrototype[nameof(NativeValue)] = (string)this.NativeValue;
				}
			}

			foreach (var pair in this.Properties)
			{
				if (pair.Value != null)
					jsonPrototype[Prototypes.GetPrototypeName(pair.Key)] = pair.Value?.ToJsonObject();
			}

			if (this.Children.Count > 0)
			{
				JsonArray jsonChildren = new JsonArray();

				foreach (Prototype child in this.Children)
				{
					jsonChildren.Add(child == null ? null : child.ToJsonObject());
				}

				jsonPrototype[nameof(Children)] = jsonChildren;
			}

			return jsonPrototype;
		}

		//Needed for inheritance
		public override JsonObject ToFriendlyJsonObjectCircular(Set<int> setHashes)
		{
			return ToFriendlyJsonObject();
		}


		public override IEnumerable<int> GetParents()
		{
			//Parent includes the base type here
			if (null != NativeValue)
				yield return PrototypeID;

			foreach (int protoParent in base.GetParents())
				yield return protoParent;
		}

		new public static Prototype FromJSON(string strJSON)
		{
			return FromJsonValue(new JsonValue(strJSON));
		}

		public static Prototype FromJSON(string strJSON, bool bConvertErrorToNull)
		{
			return FromJsonValue(new JsonValue(strJSON), bConvertErrorToNull);
		}

		new public static Prototype FromJsonValue(JsonValue jsonValue)
		{
			return FromJsonValue(jsonValue, false);
		}

		public static Prototype FromJsonValue(JsonValue jsonValue, bool bConvertErrorToNull)
		{
			Prototype prototype = null;

			try
			{

				if (jsonValue.ToJsonObject() != null)
				{
					JsonObject jsonPrototype = jsonValue.ToJsonObject();

					if (jsonPrototype.ContainsKey(nameof(NativeValuePrototype.NativeValue)))
					{
						if (!jsonPrototype.ContainsKey(nameof(Prototype.PrototypeName)))
							throw new Exception("JsonObject does not contain PrototypeName - required for NativeValuePrototype");

						JsonValue jsonNV = jsonPrototype[nameof(NativeValuePrototype.NativeValue)];
						string strPrototypeName = jsonPrototype[nameof(Prototype.PrototypeName)].ToString();

						if (strPrototypeName.StartsWith(System_Boolean.PrototypeName))
							prototype = NativeValuePrototype.GetOrCreateNativeValuePrototype(jsonNV.ToBoolean());
						else if (strPrototypeName.StartsWith(System_Double.PrototypeName))
							prototype = NativeValuePrototype.GetOrCreateNativeValuePrototype(jsonNV.ToDouble());
						else if (strPrototypeName.StartsWith(System_Int32.PrototypeName))
							prototype = NativeValuePrototype.GetOrCreateNativeValuePrototype(jsonNV.ToInteger());
						else if (strPrototypeName.StartsWith(System_String.PrototypeName))
							prototype = NativeValuePrototype.GetOrCreateNativeValuePrototype(jsonNV.ToString());
						else
							throw new Exception("Converting to NV not supported for this type: " + strPrototypeName);

						//There may be augmented properties under the root
						foreach (string strKey in jsonPrototype.Keys)
						{
							if (strKey == nameof(Prototype.PrototypeID) || strKey == nameof(Prototype.PrototypeName) || strKey == nameof(NativeValuePrototype.NativeValue))
								continue;

							if (strKey == nameof(Prototype.NormalProperties)
							//TODO: Currently the JsonSerializer in WebAppUtilities is not called on every property
							//so a prototype nested in another object won't have the right format
								|| strKey == nameof(Prototype.Properties) || strKey == nameof(Prototype.Value)
								)
								continue;

							if (strKey == nameof(Prototype.Children))
							{
								Prototype propValue = FromJsonValue(jsonPrototype[strKey], bConvertErrorToNull);
								prototype.Children = propValue.Children;
							}

							else
							{
								Prototype propName = FromJsonValue(strKey, bConvertErrorToNull);

								if (null != propName)
								{
									Prototype propValue = FromJsonValue(jsonPrototype[strKey], bConvertErrorToNull);

									prototype.Properties[propName.PrototypeID] = propValue;
								}
							}
						}
					}

					else
					{
						if (jsonPrototype.ContainsKey(nameof(Prototype.PrototypeID)))
							prototype = Prototypes.GetPrototype(jsonPrototype[nameof(prototype.PrototypeID)].ToInt());

						else if (jsonPrototype.ContainsKey(nameof(Prototype.PrototypeName)))
							prototype = Prototypes.GetPrototypeByPrototypeName(jsonPrototype[nameof(Prototype.PrototypeName)].ToString());

						else
							throw new Exception("JsonObject does not contain PrototypeID or PrototypeName");

						if (prototype.PrototypeID < 0)
							return Prototype.FromJsonValue(jsonPrototype);


						foreach (string strKey in jsonPrototype.Keys)
						{
							if (strKey == nameof(Prototype.PrototypeID) || strKey == nameof(Prototype.PrototypeName) || strKey == nameof(NativeValuePrototype.NativeValue))
								continue;

							if (strKey == nameof(Prototype.Children))
							{
								Prototype propValue = FromJsonValue(jsonPrototype[strKey], bConvertErrorToNull);
								prototype.Children = propValue.Children;
							}

							else
							{
								Prototype propName = FromJsonValue(strKey, bConvertErrorToNull);

								if (null != propName)
								{
									Prototype propValue = FromJsonValue(jsonPrototype[strKey], bConvertErrorToNull);

									prototype.Properties[propName.PrototypeID] = propValue;
								}
							}
						}
					}
				}

				else if (jsonValue.ToJsonArray() != null)
				{
					prototype = Ontology.Collection.Prototype.Clone();
					foreach (JsonValue element in jsonValue.ToJsonArray())
					{
						prototype.Children.Add(FromJsonValue(element, bConvertErrorToNull));
					}
				}

				else if (StringUtil.IsInteger(jsonValue.ToString()))
				{
					prototype = Prototypes.GetPrototype(jsonValue.ToInteger());
				}

				else if (jsonValue.ToString() == "null")
				{
					prototype = null;
				}
				else
				{
					prototype = Prototypes.GetPrototypeByPrototypeName(jsonValue.ToString());
				}
			}
			catch (Exception err)
			{
				if (bConvertErrorToNull)
					prototype = null;

				else
					throw err;
			}

			return prototype;
		}
	}

	public class NativeValuePrototypes : Prototypes
	{
		static public NativeValuePrototype ToPrototype(object obj)
		{
			if (obj == null)
				return null;

			if (obj is NativeValuePrototype)
				return obj as NativeValuePrototype;



			if (obj is int || obj is Enum)
				return NativeValuePrototype.GetOrCreateNativeValuePrototype((int)obj);

			if (obj is double)
				return NativeValuePrototype.GetOrCreateNativeValuePrototype((double)obj);

			if (obj is string)
				return NativeValuePrototype.GetOrCreateNativeValuePrototype((string)obj);

			if (obj is bool)
				return NativeValuePrototype.GetOrCreateNativeValuePrototype((bool)obj);

			if (obj is System.Collections.IEnumerable)
				return ToPrototype((System.Collections.IEnumerable)obj);

			if (obj is Prototype)
				throw new NotImplementedException();

			//if (obj is System.Type)
			//	return ToPrototype((System.Type)obj);

			return ToPrototypeByReflection(obj);
		}



		static private NativeValuePrototype ToPrototypeByReflection(object obj)
		{
			throw new NotImplementedException();
		}

		static private NativeValuePrototype ToPrototype(System.Collections.IEnumerable obj)
		{

			throw new NotImplementedException("ToPrototype for IEnumerable is not implemented yet..");

			////This method should be private so it isn't used by strings

			//List<Prototype> lstChildren = new List<Prototype>();
			//foreach (var el in obj)
			//{
			//	//N-20181228-02
			//	if (null != el)
			//		lstChildren.Add(ToPrototype(el));
			//}

			////Collections do not need instances, the instance will be defined by the children
			//NativeValuePrototype protoResult = new NativeValuePrototype(obj, false);
			//protoResult.Children = lstChildren;
			//protoResult.InsertTypeOf(Ontology.Collection.Prototype.PrototypeID);
			//return protoResult;
		}

		static public object FromPrototype(NativeValuePrototype prototype)
		{

			if (prototype.NativeValue is string)
			{
				return prototype.NativeValue;
			}
			if (prototype.NativeValue is int)
			{
				return prototype.NativeValue;
			}
			if (prototype.NativeValue is bool)
			{
				return prototype.NativeValue;
			}
			if (prototype.NativeValue is double)
			{
				return prototype.NativeValue;
			}

			//N20200726-01 - other native types could be modified, so don't return them directly 
			//At some point may want to check if the native was modified before reflecting
			return NativeValuePrototypes.FromPrototypeByReflection(prototype, new Map<int, object>());
		}

		static private object FromPrototypeByReflection(NativeValuePrototype prototype, Map<int, object> m_mapPrototypeToObjects)
		{
			throw new NotImplementedException();
		}

		static public bool IsBaseType(string strPrototypeName)
		{
			switch (strPrototypeName)
			{
				case System_String.PrototypeName:
				case System_Int32.PrototypeName:
				case System_Double.PrototypeName:
				case System_Boolean.PrototypeName:
					return true;
				default:
					return false;
			}
		}

		static public bool IsBaseType(int iPrototypeID)
		{
			if (iPrototypeID == System_String.PrototypeID)
				return true;

			if (iPrototypeID == System_Int32.PrototypeID)
				return true;

			if (iPrototypeID == System_Double.PrototypeID)
				return true;

			if (iPrototypeID == System_Boolean.PrototypeID)
				return true;

			return false;
		}


	}
}
