//added
using Ontology;
using WebAppUtilities;
using Buffaly.NLU.Tagger.Nodes;
using Buffaly.NLU.Tagger;
using Buffaly.NLU.Nodes;
using ProtoScript.Interpretter.RuntimeInfo;

namespace Buffaly.NLU
{
	public class FragmentingTagger
	{
		public bool TagAfterFragment = true;
		public ProtoScriptTagger Tagger = null;

		static public FunctionRuntimeInfo ExitCondition = null;

		public FragmentingTagger(ProtoScriptTagger tagger)
		{
			Tagger = tagger;
		}

		static public Collection Tag(string strPhrase, ProtoScriptTagger tagger)
		{
			FragmentingTagger fragmentingTagger = new FragmentingTagger(tagger);
			
			return fragmentingTagger.Tag(strPhrase);
		}


		public Collection Tag(string strPhrase)
		{
			//This version accumulates and tags everything at the clearing lexeme
			Collection colResult = new Collection();

			Tagger2 tagFragments = new Tagger2();
			tagFragments.MaxIterations = Tagger.MaxIterations;
			TaggingNode node = new TaggingNode();
			Collection colAccumulator = new Collection();

			Tagger.AllowSubTypingDuringTagging = true;
			foreach (Prototype protoFragment in FragmentInput(strPhrase))
			{
				if (protoFragment.Children.Count == 1 && IsClearingLexeme(protoFragment.Children[0]))
				{
					if (this.TagAfterFragment)
					{
						Prototype protoBest = TagAccumulator(tagFragments, colAccumulator);
						colResult.Children.AddRange(protoBest.Children);
						//Clear this here, as well as below in case this is the end of the input
						colAccumulator = new Collection();
					}

					continue;
				}

				Prototype protoResult = Tagger.Tag(protoFragment);
				PrototypeLogging.Log(protoResult);


				if (Prototypes.TypeOf(protoResult, Collection.Prototype))
					colAccumulator.Children.AddRange(protoResult.Children.Select(x => UnderstandUtil.SubType(x, Tagger.Interpretter)));
				else
					colAccumulator.Add(UnderstandUtil.SubType(protoResult, Tagger.Interpretter));
			}

			if (colAccumulator.Children.Count > 0)
			{
				if (TagAfterFragment)
				{
					Prototype protoBest = TagAccumulator(tagFragments, colAccumulator);
					colAccumulator = new Collection();
					colResult.Children.AddRange(protoBest.Children);
				}
			}

			colResult.AddRange(colAccumulator);

			PrototypeLogging.Log(colResult);

			return colResult;
		}


		private Prototype TagAccumulator(Tagger2 tagFragments, Collection colAcumulator)
		{
			FragmentTaggingNode node = new FragmentTaggingNode();
			if (null != ExitCondition)
			{
				node.ExitCondition = x =>
				{
					return ExitConditionWrapper(x);
				};
			}

			RolloutNode rolloutNodeNew = new RolloutNode(colAcumulator);
			node.AddPossibility(rolloutNodeNew);

			Buffaly.NLU.Tagger.RolloutController.Result resFrag = tagFragments.Tag2(node);
			if (null != resFrag && resFrag is SingleResult)
				return (resFrag as SingleResult).Result;

			//select the best possibility as that were we have least prototypes - meaning 
			//it was rolled up best. Note: for some unexplored reason the Value function 
			//here doesn't work
			var node2 = node.Possibilities.OrderBy(x => (x as RolloutNode).Source.Children.Count).First();
			Prototype protoBest = (node2 as Buffaly.NLU.Tagger.BaseFunctionNode).Source;
			return protoBest;
		}
		public IEnumerable<Prototype> FragmentInput(string strPhrase)
		{
			Prototype protoTokens = UnderstandUtil.Tokenize(strPhrase);
			Prototype protoLexemes = UnderstandUtil.ConvertToLexemes(protoTokens);

			if (protoLexemes.Children.Any(x => !Prototypes.TypeOf(x, Lexeme.Prototype)))
			{
				if (this.Tagger.AllowUnknownLexemes)
					protoLexemes = UnknownLexemes.ResolveAll(protoLexemes, this.Tagger.Interpretter);

				if (protoLexemes.Children.Any(x => !Prototypes.TypeOf(x, Lexeme.Prototype)))
					throw new JsonWsException("Unrecognized lexeme: " + string.Join(", ", protoLexemes.Children.Where(x => !Prototypes.TypeOf(x, Lexeme.Prototype)).Select(x => x.PrototypeName).ToArray()));
			}

			int iPos = 0;
			Prototype fragment = GetNextFragment(protoLexemes, ref iPos);
			while (fragment.Children.Count > 0)
			{
				PrototypeLogging.Log(fragment);

				yield return fragment;

				fragment = GetNextFragment(protoLexemes, ref iPos);
			}

			yield break;
		}

