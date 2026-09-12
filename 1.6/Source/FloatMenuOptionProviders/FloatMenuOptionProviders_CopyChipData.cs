using Verse.AI;

namespace ProjectSilverSquad
{
	public class FloatMenuOptionProviders_CopyChipData : FloatMenuOptionProvider
	{
		private readonly TargetingParameters TargetParams = new()
		{
			canTargetSelf = false,
			canTargetBuildings = false,
			canTargetPawns = false,
			canTargetItems = true,
			thingCategory = ThingCategory.Item,
			mapObjectTargetsMustBeAutoAttackable = false,
		};

		protected override bool Drafted => false;
		protected override bool Undrafted => true;
		protected override bool Multiselect => false;


		protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
		{
			if (clickedThing is not IChipDataCopiable dataCopiable)
			{
				return null;
			}
			if (!dataCopiable.IsEmpty())
			{
				return null;
			}
			if (!ChipValidator(context.FirstSelectedPawn, clickedThing))
				return null;


			return new FloatMenuOption("SilverSquad_CopyChipData_Label".Translate(), () =>
			{
				Find.Targeter.BeginTargeting(TargetParams,
					action: target =>
					{
						if (target.Thing is not null)
						{
							Job job = JobMaker.MakeJob(SilverSquad_JobDefOfs.SilverSquad_CopyData, clickedThing, target);
							job.count = 2;
							context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job);
						}
					},
					highlightAction: null,
					targetValidator: target =>
					{
						if (target.Thing == clickedThing)
						{
							return false;
						}
						if (target.Thing is not IChipDataCopiable dataCopiable)
						{
							return false;
						}
						if (dataCopiable.IsEmpty())
						{
							return false;
						}
						if (!ChipValidator(context.FirstSelectedPawn, target.Thing))
						{
							return false;
						}
						if (target.Thing.GetType() != clickedThing.GetType())
						{
							return false;
						}
						return true;
					});
			});
		}


		private static bool ChipValidator(Pawn pawn, Thing chip)
		{
			if (!pawn.CanReach(chip, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				return false;
			}
			if (!pawn.CanReserve(chip))
			{
				return false;
			}
			if (chip.IsBurning())
			{
				return false;
			}
			return true;
		}
	}
}
