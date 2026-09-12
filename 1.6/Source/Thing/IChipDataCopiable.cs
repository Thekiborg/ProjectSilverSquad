namespace ProjectSilverSquad
{
	public interface IChipDataCopiable
	{
		public void CopyDataFrom(Thing sourceChip);
		public bool IsEmpty();
	}
}
