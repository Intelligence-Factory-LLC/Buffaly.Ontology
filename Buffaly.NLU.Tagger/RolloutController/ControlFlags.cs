namespace Buffaly.NLU.Tagger.RolloutController
{
	public class ControlFlag
	{
		public virtual ControlFlag Clone()
		{
			return this;
		}
	}

	public class Continue : ControlFlag
	{

	}

	public class Result : ControlFlag
	{
	}
}
