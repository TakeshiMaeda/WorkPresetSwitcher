using RimWorld;
using UnityEngine;
using Verse;
using static WorkPresetSwitcher.WorkPresetData;

namespace WorkPresetSwitcher
{
    public class CustomWorkTab : MainTabWindow_Work
    {
        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            float buttonWidth = 80f;
            float buttonHeight = 24f;
            float padding = 5f;

            float posX = inRect.xMin + 10f;
            float posY = inRect.yMax - buttonHeight - 10f;

            Rect rect = new Rect(posX, posY, buttonWidth, buttonHeight);

            var manager = WorkPresetManager.Instance;
            if (manager == null) return;

            for (int i = 0; i < WorkPresetManager.PresetMax; i++)
            {
                Color back = GUI.color;
                GUI.color = manager.DoesExistPreset(i) ? Color.white : Color.gray;
                if (Widgets.ButtonText(rect, manager.GetPresetName(i)))
                {
                    manager.CurrentPreset = i;
                }
                if (manager.CurrentPreset == i)
                {
                    Widgets.DrawBoxSolid(rect.ExpandedBy(2f), new Color(1f, 1f, 1f, 0.25f));
                }
                GUI.color = back;
                rect.x += buttonWidth + padding;
            }

            rect.x += buttonWidth + padding;
            if (Widgets.ButtonText(rect, "WorkPresetSwitcher_Clear".Translate()))
            {
                foreach (var pawn in Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
                {
                    if (pawn.workSettings == null) continue;
                    if (!pawn.RaceProps.Humanlike) continue;
                    foreach (WorkTypeDef workDef in DefDatabase<WorkTypeDef>.AllDefs)
                    {
                        pawn.workSettings.SetPriority(workDef, 0);
                    }
                }
                Messages.Message("WorkPresetSwitcher_Cleared".Translate(), MessageTypeDefOf.PositiveEvent);
            }

            //ここから逆順

            posX = inRect.xMax - 10f - buttonWidth;
            rect = new Rect(posX, posY, buttonWidth, buttonHeight);

            {
                Color back = GUI.color;
                GUI.color = manager.DoesExistCurrentPreset ? Color.white : Color.gray;
                if (Widgets.ButtonText(rect, "WorkPresetSwitcher_Rename".Translate()))
                {
                    if (manager.DoesExistCurrentPreset)
                    {
                        Find.WindowStack.Add(new RenameWindow());
                    }
                }
                GUI.color = back;
                rect.x -= buttonWidth + padding;
            }

            {
                Color back = GUI.color;
                GUI.color = manager.DoesExistCurrentPreset ? Color.white : Color.gray;
                if (Widgets.ButtonText(rect, "WorkPresetSwitcher_Delete".Translate()))
                {
                    if (manager.DoesExistCurrentPreset)
                    {
                        string name = manager.CurrentPresetName;
                        manager.DeletePreset(manager.CurrentPreset);
                        Messages.Message("WorkPresetSwitcher_Deleted".Translate(name), MessageTypeDefOf.PositiveEvent);
                    }
                }
                GUI.color = back;
                rect.x -= buttonWidth + padding;
            }

            {
                Color back = GUI.color;
                GUI.color = manager.DoesExistCurrentPreset ? Color.white : Color.gray;
                if (Widgets.ButtonText(rect, "WorkPresetSwitcher_Load".Translate()))
                {
                    if (manager.DoesExistCurrentPreset)
                    {
                        manager.LoadPreset(manager.CurrentPreset);
                        Messages.Message("WorkPresetSwitcher_Loaded".Translate(manager.CurrentPresetName), MessageTypeDefOf.PositiveEvent);
                    }
                }
                GUI.color = back;
                rect.x -= buttonWidth + padding;
            }

            {
                if (Widgets.ButtonText(rect, "WorkPresetSwitcher_Save".Translate()))
                {
                    manager.SavePreset(manager.CurrentPreset);
                    Messages.Message("WorkPresetSwitcher_Saved".Translate(manager.CurrentPresetName), MessageTypeDefOf.PositiveEvent);
                }
                rect.x -= buttonWidth + padding;
            }
        }
    }

    public class RenameWindow : Window
    {
        string name;
        const int MaxNameLength = 9;

        public override Vector2 InitialSize => new Vector2(400f, 200f);

        public RenameWindow()
        {
            doCloseX = true;
            closeOnClickedOutside = true;
            var manager = WorkPresetManager.Instance;
            if (manager == null) return;
            name = manager.CurrentPresetName;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var manager = WorkPresetManager.Instance;
            if (manager == null) return;

            name = Widgets.TextField(new Rect(0f, 35f, inRect.width, 30f), name, MaxNameLength);

            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "WorkPresetSwitcher_DoRename".Translate());
            if (Widgets.ButtonText(new Rect(0f, 110f, inRect.width / 2 - 5f, 35f), "WorkPresetSwitcher_Save".Translate()))
            {
                if (manager.CurrentPresetName != name)
                {
                    string oldName = manager.CurrentPresetName;
                    manager.CurrentPresetName = name;
                    Messages.Message("WorkPresetSwitcher_Renamed".Translate(oldName, manager.CurrentPresetName), MessageTypeDefOf.PositiveEvent);
                }
                Close();
            }
            if (Widgets.ButtonText(new Rect(inRect.width / 2 + 5f, 110f, inRect.width / 2 - 5f, 35f), "WorkPresetSwitcher_Cancel".Translate()))
            {
                Close();
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class CustomWorkTabInitializer
    {
        static CustomWorkTabInitializer()
        {
            var workTabDef = DefDatabase<MainButtonDef>.GetNamed("Work");
            if (workTabDef != null)
            {
                workTabDef.tabWindowClass = typeof(CustomWorkTab);
            }
        }
    }
}