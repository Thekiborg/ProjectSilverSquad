using System.Text;

namespace ProjectSilverSquad
{
	public class ThingClass_CloningVat : Building, IThingHolder
	{
		private Effecter progressBar;
		private CloningSettings cloningSettings;
		private ThingOwner thingOwner;
		private VatState curState = VatState.Inactive;
		private List<ThingDef> wantedIngredients;
		private float nutrientPasteNutrition;
		private int pawnGrowTimeLeft;
		private int embryoIncubationTimeLeft;
		private int surgeryProgBarTicks;


		private int PawnGrowTimeLeft { get => pawnGrowTimeLeft; set => pawnGrowTimeLeft = Math.Max(0, value); }
		private int EmbryoIncubationTimeLeft { get => embryoIncubationTimeLeft; set => embryoIncubationTimeLeft = Math.Max(0, value); }
		private GrowingPhase CurGrowingPhase
		{
			get
			{
				if (curState != VatState.Growing) return GrowingPhase.None;
				else
				{
					if (embryoIncubationTimeLeft > 0) return GrowingPhase.Incubation;
					else return GrowingPhase.GrowingBody;
				}
			}
		}
		private int TotalTicksRemaining => PawnGrowTimeLeft + EmbryoIncubationTimeLeft;
		public CompPowerTrader CompPowerTrader => field ??= GetComp<CompPowerTrader>();
		private ModExtension ModExtension => field ??= def.GetModExtension<ModExtension>();
		public CloningSettings Settings { get => cloningSettings; private set => cloningSettings = value; }
		public float Nutrition { get => nutrientPasteNutrition; set => nutrientPasteNutrition = Mathf.Clamp(value, 0f, ModExtension.maxNutPasteCapacity); }
		public List<ThingDef> WantedIngredients
		{
			get
			{
				if (wantedIngredients.NullOrEmpty())
				{
					var ingredients = Settings.GetIngredients();
					foreach (var thing in GetDirectlyHeldThings())
					{
						ingredients.Remove(thing.def);
					}
					wantedIngredients = ingredients;
				}
				return wantedIngredients;
			}
		}
		public VatState State => curState;


		public ThingClass_CloningVat()
		{
			thingOwner = new ThingOwner<Thing>(this, oneStackOnly: false);
		}


		public override void TickRare()
		{
			base.TickRare();
			CompPowerTrader.PowerOutput = curState == VatState.Growing ? (0f - CompPowerTrader.Props.PowerConsumption) : (0f - CompPowerTrader.Props.idlePowerDraw);
			if (curState == VatState.Growing)
			{
				Nutrition -= ModExtension.baseNutConsumptionPerDay / (GenDate.TicksPerDay / (float)GenTicks.TickRareInterval);

				if (CurGrowingPhase == GrowingPhase.Incubation)
				{
					EmbryoIncubationTimeLeft -= GenTicks.TickRareInterval;
				}
				else if (CurGrowingPhase == GrowingPhase.GrowingBody)
				{
					if (PawnGrowTimeLeft <= 0)
					{
						ApplySurgeries();
					}
					else
					{
						PawnGrowTimeLeft -= GenTicks.TickRareInterval;

						float norm = MathUtils.Normalization01((PawnGrowTimeLeft - ModExtension.basePawnGrowTimeTicks) * -1, 0, ModExtension.basePawnGrowTimeTicks);
						Settings.Clone.ageTracker.AgeBiologicalTicks = (long)Mathf.Lerp(GenDate.TicksPerYear * 3, Settings.GenomeImprint.genome.OriginalAgeTicks, norm);
						if (Settings.Clone.ageTracker.CurLifeStage == LifeStageDefOf.HumanlikeAdult)
						{
							Settings.Clone.story.bodyType = Settings.GenomeImprint.genome.OriginalBody;
						}

						if (PawnGrowTimeLeft >= ModExtension.ticksOfNoReturn)
						{
							ApplyBrainChips();
						}
					}
				}
			}
		}



