using System.Xml;

namespace ProjectSilverSquad
{
	public class BrainChipTraitModification : IExposable
	{
		public TraitDef traitDef;
		public int traitDegree;


		public BrainChipTraitModification() { }
		public BrainChipTraitModification(TraitDef traitDef, int traitDegree)
		{
			this.traitDef = traitDef;
			this.traitDegree = traitDegree;
		}


		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "traitDef", xmlRoot.Name);
			traitDegree = ParseHelper.ParseIntPermissive(xmlRoot.FirstChild.Value);
		}


		public void ExposeData()
		{
			Scribe_Defs.Look(ref traitDef, "ProjectSilverSquad_BrainChipTraitModification_traitDef");
			Scribe_Values.Look(ref traitDegree, "ProjectSilverSquad_BrainChipTraitModification_traitDegree");
		}
	}
}
