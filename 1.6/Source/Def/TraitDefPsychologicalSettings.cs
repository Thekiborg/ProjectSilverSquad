using System.Xml;

namespace ProjectSilverSquad
{
	public class TraitDefPsychologicalSettings
	{
		public TraitDef traitDef;
		public bool isPsychological;


		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "traitDef", xmlRoot.Name);
			isPsychological = ParseHelper.FromString<bool>(xmlRoot.FirstChild.Value);
		}
	}
}