		private void ApplySurgeries()
		{
			var curSurg = Settings.Surgeries.FirstOrFallback(null);
			if (curSurg is not null)
			{
				progressBar ??= EffecterDefOf.ProgressBarAlwaysVisible.SpawnAttached(this, Map);
				progressBar?.EffectTick(this, TargetInfo.Invalid);
				var progressBarMote = (progressBar.children[0] as SubEffecter_ProgressBar)?.mote;
				progressBarMote.progress = Mathf.Clamp01(surgeryProgBarTicks / curSurg.RecipeDef.workAmount);
				surgeryProgBarTicks += GenTicks.TickRareInterval;

				if (surgeryProgBarTicks > curSurg.RecipeDef.workAmount)
				{
					Settings.Clone.health.AddHediff(curSurg.RecipeDef.addsHediff, curSurg.Part);
					foreach (var ing in curSurg.Ingredients)
					{
						foreach (var item in thingOwner)
						{
							if (ing == item.def)
							{
								thingOwner.Remove(item);
								break;
							}
						}
					}
					Settings.Surgeries.RemoveAt(0);
					surgeryProgBarTicks = 0;
				}
			}
			else
			{
				progressBar?.Cleanup();
				progressBar = null;
			}
		}


		public void StartCloning(CloningSettings cloningSettings)
		{
			Settings = cloningSettings;
			WantedIngredients.Clear();
			curState = VatState.AwaitingIngredients;
		}


		public void FinishCloning()
		{
			thingOwner.Remove(Settings.GenomeImprint);
			GenSpawn.Spawn(Settings.Clone, Position, Map);
			Reset();
		}


		public void Reset()
		{
			Settings = null;
			PawnGrowTimeLeft = 0;
			EmbryoIncubationTimeLeft = 0;
			curState = VatState.Inactive;
			thingOwner.ClearAndDestroyContents();
		}


		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
				yield return gizmo;

			if (curState == VatState.Inactive)
			{
				yield return new Command_Action()
				{
					defaultLabel = "SilverSquad_CloningVat_StartGizmoLabel".Translate(),
					action = () =>
					{
						Find.WindowStack.Add(new Window_CloningSettings(this));
					}
				};
			}

			yield return new Command_Action()
			{
				defaultLabel = "RELEASE HIM",
				action = FinishCloning
			};

			if (Prefs.DevMode)
			{
				yield return new Command_Action()
				{
					defaultLabel = "Set nut to full",
					action = () => Nutrition = ModExtension.maxNutPasteCapacity,
				};
				yield return new Command_Action()
				{
					defaultLabel = "Set nut to empty",
					action = () => Nutrition = 0
				};
				yield return new Command_Action()
				{
					defaultLabel = "Finish growing phase",
					action = () =>
					{
						if (CurGrowingPhase == GrowingPhase.Incubation) EmbryoIncubationTimeLeft = 0;
						else if (CurGrowingPhase == GrowingPhase.GrowingBody) PawnGrowTimeLeft = 0;
					}
				};
			}
		}


		public override string GetInspectString()
		{
			StringBuilder sb = new();
			if (curState == VatState.Growing)
			{
				sb.AppendLine("SilverSquad_CloningVat_Instability".Translate(Settings.Instability.ToStringPercent()).Colorize(ColorLibrary.RedReadable));
				sb.AppendLine("SilverSquad_CloningVat_RemainingDays".Translate(TotalTicksRemaining.ToStringTicksToDays()));
			}
			sb.AppendLine("SilverSquad_CloningVat_StoredNutPaste".Translate(Nutrition.ToString("F1")));
			sb.Append(base.GetInspectString());
			if (curState == VatState.AwaitingIngredients)
			{
				sb.AppendLine();
				sb.Append("SilverSquad_CloningVat_WantedIngredients".Translate());

				if (!GetDirectlyHeldThings().Contains(Settings.GenomeImprint))
				{
					sb.Append($"{Settings.GenomeImprint.LabelCap}");
				}
				if (!GetDirectlyHeldThings().Contains(Settings.Xenogerm) && Settings.Xenogerm is not null)
				{
					sb.Append($", {Settings.Xenogerm.LabelCap}");
				}
				for (int i = 0; i < WantedIngredients.Count; i++)
				{
					var def = WantedIngredients[i];
					sb.Append($", {def.LabelCap}");
				}
			}
			return sb.ToString();
		}


