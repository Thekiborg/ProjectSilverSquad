using System.Linq;

namespace ProjectSilverSquad
{
	public class Alert_CloningVatsFailingNoNutrients : Alert_Critical
	{
		private static List<Thing> Vats => [.. Find.CurrentMap?.listerThings.ThingsOfDef(SilverSquad_ThingDefOfs.SilverSquad_CloningVat).Where(
					t => ((ThingClass_CloningVat)t).Nutrition <= 0.1f && ((ThingClass_CloningVat)t).State == VatState.Growing)];

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Vats);
		}

		public override string GetLabel()
		{
			return "SilverSquad_Alert_FailingNoNutPasteTitle".Translate();
		}

		public override TaggedString GetExplanation()
		{
			return "SilverSquad_Alert_FailingNoNutPaste".Translate();
		}
	}
}
