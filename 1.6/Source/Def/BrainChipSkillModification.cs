using System.Xml;

namespace ProjectSilverSquad
{
	public class BrainChipSkillModification : IExposable
	{
		public SkillDef skillDef;
		public int skillOffset;


		public BrainChipSkillModification() { }
		public BrainChipSkillModification(SkillDef skillDef, int skillOffset)
		{
			this.skillDef = skillDef;
			this.skillOffset = skillOffset;
		}


		public override string ToString()
		{
			return $"{skillDef.LabelCap} {skillOffset.ToStringWithSign()}";
		}


		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skillDef", xmlRoot.Name);
			skillOffset = ParseHelper.ParseIntPermissive(xmlRoot.FirstChild.Value);

			/*string[] sections = xmlRoot.FirstChild.Value.Split(',');
			skillOffset = ParseHelper.ParseIntPermissive(sections[0]);
			passionMod = Enum.Parse<PassionMod.PassionModType>(sections[1]);*/
		}


		public void ExposeData()
		{
			Scribe_Defs.Look(ref skillDef, "ProjectSilverSquad_BrainChipSkillModification_skillDef");
			Scribe_Values.Look(ref skillOffset, "ProjectSilverSquad_BrainChipSkillModification_skillOffset");
		}
	}
}