		private void ApplyBrainChips()
		{
			Log.Message(thingOwner.Count);

			foreach (SkillRecord skill in Settings.Clone.skills.skills)
			{
				skill.levelInt = Settings.SkillLevels[skill.def];
			}
			foreach (var skillTrait in Settings.BrainChipsSkill)
			{
				thingOwner.RemoveAll(t => t.def == skillTrait);
			}
			foreach (var chipTrait in Settings.BrainChipsTrait)
			{
				foreach (var traitMod in chipTrait.traitMods)
				{
					Trait trait = new(traitMod.traitDef, traitMod.traitDegree);
					Settings.Clone.story.traits.GainTrait(trait);
				}
				thingOwner.RemoveAll(t => t.def == chipTrait);
			}

			Log.Message(thingOwner.Count);
		}


		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if (CurGrowingPhase == GrowingPhase.Incubation)
			{
				Vector3 loc = drawLoc;
				loc.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
				ModExtension.embryoGraphicData.Graphic.Draw(loc, Rotation, this);
			}
		}


		public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
		{
			base.DynamicDrawPhaseAt(phase, drawLoc, flip);
			if (CurGrowingPhase == GrowingPhase.GrowingBody)
			{
				Vector3 loc = drawLoc;
				loc.y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop);
				Settings.Clone.Drawer.renderer.DynamicDrawPhaseAt(phase, loc, Rotation);
			}
		}


		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}


		public ThingOwner GetDirectlyHeldThings()
		{
			return thingOwner;
		}


		public void AddIngredient(Thing thing)
		{
			thingOwner.TryAddOrTransfer(thing, thing.stackCount);
			WantedIngredients.Remove(thing.def);
			if (wantedIngredients.Count <= 0
				&& GetDirectlyHeldThings().Contains(Settings.GenomeImprint)
				&& (Settings.Xenogerm is null || (Settings.Xenogerm is not null && GetDirectlyHeldThings().Contains(Settings.Xenogerm))))
			{
				StartGrowing();
			}
		}


		public void StartGrowing()
		{
			pawnGrowTimeLeft = ModExtension.basePawnGrowTimeTicks;
			embryoIncubationTimeLeft = ModExtension.baseEmbryoIncubationTicks;
			Settings.Clone.ageTracker.AgeBiologicalTicks = GenDate.TicksPerYear * 3;
			Settings.Clone.story.bodyType = PawnGenerator.GetBodyTypeFor(Settings.Clone);
			curState = VatState.Growing;
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref cloningSettings, "ProjectSilverSquad_CloningVat_CloningSettings");
			Scribe_Values.Look(ref nutrientPasteNutrition, "ProjectSilverSquad_CloningVat_NutrientPasteNutrition");
			Scribe_Values.Look(ref pawnGrowTimeLeft, "ProjectSilverSquad_CloningVat_PawnGrowTimeLeft");
			Scribe_Values.Look(ref embryoIncubationTimeLeft, "ProjectSilverSquad_CloningVat_EmbryoIncubationTimeLeft");
			Scribe_Deep.Look(ref thingOwner, "ProjectSilverSquad_CloningVat_ThingOwner");
			Scribe_Values.Look(ref curState, "ProjectSilverSquad_CloningVat_CurState");
			Scribe_Values.Look(ref surgeryProgBarTicks, "ProjectSilverSquad_CloningVat_SurgeryProgBarTicks");
		}
	}
}
