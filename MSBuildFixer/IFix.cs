namespace MSBuildFixer
{
	public interface IFix
	{
		void AttachTo(SolutionWalker walker);
	}
}