		static public Prototype GetNextFragment(Prototype protoLexemes, ref int iPos)
		{
			Collection fragment = new Collection();

			if (iPos >= protoLexemes.Children.Count)
				return fragment;

			do
			{
				Prototype protoLexeme = protoLexemes.Children[iPos++];
				if (IsStopLexeme(protoLexeme))
				{
					if (IsHiddenStopLexeme(protoLexeme))
					{
						if (fragment.Children.Count > 0)
							break;

						//Skip

					}
					else if (fragment.Children.Count == 0)
					{
						fragment.Add(protoLexeme);
						break;
					}
					else
					{
						iPos--;
						break;
					}
				}
				else
				{
					fragment.Add(protoLexeme);
				}
			}
			while (iPos < protoLexemes.Children.Count);

			return fragment;
		}

		static public bool IsStopLexeme(Prototype protoLexeme)
		{
			return Prototypes.TypeOf(protoLexeme, TemporaryPrototypes.GetTemporaryPrototypeOrNull("SeparatingLexeme"));
		}

		static public bool IsHiddenStopLexeme(Prototype protoLexeme)
		{
			return Prototypes.TypeOf(protoLexeme, TemporaryPrototypes.GetTemporaryPrototypeOrNull("HiddenSeparatingLexeme"));
		}

		static public bool IsClearingLexeme(Prototype protoLexeme)
		{
			return Prototypes.TypeOf(protoLexeme, TemporaryPrototypes.GetTemporaryPrototypeOrNull("ClearLexeme"));
		}

		private bool ExitConditionWrapper(Prototype prototype)
		{
			object oRes = Tagger.Interpretter.RunMethod(ExitCondition, null, new List<object>() { prototype });

			return (bool)oRes;
		}

