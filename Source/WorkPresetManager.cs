using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Verse;

namespace WorkPresetSwitcher
{
    public class WorkPresetData : IExposable
    {
        public class WorkPriorityPawn : IExposable
        {
            public string id;
            Dictionary<WorkTypeDef, int> workPriorities = new Dictionary<WorkTypeDef, int>();

            public WorkPriorityPawn()
            {
            }
            public bool FromPawn(Pawn pawn)
            {
                id = pawn.GetUniqueLoadID();
Log.Message($"FromPawn: {id}");
                if (pawn.workSettings == null) return false;
Log.Message($"FromPawn: {pawn.workSettings!=null}");
                foreach (WorkTypeDef workDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    int priority = pawn.workSettings.GetPriority(workDef);
                    if (workPriorities.ContainsKey(workDef))
                    {
                        workPriorities[workDef] = priority;
                    }
                    else
                    {
                        workPriorities.Add(workDef, priority);
                    }
                }
                return true;
            }

            public void ToPawn(Pawn pawn)
            {
                if (id != pawn.GetUniqueLoadID()) return;
                if (pawn.workSettings == null) return;
                foreach (WorkTypeDef workDef in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    //存在すればPawnにセット
                    if (workPriorities.ContainsKey(workDef))
                    {
                        pawn.workSettings.SetPriority(workDef, workPriorities[workDef]);
                    }
                }
            }
            public void ExposeData()
            {
                Scribe_Values.Look(ref id, "id", "");
                Scribe_Collections.Look(ref workPriorities, "propaties", LookMode.Def, LookMode.Value);
                if (workPriorities == null)
                {
                    workPriorities = new Dictionary<WorkTypeDef, int>();
                }
            }
        }

        public string PresetName="Nothing";
        Dictionary<string, WorkPriorityPawn> workPriorityPawns= new Dictionary<string, WorkPriorityPawn>();

        public WorkPresetData()
        {
        }

        public WorkPresetData(string name)
        {
            PresetName = name;
        }

        public void FromPawn()
        {
            foreach (var pawn in Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)){
                string id = pawn.GetUniqueLoadID();
                if (!pawn.RaceProps.Humanlike) continue;
                if (workPriorityPawns.ContainsKey(id))
                {
                    Log.Message($"{id} is contains");
                    workPriorityPawns[id].FromPawn(pawn);
                }
                else
                {
                    var prioPawn = new WorkPriorityPawn();
                    bool add = prioPawn.FromPawn(pawn);
                    if (add) {
                        Log.Message($"{id} add ");
                        workPriorityPawns.Add(id, prioPawn);
                    } else
                    {
                        Log.Message($"{id} not add ");

                    }
                }
            }
        }

        public void ToPawn()
        {
            foreach (var pawn in Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
            {
                string id = pawn.GetUniqueLoadID();
                if (workPriorityPawns.ContainsKey(id))
                {
                    workPriorityPawns[id].ToPawn(pawn);
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref PresetName, "PresetName", "Error");
            Scribe_Collections.Look(ref workPriorityPawns, "WorkPriorityPawns", LookMode.Value, LookMode.Deep);
            if (workPriorityPawns == null)
            {
                PresetName = "Error";
                workPriorityPawns = new Dictionary<string, WorkPriorityPawn>();
            }
        }
    }



    public class WorkPresetManager : GameComponent
	{
        public const int PresetMax = 5;

        static WorkPresetManager instance = null;
        public static WorkPresetManager Instance => instance ?? (instance = Current.Game.GetComponent<WorkPresetManager>());

        Dictionary<int,WorkPresetData> workPresetDatas = new Dictionary<int, WorkPresetData>();
        public int CurrentPreset = 0;
        public string CurrentPresetName { 
            get { return GetPresetName(CurrentPreset); } 
            set { SetPresetName(CurrentPreset, value); }
        }
        public bool DoesExistCurrentPreset => DoesExistPreset(CurrentPreset);

        public WorkPresetManager(Game game)
		{
            instance = this;
        }

        public string GetPresetName(int preset)
        {
            string ret = $"preset{preset}";
            if (workPresetDatas!=null && workPresetDatas.ContainsKey(preset))
            {
                if (workPresetDatas[preset] != null)
                {
                    ret = workPresetDatas[preset].PresetName;
                }
            }
            return ret;
        }

        public void SetPresetName(int preset, string name)
        {
            if (workPresetDatas != null && workPresetDatas.ContainsKey(preset))
            {
                if (workPresetDatas[preset] != null)
                {
                    workPresetDatas[preset].PresetName = name;
                }
            }
        }

        public bool DoesExistPreset(int preset)
        {
            return workPresetDatas!=null && workPresetDatas.ContainsKey(preset);
        }

        //ポーンからプリセットデータに保存する
        public void SavePreset(int preset)
        {
            if (!workPresetDatas.ContainsKey(preset))
            {
                workPresetDatas.Add(preset, null);
            }

            WorkPresetData presetData = workPresetDatas[preset];
            if (presetData == null)
            {
                //データが無いので作成
                presetData = new WorkPresetData(GetPresetName(preset));
            }
            //ポーンから取得
            presetData.FromPawn();
            workPresetDatas[preset] = presetData;
        }

        //プリセットデータからポーンに設定する
        public void LoadPreset(int preset)
        {
            WorkPresetData presetData = null;
            if (workPresetDatas.ContainsKey(preset))
            {
                presetData = workPresetDatas[preset];
            }
            if (presetData != null)
            {
                presetData.ToPawn();
            }
        }

        //プリセットデータを削除
        public void DeletePreset(int preset)
        {
            if (workPresetDatas.ContainsKey(preset))
            {
                workPresetDatas.Remove(preset);
            }
        }
        public override void ExposeData()
        {
            Scribe_Values.Look(ref CurrentPreset, "CurrentPreset", 0);
            Scribe_Collections.Look(ref workPresetDatas, "WorkPresetDatas",LookMode.Value,LookMode.Deep);
            if(workPresetDatas == null)
            {
                workPresetDatas = new Dictionary<int, WorkPresetData>();
            }
            //範囲外を削除
            var keys = workPresetDatas.Keys.ToList();
            foreach (var key in keys)
            {
                if (key >= PresetMax)
                {
                    workPresetDatas.Remove(key);
                }
            }
        }
    }
}