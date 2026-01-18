using BasicUtilities;
using BasicUtilities.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ontology.GraphInduction.Model
{
	public class HCP
	{
		public Prototype Shadow;
		public Prototype HiddenShadow;
		public Map<int, Prototype> Paths = new Map<int, Prototype>();
		//public Map<int, HCPTree.Node> Expectations = new Map<int, HCPTree.Node>();

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(HiddenShadow.ToString());
			sb.Append(" {").Append(HiddenShadow.Properties.ToString()).Append(" }");
			return sb.ToString();
		}

		//public static Prototype ToPrototype(HCP hcp)
		//{
		//	//convert now to prototype on HCP level
		//	Prototype protoHCP = Prototypes.ToPrototype(hcp);

		//	//Prototype protoExpectations = new Prototype("Ontology.Meta.HCP.HCP.Field.Expectations");
		//	//foreach (var pair in hcp.Expectations)
		//	//{
		//	//	protoExpectations.Properties[pair.Key] = Prototypes.ToPrototype(pair.Value);
		//	//}

		//	protoHCP.Properties["Ontology.Meta.HCP.HCP.Field.Expectations"] = protoExpectations;

		//	Prototype protoPaths = new Prototype("Ontology.Meta.HCP.HCP.Field.Paths");
		//	foreach (var pair in hcp.Paths)
		//	{
		//		protoPaths.Properties[pair.Key] = Prototypes.ToPrototype(pair.Value);
		//	}

		//	protoHCP.Properties["Ontology.Meta.HCP.HCP.Field.Paths"] = protoPaths;

		//	return protoHCP;
		//}

		//public static HCP FromPrototype(Prototype protoHCP)
		//{
		//	HCP hcp = Prototypes.FromPrototype(protoHCP) as HCP;

		//	Prototype protoExpectations = protoHCP.Properties["Ontology.Meta.HCP.HCP.Field.Expectations"];
		//	Prototype protoPaths = protoHCP.Properties["Ontology.Meta.HCP.HCP.Field.Paths"];

		//	//foreach (var pair in protoExpectations.Properties)
		//	//{
		//	//	HCPTree.Node node = Prototypes.FromPrototype(pair.Value) as HCPTree.Node;
		//	//	hcp.Expectations[pair.Key] = node;
		//	//}

		//	foreach (var pair in protoPaths.Properties)
		//	{
		//		hcp.Paths[pair.Key] = pair.Value;
		//	}

		//	return hcp;
		//}
	}

	public class HCPs
	{
		static private Map<int, HCP> m_mapHCPs = new Map<int, HCP>();
		static public Map<int, HCP> HCPCollection
		{
			get
			{
				return m_mapHCPs;
			}
			set
			{
				m_mapHCPs = value;
			}
		}

		static public HCP Get(int iPrototypeID)
		{
			HCP hcp = null;

			if (!m_mapHCPs.TryGetValue(iPrototypeID, out hcp))
			{
				if (null == hcp)
				{
					//throw new Exception("Could not find HCP: " + iPrototypeID);
				}

				m_mapHCPs[iPrototypeID] = hcp;
			}

			return hcp;
		}

		static public void Add(HCP hcp, bool bMaterialize = true)
		{
			m_mapHCPs[hcp.HiddenShadow.PrototypeID] = hcp;
		}



		//public static HCP CreateHCPFromExtractedInstanceInfo(HiddenContextPrototypes.ExtractedInstanceInfo info)
		//{
		//	HCP hcp = new HCP();
		//	hcp.Shadow = info.Shadow;
		//	if (info.Instances.Count > 0)
		//	{
		//		hcp.HiddenShadow = PrototypeGraphLists.ComparePrototypes(info.Instances);
		//	}

		//	for (int i = 0; i < info.Paths.Count; i++)
		//	{
		//		Prototype protoPath = info.Paths[i];

		//		hcp.Paths.Add(new Prototype(hcp.HiddenShadow.PrototypeName + ".Field." + i).PrototypeID, protoPath);
		//	}

		//	//Note this does not populate the Expectations;
		//	return hcp;
		//}

		static public Prototype Convert(Prototype prototype, Prototype target)
		{
			if (Prototypes.AreShallowEqual(prototype, target))
				return prototype;

			Prototype protoUnpacked;

			if (Prototypes.TypeOf(prototype, Hidden.Base.Prototype))
				protoUnpacked = HCPs.Unpack(prototype);
			else
				protoUnpacked = prototype;

			HCP hcp = HCPs.Get(target.PrototypeID);
			Prototype protoInstance = HCPs.GetHCPInstance(protoUnpacked, hcp);

			return protoInstance;
		}


		static public Prototype GetHCPInstance(Prototype prototype, HCP hcp)
		{
			Prototype protoResult = hcp.HiddenShadow.Clone();

			//This differs from the original in that it uses Paths
			foreach (var pair in hcp.Paths)
			{
				Prototype protoPath = hcp.Paths[pair.Key];
				Prototype ? protoValue = PrototypeGraphs.GetValue(prototype, protoPath);
				protoResult.Properties[pair.Key] = protoValue;
			}
			return protoResult;
		}

		static public Prototype Unpack(Prototype prototype)
		{
			Prototype protoResult = prototype;
			if (Prototypes.TypeOf(protoResult, Hidden.Base.Prototype))
			{
				HCP hcp = HCPs.Get(prototype.PrototypeID);
				if (null != hcp)
				{
					protoResult = hcp.Shadow.Clone();
					protoResult = PrototypeGraphs.RemoveComparisonOperations(protoResult);

					foreach (var pair in prototype.Properties)
					{
						if (null != pair.Value)
						{
							Prototype protoPath = hcp.Paths[pair.Key];

							//N20200725-01 - Merge doesn't work for NativeValuePrototypes
							protoResult = PrototypeGraphs.SetValue(protoResult, protoPath, pair.Value, false, false);
						}
					}
				}
			}

			return protoResult;
		}


		//	static public Prototype UnpackRecursively(Prototype prototype)
		//	{
		//		Prototype protoResult = Unpack(prototype);
		//		Prototype protoResult2 = protoResult.Clone();

		//		foreach (var pair in protoResult.Properties)
		//		{
		//			if (null != pair.Value)
		//				protoResult2.Properties[pair.Key] = UnpackRecursively(pair.Value);
		//		}

		//		for (int i = 0; i < protoResult.Children.Count; i++)
		//		{
		//			protoResult2.Children[i] = UnpackRecursively(protoResult.Children[i]);
		//		}

		//		return protoResult2;
		//	}



		//	static public Prototype UnpackShadow(Prototype prototype)
		//	{
		//		//Based on Unpack but does not remove the comparison operators or set the values of the fields
		//		Prototype protoResult = prototype;
		//		if (Prototypes.TypeOf(protoResult, Hidden.Base.Prototype))
		//		{
		//			HCP hcp = HCPs.Get(prototype.PrototypeID);
		//			if (null != hcp)
		//			{
		//				protoResult = hcp.Shadow.Clone();
		//			}
		//		}

		//		return protoResult;
		//	}


		//	static public Prototype UnpackShadowRecursively(Prototype prototype)
		//	{
		//		Prototype protoResult = UnpackShadow(prototype);
		//		Prototype protoResult2 = protoResult.Clone();

		//		foreach (var pair in protoResult.Properties)
		//		{
		//			if (null != pair.Value)
		//				protoResult2.Properties[pair.Key] = UnpackShadowRecursively(pair.Value);
		//		}

		//		for (int i = 0; i < protoResult.Children.Count; i++)
		//		{
		//			protoResult2.Children[i] = UnpackShadowRecursively(protoResult.Children[i]);
		//		}

		//		return protoResult2;
		//	}

	}
	}
