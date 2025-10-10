using System.Xml;

namespace ProjectSilverSquad
{
	public class BrainChipTraitModification
	{
		public ThingDef parent;
		public TraitDef traitDef;
		public int traitDegree;


		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "traitDef", xmlRoot.Name);
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "parent", xmlRoot.ParentNode.ParentNode.SelectSingleNode("/defName").FirstChild.Value);
			traitDegree = ParseHelper.ParseIntPermissive(xmlRoot.FirstChild.Value);
		}
	}
}
