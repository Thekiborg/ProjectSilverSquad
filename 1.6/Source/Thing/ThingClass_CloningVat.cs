using System.Linq;
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
		private bool passedTicksOfNoReturn;


		public int PawnGrowTimeLeft { get => pawnGrowTimeLeft; private set => pawnGrowTimeLeft = Math.Max(0, value); }
		public int EmbryoIncubationTimeLeft { get => embryoIncubationTimeLeft; private set => embryoIncubationTimeLeft = Math.Max(0, value); }
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
		public ModExtension ModExtension => field ??= def.GetModExtension<ModExtension>();
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
		public bool PastTicksOfNoReturn => PawnGrowTimeLeft <= cloningSettings.PawnGrowTicks / 2;


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
				if (CompPowerTrader.Off)
				{
					MissingResourceInstability();
				}
				if (Nutrition <= 0f)
				{
					MissingResourceInstability();
				}

				if (Settings.Instability >= 1)
				{
					DoBadOutcome();
					Reset();
					return;
				}
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
						if (Settings.Clone.ageTracker.CurLifeStage != LifeStageDefOf.HumanlikeChild)
						{
							Settings.Clone.story.bodyType = Settings.GenomeImprint.genome.OriginalBody;
						}

						if (!passedTicksOfNoReturn && PastTicksOfNoReturn)
						{
							passedTicksOfNoReturn = true;
							if (Rand.Chance(Settings.Instability))
							{
								DoBadOutcome();
								Reset();
							}
							else
							{
								ApplyBrainChips();
							}
						}
					}
				}
			}
		}


		private void MissingResourceInstability()
		{
			Settings.Instability += ModExtension.instabilityPerPeriod / (ModExtension.instabilityPeriod / GenTicks.TickRareInterval);
		}


		private void DoBadOutcome()
		{
			if (CurGrowingPhase == GrowingPhase.Incubation)
			{
				thingOwner.Take(Settings.GenomeImprint).Destroy();
				thingOwner.TryDropAll(Position, Map, ThingPlaceMode.Near);
			}
			else
			{
				int totalWeight = ModExtension.OrderedOutcomes.Sum(outcome => outcome.weight);
				int rand = Rand.RangeInclusive(1, totalWeight);

				foreach (var outcome in ModExtension.OrderedOutcomes)
				{
					if (rand <= outcome.weight)
					{
						outcome.worker.Do(this, Settings.Clone);
						break;
					}
					else
					{
						rand -= outcome.weight;
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
			GenSpawn.Spawn(Settings.Clone, InteractionCell, Map);
			Reset();
		}


		public void Reset()
		{
			Settings = null;
			PawnGrowTimeLeft = 0;
			EmbryoIncubationTimeLeft = 0;
			curState = VatState.Inactive;
			passedTicksOfNoReturn = false;
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
					icon = TextureLibrary.CreateCloneIcon,
					action = () =>
					{
						Find.WindowStack.Add(new Window_CloningSettings(this));
					}
				};
			}


			if (DebugSettings.godMode)
			{
				yield return new Command_Action()
				{
					defaultLabel = "RELEASE HIM",
					action = FinishCloning
				};
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
						else if ((CurGrowingPhase == GrowingPhase.GrowingBody) && !passedTicksOfNoReturn) PawnGrowTimeLeft = cloningSettings.PawnGrowTicks / 2;
						else if ((CurGrowingPhase == GrowingPhase.GrowingBody) && passedTicksOfNoReturn) PawnGrowTimeLeft = 0;
					}
				};
				yield return new Command_Action()
				{
					defaultLabel = "Fire outcome",
					action = () =>
					{
						List<FloatMenuOption> options = [];
						foreach (var outcome in ModExtension.OrderedOutcomes)
						{
							options.Add(new FloatMenuOption(outcome.worker.ToString(), () => outcome.worker.Do(this, Settings.Clone)));
						}
						Find.WindowStack.Add(new FloatMenu(options));
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
			sb.Append("SilverSquad_CloningVat_StoredNutPaste".Translate(Nutrition.ToString("F1")));
			if (base.GetInspectString().Length > 0)
			{
				sb.AppendLine();
				sb.Append(base.GetInspectString());
			}
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
		}


		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			Vector3 drawPos = DrawPos;
			drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.PawnUnused);
			ModExtension.vatAboveGraphicData.Graphic.Draw(drawPos, Rotation, this);
			if (CurGrowingPhase == GrowingPhase.Incubation)
			{
				Vector3 loc = drawLoc;
				loc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
				ModExtension.embryoGraphicData.Graphic.GetColoredVersion(
					ModExtension.embryoGraphicData.Graphic.Shader,
					Settings.Clone.story.SkinColor,
					Color.white)
					.Draw(loc, Rot4.North, this);
			}
		}


		public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
		{
			base.DynamicDrawPhaseAt(phase, drawLoc, flip);
			if (CurGrowingPhase == GrowingPhase.GrowingBody)
			{
				Vector3 loc = drawLoc;
				loc.y = Altitudes.AltitudeFor(AltitudeLayer.Pawn);
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
			thingOwner.TryAddOrTransfer(thing, thing.stackCount, false);
			WantedIngredients.Remove(thing.def);
			if (wantedIngredients.Count <= 0
				&& GetDirectlyHeldThings().Contains(Settings.GenomeImprint)
				&& (Settings.Xenogerm is null || (Settings.Xenogerm is not null && GetDirectlyHeldThings().Contains(Settings.Xenogerm))))
			{
				StartGrowing();
			}
		}


		public void LoadPaste(Thing paste)
		{
			Nutrition += paste.GetStatValue(StatDefOf.Nutrition) * paste.stackCount;
			paste.Destroy();
		}


		public void StartGrowing()
		{
			pawnGrowTimeLeft = cloningSettings.PawnGrowTicks;
			embryoIncubationTimeLeft = cloningSettings.EmbryoGrowTicks;
			Settings.Clone.ageTracker.AgeBiologicalTicks = GenDate.TicksPerYear * 3;
			Settings.Clone.ageTracker.AgeChronologicalTicks = 0;
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
			Scribe_Values.Look(ref passedTicksOfNoReturn, "ProjectSilverSquad_cloningVat_PassedTicksOfNoReturn");
		}
	}
}
