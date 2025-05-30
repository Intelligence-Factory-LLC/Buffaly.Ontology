namespace ProtoScript.Interpretter.Compiled
{
	public class DotNetNewInstance : Expression
	{
		public System.Reflection.ConstructorInfo Constructor;
		public List<Compiled.Expression> Parameters = new List<Expression>();
	}
}