		//Deprecated methods
		/*		static public Collection TagAlt(string strPhrase, ProtoScriptTagger tagger)
				{
					//This version tries to tag at each additional fragement. It may 
					//be better with forward pressure, but it doesn't handle long sequences well
					//Note: If you restore this version, try switching to FragmentTaggingNode as that
					//may solve the latency problem. 
					Collection colResult = new Collection();

					Tagger2 tagFragments = new Tagger2();
					tagFragments.MaxIterations = tagger.MaxIterations;
					TaggingNode node = new TaggingNode();
					Collection colAccumulator = new Collection();

					tagger.AllowSubTypingDuringTagging = true;

					bool bFirst = true;
					foreach (Prototype protoFragment in FragementInput(strPhrase))
					{
						if (protoFragment.Children.Count == 1 && IsClearingLexeme(protoFragment.Children[0]))
						{
							colResult.AddRange(colAccumulator);

							//Clear this here, as well as below in case this is the end of the input
							colAccumulator = new Collection();
							bFirst = true;
							continue;
						}

						colAccumulator = new Collection();

						Prototype protoResult = tagger.Tag(protoFragment);
						PrototypeLogging.Log(protoResult);


						if (Prototypes.TypeOf(protoResult, Collection.Prototype))
							colAccumulator.Children.AddRange(protoResult.Children.Select(x => UnderstandUtil.SubType(x, tagger.Interpretter)));
						else
							colAccumulator.Add(UnderstandUtil.SubType(protoResult, tagger.Interpretter));

						if (bFirst)
						{
							node = new TaggingNode();
							node.AddPossibility(new RolloutNode(colAccumulator));

							bFirst = false;
						}
						else
						{
							TaggingNode nodeNew = new TaggingNode();

							List<ControllerNode> lstNodes = new List<ControllerNode>();
							foreach (ControllerNode controllerNode in node.Possibilities)
							{
								RolloutNode rolloutNode = (RolloutNode)controllerNode;
								RolloutNode rolloutNodeNew = new RolloutNode(rolloutNode.Source);
								rolloutNodeNew.Source.Children.AddRange(colAccumulator.Children);

								nodeNew.AddPossibility(rolloutNodeNew);
							}

							node = nodeNew;


							//Note: Could probably save all the fragments and just tag when we are at the end
							//or at a clearing lexeme. But this approach puts forward pressure on the fragment
							Prototype protoBest = TagAccumulator2(tagFragments, node);
							colAccumulator = new Collection();

							if (Prototypes.TypeOf(protoBest, Collection.Prototype))
								colAccumulator.Children.AddRange(protoBest.Children.Select(x => UnderstandUtil.SubType(x, tagger.Interpretter)));
							else
								colAccumulator.Add(UnderstandUtil.SubType(protoBest, tagger.Interpretter));
						}
					}

					colResult.AddRange(colAccumulator);

					PrototypeLogging.Log(colResult);

					return colResult;
				}


				static private Prototype TagAccumulator2(Tagger2 tagFragments, BaseFunctionNode node)
				{
					Buffaly.NLU.Tagger.RolloutController.Result resFrag = tagFragments.Tag2(node);
					if (null != resFrag && resFrag is SingleResult)
						return (resFrag as SingleResult).Result;

					//select the best possibility as that were we have least prototypes - meaning 
					//it was rolled up best. Note: for some unexplored reason the Value function 
					//here doesn't work
					var node2 = node.Possibilities.OrderBy(x => (x as RolloutNode).Source.Children.Count).First();
					Prototype protoBest = (node2 as Buffaly.NLU.Tagger.BaseFunctionNode).Source;
					return protoBest;
				}

				static public Collection Tag2(string strPhrase, ProtoScriptTagger tagger)
				{
					Collection colResult = new Collection();
					Collection colAccumulator = new Collection();
					bool bFirst = true;
					foreach (Prototype protoFragment in FragementInput(strPhrase))
					{
						if (protoFragment.Children.Count == 1 && IsClearingLexeme(protoFragment.Children[0]))
						{
							colResult.AddRange(colAccumulator);
							colAccumulator = new Collection();
							continue;
						}

						Prototype protoResult = tagger.Tag(protoFragment);
						PrototypeLogging.Log(protoResult);

						if (Prototypes.TypeOf(protoResult, Collection.Prototype))
							colAccumulator.Children.AddRange(protoResult.Children.Select(x => UnderstandUtil.SubType(x, tagger.Interpretter)));
						else
							colAccumulator.Add(UnderstandUtil.SubType(protoResult, tagger.Interpretter));

						if (bFirst)
							bFirst = false;
						else
						{
							Prototype protoFinal = tagger.Tag(colAccumulator);
							PrototypeLogging.IncludeLexemes = true;
							PrototypeLogging.IncludeTypeOfs = false;
							PrototypeLogging.Log(protoFinal);
							colAccumulator = new Collection();
							if (Prototypes.TypeOf(protoFinal, Collection.Prototype))
								colAccumulator.Children.AddRange(protoFinal.Children);
							else
								colAccumulator.Add(protoFinal);
						}
					}

					colResult.AddRange(colAccumulator);

					PrototypeLogging.Log(colResult);

					return colResult;
				}
		*/

	}
}
