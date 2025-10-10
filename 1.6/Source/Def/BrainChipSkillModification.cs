using System.Text;
using System.Xml;

namespace ProjectSilverSquad
{
	public class BrainChipSkillModification
	{
		public ThingDef parent;
		public SkillDef skillDef;
		public int skillOffset;
		//public PassionMod.PassionModType passionMod;


		public override string ToString()
		{
			StringBuilder sb = new();

			sb.AppendLine($"{skillDef.LabelCap} {skillOffset.ToStringWithSign()} ");
			/*switch (passionMod)
			{
				case PassionMod.PassionModType.AddOneLevel:
					sb.AppendLine("PassionModAdd".Translate(skillDef));
					break;
				case PassionMod.PassionModType.DropAll:
					sb.AppendLine("PassionModDrop".Translate(skillDef));
					break;
				case PassionMod.PassionModType.None:
				default:
					break;
			}*/
			return sb.ToString();
		}


		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skillDef", xmlRoot.Name);
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "parent", xmlRoot.ParentNode.ParentNode.SelectSingleNode("/defName").FirstChild.Value);
			skillOffset = ParseHelper.ParseIntPermissive(xmlRoot.FirstChild.Value);

			/*string[] sections = xmlRoot.FirstChild.Value.Split(',');
			skillOffset = ParseHelper.ParseIntPermissive(sections[0]);
			passionMod = Enum.Parse<PassionMod.PassionModType>(sections[1]);*/
		}
	}
}
